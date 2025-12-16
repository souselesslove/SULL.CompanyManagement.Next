/*
 *所有关于HandoverCheck类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*HandoverCheckService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
*/
using VOL.Core.BaseProvider;
using VOL.Core.Extensions.AutofacManager;
using VOL.Entity.DomainModels;
using System.Linq;
using VOL.Core.Utilities;
using System.Linq.Expressions;
using VOL.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using SULL.HospitalManagement.IRepositories;
using SULL.HospitalManagement.IServices;
using VOL.Core.ManageUser;
using VOL.Core.RabbitMQ;

namespace SULL.HospitalManagement.Services
{
    public partial class HandoverCheckService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHandoverCheckRepository _repository; //访问数据库
        private readonly IHandoverRepository _handoverRepository;
        private readonly IHandoverService _handoverService;
        private readonly IMessageProducer _messageProducer;

        [ActivatorUtilitiesConstructor]
        public HandoverCheckService(
            IHandoverCheckRepository dbRepository,
            IHttpContextAccessor httpContextAccessor,
            IHandoverRepository handoverRepository,
            IHandoverService handoverService, IMessageProducer messageProducer)
            : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            _handoverRepository = handoverRepository;
            _handoverService = handoverService;
            _messageProducer = messageProducer;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        public override WebResponseContent Add(SaveModel saveModel)
        {
            if (saveModel.MainData.TryGetValue("HandoverId", out object handoverIdObj) && handoverIdObj != null
                && saveModel.MainData.TryGetValue("CheckId", out object checkIdObj) && checkIdObj != null)
            {
                long handoverId = handoverIdObj.ToString().GetLong();
                int checkId = checkIdObj.ToString().GetInt();

                int counter = 0;
                if (saveModel.MainData.TryGetValue("Counter", out object counterObj))
                {
                    counter = counterObj.ToString().GetInt();
                }

                var handover = _handoverRepository.FindFirst(x => x.Id == handoverId);
                if (handover == null)
                    return new WebResponseContent().Error("交接班记录不存在");

                if (handover.AuditStatus >= 10)
                    return new WebResponseContent().Error("交接班已完成，不允许修改");

                if (handoverId > 0 && checkId > 0)
                {
                    WebResponseContent response;
                    // 使用通用细粒度锁工具，Key包含业务前缀防止冲突
                    response = KeyLock.Lock($"HandoverCheck_Add_{handoverId}", () =>
                    {
                        var exist = _repository.FindFirst(x => x.HandoverId == handoverId && x.CheckId == checkId);
                        if (exist != null)
                        {
                            saveModel.MainData["Id"] = exist.Id;
                            return base.Update(saveModel);
                        }
                        else
                        {
                            return base.Add(saveModel);
                        }
                    });
                    if (response.Status && counterObj != null)
                    {
                        if (counter > 0)
                        {
                            var count = _repository.FindAsIQueryable(x =>
                                x.HandoverId == handoverId && !string.IsNullOrEmpty(x.TextResult)).Count();
                            if (count == counter)
                            {
                                var updateModel = new SaveModel();
                                updateModel.MainData = new Dictionary<string, object>
                                    {
                                        { "Id", handover.Id },
                                        { "CheckCounter", counter },
                                        { "AuditStatus", 1 },
                                    };
                                // 英文：When all check items are completed (count equals counter), update the main Handover record status to 1 (Confirmed/Pending Audit).
                                // 中文：当所有检查项都完成时（数量相等），更新主交接班记录的状态为1（已确认/待审核）。
                                var updateResponse = _handoverService.Update(updateModel);

                                // 英文：Check the update result to ensure data consistency. If the update fails, return the error response.
                                // 中文：检查更新结果以确保数据一致性。如果更新失败，返回错误响应。
                                if (!updateResponse.Status) return updateResponse;

                                // 发送消息
                                var todo = new SaveModel();
                                todo.MainData = new Dictionary<string, object>();
                                todo.MainData.Add("RelatedId", handover.Id);
                                todo.MainData.Add("Category", 40);
                                todo.MainData.Add("Description", $"{handover.Creator}向您提交了交班申请");
                                todo.MainData.Add("LatestAssignUserId", handover.ReceiverId);
                                todo.MainData.Add("LatestAssignUser", handover.ReceiverName);
                                todo.MainData.Add("Kind",1);


                                var detail = new Dictionary<string, object>();
                                detail.Add("Description", "交接班申请");
                                detail.Add("Status", 0);
                                todo.DetailData = new List<Dictionary<string, object>>();
                                todo.DetailData.Add(detail);

                                // TODO 后台线程无法获取用户信息
                                todo.MainData.Add("UserInfo", UserContext.Current.UserInfo);
                                todo.MainData.Add("IsSuperAdmin", UserContext.Current.IsSuperAdmin);

                                _messageProducer.PublishAsync<SaveModel>(todo, Define.ExchangeName,
                                    Define.TodoQueue);
                            }
                        }
                    }
                    return response;
                }
            }
            return base.Add(saveModel);
        }
    }
}

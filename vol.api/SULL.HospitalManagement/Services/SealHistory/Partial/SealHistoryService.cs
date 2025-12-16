/*
 *所有关于SealHistory类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*SealHistoryService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
using VOL.Core.ManageUser;
using VOL.Core.RabbitMQ;

namespace SULL.HospitalManagement.Services
{
    public partial class SealHistoryService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISealHistoryRepository _repository;//访问数据库

        private readonly IMessageProducer _messageProducer;

        [ActivatorUtilitiesConstructor]
        public SealHistoryService(
            ISealHistoryRepository dbRepository,
            IHttpContextAccessor httpContextAccessor, IMessageProducer messageProducer
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            _messageProducer = messageProducer;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        public override WebResponseContent Add(SaveModel saveModel)
        {
            saveModel.MainData.Add("Status", 0);
            base.AddOnExecuted = (sealHistory, response) =>
            {
                //新增成功后，给确认人发送一条确认消息
                if (sealHistory.AssignUserId != null)
                {
                    // 发送消息
                    var todo = new SaveModel();
                    todo.MainData = new Dictionary<string, object>();
                    todo.MainData.Add("RelatedId", sealHistory.Id);
                    todo.MainData.Add("Category", 40);
                    todo.MainData.Add("Description", $"{sealHistory.Creator}向您提交了电子封存申请");
                    todo.MainData.Add("LatestAssignUserId", sealHistory.AssignUserId);
                    todo.MainData.Add("LatestAssignUser", sealHistory.AssignUser);
                    todo.MainData.Add("Kind", 2);

                    var detail = new Dictionary<string, object>();
                    detail.Add("Description", "电子封存申请");
                    detail.Add("Status", 0);
                    todo.DetailData = new List<Dictionary<string, object>>();
                    todo.DetailData.Add(detail);

                    // TODO 后台线程无法获取用户信息
                    todo.MainData.Add("UserInfo", UserContext.Current.UserInfo);
                    todo.MainData.Add("IsSuperAdmin", UserContext.Current.IsSuperAdmin);
                    _messageProducer.PublishAsync(todo, Define.ExchangeName, Define.TodoQueue).GetAwaiter().GetResult();
                }
                return new WebResponseContent(true);
            };
            return base.Add(saveModel);
        }

        public override WebResponseContent Update(SaveModel saveModel)
        {
            if (saveModel.MainData.TryGetValue("Status", out object status) && status != null)
            {
                string statusStr = status.ToString();
                if (statusStr == "1")
                {
                    if (saveModel.MainData.TryGetValue("Id", out object id) && id != null && long.TryParse(id.ToString(), out long handoverId))
                    {
                        var entity = _repository.FindFirst(x => x.Id == handoverId);
                        if (entity == null)
                        {
                            return new WebResponseContent().Error("数据不存在");
                        }
                        if (entity.Status != 0)
                        {
                            return new WebResponseContent().Error("非法操作");
                        }
                    }
                }
            }
            return base.Update(saveModel);
        }
  }
}

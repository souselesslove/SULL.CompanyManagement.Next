/*
 *所有关于TodoList类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*TodoListService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using VOL.Core.ManageUser;
using SULL.HospitalManagement.IServices;
using Yitter.IdGenerator;

namespace SULL.HospitalManagement.Services
{
    public partial class TodoListService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITodoListRepository _repository;//访问数据库
        private readonly ITodoListDetailRepository _todoListDetailRepository;

        [ActivatorUtilitiesConstructor]
        public TodoListService(
            ITodoListRepository dbRepository,
            IHttpContextAccessor httpContextAccessor, ITodoListDetailRepository todoListDetailRepository)
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _todoListDetailRepository = todoListDetailRepository;
            _repository = dbRepository;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        public override PageGridData<TodoList> GetPageData(PageDataOptions options)
        {
            if (options.Filter == null)
            {
                options.Filter = new List<SearchParameters>();
            }
            options.Filter.Add(new SearchParameters()
            {
                Name = "LatestAssignUserId",
                Value = UserContext.Current.UserId.ToString(),
            });
            options.Filter.Add(new SearchParameters()
            {
                Name = "IsFinished",
                Value = "0",
            });
            return base.GetPageData(options);
        }

        public override WebResponseContent Add(SaveModel saveDataModel)
        {
            var desc = saveDataModel.DetailData[0]["Description"].ToString();
            var status = saveDataModel.DetailData[0]["Status"].GetByte();
            saveDataModel.DetailData = null;
            base.AddOnExecuted = (obj, o) =>
            {
                var did = YitIdHelper.NextId();
                var detail = new TodoListDetail()
                {
                    Id = did,
                    TodoListId = obj.Id,
                    Description = desc,
                    Status = status,
                    CreatorId = obj.CreatorId,
                    Creator = obj.Creator,
                    CreationTime = DateTime.Now,
                    IsDeleted = false,
                };
                detail.AssignUserId = obj.LatestAssignUserId;
                detail.AssignUser = obj.LatestAssignUser;
                _todoListDetailRepository.Add(detail, true);
                obj.LatestStatusId = did;
                obj.LatestStatus = status;
                obj.LatestTime = detail.CreationTime;
                _repository.Update(obj, true);
                return new WebResponseContent(true);
            };

            return base.Add(saveDataModel);
        }
    }
}

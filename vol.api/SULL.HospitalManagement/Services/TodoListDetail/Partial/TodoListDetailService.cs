/*
 *所有关于TodoListDetail类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*TodoListDetailService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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

namespace SULL.HospitalManagement.Services
{
    public partial class TodoListDetailService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITodoListDetailRepository _repository; //访问数据库

        [ActivatorUtilitiesConstructor]
        public TodoListDetailService(
            ITodoListDetailRepository dbRepository,
            IHttpContextAccessor httpContextAccessor
        )
            : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        public override WebResponseContent Add(SaveModel saveModel)
        {
            AddOnExecuted = (detail, response) =>
            {
                var db = _repository.DbContext;
                var todoList = db.Set<TodoList>().FirstOrDefault(x => x.Id == detail.TodoListId);
                if (todoList != null)
                {
                    // 更新TodoList的最新状态信息
                    todoList.LatestStatusId = detail.Id;
                    todoList.LatestStatus = detail.Status;
                    todoList.LatestTime = detail.CreationTime ?? DateTime.Now;
                    // 如果明细中有指派人信息，则更新TodoList的指派人，否则保持原样或根据业务需求调整
                    // 这里假设新增明细的操作人或指派人是新的LatestAssignUser
                    if (detail.AssignUserId != null)
                    {
                        todoList.LatestAssignUserId = detail.AssignUserId;
                        todoList.LatestAssignUser = detail.AssignUser;
                    }

                    // 当新增的状态>=200时，表示完成
                    if (detail.Status >= 200)
                    {
                        todoList.IsFinished = true;
                        todoList.FinishTime = DateTime.Now;
                        todoList.FinalStatus = detail.Status;
                        todoList.Result = detail.Description;
                    }

                    db.Entry(todoList).State = EntityState.Modified;
                    db.SaveChanges();
                }

                return new WebResponseContent(true);
            };
            return base.Add(saveModel);
        }
    }
}

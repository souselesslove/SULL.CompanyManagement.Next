/*
 *所有关于UserAccount类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*UserAccountService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
using VOL.Core.CacheManager;

namespace SULL.HospitalManagement.Services
{
    public partial class UserAccountService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserAccountRepository _repository;//访问数据库
        private readonly ICacheService _cache;

        [ActivatorUtilitiesConstructor]
        public UserAccountService(
            IUserAccountRepository dbRepository,
            IHttpContextAccessor httpContextAccessor,
            ICacheService cache
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            _cache = cache;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        public override WebResponseContent Add(SaveModel saveModel)
        {
            int userId = UserContext.Current.UserId;
            if (userId <= 0)
            {
                return new WebResponseContent().Error("请先登录系统");
            }
            saveModel.MainData["UserId"] = userId;

            // 获取Category
            if (saveModel.MainData.TryGetValue("Category", out object categoryObj) && categoryObj != null)
            {
                int category = categoryObj.ToString().GetInt();
                var db = _repository.DbContext;
                // 确保使用 AsTracking 跟踪实体状态，并处理 IsDeleted 为 null 的情况
                var existingAccounts = db.Set<UserAccount>()
                    .Where(x => x.UserId == userId && x.Category == (byte)category && (x.IsDeleted == null || x.IsDeleted == false))
                    .AsTracking()
                    .ToList();

                if (existingAccounts.Any())
                {
                    foreach (var item in existingAccounts)
                    {
                        if(item.Account == saveModel.MainData["Account"].ToString()) continue;
                        item.IsDeleted = true;
                        item.DeletionTime = DateTime.Now;
                    }
                    db.SaveChanges();
                }
            }

            AddOnExecuted = (newEntity, list) =>
            {
                if (newEntity.UserId != null && newEntity.Category != null)
                {
                    string cacheKey = $"UserAccount_{newEntity.UserId}_{newEntity.Category}_Account";
                    _cache.AddObject(cacheKey, newEntity);
                }
                return new WebResponseContent(true);
            };

            return base.Add(saveModel);
        }
  }
}

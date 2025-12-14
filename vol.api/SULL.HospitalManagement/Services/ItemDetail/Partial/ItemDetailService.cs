/*
 *所有关于ItemDetail类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*ItemDetailService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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

namespace SULL.HospitalManagement.Services
{
    public partial class ItemDetailService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IItemDetailRepository _repository;//访问数据库

        [ActivatorUtilitiesConstructor]
        public ItemDetailService(
            IItemDetailRepository dbRepository,
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
            AddOnExecuted = (itemDetail, response) =>
            {
                UpdateItemInfo(itemDetail.ItemId);
                return new WebResponseContent(true);
            };
            return base.Add(saveModel);
        }

        public override WebResponseContent Del(object[] keys, bool delList = true)
        {
            // 删除前查询涉及的ItemId
            var itemIds = new HashSet<long>();
            if (keys != null && keys.Length > 0)
            {
                var longKeys = keys.Select(x => x.ToString().GetLong()).ToList();
                var details = _repository.Find(x => longKeys.Contains(x.Id)).Select(x => x.ItemId).Distinct().ToList();
                foreach (var id in details)
                {
                    itemIds.Add(id);
                }
            }

            DelOnExecuted = (k) =>
            {
                foreach (var itemId in itemIds)
                {
                    UpdateItemInfo(itemId);
                }
                return new WebResponseContent(true);
            };

            return base.Del(keys, delList);
        }

        private void UpdateItemInfo(long itemId)
        {
            var db = _repository.DbContext;
            // 排除逻辑删除的数据
            var details = _repository.Find(x => x.ItemId == itemId && x.IsDeleted != true);

            var item = db.Set<Item>().FirstOrDefault(x => x.Id == itemId);
            if (item != null)
            {
                item.KindCounter = details.Count;
                item.AvailableStock = details.Sum(x => x.Stock ?? 0);
                // 最近过期日期：忽略已过期的，取最早的过期时间
                item.LatestExpirationDate = details.Where(x => x.ExpirationDate > DateTime.Now).Min(x => (DateTime?)x.ExpirationDate);
                // 更新修改信息
                item.LastModificationTime = DateTime.Now;
                item.LastModifierId = UserContext.Current.UserId;
                item.LastModifier = UserContext.Current.UserTrueName;

                db.Entry(item).State = EntityState.Modified;
                db.SaveChanges();
            }
        }
    }
}

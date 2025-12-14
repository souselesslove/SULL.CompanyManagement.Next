/*
 *所有关于Organization类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*OrganizationService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using SULL.HospitalManagement.IServices;
using Yitter.IdGenerator;
using Azure;
using VOL.Core.Enums;

namespace SULL.HospitalManagement.Services
{
    public partial class OrganizationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOrganizationRepository _repository; //访问数据库
        private readonly IOrganizationUserRepository _organizationUserRepository;

        [ActivatorUtilitiesConstructor]
        public OrganizationService(
            IOrganizationRepository dbRepository,
            IHttpContextAccessor httpContextAccessor, IOrganizationUserRepository organizationUserRepository)
            : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _organizationUserRepository = organizationUserRepository;
            _repository = dbRepository;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        public virtual async Task<PageGridData<Organization>> GetUserOrganizationAsync(
            CancellationToken cancellationToken = default)
        {
            var pageGridData = new PageGridData<Organization>();
            var userId = UserContext.Current.UserId;
            var db = _repository.DbContext;
            var qry = from a in db.Set<Organization>()
                join b in db.Set<OrganizationUser>() on a.Id equals b.OrganizationId
                where b.UserId == userId
                select a;
            pageGridData.total = await qry.CountAsync(cancellationToken);
            pageGridData.rows = await qry.ToListAsync(cancellationToken);
            return pageGridData;
        }

        public override WebResponseContent Add(SaveModel saveModel)
        {
            base.AddOnExecute = model =>
            {
                var exist = _repository.FindFirst(x => x.Name == saveModel.MainData["Name"].ToString());
                return exist == null ? new WebResponseContent(true) : new WebResponseContent().Error("机构已存在");
            };
            base.AddOnExecuting = (organization, o) =>
            {
                organization.UserCounter = 1;
                return new WebResponseContent(true);
            };
            base.AddOnExecuted = (organization, obj) =>
            {
                var ou = new OrganizationUser();
                ou.Id = YitIdHelper.NextId();
                ou.UserId = UserContext.Current.UserId;
                ou.OrganizationId = organization.Id;
                ou.UserName = UserContext.Current.UserTrueName;
                ou.CreatorId = UserContext.Current.UserId;
                ou.Creator = UserContext.Current.UserTrueName;
                ou.CreationTime = DateTime.Now;
                ou.AuditStatus = 0;
                ou.Category = 0;
                ou.Level = 1;
                _organizationUserRepository.Add(ou, true);
                return new WebResponseContent(true).OK(ResponseType.SaveSuccess); 
            };
            // 使用细粒度锁防止重复添加机构
            string key = $"Organization_Add_{saveModel.MainData["Name"]}";
            return KeyLock.Lock(key, () =>
            {
                return base.Add(saveModel);
            });
        }
    }
}

/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 */
using VOL.Core.BaseProvider;
using VOL.Entity.DomainModels;

namespace SULL.HospitalManagement.IServices
{
    public partial interface IOrganizationService : IService<Organization>
    {
        /// <summary>
        /// 更新机构用户计数器
        /// </summary>
        /// <param name="orgId">机构ID</param>
        /// <param name="delta">增量</param>
        System.Threading.Tasks.Task UpdateUserCounterAsync(long orgId, int delta);
        /// <summary>
        /// 更新机构部门计数器
        /// </summary>
        /// <param name="orgId">机构ID</param>
        /// <param name="delta">增量</param>
        System.Threading.Tasks.Task UpdateDepartmentCounterAsync(long orgId, int delta);
    }
}

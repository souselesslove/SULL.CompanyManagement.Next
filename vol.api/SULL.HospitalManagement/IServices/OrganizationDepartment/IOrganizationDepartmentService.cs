/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 */
using VOL.Core.BaseProvider;
using VOL.Entity.DomainModels;

namespace SULL.HospitalManagement.IServices
{
    public partial interface IOrganizationDepartmentService : IService<OrganizationDepartment>
    {
        /// <summary>
        /// 更新部门用户计数器
        /// </summary>
        /// <param name="deptId">部门ID</param>
        /// <param name="delta">增量</param>
        System.Threading.Tasks.Task UpdateUserCounterAsync(long deptId, int delta);
    }
}

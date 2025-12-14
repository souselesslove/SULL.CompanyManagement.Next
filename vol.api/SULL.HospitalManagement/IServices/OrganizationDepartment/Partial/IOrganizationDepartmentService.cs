/*
*所有关于OrganizationDepartment类的业务代码接口应在此处编写
*/
using VOL.Core.BaseProvider;
using VOL.Entity.DomainModels;
using VOL.Core.Utilities;
using System.Linq.Expressions;
using SULL.HospitalManagement.IServices.Dtos;

namespace SULL.HospitalManagement.IServices
{
    public partial interface IOrganizationDepartmentService
    {
        Task<PageGridData<OrganizationDepartment>> GetUserDepartmentAsync(CancellationToken cancellationToken = default);
        Task<PageGridData<OrganizationDepartment>> GetOrganizationDepartmentAsync(CancellationToken cancellationToken = default);
    }
}

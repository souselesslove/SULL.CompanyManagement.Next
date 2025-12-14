/*
*所有关于OrganizationUser类的业务代码接口应在此处编写
*/
using VOL.Core.BaseProvider;
using VOL.Entity.DomainModels;
using VOL.Core.Utilities;
using System.Linq.Expressions;
using SULL.HospitalManagement.IServices.Dtos;
namespace SULL.HospitalManagement.IServices
{
    public partial interface IOrganizationUserService
    {
        Task<UserOrganizationDto> GetUserOrganizationAsync(CancellationToken cancellationToken = default);
        Task<PageGridData<OrganizationUser>> GetDepartmentUserAsync(CancellationToken cancellationToken = default);
    }
}

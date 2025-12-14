/*
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下OrganizationDepartmentService与IOrganizationDepartmentService中编写
 */
using SULL.HospitalManagement.IRepositories;
using SULL.HospitalManagement.IServices;
using VOL.Core.BaseProvider;
using VOL.Core.Extensions.AutofacManager;
using VOL.Entity.DomainModels;

namespace SULL.HospitalManagement.Services
{
    public partial class OrganizationDepartmentService : ServiceBase<OrganizationDepartment, IOrganizationDepartmentRepository>
    , IOrganizationDepartmentService, IDependency
    {
    public static IOrganizationDepartmentService Instance
    {
      get { return AutofacContainerModule.GetService<IOrganizationDepartmentService>(); } }
    }
 }

/*
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下DbConfigService与IDbConfigService中编写
 */
using SULL.HospitalManagement.IRepositories;
using SULL.HospitalManagement.IServices;
using VOL.Core.BaseProvider;
using VOL.Core.Extensions.AutofacManager;
using VOL.Entity.DomainModels;

namespace SULL.HospitalManagement.Services
{
    public partial class DbConfigService : ServiceBase<DbConfig, IDbConfigRepository>
    , IDbConfigService, IDependency
    {
    public static IDbConfigService Instance
    {
      get { return AutofacContainerModule.GetService<IDbConfigService>(); } }
    }
 }

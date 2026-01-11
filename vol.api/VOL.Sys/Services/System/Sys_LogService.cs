/*
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下Sys_LogService与ISys_LogService中编写
 */
using VOL.Sys.IRepositories;
using VOL.Sys.IServices;
using VOL.Core.BaseProvider;
using VOL.Core.Extensions.AutofacManager;
using VOL.Entity.DomainModels;

namespace VOL.Sys.Services
{
    public partial class Sys_LogService : ServiceBase<Sys_Log, ISys_LogRepository>
    , ISys_LogService, IDependency
    {
    public static ISys_LogService Instance
    {
      get { return AutofacContainerModule.GetService<ISys_LogService>(); } }
    }
 }

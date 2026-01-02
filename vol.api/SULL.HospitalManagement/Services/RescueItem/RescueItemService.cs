/*
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下RescueItemService与IRescueItemService中编写
 */
using SULL.HospitalManagement.IRepositories;
using SULL.HospitalManagement.IServices;
using VOL.Core.BaseProvider;
using VOL.Core.Extensions.AutofacManager;
using VOL.Entity.DomainModels;

namespace SULL.HospitalManagement.Services
{
    public partial class RescueItemService : ServiceBase<RescueItem, IRescueItemRepository>
    , IRescueItemService, IDependency
    {
    public static IRescueItemService Instance
    {
      get { return AutofacContainerModule.GetService<IRescueItemService>(); } }
    }
 }

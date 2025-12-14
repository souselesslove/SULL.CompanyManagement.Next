/*
 *Author：jxx
 *Contact：283591387@qq.com
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下DeviceService与IDeviceService中编写
 */
using SULL.Devices.IRepositories;
using SULL.Devices.IServices;
using VOL.Core.BaseProvider;
using VOL.Core.Extensions.AutofacManager;
using VOL.Entity.DomainModels;

namespace SULL.Devices.Services
{
    public partial class DeviceService : ServiceBase<Device, IDeviceRepository>
    , IDeviceService, IDependency
    {
    public static IDeviceService Instance
    {
      get { return AutofacContainerModule.GetService<IDeviceService>(); } }
    }
 }

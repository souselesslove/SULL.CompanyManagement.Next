/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹DeviceController编写
 */
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using VOL.Core.Controllers.Basic;
using VOL.Entity.AttributeManager;
using SULL.Devices.IServices;
using VOL.WebApi.Controllers.Hubs;

namespace SULL.Devices.Controllers
{
    [Route("api/Device")]
    [PermissionTable(Name = "Device")]
    public partial class DeviceController : ApiBaseController<IDeviceService>
    {
        public DeviceController(IDeviceService service,IHubContext<MLPrinterHub> hubContext, MLPrinterHubClientHelper hubHelper)
        : base(service)
        {
            _hubHelper = hubHelper;
            _hubContext = hubContext;
        }
    }
}


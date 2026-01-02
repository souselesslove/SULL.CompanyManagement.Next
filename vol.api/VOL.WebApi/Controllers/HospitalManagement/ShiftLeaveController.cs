/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹ShiftLeaveController编写
 */
using Microsoft.AspNetCore.Mvc;
using VOL.Core.Controllers.Basic;
using VOL.Entity.AttributeManager;
using SULL.HospitalManagement.IServices;
namespace SULL.HospitalManagement.Controllers
{
    [Route("api/ssbh/ShiftLeave")]
    [PermissionTable(Name = "ShiftLeave")]
    public partial class ShiftLeaveController : ApiBaseController<IShiftLeaveService>
    {
    public ShiftLeaveController(IShiftLeaveService service)
    : base(service)
    {
    }
    }
    }


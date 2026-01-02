/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹RescueStaffController编写
 */
using Microsoft.AspNetCore.Mvc;
using VOL.Core.Controllers.Basic;
using VOL.Entity.AttributeManager;
using SULL.HospitalManagement.IServices;
namespace SULL.HospitalManagement.Controllers
{
    [Route("api/ssbh/RescueStaff")]
    [PermissionTable(Name = "RescueStaff")]
    public partial class RescueStaffController : ApiBaseController<IRescueStaffService>
    {
    public RescueStaffController(IRescueStaffService service)
    : base(service)
    {
    }
    }
    }


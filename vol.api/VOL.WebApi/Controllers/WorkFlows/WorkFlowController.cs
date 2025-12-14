/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹WorkFlowController编写
 */
using Microsoft.AspNetCore.Mvc;
using VOL.Core.Controllers.Basic;
using VOL.Entity.AttributeManager;
using SULL.WorkFlows.IServices;
namespace SULL.WorkFlows.Controllers
{
    [Route("api/WorkFlow")]
    [PermissionTable(Name = "WorkFlow")]
    public partial class WorkFlowController : ApiBaseController<IWorkFlowService>
    {
        public WorkFlowController(IWorkFlowService service)
        : base(service)
        {
        }
    }
}


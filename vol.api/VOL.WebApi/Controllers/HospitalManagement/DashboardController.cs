using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SULL.HospitalManagement.IServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using VOL.Core.Controllers.Basic;
using VOL.Core.Filters;
using VOL.Entity.DomainModels;

namespace SULL.HospitalManagement.Controllers
{
    [Route("api/ssbh/Dashboard")]
    public class DashboardController : ApiBaseController<IChecklistHistoryService>
    {
        private readonly IDashboardService _service;

        public DashboardController(IDashboardService service)
        {
            _service = service;
        }

        [AllowAnonymous]
        [HttpPost, Route("Get")]
        public IActionResult Get([FromBody] PageDataOptions options)
        {
            return Json(_service.GetDashboardData(options));
        }
    }
}

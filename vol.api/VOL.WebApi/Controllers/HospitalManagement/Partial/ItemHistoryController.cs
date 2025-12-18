/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("ItemHistory",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using VOL.Entity.DomainModels;
using SULL.HospitalManagement.IServices;
using SULL.HospitalManagement.Services;
using VOL.Core.Filters;

namespace SULL.HospitalManagement.Controllers
{
    public partial class ItemHistoryController
    {
        private readonly IItemHistoryService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public ItemHistoryController(
            IItemHistoryService service,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost, Route("Statistics")]
        public ActionResult Statistics([FromBody] PageDataOptions options)
        {
            return Json(_service.Statistics(options));
        }

        [HttpPost, Route("SetAllStock")]
        [ApiActionPermission("ItemHistory", VOL.Core.Enums.ActionPermissionOptions.Add)]
        public ActionResult SetAllStock([FromBody] SaveModel saveModel)
        {
            return Json(_service.SetAllStock(saveModel));
        }
    }
}

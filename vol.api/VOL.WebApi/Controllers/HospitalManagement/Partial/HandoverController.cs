/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("Handover",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using VOL.Entity.DomainModels;
using SULL.HospitalManagement.IServices;
using SULL.HospitalManagement.IServices.Handover.Dtos;
using VOL.Core.Utilities;

namespace SULL.HospitalManagement.Controllers
{
    public partial class HandoverController
    {
        private readonly IHandoverService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public HandoverController(
            IHandoverService service,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet, Route("getMissingShifts")]
        [AllowAnonymous]
        public WebResponseContent GetMissingShifts([FromQuery] MissingShiftInputDto input)
        {
            if(input.AccessToken != "5EAA3F1B5882A5D1637A841720828C5C")
                return new WebResponseContent().Error("非法访问");
            return _service.GetMissingShifts(input);
        }
    }
}

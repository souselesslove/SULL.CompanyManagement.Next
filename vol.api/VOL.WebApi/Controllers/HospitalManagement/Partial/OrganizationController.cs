/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("Organization",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using VOL.Entity.DomainModels;
using SULL.HospitalManagement.IServices;
using VOL.Core.Filters;

namespace SULL.HospitalManagement.Controllers
{
    public partial class OrganizationController
    {
        private readonly IOrganizationService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public OrganizationController(
            IOrganizationService service,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet, Route("UserOrganization")]
        public async Task<IActionResult> GetUserOrganizationAsync(CancellationToken cancellationToken = default)
        {
            return JsonNormal(await Service.GetUserOrganizationAsync(cancellationToken));
        }
    }
}

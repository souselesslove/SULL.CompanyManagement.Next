/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("OrganizationDepartment",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using VOL.Entity.DomainModels;
using SULL.HospitalManagement.IServices;
using System.Threading;

namespace SULL.HospitalManagement.Controllers
{
    public partial class OrganizationDepartmentController
    {
        private readonly IOrganizationDepartmentService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public OrganizationDepartmentController(
            IOrganizationDepartmentService service,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet, Route("UserDepartment")]
        public async Task<IActionResult> GetUserDepartmentAsync(CancellationToken cancellationToken = default)
        {
            return JsonNormal(await Service.GetUserDepartmentAsync(cancellationToken));
        }

        [HttpGet, Route("OrganizationDepartment")]
        public async Task<IActionResult> GetOrganizationDepartmentAsync(CancellationToken cancellationToken = default)
        {
            return JsonNormal(await Service.GetOrganizationDepartmentAsync(cancellationToken));
        }
    }
}

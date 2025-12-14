/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("WorkFlow",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SULL.WorkFlows.IServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VOL.Entity.AttributeManager;
using VOL.Entity.DomainModels;

namespace SULL.WorkFlows.Controllers
{
    public partial class WorkFlowController
    {
        private readonly IWorkFlowService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public WorkFlowController(
            IWorkFlowService service,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }
    }
}

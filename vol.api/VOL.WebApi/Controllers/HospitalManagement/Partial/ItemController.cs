/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("Item",Enums.ActionPermissionOptions.Search)]
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
using VOL.Core.Filters;
using VOL.Core.Utilities;

namespace SULL.HospitalManagement.Controllers
{
    public partial class ItemController
    {
        private readonly IItemService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public ItemController(
            IItemService service,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet, Route("CheckItemWarnings")]
        [AllowAnonymous]
        [LocalNetworkOnly]
        public async Task<WebResponseContent> CheckItemWarnings(long id = 0)
        {
            return await _service.CheckItemWarningsAsync(id);
        }

        [HttpGet, Route("Remark")]
        [AllowAnonymous]
        public async Task<WebResponseContent> GetItemRemark(string combinedId)
        {
            return await _service.GetItemRemarkAsync(combinedId);
        }

        [HttpPost, Route("Refresh")]
        public WebResponseContent Refresh([FromBody] long? id)
        {
            return _service.Refresh(id);
        }
    }
}

/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("Device",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SULL.Devices.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SULL.Devices.Services.Devices.Dtos;
using VOL.Entity.DomainModels;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using VOL.WebApi.Controllers.Hubs;
using VOL.Entity.AttributeManager;
using Quartz.Util;
using System.Reflection;

namespace SULL.Devices.Controllers
{
    [Route("api/ssbh")]
    public partial class DeviceController
    {
        private readonly IDeviceService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHubContext<MLPrinterHub> _hubContext;
        private readonly MLPrinterHubClientHelper _hubHelper;
        private readonly IConfiguration _configuration;
        private readonly string KeyIp = "ip";

        [ActivatorUtilitiesConstructor]
        public DeviceController(
            IDeviceService service,
            IHttpContextAccessor httpContextAccessor, IHubContext<MLPrinterHub> hubContext, MLPrinterHubClientHelper hubHelper, IConfiguration configuration)
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
            _hubContext = hubContext;
            _hubHelper = hubHelper;
            _configuration = configuration;
        }

        /*
        [AllowAnonymous]
        [HttpGet("Register")]
        public async Task<IActionResult> RegisterAsync(DeviceRegisterInputDto input)
        {
            return Json(_service.GetRegisterAsync(input));
        }
        */

        [AllowAnonymous]
        [HttpPost("PrintCatheter")]
        public async Task<IActionResult> PrintCatheterAsync(string message, string selfId, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Deserialize<Dictionary<string, object>>(message);
            if (string.IsNullOrWhiteSpace(selfId))
            {
                if (json.TryGetValue(KeyIp, out var ip))
                {
                    selfId = ip?.ToString();
                }
            }
            else
            {
                if (json.ContainsKey(KeyIp))
                {
                    json[KeyIp] = selfId;
                    message = JsonSerializer.Serialize(json);
                }
            }
            var printers = new List<Printer>();
            _configuration.GetSection("printers").Bind(printers);
            var exist = printers.FirstOrDefault(a => a.ComputerIp == selfId);
            if (exist != null)
            {
                var ok = await _hubHelper.StartAsync(_hubContext, exist.DeviceCode, message);
                await Task.Delay(4000, cancellationToken);
                return Json(ok);
            }
            return Json(new PrintResultDto(-2, $"客户端'{selfId}'未找到对应打印机"));
        }

        [AllowAnonymous]
        [HttpGet("PrintCatheter")]
        public async Task<IActionResult> PrintCatheterTestAsync(CancellationToken cancellationToken = default)
        {
            return Ok("{\r\n        \"id\": \"1274EC68-E644-7AB9-DB5C-2296DF2EFAC6\",\r\n        \"ip\": \"192.168.0.11\",\r\n        \"category\": \"22\",\r\n        \"copies\": 1,\r\n        \"port\": -1,\r\n        \"params\": [\r\n            {\r\n                \"name\": \"personId\",\r\n                \"value\": \"1202503310001\"\r\n            },\r\n            {\r\n                \"name\": \"personName\",\r\n                \"value\": \"张静初\"\r\n            },\r\n            {\r\n                \"name\": \"level\",\r\n                \"value\": \"高危\"\r\n            },\r\n            {\r\n                \"name\": \"bedNO\",\r\n                \"value\": \"12\"\r\n            },\r\n            {\r\n                \"name\": \"inside\",\r\n                \"value\": \"30\"\r\n            },\r\n            {\r\n                \"name\": \"outside\",\r\n                \"value\": \"80\"\r\n            },\r\n            {\r\n                \"name\": \"name\",\r\n                \"value\": \"动脉导管\"\r\n            },\r\n            {\r\n                \"name\": \"extendLabel\",\r\n                \"value\": \"流速\"\r\n            },\r\n            {\r\n                \"name\": \"extendValue\",\r\n                \"value\": \"0.5\"\r\n            },\r\n            {\r\n                \"name\": \"signature\",\r\n                \"value\": \"张三丰\"\r\n            },\r\n            {\r\n                \"name\": \"start\",\r\n                \"value\": \"2025-10-21 12:00:00\"\r\n            },\r\n            {\r\n                \"name\": \"end\",\r\n                \"value\": \"2025-10-27 12:00:00\"\r\n            }\r\n        ]\r\n    }");
        }

        [AllowAnonymous]
        [HttpPost("PrintDrug")]
        public async Task<IActionResult> PrintDrugAsync(string message, string selfId, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Deserialize<Dictionary<string, object>>(message);
            if (string.IsNullOrWhiteSpace(selfId))
            {
                if (json.TryGetValue(KeyIp, out var ip))
                {
                    selfId = ip?.ToString();
                }
            }
            else
            {
                if (json.ContainsKey(KeyIp))
                {
                    json[KeyIp] = selfId;
                    message = JsonSerializer.Serialize(json);
                }
            }
            var printers = new List<Printer>();
            _configuration.GetSection("printers").Bind(printers);
            var exist = printers.FirstOrDefault(a => a.ComputerIp == selfId);
            if (exist != null)
            {
                var ok = await _hubHelper.StartAsync(_hubContext, exist.DeviceCode, message);
                await Task.Delay(4000, cancellationToken);
                return Json(ok);
            }
            return Json(new PrintResultDto(-2, $"客户端'{selfId}'未找到对应打印机"));
        }

        [AllowAnonymous]
        [HttpGet("PrintDrug")]
        public async Task<IActionResult> PrintDrugTestAsync(CancellationToken cancellationToken = default)
        {
            return Ok("{\r\n  \"id\": \"1274EC68-E644-7AB9-DB5C-2296DF2EFAC6\",\r\n        \"ip\": \"192.168.0.11\",\r\n        \"category\": \"20\",\r\n  \"copies\": 1,\r\n  \"port\": -1,\r\n  \"params\": [\r\n    {\r\n      \"name\": \"personId\",\r\n      \"value\": \"1202503310001\"\r\n    },\r\n    {\r\n      \"name\": \"personName\",\r\n      \"value\": \"张静初\"\r\n    },\r\n    {\r\n      \"name\": \"level\",\r\n      \"value\": \"高危\"\r\n    },\r\n    {\r\n      \"name\": \"bedNO\",\r\n      \"value\": \"12\"\r\n    },\r\n    {\r\n      \"name\": \"name\",\r\n      \"value\": \"d400\"\r\n    },\r\n    {\r\n      \"name\": \"dosage\",\r\n      \"value\": \"1\"\r\n    },\r\n    {\r\n      \"name\": \"dosageUnit\",\r\n      \"value\": \"mg/ml\"\r\n    },\r\n    {\r\n      \"name\": \"signature\",\r\n      \"value\": \"张三丰\"\r\n    },\r\n    {\r\n      \"name\": \"checkSignature\",\r\n      \"value\": \"陈晓太\"\r\n    }\r\n  ]\r\n}");
        }

        [AllowAnonymous]
        [HttpPost("PrintExpiry")]
        public async Task<IActionResult> PrintExpiryAsync(string message, string selfId, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Deserialize<Dictionary<string, object>>(message);
            if (string.IsNullOrWhiteSpace(selfId))
            {
                if (json.TryGetValue(KeyIp, out var ip))
                {
                    selfId = ip?.ToString();
                }
            }
            else
            {
                if (json.ContainsKey(KeyIp))
                {
                    json[KeyIp] = selfId;
                    message = JsonSerializer.Serialize(json);
                }
            }
            var printers = new List<Printer>();
            _configuration.GetSection("printers").Bind(printers);
            var exist = printers.FirstOrDefault(a => a.ComputerIp == selfId);
            if (exist != null)
            {
                var ok = await _hubHelper.StartAsync(_hubContext, exist.DeviceCode, message);
                await Task.Delay(4000, cancellationToken);
                return Json(ok);
            }
            return Json(new PrintResultDto(-2, $"客户端'{selfId}'未找到对应打印机"));
        }

        [AllowAnonymous]
        [HttpGet("PrintExpiry")]
        public async Task<IActionResult> PrintExpiryTestAsync(CancellationToken cancellationToken = default)
        {
            return Ok("{\r\n        \"id\": \"1274EC68-E644-7AB9-DB5C-2296DF2EFAC6\",\r\n        \"ip\": \"192.168.0.11\",\r\n        \"category\": \"21\",\r\n        \"copies\": 1,\r\n        \"port\": -1,\r\n        \"params\": [\r\n            {\r\n                \"name\": \"name\",\r\n                \"value\": \"一次性无菌棉球\"\r\n            },\r\n            {\r\n                \"name\": \"signature\",\r\n                \"value\": \"张三丰\"\r\n            },\r\n            {\r\n                \"name\": \"start\",\r\n                \"value\": \"2025-10-22 12:00:00\"\r\n            },\r\n            {\r\n                \"name\": \"end\",\r\n                \"value\": \"2025-10-23 12:00:00\"\r\n            }\r\n        ]\r\n    }");
        }
    }
}

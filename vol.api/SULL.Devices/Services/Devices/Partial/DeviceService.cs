/*
 *所有关于Device类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*DeviceService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
*/
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SULL.Devices.IRepositories;
using SULL.Devices.Services.Devices.Dtos;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using VOL.Core.BaseProvider;
using VOL.Core.Extensions;
using VOL.Core.Extensions.AutofacManager;
using VOL.Core.Utilities;
using VOL.Entity.DomainModels;
using Yitter.IdGenerator;

namespace SULL.Devices.Services
{
    public partial class DeviceService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDeviceRepository _repository;//访问数据库
        private readonly WebResponseContent _webResponse;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger _logger;


        [ActivatorUtilitiesConstructor]
        public DeviceService(
            IDeviceRepository dbRepository,
            IHttpContextAccessor httpContextAccessor, IWebHostEnvironment env, ILogger<DeviceService> logger)
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _env = env;
            _logger = logger;
            _repository = dbRepository;
            _webResponse = new WebResponseContent();
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        public async Task<WebResponseContent> GetRegisterAsync(DeviceRegisterInputDto input, CancellationToken cancellationToken = default)
        {
            // if (input.appVersionCode is null)
            // {
            //     return _webResponse.Error($"设备'{input.deviceId}'软件版本号为空");
            // }
            var saveModel = new SaveModel();
            saveModel.MainData = new Dictionary<string, object>();
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return _webResponse.Error($"内部错误");
            }
            var remoteIpAddress = httpContext.Connection.RemoteIpAddress;
            var ip = remoteIpAddress?.ToString();
            if (string.IsNullOrWhiteSpace(ip))
            {
                return _webResponse.Error($"未能获取到IP地址");
            }
            var inputStr = input.Serialize();
            // var exist = await _repository.ExistsAsync(x => x.Ip == ip);
            // if (!exist)
            // {
            //     saveModel.MainData.Add(nameof(Device.Ip), ip);
            //     saveModel.MainData.Add(nameof(Device.Information), inputStr);
            //     saveModel.MainData.Add(nameof(Device.ApplyTime), DateTime.Now);
            //     saveModel.MainData.Add(nameof(Device.IsRegistered), false);
            //
            //     base.Add(saveModel);
            // }
            saveModel.MainData.Add(nameof(Device.Ip), ip);
            saveModel.MainData.Add(nameof(Device.Information), inputStr);
            saveModel.MainData.Add(nameof(Device.ApplyTime), DateTime.Now);
            saveModel.MainData.Add(nameof(Device.IsRegistered), false);

            base.Add(saveModel);
            return _webResponse.OK("添加成功");
        }
    }
}

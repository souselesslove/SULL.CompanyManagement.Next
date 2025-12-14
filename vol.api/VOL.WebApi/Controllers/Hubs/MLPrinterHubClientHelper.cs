using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SULL.Devices.Services.Devices.Dtos;
using VOL.Core.Extensions.AutofacManager;

namespace VOL.WebApi.Controllers.Hubs;

public class MLPrinterHubClientHelper : IDependency
{
    public const string MethodName = "MLPrintMessage";
    private readonly ILogger _logger;

    public MLPrinterHubClientHelper(ILogger<MLPrinterHubClientHelper> logger)
    {
        _logger = logger;
    }
    public async Task<PrintResultDto> StartAsync(IHubContext<MLPrinterHub> hubContext, string selfId, string message)
    {
        var clientId = MLPrinterHub.GetClientId(selfId);
        if (string.IsNullOrWhiteSpace(clientId))
        {
            _logger.LogError("打印机'{selfId}'不存在", selfId);
            return new PrintResultDto(-1, $"打印机'{selfId}'不存在");
        }
        await hubContext.Clients.Clients(clientId).SendAsync(MethodName, message);
        return new PrintResultDto();
    }
}
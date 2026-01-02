using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SULL.HospitalManagement.UniPush;

public interface IUniPushService
{
    Task<UniPushResult> SendAsync(IEnumerable<string> clientIds, string title, string content, object payload = null);
}

public class UniPushService : IUniPushService
{
    private readonly HttpClient _httpClient;
    private readonly string _pushUrl;
    private readonly ILogger<UniPushService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public UniPushService(HttpClient httpClient, ILogger<UniPushService> logger, string pushUrl)
    {
        _httpClient = httpClient;
        _logger = logger;
        _pushUrl = pushUrl; // 你的云函数 URL，例如 https://xxxx.bspapp.com/http/pushMessage
    }

    public async Task<UniPushResult> SendAsync(IEnumerable<string> clientIds, string title, string content, object payload = null)
    {
        if (clientIds == null || !clientIds.Any())
        {
            return new UniPushResult { Success = false, Msg = "clientIds 不能为空" };
        }

        var requestBody = new
        {
            clientIds = clientIds.ToList(),
            title,
            content,
            payload = payload ?? new { }
        };

        var json = JsonSerializer.Serialize(requestBody, JsonOptions);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            _logger.LogInformation("正在发送 uniPush 推送: {Title} -> {Count} 个设备", title, clientIds.Count());

            HttpResponseMessage response = await _httpClient.PostAsync(_pushUrl, httpContent);

            string resultJson = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<UniPushResult>(resultJson, JsonOptions);
                _logger.LogInformation("uniPush 推送成功: {Result}", resultJson);
                return result ?? new UniPushResult { Success = true };
            }
            else
            {
                _logger.LogWarning("uniPush 推送失败 HTTP {Status}: {Body}", response.StatusCode, resultJson);
                return new UniPushResult { Success = false, Msg = $"HTTP {response.StatusCode}: {resultJson}" };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "uniPush 推送异常");
            return new UniPushResult { Success = false, Msg = "推送异常: " + ex.Message };
        }
    }
}
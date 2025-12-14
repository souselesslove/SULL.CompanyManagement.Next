using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace VOL.WebApi.Controllers.Hubs;

public class MLPrinterHub : Hub
{
    private static ConcurrentDictionary<string, string> _connectionIds = new ConcurrentDictionary<string, string>();
    private readonly ILogger _logger;

    public MLPrinterHub(ILogger<MLPrinterHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 建立连接时异步触发
    /// </summary>
    /// <returns></returns>
    public override async Task OnConnectedAsync()
    {
        _logger.LogDebug("建立连接{ConnectionId}", Context.ConnectionId);
        var userName = Context.GetHttpContext().Request.Query["userName"].ToString();
        if (!string.IsNullOrWhiteSpace(GetClientId(userName)))
        {
            Context.Abort();
        }
        _connectionIds[Context.ConnectionId] = userName;
        //添加到一个组下
        //await Groups.AddToGroupAsync(Context.ConnectionId, "SignalR Users");
        //发送上线消息
        //await Clients.All.SendAsync("ReceiveHomePageMessage", 1, new { title = "系统消息", content = $"{Context.ConnectionId} 上线" });
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// 离开连接时异步触发
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public override async Task OnDisconnectedAsync(Exception ex)
    {
        _logger.LogWarning("断开连接{ConnectionId}", Context.ConnectionId);
        //从组中删除
        // await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SignalR Users");
        //可自行调用下线业务处理方法...
        await UserOffline();
        //发送下线消息
        //   await Clients.All.SendAsync("ReceiveHomePageMessage", 4, new { title = "系统消息", content = $"{Context.ConnectionId} 离线" });
        await base.OnDisconnectedAsync(ex);
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    /// <returns></returns>
    public async Task<bool> UserOffline()
    {
        var cid = Context.ConnectionId;//也可以从缓存中获取ConnectionId
        //  await Clients.Client(cid).SendAsync("ReceiveHomePageMessage", 3, new { title = "系统消息", content = "离线成功" });
        //移除缓存
        if (_connectionIds.TryRemove(cid, out string value))
        {
        }
        await Task.CompletedTask;
        return true;
    }

    public static string GetClientId(string userName)
    {
        foreach (var connectionId in _connectionIds)
        {
            if (connectionId.Value == userName)
            {
                return connectionId.Key;
            }
        }
        return null;
    }
}
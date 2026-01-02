using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Net;
using System.Linq;

namespace VOL.Core.Filters
{
    /// <summary>
    /// 仅允许本地网络访问过滤器
    /// </summary>
    public class LocalNetworkOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var remoteIp = context.HttpContext.Connection.RemoteIpAddress;

            if (remoteIp == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            if (!IsLocalNetwork(remoteIp))
            {
                context.Result = new ContentResult
                {
                    Content = "该接口仅允许本地网络访问",
                    StatusCode = (int)HttpStatusCode.Forbidden
                };
            }

            base.OnActionExecuting(context);
        }

        private bool IsLocalNetwork(IPAddress address)
        {
            // 如果是 IPv4 映射的 IPv6 地址 (如 ::ffff:192.168.1.1)，先转回 IPv4
            if (address.IsIPv4MappedToIPv6)
            {
                address = address.MapToIPv4();
            }

            // 回环地址 (127.0.0.1 或 ::1)
            if (IPAddress.IsLoopback(address))
            {
                return true;
            }

            byte[] bytes = address.GetAddressBytes();

            if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                // IPv4 私有地址范围 (RFC 1918)
                // 10.0.0.0 - 10.255.255.255
                // 172.16.0.0 - 172.31.255.255
                // 192.168.0.0 - 192.168.255.255
                if (bytes[0] == 10) return true;
                if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return true;
                if (bytes[0] == 192 && bytes[1] == 168) return true;
                
                // 额外增加运营商级 NAT 范围 (100.64.0.0/10)
                if (bytes[0] == 100 && (bytes[1] & 0xC0) == 0x40) return true;
            }
            else if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                // IPv6 唯一本地地址 (fc00::/7)
                if ((bytes[0] & 0xFE) == 0xFC) return true;
                // IPv6 链路本地地址 (fe80::/10)
                if (bytes[0] == 0xFE && (bytes[1] & 0xC0) == 0x80) return true;
            }

            return false;
        }
    }
}

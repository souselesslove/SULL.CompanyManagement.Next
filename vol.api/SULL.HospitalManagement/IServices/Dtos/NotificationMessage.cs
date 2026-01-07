using System.Collections.Generic;

namespace SULL.HospitalManagement.IServices.Dtos
{
    /// <summary>
    /// 消息通知 DTO
    /// </summary>
    public class NotificationMessage
    {
        /// <summary>
        /// 接收人客户端ID列表 (UniPush ClientIds)
        /// </summary>
        public List<string> ClientIds { get; set; } = new List<string>();

        /// <summary>
        /// 消息标题
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 附加负载 (可选)
        /// </summary>
        public object Payload { get; set; } = new object();
    }
}

using Microsoft.AspNetCore.Http;

namespace VOL.Core.Const
{
    /// <summary>
    /// 加密对应密钥Key
    /// </summary>
    public class Secret
    {
        /// <summary>
        /// 用户密码加密key
        /// </summary>
        public string User { get; set; }
        /// <summary>
        /// 数据库加密key
        /// </summary>
        public string DB { get; set; }

        /// <summary>
        /// redis加密key
        /// </summary>
        public string Redis { get; set; }

        /// <summary>
        /// jwt加密key
        /// </summary>
        public string JWT { get; set; }

        public string Audience { get; set; }
        public string Issuer { get; set; }

        /// <summary>
        /// 导出文件加密key
        /// </summary>
        public string ExportFile = "kL4sC0rV3dJ4jX2cY3iQ2pO6wW3fkhyt";

        /// <summary>
        /// 新用户默认密码
        /// </summary>
        public string DefaultPassword = "ssbh1234";
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using VOL.Core.Filters;

namespace VOL.WebApi.Controllers
{
    [AllowAnonymous]
    public class ApiHomeController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public ApiHomeController(IWebHostEnvironment env, IConfiguration config)
        {
            _env = env;
            _config = config;
        }

        public IActionResult Index()
        {
            return new RedirectResult("/swagger/");
        }

        [HttpGet, Route("Version")]
        public IActionResult GetVersion()
        {
            try
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "upgrade.json");
                if (System.IO.File.Exists(path))
                {
                    string json = System.IO.File.ReadAllText(path);
                    var data = JsonConvert.DeserializeObject(json);
                    return Json(data);
                }
            }
            catch { }
            return Json(new { });
        }

        [HttpGet, Route("VersionHistory")]
        public IActionResult GetVersionHistory()
        {
            try
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "upgradeHistory.json");
                if (System.IO.File.Exists(path))
                {
                    string json = System.IO.File.ReadAllText(path);
                    var data = JsonConvert.DeserializeObject(json);
                    return Json(data);
                }
            }
            catch { }
            return Json(new { });
        }

        public class UpgradeInput
        {
            public string Version { get; set; }
            public int Force { get; set; }
            public string Date { get; set; }
            public string Description { get; set; }
        }

        [HttpPost, Route("api/ssbh/Publish"), LocalNetworkOnly]
        public IActionResult Publish([FromBody] UpgradeInput input)
        {
            if (string.IsNullOrEmpty(input.Version))
            {
                return Json(new { status = false, message = "版本号不能为空" });
            }

            // 1. 定位原始文件
            string appDir = Path.Combine(_env.WebRootPath, "app");
            string sourceFile = Path.Combine(appDir, $"{input.Version}.apk");

            if (!System.IO.File.Exists(sourceFile))
            {
                return Json(new { status = false, message = $"未找到版本文件: {input.Version}.apk 在目录 {appDir}" });
            }

            try
            {
                // 2. 计算 MD5
                string md5 = GetFileMD5(sourceFile);

                // 3. 生成新文件名和 URL
                DateTime updateDate = DateTime.Now;
                if (!string.IsNullOrEmpty(input.Date) && DateTime.TryParse(input.Date, out DateTime parsedDate))
                {
                    updateDate = parsedDate;
                }

                string timestamp = updateDate.ToString("MMddHHmmss");
                string newFileName = $"ssbh-rcms{timestamp}.apk";
                string destPath = Path.Combine(appDir, newFileName);

                // 复制并重命名文件
                System.IO.File.Copy(sourceFile, destPath, true);

                string url = $"{_config["App:Domain"]}/app/{newFileName}";

                // 4. 组装结果
                var result = new
                {
                    input.Version,
                    input.Force,
                    input.Date,
                    input.Description,
                    Url = url,
                    MD5 = md5
                };

                // 5. 写入 upgrade.json
                string rootPath = _env.ContentRootPath;
                string upgradeJsonPath = Path.Combine(rootPath, "upgrade.json");
                string upgradeHistoryJsonPath = Path.Combine(rootPath, "upgradeHistory.json");

                System.IO.File.WriteAllText(upgradeJsonPath, JsonConvert.SerializeObject(result, Formatting.Indented));

                // 6. 写入 upgradeHistory.json
                Dictionary<string, List<object>> historyObj;
                if (System.IO.File.Exists(upgradeHistoryJsonPath))
                {
                    var historyContent = System.IO.File.ReadAllText(upgradeHistoryJsonPath);
                    historyObj = JsonConvert.DeserializeObject<Dictionary<string, List<object>>>(historyContent);
                }
                else
                {
                    historyObj = new Dictionary<string, List<object>>();
                }

                if (historyObj == null) historyObj = new Dictionary<string, List<object>>();
                if (!historyObj.ContainsKey("History")) historyObj["History"] = new List<object>();

                historyObj["History"].Insert(0, result);
                System.IO.File.WriteAllText(upgradeHistoryJsonPath, JsonConvert.SerializeObject(historyObj, Formatting.Indented));

                return Json(new { status = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = $"发布失败: {ex.Message}" });
            }
        }

        private string GetFileMD5(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = System.IO.File.OpenRead(filePath))
                {
                    var hash = md5.ComputeHash(stream);
                    StringBuilder sb = new StringBuilder();
                    foreach (var b in hash)
                    {
                        sb.Append(b.ToString("x2"));
                    }
                    return sb.ToString();
                }
            }
        }
    }
}
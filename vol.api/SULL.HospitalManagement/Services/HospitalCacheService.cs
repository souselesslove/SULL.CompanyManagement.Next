using SULL.HospitalManagement.IServices;
using VOL.Core.CacheManager;
using VOL.Core.Extensions;
using VOL.Core.Extensions.AutofacManager;

namespace SULL.HospitalManagement.Services
{
    /// <summary>
    /// 医院管理系统统一缓存管理服务
    /// 集中管理：用户职级、消息账号、仪表盘日志及数据库配置缓存。
    /// 核心目标：解耦具体缓存实现，统一缓存键名规范，提高系统可维护性。
    /// </summary>
    public class HospitalCacheService : IHospitalCacheService, IDependency
    {
        private readonly ICacheService _cache;
        
        // 缓存键名常量定义
        private const string UserLevelPrefix = "UserLevel_";
        private const string UserAccountPrefix = "UserAccount_";
        private const string DashboardLogsPrefix = "DashboardLogs_";
        private const string DbConfigKey = "DB_CONFIG_CACHE";

        public HospitalCacheService(ICacheService cache)
        {
            _cache = cache;
        }

        #region 用户职级缓存 (User Level) - 用于权限控制与业务分流

        public int GetUserLevel(int userId)
        {
            return _cache.Get($"{UserLevelPrefix}{userId}").GetInt();
        }

        public void SetUserLevel(int userId, int level)
        {
            // 缓存一个月 (3600 * 24 * 30 秒)
            _cache.Add($"{UserLevelPrefix}{userId}", level.ToString(), 3600 * 24 * 30);
        }

        public void RemoveUserLevel(int userId)
        {
            _cache.Remove($"{UserLevelPrefix}{userId}");
        }

        #endregion

        #region 用户账号缓存 (User Account)

        public T GetUserAccount<T>(int userId, int category) where T : class
        {
            return _cache.Get<T>($"{UserAccountPrefix}{userId}_{category}_Account");
        }

        public void SetUserAccount(int userId, int category, object account)
        {
            _cache.AddObject($"{UserAccountPrefix}{userId}_{category}_Account", account);
        }

        public void RemoveUserAccount(int userId, int category)
        {
            _cache.Remove($"{UserAccountPrefix}{userId}_{category}_Account");
        }

        #endregion

        #region 数据库配置缓存 (DB Config)

        public T GetDbConfigs<T>() where T : class
        {
            return _cache.Get<T>(DbConfigKey);
        }

        public void SetDbConfigs(object configs)
        {
            _cache.AddObject(DbConfigKey, configs);
        }

        public void RemoveDbConfigs()
        {
            _cache.Remove(DbConfigKey);
            _cache.Remove("DB_CONFIG_CACHE");
        }

        #endregion

        #region 仪表盘日志缓存 (Dashboard Logs)

        public T GetDashboardLogs<T>(long orgId) where T : class
        {
            return _cache.Get<T>($"{DashboardLogsPrefix}{orgId}");
        }

        public void SetDashboardLogs(long orgId, object logs)
        {
            _cache.AddObject($"{DashboardLogsPrefix}{orgId}", logs);
        }

        public void RemoveDashboardLogs(long orgId)
        {
            _cache.Remove($"{DashboardLogsPrefix}{orgId}");
        }

        /// <summary>
        /// 获取面板数据缓存（结果集）
        /// </summary>
        /// <param name="orgId">机构ID</param>
        /// <returns>面板数据对象</returns>
        public object GetDashboardData(long orgId)
        {
            return _cache.Get<object>($"Dashboard_Data_{orgId}");
        }

        /// <summary>
        /// 缓存面板数据（结果集）
        /// 用于提升 Dashboard 首页渲染速度，默认缓存 60 秒。
        /// </summary>
        /// <param name="orgId">机构ID</param>
        /// <param name="data">面板数据对象</param>
        public void SetDashboardData(long orgId, object data)
        {
            // 注意：序列化存储以确保跨进程兼容性（如果使用 Redis）
            _cache.Add($"Dashboard_Data_{orgId}", Newtonsoft.Json.JsonConvert.SerializeObject(data), 60);
        }

        /// <summary>
        /// 获取通用缓存实体
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="key">缓存键名</param>
        /// <returns>缓存对象</returns>
        public T Get<T>(string key) where T : class
        {
            return _cache.Get<T>(key);
        }

        /// <summary>
        /// 设置通用缓存对象
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="val">值</param>
        /// <param name="expire">过期时间（分钟）</param>
        public void Set(string key, object val, int? expire = null)
        {
            if (expire.HasValue)
            {
                _cache.AddObject(key, val, expire.Value);
            }
            else
            {
                _cache.AddObject(key, val);
            }
        }

        /// <summary>
        /// 移除指定键名的缓存
        /// </summary>
        /// <param name="key">键名</param>
        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        #endregion
    }
}

using System.Collections.Generic;

namespace SULL.HospitalManagement.IServices
{
    /// <summary>
    /// 医院管理系统统一缓存管理服务接口
    /// </summary>
    public interface IHospitalCacheService
    {
        #region 用户职级缓存 (User Level)
        
        /// <summary>
        /// 获取用户职级
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>职级</returns>
        int GetUserLevel(int userId);

        /// <summary>
        /// 缓存用户职级
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="level">职级</param>
        void SetUserLevel(int userId, int level);

        /// <summary>
        /// 移除用户职级缓存
        /// </summary>
        /// <param name="userId">用户ID</param>
        void RemoveUserLevel(int userId);

        #endregion

        #region 用户账号缓存 (User Account)

        /// <summary>
        /// 获取用户账号实体
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="userId">用户ID</param>
        /// <param name="category">分类</param>
        /// <returns>账号实体</returns>
        T GetUserAccount<T>(int userId, int category) where T : class;

        /// <summary>
        /// 缓存用户账号实体
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="category">分类</param>
        /// <param name="account">账号实体对象</param>
        void SetUserAccount(int userId, int category, object account);

        /// <summary>
        /// 移除用户账号缓存
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="category">分类</param>
        void RemoveUserAccount(int userId, int category);

        #endregion

        #region 数据库配置缓存 (DB Config)

        /// <summary>
        /// 获取数据库配置列表
        /// </summary>
        /// <typeparam name="T">列表类型</typeparam>
        /// <returns>配置列表</returns>
        T GetDbConfigs<T>() where T : class;

        /// <summary>
        /// 缓存数据库配置列表
        /// </summary>
        /// <param name="configs">配置列表对象</param>
        void SetDbConfigs(object configs);

        /// <summary>
        /// 移除数据库配置缓存
        /// </summary>
        void RemoveDbConfigs();

        #endregion

        #region 仪表盘日志缓存 (Dashboard Logs)

        /// <summary>
        /// 获取仪表盘日志列表
        /// </summary>
        /// <typeparam name="T">列表类型</typeparam>
        /// <param name="orgId">机构ID</param>
        /// <returns>日志列表</returns>
        T GetDashboardLogs<T>(long orgId) where T : class;

        /// <summary>
        /// 缓存仪表盘日志列表
        /// </summary>
        /// <param name="orgId">机构ID</param>
        /// <param name="logs">日志列表对象</param>
        void SetDashboardLogs(long orgId, object logs);

        /// <summary>
        /// 移除仪表盘日志缓存
        /// </summary>
        /// <param name="orgId">机构ID</param>
        void RemoveDashboardLogs(long orgId);

        /// <summary>
        /// 获取面板数据缓存
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        object GetDashboardData(long orgId);

        /// <summary>
        /// 设置面板数据缓存
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="data"></param>
        void SetDashboardData(long orgId, object data);

        /// <summary>
        /// 获取指定键名的缓存（通用逻辑）
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="key">键名</param>
        /// <returns>缓存对象</returns>
        T Get<T>(string key) where T : class;

        /// <summary>
        /// 缓存指定键名的数据（通用逻辑）
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="val">值</param>
        /// <param name="expire">过期时间（秒）</param>
        void Set(string key, object val, int? expire = null);

        /// <summary>
        /// 移除指定键名的缓存（通用逻辑）
        /// </summary>
        /// <param name="key">缓存键名</param>
        void Remove(string key);

        #endregion
    }
}

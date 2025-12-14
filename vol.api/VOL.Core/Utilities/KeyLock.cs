using System;
using System.Collections.Concurrent;

namespace VOL.Core.Utilities
{
    /// <summary>
    /// 细粒度锁工具类
    /// 用于对特定Key进行加锁，避免全局锁导致的性能瓶颈
    /// </summary>
    public static class KeyLock
    {
        private static readonly ConcurrentDictionary<string, object> _locks = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// 对指定的Key进行加锁并执行Action
        /// </summary>
        /// <param name="key">锁的Key（建议包含业务前缀以防冲突，如 "Handover_123"）</param>
        /// <param name="action">需要执行的逻辑</param>
        public static void Lock(string key, Action action)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var lockObj = _locks.GetOrAdd(key, _ => new object());
            lock (lockObj)
            {
                action();
            }
        }

        /// <summary>
        /// 对指定的Key进行加锁并执行Func，返回结果
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="key">锁的Key</param>
        /// <param name="func">需要执行的逻辑</param>
        /// <returns></returns>
        public static TResult Lock<TResult>(string key, Func<TResult> func)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var lockObj = _locks.GetOrAdd(key, _ => new object());
            lock (lockObj)
            {
                return func();
            }
        }
    }
}

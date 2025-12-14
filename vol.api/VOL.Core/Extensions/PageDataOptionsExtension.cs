using System;
using System.Collections.Generic;
using System.Linq;
using VOL.Entity.DomainModels;

namespace VOL.Core.Extensions
{
    public static class PageDataOptionsExtension
    {
        /// <summary>
        /// 从查询条件中获取指定参数的整型值。
        /// 如果参数不存在或无效，返回0。
        /// </summary>
        /// <param name="options">分页数据选项</param>
        /// <param name="parameterName">参数名称</param>
        /// <returns>参数值的长整型表示，如果无效则返回0</returns>
        public static long GetParameterLongValue(this PageDataOptions options, string parameterName)
        {
            if (options == null || string.IsNullOrEmpty(options.Wheres) || string.IsNullOrEmpty(parameterName))
            {
                return 0;
            }

            try
            {
                var wheres = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SearchParameters>>(options.Wheres);
                if (wheres != null)
                {
                    var param = wheres.FirstOrDefault(x => x.Name == parameterName);
                    if (param != null && long.TryParse(param.Value?.ToString(), out long result))
                    {
                        return result;
                    }
                }
            }
            catch
            {
                // 解析异常时忽略，返回0
            }

            return 0;
        }

        /// <summary>
        /// 从查询条件中获取指定参数的值。
        /// </summary>
        /// <param name="options">分页数据选项</param>
        /// <param name="parameterName">参数名称</param>
        /// <returns>参数值字符串，如果无效则返回null</returns>
        public static string GetParameterValue(this PageDataOptions options, string parameterName)
        {
            if (options == null || string.IsNullOrEmpty(options.Wheres) || string.IsNullOrEmpty(parameterName))
            {
                return null;
            }

            try
            {
                var wheres = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SearchParameters>>(options.Wheres);
                if (wheres != null)
                {
                    var param = wheres.FirstOrDefault(x => x.Name == parameterName);
                    return param?.Value?.ToString();
                }
            }
            catch
            {
                // 解析异常时忽略
            }

            return null;
        }

        /// <summary>
        /// 检测是否包含指定的有效参数值（字符串不为空且不是"undefined"或"null"）。
        /// 如果参数值无效，则返回默认的空 PageGridData。
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="options">分页数据选项</param>
        /// <param name="parameterName">参数名称</param>
        /// <returns>如果不包含有效参数，返回空的PageGridData；否则返回null</returns>
        public static PageGridData<T> CheckRequiredParameter<T>(this PageDataOptions options, string parameterName) where T : class
        {
            string value = options.GetParameterValue(parameterName);
            if (string.IsNullOrEmpty(value) 
                || string.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "null", StringComparison.OrdinalIgnoreCase))
            {
                return new PageGridData<T> { rows = new List<T>(), total = 0 };
            }
            return null;
        }

        /// <summary>
        /// 验证是否包含指定的有效参数值（字符串不为空且不是"undefined"或"null"）。
        /// </summary>
        /// <param name="options">分页数据选项</param>
        /// <param name="parameterName">参数名称</param>
        /// <returns>是否包含有效参数</returns>
        public static bool HasValidParameter(this PageDataOptions options, string parameterName)
        {
            var value = options.GetParameterValue(parameterName);
            return !string.IsNullOrEmpty(value) 
                && !string.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(value, "null", StringComparison.OrdinalIgnoreCase);
        }
    }
}

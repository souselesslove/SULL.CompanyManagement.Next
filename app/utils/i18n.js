/**
 * 跨平台消息格式化（适配安卓/小程序）
 * @param {String|Object} message - 翻译模板
 * @param {Object} values - 变量对象
 */
export function formatMessage(message, values) {
    // 1. 处理空值或无变量情况
    if (!message) return '';
    if (!values || typeof values !== 'object') return String(message);

    // 2. 强制转为纯字符串。小程序下 $t 返回的可能是 Proxy，直接 replace 可能会失败
    const template = String(message);

    // 3. 增强正则：
    // - {(\s*[\w.-]+\s*)} : 支持 {name}, { name }, {user.id} 等带空格或点的形式
    // - g : 全局匹配
    return template.replace(/{(\s*[\w.-]+\s*)}/g, (match, key) => {
        // 去掉变量两侧的空格
        const k = key.trim();

        // 4. 从 values 中取值
        // 使用 values[k] ?? match 以保留未匹配的占位符，方便调试
        // 如果值是 0 或 false，应该被正确替换，所以不用简单用 values[k] || match
        const val = values[k];

        if (val !== undefined && val !== null) {
            return String(val);
        }

        // 如果 values 是层级对象，如 { user: { name: 'Lee' } } 且模板是 {user.name}
        if (k.includes('.')) {
            const nestedVal = k.split('.').reduce((acc, part) => acc && acc[part], values);
            if (nestedVal !== undefined && nestedVal !== null) return String(nestedVal);
        }

        return match;
    });
}
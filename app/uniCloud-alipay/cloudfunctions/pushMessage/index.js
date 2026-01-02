'use strict';

const uniPush = uniCloud.getPushManager({
    appId: "__UNI__FFF233E"  // 替换成你的实际 AppID，如 __UNI__ABCDEF1
});

exports.main = async (event, context) => {
    console.log('【调试】原始 event keys:', Object.keys(event));

    // URL化调用时，实际参数在 event.body 中，是 JSON 字符串
    let params = event;
    if (event.body) {
        try {
            params = JSON.parse(event.body);
            console.log('【调试】解析后的 params:', params);
        } catch (err) {
            return { success: false, msg: 'JSON 解析失败: ' + err.message };
        }
    }

    let clientIdStr = '';

    if (Array.isArray(params.clientIds)) {
        if (params.clientIds.length === 0) {
            return { success: false, msg: 'clientIds 不能为空数组' };
        }
        clientIdStr = params.clientIds.join(',');
    } else if (typeof params.clientIds === 'string') {
        if (params.clientIds.trim() === '') {
            return { success: false, msg: 'clientIds 不能为空字符串' };
        }
        clientIdStr = params.clientIds.trim();
    } else {
        return { success: false, msg: 'clientIds 格式错误，必须是数组或字符串' };
    }

    try {
        const res = await uniPush.sendMessage({
            push_clientid: clientIdStr,
            title: params.title || '通知',
            content: params.content || '您有新消息',
            payload: params.payload || {},
            force_notification: true
        });

        return { success: true, data: res };
    } catch (err) {
        console.error('推送失败:', err);
        return { success: false, msg: err.message || '推送异常' };
    }
};
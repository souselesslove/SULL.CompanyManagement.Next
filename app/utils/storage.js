import md5 from 'js-md5';

const KEY_LAST_USER = 'sys_last_uid';
const KEY_PREFIX = 'u_';

class StorageManager {
    constructor() {
        this.userId = '';
        try {
            // Attempt to restore last active user context on valid session
            const lastUid = uni.getStorageSync(KEY_LAST_USER);
            if (lastUid) {
                this.userId = lastUid;
            }
        } catch (e) {
            console.error('Storage Init Error', e);
        }
    }

    /**
     * Initialize user context
     * @param {String|Number} userId 
     */
    init(userId) {
        if (!userId) return;
        this.userId = md5(userId.toString());
        uni.setStorageSync(KEY_LAST_USER, this.userId);
    }

    /**
     * Clear user context (Logout)
     */
    logout() {
        this.userId = '';
        uni.removeStorageSync(KEY_LAST_USER);
    }

    getCurrentUserId() {
        return this.userId;
    }

    _getKey(key) {
        if (!this.userId) return null;
        return `${KEY_PREFIX}${this.userId}_${key}`;
    }

    /**
     * Set User Scoped Data
     */
    set(key, value) {
        const k = this._getKey(key);
        if (k) {
            uni.setStorageSync(k, value);
        } else {
            console.warn(`Storage: Set '${key}' ignored - No User Context`);
        }
    }

    /**
     * Get User Scoped Data
     */
    get(key) {
        const k = this._getKey(key);
        if (k) {
            return uni.getStorageSync(k);
        }
        return null;
    }

    /**
     * Remove User Scoped Data
     */
    remove(key) {
        const k = this._getKey(key);
        if (k) {
            uni.removeStorageSync(k);
        }
    }

    /**
     * Get Token (Helper)
     */
    getToken() {
        // Wrapper to ensure we try to get token from current user context
        if (!this.userId) return null;
        return this.get('token');
    }

    /**
     * Handle Login Persistence
     * @param {String|Number} userId
     * @param {String} token
     * @param {Object} userInfo
     */
    login(token, userInfo) {
        let uid = userInfo.userName;

        if (uid === undefined || uid === null || uid === '') {
            console.error('Storage Login Error: Missing User ID', userInfo);
            return;
        }

        this.init(uid);
        this.set('token', token);
        if (userInfo) {
            this.set('userInfo', userInfo);
        }
    }

    // --- Global History (System Scope) ---

    getLoginHistory() {
        try {
            return uni.getStorageSync('sys_login_history') || [];
        } catch (e) { return []; }
    }

    addLoginHistory(username) {
        if (!username) return;
        let history = this.getLoginHistory();
        // Remove existing
        const idx = history.indexOf(username);
        if (idx > -1) history.splice(idx, 1);
        // Add to front
        history.unshift(username);
        // Limit
        if (history.length > 5) history.length = 5;
        uni.setStorageSync('sys_login_history', history);
        return history;
    }

    removeLoginHistory(index) {
        let history = this.getLoginHistory();
        if (index >= 0 && index < history.length) {
            history.splice(index, 1);
            uni.setStorageSync('sys_login_history', history);
        }
        return history;
    }

    // --- Features ---

    /**
     * Clear All Data for Current User
     */
    clearPersonalCache() {
        if (!this.userId) return;

        try {
            const res = uni.getStorageInfoSync();
            const prefix = `${KEY_PREFIX}${this.userId}_`;

            res.keys.forEach(k => {
                if (k.startsWith(prefix)) {
                    uni.removeStorageSync(k);
                }
            });

            // Also clear system last user reference to be clean?
            // No, keep context until logout?
            // User asked for "Personal Cache Clear". Usually means data.
            // But if token is cleared, they are logged out.
            // Let's assume clear cache = wipe everything including token.
            this.logout();

        } catch (e) {
            console.error('Clear Cache Error', e);
        }
    }

    /**
     * Export Current User Data (JSON)
     */
    exportData() {
        if (!this.userId) return null;

        const data = {};
        try {
            const res = uni.getStorageInfoSync();
            const prefix = `${KEY_PREFIX}${this.userId}_`;

            res.keys.forEach(k => {
                if (k.startsWith(prefix)) {
                    const shortKey = k.substring(prefix.length);
                    // Skip token in backup
                    if (shortKey === 'token') return;

                    data[shortKey] = uni.getStorageSync(k);
                }
            });
        } catch (e) {
            console.error('Export Error', e);
        }
        return data; // Object
    }

    /**
     * Import Data (Merge/Overwrite)
     * @param {Object} data 
     */
    importData(data) {
        if (!this.userId || !data) return;

        Object.keys(data).forEach(key => {
            this.set(key, data[key]);
        });
    }

    // --- Notification Module ---

    /**
     * Get all notifications for current user
     */
    getNotifications() {
        return this.get('notifications') || [];
    }

    /**
     * Add a new notification
     * @param {Object} rawData - Data from uni.onPushMessage
     */
    addNotification(rawData) {
        if (!rawData) return;
        const list = this.getNotifications();

        const newMsg = {
            uuid: rawData.__UUID__ || `msg_${Date.now()}`,
            category: rawData.payload?.category || '系统',
            time: rawData.payload?.creationTime || new Date().toISOString().replace('T', ' ').substring(0, 19),
            title: rawData.title || '通知',
            content: rawData.content || '',
            payload: rawData.payload || {},
            isRead: false
        };

        // Prevent duplicates
        if (list.some(item => item.uuid === newMsg.uuid)) return;

        list.unshift(newMsg);
        // Limit to last 100 messages
        if (list.length > 100) list.length = 100;

        this.set('notifications', list);
        return list;
    }

    /**
     * Mark a notification as read
     * @param {String} uuid 
     */
    markAsRead(uuid) {
        const list = this.getNotifications();
        const item = list.find(i => i.uuid === uuid);
        if (item) {
            item.isRead = true;
            this.set('notifications', list);
        }
    }

    /**
     * Mark a notification as unread
     * @param {String} uuid 
     */
    markAsUnread(uuid) {
        const list = this.getNotifications();
        const item = list.find(i => i.uuid === uuid);
        if (item) {
            item.isRead = false;
            this.set('notifications', list);
        }
    }

    /**
     * Mark all as read
     */
    markAllAsRead() {
        const list = this.getNotifications();
        list.forEach(item => item.isRead = true);
        this.set('notifications', list);
    }

    /**
     * Delete a notification
     */
    deleteNotification(uuid) {
        let list = this.getNotifications();
        list = list.filter(item => item.uuid !== uuid);
        this.set('notifications', list);
    }
}

export const storage = new StorageManager();
export default storage;

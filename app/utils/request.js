// Server Configuration
const SERVER_CONFIG = {
    // Replace with your actual server address
    BASE_URL: 'http://10.0.0.1:64624/',
    RESOURCE_URL: 'http://10.0.0.1:64624/',
    TIMEOUT: 30000
};

/**
 * Core Request Function
 * @param {Object} options - Request options
 * @returns {Promise}
 */
const request = (options) => {
    return new Promise((resolve, reject) => {
        // Construct full URL
        const url = options.url.startsWith('http') || options.url.startsWith('https')
            ? options.url
            : SERVER_CONFIG.BASE_URL + options.url;

        // Get token from storage
        const token = uni.getStorageSync('token');

        // Default headers
        const header = {
            'content-type': 'application/json',
            ...options.header
        };

        // Add Authorization header if token exists
        if (token) {
            header['Authorization'] = `Bearer ${token}`;
        }

        uni.request({
            url: url,
            method: options.method || 'GET',
            data: options.data || {},
            header: header,
            timeout: SERVER_CONFIG.TIMEOUT,
            success: (res) => {
                // Standardize response handling
                // Customize this based on your backend response structure
                if (res.statusCode >= 200 && res.statusCode < 300) {
                    // Check for token refresh
                    const volExp = res.header?.['vol_exp'];
                    if (volExp === '1') {
                        replaceToken();
                    }
                    resolve(res.data);
                } else {
                    // Handle specific HTTP errors
                    handleHttpError(res);
                    reject(res);
                }
            },
            fail: (err) => {
                uni.showToast({
                    title: 'Network Error',
                    icon: 'none'
                });
                reject(err);
            }
        });
    });
};

/**
 * Error Handler
 */
const handleHttpError = (res) => {
    let message = 'Request Failed';

    switch (res.statusCode) {
        case 401:
            message = 'Unauthorized (401)';
            uni.removeStorageSync('token');
            uni.reLaunch({ url: '/pages/profile/profile' });
            break;
        case 403:
            message = 'Forbidden (403)';
            break;
        case 404:
            message = 'Not Found (404)';
            break;
        case 500:
            message = 'Server Error (500)';
            break;
        default:
            message = `Error (${res.statusCode})`;
    }

    uni.showToast({
        title: message,
        icon: 'none'
    });
};

/**
 * Replace Token
 */
const replaceToken = () => {
    // We use a clean request without interception loop for token refresh to avoid recursion
    const token = uni.getStorageSync('token');
    if (!token) return;

    uni.request({
        url: SERVER_CONFIG.BASE_URL + 'api/User/replaceToken',
        method: 'POST',
        header: {
            'Authorization': `Bearer ${token}`
        },
        success: (res) => {
            if (res.statusCode >= 200 && res.statusCode < 300 && res.data && res.data.data) {
                const newToken = res.data.data;
                uni.setStorageSync('token', newToken);
                console.log('Token refreshed successfully');
            } else {
                // Refresh failed
                uni.removeStorageSync('token');
                uni.reLaunch({ url: '/pages/profile/profile' });
            }
        },
        fail: (err) => {
            console.error('Failed to replace token', err);
            uni.removeStorageSync('token');
            uni.reLaunch({ url: '/pages/profile/profile' });
        }
    });
};

export default {
    config: SERVER_CONFIG,

    /**
     * GET Request
     * @param {String} url - Endpoint URL
     * @param {Object} params - Query parameters
     * @param {Object} header - Custom headers
     */
    get(url, params = {}, header = {}) {
        return request({
            url,
            method: 'GET',
            data: params,
            header
        });
    },

    /**
     * POST Request
     * @param {String} url - Endpoint URL
     * @param {Object} data - Body data
     * @param {Object} header - Custom headers
     */
    post(url, data = {}, header = {}) {
        return request({
            url,
            method: 'POST',
            data,
            header
        });
    },

    /**
     * PUT Request
     */
    put(url, data = {}, header = {}) {
        return request({
            url,
            method: 'PUT',
            data,
            header
        });
    },

    /**
     * DELETE Request
     */
    delete(url, data = {}, header = {}) {
        return request({
            url,
            method: 'DELETE',
            data,
            header
        });
    }
};

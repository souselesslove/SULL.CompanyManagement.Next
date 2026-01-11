import { scan } from '@/uni_modules/sn-e-scankit';

/**
 * 统一扫码工具函数
 * @param {Function} successCallback 成功回调，接收结果对象 { data, format }
 */
export function globalScan(successCallback) {
    // #ifdef APP-PLUS
    // App 端优先使用华为统一扫码服务
    scan({}, (res) => {
        if (res && res.data) {
            successCallback({
                data: res.data,
                format: res.format
            });
        }
    });
    return;
    // #endif

    // #ifdef H5
    // H5 浏览器环境下，原生 uni.scanCode 经常无法识别图片。
    // 我们采用动态加载 ZXing 解码库的方案来实现纯前端图片解码。

    const loadZXing = () => {
        return new Promise((resolve, reject) => {
            if (window.ZXing) {
                resolve(window.ZXing);
                return;
            }
            const script = document.createElement('script');
            script.src = 'https://unpkg.com/@zxing/library@latest';
            script.onload = () => resolve(window.ZXing);
            script.onerror = () => reject(new Error('Failed to load ZXing library'));
            document.head.appendChild(script);
        });
    };

    uni.chooseImage({
        count: 1,
        sizeType: ['original', 'compressed'],
        sourceType: ['album'],
        success: async (chooseRes) => {
            const filePath = chooseRes.tempFilePaths[0];
            uni.showLoading({ title: '正在识别...' });

            try {
                await loadZXing();
                const reader = new window.ZXing.BrowserMultiFormatReader();

                // ZXing 需要一个 Image 元素
                const img = new Image();
                img.src = filePath;
                img.onload = async () => {
                    try {
                        const result = await reader.decodeFromImageElement(img);
                        uni.hideLoading();
                        if (result) {
                            successCallback({
                                data: result.text,
                                format: result.format ? result.format.toString() : 'UNKNOWN'
                            });
                        }
                    } catch (decodeErr) {
                        uni.hideLoading();
                        console.error('ZXing Decode Error:', decodeErr);
                        uni.showToast({
                            title: '识别失败，请确保图片清晰且包含有效的条码/二维码',
                            icon: 'none',
                            duration: 3000
                        });
                    }
                };
                img.onerror = () => {
                    uni.hideLoading();
                    uni.showToast({ title: '图片加载失败', icon: 'none' });
                };

            } catch (err) {
                uni.hideLoading();
                console.error('H5 Scan Error:', err);
                uni.showToast({ title: '解码库加载失败', icon: 'none' });
            }
        }
    });
    // #endif

    // #ifndef APP-PLUS || H5
    // 其他小程序环境（如微信等）原生 scanCode 是健全的
    uni.scanCode({
        success: (res) => {
            successCallback({
                data: res.result,
                format: res.scanType
            });
        }
    });
    // #endif
}

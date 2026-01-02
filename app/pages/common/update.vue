<template>
    <view class="update-container">
        <view class="glass-modal">
            <view class="modal-header">
                <view class="rocket-icon">
                    <view class="rocket-body">🚀</view>
                    <view class="rocket-glow"></view>
                </view>
                <text class="update-title">{{ $t('update.title') }}</text>
                <text class="update-version">v{{ versionData.Version || versionData.version || '1.0.0' }}</text>
            </view>

            <view class="modal-body">
                <scroll-view scroll-y class="desc-scroll">
                    <view class="description">
                        <text class="desc-text">{{ versionData.Description || versionData.updateDescription ||
                            $t('update.default_desc') }}</text>
                    </view>
                </scroll-view>

                <view class="progress-section" v-if="downloading">
                    <view class="progress-bar-bg">
                        <view class="progress-bar-fill" :style="{ width: progress + '%' }"></view>
                    </view>
                    <text class="progress-text">{{ downloadingText }}</text>
                </view>
            </view>

            <view class="modal-footer">
                <button v-if="!downloading" class="update-btn" @click="startUpdate">{{ $t('update.button_now')
                    }}</button>
                <button v-else class="update-btn disabled" disabled>{{ $t('update.button_updating') }}</button>
            </view>
        </view>

        <view class="background-decor">
            <view class="circle circle-1"></view>
            <view class="circle circle-2"></view>
        </view>
    </view>
</template>

<script>
import { storage } from '@/utils/storage.js';
import { formatMessage } from '@/utils/i18n.js';

export default {
    data() {
        return {
            versionData: {},
            downloading: false,
            progress: 0,
            downloadTask: null,
            localPath: '',
            fileExists: false
        };
    },
    computed: {
        downloadingText() {
            // Use formatMessage to ensure interpolation works on all platforms
            const template = this.$t('update.downloading');
            return formatMessage(template, { progress: this.progress });
        }
    },
    async onLoad(options) {
        if (options.data) {
            try {
                this.versionData = JSON.parse(decodeURIComponent(options.data));

                // Use a consistent relative path. Uni-app's FileProvider handles this best on Android.
                this.localPath = `_doc/update_${this.versionData.Version}.apk`;

                this.fileExists = await this.checkFileExists(this.localPath);
            } catch (e) {
                console.error('Parse version data failed', e);
            }
        }
    },
    async onShow() {
        if (this.localPath) {
            this.fileExists = await this.checkFileExists(this.localPath);
            // If we are in "updating" state and file now exists, we can try install again if user clicks
            // Or we could even auto-trigger it, but clicking again is safer.
        }
    },
    onBackPress() {
        return true;
    },
    methods: {
        checkFileExists(path) {
            return new Promise((resolve) => {
                // #ifdef APP-PLUS
                // Resolve relative path to absolute system path first
                let absolutePath = plus.io.convertLocalFileSystemURL(path);
                if (uni.getSystemInfoSync().platform === 'android' && !absolutePath.startsWith('file://')) {
                    absolutePath = 'file://' + absolutePath;
                }

                plus.io.resolveLocalFileSystemURL(absolutePath, (entry) => {
                    entry.getMetadata((metadata) => {
                        console.log('File detected at:', absolutePath, 'Size:', metadata.size);
                        resolve(metadata.size > 0);
                    }, () => resolve(false));
                }, () => resolve(false));
                // #endif
                // #ifndef APP-PLUS
                resolve(false);
                // #endif
            });
        },
        async startUpdate() {
            // High priority: re-verify file existence to avoid redundant downloads
            this.fileExists = await this.checkFileExists(this.localPath);

            if (this.fileExists) {
                // If exists, show "Verifying" to give user feedback
                this.downloading = true;
                this.progress = 0; // Or keep at 0/100 to show bar

                // Verification: check MD5 if provided
                if (this.versionData.MD5) {
                    const localMd5 = await this.getFileMD5(this.localPath);
                    console.log('Verifying existing file MD5:', localMd5, 'Expected:', this.versionData.MD5);

                    if (localMd5 && localMd5 === this.versionData.MD5.toLowerCase()) {
                        this.checkAndInstall(this.localPath);
                        return;
                    } else {
                        console.warn('Existing file MD5 mismatch, will redownload.');
                        await this.deleteFile(this.localPath);
                        this.fileExists = false;
                    }
                } else {
                    this.checkAndInstall(this.localPath);
                    return;
                }
            }

            const updateUrl = this.versionData.Url || this.versionData.updateUrl;
            if (!updateUrl) {
                this.downloading = false;
                uni.showToast({ title: this.$t('update.invalid_url'), icon: 'none' });
                return;
            }

            this.downloading = true;
            this.progress = 0;

            // Step 1: Clean up
            await this.deleteFile(this.localPath);

            // Step 2: Download
            const dtask = plus.downloader.createDownload(updateUrl, {
                filename: this.localPath,
                timeout: 30,
                retry: 3
            }, async (d, status) => {
                if (status == 200) {
                    console.log('Download finished. Verifying file:', d.filename);
                    const isOk = await this.checkFileExists(d.filename);
                    if (isOk) {
                        // MD5 Check for download
                        if (this.versionData.MD5) {
                            const localMd5 = await this.getFileMD5(d.filename);
                            if (localMd5 !== this.versionData.MD5.toLowerCase()) {
                                console.error('Download MD5 mismatch!', localMd5);
                                this.downloading = false;
                                uni.showToast({ title: 'MD5 Verification Failed', icon: 'none' });
                                this.deleteFile(d.filename);
                                return;
                            }
                        }
                        console.log('File verification successful, proceeding to install.');
                        this.fileExists = true;
                        this.checkAndInstall(d.filename);
                    } else {
                        console.error('File verification failed: File may be empty or missing.');
                        this.downloading = false;
                        uni.showToast({ title: 'Download verification failed', icon: 'none' });
                        this.deleteFile(d.filename);
                    }
                } else {
                    this.downloading = false;
                    uni.showToast({ title: this.$t('update.download_failed') + ': ' + status, icon: 'none' });
                }
            });

            dtask.addEventListener("statechanged", (download, status) => {
                if (download.state == 3) {
                    if (download.totalSize > 0) {
                        this.progress = Math.floor(download.downloadedSize / download.totalSize * 100);
                    }
                }
            }, false);
            dtask.start();
        },
        async checkAndInstall(path) {
            // #ifdef APP-PLUS
            if (uni.getSystemInfoSync().platform === 'android') {
                const main = plus.android.runtimeMainActivity();
                const packageManager = main.getPackageManager();
                const Build = plus.android.importClass("android.os.Build");

                if (Build.VERSION.SDK_INT >= 26) {
                    const canInstall = plus.android.invoke(packageManager, 'canRequestPackageInstalls');
                    if (!canInstall) {
                        uni.showModal({
                            title: this.$t('update.permission_title'),
                            content: this.$t('update.permission_msg'),
                            confirmText: this.$t('update.go_to_setting'),
                            success: (res) => {
                                if (res.confirm) {
                                    const Intent = plus.android.importClass('android.content.Intent');
                                    const Settings = plus.android.importClass('android.provider.Settings');
                                    const Uri = plus.android.importClass('android.net.Uri');
                                    const intent = new Intent(Settings.ACTION_MANAGE_UNKNOWN_APP_SOURCES);
                                    intent.setData(Uri.parse('package:' + main.getPackageName()));
                                    main.startActivity(intent);
                                }
                            }
                        });
                        this.downloading = false;
                        return;
                    }
                }
            }

            // Crucial: Use the relative path (_doc/...) directly. 
            // plus.runtime.install will use its internal FileProvider to map it safely for the system installer.
            setTimeout(() => {
                this.executeInstall(path);
            }, 500);
            // #endif
        },
        executeInstall(path) {
            console.log('Executing install with path:', path);

            // Crucial: DO NOT delete the file here. The system installer needs it.
            // Also DO NOT call plus.runtime.restart() as it may kill the installer intent.
            plus.runtime.install(path, {
                force: true
            }, () => {
                console.log('Installation intent launched successfully');
                this.downloading = false;
            }, (e) => {
                // Secondary fallback: try absolute URI if relative path fails
                const absPath = plus.io.convertLocalFileSystemURL(path);
                const uriPath = absPath.startsWith('file://') ? absPath : 'file://' + absPath;
                console.log('Relative path install failed, trying absolute URI:', uriPath);

                plus.runtime.install(uriPath, { force: true }, () => {
                    console.log('Installation intent launched via absolute URI');
                    this.downloading = false;
                }, (err) => {
                    this.downloading = false;
                    uni.showToast({ title: this.$t('update.install_failed') + ': ' + err.message, icon: 'none' });
                    // Only delete if MD5 or parsing is definitely broken (this allows redownload)
                    if (err.code !== -10) {
                        this.deleteFile(path);
                        this.fileExists = false;
                    }
                });
            });
        },
        deleteFile(path) {
            return new Promise((resolve) => {
                plus.io.resolveLocalFileSystemURL(path, (entry) => {
                    entry.remove(() => {
                        console.log('Update file deleted');
                        resolve(true);
                    }, (e) => {
                        console.error('Delete file failed', e);
                        resolve(false);
                    });
                }, () => resolve(true));
            });
        },
        getFileMD5(path) {
            return new Promise((resolve) => {
                console.log('Starting MD5 calculation with uni.getFileInfo:', path);

                // Use uni.getFileInfo which is implemented in native code and much faster
                uni.getFileInfo({
                    filePath: path,
                    digestAlgorithm: 'md5',
                    success: (res) => {
                        console.log('MD5 Hash success:', res.digest);
                        resolve(res.digest ? res.digest.toLowerCase() : null);
                    },
                    fail: (err) => {
                        console.error('uni.getFileInfo failed:', err);

                        // Fallback: If it's a relative path error on some platforms, try adding file://
                        if (path.startsWith('_')) {
                            const absPath = plus.io.convertLocalFileSystemURL(path);
                            const uri = absPath.startsWith('file://') ? absPath : 'file://' + absPath;
                            console.log('Trying fallback MD5 with URI:', uri);
                            uni.getFileInfo({
                                filePath: uri,
                                digestAlgorithm: 'md5',
                                success: (r) => resolve(r.digest ? r.digest.toLowerCase() : null),
                                fail: () => resolve(null)
                            });
                        } else {
                            resolve(null);
                        }
                    }
                });
            });
        }
    }
};
</script>

<style lang="scss" scoped>
.update-container {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    display: flex;
    align-items: center;
    justify-content: center;
    background-color: rgba(0, 0, 0, 0.6);
    z-index: 99999;
    overflow: hidden;
}

.glass-modal {
    width: 600rpx;
    background: rgba(255, 255, 255, 0.85);
    backdrop-filter: blur(20px);
    border-radius: 48rpx;
    border: 1px solid rgba(255, 255, 255, 0.3);
    box-shadow: 0 40rpx 100rpx rgba(0, 0, 0, 0.2);
    display: flex;
    flex-direction: column;
    overflow: hidden;
    position: relative;
    z-index: 10;
    animation: modalShow 0.5s cubic-bezier(0.18, 0.89, 0.32, 1.28);
}

@keyframes modalShow {
    from {
        opacity: 0;
        transform: scale(0.8) translateY(40rpx);
    }

    to {
        opacity: 1;
        transform: scale(1) translateY(0);
    }
}

.modal-header {
    padding: 60rpx 40rpx 40rpx;
    display: flex;
    flex-direction: column;
    align-items: center;
    background: linear-gradient(to bottom, rgba($sull-color-primary, 0.1), transparent);
}

.rocket-icon {
    width: 120rpx;
    height: 120rpx;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 80rpx;
    margin-bottom: 24rpx;
    position: relative;
}

.rocket-body {
    animation: rocketFloat 3s ease-in-out infinite;
    z-index: 2;
}

@keyframes rocketFloat {

    0%,
    100% {
        transform: translateY(0);
    }

    50% {
        transform: translateY(-10rpx);
    }
}

.rocket-glow {
    position: absolute;
    width: 80rpx;
    height: 80rpx;
    background: rgba($sull-color-primary, 0.4);
    filter: blur(20rpx);
    border-radius: 50%;
    z-index: 1;
}

.update-title {
    font-size: 40rpx;
    font-weight: 800;
    color: $sull-text-main;
    margin-bottom: 8rpx;
}

.update-version {
    font-size: 28rpx;
    color: $sull-color-primary;
    font-weight: 600;
    background: rgba($sull-color-primary, 0.1);
    padding: 4rpx 20rpx;
    border-radius: 20rpx;
}

.modal-body {
    padding: 30rpx 50rpx 50rpx;
}

.desc-scroll {
    max-height: 300rpx;
    margin-bottom: 40rpx;
}

.desc-text {
    font-size: 28rpx;
    color: $sull-text-secondary;
    line-height: 1.6;
    white-space: pre-wrap;
}

.progress-section {
    display: flex;
    flex-direction: column;
    align-items: center;
}

.progress-bar-bg {
    width: 100%;
    height: 16rpx;
    background: #eee;
    border-radius: 8rpx;
    overflow: hidden;
    margin-bottom: 16rpx;
}

.progress-bar-fill {
    height: 100%;
    background: linear-gradient(to right, $sull-color-primary, lighten($sull-color-primary, 20%));
    border-radius: 8rpx;
    transition: width 0.3s ease;
}

.progress-text {
    font-size: 24rpx;
    color: $sull-color-primary;
    font-weight: 500;
}

.modal-footer {
    padding: 0 40rpx 60rpx;
}

.update-btn {
    width: 100%;
    height: 100rpx;
    background: linear-gradient(135deg, $sull-color-primary 0%, darken($sull-color-primary, 10%) 100%);
    color: #fff;
    border-radius: 50rpx;
    font-size: 32rpx;
    font-weight: bold;
    display: flex;
    align-items: center;
    justify-content: center;
    box-shadow: 0 10rpx 30rpx rgba($sull-color-primary, 0.4);
    border: none;

    &:active {
        transform: scale(0.98);
        opacity: 0.9;
    }

    &.disabled {
        background: #ccc;
        box-shadow: none;
        opacity: 0.8;
    }
}

.background-decor {
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    overflow: hidden;
    z-index: 1;
}

.circle {
    position: absolute;
    border-radius: 50%;
    filter: blur(60rpx);
}

.circle-1 {
    width: 400rpx;
    height: 400rpx;
    background: rgba($sull-color-primary, 0.2);
    top: -100rpx;
    right: -100rpx;
}

.circle-2 {
    width: 300rpx;
    height: 300rpx;
    background: rgba($sull-color-secondary, 0.15);
    bottom: -50rpx;
    left: -50rpx;
}
</style>

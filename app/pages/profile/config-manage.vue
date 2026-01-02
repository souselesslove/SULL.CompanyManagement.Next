<template>
    <view class="container">
        <!-- Custom Navbar -->
        <view class="header" :style="{ paddingTop: statusBarHeight + 'px' }">
            <view class="header-content">
                <BackButton class="header-back-btn" @click="goBack" />
                <text class="header-title">{{ $t('profile.config_manage') }}</text>
            </view>
        </view>

        <view class="content">
            <!-- System Config Section -->
            <view class="section-title">{{ $t('configManage.system_config') }}</view>
            <view class="menu-list-card">
                <view class="menu-item" @click="handleManageNotification">
                    <view class="menu-item-left">
                        <text class="menu-label">🔔 {{ $t('configManage.notification_status') }}</text>
                    </view>
                    <view class="menu-item-right">
                        <text class="status-text" :class="{ 'enabled': isNotificationEnabled }">
                            {{ isNotificationEnabled ? $t('configManage.notification_enabled') :
                                $t('configManage.notification_disabled') }}
                        </text>
                        <text class="menu-arrow">›</text>
                    </view>
                </view>
                <view class="menu-item" @click="handleCheckUpdate">
                    <view class="menu-item-left">
                        <text class="menu-label">🆙 {{ $t('configManage.check_update') }}</text>
                    </view>
                    <view class="menu-item-right">
                        <text class="menu-arrow">›</text>
                    </view>
                </view>
            </view>

            <!-- Data Management Section -->
            <view class="section-title" style="margin-top: 40rpx;">{{ $t('configManage.data_manage') }}</view>
            <view class="menu-list-card">
                <view class="menu-item" @click="handleBackupCache">
                    <view class="menu-item-left">
                        <text class="menu-label">📤 {{ $t('profile.upload_config') }}</text>
                    </view>
                    <text class="menu-arrow">›</text>
                </view>
                <view class="menu-item" @click="handleRestoreCache">
                    <view class="menu-item-left">
                        <text class="menu-label">📥 {{ $t('profile.download_config') }}</text>
                    </view>
                    <text class="menu-arrow">›</text>
                </view>
                <view class="menu-item" @click="handleClearCache">
                    <view class="menu-item-left">
                        <text class="menu-label" style="color: #ff6b6b;">🗑️ {{ $t('profile.clear_cache') }}</text>
                    </view>
                    <text class="menu-arrow">›</text>
                </view>
            </view>
        </view>
    </view>
</template>

<script>
import { storage } from '@/utils/storage.js';
import BackButton from '@/components/BackButton/BackButton.vue';

export default {
    components: { BackButton },
    data() {
        return {
            statusBarHeight: 20,
            isNotificationEnabled: false
        }
    },
    onLoad() {
        const sysInfo = uni.getSystemInfoSync();
        this.statusBarHeight = sysInfo.statusBarHeight || 20;
    },
    onShow() {
        this.checkNotificationStatus();
    },
    methods: {
        goBack() {
            uni.navigateBack();
        },
        checkNotificationStatus() {
            // #ifdef APP-PLUS
            this.getNotificationStatus((isEnabled) => {
                this.isNotificationEnabled = isEnabled;
            });
            // #endif
            // #ifndef APP-PLUS
            this.isNotificationEnabled = true; // Mock for H5/Sim
            // #endif
        },
        getNotificationStatus(callback) {
            if (uni.getSystemInfoSync().platform == "ios") {
                let center = plus.ios.invoke('UNUserNotificationCenter', 'currentNotificationCenter');
                plus.ios.invoke(center, 'getNotificationSettingsWithCompletionHandler:', plus.ios.implements("UNNotificationSettings", {
                    "getNotificationSettingsWithCompletionHandler:": function (settings) {
                        let authorizationStatus = plus.ios.invoke(settings, 'authorizationStatus');
                        callback(authorizationStatus === 3);
                    }
                }));
            } else {
                let main = plus.android.runtimeMainActivity();
                let pkName = main.getPackageName();
                let NotificationManagerCompat = plus.android.importClass("androidx.core.app.NotificationManagerCompat");
                let notificationManager = NotificationManagerCompat.from(main);
                callback(notificationManager.areNotificationsEnabled());
            }
        },
        handleManageNotification() {
            // #ifdef APP-PLUS
            this.openAppSettings();
            // #endif
            // #ifndef APP-PLUS
            uni.showToast({ title: this.$t('profile.app_only'), icon: 'none' });
            // #endif
        },
        openAppSettings() {
            if (uni.getSystemInfoSync().platform == "ios") {
                let UIApplication = plus.ios.import("UIApplication");
                let NSURL = plus.ios.import("NSURL");
                let setting = NSURL.URLWithString("app-settings:");
                let application = UIApplication.sharedApplication();
                application.openURL(setting);
                plus.ios.deleteObject(setting);
                plus.ios.deleteObject(application);
            } else {
                let main = plus.android.runtimeMainActivity();
                let pkName = main.getPackageName();
                let Intent = plus.android.importClass('android.content.Intent');
                let Settings = plus.android.importClass('android.provider.Settings');
                let Uri = plus.android.importClass('android.net.Uri');
                let intent = new Intent();
                let Build = plus.android.importClass("android.os.Build");

                if (Build.VERSION.SDK_INT >= 26) {
                    intent.setAction(Settings.ACTION_APP_NOTIFICATION_SETTINGS);
                    intent.putExtra(Settings.EXTRA_APP_PACKAGE, pkName);
                } else {
                    intent.setAction(Settings.ACTION_APPLICATION_DETAILS_SETTINGS);
                    let uri = Uri.fromParts("package", pkName, null);
                    intent.setData(uri);
                }
                main.startActivity(intent);
            }
        },
        handleClearCache() {
            uni.showModal({
                title: this.$t('common.tip'),
                content: this.$t('profile.clear_cache_confirm'),
                confirmColor: '#ff6b6b',
                success: (res) => {
                    if (res.confirm) {
                        storage.clearPersonalCache();
                        uni.showToast({ title: this.$t('profile.cache_cleared'), icon: 'none' });
                        setTimeout(() => {
                            uni.reLaunch({ url: '/pages/profile/profile' });
                        }, 1000);
                    }
                }
            });
        },
        handleBackupCache() {
            const data = storage.exportData();
            const str = JSON.stringify(data);
            uni.setClipboardData({
                data: str,
                success: () => {
                    uni.showToast({ title: this.$t('profile.config_uploaded'), icon: 'none' });
                }
            });
        },
        handleRestoreCache() {
            uni.getClipboardData({
                success: (res) => {
                    if (res.data) {
                        try {
                            const data = JSON.parse(res.data);
                            storage.importData(data);
                            uni.showToast({ title: this.$t('profile.config_restored'), icon: 'success' });
                        } catch (e) {
                            uni.showToast({ title: this.$t('profile.invalid_config'), icon: 'none' });
                        }
                    }
                }
            });
        },
        handleCheckUpdate() {
            // #ifdef APP-PLUS
            getApp().checkUpdate({
                manual: true,
                title: this.$t('index.refreshing'),
                latestMessage: this.$t('configManage.already_latest')
            });
            // #endif
            // #ifndef APP-PLUS
            uni.showToast({ title: this.$t('profile.app_only'), icon: 'none' });
            // #endif
        }
    }
}
</script>

<style lang="scss" scoped>
.container {
    background-color: #f8f9fb;
    min-height: 100vh;
    display: flex;
    flex-direction: column;
}

.header {
    background-color: $sull-color-primary;
    padding-bottom: 20rpx;
    padding-left: 20rpx;
    padding-right: 20rpx;
}

.header-content {
    height: 88rpx;
    display: flex;
    align-items: center;
    position: relative;
    justify-content: center;
}

.header-back-btn {
    position: absolute;
    left: 0;
}

.header-title {
    color: #fff;
    font-size: 34rpx;
    font-weight: 500;
}

.content {
    flex: 1;
    padding: 30rpx;
}

.section-title {
    font-size: 26rpx;
    color: #999;
    margin-left: 20rpx;
    margin-bottom: 16rpx;
    text-transform: uppercase;
    font-weight: 500;
}

.menu-list-card {
    background-color: #fff;
    border-radius: 24rpx;
    padding: 0 24rpx;
    margin-bottom: 20rpx;
    box-shadow: 0 4rpx 16rpx rgba(0, 0, 0, 0.02);
}

.menu-item {
    display: flex;
    align-items: center;
    justify-content: space-between;
    height: 110rpx;
    border-bottom: 1rpx solid #f0f0f0;

    &:active {
        background-color: #f9f9f9;
        border-radius: 12rpx;
    }
}

.menu-item:last-child {
    border-bottom: none;
}

.menu-item-left {
    display: flex;
    align-items: center;
}

.menu-item-right {
    display: flex;
    align-items: center;
}

.menu-label {
    font-size: 30rpx;
    color: #333;
}

.status-text {
    font-size: 28rpx;
    color: #999;

    &.enabled {
        color: $sull-color-success;
    }
}

.menu-arrow {
    font-size: 32rpx;
    color: #ccc;
    margin-left: 10rpx;
}
</style>

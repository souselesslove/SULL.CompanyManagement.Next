<template>
    <view class="container">
        <!-- Custom Navbar -->
        <view class="header" :style="{ paddingTop: statusBarHeight + 'px' }">
            <view class="header-content">
                <BackButton class="header-back-btn" @click="goBack" />
                <text class="header-title">{{ $t('profile.notification') }}</text>
                <text v-if="hasUnread" class="header-action" @click="handleMarkAllRead">{{
                    $t('profile.mark_all_read') }}</text>
            </view>
        </view>

        <!-- Unified Capsule Tabs -->
        <CapsuleTabs :tabs="tabList" v-model:current="currentTab"
            container-style="padding: 16rpx 32rpx; border-bottom: 2rpx solid #f0f0f0;" />

        <scroll-view scroll-y class="content" refresher-enabled="true" :refresher-triggered="isRefreshing"
            @refresherrefresh="onRefresh">

            <view v-if="filteredNotifications.length === 0" class="empty-state">
                <image class="empty-img" src="/static/imgs/empty.png" mode="aspectFit"></image>
                <text class="empty-text">{{ $t('common.no_data') }}</text>
            </view>

            <view v-else class="notification-list">
                <view class="notification-item" v-for="(item, index) in filteredNotifications" :key="item.uuid"
                    :class="{ 'unread': !item.isRead }" @click="handleItemClick(item)">

                    <view class="item-header">
                        <view class="item-tag" :class="getCategoryClass(item.category)">
                            {{ getCategoryLabel(item.category) }}
                        </view>
                        <text class="item-time">{{ item.time }}</text>

                        <!-- Toggle Status Button -->
                        <view v-if="!item.isRead" class="item-action-btn read" @click.stop="handleReadItem(item)">
                            <text class="btn-text">{{ $t('index.todo.read') }}</text>
                        </view>
                        <view v-else class="item-action-btn unread" @click.stop="handleUnreadItem(item)">
                            <text class="btn-text">{{ $t('index.todo.unread') }}</text>
                        </view>
                    </view>

                    <view class="item-body">
                        <view class="title-row">
                            <text class="item-title">{{ item.title }}</text>
                            <view v-if="!item.isRead" class="item-dot"></view>
                        </view>
                        <text class="item-content">{{ item.content }}</text>
                    </view>
                </view>
            </view>

            <view class="bottom-spacer"></view>
        </scroll-view>
    </view>
</template>

<script>
import { storage } from '@/utils/storage.js';
import BackButton from '@/components/BackButton/BackButton.vue';
import CapsuleTabs from '@/components/CapsuleTabs/CapsuleTabs.vue';

export default {
    components: { BackButton, CapsuleTabs },
    data() {
        return {
            statusBarHeight: 20,
            notifications: [],
            isRefreshing: false,
            currentTab: 'all' // all, unread, read
        }
    },
    computed: {
        tabList() {
            return [
                { key: 'all', label: this.$t('index.todo.all') },
                {
                    key: 'unread',
                    label: this.unreadCount > 0
                        ? `${this.$t('index.todo.unread')} (${this.unreadCount})`
                        : this.$t('index.todo.unread')
                },
                { key: 'read', label: this.$t('index.todo.read') }
            ];
        },
        filteredNotifications() {
            if (this.currentTab === 'all') return this.notifications;
            if (this.currentTab === 'unread') return this.notifications.filter(i => !i.isRead);
            if (this.currentTab === 'read') return this.notifications.filter(i => i.isRead);
            return this.notifications;
        },
        unreadCount() {
            return this.notifications.filter(i => !i.isRead).length;
        },
        hasUnread() {
            return this.unreadCount > 0;
        }
    },
    onLoad() {
        const sysInfo = uni.getSystemInfoSync();
        this.statusBarHeight = sysInfo.statusBarHeight || 20;
        this.fetchNotifications();

        // Listen for new messages
        uni.$on('pushMessageReceived', () => {
            this.fetchNotifications();
        });
    },
    onUnload() {
        uni.$off('pushMessageReceived');
        // Refresh parent unread count
        const eventChannel = this.getOpenerEventChannel();
        if (eventChannel && eventChannel.emit) {
            eventChannel.emit('refreshUnread');
        }
    },
    methods: {
        goBack() {
            uni.navigateBack();
        },
        fetchNotifications() {
            this.notifications = storage.getNotifications();
        },
        onRefresh() {
            this.isRefreshing = true;
            this.fetchNotifications();
            setTimeout(() => {
                this.isRefreshing = false;
            }, 500);
        },
        handleItemClick(item) {
            if (!item.isRead) {
                storage.markAsRead(item.uuid);
                this.fetchNotifications();
            }
        },
        handleReadItem(item) {
            storage.markAsRead(item.uuid);
            this.fetchNotifications();
        },
        handleUnreadItem(item) {
            storage.markAsUnread(item.uuid);
            this.fetchNotifications();
        },
        handleMarkAllRead() {
            storage.markAllAsRead();
            this.fetchNotifications();
            uni.showToast({ title: this.$t('common.success'), icon: 'none' });
        },
        getCategoryLabel(category) {
            switch (category) {
                case '系统': return this.$t('notification.category.system');
                case '待办': return this.$t('notification.category.todo');
                case '警告': return this.$t('notification.category.warning');
                case '异常': return this.$t('notification.category.abnormal');
                default: return category;
            }
        },
        getCategoryClass(category) {
            switch (category) {
                case '系统': return 'tag-system';
                case '待办': return 'tag-todo';
                case '警告': return 'tag-warning';
                case '异常': return 'tag-abnormal';
                default: return 'tag-default';
            }
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

.header-action {
    position: absolute;
    right: 10rpx;
    color: #fff;
    font-size: 28rpx;
    opacity: 0.9;
}

.content {
    flex: 1;
    padding: 20rpx 0;
}

.empty-state {
    padding-top: 200rpx;
    padding-left: 40rpx;
    padding-right: 40rpx;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
}

.empty-img {
    width: 240rpx;
    height: 240rpx;
    margin-bottom: 30rpx;
    opacity: 0.6;
}

.empty-text {
    font-size: 28rpx;
    color: #999;
}

.notification-list {
    display: flex;
    flex-direction: column;
    gap: 20rpx;
    padding: 0 24rpx;
}

.notification-item {
    background-color: #fff;
    border-radius: 20rpx;
    padding: 24rpx;
    position: relative;
    box-shadow: 0 4rpx 12rpx rgba(0, 0, 0, 0.03);
    border: 1rpx solid transparent;
    transition: all 0.2s;

    &:active {
        opacity: 0.8;
        transform: scale(0.98);
    }

    &.unread {
        background: linear-gradient(to right, #ffffff, #f0f7ff);
        border-color: rgba($sull-color-primary, 0.1);
    }
}

.item-header {
    display: flex;
    align-items: center;
    margin-bottom: 16rpx;
}

.item-tag {
    font-size: 20rpx;
    padding: 4rpx 12rpx;
    border-radius: 6rpx;
    margin-right: 16rpx;
    font-weight: bold;
}

.tag-system {
    background-color: #E6F7FF;
    color: #1890FF;
}

.tag-todo {
    background-color: #F6FFED;
    color: #52C41A;
}

.tag-warning {
    background-color: #FFF7E6;
    color: #FAAD14;
}

.tag-abnormal {
    background-color: #FFF1F0;
    color: #FF4D4F;
}

.tag-default {
    background-color: #F5F5F5;
    color: #999;
}

.item-time {
    font-size: 24rpx;
    color: #999;
    flex: 1;
}

.item-action-btn {
    padding: 0 20rpx;
    height: 48rpx;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: 24rpx;
    transition: all 0.2s;

    .btn-text {
        font-size: 22rpx;
        font-weight: 500;
    }

    &.read {
        background-color: rgba($sull-color-primary, 0.08);

        .btn-text {
            color: $sull-color-primary;
        }
    }

    &.unread {
        background-color: #f5f5f5;

        .btn-text {
            color: #999;
        }
    }

    &:active {
        opacity: 0.7;
        transform: scale(0.95);
    }
}

.item-body {
    display: flex;
    flex-direction: column;
}

.title-row {
    display: flex;
    align-items: center;
    margin-bottom: 8rpx;
}

.item-title {
    font-size: 30rpx;
    color: #333;
    font-weight: bold;
    flex: 1;
}

.item-content {
    font-size: 26rpx;
    color: #666;
    line-height: 1.5;
}

.item-dot {
    width: 12rpx;
    height: 12rpx;
    background-color: #FF4D4F;
    border-radius: 50%;
    margin-left: 10rpx;
    box-shadow: 0 0 8rpx rgba(255, 77, 79, 0.5);
}

.bottom-spacer {
    height: 60rpx;
}
</style>

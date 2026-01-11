<template>
    <view class="custom-refresher"
        :style="{ transform: (isPulling || isRefreshing) ? 'translateY(120rpx)' : 'translateY(0)' }">
        <view class="refresher-content">
            <view class="refresher-icon" :class="{ 'animating': isRefreshing }"></view>
            <text class="refresher-text">{{ isRefreshing ? $t('index.refreshing') : $t('index.pullToRefresh') }}</text>
        </view>
    </view>
</template>

<script>
/**
 * CustomRefresher - 救护车动画下拉刷新控件
 * @description 用于 scroll-view 的 refresher 插槽，提供品牌特色的救护车弹跳动画
 * @property {Boolean} isRefreshing 是否正在刷新中
 * @property {Boolean} isPulling 是否正在下拉中
 */
export default {
    name: 'CustomRefresher',
    props: {
        isRefreshing: {
            type: Boolean,
            default: false
        },
        isPulling: {
            type: Boolean,
            default: false
        }
    }
}
</script>

<style lang="scss" scoped>
.custom-refresher {
    height: 120rpx;
    width: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    position: absolute;
    top: -120rpx;
    left: 0;
    transition: transform 0.2s linear;
    z-index: 10;
}

.refresher-content {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 8rpx;
}

.refresher-icon {
    width: 64rpx;
    height: 64rpx;
    background-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='%23009A97'%3E%3Cpath d='M19 14h-1V8h-3V6h-2v2h-2V6H9v2H7v6H6c-1.1 0-2 .9-2 2v3c0 1.1.9 2 2 2h1c0 1.66 1.34 3 3 3s3-1.34 3-3h4c0 1.66 1.34 3 3 3s3-1.34 3-3h1c1.1 0 2-.9 2-2v-3c0-1.1-.9-2-2-2zM8 19c-.55 0-1-.45-1-1s.45-1 1-1 1 .45 1 1-.45 1-1 1zm10 0c-.55 0-1-.45-1-1s.45-1 1-1 1 .45 1 1-.45 1-1 1zM8 8h2v2H8V8zm0 4h2v2H8v-2zm4 4h-2v-2h2v2zm0-4h-2V8h2v4zm4 4h-2v-2h2v2zm0-4h-2V8h2v4z'/%3E%3Cpath d='M4 8h2v2H4zm0 4h2v2H4z' fill-opacity='.3'/%3E%3C/svg%3E");
    background-size: contain;
    background-repeat: no-repeat;
    transition: transform 0.3s ease;
}

.refresher-icon.animating {
    animation: ambulance-bounce 0.8s infinite ease-in-out;
}

.refresher-text {
    font-size: 20rpx;
    color: #009A97;
    opacity: 0.8;
}

@keyframes ambulance-bounce {

    0%,
    100% {
        transform: translateY(0);
    }

    50% {
        transform: translateY(-12rpx);
    }
}
</style>

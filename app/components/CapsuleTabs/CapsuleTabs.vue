<template>
    <view class="capsule-tabs-container" :style="containerStyle">
        <view class="capsule-tabs">
            <view class="capsule-tab-item" v-for="(tab, index) in tabs" :key="index"
                :class="{ active: current === (tab.key !== undefined ? tab.key : tab.value) }" @click="onTabClick(tab)">
                <text>{{ getLabel(tab) }}</text>
            </view>
        </view>
    </view>
</template>

<script>
export default {
    name: 'CapsuleTabs',
    props: {
        tabs: {
            type: Array,
            default: () => []
        },
        current: {
            type: [String, Number],
            default: ''
        },
        containerStyle: {
            type: String,
            default: ''
        }
    },
    methods: {
        getLabel(tab) {
            const label = tab.label || tab.name || '';
            // If it looks like an i18n key (contains dots), try to translate it
            if (typeof label === 'string' && label.includes('.')) {
                return this.$t(label);
            }
            return label;
        },
        onTabClick(tab) {
            const key = tab.key !== undefined ? tab.key : tab.value;
            if (this.current === key) return;
            this.$emit('change', key);
            this.$emit('update:current', key);
        }
    }
}
</script>

<style lang="scss" scoped>
.capsule-tabs-container {
    width: 100%;
    padding: 16rpx 32rpx;
    box-sizing: border-box;
    background: #fff;
}

.capsule-tabs {
    display: flex;
    background: #F5F7FA;
    border-radius: 50rpx;
    padding: 6rpx;
    box-shadow: 0 2rpx 10rpx rgba(0, 0, 0, 0.02);
}

.capsule-tab-item {
    flex: 1;
    height: 72rpx;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 26rpx;
    color: #666;
    font-weight: 500;
    border-radius: 36rpx;
    transition: all 0.3s;
    white-space: nowrap;

    &.active {
        background: $sull-color-primary;
        color: #fff;
        font-weight: 600;
        box-shadow: 0 4rpx 12rpx rgba(0, 154, 151, 0.3);
    }
}
</style>

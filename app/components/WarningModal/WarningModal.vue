<template>
    <view class="modal-mask" v-if="visible" @click.stop="handleMaskClick">
        <view class="modal-content" @click.stop>
            <!-- Close Button -->
            <view class="close-icon" v-if="showClose" @click="handleClose">×</view>

            <!-- Illustration Area -->
            <view class="illustration-container">
                <view class="thumb-circle">
                    <text class="thumb-icon">{{ icon || '💡' }}</text>
                    <!-- Decorative dots -->
                    <view class="dot dot-1"></view>
                    <view class="dot dot-2"></view>
                    <view class="dot dot-3"></view>
                </view>
            </view>

            <!-- Text Content -->
            <view class="modal-title">{{ title || $t('modal.warning_title') }}</view>
            <view class="modal-message">{{ message }}</view>

            <!-- Action Buttons -->
            <view class="modal-btns" v-if="showCancel">
                <view class="modal-btn btn-cancel" @click="handleCancel">
                    <text class="btn-text btn-text-cancel">{{ cancelText || $t('common.cancel') || '取消' }}</text>
                </view>
                <view class="modal-btn btn-confirm" @click="handleConfirm">
                    <text class="btn-text">{{ buttonText || $t('modal.confirm_btn') }}</text>
                </view>
            </view>
            <view class="modal-btn" v-else @click="handleConfirm">
                <text class="btn-text">{{ buttonText || $t('modal.confirm_btn') }}</text>
            </view>
        </view>
    </view>
</template>

<script>
export default {
    name: 'WarningModal',
    props: {
        visible: {
            type: Boolean,
            default: false
        },
        title: {
            type: String,
            default: ''
        },
        message: {
            type: String,
            default: ''
        },
        buttonText: {
            type: String,
            default: ''
        },
        icon: {
            type: String,
            default: '💡'
        },
        maskClosable: {
            type: Boolean,
            default: false
        },
        showCancel: {
            type: Boolean,
            default: false
        },
        cancelText: {
            type: String,
            default: ''
        },
        showClose: {
            type: Boolean,
            default: false
        }
    },
    emits: ['update:visible', 'confirm', 'cancel', 'close'],
    methods: {
        handleConfirm() {
            this.$emit('confirm');
            this.close();
        },
        handleCancel() {
            this.$emit('cancel');
            this.close();
        },
        handleClose() {
            this.$emit('close');
            this.close();
        },
        handleMaskClick() {
            if (this.maskClosable) {
                this.close();
            }
        },
        close() {
            this.$emit('update:visible', false);
        }
    }
}
</script>

<style lang="scss" scoped>
.modal-mask {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: rgba(0, 0, 0, 0.6);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 9999;
}

.modal-content {
    width: 600rpx;
    background-color: #ffffff;
    border-radius: 48rpx;
    padding: 64rpx 48rpx;
    display: flex;
    flex-direction: column;
    align-items: center;
    animation: popIn 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
    box-sizing: border-box;
    position: relative;
}

.close-icon {
    position: absolute;
    top: 32rpx;
    right: 32rpx;
    font-size: 48rpx;
    color: #999;
    line-height: 1;
    padding: 10rpx;
    z-index: 10;
}

.illustration-container {
    margin-bottom: 48rpx;
    position: relative;
}

.thumb-circle {
    width: 200rpx;
    height: 200rpx;
    background-color: #FFF9E6;
    /* Light Yellow background for warning/tip */
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    position: relative;
}

.thumb-icon {
    font-size: 96rpx;
}

.dot {
    position: absolute;
    border-radius: 50%;
}

.dot-1 {
    width: 24rpx;
    height: 24rpx;
    background-color: #FFE58F;
    top: 20rpx;
    left: 0;
}

.dot-2 {
    width: 20rpx;
    height: 20rpx;
    background-color: #7EC0EE;
    top: 40rpx;
    right: 0;
}

.dot-3 {
    width: 16rpx;
    height: 16rpx;
    background-color: #FFBB96;
    bottom: 20rpx;
    right: 20rpx;
}

.modal-title {
    font-size: 40rpx;
    font-weight: bold;
    color: #333333;
    margin-bottom: 24rpx;
    text-align: center;
}

.modal-message {
    font-size: 28rpx;
    color: #666666;
    margin-bottom: 64rpx;
    text-align: center;
    line-height: 1.5;
}

.modal-btn {
    width: 100%;
    height: 100rpx;
    background-color: $sull-color-primary;
    border-radius: 50rpx;
    display: flex;
    align-items: center;
    justify-content: center;
    box-shadow: 0 8rpx 20rpx rgba(0, 154, 151, 0.2);
}

.btn-text {
    color: #ffffff;
    font-size: 32rpx;
    font-weight: 600;
}

.modal-btns {
    width: 100%;
    display: flex;
    gap: 24rpx;
}

.modal-btn.btn-cancel {
    background-color: #f5f5f5;
    box-shadow: none;
    flex: 1;
}

.modal-btn.btn-confirm {
    flex: 1;
}

.btn-text-cancel {
    color: #666;
}

@keyframes popIn {
    from {
        opacity: 0;
        transform: scale(0.9);
    }

    to {
        opacity: 1;
        transform: scale(1);
    }
}
</style>

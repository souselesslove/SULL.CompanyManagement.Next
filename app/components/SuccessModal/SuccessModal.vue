<template>
    <view class="modal-mask" v-if="visible" @click.stop="handleMaskClick">
        <view class="modal-content" @click.stop>
            <!-- Illustration Area -->
            <view class="illustration-container">
                <view class="thumb-circle">
                    <text class="thumb-icon">👍</text>
                    <!-- Decorative dots -->
                    <view class="dot dot-1"></view>
                    <view class="dot dot-2"></view>
                    <view class="dot dot-3"></view>
                </view>
            </view>

            <!-- Text Content -->
            <view class="modal-title" v-if="title !== ''">{{ title || $t('modal.success_title') }}</view>
            <view class="modal-message" v-if="message !== ''">{{ message === null ? $t('modal.success_message') :
                message }}</view>

            <!-- Action Button -->
            <view class="modal-btn" @click="handleConfirm">
                <text class="btn-text">{{ buttonText || $t('modal.continue') }}</text>
            </view>
        </view>
    </view>
</template>

<script>
export default {
    name: 'SuccessModal',
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
            default: null
        },
        buttonText: {
            type: String,
            default: ''
        },
        maskClosable: {
            type: Boolean,
            default: false
        }
    },
    emits: ['update:visible', 'confirm'],
    methods: {
        handleConfirm() {
            this.$emit('confirm');
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
    background-color: rgba(0, 0, 0, 0.5);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 9999;
    pointer-events: auto;
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
}

.illustration-container {
    margin-bottom: 48rpx;
    position: relative;
}

.thumb-circle {
    width: 200rpx;
    height: 200rpx;
    background-color: #FFF0EB;
    /* Light reddish/pinkish background */
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    position: relative;
}

.thumb-icon {
    font-size: 96rpx;
    // Use a transform to match the hand angle roughly if needed, 
    // but standard emoji is fine for MVP.
}

/* Decorative dots matching the style */
.dot {
    position: absolute;
    border-radius: 50%;
}

.dot-1 {
    width: 24rpx;
    height: 24rpx;
    background-color: #F8B4B4;
    /* reddish */
    top: 20rpx;
    left: 0;
}

.dot-2 {
    width: 20rpx;
    height: 20rpx;
    background-color: #7EC0EE;
    /* blue */
    top: 40rpx;
    right: 0;
}

.dot-3 {
    width: 16rpx;
    height: 16rpx;
    background-color: #EBC0A5;
    /* brownish */
    bottom: 20rpx;
    right: 20rpx;
}

.modal-title {
    font-size: 48rpx;
    font-weight: bold;
    color: #000000;
    margin-bottom: 24rpx;
    text-align: center;
    width: 100%;
    word-break: break-all;
}

.modal-message {
    font-size: 28rpx;
    color: #666666;
    margin-bottom: 64rpx;
    text-align: center;
    line-height: 1.4;
    width: 100%;
    word-break: break-all;
    white-space: pre-wrap;
}

.modal-btn {
    width: 100%;
    height: 100rpx;
    background-color: #92C5F7;
    /* Light Blue similar to screenshot */
    border-radius: 50rpx;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: opacity 0.2s;
}

.modal-btn:active {
    opacity: 0.9;
}

.btn-text {
    color: #ffffff;
    font-size: 32rpx;
    font-weight: 600;
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

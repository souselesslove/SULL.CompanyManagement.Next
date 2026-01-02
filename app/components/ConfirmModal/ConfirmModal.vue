<template>
    <view class="modal-mask" v-if="visible" @click.stop="handleMaskClick">
        <view class="modal-content" @click.stop>
            <!-- Illustration Area -->
            <view class="illustration-container">
                <view class="icon-circle">
                    <view class="chat-bubble">
                        <text class="question-mark">?</text>
                    </view>
                </view>
            </view>

            <!-- Text Content -->
            <text class="modal-title">{{ displayTitle }}</text>
            <text class="modal-message">{{ displayMessage }}</text>

            <!-- Action Buttons -->
            <view class="modal-actions">
                <view class="btn btn-cancel" :style="{ color: cancelColor, borderColor: cancelColor }"
                    @click="handleCancel">
                    <text class="btn-text">{{ displayCancelText }}</text>
                </view>
                <view class="btn btn-confirm" :style="{ backgroundColor: confirmColor }" @click="handleConfirm">
                    <text class="btn-text-white">{{ displayConfirmText }}</text>
                </view>
            </view>
        </view>
    </view>
</template>

<script>
export default {
    name: 'ConfirmModal',
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
        confirmText: {
            type: String,
            default: ''
        },
        cancelText: {
            type: String,
            default: ''
        },
        confirmColor: {
            type: String,
            default: '#92C5F7' // Default blue from screenshot
        },
        cancelColor: {
            type: String,
            default: '#FF6B6B' // Default red from screenshot
        },
        maskClosable: {
            type: Boolean,
            default: false
        }
    },
    emits: ['update:visible', 'confirm', 'cancel'],
    computed: {
        displayTitle() {
            return this.title || this.$t('modal.confirm_title');
        },
        displayMessage() {
            return this.message || this.$t('modal.confirm_message');
        },
        displayConfirmText() {
            return this.confirmText || this.$t('modal.confirm_btn');
        },
        displayCancelText() {
            return this.cancelText || this.$t('modal.cancel_btn');
        }
    },
    methods: {
        handleConfirm() {
            this.$emit('confirm');
            this.close();
        },
        handleCancel() {
            this.$emit('cancel');
            this.close();
        },
        handleMaskClick() {
            if (this.maskClosable) {
                this.handleCancel();
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
    z-index: 1000;
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
}

.illustration-container {
    margin-bottom: 48rpx;
}

.icon-circle {
    width: 200rpx;
    height: 200rpx;
    background-color: #FFF4E6;
    /* Light orange background */
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
}

.chat-bubble {
    width: 120rpx;
    height: 100rpx;
    background-color: #FFC107;
    /* Orange/Yellow */
    border-radius: 24rpx;
    position: relative;
    display: flex;
    align-items: center;
    justify-content: center;
}

.chat-bubble::after {
    content: '';
    position: absolute;
    bottom: -20rpx;
    left: 50%;
    transform: translateX(-50%);
    border-width: 20rpx 20rpx 0;
    border-style: solid;
    border-color: #FFC107 transparent transparent transparent;
}

.question-mark {
    color: #ffffff;
    font-size: 64rpx;
    font-weight: bold;
    line-height: 1;
}

.modal-title {
    font-size: 40rpx;
    font-weight: bold;
    color: #000000;
    margin-bottom: 24rpx;
    text-align: center;
}

.modal-message {
    font-size: 28rpx;
    color: #999999;
    margin-bottom: 64rpx;
    text-align: center;
    line-height: 1.4;
    max-width: 480rpx;
}

.modal-actions {
    display: flex;
    width: 100%;
    gap: 32rpx;
}

.btn {
    flex: 1;
    height: 88rpx;
    border-radius: 44rpx;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: opacity 0.2s;
}

.btn:active {
    opacity: 0.8;
}

.btn-cancel {
    background-color: transparent;
    border: 2rpx solid currentColor;
}

.btn-confirm {
    border: 2rpx solid transparent;
    /* Maintain size parity */
}

.btn-text {
    font-size: 32rpx;
    font-weight: 600;
    /* Color inherited from parent style binding */
}

.btn-text-white {
    font-size: 32rpx;
    font-weight: 600;
    color: #ffffff;
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

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
            <text class="modal-title">{{ title || 'Are You Sure?' }}</text>
            <text class="modal-message">{{ message || 'Do you want to proceed?' }}</text>

            <!-- Action Buttons -->
            <view class="modal-actions">
                <view class="btn btn-cancel" :style="{ color: cancelColor, borderColor: cancelColor }"
                    @click="handleCancel">
                    <text class="btn-text">{{ cancelText || 'Cancel' }}</text>
                </view>
                <view class="btn btn-confirm" :style="{ backgroundColor: confirmColor }" @click="handleConfirm">
                    <text class="btn-text-white">{{ confirmText || 'Confirm' }}</text>
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
            default: 'Confirm'
        },
        cancelText: {
            type: String,
            default: 'Cancel'
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
    width: 300px;
    background-color: #ffffff;
    border-radius: 24px;
    padding: 32px 24px;
    display: flex;
    flex-direction: column;
    align-items: center;
    animation: popIn 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

.illustration-container {
    margin-bottom: 24px;
}

.icon-circle {
    width: 100px;
    height: 100px;
    background-color: #FFF4E6;
    /* Light orange background */
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
}

.chat-bubble {
    width: 60px;
    height: 50px;
    background-color: #FFC107;
    /* Orange/Yellow */
    border-radius: 12px;
    position: relative;
    display: flex;
    align-items: center;
    justify-content: center;
}

.chat-bubble::after {
    content: '';
    position: absolute;
    bottom: -10px;
    left: 50%;
    transform: translateX(-50%);
    border-width: 10px 10px 0;
    border-style: solid;
    border-color: #FFC107 transparent transparent transparent;
}

.question-mark {
    color: #ffffff;
    font-size: 32px;
    font-weight: bold;
    line-height: 1;
}

.modal-title {
    font-size: 20px;
    font-weight: bold;
    color: #000000;
    margin-bottom: 12px;
    text-align: center;
}

.modal-message {
    font-size: 14px;
    color: #999999;
    margin-bottom: 32px;
    text-align: center;
    line-height: 1.4;
    max-width: 240px;
}

.modal-actions {
    display: flex;
    width: 100%;
    gap: 16px;
}

.btn {
    flex: 1;
    height: 44px;
    border-radius: 22px;
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
    border: 1px solid currentColor;
}

.btn-confirm {
    border: 1px solid transparent;
    /* Maintain size parity */
}

.btn-text {
    font-size: 16px;
    font-weight: 600;
    /* Color inherited from parent style binding */
}

.btn-text-white {
    font-size: 16px;
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

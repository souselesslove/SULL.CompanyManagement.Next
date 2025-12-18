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
            <text class="modal-title">{{ title || 'Success' }}</text>
            <text class="modal-message">{{ message || 'Operation completed successfully.' }}</text>

            <!-- Action Button -->
            <view class="modal-btn" @click="handleConfirm">
                <text class="btn-text">{{ buttonText || 'Continue' }}</text>
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
            default: 'Success'
        },
        message: {
            type: String,
            default: ''
        },
        buttonText: {
            type: String,
            default: 'Continue'
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
    z-index: 999;
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
    position: relative;
}

.thumb-circle {
    width: 100px;
    height: 100px;
    background-color: #FFF0EB;
    /* Light reddish/pinkish background */
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    position: relative;
}

.thumb-icon {
    font-size: 48px;
    // Use a transform to match the hand angle roughly if needed, 
    // but standard emoji is fine for MVP.
}

/* Decorative dots matching the style */
.dot {
    position: absolute;
    border-radius: 50%;
}

.dot-1 {
    width: 12px;
    height: 12px;
    background-color: #F8B4B4;
    /* reddish */
    top: 10px;
    left: 0px;
}

.dot-2 {
    width: 10px;
    height: 10px;
    background-color: #7EC0EE;
    /* blue */
    top: 20px;
    right: 0px;
}

.dot-3 {
    width: 8px;
    height: 8px;
    background-color: #EBC0A5;
    /* brownish */
    bottom: 10px;
    right: 10px;
}

.modal-title {
    font-size: 24px;
    font-weight: bold;
    color: #000000;
    margin-bottom: 12px;
    text-align: center;
}

.modal-message {
    font-size: 14px;
    color: #666666;
    margin-bottom: 32px;
    text-align: center;
    line-height: 1.4;
    max-width: 240px;
}

.modal-btn {
    width: 100%;
    height: 50px;
    background-color: #92C5F7;
    /* Light Blue similar to screenshot */
    border-radius: 25px;
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
    font-size: 16px;
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

<template>
    <view class="keypad-container">
        <view class="keypad-grid">
            <view class="key-row">
                <view class="key-cell" hover-class="key-hover" @click="onInput('1')">1</view>
                <view class="key-cell" hover-class="key-hover" @click="onInput('2')">2</view>
                <view class="key-cell" hover-class="key-hover" @click="onInput('3')">3</view>
            </view>
            <view class="key-row">
                <view class="key-cell" hover-class="key-hover" @click="onInput('4')">4</view>
                <view class="key-cell" hover-class="key-hover" @click="onInput('5')">5</view>
                <view class="key-cell" hover-class="key-hover" @click="onInput('6')">6</view>
            </view>
            <view class="key-row">
                <view class="key-cell" hover-class="key-hover" @click="onInput('7')">7</view>
                <view class="key-cell" hover-class="key-hover" @click="onInput('8')">8</view>
                <view class="key-cell" hover-class="key-hover" @click="onInput('9')">9</view>
            </view>
            <view class="key-row">
                <view class="key-cell" hover-class="key-hover" @click="onInput('.')">.</view>
                <view class="key-cell" hover-class="key-hover" @click="onInput('0')">0</view>
                <view class="key-cell action-key" hover-class="key-hover" @click="onDelete">
                    <text class="delete-icon">⌫</text>
                </view>
            </view>
        </view>
    </view>
</template>

<script>
export default {
    name: 'NumericKeypad',
    props: {
        maxLength: {
            type: Number,
            default: 10
        }
    },
    methods: {
        onInput(val) {
            this.$emit('input', val);
        },
        onDelete() {
            this.$emit('delete');
        }
    }
}
</script>

<style lang="scss" scoped>
.keypad-container {
    background-color: #F2F3F5;
    padding: 12px;
    border-radius: 12px;
}

.keypad-grid {
    display: flex;
    flex-direction: column;
    gap: 8px;
}

.key-row {
    display: flex;
    gap: 8px;
}

.key-cell {
    flex: 1;
    background: #FFFFFF;
    height: 48px;
    border-radius: 8px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 20px;
    font-weight: 600;
    color: #1F1F1F;
    box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);
    transition: all 0.1s;
}

.key-hover {
    background-color: #EBEDF0;
    transform: scale(0.98);
}

.action-key {
    background-color: #EBEDF0;
}

.delete-icon {
    font-size: 18px;
}
</style>

<template>
    <view class="container">
        <!-- Header Gradient Section -->
        <view class="header-section">
            <BackButton class="header-back-btn" @click="goBack" />
            <view class="header-content">
                <text class="page-title">添加抢救车</text>
                <text class="page-subtitle">请输入抢救车的基本信息以开始管理</text>
            </view>
        </view>

        <!-- Form Section -->
        <view class="form-section">
            <view class="form-group">
                <text class="label">抢救车名称</text>
                <view class="input-wrapper">
                    <input class="custom-input" v-model="form.Name" placeholder="请输入名称 (如: 1号车)"
                        placeholder-class="placeholder-style" />
                </view>
            </view>

            <view class="form-group">
                <text class="label">层数</text>
                <!-- Quick Selection Tags -->
                <view class="floor-tags">
                    <view class="floor-tag" v-for="item in [2, 3, 4, 5]" :key="item"
                        :class="{ 'active': form.Floor == item }" @click="selectFloor(item)">
                        {{ item }}层
                    </view>
                </view>
                <view class="input-wrapper">
                    <input class="custom-input" type="number" v-model="form.Floor" placeholder="请输入层数 (如: 5)"
                        placeholder-class="placeholder-style" />
                </view>
            </view>

            <view class="form-group">
                <text class="label">备注</text>
                <view class="input-wrapper">
                    <textarea class="custom-textarea" v-model="form.Remark" placeholder="请输入备注信息..."
                        placeholder-class="placeholder-style" />
                </view>
            </view>

            <view class="action-area">
                <button class="submit-btn" :class="{ 'disabled': isLoading }" @click="submit">
                    {{ isLoading ? '提交中...' : '确认添加' }}
                </button>
            </view>
        </view>
    </view>
</template>

<script>
import request from '@/utils/request.js';
import { storage } from '@/utils/storage.js';
import BackButton from '@/components/BackButton/BackButton.vue';

export default {
    components: {
        BackButton
    },
    data() {
        return {
            form: {
                Name: '',
                Floor: '',
                Remark: ''
            },
            userInfo: {},
            userOrgInfo: {},
            isLoading: false
        }
    },
    onLoad() {
        this.userInfo = storage.get('userInfo') || {};
        this.userOrgInfo = storage.get('userOrgInfo') || {};
    },
    methods: {
        goBack() {
            uni.navigateBack();
        },
        selectFloor(floor) {
            this.form.Floor = floor;
        },
        submit() {
            if (this.isLoading) return;
            if (!this.form.Name) {
                uni.showToast({ title: '请输入名称', icon: 'none' });
                return;
            }
            if (!this.form.Floor) {
                uni.showToast({ title: '请输入层数', icon: 'none' });
                return;
            }

            this.isLoading = true;

            const payload = {
                DisplayName: this.form.Name,
                Category: 10,
                OrganizationId: this.userOrgInfo.Organization.Id,
                DepartmentId: this.userOrgInfo.Department.Id,
                FloorId: this.form.Floor,
                Remark: this.form.Remark
            };

            request.post('api/ssbh/Position/Add', { mainData: payload })
                .then(res => {
                    this.isLoading = false;
                    if (res.success || res.status) {
                        uni.showToast({ title: '添加成功', icon: 'success' });
                        setTimeout(() => {
                            this.goBack();
                        }, 1500);
                    } else {
                        uni.showToast({ title: res.msg || '添加失败', icon: 'none' });
                    }
                })
                .catch(e => {
                    this.isLoading = false;
                    console.error(e);
                    uni.showToast({ title: '添加失败', icon: 'none' });
                });
        }
    }
}
</script>

<style lang="scss" scoped>
.container {
    min-height: 100vh;
    background-color: #fff;
    display: flex;
    flex-direction: column;
}

.header-section {
    height: 400rpx;
    background: linear-gradient(135deg, $sull-color-primary 0%, lighten($sull-color-primary, 20%) 100%);
    /* Soft Blue Gradient matching reference */
    padding: 40rpx 48rpx;
    display: flex;
    flex-direction: column;
    position: relative;
    /* Ensure padding for status bar if needed, though usually handled by page structure */
    padding-top: 80rpx;
}

.header-back-btn {
    margin-bottom: 20rpx;
}

.header-content {
    margin-top: 20rpx;
    text-align: center;
}

.page-title {
    font-size: 56rpx;
    font-weight: bold;
    color: #fff;
    display: block;
    margin-bottom: 16rpx;
    text-shadow: 0 4rpx 8rpx rgba(0, 0, 0, 0.1);
}

.page-subtitle {
    font-size: 32rpx;
    color: rgba(255, 255, 255, 0.9);
    display: block;
}

.form-section {
    flex: 1;
    background-color: #fff;
    border-top-left-radius: 60rpx;
    border-top-right-radius: 60rpx;
    margin-top: -100rpx;
    /* Overlap header */
    padding: 80rpx 60rpx;
    z-index: 10;
}

.form-group {
    margin-bottom: 48rpx;
}

.label {
    font-size: 32rpx;
    font-weight: bold;
    color: #555;
    margin-bottom: 24rpx;
    display: block;
}

.input-wrapper {
    position: relative;
}

.custom-input {
    width: 100%;
    height: 112rpx;
    background-color: #F7F8FA;
    border-radius: 56rpx;
    padding: 0 48rpx;
    font-size: 32rpx;
    color: #333;
    box-sizing: border-box;
}

.custom-textarea {
    width: 100%;
    height: 240rpx;
    background-color: #F7F8FA;
    border-radius: 56rpx;
    padding: 32rpx 48rpx;
    font-size: 32rpx;
    color: #333;
    box-sizing: border-box;
}

.placeholder-style {
    color: #B0B0B0;
}

.action-area {
    margin-top: 120rpx;
}

.submit-btn {
    width: 100%;
    height: 112rpx;
    background: $sull-color-primary;
    /* Primary theme color */
    border-radius: 56rpx;
    color: #fff;
    font-size: 36rpx;
    font-weight: bold;
    display: flex;
    align-items: center;
    justify-content: center;
    box-shadow: 0 12rpx 40rpx rgba(149, 202, 255, 0.4);
    border: none;

    &:active {
        transform: scale(0.98);
        opacity: 0.9;
    }

    &.disabled {
        background: #ccc;
        box-shadow: none;
    }
}

.floor-tags {
    display: flex;
    justify-content: space-between;
    width: 100%;
    margin-bottom: 24rpx;
}

.floor-tag {
    flex: 1;
    text-align: center;
    padding: 16rpx 0;
    margin: 0 8rpx;
    background: #F7F8FA;
    border-radius: 40rpx;
    font-size: 28rpx;
    color: #555;
    cursor: pointer;
    transition: all 0.2s;
    border: 2rpx solid transparent;

    &:first-child {
        margin-left: 0;
    }

    &:last-child {
        margin-right: 0;
    }

    &.active {
        background: rgba(149, 202, 255, 0.2);
        color: $sull-color-primary;
        border-color: $sull-color-primary;
        font-weight: 600;
    }

    &:active {
        transform: scale(0.95);
    }
}
</style>

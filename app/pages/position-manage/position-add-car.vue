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
        this.userInfo = uni.getStorageSync('userInfo') || {};
        this.userOrgInfo = uni.getStorageSync('userOrgInfo') || {};
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
@import '@/sull.scss';

.container {
    min-height: 100vh;
    background-color: #fff;
    display: flex;
    flex-direction: column;
}

.header-section {
    height: 200px;
    background: linear-gradient(135deg, #A1C4FD 0%, #C2E9FB 100%);
    /* Soft Blue Gradient matching reference */
    padding: 20px 24px;
    display: flex;
    flex-direction: column;
    position: relative;
    /* Ensure padding for status bar if needed, though usually handled by page structure */
    padding-top: 40px;
}

.header-back-btn {
    margin-bottom: 10px;
}

.header-content {
    margin-top: 10px;
    text-align: center;
}

.page-title {
    font-size: 28px;
    font-weight: bold;
    color: #fff;
    display: block;
    margin-bottom: 8px;
    text-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.page-subtitle {
    font-size: 16px;
    color: rgba(255, 255, 255, 0.9);
    display: block;
}

.form-section {
    flex: 1;
    background-color: #fff;
    border-top-left-radius: 30px;
    border-top-right-radius: 30px;
    margin-top: -50px;
    /* Overlap header */
    padding: 40px 30px;
    z-index: 10;
}

.form-group {
    margin-bottom: 24px;
}

.label {
    font-size: 16px;
    font-weight: bold;
    color: #555;
    margin-bottom: 12px;
    display: block;
}

.input-wrapper {
    position: relative;
}

.custom-input {
    width: 100%;
    height: 56px;
    background-color: #F7F8FA;
    border-radius: 28px;
    padding: 0 24px;
    font-size: 16px;
    color: #333;
    box-sizing: border-box;
}

.custom-textarea {
    width: 100%;
    height: 120px;
    background-color: #F7F8FA;
    border-radius: 28px;
    padding: 16px 24px;
    font-size: 16px;
    color: #333;
    box-sizing: border-box;
}

.placeholder-style {
    color: #B0B0B0;
}

.action-area {
    margin-top: 60px;
}

.submit-btn {
    width: 100%;
    height: 56px;
    background: #95CAFF;
    /* Primary theme color */
    border-radius: 28px;
    color: #fff;
    font-size: 18px;
    font-weight: bold;
    display: flex;
    align-items: center;
    justify-content: center;
    box-shadow: 0 6px 20px rgba(149, 202, 255, 0.4);
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
    margin-bottom: 12px;
}

.floor-tag {
    flex: 1;
    text-align: center;
    padding: 8px 0;
    margin: 0 4px;
    background: #F7F8FA;
    border-radius: 20px;
    font-size: 14px;
    color: #555;
    cursor: pointer;
    transition: all 0.2s;
    border: 1px solid transparent;

    &:first-child {
        margin-left: 0;
    }

    &:last-child {
        margin-right: 0;
    }

    &.active {
        background: rgba(149, 202, 255, 0.2);
        color: #95CAFF;
        border-color: #95CAFF;
        font-weight: 600;
    }

    &:active {
        transform: scale(0.95);
    }
}
</style>

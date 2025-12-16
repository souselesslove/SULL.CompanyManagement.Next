<template>
    <view class="container">
        <scroll-view scroll-y class="main-scroll">
            <view class="scroll-content">
                <!-- Header -->
                <view class="page-header">
                    <view class="back-btn" @click="goBack">
                        <view class="icon-arrow-left"></view>
                    </view>
                    <text class="page-title">{{ $t('profile.personal_info') }}</text>
                    <view class="placeholder-right"></view>
                </view>

                <!-- Form -->
                <view class="form-container">
                    <!-- Avatar -->
                    <view class="form-item avatar-item" @click="handleAvatarClick">
                        <text class="label">{{ $t('profile.avatar') }}</text>
                        <view class="avatar-wrapper">
                            <image class="avatar" :src="avatarUrl || '/static/avatar_placeholder.png'"
                                mode="aspectFill"></image>
                            <view class="icon-camera"></view>
                        </view>
                    </view>

                    <!-- User True Name -->
                    <view class="form-item">
                        <text class="label">{{ $t('profile.name') }}</text>
                        <input class="input" type="text" v-model="formData.UserTrueName"
                            :placeholder="$t('profile.name_placeholder')" />
                    </view>

                    <!-- Gender -->
                    <view class="form-item">
                        <text class="label">{{ $t('profile.gender') }}</text>
                        <picker :range="genderOptions" range-key="label" @change="onGenderChange" :value="genderIndex">
                            <view class="picker-input">
                                <text :class="{ 'placeholder-text': genderIndex === -1 }">{{ ((genderIndex > -1) ?
                                    genderOptions[genderIndex].label : $t('profile.gender')) }}</text>
                                <view class="icon-arrow-down"></view>
                            </view>
                        </picker>
                    </view>

                    <!-- Email -->
                    <view class="form-item">
                        <text class="label">{{ $t('profile.email') }}</text>
                        <input class="input" type="text" v-model="formData.Email"
                            :placeholder="$t('profile.email_placeholder')" />
                    </view>

                    <!-- Remark -->
                    <view class="form-item">
                        <text class="label">{{ $t('profile.remark') }}</text>
                        <input class="input" type="text" v-model="formData.Remark"
                            :placeholder="$t('profile.remark_placeholder')" />
                    </view>

                </view>

                <!-- Save Button -->
                <view class="button-container">
                    <button class="btn-save" @click="handleSave">{{ $t('common.save') }}</button>
                </view>

                <!-- Change Password Button (Styled differently/secondary) -->
                <view class="button-container-secondary">
                    <button class="btn-secondary" @click="showPasswordModal = true">{{ $t('profile.change_password')
                    }}</button>
                </view>

            </view>
        </scroll-view>

        <!-- Password Modal -->
        <view class="modal-mask" v-if="showPasswordModal" @click="closePasswordModal">
            <view class="modal-content" @click.stop>
                <view class="modal-header">
                    <text class="modal-title">{{ $t('profile.change_password') }}</text>
                    <text class="close-btn" @click="closePasswordModal">×</text>
                </view>
                <view class="modal-body">
                    <view class="form-item">
                        <text class="label">{{ $t('profile.current_password') }}</text>
                        <input class="modal-input" name="current-password" autocomplete="current-password" password
                            type="text" v-model="pwdData.oldPwd"
                            :placeholder="$t('profile.current_password_placeholder')" />
                    </view>
                    <view class="form-item">
                        <text class="label">{{ $t('profile.new_password') }}</text>
                        <input class="modal-input" name="new-password" autocomplete="new-password" password type="text"
                            v-model="pwdData.newPwd" :placeholder="$t('profile.new_password_placeholder')" />
                    </view>
                    <view class="form-item">
                        <text class="label">{{ $t('profile.confirm_password') }}</text>
                        <input class="modal-input" name="confirm-password" autocomplete="new-password" password
                            type="text" v-model="pwdData.confirmPwd"
                            :placeholder="$t('profile.confirm_password_placeholder')" />
                    </view>
                </view>
                <view class="modal-footer">
                    <button class="btn btn-secondary btn-sm" @click="closePasswordModal">{{ $t('common.cancel')
                        }}</button>
                    <button class="btn btn-primary btn-sm" @click="submitPasswordChange">{{ $t('common.confirm')
                        }}</button>
                </view>
            </view>
        </view>

    </view>
</template>

<script>
import request from '@/utils/request.js';

export default {
    data() {
        return {
            userInfo: {},
            formData: {
                UserTrueName: '',
                Email: '',
                Gender: 0, // 0: Male, 1: Female
                Remark: '',
                HeadImageUrl: ''
            },
            pwdData: {
                oldPwd: '',
                newPwd: '',
                confirmPwd: ''
            },
            avatarUrl: '',
            showPasswordModal: false
        }
    },
    computed: {
        genderOptions() {
            return [
                { value: 0, label: this.$t('profile.male') },
                { value: 1, label: this.$t('profile.female') }
            ];
        },
        genderIndex() {
            return this.genderOptions.findIndex(o => o.value === this.formData.Gender);
        }
    },
    onShow() {
        this.fetchUserInfo();
    },
    methods: {
        goBack() {
            uni.navigateBack();
        },
        fetchUserInfo() {
            const info = uni.getStorageSync('userInfo');
            if (info) {
                this.userInfo = info;
                this.formData.UserTrueName = info.userTrueName || info.UserTrueName;
                this.formData.Email = info.email || info.Email;
                this.formData.Remark = info.remark || info.Remark;
                this.formData.Gender = (info.gender !== undefined) ? info.gender : (info.Gender !== undefined ? info.Gender : 0);
                this.formData.HeadImageUrl = info.headImageUrl || info.HeadImageUrl;

                this.updateAvatarUrl(this.formData.HeadImageUrl);
            }
        },
        updateAvatarUrl(path) {
            if (path) {
                if (path.startsWith('http')) {
                    this.avatarUrl = path;
                } else {
                    // Ensure request.config.RESOURCE_URL is correct or fallback
                    this.avatarUrl = (request.config.RESOURCE_URL || '') + path;
                }
            } else {
                this.avatarUrl = '';
            }
        },
        onGenderChange(e) {
            const index = e.detail.value;
            this.formData.Gender = this.genderOptions[index].value;
        },
        handleAvatarClick() {
            uni.chooseImage({
                count: 1,
                sizeType: ['compressed'],
                success: (res) => {
                    const tempFilePaths = res.tempFilePaths;
                    this.uploadAvatar(tempFilePaths[0], res.tempFiles[0].name);
                }
            });
        },
        uploadAvatar(filePath, fileName) {
            const token = uni.getStorageSync('token');
            uni.uploadFile({
                url: request.config.BASE_URL + 'api/sys_user/upload',
                filePath: filePath,
                name: 'fileInput',
                header: {
                    'Authorization': token ? `Bearer ${token}` : ''
                },
                success: (uploadFileRes) => {
                    let data;
                    try {
                        data = JSON.parse(uploadFileRes.data);
                    } catch (e) {
                        data = uploadFileRes.data;
                    }

                    if (uploadFileRes.statusCode === 200 && (data.status || data.success)) {
                        const newAvatarPath = data.data + fileName;
                        // Update local form data
                        this.formData.HeadImageUrl = newAvatarPath;
                        this.updateAvatarUrl(newAvatarPath);
                    } else {
                        uni.showToast({
                            title: data.message || 'Upload Failed',
                            icon: 'none'
                        });
                    }
                },
                fail: (err) => {
                    console.error(err);
                    uni.showToast({
                        title: 'Upload Error',
                        icon: 'none'
                    });
                }
            });
        },
        handleSave() {
            // Validation
            if (!this.formData.UserTrueName) {
                uni.showToast({ title: this.$t('profile.name_placeholder'), icon: 'none' });
                return;
            }

            // Email Validation
            if (this.formData.Email) {
                const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
                if (!emailRegex.test(this.formData.Email)) {
                    uni.showToast({ title: this.$t('profile.invalid_email'), icon: 'none' });
                    return;
                }
            }

            const updateData = {
                ...this.userInfo,
                UserTrueName: this.formData.UserTrueName,
                Gender: this.formData.Gender,
                Email: this.formData.Email,
                Remark: this.formData.Remark,
                HeadImageUrl: this.formData.HeadImageUrl
            };

            request.post('api/user/updateUserInfo', updateData)
                .then(res => {
                    if (res === '修改成功') {
                        uni.showToast({ title: this.$t('profile.save_success'), icon: 'success' });
                        // Update local storage
                        const newUser = { ...this.userInfo, ...updateData, userTrueName: updateData.UserTrueName, headImageUrl: updateData.HeadImageUrl };
                        // Maintain camelCase compatibility if original was camelCase
                        uni.setStorageSync('userInfo', newUser);

                        setTimeout(() => uni.navigateBack(), 1000);
                    } else {
                        uni.showToast({ title: res.message || 'Update Failed', icon: 'none' });
                    }
                })
                .catch(err => {
                    console.error(err);
                    uni.showToast({ title: 'Error', icon: 'none' });
                });
        },
        // Password Logic
        closePasswordModal() {
            this.showPasswordModal = false;
            this.pwdData = { oldPwd: '', newPwd: '', confirmPwd: '' };
        },
        submitPasswordChange() {
            if (!this.pwdData.oldPwd) {
                uni.showToast({ title: this.$t('profile.current_password_placeholder'), icon: 'none' });
                return;
            }
            if (!this.pwdData.newPwd) {
                uni.showToast({ title: this.$t('profile.new_password_placeholder'), icon: 'none' });
                return;
            }
            if (this.pwdData.newPwd.length < 6) {
                uni.showToast({ title: 'Password must be at least 6 characters', icon: 'none' });
                return;
            }
            if (this.pwdData.newPwd !== this.pwdData.confirmPwd) {
                uni.showToast({ title: 'Passwords do not match', icon: 'none' });
                return;
            }

            request.post('api/user/modifyPwd', {
                oldPwd: this.pwdData.oldPwd,
                newPwd: this.pwdData.newPwd
            }).then(res => {
                if (res.success || res.status) {
                    uni.showToast({ title: this.$t('profile.password_changed'), icon: 'success' });
                    this.closePasswordModal();
                } else {
                    uni.showToast({ title: res.message || 'Failed', icon: 'none' });
                }
            }).catch(e => {
                uni.showToast({ title: 'Error', icon: 'none' });
            });
        }
    }
}
</script>

<style lang="scss" scoped>


page {
    background-color: #F8F8F8;
    height: 100%;
    font-family: $font-family-main;
}

.container {
    display: flex;
    flex-direction: column;
    height: 100vh;
    width: 100%;
}

.main-scroll {
    flex: 1;
    height: 0;
}

.scroll-content {
    padding: 0 24px 40px;
}

/* Header */
.page-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 40px 0 20px 0;
    height: 44px;
}

.back-btn {
    width: 40px;
    height: 40px;
    display: flex;
    align-items: center;
    justify-content: flex-start;
}

.icon-arrow-left {
    width: 24px;
    height: 24px;
    background-color: $sull-text-main;
    -webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M15.41 7.41L14 6l-6 6 6 6 1.41-1.41L10.83 12z'/%3E%3C/svg%3E");
    -webkit-mask-size: contain;
    mask-size: contain;
    -webkit-mask-repeat: no-repeat;
    mask-repeat: no-repeat;
    -webkit-mask-position: center;
    mask-position: center;
    mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M15.41 7.41L14 6l-6 6 6 6 1.41-1.41L10.83 12z'/%3E%3C/svg%3E");
}

.page-title {
    font-size: 18px;
    font-weight: 600;
    color: $sull-text-main;
}

.placeholder-right {
    width: 40px;
}

/* Form */
.form-container {
    background: #FFFFFF;
    border-radius: 16px;
    padding: 20px;
    margin-top: 10px;
}

.form-item {
    display: flex;
    flex-direction: column;
    margin-bottom: 20px;
}

.avatar-item {
    align-items: center;
    margin-bottom: 30px;
}

.avatar-wrapper {
    position: relative;
    margin-top: 10px;
}

.avatar {
    width: 80px;
    height: 80px;
    border-radius: 50%;
    background-color: #f0f0f0;
    border: 2px solid #fff;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.icon-camera {
    position: absolute;
    bottom: 0;
    right: 0;
    width: 24px;
    height: 24px;
    background-color: $sull-color-primary;
    border-radius: 50%;
    border: 2px solid #fff;
    -webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M12 9c-1.66 0-3 1.34-3 3s1.34 3 3 3 3-1.34 3-3-1.34-3-3-3zm0 4.5c-.83 0-1.5-.67-1.5-1.5s.67-1.5 1.5-1.5 1.5.67 1.5 1.5-.67 1.5-1.5 1.5zm8-4.5h-2.19l-.81-1.48C16.66 6.66 16.03 6 15.19 6H8.81c-.84 0-1.47.66-1.81 1.52L6.19 9H4c-1.1 0-2 .9-2 2v10c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V11c0-1.1-.9-2-2-2z'/%3E%3C/svg%3E");
    mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M12 9c-1.66 0-3 1.34-3 3s1.34 3 3 3 3-1.34 3-3-1.34-3-3-3zm0 4.5c-.83 0-1.5-.67-1.5-1.5s.67-1.5 1.5-1.5 1.5.67 1.5 1.5-.67 1.5-1.5 1.5zm8-4.5h-2.19l-.81-1.48C16.66 6.66 16.03 6 15.19 6H8.81c-.84 0-1.47.66-1.81 1.52L6.19 9H4c-1.1 0-2 .9-2 2v10c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V11c0-1.1-.9-2-2-2z'/%3E%3C/svg%3E");
    background-size: 16px;
    background-repeat: no-repeat;
    background-position: center;
    /* Using mask-image above with background-color to color the icon white? No, primary bg, white icon. */
    /* Let's fix mask logic: bg color is primary. Image should be white. */
    -webkit-mask-size: 16px;
    mask-size: 16px;
    -webkit-mask-repeat: no-repeat;
    mask-repeat: no-repeat;
    -webkit-mask-position: center;
    mask-position: center;
}

/* Fix camera icon mask logic */
.icon-camera::after {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: #fff;
    -webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M12 9c-1.66 0-3 1.34-3 3s1.34 3 3 3 3-1.34 3-3-1.34-3-3-3zm0 4.5c-.83 0-1.5-.67-1.5-1.5s.67-1.5 1.5-1.5 1.5.67 1.5 1.5-.67 1.5-1.5 1.5zm8-4.5h-2.19l-.81-1.48C16.66 6.66 16.03 6 15.19 6H8.81c-.84 0-1.47.66-1.81 1.52L6.19 9H4c-1.1 0-2 .9-2 2v10c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V11c0-1.1-.9-2-2-2z'/%3E%3C/svg%3E");
    mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M12 9c-1.66 0-3 1.34-3 3s1.34 3 3 3 3-1.34 3-3-1.34-3-3-3zm0 4.5c-.83 0-1.5-.67-1.5-1.5s.67-1.5 1.5-1.5 1.5.67 1.5 1.5-.67 1.5-1.5 1.5zm8-4.5h-2.19l-.81-1.48C16.66 6.66 16.03 6 15.19 6H8.81c-.84 0-1.47.66-1.81 1.52L6.19 9H4c-1.1 0-2 .9-2 2v10c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V11c0-1.1-.9-2-2-2z'/%3E%3C/svg%3E");
    -webkit-mask-size: 14px;
    mask-size: 14px;
    -webkit-mask-repeat: no-repeat;
    mask-repeat: no-repeat;
    -webkit-mask-position: center;
    mask-position: center;

}

.label {
    font-size: 14px;
    color: $sull-text-secondary;
    margin-bottom: 8px;
    font-weight: 500;
}

.input,
.picker-input {
    height: 48px;
    background: #F6F8FA;
    border-radius: 12px;
    padding: 0 16px;
    font-size: 14px;
    color: $sull-text-main;
    display: flex;
    align-items: center;
    justify-content: space-between;
}

.placeholder-text {
    color: #999;
}

.icon-arrow-down {
    width: 20px;
    height: 20px;
    background-color: #ccc;
    -webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M7.41 8.59L12 13.17l4.59-4.58L18 10l-6 6-6-6 1.41-1.41z'/%3E%3C/svg%3E");
    mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M7.41 8.59L12 13.17l4.59-4.58L18 10l-6 6-6-6 1.41-1.41z'/%3E%3C/svg%3E");
    -webkit-mask-size: contain;
    mask-size: contain;
    -webkit-mask-repeat: no-repeat;
    mask-repeat: no-repeat;
    -webkit-mask-position: center;
    mask-position: center;
}

.button-container {
    margin-top: 30px;
}

.button-container-secondary {
    margin-top: 16px;
}

.btn-save {
    width: 100%;
    height: 50px;
    background: $sull-color-primary;
    color: #FFFFFF;
    border-radius: 25px;
    font-size: 16px;
    font-weight: 600;
    display: flex;
    align-items: center;
    justify-content: center;
}

.btn-secondary {
    width: 100%;
    height: 50px;
    background: #FFFFFF;
    color: $sull-color-primary;
    border: 1px solid $sull-color-primary;
    border-radius: 25px;
    font-size: 16px;
    font-weight: 600;
    display: flex;
    align-items: center;
    justify-content: center;
}

/* Modal Styles */
.modal-mask {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0, 0, 0, 0.5);
    z-index: 999;
    display: flex;
    align-items: center;
    justify-content: center;
}

.modal-content {
    width: 85%;
    background: #FFFFFF;
    border-radius: 16px;
    overflow: hidden;
}

.modal-header {
    padding: 16px;
    border-bottom: 1px solid #eee;
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.modal-title {
    font-size: 16px;
    font-weight: bold;
    color: $sull-text-main;
}

.close-btn {
    font-size: 24px;
    color: #999;
    padding: 0 8px;
}

.modal-body {
    padding: 20px;
}

.modal-input {
    height: 44px;
    border: 1px solid #eee;
    border-radius: 8px;
    padding: 0 12px;
    font-size: 14px;
    width: 100%;
    box-sizing: border-box;
    display: block;
}

.modal-footer {
    padding: 16px;
    display: flex;
    gap: 12px;
    border-top: 1px solid #eee;
}

.btn {
    flex: 1;
    height: 40px;
    /* Adjust height if btn-sm overrides aren't enough or need specificity, but base class handles padding */
    /* border-radius is handled by .btn class in sull.scss */
    /* Inherit or reset properties if needed, but here we use global classes */
}

/* Overrides for local modal layout if global classes aren't enough */
.modal-footer .btn {
    min-width: 80px;
}
</style>

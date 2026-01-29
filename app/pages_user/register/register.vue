<template>
	<view class="container">
		<view class="login-container">
			<!-- Header -->
			<view class="login-header">
				<view class="back-btn-placeholder" @click="goBack">
					<BackButton :color="'#000000'" />
				</view>
				<view class="login-title-section">
					<text class="login-title">{{ $t('register.title') }}</text>
					<text class="login-subtitle">{{ $t('register.subtitle') }}</text>
				</view>
			</view>

			<!-- Form -->
			<scroll-view scroll-y class="login-form-scroll">
				<view class="login-form">

					<!-- Login Name -->
					<view class="form-group">
						<text class="label">{{ $t('register.username_label') }}</text>
						<input class="input" type="text" v-model="username"
							:placeholder="$t('register.username_placeholder')" placeholder-class="placeholder" />
					</view>

					<!-- Password -->
					<view class="form-group">
						<text class="label">{{ $t('register.password_label') }}</text>
						<view class="input-wrapper">
							<input class="input" :password="!showPassword" v-model="password"
								:placeholder="$t('register.password_placeholder')" placeholder-class="placeholder" />
							<view class="eye-icon" @click="togglePassword">
								<view :class="showPassword ? 'icon-eye-off' : 'icon-eye'"></view>
							</view>
						</view>
					</view>

					<!-- Full Name -->
					<view class="form-group">
						<text class="label">{{ $t('register.fullname_label') }}</text>
						<input class="input" type="text" v-model="fullname"
							:placeholder="$t('register.fullname_placeholder')" placeholder-class="placeholder" />
					</view>

					<!-- Mobile Number -->
					<!-- <view class="form-group">
						<text class="label">{{ $t('register.phone_label') }}</text>
						<input class="input" type="number" v-model="phone" :placeholder="$t('register.phone_placeholder')" placeholder-class="placeholder" />
					</view> -->

					<!-- Verification Code -->
					<!-- <view class="form-group">
						<text class="label">{{ $t('register.code_label') }}</text>
						<view class="code-input-row">
							<input class="input" type="number" v-model="code" :placeholder="$t('register.code_placeholder')" placeholder-class="placeholder" />
							<view class="code-btn" :class="{ 'disabled': countdown > 0 }" @click="getVerificationCode">
								<text class="code-btn-text">{{ countdown > 0 ? `${countdown}s` : $t('login.get_code') }}</text>
							</view>
						</view>
					</view> -->

					<!-- Invitation Code -->
					<view class="form-group">
						<text class="label">{{ $t('register.invite_label') }}</text>
						<input class="input" type="text" v-model="inviteCode"
							:placeholder="$t('register.invite_placeholder')" placeholder-class="placeholder" />
					</view>

					<!-- Submit Button -->
					<button class="btn-signin" @click="handleRegister">{{ $t('register.submit') }}</button>

					<!-- Login Link -->
					<view class="signup-area">
						<text class="no-account-text">{{ $t('register.have_account') }}</text>
						<text class="signup-text" @click="goBack">{{ $t('register.sign_in') }}</text>
					</view>

					<!-- Footer Terms -->
					<view class="login-footer">
						<text class="footer-text">{{ $t('login.terms_agree') }} <text class="footer-link">{{
							$t('login.terms') }}</text></text>
						<text class="footer-text">{{ $t('login.and') }} <text class="footer-link">{{
							$t('login.conditions') }}</text></text>
					</view>
				</view>
			</scroll-view>
		</view>

		<!-- Success Modal -->
		<SuccessModal v-model:visible="successModal.visible" :title="successModal.title" :message="successModal.message"
			:buttonText="successModal.buttonText" @confirm="handleSuccessConfirm" />
	</view>
</template>

<script>
import request from '@/utils/request.js';
import BackButton from '@/components/BackButton/BackButton.vue';
import SuccessModal from '@/components/SuccessModal/SuccessModal.vue';

export default {
	components: {
		BackButton,
		SuccessModal
	},
	data() {
		return {
			username: '',
			password: '',
			fullname: '',
			phone: '',
			code: '',
			inviteCode: '',

			showPassword: false,
			countdown: 0,

			// Success Modal State
			successModal: {
				visible: false,
				title: '',
				message: '',
				buttonText: ''
			}
		}
	},
	methods: {
		goBack() {
			const pages = getCurrentPages();
			if (pages.length > 1) {
				uni.navigateBack();
			} else {
				// If refreshed or direct entry, fallback to profile/login page
				uni.reLaunch({
					url: '/pages/profile/profile'
				});
			}
		},
		togglePassword() {
			this.showPassword = !this.showPassword;
		},
		getVerificationCode() {
			if (this.countdown > 0) return;
			if (!this.phone) {
				uni.showToast({
					title: this.$t('register.phone_placeholder'),
					icon: 'none'
				});
				return;
			}

			// Mock sending code
			uni.showToast({
				title: 'Code sent: 1234',
				icon: 'none'
			});

			this.countdown = 60;
			const timer = setInterval(() => {
				this.countdown--;
				if (this.countdown <= 0) {
					clearInterval(timer);
				}
			}, 1000);
		},
		handleRegister() {
			// Basic validation
			if (!this.username || !this.password || !this.fullname) {
				uni.showToast({
					title: this.$t('register.required_fields'),
					icon: 'none'
				});
				return;
			}

			const mainData = {
				UserName: this.username,
				UserTrueName: this.fullname,
				Password: this.password,
				InviteCode: this.inviteCode,
				Enable: 1,
				Gender: 0,
				Email: null,
				DeptIds: "",
				Remark: null,
				HeadImageUrl: "",
				User_Id: null
			};
			const data = { "mainData": mainData, "delKeys": null };
			request.post('api/Sys_User/register', data)
				.then(res => {
					if (res.status) {
						// Show custom success modal
						this.successModal.title = this.$t('register.reg_success_title');
						this.successModal.buttonText = this.$t('modal.continue');
						this.successModal.message = res.message || this.$t('register.reg_success_msg');
						this.successModal.visible = true;
					} else {
						uni.showToast({
							title: res.message || this.$t('register.fail_msg'),
							icon: 'none'
						});
					}
				})
				.catch(err => {
					// Error handling is mostly done in request.js, but we can do extra here
					console.error(err);
				});
		},
		handleSuccessConfirm() {
			this.goBack();
		}
	}
}
</script>

<style lang="scss" scoped>
.container {
	display: flex;
	flex-direction: column;
	height: 100vh;
	width: 100%;
	background-color: #ffffff;
}

.login-container {
	display: flex;
	flex-direction: column;
	height: 100vh;
	background-color: #ffffff;
}

.login-header {
	padding: 120rpx 48rpx 80rpx;
	background: linear-gradient(180deg, $sull-color-primary 0%, $sull-color-secondary-light 100%);
	display: flex;
	flex-direction: column;
}

.back-btn-placeholder {
	width: 80rpx;
	height: 80rpx;
	background: #ffffff;
	border-radius: 50%;
	display: flex;
	align-items: center;
	justify-content: center;
	margin-bottom: 48rpx;
}

.login-title {
	font-size: 56rpx;
	font-weight: bold;
	color: #ffffff;
	margin-bottom: 16rpx;
	display: block;
}

.login-subtitle {
	font-size: 32rpx;
	color: rgba(255, 255, 255, 0.9);
}

.login-form-scroll {
	flex: 1;
	background: #ffffff;
	border-radius: 48rpx 48rpx 0 0;
	margin-top: -48rpx;
	padding-top: 60rpx;
	overflow: hidden;
}

.login-form {
	padding: 0 48rpx 80rpx;
}

.form-group {
	margin-bottom: 40rpx;
}

.label {
	font-size: 28rpx;
	font-weight: 600;
	color: $sull-text-secondary;
	margin-bottom: 16rpx;
	display: block;
}

.input {
	width: 100%;
	height: 100rpx;
	background: #F6F8FA;
	border-radius: 32rpx;
	padding: 0 32rpx;
	font-size: 28rpx;
	color: $sull-text-main;
	box-sizing: border-box;
}

.input-wrapper {
	position: relative;
}

.eye-icon {
	position: absolute;
	right: 32rpx;
	top: 50%;
	transform: translateY(-50%);
	width: 48rpx;
	height: 48rpx;
	display: flex;
	align-items: center;
	justify-content: center;
	z-index: 10;
}

.icon-eye {
	width: 40rpx;
	height: 40rpx;
	background-color: $sull-text-placeholder;
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M12 4.5C7 4.5 2.73 7.61 1 12c1.73 4.39 6 7.5 11 7.5s9.27-3.11 11-7.5c-1.73-4.39-6-7.5-11-7.5zM12 17c-2.76 0-5-2.24-5-5s2.24-5 5-5 5 2.24 5 5-2.24 5-5 5zm0-8c-1.66 0-3 1.34-3 3s1.34 3 3 3 3-1.34 3-3-1.34-3-3-3z'/%3E%3C/svg%3E");
	-webkit-mask-size: contain;
	mask-size: contain;
	-webkit-mask-repeat: no-repeat;
	mask-repeat: no-repeat;
	-webkit-mask-position: center;
	mask-position: center;
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M12 4.5C7 4.5 2.73 7.61 1 12c1.73 4.39 6 7.5 11 7.5s9.27-3.11 11-7.5c-1.73-4.39-6-7.5-11-7.5zM12 17c-2.76 0-5-2.24-5-5s2.24-5 5-5 5 2.24 5 5-2.24 5-5 5zm0-8c-1.66 0-3 1.34-3 3s1.34 3 3 3 3-1.34 3-3-1.34-3-3-3z'/%3E%3C/svg%3E");
}

.icon-eye-off {
	width: 40rpx;
	height: 40rpx;
	background-color: $sull-text-placeholder;
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M12 7c2.76 0 5 2.24 5 5 0 .65-.13 1.26-.36 1.83l2.92 2.92c1.51-1.26 2.7-2.89 3.43-4.75-1.73-4.39-6-7.5-11-7.5-1.4 0-2.74.25-3.98.7l2.16 2.16C10.74 7.13 11.35 7 12 7zM2 4.27l2.28 2.28.46.46C3.08 8.3 1.78 10.02 1 12c1.73 4.39 6 7.5 11 7.5 3.1 0 5.81-1.3 7.78-3.41l.95.95 2.27-2.27 1.62-.27L2 4.27zM7.53 9.8l1.55 1.55c-.05.21-.08.43-.08.65 0 1.66 1.34 3 3 3 .22 0 .44-.03.65-.08l1.55 1.55c-.67.33-1.41.53-2.2.53-2.76 0-5-2.24-5-5 0-.79.2-1.53.53-2.2zm4.31-.78l3.15 3.15.02-.16c0-1.66-1.34-3-3-3l-.17.01z'/%3E%3C/svg%3E");
	-webkit-mask-size: contain;
	mask-size: contain;
	-webkit-mask-repeat: no-repeat;
	mask-repeat: no-repeat;
	-webkit-mask-position: center;
	mask-position: center;
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M12 7c2.76 0 5 2.24 5 5 0 .65-.13 1.26-.36 1.83l2.92 2.92c1.51-1.26 2.7-2.89 3.43-4.75-1.73-4.39-6-7.5-11-7.5-1.4 0-2.74.25-3.98.7l2.16 2.16C10.74 7.13 11.35 7 12 7zM2 4.27l2.28 2.28.46.46C3.08 8.3 1.78 10.02 1 12c1.73 4.39 6 7.5 11 7.5 3.1 0 5.81-1.3 7.78-3.41l.95.95 2.27-2.27 1.62-.27L2 4.27zM7.53 9.8l1.55 1.55c-.05.21-.08.43-.08.65 0 1.66 1.34 3 3 3 .22 0 .44-.03.65-.08l1.55 1.55c-.67.33-1.41.53-2.2.53-2.76 0-5-2.24-5-5 0-.79.2-1.53.53-2.2zm4.31-.78l3.15 3.15.02-.16c0-1.66-1.34-3-3-3l-.17.01z'/%3E%3C/svg%3E");
}

.code-input-row {
	display: flex;
	align-items: center;
	gap: 24rpx;
}

.code-btn {
	height: 100rpx;
	padding: 0 40rpx;
	background: #F6F8FA;
	border-radius: 32rpx;
	display: flex;
	align-items: center;
	justify-content: center;
	white-space: nowrap;
	transition: all 0.3s;
	min-width: 200rpx;
}

.code-btn.disabled {
	opacity: 0.6;
	background: #eeeeee;
}

.code-btn-text {
	color: $sull-color-primary;
	font-size: 28rpx;
	font-weight: 600;
}

.code-btn.disabled .code-btn-text {
	color: #999999;
}

.btn-signin {
	width: 100%;
	height: 108rpx;
	background: $sull-color-primary;
	border-radius: 54rpx;
	display: flex;
	align-items: center;
	justify-content: center;
	color: #ffffff;
	font-size: 36rpx;
	font-weight: 600;
	margin-bottom: 48rpx;
	margin-top: 40rpx;
}

.signup-area {
	display: flex;
	justify-content: center;
	gap: 8rpx;
	margin-bottom: 80rpx;
}

.no-account-text {
	font-size: 28rpx;
	color: $sull-text-secondary;
}

.signup-text {
	font-size: 28rpx;
	color: $sull-color-primary;
	font-weight: 600;
}

.login-footer {
	display: flex;
	flex-direction: column;
	align-items: center;
	margin-top: auto;
	gap: 8rpx;
	padding-bottom: 40rpx;
}

.footer-text {
	font-size: 24rpx;
	color: $sull-text-secondary;
}

.footer-link {
	color: $sull-text-main;
	font-weight: bold;
}
</style>

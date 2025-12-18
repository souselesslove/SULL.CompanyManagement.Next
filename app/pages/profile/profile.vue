<template>
	<view class="container">
		<!-- Login View -->
		<view v-if="!isLoggedIn" class="login-container">
			<view class="login-header">
				<view class="back-btn-placeholder">
					<view class="icon-arrow-left"></view>
				</view>
				<view class="login-title-section">
					<text class="login-title">{{ $t('login.title') }}</text>
					<text class="login-subtitle">{{ $t('login.subtitle') }}</text>
				</view>
			</view>

			<view class="login-form">
				<!-- Username Login Mode -->
				<template v-if="loginMethod === 'username'">
					<!-- Username -->
					<view class="form-group">
						<text class="label">{{ $t('login.username_label') }}</text>
						<input class="input" type="text" v-model="username"
							:placeholder="$t('login.username_placeholder')" placeholder-class="placeholder" />
					</view>

					<!-- Password -->
					<view class="form-group">
						<text class="label">{{ $t('login.password_label') }}</text>
						<view class="input-wrapper">
							<input class="input" :password="!showPassword" v-model="password"
								:placeholder="$t('login.password_placeholder')" placeholder-class="placeholder" />
							<view class="eye-icon" @click="togglePassword">
								<view :class="showPassword ? 'icon-eye-off' : 'icon-eye'"></view>
							</view>
						</view>
					</view>

					<!-- Verification Code (Captcha) -->
					<view class="form-group">
						<text class="label">{{ $t('login.code_label') }}</text>
						<view class="code-input-row">
							<input class="input" type="text" v-model="verificationCode"
								:placeholder="$t('login.code_placeholder')" placeholder-class="placeholder" />
							<image class="captcha-img" :src="captchaBase64" mode="scaleToFill" @click="getCaptcha">
							</image>
						</view>
					</view>
				</template>

				<!-- Phone Login Mode -->
				<template v-else>
					<!-- Phone Number -->
					<view class="form-group">
						<text class="label">{{ $t('login.phone_label') }}</text>
						<input class="input" type="number" v-model="phone" :placeholder="$t('login.phone_placeholder')"
							placeholder-class="placeholder" />
					</view>

					<!-- Verification Code -->
					<view class="form-group">
						<text class="label">{{ $t('login.code_label') }}</text>
						<view class="code-input-row">
							<input class="input" type="number" v-model="code"
								:placeholder="$t('login.code_placeholder')" placeholder-class="placeholder" />
							<view class="code-btn" :class="{ 'disabled': countdown > 0 }" @click="getVerificationCode">
								<text class="code-btn-text">{{ countdown > 0 ? `${countdown}s` : $t('login.get_code')
								}}</text>
							</view>
						</view>
					</view>
				</template>

				<!-- Forgot Password -->
				<view class="form-actions" style="justify-content: flex-end;">
					<text class="forgot-text">{{ $t('login.forgot_password') }}</text>
				</view>

				<!-- Sign In Button -->
				<button class="btn-signin" @click="handleLogin">{{ $t('login.sign_in') }}</button>

				<!-- Sign Up Link -->
				<view class="signup-area">
					<text class="no-account-text">{{ $t('login.no_account') }}</text>
					<text class="signup-text" @click="navToRegister">{{ $t('login.sign_up') }}</text>
				</view>

				<!-- Social Login -->
				<view class="social-login">
					<view class="divider">
						<view class="line"></view>
						<text class="or-text">{{ $t('login.or_sign_in_with') }}</text>
						<view class="line"></view>
					</view>

					<view class="social-icons">
						<!-- Toggle to Phone Login -->
						<view v-if="loginMethod === 'username'" class="social-btn mobile"
							@click="switchLoginMethod('phone')">
							<text class="social-icon">📱</text>
						</view>

						<!-- Toggle to Username Login -->
						<view v-else class="social-btn username" @click="switchLoginMethod('username')">
							<text class="social-icon">👤</text>
						</view>

						<!-- WeChat Login -->
						<view class="social-btn wechat" @click="handleWechatLogin">
							<text class="social-icon">💬</text>
						</view>
					</view>
				</view>

				<!-- Footer Terms -->
				<view class="login-footer">
					<text class="footer-text">{{ $t('login.terms_agree') }} <text class="footer-link">{{
						$t('login.terms')
					}}</text></text>
					<text class="footer-text">{{ $t('login.and') }} <text class="footer-link">{{ $t('login.conditions')
					}}</text></text>
				</view>
			</view>
		</view>

		<!-- Profile View (Existing) -->
		<scroll-view v-else scroll-y class="main-scroll" refresher-enabled="true" :refresher-triggered="isRefreshing"
			@refresherrefresh="onRefresh" refresher-default-style="none" refresher-background="#f0f5ff">
			<view slot="refresher" class="custom-refresher">
				<view class="refresher-content">
					<view class="refresher-icon" :class="{ 'animating': isRefreshing }"></view>
					<text class="refresher-text">{{ isRefreshing ? $t('index.refreshing') : $t('index.pullToRefresh')
					}}</text>
				</view>
			</view>
			<!-- Header -->
			<view class="header">
				<text class="header-title">{{ $t('index.nav.profile') }}</text>
			</view>

			<view class="content">
				<!-- User Profile -->
				<view class="user-section">
					<view class="avatar-container" @click="handleAvatarClick">
						<image class="avatar" :src="avatarUrl" mode="aspectFill"></image>
						<view class="edit-badge">
							<view class="icon-edit"></view>
						</view>
					</view>
					<text class="user-name">{{ userInfo.userTrueName || 'Guest' }}</text>
					<text class="user-email">{{ userInfo.remark || '' }}</text>
				</view>

				<!-- Settings Section -->
				<view class="settings-section">
					<view class="menu-list">
						<!-- Organization -->
						<view class="menu-item" @click="navToOrgInfo">
							<view class="item-left">
								<view class="menu-icon icon-org"></view>
								<text class="menu-label">{{ orgBtnText }}</text>
							</view>
							<view class="item-right">
								<view class="icon-arrow"></view>
							</view>
						</view>

						<!-- Department -->
						<view class="menu-item" @click="navToDeptInfo">
							<view class="item-left">
								<view class="menu-icon icon-dept"></view>
								<text class="menu-label">{{ deptBtnText }}</text>
							</view>
							<view class="item-right">
								<view class="icon-arrow"></view>
							</view>
						</view>

						<!-- Personal Info -->
						<view class="menu-item" @click="navToPersonalInfo">
							<view class="item-left">
								<view class="menu-icon icon-user-info"></view>
								<text class="menu-label">{{ $t('profile.personal_info') }}</text>
							</view>
							<view class="item-right">
								<view class="icon-arrow"></view>
							</view>
						</view>

						<!-- Notification -->
						<view class="menu-item">
							<view class="item-left">
								<view class="menu-icon icon-bell"></view>
								<text class="menu-label">{{ $t('profile.notification') }}</text>
							</view>
							<view class="item-right">
								<view class="icon-arrow"></view>
							</view>
						</view>

						<!-- Languages -->
						<view class="menu-item" @click="navToLanguage">
							<view class="item-left">
								<view class="menu-icon icon-globe"></view>
								<text class="menu-label">{{ $t('profile.languages') }}</text>
							</view>
							<view class="item-right">
								<view class="icon-arrow"></view>
							</view>
						</view>

						<!-- Help and Support -->
						<view class="menu-item">
							<view class="item-left">
								<view class="menu-icon icon-info"></view>
								<text class="menu-label">{{ $t('profile.help') }}</text>
							</view>
							<view class="item-right">
								<view class="icon-arrow"></view>
							</view>
						</view>

						<!-- Logout -->
						<view class="menu-item logout-item" @click="handleLogout">
							<view class="item-left">
								<view class="menu-icon icon-logout"></view>
								<text class="menu-label">{{ $t('profile.logout') }}</text>
							</view>
						</view>
					</view>
				</view>
			</view>
		</scroll-view>

		<!-- Bottom Navigation (Only show if logged in) -->
		<BottomNav v-if="isLoggedIn" activePage="profile" />

		<!-- Confirm Modal -->
		<ConfirmModal v-model:visible="confirmModal.visible" :title="confirmModal.title" :message="confirmModal.message"
			:confirmText="confirmModal.confirmText" :cancelText="confirmModal.cancelText"
			:confirmColor="confirmModal.confirmColor" :cancelColor="confirmModal.cancelColor"
			@confirm="handleConfirmAction" @cancel="handleCancelAction" />
	</view>

</template>

<script>
import BottomNav from '@/components/BottomNav/BottomNav.vue';
import ConfirmModal from '@/components/ConfirmModal/ConfirmModal.vue';
import request from '@/utils/request.js';

export default {
	components: {
		BottomNav,
		ConfirmModal
	},
	data() {
		return {
			isLoggedIn: false,
			isDarkMode: false,
			// Login State
			loginMethod: 'username', // 'username' or 'phone'

			// Username Login Data
			username: '',
			password: '',
			showPassword: false,
			verificationCode: '', // Captcha input
			captchaBase64: '', // Captcha image
			uuid: '', // Captcha UUID

			// Phone Login Data
			phone: '',
			code: '',
			countdown: 0,

			// User Info
			userInfo: {},
			userOrgInfo: {},

			// Refresher state
			isRefreshing: false,

			// Confirm Modal State
			confirmModal: {
				visible: false,
				title: '',
				message: '',
				confirmText: 'Confirm',
				cancelText: 'Cancel',
				confirmColor: '#92C5F7',
				cancelColor: '#FF6B6B',
				actionType: '' // 'joinOrg', 'logout', etc.
			}
		}
	},
	computed: {
		avatarUrl() {
			// Ensure correct property name from API response, typically lowercase or camelCase
			// Also handle potential base URL requirement if headImageUrl is relative
			const url = this.userInfo.headImageUrl || this.userInfo.HeadImageUrl;
			if (url) {
				// Check if it's a full URL or relative path
				if (url.startsWith('http')) {
					return url;
				}
				// If using global config for base URL, import and use it here
				// For now assuming it might need base URL if relative
				return request.config.RESOURCE_URL + url;
			}
			return '/static/avatar_placeholder.png';
		},
		orgBtnText() {
			return (this.userOrgInfo && this.userOrgInfo.Organization && this.userOrgInfo.Organization.Name)
				? this.userOrgInfo.Organization.Name
				: this.$t('organization.title_info');
		},
		deptBtnText() {
			return (this.userOrgInfo && this.userOrgInfo.Department && this.userOrgInfo.Department.Name)
				? this.userOrgInfo.Department.Name
				: this.$t('department.title_info');
		}
	},
	onShow() {
		this.checkLogin();
		if (!this.isLoggedIn) {
			this.getCaptcha();
		} else {
			this.fetchUserInfo();
		}
	},
	methods: {
		checkLogin() {
			const token = uni.getStorageSync('token');
			this.isLoggedIn = !!token;
			if (this.isLoggedIn) {
				this.userInfo = uni.getStorageSync('userInfo') || {};
			}
		},
		togglePassword() {
			this.showPassword = !this.showPassword;
		},
		switchLoginMethod(method) {
			this.loginMethod = method;
		},
		getVerificationCode() {
			if (this.countdown > 0) return;
			if (!this.phone) {
				uni.showToast({
					title: this.$t('login.phone_placeholder'),
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
		getCaptcha() {
			request.get('api/User/getVerificationCode')
				.then(res => {
					// Adjust this based on actual response structure
					// Assuming res.img and res.uuid based on typical patterns, 
					// or the prompt description: "base64 image and UUID"
					if (res.status && res.data) {
						this.captchaBase64 = 'data:image/png;base64,' + res.data.img;
						this.uuid = res.data.uuid;
					} else {
						// Fallback if structure is flat
						const imgData = res.img || res.data?.img;
						if (imgData) {
							this.captchaBase64 = 'data:image/png;base64,' + imgData;
						}
						this.uuid = res.uuid || res.data?.uuid;
					}
				})
				.catch(err => {
					console.error("Failed to get captcha", err);
				});
		},
		handleLogin() {
			if (this.loginMethod === 'username') {
				if (!this.username || !this.password || !this.verificationCode) {
					uni.showToast({
						title: 'Please enter username, password and verification code',
						icon: 'none'
					});
					return;
				}

				const loginData = {
					userName: this.username,
					password: this.password,
					verificationCode: this.verificationCode,
					UUID: this.uuid
				};
				const t = this;
				request.post('api/user/login', loginData)
					.then(res => {
						if (res.status) {
							uni.setStorageSync('token', res.token || res.data?.token || 'mock-token-if-missing');
							// Fetch User Info
							t.fetchUserInfo();
							this.isLoggedIn = true;
							uni.showToast({
								title: 'Login Success',
								icon: 'success'
							});
						} else {
							uni.showToast({
								title: res.message || 'Login Failed',
								icon: 'none'
							});
							// Refresh captcha on failure
							this.getCaptcha();
						}
					})
					.catch(err => {
						console.error(err);
						this.getCaptcha();
					});

			} else {
				// Phone login logic (kept as mock for now, or implement if API exists)
				if (!this.phone || !this.code) {
					uni.showToast({
						title: this.$t('login.code_placeholder'),
						icon: 'none'
					});
					return;
				}
				// Mock success for phone login
				uni.setStorageSync('token', 'mock-token-phone-123456');
				this.isLoggedIn = true;
				uni.showToast({
					title: this.$t('login.sign_in') + ' Success',
					icon: 'success'
				});
			}
		},
		handleWechatLogin() {
			uni.showToast({
				title: 'WeChat Login Clicked',
				icon: 'none'
			});
		},
		handleLogout() {
			uni.removeStorageSync('token');
			this.isLoggedIn = false;
			// Reset form
			this.username = '';
			this.password = '';
			this.phone = '';
			this.code = '';
			uni.showToast({
				title: 'Logged Out',
				icon: 'none'
			});
			this.getCaptcha();
		},
		navTo(page) {
			// Navigation handled by BottomNav component
		},
		navToRegister() {
			uni.navigateTo({
				url: '/pages/register/register'
			});
		},
		navToOrgInfo() {
			uni.navigateTo({
				url: '/pages/organization/organization'
			});
		},
		navToDeptInfo() {
			uni.navigateTo({
				url: '/pages/department/department'
			});
		},
		navToLanguage() {
			uni.navigateTo({
				url: '/pages/profile/language'
			});
		},
		navToPersonalInfo() {
			uni.navigateTo({
				url: '/pages/profile/personal-info'
			});
		},
		handleAvatarClick() {
			uni.chooseImage({
				count: 1,
				sizeType: ['compressed'],
				success: (res) => {
					const tempFilePaths = res.tempFilePaths;
					let fileName = res.tempFiles[0].name;
					// Fallback if name is missing (common in WeChat/App)
					if (!fileName && tempFilePaths[0]) {
						const path = tempFilePaths[0];
						const lastSlash = path.lastIndexOf('/');
						if (lastSlash !== -1) {
							fileName = path.substring(lastSlash + 1);
						} else {
							fileName = path;
						}
					}
					// Final fallback if parsing failed
					if (!fileName) {
						fileName = `avatar_${new Date().getTime()}.jpg`;
					}
					this.uploadAvatar(tempFilePaths[0], fileName);
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
					// Response is a string, needs parsing if JSON
					let data;
					try {
						data = JSON.parse(uploadFileRes.data);
					} catch (e) {
						data = uploadFileRes.data;
					}
					if (uploadFileRes.statusCode === 200 && (data.status || data.success)) {
						// Update avatar path
						const newAvatarPath = data.data + fileName;
						if (newAvatarPath) {
							// Create update payload with existing user info and new avatar
							const updateData = {
								...this.userInfo,
								img: request.config.RESOURCE_URL + this.userInfo.headImageUrl
							};
							updateData.headImageUrl = newAvatarPath;
							request.post('api/user/updateUserInfo', updateData)
								.then(updateRes => {
									if (updateRes.statusCode === 200 || updateRes === '修改成功') {
										uni.showToast({
											title: 'Upload Success',
											icon: 'success'
										});
										// Refresh User Info
										this.fetchUserInfo();
									} else {
										uni.showToast({
											title: updateRes.message || 'Update Failed',
											icon: 'none'
										});
									}
								})
								.catch(err => {
									console.error(err);
									uni.showToast({
										title: 'Update Error',
										icon: 'none'
									});
								});
						} else {
							this.fetchUserInfo();
						}
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
		fetchUserInfo() {
			return request.post('api/user/GetCurrentUserInfo')
				.then(infoRes => {
					if (infoRes.status) {
						uni.setStorageSync('userInfo', infoRes.data);
						this.userInfo = infoRes.data;
						// Fetch Org Info after user info
						this.getUserOrganization();
					}
				})
				.catch(err => console.error("Failed to get user info", err));
		},
		getUserOrganization() {
			request.get('api/ssbh/OrganizationUser/UserOrganization')
				.then(res => {
					if (res) {
						const data = res;
						// Store in local cache
						uni.setStorageSync('userOrgInfo', data);
						this.userOrgInfo = data;

						// Check if Organization is missing
						if (!data.Organization || !data.Organization.Id || data.Organization.Id === 0) {
							this.showOrgMissingModal();
						}
					}
				})
				.catch(err => {
					console.error("Failed to get user organization", err);
				});
		},
		showOrgMissingModal() {
			this.confirmModal = {
				visible: true,
				title: this.$t('profile.org_required_title'),
				message: this.$t('profile.org_required_msg'),
				confirmText: this.$t('profile.join_now'),
				cancelText: this.$t('profile.not_now'),
				confirmColor: '#92C5F7',
				cancelColor: '#999999',
				actionType: 'joinOrg'
			};
		},
		handleConfirmAction() {
			if (this.confirmModal.actionType === 'joinOrg') {
				this.navToOrgInfo();
			}
		},
		handleCancelAction() {
			// Do nothing or maybe logout if strict
		},
		onRefresh() {
			this.isRefreshing = true;
			// Refresh user info
			this.fetchUserInfo().finally(() => {
				setTimeout(() => {
					this.isRefreshing = false;
					uni.showToast({
						title: this.$t('index.refreshed'),
						icon: 'none'
					});
				}, 1000); // Small delay for visual effect
			});
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

/* Login Styles */
.login-container {
	display: flex;
	flex-direction: column;
	height: 100vh;
	background-color: #ffffff;
}

.login-header {
	padding: 60px 24px 40px;
	background: linear-gradient(180deg, $sull-color-primary 0%, $sull-color-secondary-light 100%);
	display: flex;
	flex-direction: column;
}

.back-btn-placeholder {
	width: 40px;
	height: 40px;
	background: #ffffff;
	border-radius: 50%;
	display: flex;
	align-items: center;
	justify-content: center;
	margin-bottom: 24px;
}

.icon-arrow-left {
	width: 20px;
	height: 20px;
	background-color: #000;
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M20 11H7.83l5.59-5.59L12 4l-8 8 8 8 1.41-1.41L7.83 13H20v-2z'/%3E%3C/svg%3E");
	-webkit-mask-size: contain;
	mask-size: contain;
	-webkit-mask-repeat: no-repeat;
	mask-repeat: no-repeat;
	-webkit-mask-position: center;
	mask-position: center;
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M20 11H7.83l5.59-5.59L12 4l-8 8 8 8 1.41-1.41L7.83 13H20v-2z'/%3E%3C/svg%3E");
}

.login-title {
	font-size: 28px;
	font-weight: bold;
	color: #ffffff;
	margin-bottom: 8px;
	display: block;
}

.login-subtitle {
	font-size: 16px;
	color: rgba(255, 255, 255, 0.9);
}

.login-form {
	padding: 30px 24px;
	background: #ffffff;
	flex: 1;
	border-radius: 24px 24px 0 0;
	margin-top: -24px;
	/* Slight overlap if needed, or just flow */
}

.form-group {
	margin-bottom: 20px;
}

.label {
	font-size: 14px;
	font-weight: 600;
	color: $sull-text-secondary;
	margin-bottom: 8px;
	display: block;
}

.input {
	width: 100%;
	height: 50px;
	background: #F6F8FA;
	border-radius: 16px;
	padding: 0 16px;
	font-size: 14px;
	color: $sull-text-main;
	box-sizing: border-box;
}

.input-wrapper {
	position: relative;
}

.eye-icon {
	position: absolute;
	right: 16px;
	top: 50%;
	transform: translateY(-50%);
	width: 24px;
	height: 24px;
	display: flex;
	align-items: center;
	justify-content: center;
	z-index: 10;
}

.code-input-row {
	display: flex;
	align-items: center;
	gap: 12px;
}

.code-btn {
	height: 50px;
	padding: 0 20px;
	background: #F6F8FA;
	border-radius: 16px;
	display: flex;
	align-items: center;
	justify-content: center;
	white-space: nowrap;
	transition: all 0.3s;
	min-width: 100px;
}

.code-btn.disabled {
	opacity: 0.6;
	background: #eeeeee;
}

.code-btn-text {
	color: $sull-color-primary;
	font-size: 14px;
	font-weight: 600;
}

.code-btn.disabled .code-btn-text {
	color: #999999;
}

.icon-eye {
	width: 20px;
	height: 20px;
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
	width: 20px;
	height: 20px;
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

.form-actions {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-bottom: 30px;
}

.remember-me {
	display: flex;
	align-items: center;
	gap: 8px;
}

.checkbox {
	width: 20px;
	height: 20px;
	border: 1px solid #ddd;
	border-radius: 4px;
	display: flex;
	align-items: center;
	justify-content: center;
}

.checkbox.checked {
	background-color: $sull-color-primary;
	border-color: $sull-color-primary;
}

.checkmark {
	color: #fff;
	font-size: 14px;
	line-height: 1;
}

.action-text {
	font-size: 14px;
	color: $sull-text-secondary;
}

.forgot-text {
	font-size: 14px;
	color: $sull-color-error;
}

.btn-signin {
	width: 100%;
	height: 54px;
	background: $sull-color-primary;
	border-radius: 27px;
	display: flex;
	align-items: center;
	justify-content: center;
	color: #ffffff;
	font-size: 18px;
	font-weight: 600;
	margin-bottom: 24px;
}

.signup-area {
	display: flex;
	justify-content: center;
	gap: 4px;
	margin-bottom: 40px;
}

.no-account-text {
	font-size: 14px;
	color: $sull-text-secondary;
}

.signup-text {
	font-size: 14px;
	color: $sull-color-primary;
	font-weight: 600;
}

.social-login {
	display: flex;
	flex-direction: column;
	align-items: center;
	gap: 24px;
}

.divider {
	display: flex;
	align-items: center;
	width: 100%;
	gap: 16px;
}

.line {
	flex: 1;
	height: 1px;
	background-color: #eee;
}

.or-text {
	font-size: 14px;
	color: $sull-text-secondary;
}

.social-icons {
	display: flex;
	gap: 20px;
	margin-bottom: 40px;
}

.social-btn {
	/* width: 50px; */
	/* Make width auto to fit text */
	height: 50px;
	background: #f8f8f8;
	border-radius: 12px;
	display: flex;
	align-items: center;
	justify-content: center;
	padding: 0 16px;
	/* Add padding */
	gap: 8px;
	/* Space between icon and text */
}

.social-icon {
	font-size: 20px;
	font-weight: bold;
}

.social-label {
	font-size: 14px;
	font-weight: 600;
	color: $sull-text-main;
}

.social-btn.wechat .social-icon {
	color: #07C160;
}

.social-btn.mobile .social-icon {
	color: #000000;
}

.social-btn.username .social-icon {
	color: #000000;
}

.login-footer {
	display: flex;
	flex-direction: column;
	align-items: center;
	margin-top: auto;
	gap: 4px;
}

.captcha-img {
	width: 120px;
	height: 50px;
	border-radius: 16px;
	background-color: #eee;
}

.footer-text {
	font-size: 12px;
	color: $sull-text-secondary;
}

.footer-link {
	color: $sull-text-main;
	font-weight: bold;
}


/* Existing Profile Styles */
.main-scroll {
	flex: 1;
	height: 0;
}

.header {
	padding: 44px 24px 20px;
	display: flex;
	justify-content: center;
	align-items: center;
}

.header-title {
	font-size: 18px;
	font-weight: bold;
	color: $sull-text-main;
}

.content {
	padding: 0 24px 120px;
}

.user-section {
	display: flex;
	flex-direction: column;
	align-items: center;
	margin-bottom: 40px;
}

.avatar-container {
	position: relative;
	margin-bottom: 16px;
}

.avatar {
	width: 100px;
	height: 100px;
	border-radius: 50%;
	background-color: #f0f0f0;
	border: 4px solid #fff;
	box-shadow: 0 0 0 1px rgba(0, 0, 0, 0.05);
}

.edit-badge {
	position: absolute;
	bottom: 0;
	right: 0;
	width: 32px;
	height: 32px;
	background: #FFD4C8;
	border-radius: 50%;
	display: flex;
	align-items: center;
	justify-content: center;
	border: 3px solid #fff;
}

.icon-edit {
	width: 16px;
	height: 16px;
	background-color: #FF6B4A;
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04c.39-.39.39-1.02 0-1.41l-2.34-2.34c-.39-.39-1.02-.39-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z'/%3E%3C/svg%3E");
	-webkit-mask-size: contain;
	-webkit-mask-repeat: no-repeat;
	-webkit-mask-position: center;
	mask-size: contain;
	mask-repeat: no-repeat;
	mask-position: center;
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04c.39-.39.39-1.02 0-1.41l-2.34-2.34c-.39-.39-1.02-.39-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z'/%3E%3C/svg%3E");
}

.user-name {
	font-size: 20px;
	font-weight: bold;
	color: $sull-text-main;
	margin-bottom: 4px;
}

.user-email {
	font-size: 14px;
	color: $sull-text-secondary;
}

.settings-section {
	display: flex;
	flex-direction: column;
}

.section-title {
	font-size: 16px;
	font-weight: bold;
	color: $sull-text-secondary;
	margin-bottom: 20px;
	opacity: 0.8;
}

.menu-list {
	display: flex;
	flex-direction: column;
}

/* Custom Refresher */
.custom-refresher {
	height: 60px;
	width: 100%;
	display: flex;
	align-items: center;
	justify-content: center;
}

.refresher-content {
	display: flex;
	flex-direction: column;
	align-items: center;
	gap: 4px;
}

.refresher-icon {
	width: 32px;
	height: 32px;
	background-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='%23409EFF'%3E%3Cpath d='M19 14h-1V8h-3V6h-2v2h-2V6H9v2H7v6H6c-1.1 0-2 .9-2 2v3c0 1.1.9 2 2 2h1c0 1.66 1.34 3 3 3s3-1.34 3-3h4c0 1.66 1.34 3 3 3s3-1.34 3-3h1c1.1 0 2-.9 2-2v-3c0-1.1-.9-2-2-2zM8 19c-.55 0-1-.45-1-1s.45-1 1-1 1 .45 1 1-.45 1-1 1zm10 0c-.55 0-1-.45-1-1s.45-1 1-1 1 .45 1 1-.45 1-1 1zM8 8h2v2H8V8zm0 4h2v2H8v-2zm4 4h-2v-2h2v2zm0-4h-2V8h2v4zm4 4h-2v-2h2v2zm0-4h-2V8h2v4z'/%3E%3Cpath d='M4 8h2v2H4zm0 4h2v2H4z' fill-opacity='.3'/%3E%3C/svg%3E");
	background-size: contain;
	background-repeat: no-repeat;
	transition: transform 0.3s ease;
}

.refresher-icon.animating {
	animation: ambulance-bounce 0.8s infinite ease-in-out;
}

.refresher-text {
	font-size: 10px;
	color: $sull-color-primary;
	opacity: 0.8;
}

@keyframes ambulance-bounce {

	0%,
	100% {
		transform: translateY(0);
	}

	50% {
		transform: translateY(-6px);
	}
}

.menu-item {
	display: flex;
	justify-content: space-between;
	align-items: center;
	padding: 16px 0;
	position: relative;
}

.menu-item::after {
	content: '';
	position: absolute;
	bottom: 0;
	left: 40px;
	right: 0;
	height: 1px;
	background-color: rgba(0, 0, 0, 0.06);
}

.menu-item:last-child::after {
	display: none;
}

.item-left {
	display: flex;
	align-items: center;
	gap: 16px;
}

.menu-icon {
	width: 24px;
	height: 24px;
	background-color: $sull-text-main;
	-webkit-mask-size: contain;
	-webkit-mask-repeat: no-repeat;
	-webkit-mask-position: center;
	mask-size: contain;
	mask-repeat: no-repeat;
	mask-position: center;
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M8.59 16.59L13.17 12 8.59 7.41 10 6l6 6-6 6-1.41-1.41z'/%3E%3C/svg%3E");
}

.menu-label {
	font-size: 16px;
	font-weight: 500;
	color: $sull-text-main;
}

.icon-arrow {
	width: 20px;
	height: 20px;
	background-color: #C4C4C4;
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M8.59 16.59L13.17 12 8.59 7.41 10 6l6 6-6 6-1.41-1.41z'/%3E%3C/svg%3E");
	-webkit-mask-size: contain;
	-webkit-mask-repeat: no-repeat;
	-webkit-mask-position: center;
	mask-size: contain;
	mask-repeat: no-repeat;
	mask-position: center;
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M8.59 16.59L13.17 12 8.59 7.41 10 6l6 6-6 6-1.41-1.41z'/%3E%3C/svg%3E");
}

/* Icons */
.icon-cart {
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M7 18c-1.1 0-1.99.9-1.99 2S5.9 22 7 22s2-.9 2-2-.9-2-2-2zM1 2v2h2l3.6 7.59-1.35 2.45c-.16.28-.25.61-.25.96 0 1.1.9 2 2 2h12v-2H7.42c-.14 0-.25-.11-.25-.25l.03-.12.9-1.63h7.45c.75 0 1.41-.41 1.75-1.03l3.58-6.49c.08-.14.12-.31.12-.48 0-.55-.45-1-1-1H5.21l-.94-2H1zm16 16c-1.1 0-1.99.9-1.99 2s.89 2 1.99 2 2-.9 2-2-.9-2-2-2z'/%3E%3C/svg%3E");
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M7 18c-1.1 0-1.99.9-1.99 2S5.9 22 7 22s2-.9 2-2-.9-2-2-2zM1 2v2h2l3.6 7.59-1.35 2.45c-.16.28-.25.61-.25.96 0 1.1.9 2 2 2h12v-2H7.42c-.14 0-.25-.11-.25-.25l.03-.12.9-1.63h7.45c.75 0 1.41-.41 1.75-1.03l3.58-6.49c.08-.14.12-.31.12-.48 0-.55-.45-1-1-1H5.21l-.94-2H1zm16 16c-1.1 0-1.99.9-1.99 2s.89 2 1.99 2 2-.9 2-2-.9-2-2-2z'/%3E%3C/svg%3E");
}

.icon-card {
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M20 4H4c-1.11 0-1.99.89-1.99 2L2 18c0 1.11.89 2 2 2h16c1.11 0 2-.89 2-2V6c0-1.11-.89-2-2-2zm0 14H4v-6h16v6zm0-10H4V6h16v2z'/%3E%3C/svg%3E");
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M20 4H4c-1.11 0-1.99.89-1.99 2L2 18c0 1.11.89 2 2 2h16c1.11 0 2-.89 2-2V6c0-1.11-.89-2-2-2zm0 14H4v-6h16v6zm0-10H4V6h16v2z'/%3E%3C/svg%3E");
}

/* Icons */
.icon-org {
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M12 7V3H2v18h20V7H12zM6 19H4v-2h2v2zm0-4H4v-2h2v2zm0-4H4V9h2v2zm0-4H4V5h2v2zm4 12H8v-2h2v2zm0-4H8v-2h2v2zm0-4H8V9h2v2zm0-4H8V5h2v2zm10 12h-8v-2h2v-2h-2v-2h2v-2h-2V9h8v10zm-2-8h-2v2h2v-2zm0 4h-2v2h2v-2z'/%3E%3C/svg%3E");
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M12 7V3H2v18h20V7H12zM6 19H4v-2h2v2zm0-4H4v-2h2v2zm0-4H4V9h2v2zm0-4H4V5h2v2zm4 12H8v-2h2v2zm0-4H8v-2h2v2zm0-4H8V9h2v2zm0-4H8V5h2v2zm10 12h-8v-2h2v-2h-2v-2h2v-2h-2V9h8v10zm-2-8h-2v2h2v-2zm0 4h-2v2h2v-2z'/%3E%3C/svg%3E");
}

.icon-dept {
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M16 11c1.66 0 2.99-1.34 2.99-3S17.66 5 16 5c-1.66 0-3 1.34-3 3s1.34 3 3 3zm-8 0c1.66 0 2.99-1.34 2.99-3S9.66 5 8 5C6.34 5 5 6.34 5 8s1.34 3 3 3zm0 2c-2.33 0-7 1.17-7 3.5V19h14v-2.5c0-2.33-4.67-3.5-7-3.5zm8 0c-.29 0-.62.02-.97.05 1.16.84 1.97 1.97 1.97 3.45V19h6v-2.5c0-2.33-4.67-3.5-7-3.5z'/%3E%3C/svg%3E");
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M16 11c1.66 0 2.99-1.34 2.99-3S17.66 5 16 5c-1.66 0-3 1.34-3 3s1.34 3 3 3zm-8 0c1.66 0 2.99-1.34 2.99-3S9.66 5 8 5C6.34 5 5 6.34 5 8s1.34 3 3 3zm0 2c-2.33 0-7 1.17-7 3.5V19h14v-2.5c0-2.33-4.67-3.5-7-3.5zm8 0c-.29 0-.62.02-.97.05 1.16.84 1.97 1.97 1.97 3.45V19h6v-2.5c0-2.33-4.67-3.5-7-3.5z'/%3E%3C/svg%3E");
}

.icon-user-info {
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z'/%3E%3C/svg%3E");
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z'/%3E%3C/svg%3E");
}

.icon-bell {
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M12 22c1.1 0 2-.9 2-2h-4c0 1.1.9 2 2 2zm6-6v-5c0-3.07-1.63-5.64-4.5-6.32V4c0-.83-.67-1.5-1.5-1.5s-1.5.67-1.5 1.5v.68C7.64 5.36 6 7.92 6 11v5l-2 2v1h16v-1l-2-2zm-2 1H8v-6c0-2.48 1.51-4.5 4-4.5s4 2.02 4 4.5v6z'/%3E%3C/svg%3E");
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M12 22c1.1 0 2-.9 2-2h-4c0 1.1.9 2 2 2zm6-6v-5c0-3.07-1.63-5.64-4.5-6.32V4c0-.83-.67-1.5-1.5-1.5s-1.5.67-1.5 1.5v.68C7.64 5.36 6 7.92 6 11v5l-2 2v1h16v-1l-2-2zm-2 1H8v-6c0-2.48 1.51-4.5 4-4.5s4 2.02 4 4.5v6z'/%3E%3C/svg%3E");
}

.icon-globe {
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M11.99 2C6.47 2 2 6.48 2 12s4.47 10 9.99 10C17.52 22 22 17.52 22 12S17.52 2 11.99 2zm6.93 6h-2.95c-.32-1.25-.78-2.45-1.38-3.56 1.84.63 3.37 1.91 4.33 3.56zM12 4.04c.83 1.2 1.48 2.53 1.91 3.96h-3.82c.43-1.43 1.08-2.76 1.91-3.96zM4.26 14C4.1 13.36 4 12.69 4 12s.1-1.36.26-2h3.38c-.08.66-.14 1.32-.14 2 0 .68.06 1.34.14 2H4.26zm.82 2h2.95c.32 1.25.78 2.45 1.38 3.56-1.84-.63-3.37-1.91-4.33-3.56zm2.95-8H5.08c.96-1.65 2.49-2.93 4.33-3.56C8.81 5.55 8.35 6.75 8.03 8zM12 19.96c-.83-1.2-1.48-2.53-1.91-3.96h3.82c-.43 1.43-1.08 2.76-1.91 3.96zM14.34 14H9.66c-.09-.66-.16-1.32-.16-2 0-.68.07-1.35.16-2h4.68c.09.65.16 1.32.16 2 0 .68-.07 1.34-.16 2zm.25 5.56c.6-1.11 1.06-2.31 1.38-3.56h2.95c-.96 1.65-2.49 2.93-4.33 3.56zM16.36 14c.08-.66.14-1.32.14-2 0-.68-.06-1.34-.14-2h3.38c.16.64.26 1.31.26 2s-.1 1.36-.26 2h-3.38z'/%3E%3C/svg%3E");
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M11.99 2C6.47 2 2 6.48 2 12s4.47 10 9.99 10C17.52 22 22 17.52 22 12S17.52 2 11.99 2zm6.93 6h-2.95c-.32-1.25-.78-2.45-1.38-3.56 1.84.63 3.37 1.91 4.33 3.56zM12 4.04c.83 1.2 1.48 2.53 1.91 3.96h-3.82c.43-1.43 1.08-2.76 1.91-3.96zM4.26 14C4.1 13.36 4 12.69 4 12s.1-1.36.26-2h3.38c-.08.66-.14 1.32-.14 2 0 .68.06 1.34.14 2H4.26zm.82 2h2.95c.32 1.25.78 2.45 1.38 3.56-1.84-.63-3.37-1.91-4.33-3.56zm2.95-8H5.08c.96-1.65 2.49-2.93 4.33-3.56C8.81 5.55 8.35 6.75 8.03 8zM12 19.96c-.83-1.2-1.48-2.53-1.91-3.96h3.82c-.43 1.43-1.08 2.76-1.91 3.96zM14.34 14H9.66c-.09-.66-.16-1.32-.16-2 0-.68.07-1.35.16-2h4.68c.09.65.16 1.32.16 2 0 .68-.07 1.34-.16 2zm.25 5.56c.6-1.11 1.06-2.31 1.38-3.56h2.95c-.96 1.65-2.49 2.93-4.33 3.56zM16.36 14c.08-.66.14-1.32.14-2 0-.68-.06-1.34-.14-2h3.38c.16.64.26 1.31.26 2s-.1 1.36-.26 2h-3.38z'/%3E%3C/svg%3E");
}

.icon-moon {
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M9 21c-4.97 0-9-4.03-9-9s4.03-9 9-9c.25 0 .5.01.74.04-.42.71-.67 1.53-.67 2.41 0 2.76 2.24 5 5 5 .88 0 1.7-.25 2.41-.67.03.24.04.49.04.74 0 4.97-4.03 9-9 9z'/%3E%3C/svg%3E");
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M9 21c-4.97 0-9-4.03-9-9s4.03-9 9-9c.25 0 .5.01.74.04-.42.71-.67 1.53-.67 2.41 0 2.76 2.24 5 5 5 .88 0 1.7-.25 2.41-.67.03.24.04.49.04.74 0 4.97-4.03 9-9 9z'/%3E%3C/svg%3E");
}

.icon-info {
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-6h2v6zm0-8h-2V7h2v2z'/%3E%3C/svg%3E");
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-6h2v6zm0-8h-2V7h2v2z'/%3E%3C/svg%3E");
}


.icon-logout {
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M17 7l-1.41 1.41L18.17 11H8v2h10.17l-2.58 2.58L17 17l5-5zM4 5h8V3H4c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h8v-2H4V5z'/%3E%3C/svg%3E");
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M17 7l-1.41 1.41L18.17 11H8v2h10.17l-2.58 2.58L17 17l5-5zM4 5h8V3H4c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h8v-2H4V5z'/%3E%3C/svg%3E");
}

.logout-item .menu-label {
	color: $sull-text-placeholder;
}

.logout-item .menu-icon {
	background-color: $sull-text-placeholder;
}
</style>

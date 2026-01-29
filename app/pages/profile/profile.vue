<template>
	<view class="container">
		<!-- Login View -->
		<view v-if="!isLoggedIn" class="login-container">
			<view class="login-header" :style="{ paddingTop: (statusBarHeight * 2 + 80) + 'rpx' }">
				<view class="login-title-section">
					<text class="login-title">{{ $t('login.title') }}</text>
					<text class="login-subtitle">{{ $t('login.subtitle') }}</text>
				</view>
			</view>

			<view class="login-form">
				<!-- Username Login Mode -->
				<template v-if="loginMethod === 'username'">
					<view class="form-group" style="position: relative; z-index: 20;">
						<text class="label">{{ $t('login.username_label') }}</text>
						<view class="input-wrapper">
							<input class="input" type="text" v-model="username"
								:placeholder="$t('login.username_placeholder')" placeholder-class="placeholder" />
							<view class="history-icon" @click="toggleHistory" v-if="loginHistory.length > 0">
								<view class="icon-arrow-down" :class="{ 'rotated': showHistory }"></view>
							</view>
						</view>
						<view class="history-dropdown" v-if="showHistory && loginHistory.length > 0">
							<view class="history-item" v-for="(item, index) in loginHistory" :key="index"
								@click="selectHistoryUser(item)">
								<text>{{ item }}</text>
								<view class="delete-icon" @click.stop="removeHistoryUser(index)">✕</view>
							</view>
						</view>
					</view>
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
					<view class="form-group">
						<text class="label">{{ $t('login.phone_label') }}</text>
						<input class="input" type="number" v-model="phone" :placeholder="$t('login.phone_placeholder')"
							placeholder-class="placeholder" />
					</view>
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

				<view class="form-actions" style="justify-content: flex-end;">
					<text class="forgot-text">{{ $t('login.forgot_password') }}</text>
				</view>

				<button class="btn-signin" @click="handleLogin">{{ $t('login.sign_in') }}</button>

				<view class="signup-area">
					<text class="no-account-text">{{ $t('login.no_account') }}</text>
					<text class="signup-text" @click="navToRegister">{{ $t('login.sign_up') }}</text>
				</view>

				<view class="login-footer">
					<text class="footer-text">{{ $t('login.terms_agree') }} <text class="footer-link">{{
						$t('login.terms')
					}}</text></text>
					<text class="footer-text">{{ $t('login.and') }} <text class="footer-link">{{ $t('login.conditions')
					}}</text></text>
				</view>
			</view>
		</view>

		<!-- Profile View -->
		<scroll-view v-else scroll-y class="main-scroll" refresher-enabled="true" :refresher-triggered="isRefreshing"
			:refresher-threshold="60" @refresherrefresh="onRefresh" @refresherpulling="onPulling"
			@refresherrestore="onRestore" @refresherabort="onRestore" refresher-default-style="none"
			refresher-background="#f8f9fb">

			<CustomRefresher slot="refresher" :isRefreshing="isRefreshing" :isPulling="isPulling" />

			<!-- Premium Purple Header -->
			<view class="premium-header-bg" :style="{ paddingTop: statusBarHeight * 2 + 'rpx' }">
				<view class="header-content">
					<view class="premium-user-section">
						<view class="avatar-box" @click="handleAvatarClick">
							<image class="avatar" :src="avatarUrl" mode="aspectFill" @error="handleAvatarError"></image>
						</view>
						<view class="user-info-text">
							<text class="user-name">{{ userInfo.userTrueName || $t('profile.please_login') }}</text>
							<text class="user-id">{{ userOrgText }}</text>
						</view>
						<view class="user-score-box">
							<text class="user-role">{{ (userOrgInfo.User && userOrgInfo.User.Title) || userOrgInfo.Title
								||
								$t('profile.title_placeholder') }}</text>
						</view>
					</view>

					<view v-if="showInviteBanner" class="invite-banner">
						<text class="invite-dot">○</text>
						<text class="invite-text">你知道抢救车的“五定”吗？</text>
						<view class="icon-close-small" @click="showInviteBanner = false"></view>
					</view>
				</view>
				<view class="header-curve"></view>
			</view>

			<view class="main-content-wrapper">
				<!-- Dual Asset Cards -->
				<view class="asset-cards-row">
					<view class="asset-card" @click="navToOrgInfo">
						<view class="asset-info">
							<text class="asset-title text-primary">{{ orgBtnText }}</text>
							<view class="asset-count-box">
								<text class="asset-number">{{ userOrgInfo.Organization &&
									userOrgInfo.Organization.DepartmentCounter || 0 }}</text>
								<text class="asset-unit">{{ $t('profile.asset_unit_dept') }}</text>
							</view>
						</view>
						<text class="asset-emoji">🏢</text>
					</view>
					<view class="asset-card" @click="navToDeptInfo">
						<view class="asset-info">
							<text class="asset-title text-warning">{{ deptBtnText }}</text>
							<view class="asset-count-box">
								<text class="asset-number">{{ userOrgInfo.Department &&
									userOrgInfo.Department.UserCounter ||
									0 }}</text>
								<text class="asset-unit">{{ $t('profile.asset_unit_member') }}</text>
							</view>
						</view>
						<text class="asset-emoji">👥</text>
					</view>
				</view>

				<!-- Quick Services Row -->
				<!-- <view class="quick-services-card">
					<view class="service-item" @click="navToOrgInfo">
						<view class="service-icon">🎮</view>
						<text class="service-label">游戏记录</text>
					</view>
					<view class="service-item">
						<view class="service-icon">🎫</view>
						<text class="service-label">积分兑换</text>
					</view>
					<view class="service-item">
						<view class="service-icon"></view>
						<text class="service-label">收货地址</text>
					</view>
				</view> -->

				<!-- Vertical Menu List -->
				<view class="menu-list-card">
					<view class="menu-item" @click="navToPersonalInfo">
						<view class="menu-item-left">
							<image class="menu-icon" src="/static/imgs/person.png" mode="aspectFit"></image>
							<text class="menu-label">{{ $t('profile.personal_info') }}</text>
						</view>
						<text class="menu-arrow">›</text>
					</view>
					<view class="menu-item" @click="navToNotification">
						<view class="menu-item-left">
							<image class="menu-icon" src="/static/imgs/notify.png" mode="aspectFit"></image>
							<text class="menu-label">{{ $t('profile.notification') }}</text>
							<view v-if="unreadCount > 0" class="unread-badge">{{ unreadCount }}</view>
						</view>
						<text class="menu-arrow">›</text>
					</view>
					<view class="menu-item" @click="navToLanguage">
						<view class="menu-item-left">
							<image class="menu-icon" src="/static/imgs/language.png" mode="aspectFit"></image>
							<text class="menu-label">{{ $t('profile.languages') }}</text>
						</view>
						<text class="menu-arrow">›</text>
					</view>
					<view class="menu-item">
						<view class="menu-item-left">
							<image class="menu-icon" src="/static/imgs/help.png" mode="aspectFit"></image>
							<text class="menu-label">{{ $t('profile.help') }}</text>
						</view>
						<text class="menu-arrow">›</text>
					</view>
					<!-- Config Manage -->
					<view class="menu-item" @click="navToConfigManage">
						<view class="menu-item-left">
							<image class="menu-icon" src="/static/imgs/config.png" mode="aspectFit"></image>
							<text class="menu-label">{{ $t('profile.config_manage') }}</text>
						</view>
						<text class="menu-arrow">›</text>
					</view>
					<view class="menu-item logout-item" @click="handleLogout">
						<view class="menu-item-left">
							<image class="menu-icon" src="/static/imgs/logout.png" mode="aspectFit"></image>
							<text class="menu-label">{{ $t('profile.logout') }}</text>
						</view>
					</view>

				</view>

				<FooterInfo />
				<view class="bottom-spacer"></view>
			</view>
		</scroll-view>

		<!-- Bottom Navigation -->
		<BottomNav v-if="isLoggedIn" activePage="profile" />

		<!-- Confirm Modal -->
		<ConfirmModal v-model:visible="confirmModal.visible" :title="confirmModal.title" :message="confirmModal.message"
			:confirmText="confirmModal.confirmText" :cancelText="confirmModal.cancelText"
			:confirmColor="confirmModal.confirmColor" :cancelColor="confirmModal.cancelColor"
			@confirm="handleConfirmAction" @cancel="handleCancelAction" />

		<!-- Warning Modal -->
		<WarningModal v-model:visible="warningModal.visible" :title="warningModal.title" :message="warningModal.message"
			:buttonText="warningModal.buttonText" />

		<!-- Success Modal -->
		<SuccessModal v-model:visible="successModal.visible" :title="successModal.title" :message="successModal.message"
			:buttonText="successModal.buttonText" />
	</view>
</template>

<script>
import BottomNav from '@/components/BottomNav/BottomNav.vue';
import ConfirmModal from '@/components/ConfirmModal/ConfirmModal.vue';
import WarningModal from '@/components/WarningModal/WarningModal.vue';
import FooterInfo from '@/components/FooterInfo/FooterInfo.vue';
import request from '@/utils/request.js';
import { storage } from '@/utils/storage.js';
import SuccessModal from '@/components/SuccessModal/SuccessModal.vue';
import CustomRefresher from '@/components/CustomRefresher/CustomRefresher.vue';

export default {
	components: {
		BottomNav,
		ConfirmModal,
		WarningModal,
		FooterInfo,
		SuccessModal,
		CustomRefresher
	},
	data() {
		return {
			statusBarHeight: 20,
			isLoggedIn: false,
			isDarkMode: false,
			loginMethod: 'username',
			username: '',
			password: '',
			showPassword: false,
			verificationCode: '',
			captchaBase64: '',
			uuid: '',
			phone: '',
			code: '',
			countdown: 0,
			userInfo: {},
			userOrgInfo: {},
			isRefreshing: false,
			isPulling: false,
			showInviteBanner: true,
			// Login History
			loginHistory: [],
			showHistory: false,
			avatarError: false,
			unreadCount: 0,
			confirmModal: {
				visible: false,
				title: '',
				message: '',
				confirmText: '',
				cancelText: '',
				confirmColor: '#92C5F7',
				cancelColor: '#FF6B6B',
				actionType: ''
			},
			warningModal: {
				visible: false,
				title: '',
				message: '',
				buttonText: ''
			},
			successModal: {
				visible: false,
				title: '',
				message: '',
				buttonText: ''
			}
		}
	},
	onBackPress(options) {
		if (options.from === 'backbutton') {
			uni.reLaunch({
				url: '/pages/index/index'
			});
			return true;
		}
		return false;
	},
	onLoad() {
		const sysInfo = uni.getSystemInfoSync();
		this.statusBarHeight = sysInfo.statusBarHeight || 20;

		// Ensure status bar text is white for dark headers
		// #ifndef H5
		uni.setNavigationBarColor({
			frontColor: '#ffffff',
			backgroundColor: '#009A97',
			animation: { duration: 400, timingFunc: 'easeIn' }
		});
		// #endif

		// Load Login History
		try {
			const history = storage.getLoginHistory();
			if (history && Array.isArray(history)) {
				this.loginHistory = history;
				// Pre-fill latest
				if (history.length > 0) {
					this.username = history[0];
				}
			}
		} catch (e) { }

		uni.$on('refreshUserOrgInfo', () => {
			if (this.isLoggedIn) {
				this.getUserOrganization();
			}
		});
	},
	onUnload() {
		uni.$off('refreshUserOrgInfo');
	},
	onShow() {
		// #ifdef APP-PLUS
		if (uni.getSystemInfoSync().platform === 'android') {
			plus.navigator.setStatusBarBackground('transparent');
		}
		// #endif

		this.checkLogin();
		if (!this.isLoggedIn) {
			this.getCaptcha();
		} else {
			this.fetchUserInfo();
			// Check for mandatory updates if not checked yet
			// #ifdef APP-PLUS
			getApp().checkUpdate();
			// #endif
		}
	},
	watch: {
		userInfo() {
			this.avatarError = false;
		}
	},
	computed: {
		avatarUrl() {
			if (this.avatarError) return '/static/avatar_placeholder.png';
			const url = this.userInfo.headImageUrl || this.userInfo.HeadImageUrl;
			if (url) {
				if (url.startsWith('http')) return url;
				return request.config.RESOURCE_URL + url;
			}
			return '/static/avatar_placeholder.png';
		},
		orgBtnText() {
			return (this.userOrgInfo && this.userOrgInfo.Organization && this.userOrgInfo.Organization.Name)
				? this.$t('profile.asset_org_info')
				: this.$t('profile.org_placeholder');
		},
		deptBtnText() {
			return (this.userOrgInfo && this.userOrgInfo.Department && this.userOrgInfo.Department.Name)
				? this.$t('profile.asset_dept_manage')
				: this.$t('profile.dept_placeholder');
		},
		userOrgText() {
			const dept = this.userOrgInfo.Department && this.userOrgInfo.Department.Name;
			const org = this.userOrgInfo.Organization && this.userOrgInfo.Organization.Name;

			if (dept) {
				return dept;
			} else if (org) {
				return org;
			} else {
				return this.$t('profile.org_placeholder');
			}
		}
	},
	methods: {
		checkLogin() {
			const token = storage.getToken();
			this.isLoggedIn = !!token;
			if (this.isLoggedIn) {
				this.userInfo = storage.get('userInfo') || {};
				// Recover org info too if needed
				this.userOrgInfo = storage.get('userOrgInfo') || {};
				this.updateUnreadCount();
			}
		},
		updateUnreadCount() {
			const list = storage.getNotifications();
			this.unreadCount = list.filter(i => !i.isRead).length;
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
				uni.showToast({ title: this.$t('login.phone_placeholder'), icon: 'none' });
				return;
			}
			uni.showToast({ title: 'Code sent: 1234', icon: 'none' });
			this.countdown = 60;
			const timer = setInterval(() => {
				this.countdown--;
				if (this.countdown <= 0) clearInterval(timer);
			}, 1000);
		},
		getCaptcha() {
			request.get('api/User/getVerificationCode').then(res => {
				if (res.status && res.data) {
					this.captchaBase64 = 'data:image/png;base64,' + res.data.img;
					this.uuid = res.data.uuid;
				} else {
					const imgData = res.img || res.data?.img;
					if (imgData) this.captchaBase64 = 'data:image/png;base64,' + imgData;
					this.uuid = res.uuid || res.data?.uuid;
				}
			}).catch(err => console.error(err));
		},
		handleAvatarError() {
			this.avatarError = true;
		},
		handleLogin() {
			if (this.loginMethod === 'username') {
				if (!this.username || !this.password || !this.verificationCode) {
					this.warningModal = {
						visible: true,
						title: this.$t('modal.warning_title'),
						message: this.$t('login.required_tip'),
						buttonText: this.$t('modal.confirm_btn')
					};
					return;
				}
				const loginData = { userName: this.username, password: this.password, verificationCode: this.verificationCode, UUID: this.uuid };
				request.post('api/user/login', loginData).then(res => {
					if (res.status) {
						const tempToken = res.token || res.data?.token || 'mock-token';

						// 1. Fetch User Info using temp token to get ID
						request.post('api/user/GetCurrentUserInfo', {}, { 'Authorization': `Bearer ${tempToken}` })
							.then(infoRes => {
								if (infoRes.status && infoRes.data) {

									// Unified Login Storage
									storage.login(tempToken, infoRes.data);

									this.userInfo = infoRes.data;
									this.isLoggedIn = true;
									this.getUserOrganization(); // Will safe org info to storage

									// Save History
									this.loginHistory = storage.addLoginHistory(this.username);

									this.successModal = {
										visible: true,
										title: this.$t('login.signin_success'),
										message: '',
										buttonText: this.$t('modal.continue')
									};

									// Register Push Account (CID) to backend
									this.registerPushAccount();

									// Guide permission after successful login (first time)
									setTimeout(() => {
										// #ifdef APP-PLUS
										getApp().checkUpdate();
										// #endif
										this.checkAndGuideNotification();
									}, 1500);
								} else {
									this.warningModal = {
										visible: true,
										title: this.$t('login.signin_fail'),
										message: this.$t('common.load_fail'),
										buttonText: this.$t('common.confirm')
									};
								}
							});
					} else {
						this.warningModal = {
							visible: true,
							title: this.$t('login.signin_fail'),
							message: res.message || this.$t('login.signin_fail'),
							buttonText: this.$t('modal.confirm_btn')
						};
						this.getCaptcha();
					}
				}).catch(err => {
					console.error(err);
					this.getCaptcha();
					this.warningModal = {
						visible: true,
						title: this.$t('login.signin_fail'),
						message: err.message || 'Network Error',
						buttonText: this.$t('modal.confirm_btn')
					};
				});
			} else {
				if (!this.phone || !this.code) {
					uni.showToast({ title: this.$t('login.code_placeholder'), icon: 'none' });
					return;
				}
				storage.set('token', 'mock-token-phone');
				this.isLoggedIn = true;
				this.registerPushAccount();
				this.successModal = {
					visible: true,
					title: this.$t('login.signin_success'),
					message: '',
					buttonText: this.$t('modal.continue')
				};
			}
		},
		handleLogout() {
			this.confirmModal.title = this.$t('profile.logout_confirm_title');
			this.confirmModal.message = this.$t('profile.logout_confirm_msg');
			this.confirmModal.confirmText = this.$t('common.confirm');
			this.confirmModal.cancelText = this.$t('common.cancel');
			this.confirmModal.confirmColor = '#FF6B6B';
			this.confirmModal.cancelColor = '#999';
			this.confirmModal.actionType = 'logout';
			this.confirmModal.visible = true;
		},
		executeLogout() {
			storage.remove('token');
			storage.logout();
			uni.reLaunch({
				url: '/pages/profile/profile'
			});
		},

		navToRegister() { uni.navigateTo({ url: '/pages_user/register/register' }); },
		navToOrgInfo() { uni.navigateTo({ url: '/pages_user/organization/organization' }); },
		navToDeptInfo() {
			if (!this.userOrgInfo.Organization || !this.userOrgInfo.Organization.Id) {
				this.warningModal = {
					visible: true,
					title: this.$t('common.tip'),
					message: this.$t('profile.please_join_org_first'),
					buttonText: this.$t('common.confirm')
				};
				return;
			}
			uni.navigateTo({ url: '/pages_user/department/department' });
		},
		navToLanguage() { uni.navigateTo({ url: '/pages_user/profile/language' }); },
		navToPersonalInfo() { uni.navigateTo({ url: '/pages_user/profile/personal-info' }); },
		navToConfigManage() { uni.navigateTo({ url: '/pages_user/profile/config-manage' }); },
		navToNotification() {
			uni.navigateTo({
				url: '/pages_user/profile/notification-list',
				events: {
					refreshUnread: () => {
						this.updateUnreadCount();
					}
				}
			});
		},
		handleAvatarClick() {
			uni.chooseImage({
				count: 1,
				sizeType: ['compressed'],
				success: (res) => {
					let fileName = '';
					const filePath = res.tempFilePaths[0];

					// #ifdef H5
					fileName = res.tempFiles[0].name;
					// #endif

					// #ifndef H5
					// Extract filename from path for App/MP
					fileName = filePath.substring(filePath.lastIndexOf('/') + 1);
					// #endif

					if (!fileName) {
						uni.showToast({ title: this.$t('common.file_name_error'), icon: 'none' });
						return;
					}
					this.uploadAvatar(filePath, fileName);
				}
			});
		},
		uploadAvatar(filePath, fileName) {
			const token = storage.getToken();
			uni.uploadFile({
				url: request.config.BASE_URL + 'api/sys_user/upload',
				filePath: filePath,
				name: 'fileInput',
				header: { 'Authorization': token ? `Bearer ${token}` : '' },
				success: (uploadRes) => {
					let data;
					try { data = JSON.parse(uploadRes.data); } catch (e) { data = uploadRes.data; }
					if (uploadRes.statusCode === 200 && (data.status || data.success)) {
						const newPath = data.data + fileName;
						const updateData = { ...this.userInfo, img: request.config.RESOURCE_URL + this.userInfo.headImageUrl, headImageUrl: newPath };
						request.post('api/user/updateUserInfo', updateData).then(updateRes => {
							uni.showToast({ title: 'Upload Success', icon: 'success' });
							this.fetchUserInfo();
						}).catch(err => console.error(err));
					}
				}
			});
		},
		fetchUserInfo() {
			return request.post('api/user/GetCurrentUserInfo').then(res => {
				if (res.status) {
					storage.set('userInfo', res.data);
					this.userInfo = res.data;
					this.getUserOrganization();
				}
			}).catch(err => console.error(err));
		},
		getUserOrganization() {
			request.get('api/ssbh/OrganizationUser/UserOrganization').then(res => {
				if (res) {
					storage.set('userOrgInfo', res);
					this.userOrgInfo = res;
					if (!res.Organization || !res.Organization.Id) this.showOrgMissingModal();
				}
			}).catch(err => console.error(err));
		},
		showOrgMissingModal() {
			this.confirmModal = { visible: true, title: this.$t('profile.org_required_title'), message: this.$t('profile.org_required_msg'), confirmText: this.$t('profile.join_now'), cancelText: this.$t('profile.not_now'), confirmColor: '#92C5F7', cancelColor: '#999999', actionType: 'joinOrg' };
		},
		handleConfirmAction() {
			if (this.confirmModal.actionType === 'joinOrg') {
				this.navToOrgInfo();
			} else if (this.confirmModal.actionType === 'logout') {
				this.executeLogout();
			}
		},
		handleCancelAction() { },
		onPulling() { this.isPulling = true; },
		onRestore() { this.isPulling = false; },
		onRefresh() {
			this.isRefreshing = true;
			this.isPulling = true;
			this.fetchUserInfo().finally(() => {
				setTimeout(() => {
					this.isRefreshing = false;
					this.isPulling = false;
				}, 1000);
			});
		},

		// History Logic
		toggleHistory() {
			this.showHistory = !this.showHistory;
		},
		selectHistoryUser(name) {
			this.username = name;
			this.showHistory = false;
		},
		removeHistoryUser(index) {
			this.loginHistory = storage.removeLoginHistory(index);
			if (this.loginHistory.length === 0) this.showHistory = false;
		},

		// --- Notification Permission Guide ---
		checkAndGuideNotification() {
			// #ifdef APP-PLUS
			const guided = storage.get('notification_permission_guided');
			if (guided) return;

			this.getNotificationStatus((isEnabled) => {
				if (!isEnabled) {
					this.showPermissionGuide();
				} else {
					// Already enabled, mark as guided
					storage.set('notification_permission_guided', true);
				}
			});
			// #endif
		},

		getNotificationStatus(callback) {
			if (uni.getSystemInfoSync().platform == "ios") {
				let center = plus.ios.invoke('UNUserNotificationCenter', 'currentNotificationCenter');
				plus.ios.invoke(center, 'getNotificationSettingsWithCompletionHandler:', plus.ios.implements("UNNotificationSettings", {
					"getNotificationSettingsWithCompletionHandler:": function (settings) {
						let authorizationStatus = plus.ios.invoke(settings, 'authorizationStatus');
						// 3: Authorized, 2: Denied, 1: NotDetermined
						callback(authorizationStatus === 3);
					}
				}));
			} else {
				let main = plus.android.runtimeMainActivity();
				let pkName = main.getPackageName();
				let NotificationManagerCompat = plus.android.importClass("androidx.core.app.NotificationManagerCompat");
				let notificationManager = NotificationManagerCompat.from(main);
				callback(notificationManager.areNotificationsEnabled());
			}
		},
		registerPushAccount() {
			// #ifdef APP-PLUS
			uni.getPushClientId({
				success: (cidRes) => {
					if (cidRes && cidRes.cid) {
						request.post('api/ssbh/UserAccount/Add', {
							mainData: {
								Account: cidRes.cid,
								Category: 10
							}
						}).catch(err => console.error('Auto Register CID Error:', err));
					}
				}
			});
			// #endif
		},

		showPermissionGuide() {
			uni.showModal({
				title: this.$t('profile.permission_guide_title') || '开启消息通知',
				content: this.$t('profile.permission_guide_msg') || '为了保证您能及时收到推送消息（如效期警告、库存预警、异常提醒等），建议您开启系统通知权限。',
				confirmText: this.$t('profile.go_to_setting') || '去设置',
				cancelText: this.$t('profile.not_now') || '暂不开启',
				success: (res) => {
					// #ifdef APP-PLUS
					uni.getPushClientId({
						success: (cidRes) => {
							request.post('api/ssbh/UserAccount/Add', {
								mainData: {
									Account: cidRes.cid,
									Category: 10
								}
							}).then(apiRes => {
								if (apiRes.success || apiRes.status) {
									storage.set('notification_permission_guided', true);
								}
							}).catch(err => console.error('Register CID Error:', err));
						},
						fail: (err) => {
							console.error('Get CID Error:', err);
						}
					});
					// #endif

					// #ifndef APP-PLUS
					storage.set('notification_permission_guided', true);
					// #endif

					if (res.confirm) {
						this.openAppSettings();
					}
				}
			});
		},

		openAppSettings() {
			if (uni.getSystemInfoSync().platform == "ios") {
				let UIApplication = plus.ios.import("UIApplication");
				let NSURL = plus.ios.import("NSURL");
				let setting = NSURL.URLWithString("app-settings:");
				let application = UIApplication.sharedApplication();
				application.openURL(setting);
				plus.ios.deleteObject(setting);
				plus.ios.deleteObject(application);
			} else {
				let main = plus.android.runtimeMainActivity();
				let pkName = main.getPackageName();
				let Intent = plus.android.importClass('android.content.Intent');
				let Settings = plus.android.importClass('android.provider.Settings');
				let Uri = plus.android.importClass('android.net.Uri');
				let intent = new Intent();
				let Build = plus.android.importClass("android.os.Build");

				if (Build.VERSION.SDK_INT >= 26) {
					intent.setAction(Settings.ACTION_APP_NOTIFICATION_SETTINGS);
					intent.putExtra(Settings.EXTRA_APP_PACKAGE, pkName);
				} else {
					intent.setAction(Settings.ACTION_APPLICATION_DETAILS_SETTINGS);
					let uri = Uri.fromParts("package", pkName, null);
					intent.setData(uri);
				}
				main.startActivity(intent);
			}
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
	background-color: #f8f9fb;
}

.main-scroll {
	flex: 1;
	height: 0;
	width: 100%;
}

.bottom-spacer {
	height: 120rpx;
}

/* Login Styles */
.login-container {
	display: flex;
	flex-direction: column;
	height: 100vh;
	background-color: #fff;
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
	background: #fff;
	border-radius: 50%;
	display: flex;
	align-items: center;
	justify-content: center;
	margin-bottom: 48rpx;
}

.icon-arrow-left {
	width: 40rpx;
	height: 40rpx;
	background-color: $sull-color-warning;
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'%3E%3Cpath d='M20 11H7.83l5.59-5.59L12 4l-8 8 8 8 1.41-1.41L7.83 13H20v-2z'/%3E%3C/svg%3E");
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'%3E%3Cpath d='M20 11H7.83l5.59-5.59L12 4l-8 8 8 8 1.41-1.41L7.83 13H20v-2z'/%3E%3C/svg%3E");
	-webkit-mask-size: contain;
	mask-size: contain;
}

.login-title {
	font-size: 56rpx;
	font-weight: bold;
	color: #fff;
	margin-bottom: 16rpx;
	display: block;
}

.login-subtitle {
	font-size: 32rpx;
	color: rgba(255, 255, 255, 0.9);
}

.login-form {
	padding: 60rpx 48rpx;
	background: #fff;
	flex: 1;
	border-radius: 48rpx 48rpx 0 0;
	margin-top: -48rpx;
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

.input-wrapper {
	position: relative;
	display: flex;
	align-items: center;
}

.input {
	flex: 1;
	height: 100rpx;
	background: #F6F8FA;
	border-radius: 32rpx;
	padding: 0 32rpx;
	font-size: 28rpx;
	color: $sull-text-main;
	box-sizing: border-box;
}

.placeholder {
	color: $sull-text-placeholder;
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

.code-input-row {
	display: flex;
	align-items: center;
	gap: 20rpx;
}

.captcha-img {
	width: 240rpx;
	height: 100rpx;
	border-radius: 24rpx;
	background-color: #f0f0f0;
}

.code-btn {
	width: 240rpx;
	height: 100rpx;
	background: rgba($sull-color-primary, 0.1);
	border-radius: 24rpx;
	display: flex;
	align-items: center;
	justify-content: center;
}

.code-btn-text {
	color: $sull-color-primary;
	font-size: 28rpx;
	font-weight: 600;
}

.form-actions {
	display: flex;
	margin-bottom: 60rpx;
}

.forgot-text {
	font-size: 26rpx;
	color: $sull-text-secondary;
}

.signup-area {
	display: flex;
	justify-content: center;
	gap: 12rpx;
	margin-top: 40rpx;
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
	margin-top: 80rpx;
	text-align: center;
}

.footer-text {
	font-size: 24rpx;
	color: $sull-gray-60;
	line-height: 1.6;
}

.footer-link {
	color: $sull-text-main;
	font-weight: 500;
	margin: 0 4rpx;
}

.icon-eye,
.icon-eye-off {
	width: 40rpx;
	height: 40rpx;
	background-color: $sull-gray-60;
}

.icon-eye {
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'%3E%3Cpath d='M12 4.5C7 4.5 2.73 7.61 1 12c1.73 4.39 6 7.5 11 7.5s9.27-3.11 11-7.5c-1.73-4.39-6-7.5-11-7.5zM12 17c-2.76 0-5-2.24-5-5s2.24-5 5-5 5 2.24 5 5-2.24 5-5 5zm0-8c-1.66 0-3 1.34-3 3s1.34 3 3 3 3-1.34 3-3-1.34-3-3-3z'/%3E%3C/svg%3E");
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'%3E%3Cpath d='M12 4.5C7 4.5 2.73 7.61 1 12c1.73 4.39 6 7.5 11 7.5s9.27-3.11 11-7.5c-1.73-4.39-6-7.5-11-7.5zM12 17c-2.76 0-5-2.24-5-5s2.24-5 5-5 5 2.24 5 5-2.24 5-5 5zm0-8c-1.66 0-3 1.34-3 3s1.34 3 3 3 3-1.34 3-3-1.34-3-3-3z'/%3E%3C/svg%3E");
}

.icon-eye-off {
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'%3E%3Cpath d='M12 7c2.76 0 5 2.24 5 5 0 .65-.13 1.26-.36 1.82l2.92 2.92c1.51-1.26 2.7-2.89 3.44-4.74-1.73-4.39-6-7.5-11-7.5-1.4 0-2.74.25-3.98.7l2.16 2.16C10.74 7.13 11.35 7 12 7zM2 4.27l2.28 2.28.46.46C3.08 8.3 1.78 10.02 1 12c1.73 4.39 6 7.5 11 7.5 1.55 0 3.03-.3 4.38-.84l.42.42L19.73 22 21 20.73 3.27 3 2 4.27zM7.53 9.8l1.55 1.55c-.05.21-.08.43-.08.65 0 1.66 1.34 3 3 3 .22 0 .44-.03.65-.08l1.55 1.55c-.67.33-1.41.53-2.2.53-2.76 0-5-2.24-5-5 0-.79.2-1.53.53-2.2zm4.34-4.3l1.82 1.82c.11-.01.22-.02.33-.02 1.66 0 3 1.34 3 3 0 .11-.01.22-.02.33l1.82 1.82c.32-.65.51-1.37.51-2.15 0-2.76-2.24-5-5-5-.78 0-1.5.19-2.15.51z'/%3E%3C/svg%3E");
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'%3E%3Cpath d='M12 7c2.76 0 5 2.24 5 5 0 .65-.13 1.26-.36 1.82l2.92 2.92c1.51-1.26 2.7-2.89 3.44-4.74-1.73-4.39-6-7.5-11-7.5-1.4 0-2.74.25-3.98.7l2.16 2.16C10.74 7.13 11.35 7 12 7zM2 4.27l2.28 2.28.46.46C3.08 8.3 1.78 10.02 1 12c1.73 4.39 6 7.5 11 7.5 1.55 0 3.03-.3 4.38-.84l.42.42L19.73 22 21 20.73 3.27 3 2 4.27zM7.53 9.8l1.55 1.55c-.05.21-.08.43-.08.65 0 1.66 1.34 3 3 3 .22 0 .44-.03.65-.08l1.55 1.55c-.67.33-1.41.53-2.2.53-2.76 0-5-2.24-5-5 0-.79.2-1.53.53-2.2zm4.34-4.3l1.82 1.82c.11-.01.22-.02.33-.02 1.66 0 3 1.34 3 3 0 .11-.01.22-.02.33l1.82 1.82c.32-.65.51-1.37.51-2.15 0-2.76-2.24-5-5-5-.78 0-1.5.19-2.15.51z'/%3E%3C/svg%3E");
}

.btn-signin {
	width: 100%;
	height: 108rpx;
	background: $sull-color-primary;
	border-radius: 54rpx;
	display: flex;
	align-items: center;
	justify-content: center;
	color: #fff;
	font-size: 36rpx;
	font-weight: 600;
}

/* Premium Profile Styles */
.premium-header-bg {
	background: linear-gradient(135deg, $sull-color-primary 0%, lighten($sull-color-primary, 20%) 100%);
	position: relative;
	overflow: hidden;
}

.header-content {
	padding: 0 40rpx 40rpx;
	z-index: 2;
	position: relative;
}

.header-curve {
	position: absolute;
	bottom: -100rpx;
	right: -60rpx;
	width: 400rpx;
	height: 400rpx;
	background: rgba($sull-color-secondary, 0.4);
	border-radius: 50%;
	filter: blur(60rpx);
	z-index: 1;
}

.premium-user-section {
	display: flex;
	align-items: center;
	padding: 40rpx 0;
	gap: 24rpx;
}

.avatar-box {
	position: relative;
}

.avatar {
	width: 130rpx;
	height: 130rpx;
	border-radius: 50%;
	border: 4rpx solid rgba(255, 255, 255, 0.4);
}

.user-info-text {
	flex: 1;
	display: flex;
	flex-direction: column;
	gap: 4rpx;
}

.user-name {
	font-size: 40rpx;
	font-weight: bold;
	color: #fff;
}

.user-id {
	font-size: 24rpx;
	color: rgba(255, 255, 255, 0.7);
}

.user-score-box {
	background: rgba(255, 255, 255, 0.2);
	padding: 8rpx 24rpx;
	border-radius: 30rpx;
	display: flex;
	align-items: center;
	border: 1px solid rgba(255, 255, 255, 0.3);
}

.user-role {
	font-size: 24rpx;
	font-weight: 600;
	color: #fff;
}

.score-badge {
	display: flex;
	align-items: center;
	gap: 10rpx;
}

.score-icon {
	font-size: 32rpx;
}

.score-value {
	font-size: 28rpx;
	font-weight: bold;
	color: #4B5563;
}

.score-add {
	width: 44rpx;
	height: 44rpx;
	background: $sull-color-primary;
	color: #fff;
	border-radius: 50%;
	display: flex;
	align-items: center;
	justify-content: center;
	font-size: 32rpx;
	margin-left: 12rpx;
}

.invite-banner {
	display: flex;
	align-items: center;
	gap: 12rpx;
	color: #fff;
	font-size: 24rpx;
	background: rgba(0, 0, 0, 0.1);
	padding: 16rpx 24rpx;
	border-radius: 20rpx;
	margin-top: 10rpx;
}

.invite-dot {
	opacity: 0.8;
}

.invite-text {
	flex: 1;
	opacity: 0.9;
}

.icon-close-small {
	width: 32rpx;
	height: 32rpx;
	background-color: #fff;
	opacity: 0.6;
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'%3E%3Cpath d='M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z'/%3E%3C/svg%3E");
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'%3E%3Cpath d='M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z'/%3E%3C/svg%3E");
	-webkit-mask-size: contain;
	mask-size: contain;
}

.main-content-wrapper {
	padding: 0 30rpx 60rpx;
	margin-top: -30rpx;
	position: relative;
	z-index: 10;
}

.asset-cards-row {
	display: flex;
	gap: 20rpx;
	margin-bottom: 30rpx;
}

.asset-card {
	flex: 1;
	background: #fff;
	border-radius: 32rpx;
	padding: 30rpx;
	display: flex;
	justify-content: space-between;
	align-items: center;
	box-shadow: 0 8rpx 24rpx rgba(0, 0, 0, 0.05);
}

.asset-info {
	display: flex;
	flex-direction: column;
	gap: 8rpx;
}

.asset-title {
	font-size: 28rpx;
	font-weight: 600;
}

.asset-count-box {
	display: flex;
	align-items: baseline;
	gap: 6rpx;
}

.asset-number {
	font-size: 44rpx;
	font-weight: bold;
	color: #1F2937;
}

.asset-unit {
	font-size: 22rpx;
	color: #9CA3AF;
}

.asset-emoji {
	font-size: 60rpx;
	opacity: 0.8;
}

.asset-icon {
	width: 80rpx;
	height: 80rpx;
}

.text-primary {
	color: $sull-color-primary;
}

.history-icon {
	padding: 0 20rpx;
	height: 100%;
	display: flex;
	align-items: center;
	justify-content: center;
}

.icon-arrow-down {
	width: 0;
	height: 0;
	border-left: 10rpx solid transparent;
	border-right: 10rpx solid transparent;
	border-top: 10rpx solid #999;
	transition: transform 0.3s;
}

.icon-arrow-down.rotated {
	transform: rotate(180deg);
}

.history-dropdown {
	position: absolute;
	top: 100%;
	left: 0;
	right: 0;
	background: #fff;
	border-radius: 12rpx;
	box-shadow: 0 8rpx 24rpx rgba(0, 0, 0, 0.1);
	z-index: 100;
	margin-top: 8rpx;
	overflow: hidden;
	border: 1rpx solid #eee;
}

.history-item {
	display: flex;
	justify-content: space-between;
	align-items: center;
	padding: 24rpx;
	font-size: 28rpx;
	color: #333;
	border-bottom: 1rpx solid #f5f5f5;
}

.history-item:active {
	background: #f9f9f9;
}

.history-item:last-child {
	border-bottom: none;
}

.delete-icon {
	color: #ccc;
	padding: 10rpx;
	font-size: 24rpx;
}

.text-warning {
	color: $sull-color-warning;
}

.quick-services-card {
	background: #fff;
	border-radius: 32rpx;
	display: flex;
	justify-content: space-around;
	padding: 40rpx 0;
	margin-bottom: 30rpx;
	box-shadow: 0 4rpx 16rpx rgba(0, 0, 0, 0.02);
}

.service-item {
	display: flex;
	flex-direction: column;
	align-items: center;
	gap: 16rpx;
}

.service-icon {
	font-size: 50rpx;
}

.service-label {
	font-size: 26rpx;
	color: #4B5563;
	font-weight: 500;
}

.menu-list-card {
	background: #fff;
	border-radius: 32rpx;
	padding: 10rpx 30rpx;
	box-shadow: 0 4rpx 16rpx rgba(0, 0, 0, 0.02);
}

.menu-item {
	display: flex;
	justify-content: space-between;
	align-items: center;
	padding: 40rpx 0;
	border-bottom: 2rpx solid #F3F4F6;
}

.menu-item:last-child {
	border-bottom: none;
}

.menu-label-group {
	display: flex;
	align-items: center;
	gap: 16rpx;
}

.menu-item-left {
	display: flex;
	align-items: center;
	gap: 24rpx;
}

.menu-icon {
	width: 44rpx;
	height: 44rpx;
}

.menu-label {
	font-size: 30rpx;
	color: #1F2937;
	font-weight: 500;
}

.menu-tag {
	font-size: 22rpx;
	color: $sull-color-primary;
	background: rgba($sull-color-primary, 0.1);
	padding: 4rpx 12rpx;
	border-radius: 20rpx;
}

.menu-arrow {
	font-size: 40rpx;
	color: #D1D5DB;
}

.logout-item .menu-label {
	color: $sull-text-placeholder;
}

.cache-actions {
	display: flex;
	gap: 20rpx;
	padding: 0 30rpx 30rpx;
}

.btn-cache {
	flex: 1;
	font-size: 24rpx;
	height: 60rpx;
	line-height: 60rpx;
	background: #f0f2f5;
	color: #606266;
	border-radius: 30rpx;
	text-align: center;
}

/* Custom Refresher */

.unread-badge {
	background-color: #FF4D4F;
	color: #fff;
	font-size: 20rpx;
	min-width: 32rpx;
	height: 32rpx;
	border-radius: 16rpx;
	display: flex;
	align-items: center;
	justify-content: center;
	padding: 0 8rpx;
	margin-left: 12rpx;
}
</style>
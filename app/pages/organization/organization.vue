<template>
	<view class="container">
		<!-- Header -->
		<view class="header">
			<view class="back-btn-placeholder" @click="goBack">
				<view class="icon-arrow-left"></view>
			</view>
			<text class="header-title">{{ hasOrg ? (isEditing ? $t('organization.title_update') :
				$t('organization.title_info')) : $t('organization.title_create') }}</text>
			<!-- Empty view for balancing if needed, or remove if flex layout handles centering differently -->
			<!-- <view class="header-right"></view> -->
		</view>

		<scroll-view scroll-y class="content" refresher-enabled="true" :refresher-triggered="isRefreshing"
			@refresherrefresh="onRefresh" refresher-default-style="none" refresher-background="#f0f5ff">
			<view slot="refresher" class="custom-refresher">
				<view class="refresher-content">
					<view class="refresher-icon" :class="{ 'animating': isRefreshing }"></view>
					<text class="refresher-text">{{ isRefreshing ? $t('index.refreshing') : $t('index.pullToRefresh')
					}}</text>
				</view>
			</view>

			<!-- Loading State -->
			<view v-if="isLoading" class="loading-state">
				<text>Loading...</text>
			</view>

			<!-- Has Organization -->
			<view v-else-if="hasOrg" class="form-container">

				<!-- View Mode: Read Only Display -->
				<view v-if="!isEditing" class="view-mode">
					<view class="info-card">
						<view class="info-row">
							<text class="info-label">{{ $t('organization.name') }}</text>
							<text class="info-value">{{ orgInfo.Name }}</text>
						</view>
						<view class="info-row">
							<text class="info-label">{{ $t('organization.level') }}</text>
							<text class="info-value">{{ orgInfo.Level }}</text>
						</view>
						<view class="info-row">
							<text class="info-label">{{ $t('organization.invite_code') }}</text>
							<view class="value-with-copy">
								<text class="info-value">{{ orgInfo.Id }}</text>
								<view class="icon-copy" v-if="orgInfo.Id" @click="copyToClipboard(orgInfo.Id)"></view>
							</view>
						</view>
						<view class="info-row">
							<text class="info-label">{{ $t('organization.status') }}</text>
							<view class="status-badge-small" :class="getStatusClass(orgInfo.AuditStatus)">
								<text>{{ getStatusText(orgInfo.AuditStatus) }}</text>
							</view>
						</view>
						<view class="info-row">
							<text class="info-label">{{ $t('organization.creator') }}</text>
							<text class="info-value">{{ orgInfo.Creator }}</text>
						</view>
						<view class="info-row">
							<text class="info-label">{{ $t('organization.create_time') }}</text>
							<text class="info-value">{{ orgInfo.CreationTime }}</text>
						</view>
						<view class="info-row">
							<text class="info-label">{{ $t('organization.dept_count') }}</text>
							<text class="info-value">{{ orgInfo.DepartmentCounter || 0 }}</text>
						</view>
						<view class="info-row">
							<text class="info-label">{{ $t('organization.car_count') }}</text>
							<text class="info-value">{{ orgInfo.CrashCartCounter || 0 }}</text>
						</view>
					</view>

					<view class="info-card" v-if="orgInfo.Remark">
						<view class="info-col">
							<text class="info-label">{{ $t('organization.remark') }}</text>
							<text class="info-value remark-text">{{ orgInfo.Remark }}</text>
						</view>
					</view>
				</view>

				<!-- Edit Mode: Editable Fields -->
				<view v-else class="edit-mode">
					<!-- Name -->
					<view class="form-group">
						<text class="label">{{ $t('organization.name') }}</text>
						<input class="input" type="text" v-model="orgInfo.Name"
							:placeholder="$t('organization.name_placeholder')" />
					</view>

					<!-- Hospital Level -->
					<view class="form-group">
						<text class="label">{{ $t('organization.level') }}</text>
						<picker class="picker-container" mode="selector" :range="hospitalLevels"
							@change="handleLevelChange">
							<view class="input picker-view">
								<text v-if="orgInfo.Level">{{ orgInfo.Level }}</text>
								<text v-else class="placeholder">{{ $t('organization.level_placeholder') }}</text>
							</view>
						</picker>
					</view>

					<!-- Remark -->
					<view class="form-group">
						<text class="label">{{ $t('organization.remark') }}</text>
						<textarea class="textarea" v-model="orgInfo.Remark"
							:placeholder="$t('organization.remark')"></textarea>
					</view>
				</view>

				<!-- Action Button -->
				<view class="btn-container" v-if="isCreator">
					<button class="btn-primary" @click="handleEditAction">{{ isEditing ? $t('organization.update') :
						$t('organization.edit') }}</button>
				</view>

				<!-- Exit Button -->
				<view class="btn-container" v-if="hasOrg && !isEditing">
					<button class="btn-danger" @click="handleDeleteAction">{{ $t('organization.quit') }}</button>
				</view>
			</view>

			<!-- No Organization: Create Mode -->
			<view v-else class="form-container">
				<view class="info-text">
					<text>{{ $t('organization.no_org_tip') }}</text>
				</view>

				<!-- Name -->
				<view class="form-group">
					<text class="label">{{ $t('organization.name') }}</text>
					<input class="input" type="text" v-model="createForm.Name"
						:placeholder="$t('organization.name_placeholder')" />
				</view>

				<!-- Hospital Level -->
				<view class="form-group">
					<text class="label">{{ $t('organization.level') }}</text>
					<picker class="picker-container" mode="selector" :range="hospitalLevels"
						@change="handleCreateLevelChange">
						<view class="input picker-view">
							<text v-if="createForm.Level">{{ createForm.Level }}</text>
							<text v-else class="placeholder">{{ $t('organization.level_placeholder') }}</text>
						</view>
					</picker>
				</view>

				<!-- Invitation Code -->
				<view class="form-group">
					<text class="label">{{ $t('organization.invite_code') }}</text>
					<input class="input" type="text" v-model="createForm.InvitationCode"
						:placeholder="$t('organization.invite_code_placeholder')" />
				</view>

				<!-- Create Button -->
				<view class="btn-container">
					<button class="btn-primary" @click="handleCreate">{{ $t('organization.create') }}</button>
				</view>
			</view>
		</scroll-view>
	</view>
</template>

<script>
import request from '@/utils/request.js';

export default {
	data() {
		return {
			isLoading: true,
			isRefreshing: false,
			hasOrg: false,
			isEditing: false,
			orgInfo: {
				Name: '',
				Level: '',
				AuditStatus: 0, // 0: Pending, 1: Approved, 2: Expired
				Creator: '',
				CreationTime: '',
				DepartmentCounter: 0,
				CrashCartCounter: 0,
				CrashCartCounter: 0,
				Remark: '',
				CreatorId: ''
			},
			userInfo: {},
			createForm: {
				Name: '',
				Level: '',
				InvitationCode: ''
			},
			hospitalLevels: [
				'三级甲等',
				'三级乙等',
				'二级甲等',
				'二级乙等',
				'一级医院',
				'社区医院',
				'未定级'
			]
		}
	},
	onLoad() {
		const uInfo = uni.getStorageSync('userOrgInfo');
		if (uInfo && uInfo.User) {
			this.userInfo = uInfo.User;
		}
		this.fetchOrgInfo();
	},
	computed: {
		isCreator() {
			if (!this.orgInfo || !this.userInfo) return false;
			// Check CreatorId if available, otherwise check Creator name
			if (this.orgInfo.CreatorId && this.userInfo.UserId) {
				return this.orgInfo.CreatorId == this.userInfo.UserId;
			}
			return this.orgInfo.Creator === this.userInfo.UserName;
		}
	},
	methods: {
		goBack() {
			uni.navigateBack();
		},
		onRefresh() {
			this.isRefreshing = true;
			const delay = new Promise(resolve => setTimeout(resolve, 1500));
			const fetch = this.fetchOrgInfo();

			Promise.all([fetch, delay]).then(() => {
				this.isRefreshing = false;
				uni.showToast({
					title: this.$t('index.refreshed'),
					icon: 'none'
				});
			}).catch(() => {
				this.isRefreshing = false;
			});
		},
		fetchOrgInfo() {
			this.isLoading = true;
			const t = this;
			// Try to get current organization info
			// Assuming an API endpoint exists, e.g., 'api/Sys_Organization/getCurrent'
			// If 404 or empty, we assume no org.
			return request.get('api/ssbh/Organization/UserOrganization')
				.then(res => {
					if (res.total) {
						t.hasOrg = true;
						t.orgInfo = res.rows[0];
					} else {
						t.createForm.Name = '';
						t.createForm.Level = '';
						t.hasOrg = false;
					}
				})
				.catch(err => {
					console.error("Failed to fetch org info", err);
					// If 404 implied not found, treat as empty
					t.hasOrg = false;
				})
				.finally(() => {
					t.isLoading = false;
				});
		},
		getStatusText(status) {
			const map = {
				0: this.$t('organization.status_pending'),
				1: this.$t('organization.status_approved'),
				2: this.$t('organization.status_expired')
			};
			return map[status] || '-';
		},
		getStatusClass(status) {
			const map = {
				0: 'status-pending',
				1: 'status-approved',
				2: 'status-expired'
			};
			return map[status] || '';
		},
		handleEditAction() {
			if (!this.isEditing) {
				this.isEditing = true;
			} else {
				this.handleUpdate();
			}
		},
		handleUpdate() {
			if (!this.orgInfo.Name) {
				uni.showToast({ title: 'Organization Name is required', icon: 'none' });
				return;
			}

			const updateData = {
				...this.orgInfo,
				Name: this.orgInfo.Name,
				Level: this.orgInfo.Level,
				Remark: this.orgInfo.Remark
			};

			request.post('api/ssbh/Organization/update', { "mainData": updateData, "detailData": null, "delKeys": null })
				.then(res => {
					if (res.status) {
						uni.showToast({ title: this.$t('organization.success_update'), icon: 'success' });
						this.isEditing = false;
						this.fetchOrgInfo(); // Refresh
					} else {
						uni.showToast({ title: res.message || this.$t('organization.fail_update'), icon: 'none' });
					}
				})
				.catch(err => {
					uni.showToast({ title: 'Update Error', icon: 'none' });
				});
		},
		handleCreate() {
			if ((!this.createForm.Name || !this.createForm.Level) && !this.createForm.InvitationCode) {
				uni.showToast({ title: 'Please fill required fields', icon: 'none' });
				return;
			}

			request.post('api/ssbh/Organization/add', { "mainData": this.createForm, "detailData": null, "delKeys": null })
				.then(res => {
					if (res.status) {
						uni.showToast({ title: this.$t('organization.success_create'), icon: 'success' });
						this.fetchOrgInfo(); // Switch to view mode
					} else {
						uni.showToast({ title: res.message || this.$t('organization.fail_create'), icon: 'none' });
					}
				})
				.catch(err => {
					uni.showToast({ title: 'Create Error', icon: 'none' });
				});
		},
		handleDeleteAction() {
			const uInfo = uni.getStorageSync('userOrgInfo');
			if (!uInfo || !uInfo.Organization.Id) {
				uni.showToast({ title: 'Failed to find relation info', icon: 'none' });
				return;
			}
			const t = this;
			uni.showModal({
				title: this.$t('common.confirm'),
				content: this.$t('organization.quit_confirm'),
				success: (res) => {
					if (res.confirm) {
						request.post('api/ssbh/OrganizationUser/Del',
							[uInfo.User.Id]
						).then(res => {
							if (res.status || res.success) {
								uni.showToast({ title: t.$t('common.success'), icon: 'success' });
								t.hasOrg = false;
								t.orgInfo = {};
								t.createForm.Name = '';
								t.createForm.Level = '';
								uni.removeStorageSync('userOrgInfo');
								// Refresh current page logic handled by reactive hasOrg
							} else {
								uni.showToast({ title: res.message || t.$t('common.fail'), icon: 'none' });
							}
						}).catch(err => {
							console.error(err);
							uni.showToast({ title: 'Network Error', icon: 'none' });
						});
					}
				}
			});
		},

		handleLevelChange(e) {
			this.orgInfo.Level = this.hospitalLevels[e.detail.value];
		},
		handleCreateLevelChange(e) {
			this.createForm.Level = this.hospitalLevels[e.detail.value];
		},
		copyToClipboard(text) {
			if (!text) return;
			uni.setClipboardData({
				data: text,
				success: () => {
					uni.showToast({
						title: 'Copied',
						icon: 'success'
					});
				}
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
	background-color: $sull-color-background;
}

/* Header Styles */
.header {
	padding: 44px 24px 20px;
	display: flex;
	align-items: center;
	/* Use background similar to others if desired, or keep white but style back button */
	background: #fff;
	box-shadow: 0 2px 10px rgba(0, 0, 0, 0.02);
	position: relative;
}

.back-btn-placeholder {
	width: 40px;
	height: 40px;
	background: #F6F8FA;
	/* distinct background for white header */
	border-radius: 50%;
	display: flex;
	align-items: center;
	justify-content: center;
	position: absolute;
	left: 24px;
	z-index: 10;
}

.header-title {
	font-size: 18px;
	font-weight: bold;
	color: $sull-text-main;
	width: 100%;
	text-align: center;
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

.icon-copy {
	width: 16px;
	height: 16px;
	background-color: #1890FF;
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M16 1H4c-1.1 0-2 .9-2 2v14h2V3h12V1zm3 4H8c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h11c1.1 0 2-.9 2-2V7c0-1.1-.9-2-2-2zm0 16H8V7h11v14z'/%3E%3C/svg%3E");
	-webkit-mask-size: contain;
	mask-size: contain;
	-webkit-mask-repeat: no-repeat;
	mask-repeat: no-repeat;
	-webkit-mask-position: center;
	mask-position: center;
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M16 1H4c-1.1 0-2 .9-2 2v14h2V3h12V1zm3 4H8c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h11c1.1 0 2-.9 2-2V7c0-1.1-.9-2-2-2zm0 16H8V7h11v14z'/%3E%3C/svg%3E");
	cursor: pointer;
}

.content {
	flex: 1;
	height: 0;
	/* Padding handled in child container to keep scrollbar at edge */
	padding: 0;
	box-sizing: border-box;
	width: 100%;
}

.form-container {
	display: flex;
	flex-direction: column;
	gap: 24px;
	padding: 24px;
	/* Move padding here */
}

.info-text {
	padding: 16px;
	background: #E6F7FF;
	border-radius: 8px;
	color: #1890FF;
	font-size: 14px;
	margin-bottom: 8px;
}

.form-group {
	display: flex;
	flex-direction: column;
	gap: 8px;
}

.label {
	font-size: 14px;
	color: $sull-text-secondary;
	font-weight: 500;
}

.input {
	height: 50px;
	background: #fff;
	border-radius: 12px;
	padding: 0 16px;
	font-size: 14px;
	color: $sull-text-main;
	width: 100%;
	box-sizing: border-box;
}

.input.readonly {
	background: #f5f5f5;
	color: #999;
}

.textarea {
	height: 100px;
	background: #fff;
	border-radius: 12px;
	padding: 16px;
	font-size: 14px;
	color: $sull-text-main;
	width: 100%;
	box-sizing: border-box;
}

.textarea.readonly {
	background: #f5f5f5;
	color: #999;
}

.status-badge {
	display: inline-flex;
	padding: 6px 16px;
	border-radius: 20px;
	font-size: 14px;
	font-weight: 500;
	align-self: flex-start;
}

.status-pending {
	background: #E6F7FF;
	color: #1890FF;
}

.status-approved {
	background: #F6FFED;
	color: #52C41A;
}

.status-expired {
	background: #FFF1F0;
	color: #F5222D;
}

.btn-container {
	margin-top: 24px;
}

.btn-primary {
	width: 100%;
	height: 50px;
	background: $sull-color-primary;
	color: #fff;
	border-radius: 25px;
	font-size: 16px;
	font-weight: 600;
	display: flex;
	align-items: center;
	justify-content: center;
}

.btn-danger {
	width: 100%;
	height: 50px;
	background: #ffffff;
	color: #FF4D4F;
	border: 1px solid #FF4D4F;
	border-radius: 25px;
	font-size: 16px;
	font-weight: 600;
	display: flex;
	align-items: center;
	justify-content: center;
}

.loading-state {
	display: flex;
	justify-content: center;
	padding-top: 40px;
	color: $sull-text-secondary;
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

.picker-container {
	width: 100%;
}

.picker-view {
	display: flex;
	align-items: center;
	width: 100%;
}

.placeholder {
	color: #999;
}

/* View Mode Styles */
.view-mode {
	display: flex;
	flex-direction: column;
	gap: 16px;
}

.info-card {
	background: #fff;
	border-radius: 16px;
	padding: 20px;
	display: flex;
	flex-direction: column;
	gap: 16px;
}

.info-row {
	display: flex;
	justify-content: space-between;
	align-items: center;
}

.info-col {
	display: flex;
	flex-direction: column;
	gap: 8px;
}

.info-label {
	font-size: 14px;
	color: $sull-text-secondary;
}

.info-value {
	font-size: 14px;
	color: $sull-text-main;
	font-weight: 500;
	text-align: right;
}

.value-with-copy {
	display: flex;
	align-items: center;
	gap: 8px;
}

.remark-text {
	text-align: left;
	line-height: 1.5;
}

.status-badge-small {
	display: inline-flex;
	padding: 2px 8px;
	border-radius: 4px;
	font-size: 12px;
}

/* Ensure edit mode matches spacing */
.edit-mode {
	display: flex;
	flex-direction: column;
	gap: 24px;
}
</style>

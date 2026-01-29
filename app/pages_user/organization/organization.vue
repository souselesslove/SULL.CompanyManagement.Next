<template>
	<view class="container">
		<view class="header" :style="{ paddingTop: statusBarHeight * 2 + 'rpx' }">
			<view class="header-content">
				<view class="back-btn-placeholder" @click="goBack">
					<view class="icon-arrow-left"></view>
				</view>
				<text class="header-title">{{ hasOrg ? (isEditing ? $t('organization.title_update') :
					$t('organization.title_info')) : $t('organization.title_create') }}</text>
			</view>
		</view>

		<scroll-view scroll-y class="content" refresher-enabled="true" :refresher-triggered="isRefreshing"
			@refresherrefresh="onRefresh" @refresherpulling="onPulling" @refresherrestore="onRestore"
			refresher-default-style="none" refresher-background="#f0f5ff">
			<CustomRefresher slot="refresher" :isRefreshing="isRefreshing" :isPulling="isPulling" />

			<!-- Loading State -->
			<view v-if="isLoading" class="loading-state">
				<text>Loading...</text>
			</view>

			<!-- Has Organization -->
			<view v-else-if="hasOrg" class="form-container">
				<!-- Tabs -->
				<view class="tabs">
					<view class="tab-item" :class="{ active: currentTab === 0 }" @click="handleTabChange(0)">
						<text>{{ $t('organization.tab_info') || '机构信息' }}</text>
					</view>
					<view class="tab-item" :class="{ active: currentTab === 1 }" @click="handleTabChange(1)">
						<text>{{ $t('organization.tab_members') || '人员列表' }}</text>
					</view>
					<view v-if="isSuperAdmin" class="tab-item" :class="{ active: currentTab === 2 }"
						@click="handleTabChange(2)">
						<text>{{ $t('organization.tab_all_orgs') || '所有机构' }}</text>
					</view>
				</view>

				<!-- Tab 0: Organization Info -->
				<view v-if="currentTab === 0" class="info-content">
					<!-- QR Code Section -->
					<view class="qr-container" v-if="orgInfo.Id && !isEditing">
						<view class="qr-box">
							<image :src="'https://api.qrserver.com/v1/create-qr-code/?size=150x150&data=' + orgInfo.Id"
								class="qr-code" mode="aspectFit"></image>
							<text class="qr-tip">{{ $t('organization.invite_qr_tip') || '扫描二维码加入机构' }}</text>
						</view>
					</view>

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
									<view class="icon-copy" v-if="orgInfo.Id" @click="copyToClipboard(orgInfo.Id)">
									</view>
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

				<!-- Tab 1: Member List -->
				<view v-if="currentTab === 1" class="members-container">
					<!-- Search -->
					<view class="search-box">
						<view class="icon-search"></view>
						<input class="search-input" type="text" v-model="memberSearchKey"
							:placeholder="$t('common.search') || '搜索'" />
					</view>
					<!-- Filter -->
					<view class="status-filter">
						<view class="filter-chip" :class="{ active: memberStatusFilter === 'all' }"
							@click="memberStatusFilter = 'all'">全部</view>
						<view class="filter-chip" :class="{ active: memberStatusFilter === 'onDuty' }"
							@click="memberStatusFilter = 'onDuty'">当班</view>
						<view class="filter-chip" :class="{ active: memberStatusFilter === 'offDuty' }"
							@click="memberStatusFilter = 'offDuty'">休息</view>
						<view class="filter-chip" :class="{ active: memberStatusFilter === 'leave' }"
							@click="memberStatusFilter = 'leave'">请假</view>
					</view>
					<!-- List -->
					<view class="member-list">
						<view v-for="(member, index) in filteredMembers" :key="index" class="member-card">
							<view class="member-info">
								<view class="member-main">
									<text class="member-name">{{ member.UserTrueName || member.UserName }}</text>
									<text v-if="member.DepartmentName" class="member-tag dept">{{ member.DepartmentName
									}}</text>
									<text v-if="member.Title" class="member-tag title">{{ member.Title }}</text>
								</view>
								<view class="member-sub">
									<text class="member-status" :class="getMemberStatusClass(member)">{{
										getMemberStatusText(member) }}</text>
									<text class="member-time">{{ $t('department.member_join_time') }}: {{
										(member.CreationTime || '').substring(0, 10) }}</text>
								</view>
							</view>
							<view class="member-action">
								<view class="dots-btn" @click="openMemberMenu(member)">
									<view class="dot"></view>
									<view class="dot"></view>
									<view class="dot"></view>
								</view>
							</view>
						</view>
						<view v-if="filteredMembers.length === 0" class="empty-state">
							<text>{{ $t('common.no_data') }}</text>
						</view>
					</view>
				</view>

				<!-- Tab 2: All Organizations List -->
				<view v-if="currentTab === 2" class="members-container">
					<!-- Search -->
					<view class="search-box">
						<view class="icon-search"></view>
						<input class="search-input" type="text" v-model="allOrgSearchKey"
							:placeholder="$t('organization.search_placeholder') || '搜索机构名称'" />
					</view>
					<!-- List -->
					<view class="member-list">
						<view v-for="(org, index) in filteredAllOrganizations" :key="index" class="member-card">
							<view class="member-info">
								<view class="member-main">
									<text class="member-name">{{ org.Name }}</text>
									<text class="member-tag dept">{{ org.Level }}</text>
								</view>
								<view class="member-sub" style="margin-top: 8rpx;">
									<view class="status-badge-small" :class="getStatusClass(org.AuditStatus)"
										style="margin-right: 16rpx; transform: scale(0.9); transform-origin: left center;">
										<text>{{ getStatusText(org.AuditStatus) }}</text>
									</view>
									<view class="value-with-copy">
										<text class="member-time">{{ $t('organization.invite_code') }}: {{ org.Id
										}}</text>
										<view class="icon-copy" style="width: 28rpx; height: 28rpx;"
											@click="copyToClipboard(org.Id)"></view>
									</view>
								</view>
								<view class="member-sub" style="margin-top: 4rpx;">
									<text class="member-time">{{ $t('organization.creator') }}: {{ org.Creator }}</text>
									<text class="member-time" style="margin-left: 20rpx;">{{
										$t('organization.create_time') }}: {{ (org.CreationTime ||
											'').substring(0, 10) }}</text>
								</view>
							</view>
							<view class="member-action">
								<view class="dots-btn" @click="openOrgMenu(org)">
									<view class="dot"></view>
									<view class="dot"></view>
									<view class="dot"></view>
								</view>
							</view>
						</view>
						<view v-if="filteredAllOrganizations.length === 0" class="empty-state">
							<text>{{ $t('common.no_data') }}</text>
						</view>
					</view>
				</view>
			</view>

			<!-- No Organization: Create Mode -->
			<view v-else class="form-container">
				<!-- Scan to Join Hero Section -->
				<view class="scan-hero" @click="handleScan">
					<view class="scan-icon-lg"></view>
					<text class="scan-text-lg">{{ $t('organization.scan_to_join') || '扫码快速加入机构' }}</text>
				</view>

				<view class="divider-row">
					<view class="line"></view>
					<text class="divider-text">{{ $t('organization.or_create') || '或填表创建' }}</text>
					<view class="line"></view>
				</view>

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
		<!-- Modal and ActionSheet -->

		<!-- Member Action Sheet -->
		<view class="action-sheet-mask" v-if="showActionSheet" @click="showActionSheet = false">
			<view class="action-sheet" @click.stop>
				<view class="action-header">
					<text class="action-title">{{ menuType === 'member' ? (selectedMember?.UserTrueName ||
						selectedMember?.UserName) : selectedOrg?.Name }}</text>
				</view>
				<view class="action-list">
					<!-- Member Actions -->
					<template v-if="menuType === 'member'">
						<view class="action-item" v-if="currentUserLevel >= 40"
							@click="handleActionSelection('setPosition')">
							<text>设置职别</text>
						</view>
					</template>
					<!-- Organization Actions -->
					<template v-else-if="menuType === 'org'">
						<view class="action-item" @click="handleOrgAction('approve')">
							<text>{{ $t('organization.action_approve') }}</text>
						</view>
						<view class="action-item" @click="handleOrgAction('switch')">
							<text>{{ $t('organization.action_switch') }}</text>
						</view>
						<view class="action-item" style="color: #FF4D4F;" @click="handleOrgAction('delete')">
							<text>{{ $t('organization.action_delete') }}</text>
						</view>
					</template>
				</view>
				<view class="action-cancel" @click="showActionSheet = false">
					<text>{{ $t('common.cancel') }}</text>
				</view>
			</view>
		</view>

		<!-- Position Selection Modal -->
		<view class="modal-mask" v-if="showPositionModal" @click="showPositionModal = false"
			@touchmove.stop.prevent="doNothing">
			<view class="modal-content position-modal" @click.stop>
				<view class="modal-header">
					<text class="modal-title">{{ $t('department.set_position') || '设置职别' }}</text>
					<view class="close-btn" @click="showPositionModal = false">✕</view>
				</view>
				<view class="modal-body">
					<view class="position-group" v-for="group in sortedGroups" :key="group">
						<view class="level-row">
							<view class="position-tag" v-for="(pos, idx) in getPositionsByGroup(group)" :key="idx"
								:class="{ active: isPositionSelected(pos.title) }" @click="togglePosition(pos)">
								{{ pos.title }}
							</view>
						</view>
					</view>
				</view>
				<view class="modal-footer">
					<button class="btn-primary" @click="submitPositionChange">{{ $t('common.confirm') }}</button>
				</view>
			</view>
		</view>
	</view>
</template>

<script>
import { globalScan } from '@/utils/scan.js';
import request from '@/utils/request.js';
import { storage } from '@/utils/storage.js';
import CustomRefresher from '@/components/CustomRefresher/CustomRefresher.vue';

export default {
	components: { CustomRefresher },
	data() {
		return {
			statusBarHeight: 20,
			isLoading: true,
			isRefreshing: false,
			isPulling: false,
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
			],
			// Member Logic
			currentTab: 0,
			members: [],
			memberSearchKey: '',
			memberStatusFilter: 'all',
			showActionSheet: false,
			menuType: 'member', // 'member' or 'org'
			showPositionModal: false,
			selectedMember: null,
			selectedOrg: null,
			selectedPositions: [],
			positionData: [
				{ "title": "分管院长", "level": 50, "group": 50 },
				{ "title": "护理部主任", "level": 40, "group": 40 },
				{ "title": "科主任", "level": 31, "group": 30 },
				{ "title": "护士长", "level": 30, "group": 30 },
				{ "title": "医生", "level": 22, "group": 20 },
				{ "title": "护士", "level": 20, "group": 20 },
				{ "title": "技师", "level": 21, "group": 20 },
				{ "title": "护工", "level": 11, "group": 10 },
				{ "title": "后勤", "level": 10, "group": 10 }
			],
			// All Orgs Logic
			allOrganizations: [],
			allOrgSearchKey: ''
		}
	},
	onLoad() {
		const sysInfo = uni.getSystemInfoSync();
		this.statusBarHeight = sysInfo.statusBarHeight || 20;
		const uInfo = storage.get('userOrgInfo');
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
		},
		filteredMembers() {
			let res = this.members || [];
			// 1. Filter by Name only
			if (this.memberSearchKey) {
				const low = this.memberSearchKey.toLowerCase();
				res = res.filter(m => (m.UserTrueName && m.UserTrueName.toLowerCase().includes(low)) || (m.UserName && m.UserName.toLowerCase().includes(low)));
			}
			// 2. Filter by Status (Requires actual status field in member data)
			// Assuming 'WorkStatus' or similar. If not present, this is a placeholder.
			if (this.memberStatusFilter !== 'all') {
				// Mock logic or real if field exists. Let's assume 'WorkStatus' 0:OnDuty, 1:Off, 2:Leave
				// If no field, this filter does nothing or mocks.
				// User requested: "当班", "休息", "请假". map to 0, 1, 2?
				// Just implementing placeholder filter logic:
				/*
				const map = { 'onDuty': 0, 'offDuty': 1, 'leave': 2 };
				const target = map[this.memberStatusFilter];
				res = res.filter(m => m.WorkStatus === target);
				*/
			}
			return res;
		},
		filteredAllOrganizations() {
			let res = this.allOrganizations || [];
			if (this.allOrgSearchKey) {
				const low = this.allOrgSearchKey.toLowerCase();
				res = res.filter(org => (org.Name && org.Name.toLowerCase().includes(low)) || (org.Id && org.Id.toLowerCase().includes(low)));
			}
			return res;
		},
		currentUserLevel() {
			const userOrgInfo = storage.get('userOrgInfo');
			return userOrgInfo?.User?.Level || 0;
		},
		isSuperAdmin() {
			const userInfo = storage.get('userInfo');
			// Check if User object has RoleName or if it's in the top level of userOrgInfo
			return userInfo?.roleName === '超级管理员';
		},
		sortedGroups() {
			const visiblePositions = this.positionData.filter(p => p.level < this.currentUserLevel || this.isSuperAdmin);
			const groups = [...new Set(visiblePositions.map(p => p.group))];
			return groups.sort((a, b) => b - a);
		}
	},
	methods: {
		goBack() {
			uni.navigateBack();
		},
		onPulling() {
			this.isPulling = true;
		},
		onRestore() {
			this.isPulling = false;
		},
		onRefresh() {
			this.isRefreshing = true;
			this.isPulling = true;
			// Refresh actual data
			this.fetchOrgInfo().then(() => {
				setTimeout(() => {
					this.isRefreshing = false;
					this.isPulling = false;
					uni.showToast({
						title: this.$t('index.refreshed'),
						icon: 'none'
					});
				}, 800);
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
					t.refreshUserOrgInfo().finally(() => {
						t.isLoading = false;
					});
				});
		},
		refreshUserOrgInfo() {
			return request.get('api/ssbh/OrganizationUser/UserOrganization').then(res => {
				if (res) {
					storage.set('userOrgInfo', res);
					const uInfo = res;
					if (uInfo && uInfo.User) {
						this.userInfo = uInfo.User;
					}
				}
				return res;
			}).catch(err => {
				console.error("Refresh UserOrgInfo Failed", err);
			});
		},
		handleTabChange(index) {
			this.currentTab = index;
			if (index === 1 && this.members.length === 0) {
				this.fetchMembers();
			} else if (index === 2 && this.allOrganizations.length === 0) {
				this.fetchAllOrganizations();
			}
		},
		fetchMembers() {
			const form = {
				page: 1,
				rows: 1000,
				sort: 'CreationTime',
				order: 'desc',
				wheres: JSON.stringify([
					{ name: 'OrganizationId', value: this.orgInfo.Id }
				])
			};
			request.post('api/ssbh/OrganizationUser/GetPageData', form)
				.then(res => {
					this.members = res.rows || [];
				})
				.catch(err => {
					console.error("Fetch members failed", err);
				});
		},
		fetchAllOrganizations() {
			const form = {
				page: 1,
				rows: 1000,
				sort: 'CreationTime',
				order: 'desc'
			};
			uni.showLoading({ title: this.$t('common.loading') || 'Loading...' });
			request.post('api/ssbh/Organization/GetPageData', form)
				.then(res => {
					this.allOrganizations = res.rows || [];
				})
				.catch(err => {
					console.error("Fetch all organizations failed", err);
				})
				.finally(() => {
					uni.hideLoading();
				});
		},
		getMemberStatusText(member) {
			// Placeholder logic, adjust if API provides status
			const s = member.WorkStatus;
			if (s === 0) return '当班';
			if (s === 1) return '休息';
			if (s === 2) return '请假';
			return '未知';
		},
		getMemberStatusClass(member) {
			const s = member.WorkStatus;
			if (s === 0) return 'status-on-duty';
			if (s === 1) return 'status-off-duty';
			if (s === 2) return 'status-leave';
			return '';
		},
		// Menu & Position Methods
		openMemberMenu(member) {
			this.selectedMember = member;
			this.menuType = 'member';
			this.showActionSheet = true;
		},
		openOrgMenu(org) {
			this.selectedOrg = org;
			this.menuType = 'org';
			this.showActionSheet = true;
		},
		handleOrgAction(type) {
			this.showActionSheet = false;
			const org = this.selectedOrg;
			if (!org) return;

			if (type === 'switch') {
				uni.showLoading({ title: this.$t('common.syncing') });
				request.post('api/ssbh/OrganizationDepartment/SetUserInfo', { mainData: { OrganizationId: org.Id } })
					.then(res => {
						if (res.status || res.success) {
							uni.showToast({ title: this.$t('common.success'), icon: 'success' });
							// Emit global event to refresh other pages
							uni.$emit('refreshUserOrgInfo');
							this.refreshUserOrgInfo().then(() => {
								this.fetchOrgInfo();
							});
						} else {
							uni.showToast({ title: res.message || this.$t('common.fail'), icon: 'none' });
						}
					})
					.catch(err => {
						console.error(err);
						uni.showToast({ title: 'Network Error', icon: 'none' });
					})
					.finally(() => {
						uni.hideLoading();
					});
			} else if (type === 'approve') {
				// Approved logic could be a status update
				uni.showModal({
					title: this.$t('organization.action_approve'),
					content: `确定审批通过机构: ${org.Name} 吗？`,
					success: (res) => {
						if (res.confirm) {
							uni.showLoading({ title: this.$t('common.syncing') });
							request.post('api/ssbh/Organization/update', { mainData: { ...org, AuditStatus: 1 } })
								.then(res => {
									if (res.status || res.success) {
										uni.showToast({ title: this.$t('common.success'), icon: 'success' });
										this.fetchAllOrganizations();
									} else {
										uni.showToast({ title: res.message || this.$t('common.fail'), icon: 'none' });
									}
								})
								.finally(() => uni.hideLoading());
						}
					}
				});
			} else if (type === 'delete') {
				uni.showModal({
					title: this.$t('common.confirm'),
					content: `确定要删除机构: ${org.Name} 吗？`,
					success: (res) => {
						if (res.confirm) {
							uni.showLoading({ title: this.$t('common.deleting') });
							request.post('api/ssbh/Organization/Del', [org.Id])
								.then(res => {
									if (res.status || res.success) {
										uni.showToast({ title: this.$t('common.success'), icon: 'success' });
										this.fetchAllOrganizations();
									} else {
										uni.showToast({ title: res.message || this.$t('common.fail'), icon: 'none' });
									}
								})
								.finally(() => uni.hideLoading());
						}
					}
				});
			}
		},
		handleActionSelection(action) {
			this.showActionSheet = false;
			if (action === 'setPosition') {
				this.selectedPositions = this.selectedMember.Title ? this.selectedMember.Title.split(',') : [];
				this.showPositionModal = true;
			}
		},
		getPositionsByGroup(group) {
			return this.positionData.filter(p => p.group === group && p.level < this.currentUserLevel);
		},
		isPositionSelected(title) {
			return this.selectedPositions.includes(title);
		},
		togglePosition(pos) {
			if (this.selectedPositions.includes(pos.title)) {
				this.selectedPositions = [];
			} else {
				this.selectedPositions = [pos.title];
			}
		},
		submitPositionChange() {
			if (!this.selectedMember) return;
			let highestLevel = 0;
			if (this.selectedPositions.length > 0) {
				const levels = this.selectedPositions.map(title => {
					const p = this.positionData.find(pd => pd.title === title);
					return p ? p.level : 0;
				});
				highestLevel = Math.max(...levels);
			}

			const updateData = {
				...this.selectedMember,
				Title: this.selectedPositions.join(','),
				Level: highestLevel
			};

			uni.showLoading({ title: this.$t('common.syncing') || 'Updating...' });
			request.post('api/ssbh/OrganizationUser/Update', { mainData: updateData })
				.then(res => {
					uni.hideLoading();
					if (res.status || res.success) {
						uni.showToast({ title: this.$t('common.success'), icon: 'success' });
						this.showPositionModal = false;
						this.fetchMembers();
					} else {
						uni.showToast({ title: res.message || this.$t('common.fail'), icon: 'none' });
					}
				})
				.catch(err => {
					uni.hideLoading();
					uni.showToast({ title: 'Error', icon: 'none' });
				});
		},
		doNothing() { },
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
						this.refreshUserOrgInfo().then(() => {
							this.fetchOrgInfo(); // Refresh
						});
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
						this.refreshUserOrgInfo().then(() => {
							this.fetchOrgInfo(); // Switch to view mode
						});
					} else {
						uni.showToast({ title: res.message || this.$t('organization.fail_create'), icon: 'none' });
					}
				})
				.catch(err => {
					uni.showToast({ title: 'Create Error', icon: 'none' });
				});
		},
		handleDeleteAction() {
			const uInfo = storage.get('userOrgInfo');
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
								t.refreshUserOrgInfo().then(() => {
									t.hasOrg = false;
									t.orgInfo = {};
									t.createForm.Name = '';
									t.createForm.Level = '';
								});
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
		},
		handleScan() {
			globalScan((res) => {
				if (res.data) {
					this.createForm.Level = ''; // Clear level if scanning
					this.createForm.InvitationCode = res.data;
					uni.showLoading({ title: this.$t('common.syncing') || 'Joining...' });
					this.handleCreate();
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
	background: #fff;
	box-shadow: 0 4rpx 20rpx rgba(0, 0, 0, 0.02);
	position: relative;
	z-index: 100;
}

.header-content {
	height: 88rpx;
	display: flex;
	align-items: center;
	padding: 0 48rpx;
	position: relative;
}

.back-btn-placeholder {
	width: 80rpx;
	height: 80rpx;
	background: #F6F8FA;
	border-radius: 50%;
	display: flex;
	align-items: center;
	justify-content: center;
	position: absolute;
	left: 48rpx;
	z-index: 10;
}

.header-title {
	font-size: 34rpx;
	font-weight: bold;
	color: $sull-text-main;
	width: 100%;
	text-align: center;
}

.icon-arrow-left {
	width: 40rpx;
	height: 40rpx;
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
	width: 32rpx;
	height: 32rpx;
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
	padding: 48rpx;
	/* Move padding here */
}

.info-text {
	padding: 32rpx;
	background: #E6F7FF;
	border-radius: 16rpx;
	color: #1890FF;
	font-size: 28rpx;
	margin-bottom: 16rpx;
}

.form-group {
	display: flex;
	flex-direction: column;
	gap: 16rpx;
}

.label {
	font-size: 28rpx;
	color: $sull-text-secondary;
	font-weight: 500;
}

.input {
	height: 100rpx;
	background: #fff;
	border-radius: 24rpx;
	padding: 0 32rpx;
	font-size: 28rpx;
	color: $sull-text-main;
	width: 100%;
	box-sizing: border-box;
}

.input.readonly {
	background: #f5f5f5;
	color: #999;
}

.textarea {
	height: 200rpx;
	background: #fff;
	border-radius: 24rpx;
	padding: 32rpx;
	font-size: 28rpx;
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
	padding: 12rpx 32rpx;
	border-radius: 40rpx;
	font-size: 28rpx;
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
	margin-top: 20rpx;
}

.btn-primary {
	width: 100%;
	height: 100rpx;
	background: $sull-color-primary;
	color: #fff;
	border-radius: 50rpx;
	font-size: 32rpx;
	font-weight: 600;
	display: flex;
	align-items: center;
	justify-content: center;
}

.btn-danger {
	width: 100%;
	height: 100rpx;
	background: #ffffff;
	color: #FF4D4F;
	border: 2rpx solid #FF4D4F;
	border-radius: 50rpx;
	font-size: 32rpx;
	font-weight: 600;
	display: flex;
	align-items: center;
	justify-content: center;
}

.loading-state {
	display: flex;
	justify-content: center;
	padding-top: 80rpx;
	color: $sull-text-secondary;
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
	gap: 32rpx;
}

.info-card {
	background: #fff;
	border-radius: 32rpx;
	padding: 40rpx;
	display: flex;
	flex-direction: column;
	gap: 32rpx;
}

.info-row {
	display: flex;
	justify-content: space-between;
	align-items: center;
}

.info-col {
	display: flex;
	flex-direction: column;
	gap: 16rpx;
}

.info-label {
	font-size: 28rpx;
	color: $sull-text-secondary;
}

.info-value {
	font-size: 28rpx;
	color: $sull-text-main;
	font-weight: 500;
	text-align: right;
}

.value-with-copy {
	display: flex;
	align-items: center;
	gap: 16rpx;
}

.remark-text {
	text-align: left;
	line-height: 1.5;
}

.status-badge-small {
	display: inline-flex;
	padding: 4rpx 16rpx;
	border-radius: 8rpx;
	font-size: 24rpx;
}

/* Ensure edit mode matches spacing */
.edit-mode {
	display: flex;
	flex-direction: column;
	gap: 48rpx;
}

/* QR Code Section */
.qr-container {
	display: flex;
	justify-content: center;
	margin-bottom: 20rpx;
}

.qr-box {
	background: #fff;
	padding: 40rpx;
	border-radius: 32rpx;
	display: flex;
	flex-direction: column;
	align-items: center;
	gap: 20rpx;
	box-shadow: 0 8rpx 32rpx rgba(0, 0, 0, 0.05);
}

.qr-code {
	width: 300rpx;
	height: 300rpx;
}

.qr-tip {
	font-size: 24rpx;
	color: $sull-text-secondary;
}

/* Scan Hero Section */
.scan-hero {
	background: linear-gradient(135deg, $sull-color-primary 0%, #69c0ff 100%);
	padding: 60rpx 40rpx;
	border-radius: 32rpx;
	display: flex;
	flex-direction: column;
	align-items: center;
	gap: 32rpx;
	box-shadow: 0 8rpx 24rpx rgba(24, 144, 255, 0.2);
	margin-bottom: 20rpx;
}

.scan-hero:active {
	opacity: 0.9;
	transform: scale(0.98);
}

.scan-icon-lg {
	width: 100rpx;
	height: 100rpx;
	background-color: #fff;
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M3 5v4h2V5h4V3H5c-1.1 0-2 .9-2 2zm2 10H3v4c0 1.1.9 2 2 2h4v-2H5v-4zm14 4h-4v2h4c1.1 0 2-.9 2-2v-4h-2v4zm0-16h-4v2h4v4h2V5c0-1.1-.9-2-2-2z'/%3E%3C/svg%3E");
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M3 5v4h2V5h4V3H5c-1.1 0-2 .9-2 2zm2 10H3v4c0 1.1.9 2 2 2h4v-2H5v-4zm14 4h-4v2h4c1.1 0 2-.9 2-2v-4h-2v4zm0-16h-4v2h4v4h2V5c0-1.1-.9-2-2-2z'/%3E%3C/svg%3E");
	-webkit-mask-size: contain;
	mask-size: contain;
	-webkit-mask-repeat: no-repeat;
	mask-repeat: no-repeat;
	-webkit-mask-position: center;
	mask-position: center;
}

.scan-text-lg {
	color: #fff;
	font-size: 32rpx;
	font-weight: 600;
}

.divider-row {
	display: flex;
	align-items: center;
	gap: 24rpx;
	margin: 20rpx 0;
}

.divider-row .line {
	flex: 1;
	height: 2rpx;
	background: #eee;
}

.divider-text {
	font-size: 24rpx;
	color: #ccc;
}

/* Tabs */
.tabs {
	display: flex;
	background: #fff;
	border-radius: 50rpx;
	padding: 8rpx;
	margin-bottom: 32rpx;
	box-shadow: 0 4rpx 16rpx rgba(0, 0, 0, 0.02);
}

.tab-item {
	flex: 1;
	height: 80rpx;
	display: flex;
	align-items: center;
	justify-content: center;
	font-size: 28rpx;
	color: $sull-text-secondary;
	font-weight: 500;
	border-radius: 40rpx;
	transition: all 0.3s;
}

.tab-item.active {
	background: $sull-color-primary;
	color: #fff;
	font-weight: 600;
	box-shadow: 0 4rpx 12rpx rgba(149, 202, 255, 0.4);
}

/* Search */
.search-box {
	background: #F5F7FA;
	border-radius: 16rpx;
	padding: 16rpx 24rpx;
	display: flex;
	align-items: center;
	gap: 16rpx;
	margin-bottom: 24rpx;
}

.icon-search {
	width: 32rpx;
	height: 32rpx;
	background-color: #999;
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M15.5 14h-.79l-.28-.27C15.41 12.59 16 11.11 16 9.5 16 5.91 13.09 3 9.5 3S3 5.91 3 9.5 5.91 16 9.5 16c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19l-4.99-5zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 14z'/%3E%3C/svg%3E");
	-webkit-mask-size: contain;
	mask-size: contain;
	-webkit-mask-repeat: no-repeat;
	mask-repeat: no-repeat;
	-webkit-mask-position: center;
	mask-position: center;
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M15.5 14h-.79l-.28-.27C15.41 12.59 16 11.11 16 9.5 16 5.91 13.09 3 9.5 3S3 5.91 3 9.5 5.91 16 9.5 16c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19l-4.99-5zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 14z'/%3E%3C/svg%3E");
}

.search-input {
	font-size: 28rpx;
	color: $sull-text-main;
	flex: 1;
	background: transparent;
	border: none;
	outline: none;
	height: 48rpx;
	line-height: 48rpx;
}

/* Status Filter */
.status-filter {
	display: flex;
	gap: 16rpx;
	margin-bottom: 24rpx;
}

.filter-chip {
	padding: 10rpx 24rpx;
	background: #fff;
	border: 1rpx solid #E4E7ED;
	border-radius: 30rpx;
	font-size: 26rpx;
	color: #606266;
	transition: all 0.2s;
}

.filter-chip.active {
	background: #E6F7FF;
	border-color: #1890FF;
	color: #1890FF;
}

/* Member List */
.members-container {
	display: flex;
	flex-direction: column;
}

.member-card {
	background: #fff;
	border-radius: 20rpx;
	padding: 24rpx;
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-bottom: 20rpx;
	box-shadow: 0 2rpx 8rpx rgba(0, 0, 0, 0.02);
}

.member-info {
	display: flex;
	flex-direction: column;
	gap: 8rpx;
}

.member-main {
	display: flex;
	align-items: center;
	gap: 12rpx;
	flex-wrap: wrap;
}

.member-name {
	font-size: 30rpx;
	font-weight: 600;
	color: $sull-text-main;
}

.member-tag {
	font-size: 20rpx;
	padding: 2rpx 10rpx;
	border-radius: 6rpx;
}

.member-tag.dept {
	background: #F2F6FC;
	color: #909399;
}

.member-tag.title {
	background: #E6F7FF;
	color: #1890FF;
}

.member-sub {
	display: flex;
	align-items: center;
	gap: 16rpx;
}

.member-time {
	font-size: 24rpx;
	color: $sull-text-secondary;
}

.member-status {
	font-size: 24rpx;
}

.status-on-duty {
	color: #52C41A;
}

.status-off-duty {
	color: #909399;
}

.status-leave {
	color: #FA8C16;
}

/* Dots Button */
.dots-btn {
	width: 60rpx;
	height: 60rpx;
	display: flex;
	flex-direction: column;
	justify-content: center;
	align-items: center;
	gap: 6rpx;
}

.dots-btn .dot {
	width: 8rpx;
	height: 8rpx;
	background-color: #999;
	border-radius: 50%;
}

/* Action Sheet & Modals */
.action-sheet-mask,
.modal-mask {
	position: fixed;
	top: 0;
	left: 0;
	right: 0;
	bottom: 0;
	background: rgba(0, 0, 0, 0.5);
	z-index: 1000;
	display: flex;
	flex-direction: column;
	justify-content: flex-end;
}

.modal-mask {
	justify-content: center;
	align-items: center;
}

.action-sheet {
	background: #f8f8f8;
	border-radius: 32rpx 32rpx 0 0;
	overflow: hidden;
	animation: slideUp 0.3s ease-out;
}

.action-header {
	padding: 32rpx;
	background: #fff;
	text-align: center;
	border-bottom: 2rpx solid #eee;
}

.action-title {
	font-size: 28rpx;
	color: #999;
}

.action-list {
	background: #fff;
}

.action-item {
	height: 100rpx;
	display: flex;
	align-items: center;
	justify-content: center;
	font-size: 32rpx;
	color: #333;
	border-bottom: 2rpx solid #eee;
}

.action-item:active {
	background: #fafafa;
}

.action-cancel {
	margin-top: 16rpx;
	height: 100rpx;
	background: #fff;
	display: flex;
	align-items: center;
	justify-content: center;
	font-size: 32rpx;
	color: #666;
}

.modal-content.position-modal {
	background: #fff;
	width: 90%;
	border-radius: 20rpx;
	padding: 30rpx;
}

.modal-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-bottom: 20rpx;
}

.modal-title {
	font-size: 32rpx;
	font-weight: bold;
}

.close-btn {
	font-size: 40rpx;
	padding: 10rpx;
}

.modal-body {
	padding: 16rpx 0;
}

.level-row {
	display: flex;
	flex-wrap: wrap;
	gap: 16rpx;
	padding: 20rpx 0;
	border-bottom: 1px solid #eee;
}

.position-tag {
	padding: 10rpx 20rpx;
	background: #f5f5f5;
	border-radius: 8rpx;
	font-size: 26rpx;
}

.position-tag.active {
	background: #E6F7FF;
	color: #1890FF;
}

.modal-footer {
	margin-top: 48rpx;
}

@keyframes slideUp {
	from {
		transform: translateY(100%);
	}

	to {
		transform: translateY(0);
	}
}
</style>

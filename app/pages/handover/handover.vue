<template>
	<view class="page-container">

		<!-- Custom Navigation Bar (Fixed) -->
		<view class="custom-nav" :style="{ paddingTop: statusBarHeight * 2 + 'rpx' }">
			<view class="nav-content">
				<view class="nav-left">
					<BackButton class="nav-back-btn" @click="goBack" />
				</view>
				<text class="nav-title">{{ $t('handover.title') }}</text>
				<view class="nav-right"></view>
			</view>
		</view>

		<!-- Main Content -->
		<scroll-view class="scroll-content" scroll-y :enable-back-to-top="true" :refresher-enabled="true"
			:refresher-triggered="isRefreshing" :refresher-threshold="60" @refresherrefresh="onRefresh"
			@refresherpulling="onPulling" @refresherrestore="onRestore" @refresherabort="onRestore"
			@scrolltolower="loadMore" @scroll="onScroll" :scroll-top="scrollTopVal" refresher-default-style="none"
			refresher-background="#f0f5ff"
			:style="{ marginTop: `calc(${statusBarHeight * 2}rpx + 88rpx)`, height: `calc(100vh - ${statusBarHeight * 2}rpx - 88rpx)` }">

			<CustomRefresher slot="refresher" :isRefreshing="isRefreshing" :isPulling="isPulling" />

			<view class="content-wrapper">
				<!-- 1. Statistics Card (Optimized Style) -->
				<view class="stats-card overview-card">
					<view class="stats-row">
						<view class="stat-item">
							<text class="stat-num text-primary">{{ stats.today }}</text>
							<text class="stat-label">{{ $t('handover.today') }}</text>
						</view>
						<view class="stat-divider"></view>
						<view class="stat-item">
							<text class="stat-num text-success">{{ stats.week }}</text>
							<text class="stat-label">{{ $t('handover.week') }}</text>
						</view>
						<view class="stat-divider"></view>
						<view class="stat-item">
							<text class="stat-num text-warning">{{ stats.month }}</text>
							<text class="stat-label">{{ $t('handover.month') }}</text>
						</view>
						<view class="stat-divider"></view>
						<view class="stat-item">
							<text class="stat-num text-danger">{{ stats.missing }}</text>
							<text class="stat-label">{{ $t('handover.missing') }}</text>
						</view>
					</view>
				</view>

				<!-- 2. Calendar -->
				<view class="calendar-section">
					<view class="calendar-header">
						<text class="section-title">📅 {{ currentMonthYear }}</text>
						<picker mode="date" :value="selectedDate" @change="onDateChange">
							<view class="date-picker-btn">
								<text>🗓️</text>
							</view>
						</picker>
					</view>
					<view class="week-row">
						<view v-for="(day, index) in weekDays" :key="index" class="day-item"
							:class="{ active: day.fullDate === selectedDate }" @click="selectDate(day.fullDate)">
							<text class="day-name">{{ day.name }}</text>
							<text class="day-num">{{ day.date }}</text>
							<view class="day-dot" v-if="day.hasData"></view>
						</view>
					</view>
				</view>

				<!-- 3. Controls -->
				<view class="controls-row">
					<view class="start-controls">
						<view class="new-handover-btn" @click="openNewHandoverModal">
							<text class="plus-icon">+</text>
							<text class="btn-text">{{ $t('handover.new') }}</text>
						</view>
					</view>
					<view class="end-controls">
						<view class="icon-btn" @click="handleShare"><text>📤</text></view>
						<view class="icon-btn" @click="handleExport"><text>📑</text></view>
						<view class="icon-btn" @click="handlePrint"><text>🖨️</text></view>
					</view>
				</view>

				<!-- 4. List View -->
				<view class="list-section">
					<view v-if="isLoading" class="loading-state">
						<text class="loading-spinner">↻</text>
						<text>{{ $t('handover.loading') }}</text>
					</view>

					<view v-else-if="handoverList.length === 0" class="empty-state">
						<text class="empty-icon">📭</text>
						<text class="empty-text">{{ $t('handover.empty') }}</text>
					</view>

					<view v-else class="handover-list">
						<view v-for="(item, index) in handoverList" :key="item.id" class="handover-item"
							:class="{ 'new-item': item.isNew }" @click="toCheckPage(item)">
							<!-- Row 1: Time | Dept, Status, Actions -->
							<view class="item-header">
								<text class="item-time">{{ formatTime(item.time) }}</text>
								<view class="item-meta-right">
									<text class="dept-name">{{ item.dept }}</text>

									<!-- Status / Action -->
									<!-- 0: Pending Check -->
									<block v-if="item.status === 0">
										<text class="status-badge warning">{{ $t('handover.status_checking')
										}}</text>
									</block>

									<!-- 1: Pending Confirmation -->
									<block v-else-if="item.status === 1">
										<text v-if="isGiver(item)" class="status-badge pending">
											{{ $t('handover.status_pending') }}
										</text>
										<button v-if="isReceiver(item)" class="receive-btn"
											@click.stop="confirmReceive(item)">
											{{ $t('handover.receive_btn') }}
										</button>
									</block>

									<!-- 10: Finished -->
									<block v-else-if="item.status === 10">
										<text class="status-badge success">
											{{ $t('handover.status_finished') }}
										</text>
									</block>

									<!-- 20: Audited -->
									<block v-else-if="item.status === 20">
										<text class="status-badge normal">
											{{ $t('handover.audited') }}
										</text>
									</block>
									<view class="option-btn" @click.stop="showItemOptions(item)">
										<text class="option-icon">⋮</text>
									</view>
								</view>
							</view>

							<!-- Row 2: People (Giver -> Receiver) No BG -->
							<view class="people-row">
								<view class="person-group">
									<text class="role-label">{{ $t('handover.giver') }}</text>
									<text class="person-name">{{ item.giver }}</text>
								</view>
								<view class="transfer-arrow">➡️</view>
								<view class="person-group">
									<text class="role-label">{{ $t('handover.receiver') }}</text>
									<text class="person-name">{{ item.receiver }}</text>
								</view>
							</view>

							<!-- Row 3: Remark -->
							<view class="remark-row">
								<text class="remark-label">{{ $t('handover.remark') }}:</text>
								<text class="remark-text">{{ item.remark || '-' }}</text>
							</view>

							<!-- Row 4: Audit Info -->
							<view v-if="item.status === 20" class="audit-row">
								<view class="audit-item">
									<text class="audit-label">{{ $t('handover.audit_user') }}:</text>
									<text class="audit-value">{{ item.auditUser }}</text>
								</view>
								<view class="audit-item">
									<text class="audit-label">{{ $t('handover.audit_time') }}:</text>
									<text class="audit-value">{{ item.auditTime }}</text>
								</view>
							</view>
						</view>
					</view>
				</view>

				<FooterInfo :showFinished="handoverList.length > 0" />
				<view class="safe-area-bottom"></view>
			</view>
		</scroll-view>

		<!-- Back to Top -->
		<view class="back-to-top" :class="{ show: showBackToTop }" @click="scrollToTop">
			<text>⬆️</text>
		</view>

		<!-- New Handover Modal -->
		<view class="modal-overlay" v-if="showModal" @click.stop="doNothing" @touchmove.stop.prevent="doNothing">
			<view class="modal-content" @click.stop="doNothing">
				<view class="modal-header">
					<text class="modal-title">{{ formData.relatedName || '' }}{{ $t('handover.title') }}</text>
					<text class="close-btn" @click="closeNewHandoverModal">✖️</text>
				</view>
				<view class="modal-body">
					<view class="form-item">
						<text class="label">部门</text>
						<view class="value disabled">{{ userOrgInfo.Department ? userOrgInfo.Department.Name : '-' }}
						</view>
					</view>
					<view class="form-item">
						<text class="label">时间</text>
						<view class="value">{{ (formData.time || '').substring(0, 16) }}</view>
					</view>
					<view class="form-item">
						<text class="label">{{ $t('handover.shift') }}</text>
						<view class="shift-selector">
							<view v-for="(shift, index) in shifts" :key="index" class="shift-chip"
								:class="{ active: formData.shiftName === shift }" @click="selectShift(shift)">
								{{ shift }}
							</view>
						</view>
					</view>
					<view class="form-item">
						<text class="label">{{ $t('handover.giver') }}</text>
						<view class="value disabled">{{ formData.giver }}</view>
					</view>
					<view class="form-item">
						<text class="label">{{ $t('handover.receiver') }}</text>
						<view class="value disabled" @click="openMemberSelector">
							<text :class="formData.receiver ? 'text-main' : 'text-placeholder'">
								{{ formData.receiver || $t('handover.search_member_placeholder') }}
							</text>
							<text class="picker-arrow">▼</text>
						</view>
						<text v-if="showReceiverError" class="error-msg">{{ $t('handover.select_receiver_error')
						}}</text>
					</view>
				</view>
				<view class="modal-footer">
					<button class="modal-btn cancel" @click="closeNewHandoverModal">{{ $t('handover.cancel')
					}}</button>
					<button class="modal-btn confirm" @click="submitHandover">{{ $t('handover.confirm') }}</button>
				</view>
			</view>
		</view>
		<!-- Member Selector Component -->
		<MemberSelector v-model:visible="showMemberSelector" @select="selectMember" :exclude-ids="[userInfo.UserId]"
			:placeholder="$t('handover.search_member_placeholder')" />
	</view>
</template>

<script>
import FooterInfo from '@/components/FooterInfo/FooterInfo.vue';
import BackButton from '@/components/BackButton/BackButton.vue';
import MemberSelector from '@/components/MemberSelector/MemberSelector.vue';
import CustomRefresher from '@/components/CustomRefresher/CustomRefresher.vue';

import config from '@/config.js';
import request from '@/utils/request.js';
import { storage } from '@/utils/storage.js';
import dayjs from 'dayjs';

export default {
	components: {
		FooterInfo,
		BackButton,
		MemberSelector,
		CustomRefresher
	},
	data() {
		return {
			statusBarHeight: 20,
			isRefreshing: false,
			isPulling: false,
			isLoading: false,
			isSubmitting: false,
			showBackToTop: false,
			scrollTopVal: 0,
			selectedDate: '', // YYYY-MM-DD
			weekDays: [],
			handoverList: [],
			stats: {
				today: 0,
				week: 0,
				month: 0,
				missing: 0
			},
			todayHandoverCount: -1,
			// Modal Data
			showModal: false,
			shifts: ['早班', '中班', '晚班'],
			formData: {
				time: '',
				shiftName: '早班',
				giver: '',
				receiver: '',
				receiverId: null,
				relatedId: 0,
				relatedName: ''
			},
			relatedId: 0,
			relatedName: '',
			userInfo: {},
			deptMembers: [],
			pickerMembers: [], // Keep for reference if needed, but mainly using filtered list now
			currentUser: '',
			userOrgInfo: {},

			// Custom Member Selector
			showMemberSelector: false,
			showReceiverError: false
		};
	},
	computed: {

		currentMonthYear() {
			if (!this.selectedDate) return '2025-12';
			return this.selectedDate.substring(0, 7);
		}
	},
	onLoad(options) {
		if (options.relatedId) {
			this.relatedId = options.relatedId;
		}
		if (options.relatedName) {
			this.relatedName = options.relatedName;
		}

		// Load User Info
		const userOrgInfo = storage.get('userOrgInfo');
		if (userOrgInfo) {
			this.userOrgInfo = userOrgInfo;
			this.userInfo = userOrgInfo.User;
			this.currentUser = userOrgInfo.User.UserName;
		}

		this.initDate();
	},
	onShow() {
		this.fetchData();
		this.checkTodayStatus();
	},
	mounted() {
		const sysInfo = uni.getSystemInfoSync();
		this.statusBarHeight = sysInfo.statusBarHeight || 20;
	},
	methods: {
		goBack() {
			// Use navigateBack for secondary pages
			uni.navigateBack({
				delta: 1,
				fail: () => {
					// Fallback if navigateBack fails (e.g. directly opened)
					uni.reLaunch({ url: '/pages/index/index' });
				}
			});
		},
		initDate() {
			const now = new Date();
			this.selectedDate = this.formatDate(now);
			this.generateWeekDays(now);
		},
		formatDate(date) {
			const y = date.getFullYear();
			const m = String(date.getMonth() + 1).padStart(2, '0');
			const d = String(date.getDate()).padStart(2, '0');
			return `${y}-${m}-${d}`;
		},
		formatTime(timeStr) {
			if (timeStr && timeStr.length > 16) return timeStr.substring(0, 16);
			return timeStr;
		},
		generateWeekDays(refDate) {
			this.weekDays = [];
			const days = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
			const refD = dayjs(refDate);
			let currentDay = refD.day();
			// Start from Sunday of that week
			let startDate = refD.subtract(currentDay, 'day');

			for (let i = 0; i < 7; i++) {
				const d = startDate.add(i, 'day');
				const fDate = d.format('YYYY-MM-DD');
				this.weekDays.push({
					name: days[i],
					date: d.format('DD'),
					fullDate: fDate,
					hasData: fDate === dayjs().format('YYYY-MM-DD') && this.todayHandoverCount === 0
				});
			}
		},
		onDateChange(e) {
			this.selectedDate = e.detail.value;
			this.generateWeekDays(this.selectedDate); // dayjs handles string
			this.fetchData();
		},
		selectDate(date) {
			this.selectedDate = date;
			this.fetchData();
		},
		fetchData() {
			this.isLoading = true;

			const searchOptions = {
				page: 1,
				rows: 50,
				sort: 'ShiftTime',
				order: 'desc',
				wheres: []
			};

			// If selected date is today, we can use the result to update todayHandoverCount
			const isToday = this.selectedDate === this.formatDate(new Date());

			if (this.selectedDate) {
				const start = this.selectedDate + ' 00:00:00';
				const end = dayjs(this.selectedDate).add(1, 'day').format('YYYY-MM-DD') + ' 00:00:00';

				searchOptions.wheres.push({
					name: 'ShiftTime',
					value: start,
					displayType: 'thanorequal'
				});
				searchOptions.wheres.push({
					name: 'ShiftTime',
					value: end,
					displayType: 'lt'
				});
			}
			searchOptions.wheres = JSON.stringify(searchOptions.wheres);

			request.post('api/ssbh/Handover/GetPageData', searchOptions)
				.then(res => {
					const rows = res.rows || [];
					this.handoverList = rows.map(item => ({
						id: item.Id,
						time: item.ShiftTime,
						giver: item.Creator,
						giverId: item.CreatorId,
						receiver: item.ReceiverName,
						receiverId: item.ReceiverId,
						dept: item.DepartmentName || this.userInfo.DepartmentName || '',
						remark: item.Remark,
						status: item.AuditStatus,
						auditUser: item.AuditUser,
						auditTime: item.AuditDate,
						isNew: false
					}));

					if (isToday) {
						this.todayHandoverCount = rows.length; // Approximate using current page length or total
						if (res.total !== undefined) this.todayHandoverCount = res.total;
						this.stats.today = this.todayHandoverCount;
						this.updateWeekDaysDot();
					}

					this.isLoading = false;
					setTimeout(() => {
						this.isRefreshing = false;
						this.isPulling = false;
						uni.showToast({ title: '已刷新', icon: 'none' });
					}, 800);
				})
				.catch(err => {
					console.error("Fetch Data Failed", err);
					this.isLoading = false;
					this.isRefreshing = false;
					this.isPulling = false;
				});
		},
		checkTodayStatus() {
			const today = this.formatDate(new Date());
			// If already selected today, fetchData handles it, but checkTodayStatus ensures it even if looking at other dates
			// However, to avoid double loading on init, rely on fetchData if selectedDate is today.
			// But if looking at history, we still need to know if today is missing.
			if (this.selectedDate === today) return;

			const d = new Date();
			d.setDate(d.getDate() + 1);
			const tmr = this.formatDate(d);

			const searchOptions = {
				page: 1,
				rows: 1, // Minimize data
				wheres: JSON.stringify([
					{ name: 'ShiftTime', value: today + ' 00:00:00', displayType: 'thanorequal' },
					{ name: 'ShiftTime', value: tmr + ' 00:00:00', displayType: 'lt' }
				])
			};

			request.post('api/ssbh/Handover/GetPageData', searchOptions).then(res => {
				const count = res.total !== undefined ? res.total : (res.rows ? res.rows.length : 0);
				this.todayHandoverCount = count;
				this.stats.today = count;
				this.updateWeekDaysDot();
			});
		},
		updateWeekDaysDot() {
			const todayStr = this.formatDate(new Date());
			this.weekDays.forEach(day => {
				// Only show dot if it is today AND count is 0
				day.hasData = (day.fullDate === todayStr && this.todayHandoverCount === 0);
			});
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
			this.fetchData();
		},
		loadMore() {
			// Mock load more
		},
		onScroll(e) {
			this.showBackToTop = e.detail.scrollTop > 300;
		},
		scrollToTop() {
			this.scrollTopVal = 0;
			setTimeout(() => { this.scrollTopVal = -1; }, 50);
		},

		// Actions
		handleShare() { uni.showToast({ title: 'Shared', icon: 'none' }); },
		handleExport() { uni.showToast({ title: 'Exported', icon: 'none' }); },
		handlePrint() { uni.showToast({ title: 'Printing...', icon: 'none' }); },

		// Modal Logic
		openNewHandoverModal() {
			const now = new Date();
			const y = now.getFullYear();
			const m = String(now.getMonth() + 1).padStart(2, '0');
			const d = String(now.getDate()).padStart(2, '0');
			const hh = String(now.getHours()).padStart(2, '0');
			const mm = String(now.getMinutes()).padStart(2, '0');
			const ss = String(now.getSeconds()).padStart(2, '0');
			const formattedTime = `${y}-${m}-${d} ${hh}:${mm}:${ss}`;

			this.formData = {
				time: formattedTime,
				shiftName: this.shifts[0],
				giver: this.currentUser,
				receiver: '',
				receiverId: null,
				relatedId: this.relatedId,
				relatedName: this.relatedName
			};
			this.showReceiverError = false;
			this.showModal = true;
		},
		closeNewHandoverModal() {
			this.showModal = false;
		},
		selectShift(shift) {
			this.formData.shiftName = shift;
		},
		onReceiverChange(e) {
			// Deprecated picker logic
		},
		openMemberSelector() {
			this.showMemberSelector = true;
			this.memberSearchKey = '';
		},
		closeMemberSelector() {
			this.showMemberSelector = false;
		},
		selectMember(m) {
			this.formData.receiver = m.label;
			this.formData.receiverId = m.value;
			this.showReceiverError = false;
			this.closeMemberSelector();
		},


		onShiftChange(e) {
			this.formData.shiftName = this.shifts[e.detail.value];
		},
		submitHandover() {
			if (!this.formData.giver) {
				uni.showToast({ title: 'Missing Giver Info', icon: 'none' });
				return;
			}
			if (!this.formData.receiver || !this.formData.receiverId) {
				this.showReceiverError = true;
				return;
			}

			this.closeNewHandoverModal();
			uni.showLoading({ title: this.$t('handover.loading'), mask: true });
			this.isSubmitting = true;

			// Prepare API Payload
			const payload = {
				OrganizationId: this.userInfo.OrganizationId,
				DepartmentId: this.userInfo.DepartmentId,
				Category: 10, //抢救车交接班
				RelatedId: this.formData.relatedId || 0, //抢救车编号
				ShiftName: this.formData.shiftName,
				ShiftTime: this.formData.time,
				ReceiverId: this.formData.receiverId,
				ReceiverName: this.formData.receiver,
				Remark: '' // Optional initial remark
			};

			const postData = {
				mainData: payload,
				detailData: null,
				delKeys: null
			};

			request.post('api/ssbh/Handover/Add', postData)
				.then(res => {
					uni.hideLoading();
					this.isSubmitting = false;

					if (res.status) {
						// Success - Navigate to Check Page
						const newItem = JSON.parse(res.data).data;
						uni.navigateTo({
							url: '/pages/handover-check/handover-item-check?id=' + newItem.Id,
							events: {
								handoverFinished: (data) => {
									this.handoverList.unshift(data);
									uni.showToast({ title: this.$t('handover.submit_success') });
								}
							},
							success: (navRes) => {
								navRes.eventChannel.emit('acceptHandoverData', {
									data: newItem
								});
							}
						});
						// Update stats immediately if we just added one for today
						if (this.formatDate(new Date(payload.ShiftTime)) === this.formatDate(new Date())) {
							this.todayHandoverCount++;
							this.stats.today = this.todayHandoverCount;
							this.updateWeekDaysDot();
						}
					} else {
						uni.showToast({ title: res.message || 'Submission Failed', icon: 'none' });
						if (res.message !== '重复交接班') {
							this.showModal = true;
						}
					}
				})
				.catch(err => {
					uni.hideLoading();
					this.isSubmitting = false;
					console.error("Submit Handover Failed", err);
					uni.showToast({ title: 'Network Error', icon: 'none' });
				});
		},
		isGiver(item) {
			return item.giver === this.currentUser;
		},
		isReceiver(item) {
			return item.giver !== this.currentUser;
		},
		confirmReceive(item) {
			uni.navigateTo({
				url: `/pages/handover-check/handover-item-check?id=${item.id}&todoListId=${this.relatedId}`
			});
		},
		toCheckPage(item) {
			console.log(item);
			// Navigate to Check Page with ID and Status
			uni.navigateTo({
				url: `/pages/handover-check/handover-item-check?id=${item.id}&status=${item.status}`,
				events: {
					handoverFinished: (data) => {
						// Update local list if needed
						const idx = this.handoverList.findIndex(i => i.id === data.id);
						if (idx > -1) {
							this.handoverList.splice(idx, 1, { ...this.handoverList[idx], ...data });
						} else {
							this.handoverList.unshift(data);
						}
					}
				},
				success: (navRes) => {
					navRes.eventChannel.emit('acceptHandoverData', {
						data: item
					});
				}
			});
		},
		showItemOptions(item) {
			uni.showActionSheet({
				itemList: [this.$t('handover.delete')],
				itemColor: '#ff4d4f',
				success: (res) => {
					if (res.tapIndex === 0) {
						this.deleteItem(item);
					}
				}
			});
		},
		deleteItem(item) {
			console.log(item);
			uni.showModal({
				title: this.$t('handover.delete'),
				content: this.$t('handover.delete_confirm'),
				success: (res) => {
					if (res.confirm) {
						uni.showLoading({ title: this.$t('handover.loading'), mask: true });
						request.post('api/ssbh/Handover/Del', [item.id]).then(res => {
							uni.hideLoading();
							if (res.success || res.status) {
								uni.showToast({ title: this.$t('handover.delete_success') });
								this.fetchData(); // Refresh list
							} else {
								uni.showToast({ title: res.msg || 'Delete Failed', icon: 'none' });
							}
						}).catch(err => {
							uni.hideLoading();
							console.error(err);
							uni.showToast({ title: 'Error', icon: 'none' });
						});
					}
				}
			});
		},
		doNothing() { }
	}
}

</script>

<style lang="scss" scoped>
.page-container {
	background-color: $sull-color-background;
	min-height: 100vh;
	display: flex;
	flex-direction: column;
	font-family: $font-family-main;
}

.custom-nav {
	background-color: $sull-color-white;
	position: fixed;
	top: 0;
	left: 0;
	width: 100%;
	z-index: 999;
	box-shadow: 0 2rpx 8rpx rgba(0, 0, 0, 0.03);

	.nav-content {
		height: 88rpx;
		display: flex;
		align-items: center;
		justify-content: space-between;
		padding: 0 32rpx;

		.nav-left {
			width: 80rpx;
			display: flex;
			align-items: center;
		}

		.nav-title {
			font-size: $font-size-h6;
			font-weight: $font-weight-semibold;
			color: $sull-text-main;
		}

		.nav-right {
			width: 80rpx;
		}
	}
}

.scroll-content {
	flex: 1;
	height: 100vh;
}

.content-wrapper {
	padding: 32rpx;
	padding-bottom: 80rpx;
}

// 1. Stats Card - Styled like Index Overview
.overview-card {
	background: $sull-color-white;
	border-radius: $radius-lg;
	padding: 40rpx;
	margin-bottom: 40rpx;
	box-shadow: 0 4rpx 20rpx rgba(0, 0, 0, 0.03);

	.stats-row {
		display: flex;
		justify-content: space-between;
		align-items: center;

		.stat-item {
			flex: 1;
			display: flex;
			flex-direction: column;
			align-items: center;

			.stat-num {
				font-size: 48rpx;
				font-weight: $font-weight-bold;
				margin-bottom: 8rpx;
				line-height: 1.2;
			}

			.stat-label {
				font-size: $font-size-body-sm;
				color: $sull-text-secondary;
			}

			.text-primary {
				color: $sull-color-primary;
			}

			.text-success {
				color: $sull-color-success;
			}

			.text-warning {
				color: $sull-color-warning;
			}

			.text-danger {
				color: $sull-color-error;
			}
		}

		.stat-divider {
			width: 2rpx;
			height: 60rpx;
			background: $sull-color-line;
		}
	}
}

// 2. Calendar Section
.calendar-section {
	background: $sull-color-white;
	border-radius: $radius-lg;
	padding: 32rpx;
	margin-bottom: 40rpx;
	box-shadow: 0 4rpx 16rpx rgba(0, 0, 0, 0.02);

	.calendar-header {
		display: flex;
		justify-content: space-between;
		align-items: center;
		margin-bottom: 24rpx;

		.section-title {
			font-size: $font-size-body-lg;
			font-weight: $font-weight-semibold;
			color: $sull-text-main;
		}

		.date-picker-btn {
			padding: 8rpx 16rpx;
			background: #F5F7FA;
			border-radius: 12rpx;
		}
	}

	.week-row {
		display: flex;
		justify-content: space-between;
		width: 100%;
		gap: 8rpx;

		.day-item {
			flex: 1;
			display: flex;
			flex-direction: column;
			align-items: center;
			padding: 16rpx 0;
			border-radius: $radius-sm;
			min-width: 0;
			box-sizing: border-box;

			&.active {
				background-color: $sull-color-primary;

				.day-name,
				.day-num {
					color: $sull-color-white;
				}
			}

			.day-name {
				font-size: 22rpx;
				color: $sull-text-secondary;
				margin-bottom: 8rpx;
			}

			.day-num {
				font-size: 30rpx;
				font-weight: $font-weight-semibold;
				color: $sull-text-main;
			}

			.day-dot {
				width: 8rpx;
				height: 8rpx;
				background-color: $sull-color-error;
				border-radius: 50%;
				margin-top: 8rpx;
			}
		}
	}
}

// 3. Controls Row
.controls-row {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-bottom: 32rpx;

	.new-handover-btn {
		display: flex;
		align-items: center;
		justify-content: center;
		background: $sull-color-primary; // Use flat color or define gradient in sull.scss if needed. Keeping simple primary for now based on request.
		color: $sull-color-white;
		font-size: $font-size-body-md;
		padding: 0 32rpx;
		height: 72rpx;
		border-radius: 36rpx;
		box-shadow: 0 8rpx 16rpx rgba(149, 202, 255, 0.4); // Matching primary color shadow roughly
		transition: all 0.2s;

		.plus-icon {
			font-size: 40rpx;
			margin-right: 8rpx;
			line-height: 1;
			font-weight: 400;
			margin-top: -4rpx;
		}

		.btn-text {
			font-weight: $font-weight-medium;
		}

		&:active {
			transform: scale(0.96);
			box-shadow: 0 4rpx 8rpx rgba(149, 202, 255, 0.3);
		}
	}

	.end-controls {
		display: flex;
		gap: 24rpx;

		.icon-btn {
			width: 72rpx;
			height: 72rpx;
			background: $sull-color-white;
			border-radius: 50%;
			display: flex;
			align-items: center;
			justify-content: center;
			box-shadow: 0 4rpx 12rpx rgba(0, 0, 0, 0.05);
			font-size: 36rpx;

			&:active {
				transform: scale(0.95);
			}
		}
	}
}

// 4. List Section
.list-section {
	min-height: 400rpx;

	.loading-state,
	.empty-state {
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		padding: 80rpx 0;
		color: $sull-text-secondary;

		.loading-spinner,
		.empty-icon {
			font-size: 64rpx;
			margin-bottom: 24rpx;
		}

		.empty-text {
			font-size: $font-size-body-md;
		}
	}

	.handover-item {
		background: $sull-color-white;
		border-radius: $radius-md;
		padding: 32rpx;
		margin-bottom: 24rpx;
		box-shadow: 0 4rpx 16rpx rgba(0, 0, 0, 0.02);
		border-left: 8rpx solid transparent;

		&.new-item {
			border-left-color: $sull-color-primary;
			background: #F4F9FF; // Consider adding specific light bg to var if used often
		}

		.item-header {
			display: flex;
			justify-content: space-between;
			align-items: center;
			margin-bottom: 32rpx;

			.item-time {
				font-size: 30rpx;
				font-weight: $font-weight-semibold;
				color: $sull-text-main;
			}

			.item-meta-right {
				display: flex;
				align-items: center;
				gap: 16rpx;

				.dept-name {
					font-size: 24rpx;
					color: $sull-text-secondary;
					padding: 4rpx 12rpx;
					background: $sull-color-background;
					border-radius: 8rpx;
				}

				.status-badge {
					font-size: 22rpx;
					padding: 8rpx 16rpx;
					border-radius: 8rpx;

					&.pending {
						background: #FFF0E0;
						color: $sull-color-warning;
					}

					&.warning {
						background: #FDF6EC;
						color: $sull-color-warning;
						border: 2rpx solid #FAECD8;
					}

					&.success {
						background: #F0F9EB;
						color: $sull-color-success;
					}

					&.normal {
						background: #E6F7FF;
						color: $sull-color-primary;
					}
				}

				.receive-btn {
					font-size: 24rpx;
					padding: 0 24rpx;
					height: 48rpx;
					line-height: 48rpx;
					background: $sull-color-success;
					color: $sull-color-white;
					border-radius: 24rpx;
					margin: 0;
				}
			}

			.option-btn {
				padding: 0 16rpx;
				height: 48rpx;
				display: flex;
				align-items: center;
				justify-content: center;

				.option-icon {
					font-size: 40rpx;
					color: #999;
					font-weight: bold;
				}
			}
		}

		.people-row {
			display: flex;
			align-items: center;
			justify-content: center;
			/* Center aligned */
			margin-bottom: 32rpx;
			gap: 40rpx;

			.person-group {
				display: flex;
				flex-direction: column;
				align-items: center;

				.role-label {
					font-size: 20rpx;
					color: $sull-text-secondary;
					margin-bottom: 4rpx;
				}

				.person-name {
					font-size: 32rpx;
					color: $sull-text-main;
					font-weight: $font-weight-medium;
				}
			}

			.transfer-arrow {
				color: $sull-text-secondary;
				font-size: 28rpx;
				margin-top: 20rpx; // align visually with names
			}
		}

		.remark-row {
			font-size: 26rpx;
			color: $sull-text-secondary;
			padding-top: 24rpx;
			border-top: 2rpx solid $sull-color-line;

			.remark-label {
				color: $sull-text-placeholder;
				margin-right: 8rpx;
			}
		}

		.audit-row {
			font-size: 26rpx;
			color: $sull-text-secondary;
			padding-top: 16rpx;
			margin-top: 16rpx;
			border-top: 2rpx dashed $sull-color-line;
			display: flex;
			justify-content: space-between;

			.audit-item {
				display: flex;
				align-items: center;
			}

			.audit-label {
				color: $sull-text-placeholder;
				margin-right: 8rpx;
			}

			.audit-value {
				color: $sull-text-main;
			}
		}
	}
}

// Back to Top
.back-to-top {
	position: fixed;
	bottom: 160rpx;
	right: 40rpx;
	width: 88rpx;
	height: 88rpx;
	background: $sull-color-white;
	border-radius: 50%;
	display: flex;
	align-items: center;
	justify-content: center;
	box-shadow: 0 8rpx 24rpx rgba(0, 0, 0, 0.15);
	opacity: 0;
	pointer-events: none;
	transition: opacity 0.3s;
	z-index: 900;
	font-size: 40rpx;

	&.show {
		opacity: 1;
		pointer-events: auto;
	}
}

// Modal
.modal-overlay {
	position: fixed;
	top: 0;
	left: 0;
	width: 100%;
	height: 100%;
	background: rgba(0, 0, 0, 0.5);
	z-index: 2000;
	display: flex;
	align-items: center;
	justify-content: center;
	backdrop-filter: blur(4rpx);

	.modal-content {
		width: 85%;
		background: $sull-color-white;
		border-radius: $radius-lg;
		// Removed overflow: hidden to allow picker to escape if needed, though mostly it's internal. 
		// But if the modal body is small, overflow hidden clips content.
		display: flex;
		flex-direction: column;
		max-height: 80vh;
		animation: slideUp 0.3s ease;

		.modal-header {
			padding: 32rpx;
			border-bottom: 2rpx solid $sull-color-line;
			display: flex;
			justify-content: space-between;
			align-items: center;

			.modal-title {
				font-size: $font-size-h6;
				font-weight: bold;
			}

			.close-btn {
				padding: 8rpx;
				font-size: 36rpx;
				color: $sull-text-secondary;
			}
		}

		.modal-body {
			padding: 40rpx;
			overflow-y: auto; // Allow scroll if content is long

			.form-item {
				margin-bottom: 40rpx;
				display: flex;
				flex-direction: column;

				.label {
					font-size: $font-size-body-sm;
					color: $sull-text-secondary;
					margin-bottom: 16rpx;
					display: block;
				}

				.error-msg {
					font-size: 24rpx;
					color: $sull-color-error;
					margin-top: 8rpx;
				}

				.value {
					font-size: $font-size-body-md;
					font-weight: 500;
					color: $sull-text-main;
					padding: 20rpx 0;
					border-bottom: 2rpx solid #eee;
				}

				.value.disabled {
					color: $sull-text-secondary;
					background: #f9f9f9;
					padding-left: 16rpx;
					border-radius: 8rpx;
					border-bottom: none;

					/* Added for Receiver layout */
					display: flex;
					justify-content: space-between;
					align-items: center;
					padding-right: 12rpx;
				}

				.value.disabled .text-main {
					color: $sull-text-secondary;
					/* Match parent/disabled color */
					font-weight: 500;
				}

				.value-picker {
					width: 100%;
				}

				.picker-inner {
					display: flex;
					justify-content: space-between;
					align-items: center;
					padding: 20rpx 0;
					border-bottom: 2rpx solid #eee;
					width: 100%;
					font-size: $font-size-body-lg;
					color: $sull-text-main;
				}

				.text-main {
					color: $sull-text-main;
					font-weight: 500;
				}

				.text-placeholder {
					color: $sull-text-placeholder;
				}

				.picker-arrow {
					font-size: 24rpx;
					color: $sull-text-secondary;
				}

				.shift-selector {
					display: flex;
					gap: 24rpx;

					.shift-chip {
						flex: 1;
						text-align: center;
						padding: 20rpx 0;
						background: #F5F7FA;
						color: $sull-text-secondary;
						border-radius: 16rpx;
						font-size: 28rpx;
						border: 2rpx solid transparent;
						transition: all 0.2s;

						&.active {
							background: #EAF2FF; // Keep this specific shade for now or find light variant variable
							color: $sull-color-primary;
							border-color: $sull-color-primary;
							font-weight: $font-weight-medium;
						}
					}
				}

				.picker-inner {
					display: flex;
					justify-content: space-between;
					align-items: center;
					width: 100%;
					font-size: $font-size-body-lg;
					color: $sull-text-main;
				}
			}
		}

		.modal-footer {
			display: flex;
			border-top: 2rpx solid $sull-color-line;

			.modal-btn {
				flex: 1;
				border: none;
				background: #fff;
				border-radius: 0;
				padding: 28rpx 0;
				font-size: $font-size-body-md;
				line-height: normal;

				&.cancel {
					color: $sull-text-secondary;
					border-right: 2rpx solid $sull-color-line;
					border-bottom-left-radius: 32rpx;
				}

				&.confirm {
					color: $sull-color-primary;
					font-weight: $font-weight-medium;
					border-bottom-right-radius: 32rpx;
				}

				&:active {
					background: #F9F9F9;
				}
			}
		}
	}
}

@keyframes slideUp {
	from {
		transform: translateY(40rpx);
		opacity: 0;
	}

	to {
		transform: translateY(0);
		opacity: 1;
	}
}

.safe-area-bottom {
	height: env(safe-area-inset-bottom);
}



/* .picker-trigger removed as per request */

.picker-arrow {
	/* margin-right removed, spacing handled by container padding */
	font-size: 24rpx;
	color: $sull-text-secondary;
}

.custom-nav {
	background-color: $sull-color-white;
	/* Removed fixed position to scroll with content */
	/* position: fixed; */
	/* top: 0; */
	/* left: 0; */
	width: 100%;
	z-index: 999;
	/* box-shadow: 0 2rpx 8rpx rgba(0, 0, 0, 0.03); */
	/* Removed shadow to blend better, or keep if preferred */

	.nav-content {
		height: 88rpx;
		display: flex;
		align-items: center;
		justify-content: space-between;
		padding: 0 32rpx;

		.nav-left {
			width: 80rpx;
			display: flex;
			align-items: center;
		}

		.nav-title {
			font-size: $font-size-h6;
			font-weight: $font-weight-semibold;
			color: $sull-text-main;
		}

		.nav-right {
			width: 80rpx;
		}
	}
}

.scroll-content {
	flex: 1;
	height: 100vh;
}

// ... existing styles ...

/* .picker-arrow {
...
} */
/* Ensure all previous styles are kept or this block replaces only needed parts. 
Since I cannot easily see all lines, I will append the NEW styles and rely on the fact that I am modifying .custom-nav which is near the top of style block.
Wait, replace_file_content replaces a block. 
I should target .custom-nav block specifically.
*/
</style>

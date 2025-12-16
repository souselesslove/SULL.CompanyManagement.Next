<template>
	<view class="page-container">
		<!-- Custom Navigation Bar -->
		<view class="custom-nav" :style="{ paddingTop: statusBarHeight + 'px' }">
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
			:refresher-triggered="isRefreshing" @refresherrefresh="onRefresh" @scrolltolower="loadMore"
			@scroll="onScroll" :scroll-top="scrollTopVal" :style="{ paddingTop: (statusBarHeight + 44) + 'px' }">

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
										<text class="status-badge warning">{{ $t('handover.status_checking') }}</text>
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

				<FooterInfo />
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
					<button class="modal-btn cancel" @click="closeNewHandoverModal">{{ $t('handover.cancel') }}</button>
					<button class="modal-btn confirm" @click="submitHandover">{{ $t('handover.confirm') }}</button>
				</view>
			</view>
		</view>
		<view class="member-overlay" v-if="showMemberSelector" @click.stop="doNothing"
			@touchmove.stop.prevent="doNothing">
			<view class="member-modal" @click.stop="doNothing">
				<view class="member-header">
					<view class="search-box">
						<input class="member-search-input" v-model="memberSearchKey"
							:placeholder="$t('handover.search_member_placeholder')" />
						<text v-if="memberSearchKey" class="clear-icon" @click="clearSearch">✖</text>
					</view>
					<text class="close-btn" @click="closeMemberSelector">✖️</text>
				</view>
				<scroll-view scroll-y class="member-list">
					<view class="member-item" v-for="(m, i) in filteredMemberSelectorList" :key="i"
						@click="selectMember(m)">
						<text class="member-name">{{ m.label }}</text>
					</view>
					<view v-if="filteredMemberSelectorList.length === 0" class="empty-state-small">
						<text>No Result</text>
					</view>
				</scroll-view>
			</view>
		</view>
	</view>
</template>

<script>
import FooterInfo from '@/components/FooterInfo/FooterInfo.vue';
import BackButton from '@/components/BackButton/BackButton.vue';
import request from '@/utils/request.js';

export default {
	components: {
		FooterInfo,
		BackButton
	},
	data() {
		return {
			statusBarHeight: 20,
			isRefreshing: false,
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

			// Custom Member Selector
			showMemberSelector: false,
			memberSearchKey: '',
			showReceiverError: false
		};
	},
	computed: {
		filteredMemberSelectorList() {
			if (!this.memberSearchKey) return this.pickerMembers;
			const key = this.memberSearchKey.toLowerCase();
			return this.pickerMembers.filter(m => m.label.toLowerCase().includes(key));
		},
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
		const uInfo = uni.getStorageSync('userOrgInfo');
		if (uInfo) {
			this.userInfo = uInfo.User;
			this.currentUser = uInfo.User.UserName;
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
		this.fetchDeptMembers();
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
			let currentDay = refDate.getDay();
			let startDate = new Date(refDate);
			startDate.setDate(refDate.getDate() - currentDay);

			for (let i = 0; i < 7; i++) {
				let d = new Date(startDate);
				d.setDate(startDate.getDate() + i);
				const fDate = this.formatDate(d);
				this.weekDays.push({
					name: days[i],
					date: String(d.getDate()).padStart(2, '0'),
					fullDate: fDate,
					hasData: fDate === this.formatDate(new Date()) && this.todayHandoverCount === 0
				});
			}
		},
		onDateChange(e) {
			this.selectedDate = e.detail.value;
			this.generateWeekDays(new Date(this.selectedDate));
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
				const d = new Date(this.selectedDate);
				d.setDate(d.getDate() + 1);
				const y = d.getFullYear();
				const m = String(d.getMonth() + 1).padStart(2, '0');
				const day = String(d.getDate()).padStart(2, '0');
				const end = `${y}-${m}-${day} 00:00:00`;

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
					this.isRefreshing = false;
				})
				.catch(err => {
					console.error("Fetch Data Failed", err);
					this.isLoading = false;
					this.isRefreshing = false;
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
		onRefresh() {
			this.isRefreshing = true;
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
		clearSearch() {
			this.memberSearchKey = '';
		},
		fetchDeptMembers() {
			request.get('api/ssbh/OrganizationUser/DepartmentUser')
				.then(res => {
					const list = res.rows || [];
					const currentId = this.userInfo.UserId;
					let filtered = list;
					if (currentId) {
						filtered = list.filter(m => m.UserId != currentId);
					}
					this.deptMembers = filtered;
					this.pickerMembers = filtered.map(m => ({
						label: m.UserTrueName || m.userTrueName || m.UserName || m.userName || 'Unknown',
						value: m.UserId || m.userId
					}));
				})
				.catch(err => {
					console.error("Fetch Members Failed", err);
					this.pickerMembers = [];
				});
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
			item.status = 'normal';
			uni.showToast({ title: this.$t('handover.receive_success') });
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
			uni.showModal({
				title: this.$t('handover.delete'),
				content: this.$t('handover.delete_confirm'),
				success: (res) => {
					if (res.confirm) {
						uni.showLoading({ title: this.$t('handover.loading'), mask: true });
						request.post('api/ssbh/Handover/Del', [item.Id]).then(res => {
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
	box-shadow: 0 1px 4px rgba(0, 0, 0, 0.03);

	.nav-content {
		height: 44px;
		display: flex;
		align-items: center;
		justify-content: space-between;
		padding: 0 16px;

		.nav-left {
			width: 40px;
			display: flex;
			align-items: center;
		}

		.nav-title {
			font-size: $font-size-h6;
			font-weight: $font-weight-semibold;
			color: $sull-text-main;
		}

		.nav-right {
			width: 40px;
		}
	}
}

.scroll-content {
	flex: 1;
	height: 100vh;
}

.content-wrapper {
	padding: 16px;
	padding-bottom: 40px;
}

// 1. Stats Card - Styled like Index Overview
.overview-card {
	background: $sull-color-white;
	border-radius: $radius-lg;
	padding: 20px;
	margin-bottom: 20px;
	box-shadow: 0 2px 10px rgba(0, 0, 0, 0.03);

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
				font-size: 24px;
				font-weight: $font-weight-bold;
				margin-bottom: 4px;
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
			width: 1px;
			height: 30px;
			background: $sull-color-line;
		}
	}
}

// 2. Calendar Section
.calendar-section {
	background: $sull-color-white;
	border-radius: $radius-lg;
	padding: 16px;
	margin-bottom: 20px;
	box-shadow: 0 2px 8px rgba(0, 0, 0, 0.02);

	.calendar-header {
		display: flex;
		justify-content: space-between;
		align-items: center;
		margin-bottom: 12px;

		.section-title {
			font-size: $font-size-body-lg;
			font-weight: $font-weight-semibold;
			color: $sull-text-main;
		}

		.date-picker-btn {
			padding: 4px 8px;
			background: #F5F7FA;
			border-radius: 6px;
		}
	}

	.week-row {
		display: flex;
		justify-content: space-between;

		.day-item {
			display: flex;
			flex-direction: column;
			align-items: center;
			padding: 8px 6px;
			border-radius: $radius-sm;
			min-width: 36px;

			&.active {
				background-color: $sull-color-primary;

				.day-name,
				.day-num {
					color: $sull-color-white;
				}
			}

			.day-name {
				font-size: 11px;
				color: $sull-text-secondary;
				margin-bottom: 4px;
			}

			.day-num {
				font-size: 15px;
				font-weight: $font-weight-semibold;
				color: $sull-text-main;
			}

			.day-dot {
				width: 4px;
				height: 4px;
				background-color: $sull-color-error;
				border-radius: 50%;
				margin-top: 4px;
			}
		}
	}
}

// 3. Controls Row
.controls-row {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-bottom: 16px;

	.new-handover-btn {
		display: flex;
		align-items: center;
		justify-content: center;
		background: $sull-color-primary; // Use flat color or define gradient in sull.scss if needed. Keeping simple primary for now based on request.
		color: $sull-color-white;
		font-size: $font-size-body-md;
		padding: 0 16px;
		height: 36px;
		border-radius: 18px;
		box-shadow: 0 4px 8px rgba(149, 202, 255, 0.4); // Matching primary color shadow roughly
		transition: all 0.2s;

		.plus-icon {
			font-size: 20px;
			margin-right: 4px;
			line-height: 1;
			font-weight: 400;
			margin-top: -2px;
		}

		.btn-text {
			font-weight: $font-weight-medium;
		}

		&:active {
			transform: scale(0.96);
			box-shadow: 0 2px 4px rgba(149, 202, 255, 0.3);
		}
	}

	.end-controls {
		display: flex;
		gap: 12px;

		.icon-btn {
			width: 36px;
			height: 36px;
			background: $sull-color-white;
			border-radius: 50%;
			display: flex;
			align-items: center;
			justify-content: center;
			box-shadow: 0 2px 6px rgba(0, 0, 0, 0.05);
			font-size: 18px;

			&:active {
				transform: scale(0.95);
			}
		}
	}
}

// 4. List Section
.list-section {
	min-height: 200px;

	.loading-state,
	.empty-state {
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		padding: 40px 0;
		color: $sull-text-secondary;

		.loading-spinner,
		.empty-icon {
			font-size: 32px;
			margin-bottom: 12px;
		}

		.empty-text {
			font-size: $font-size-body-md;
		}
	}

	.handover-item {
		background: $sull-color-white;
		border-radius: $radius-md;
		padding: 16px;
		margin-bottom: 12px;
		box-shadow: 0 2px 8px rgba(0, 0, 0, 0.02);
		border-left: 4px solid transparent;

		&.new-item {
			border-left-color: $sull-color-primary;
			background: #F4F9FF; // Consider adding specific light bg to var if used often
		}

		.item-header {
			display: flex;
			justify-content: space-between;
			align-items: center;
			margin-bottom: 16px;

			.item-time {
				font-size: 15px;
				font-weight: $font-weight-semibold;
				color: $sull-text-main;
			}

			.item-meta-right {
				display: flex;
				align-items: center;
				gap: 8px;

				.dept-name {
					font-size: 12px;
					color: $sull-text-secondary;
					padding: 2px 6px;
					background: $sull-color-background;
					border-radius: 4px;
				}

				.status-badge {
					font-size: 11px;
					padding: 4px 8px;
					border-radius: 4px;

					&.pending {
						background: #FFF0E0;
						color: $sull-color-warning;
					}

					&.warning {
						background: #FDF6EC;
						color: $sull-color-warning;
						border: 1px solid #FAECD8;
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
					font-size: 12px;
					padding: 0 12px;
					height: 24px;
					line-height: 24px;
					background: $sull-color-success;
					color: $sull-color-white;
					border-radius: 12px;
					margin: 0;
				}
			}

			.option-btn {
				padding: 0 8px;
				height: 24px;
				display: flex;
				align-items: center;
				justify-content: center;

				.option-icon {
					font-size: 20px;
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
			margin-bottom: 16px;
			gap: 20px;

			.person-group {
				display: flex;
				flex-direction: column;
				align-items: center;

				.role-label {
					font-size: 10px;
					color: $sull-text-secondary;
					margin-bottom: 2px;
				}

				.person-name {
					font-size: 16px;
					color: $sull-text-main;
					font-weight: $font-weight-medium;
				}
			}

			.transfer-arrow {
				color: $sull-text-secondary;
				font-size: 14px;
				margin-top: 10px; // align visually with names
			}
		}

		.remark-row {
			font-size: 13px;
			color: $sull-text-secondary;
			padding-top: 12px;
			border-top: 1px solid $sull-color-line;

			.remark-label {
				color: $sull-text-placeholder;
				margin-right: 4px;
			}
		}

		.audit-row {
			font-size: 13px;
			color: $sull-text-secondary;
			padding-top: 8px;
			margin-top: 8px;
			border-top: 1px dashed $sull-color-line;
			display: flex;
			justify-content: space-between;

			.audit-item {
				display: flex;
				align-items: center;
			}

			.audit-label {
				color: $sull-text-placeholder;
				margin-right: 4px;
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
	bottom: 80px;
	right: 20px;
	width: 44px;
	height: 44px;
	background: $sull-color-white;
	border-radius: 50%;
	display: flex;
	align-items: center;
	justify-content: center;
	box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
	opacity: 0;
	pointer-events: none;
	transition: opacity 0.3s;
	z-index: 900;
	font-size: 20px;

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
	backdrop-filter: blur(2px);

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
			padding: 16px;
			border-bottom: 1px solid $sull-color-line;
			display: flex;
			justify-content: space-between;
			align-items: center;

			.modal-title {
				font-size: $font-size-h6;
				font-weight: bold;
			}

			.close-btn {
				padding: 4px;
				font-size: 18px;
				color: $sull-text-secondary;
			}
		}

		.modal-body {
			padding: 20px;
			overflow-y: auto; // Allow scroll if content is long

			.form-item {
				margin-bottom: 20px;
				display: flex;
				flex-direction: column;

				.label {
					font-size: $font-size-body-sm;
					color: $sull-text-secondary;
					margin-bottom: 8px;
					display: block;
				}

				.error-msg {
					font-size: 12px;
					color: $sull-color-error;
					margin-top: 4px;
				}

				.value {
					font-size: $font-size-body-md;
					font-weight: 500;
					color: $sull-text-main;
					padding: 10px 0;
					border-bottom: 1px solid #eee;
				}

				.value.disabled {
					color: $sull-text-secondary;
					background: #f9f9f9;
					padding-left: 8px;
					border-radius: 4px;
					border-bottom: none;

					/* Added for Receiver layout */
					display: flex;
					justify-content: space-between;
					align-items: center;
					padding-right: 6px;
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
					padding: 10px 0;
					border-bottom: 1px solid #eee;
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
					font-size: 12px;
					color: $sull-text-secondary;
				}

				.shift-selector {
					display: flex;
					gap: 12px;

					.shift-chip {
						flex: 1;
						text-align: center;
						padding: 10px 0;
						background: #F5F7FA;
						color: $sull-text-secondary;
						border-radius: 8px;
						font-size: 14px;
						border: 1px solid transparent;
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
			border-top: 1px solid $sull-color-line;

			.modal-btn {
				flex: 1;
				border: none;
				background: #fff;
				border-radius: 0;
				padding: 14px 0;
				font-size: $font-size-body-md;
				line-height: normal;

				&.cancel {
					color: $sull-text-secondary;
					border-right: 1px solid $sull-color-line;
					border-bottom-left-radius: 16px;
				}

				&.confirm {
					color: $sull-color-primary;
					font-weight: $font-weight-medium;
					border-bottom-right-radius: 16px;
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
		transform: translateY(20px);
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

/* Member Selector Custom Modal */
.member-overlay {
	position: fixed;
	top: 0;
	left: 0;
	width: 100%;
	height: 100%;
	background: rgba(0, 0, 0, 0.5);
	z-index: 3000;
	/* Higher than main modal */
	display: flex;
	align-items: flex-end;
	/* Bottom sheet style or center */
	justify-content: center;
	backdrop-filter: blur(2px);
}

.member-modal {
	width: 100%;
	background: $sull-color-white;
	border-radius: 20px 20px 0 0;
	padding: 20px;
	padding-bottom: 40px;
	display: flex;
	flex-direction: column;
	max-height: 60vh;
	animation: slideUp 0.3s ease;
}

.member-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-bottom: 16px;
	gap: 12px;
}

.member-search-input {
	flex: 1;
	height: 40px;
	background: #F5F7FA;
	border-radius: 20px;
	padding: 0 36px 0 16px;
	/* Right padding for clear icon */
	font-size: 14px;
}

.search-box {
	flex: 1;
	position: relative;
	display: flex;
	align-items: center;
}

.clear-icon {
	position: absolute;
	right: 12px;
	color: #999;
	font-size: 14px;
	padding: 4px;
	z-index: 10;
}

.member-list {
	flex: 1;
	height: 0;
	/* Important for flex scroll */
	min-height: 200px;
}

.member-item {
	padding: 16px;
	border-bottom: 1px solid #eee;
	font-size: 16px;
	color: $sull-text-main;

	&:active {
		background: #f9f9f9;
	}
}

.empty-state-small {
	padding: 20px;
	text-align: center;
	color: $sull-text-secondary;
}

/* .picker-trigger removed as per request */

.picker-arrow {
	/* margin-right removed, spacing handled by container padding */
	font-size: 12px;
	color: $sull-text-secondary;
}
</style>

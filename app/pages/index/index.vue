<template>
	<view class="container">
		<scroll-view scroll-y class="main-scroll" :scroll-top="scrollTopValue" @scroll="onScroll"
			refresher-enabled="true" :refresher-triggered="isRefreshing" @refresherrefresh="onRefresh"
			refresher-default-style="none" refresher-background="#f0f5ff">
			<view slot="refresher" class="custom-refresher">
				<view class="refresher-content">
					<view class="refresher-icon" :class="{ 'animating': isRefreshing }"></view>
					<text class="refresher-text">{{ isRefreshing ? $t('index.refreshing') : $t('index.pullToRefresh')
					}}</text>
				</view>
			</view>
			<!-- Header (Now inside scroll-view) -->
			<view class="header">
				<view class="header-left">
					<text class="greeting">{{ greetingText }}</text>
					<text class="subtitle">{{ userOrgInfo.Department ? userOrgInfo.Department.Name : $t('index.dept')
					}}</text>
				</view>
				<view class="header-right">
					<view class="icon-btn search-btn" @click="onSearch">
						<text class="icon">🔍</text>
					</view>
					<view class="icon-btn notification-btn" @click="navToTodoList">
						<text class="icon">🔔</text>
						<view class="badge" v-if="todoTotal > 0">{{ todoTotal }}</view>
					</view>
				</view>
			</view>

			<view class="scroll-content">
				<!-- Status Overview (Data Big Screen Concept) -->
				<view class="overview-card" :class="[currentStatus, { 'empty-frosted': rescueCars.length === 0 }]">
					<template v-if="rescueCars.length > 0">
						<view class="overview-header">
							<view class="status-left">
								<picker v-if="rescueCars.length > 1" @change="onRescueCarChange"
									:value="currentRescueCarIndex" :range="rescueCars" range-key="Name">
									<view class="picker-wrap">
										<text class="overview-title">{{ rescueCars[currentRescueCarIndex] ?
											rescueCars[currentRescueCarIndex].DisplayName : $t('index.deptStatus')
										}}</text>
										<text class="arrow-down">▼</text>
									</view>
								</picker>
								<view v-else class="picker-wrap">
									<text class="overview-title">{{ rescueCars[currentRescueCarIndex] ?
										rescueCars[currentRescueCarIndex].DisplayName : $t('index.deptStatus')
									}}</text>
								</view>
							</view>
							<view class="status-tag" :class="statusTagClass">
								<text class="status-text">{{ rescueCars[currentRescueCarIndex] ?
									rescueCars[currentRescueCarIndex].FloorId + '层' : '' }}</text>
							</view>
						</view>
						<view class="stats-row">
							<view class="stat-item">
								<view class="stat-value-box">
									<text class="stat-num">{{ dashboardStats.ItemCounter }}</text>
									<text class="stat-unit">{{ $t('index.unit.type') }}</text>
								</view>
								<text class="stat-label">{{ $t('index.itemTypes') }}</text>
							</view>
							<view class="stat-item">
								<view class="stat-value-box">
									<text class="stat-num">{{ dashboardStats.PriceCounter }}</text>
									<text class="stat-unit">{{ $t('index.unit.tenThousand') }}</text>
								</view>
								<text class="stat-label">{{ $t('index.itemValue') }}</text>
							</view>
							<view class="stat-item">
								<view class="stat-value-box">
									<text class="stat-num warning">{{ dashboardStats.AbnormalStock }}</text>
									<text class="stat-unit warning">{{ $t('index.unit.type') }}</text>
								</view>
								<text class="stat-label" @click="handleStatClick('AbnormalStock')">{{
									$t('index.stockAbnormal') }}</text>
							</view>
							<view class="stat-item">
								<view class="stat-value-box">
									<text class="stat-num danger">{{ dashboardStats.AbnormalExpiry }}</text>
									<text class="stat-unit danger">{{ $t('index.unit.type') }}</text>
								</view>
								<text class="stat-label" @click="handleStatClick('AbnormalExpiry')">{{
									$t('index.expiryWarning') }}</text>
							</view>

						</view>
					</template>
					<view v-else class="guide-state">
						<view class="guide-icon-box" @click="openAddRescueCar">
							<text class="guide-icon">+</text>
						</view>
						<text class="guide-title">暂无抢救车</text>
						<text class="guide-desc">点击添加，开启智能管理！</text>
					</view>
				</view>

				<!-- Quick Actions (Daily Operations) -->
				<swiper class="action-swiper" :indicator-dots="actions.length > 8" indicator-active-color="#409EFF"
					indicator-color="#E4E7ED">
					<swiper-item v-for="(pageActions, pIndex) in actionPages" :key="pIndex">
						<view class="action-grid">
							<view class="action-item" v-for="(action, index) in pageActions" :key="index"
								@click="handleAction(action)">
								<view class="action-icon-box">
									<text class="action-icon">{{ action.icon }}</text>
									<view v-if="action.reminder" class="reminder-bell"></view>
								</view>
								<text class="action-label">{{ $t(action.labelKey) }}</text>
							</view>
						</view>
					</swiper-item>
				</swiper>

				<!-- Critical Reminders (Message Reminders) -->
				<view class="section-header">
					<text class="section-title">{{ $t('index.pendingTasks') }}</text>
					<text class="see-all" @click="navToTodoList">{{ $t('index.viewAll') }}</text>
				</view>

				<!-- Todo Tabs -->
				<view class="todo-tabs">
					<view class="todo-tab-item" v-for="(tab, index) in todoTabs" :key="index"
						:class="{ active: currentTodoTab === tab.key }" @click="currentTodoTab = tab.key">
						<text>{{ $t(tab.label) }}</text>
					</view>
				</view>

				<view class="alert-list">
					<view class="alert-card" v-for="(alert, index) in filteredAlerts" :key="index"
						@click="showTaskTimeline(alert)">
						<view class="alert-left">
							<view class="alert-type-tag" :class="getParamsClass(alert.Category)">
								<text class="alert-type-text">{{ getCategoryText(alert.Category) }}</text>
							</view>
						</view>
						<view class="alert-content">
							<text class="alert-msg" @longpress="showFullDescription(alert)">{{ alert.Description
							}}</text>
							<text class="alert-time">{{ alert.CreationTime }}</text>
						</view>
						<view class="alert-action" v-if="alert.LatestAssignUserId == userOrgInfo.User.UserId">
							<view class="icon-action-btn process-btn" @click.stop="handleProcess(alert)">
								<view class="action-icon-svg icon-process"></view>
							</view>
							<view class="icon-action-btn ignore-btn" @click.stop="handleIgnore(alert)">
								<view class="action-icon-svg icon-ignore"></view>
							</view>
						</view>
					</view>
					<view v-if="filteredAlerts.length === 0" class="empty-state-simple">
						<view class="empty-icon-simple"></view>
						<text class="empty-text-simple">{{ $t('index.noTasks') }}</text>
					</view>
				</view>
				<!-- Footer -->
				<FooterInfo />

				<view class="bottom-spacer"></view>
			</view>
		</scroll-view>

		<BackToTop :scroll-top="scrollTop" @click="scrollToTop" />

		<!-- Bottom Navigation -->
		<BottomNav activePage="home" />

		<!-- Add Rescue Car Modal -->
		<view class="modal-mask" v-if="showAddCarModal" @click="closeAddRescueCar">
			<view class="modal-content" @click.stop>
				<view class="modal-header">
					<text class="modal-title">快捷添加抢救车</text>
					<text class="close-btn" @click="closeAddRescueCar">×</text>
				</view>
				<view class="modal-body">
					<input class="modal-input" v-model="addCarForm.Name" placeholder="抢救车名(如1号车)" />
					<input class="modal-input" type="number" v-model="addCarForm.Floor" placeholder="层数 (如5)" />
				</view>
				<view class="modal-footer">
					<button class="btn btn-secondary btn-sm" @click="closeAddRescueCar">取消</button>
					<button class="btn btn-primary btn-sm" @click="submitRescueCar">确定</button>
				</view>
			</view>
		</view>

		<!-- Timeline Modal -->
		<view class="modal-mask" v-if="showTimelineModal" @click="closeTimelineModal">
			<view class="modal-content timeline-modal" @click.stop>
				<view class="modal-header">
					<text class="modal-title">处理详情</text>
					<text class="close-btn" @click="closeTimelineModal">×</text>
				</view>
				<scroll-view scroll-y class="modal-body timeline-body">
					<view v-if="timelineLoading" class="loading-state">
						<text>加载�.</text>
					</view>
					<view v-else-if="timelineData.length === 0" class="empty-state-simple">
						<text class="empty-text-simple">暂无处理记录</text>
					</view>
					<view v-else class="timeline-list">
						<view class="timeline-item" v-for="(item, index) in timelineData" :key="index">
							<view class="timeline-node" :class="{
								'is-start': index === 0 && !(index === timelineData.length - 1 && currentTimelineTask.IsFinished),
								'is-end': index === timelineData.length - 1 && currentTimelineTask.IsFinished
							}">
								<view v-if="index === timelineData.length - 1 && currentTimelineTask.IsFinished"
									class="node-icon icon-end"></view>
								<view v-else-if="index === 0" class="node-icon icon-start"></view>
								<view v-else class="timeline-dot"></view>
							</view>
							<view class="timeline-line" v-if="index < timelineData.length - 1"></view>
							<view class="timeline-content">
								<view class="timeline-header">
									<text class="timeline-time">{{ formatTime(item.CreationTime) }}</text>
									<text class="timeline-creator">{{ item.Creator || '未知' }}</text>
								</view>
								<view class="timeline-desc">{{ item.Description }}</view>
								<view class="timeline-footer">
									<text class="timeline-status" v-if="item.Status">状� {{ getStatusText(item.Status)
									}}</text>
									<text class="timeline-assign" v-if="item.AssignUser">处理�{{ item.AssignUser
									}}</text>
									<text class="timeline-remark" v-if="item.Remark">备注: {{ item.Remark }}</text>
								</view>
							</view>
						</view>
					</view>
				</scroll-view>
			</view>
		</view>
	</view>

</template>

<script>
import BackToTop from '@/components/BackToTop/BackToTop.vue';
import FooterInfo from '@/components/FooterInfo/FooterInfo.vue';
import BottomNav from '@/components/BottomNav/BottomNav.vue';
import config from '@/config.js';
import request from '@/utils/request.js';

const NotifyStatus = {
	Completed: 200,
	Canceled: 210,
	Ignored: 220
};

export default {
	components: {
		BackToTop,
		FooterInfo,
		BottomNav
	},
	data() {
		return {
			scrollTop: 0,
			scrollTopValue: 0, // Controlled scroll position
			isRefreshing: false, // Pull-to-refresh state
			currentStatus: 'normal', // normal, warning, abnormal
			version: config.version,
			companyName: config.company,
			currentYear: new Date().getFullYear(),
			// Actions - Unified background
			actions: [
				{ labelKey: 'index.actions.handover', icon: '🤝', reminder: true },
				{ labelKey: 'index.actions.stockIn', icon: '📥' },
				{ labelKey: 'index.actions.itemOutbound', icon: '📤' },
				{ labelKey: 'index.actions.virtualPos', icon: '📍' },
				{ labelKey: 'index.actions.sealing', icon: '🔒' },
				// { labelKey: 'index.actions.inventory', icon: '📋', reminder: true },
				{ labelKey: 'index.actions.records', icon: '📝' },
				{ labelKey: 'index.actions.cleaning', icon: '🧹' },
				// { labelKey: 'index.actions.report', icon: '🔧' },
				// { labelKey: 'index.actions.learning', icon: '📖' },
				{ labelKey: 'index.actions.itemManage', icon: '📦' },
			],
			alerts: [],
			todoTotal: 0,
			userInfo: {},
			userOrgInfo: {},
			todoListPage: 1,
			todoListRowsInPage: 10,
			currentTodoTab: 'all',
			todoTabs: [
				{ key: 'all', label: 'index.todo.all' },
				{ key: 'expiry', label: 'index.todo.expiry' },
				{ key: 'stock', label: 'index.todo.stock' },
				{ key: 'abnormal', label: 'index.todo.abnormal' },
				{ key: 'other', label: 'index.todo.other' }
			],
			rescueCars: [],
			currentRescueCarIndex: 0,
			showAddCarModal: false,
			addCarForm: {
				Name: '',
				Floor: ''
			},
			showTimelineModal: false,
			timelineLoading: false,
			timelineData: [],
			currentTimelineTask: {},
			dashboardStats: {},
		}
	},
	onShow() {
		// Check if user is logged in
		const token = uni.getStorageSync('token');
		if (!token) {
			// Redirect to profile page if not logged in
			uni.reLaunch({
				url: '/pages/profile/profile'
			});
			return;
		}
		this.userInfo = uni.getStorageSync('userInfo') || {};
		this.userOrgInfo = uni.getStorageSync('userOrgInfo') || {};
		this.fetchRescueCars();
		this.fetchTodoData();

	},
	computed: {
		actionPages() {
			const pages = [];
			const pageSize = 8;
			for (let i = 0; i < this.actions.length; i += pageSize) {
				pages.push(this.actions.slice(i, i + pageSize));
			}
			return pages;
		},
		statusText() {
			if (this.currentStatus === 'normal') return this.$t('index.normal');
			if (this.currentStatus === 'warning') return this.$t('index.warning');
			if (this.currentStatus === 'abnormal') return this.$t('index.abnormal');
			return '';
		},
		statusTagClass() {
			return `tag-${this.currentStatus}-bg`;
		},
		filteredAlerts() {
			if (this.currentTodoTab === 'all') return this.alerts;
			return this.alerts.filter(item => {
				const cat = item.Category;
				if (this.currentTodoTab === 'expiry') return cat === 10;
				if (this.currentTodoTab === 'stock') return cat === 20;
				if (this.currentTodoTab === 'abnormal') return cat === 30;
				if (this.currentTodoTab === 'other') return cat === 0 || cat === 40;
				return true;
			});
		},
		greetingText() {
			const name = this.userInfo.userTrueName || '';
			const template = this.$t('index.greeting');
			// Manually replace if interpolation fails or for safety
			return template.replace('[name]', name);
		}
	},
	methods: {
		onSearch() {
			console.log('Search clicked');
			// Navigate to search page or show search modal
		},
		onRefresh() {
			this.isRefreshing = true;
			// Simulate data refresh
			setTimeout(() => {
				this.isRefreshing = false;
				uni.showToast({
					title: this.$t('index.refreshed'),
					icon: 'none'
				});
			}, 1500);
		},
		onScroll(e) {
			this.scrollTop = e.detail.scrollTop;
		},
		scrollToTop() {
			this.scrollTopValue = this.scrollTop; // Sync first
			this.$nextTick(() => {
				this.scrollTopValue = 0; // Then scroll to 0
			});
		},
		navToTodoList() {
			uni.navigateTo({
				url: '/pages/todo-list/todo-list'
			});
		},
		navToBigScreen() {
			uni.reLaunch({
				url: '/pages/bigscreen/rescue-car'
			});
		},
		navTo(page) {
			// Navigation handled by BottomNav component
		},
		handleAction(action) {
			if (action.labelKey === 'index.actions.handover') {
				let relatedId = '';
				let relatedName = '';
				if (this.rescueCars && this.rescueCars.length > 0) {
					const car = this.rescueCars[this.currentRescueCarIndex];
					relatedId = car.Id;
					relatedName = car.DisplayName;
				}
				uni.navigateTo({
					url: `/pages/handover/handover?relatedId=${relatedId}&relatedName=${relatedName}`
				});
			} else if (action.labelKey === 'index.actions.sealing') {
				let relatedId = '';
				let relatedName = '';
				if (this.rescueCars && this.rescueCars.length > 0) {
					const car = this.rescueCars[this.currentRescueCarIndex];
					relatedId = car.Id;
					relatedName = car.DisplayName;
				}
				uni.navigateTo({
					url: `/pages/sealing/sealing?relatedId=${relatedId}&relatedName=${relatedName}`
				});
			} else if (action.labelKey === 'index.actions.itemOutbound') {
				uni.navigateTo({
					url: '/pages/item-outbound/item-outbound'
				});
			} else if (action.labelKey === 'index.actions.stockIn') {
				uni.navigateTo({
					url: '/pages/item-inbound/item-inbound'
				});
			} else if (action.labelKey === 'index.actions.itemManage') {
				uni.navigateTo({
					url: '/pages/item-manage/item-manage'
				});
			} else if (action.labelKey === 'index.actions.virtualPos') {
				uni.navigateTo({
					url: '/pages/position-manage/position-manage'
				});
			} else {
				uni.showToast({
					title: this.$t(action.labelKey),
					icon: 'none'
				});
			}
		},
		fetchTodoData() {
			// Fetch pending tasks
			request.post('api/ssbh/TodoList/GetPageData', {
				page: this.todoListPage, rows: this.todoListRowsInPage, sort: 'CreationTime', order: 'desc'
				, wheres: JSON.stringify([
					{ name: 'IsFinished', value: false }
				])
			})
				.then(res => {
					this.alerts = res.rows || [];
					this.todoTotal = res.total || 0;
				})
				.catch(e => {
					console.error("Fetch Todo Failed", e);
					this.alerts = [];
					this.todoTotal = 0;
				});
		},
		getParamsClass(category) {
			switch (category) {
				case 10: return 'tag-danger'; // Expiry
				case 20: return 'tag-warning'; // Stock
				case 30: return 'tag-danger'; // Abnormal
				case 40: return 'tag-info'; // Notification
				default: return 'tag-info'; // Other (0)
			}
		},
		getCategoryText(category) {
			switch (category) {
				case 10: return this.$t('index.todo.expiry');
				case 20: return this.$t('index.todo.stock');
				case 30: return this.$t('index.todo.abnormal');
				case 40: return '通知';
				default: return this.$t('index.todo.other');
			}
		},
		// Rescue Car Logic
		fetchRescueCars() {
			if (!this.userOrgInfo || !this.userOrgInfo.Organization) return;

			request.post('api/ssbh/Position/GetPageData', {
				page: 1, rows: 100,
				wheres: JSON.stringify([
					{ name: 'OrganizationId', value: this.userOrgInfo.Organization.Id },
					{ name: 'DepartmentId', value: this.userOrgInfo.Department.Id },
					{ name: 'Category', value: 10 }
				])
			}).then(res => {
				this.rescueCars = res.rows || [];
				if (this.rescueCars.length > 0) {
					this.currentRescueCarIndex = 0;
					// Fetch stats
					this.fetchDashboardStats();
				} else {
					// Guide mode handled in template
				}
			}).catch(e => console.error(e));
		},
		onRescueCarChange(e) {
			this.currentRescueCarIndex = e.detail.value;
			this.fetchDashboardStats();
		},



		openAddRescueCar() {
			uni.navigateTo({
				url: '/pages/position-manage/position-add-car'
			});
		},
		showFullDescription(alert) {
			const title = this.getCategoryText(alert.Category);
			uni.showModal({
				title: title,
				content: alert.Description,
				showCancel: false,
				confirmText: this.$t('handover.confirm') || '确定'
			});
		},
		handleProcess(item) {
			const category = item.Category;
			const relatedId = item.RelatedId;
			switch (category) {
				case 10: // Expiry
				case 20: // Stock
					// Navigate to Rescue Car Big Screen (reLaunch as it is a main tab/screen)
					// Passing params in url. Note: rescue-car need to handle onLoad options
					uni.reLaunch({
						url: `/pages/bigscreen/rescue-car?type=${category}&id=${relatedId}`
					});
					break;
				case 30: // Abnormal
					// Using navigateTo to allow back navigation
					uni.navigateTo({
						url: `/pages/handover-check/handover-item-check?id=${relatedId}&todoListId=${item.Id}`
					});
					break;
				case 40: // Notification
					if (item.Kind === 1) {
						uni.navigateTo({
							url: `/pages/handover-check/handover-item-check?id=${relatedId}&todoListId=${item.Id}`
						});
					} else if (item.Kind === 2) {
						uni.navigateTo({
							url: `/pages/sealing/sealing?relatedId=${relatedId}&todoListId=${item.Id}&relatedName=${item.Title}`
						});
					}
					break;
				default:
					uni.showToast({
						title: 'Feature not available',
						icon: 'none'
					});
					break;
			}
		},
		handleIgnore(item) {
			uni.showModal({
				title: '忽略提醒',
				content: '确定要忽略这条提醒吗？',
				success: (res) => {
					if (res.confirm) {
						const payload = {
							TodoListId: item.Id,
							Description: '已选择忽略',
							Status: NotifyStatus.Ignored
						};
						request.post('api/ssbh/TodoListDetail/Add', {
							mainData: payload
						}).then(res => {
							if (res.success || res.status) {
								uni.showToast({
									title: '已忽略',
									icon: 'none'
								});
								// Remove from local list
								const index = this.alerts.findIndex(a => a.Id === item.Id);
								if (index > -1) {
									this.alerts.splice(index, 1);
									if (this.todoTotal > 0) this.todoTotal--;
								}
							} else {
								uni.showToast({
									title: '操作失败',
									icon: 'none'
								});
							}
						}).catch(err => {
							console.error(err);
							uni.showToast({
								title: '网络错误',
								icon: 'none'
							});
						});
					}
				}
			});
		},
		showTaskTimeline(alert) {
			this.currentTimelineTask = alert;
			this.showTimelineModal = true;
			this.timelineLoading = true;
			this.timelineData = [];

			const wheres = [
				{ name: 'TodoListId', value: alert.Id, displayType: 'numeric' }
			];

			request.post('api/ssbh/TodoListDetail/GetPageData', {
				page: 1,
				rows: 100,
				wheres: JSON.stringify(wheres),
				sort: 'CreationTime',
				order: 'asc'
			}).then(res => {
				this.timelineLoading = false;
				if (res.status === 0) {
					this.timelineData = res.rows || [];
				} else {
					uni.showToast({ title: 'Fetch Timeline Failed', icon: 'none' });
				}
			}).catch(err => {
				console.error(err);
				this.timelineLoading = false;
				uni.showToast({ title: 'Error fetching timeline', icon: 'none' });
			});
		},
		fetchDashboardStats() {
			if (this.rescueCars.length === 0) return;
			const currentCar = this.rescueCars[this.currentRescueCarIndex];
			if (!currentCar) return;
			request.get(`api/ssbh/Position/Statistic?warehouseId=${currentCar.Id}`)
				.then(res => {
					if (res) {
						let price = (res.PriceCounter || 0) / 10000 / 100; // Cent to Yuan (100), then to 10k (10000) => / 1000000? 
						// Request: "单位为分，需要转化为万元，保留两位小�
						// 1 � 100 �
						// 1 万元 = 10000 � 1000000 �
						price = (res.PriceCounter || 0) / 1000000;
						this.dashboardStats = {
							ItemCounter: res.ItemCounter || 0,
							PriceCounter: price.toFixed(2),
							AbnormalStock: res.AbnormalStock || 0,
							AbnormalExpiry: res.AbnormalExpiry || 0
						};
					}
				})
				.catch(err => {
					console.error('Fetch Dashboard Stats Failed', err);
				});
		},
		handleStatClick(type) {
			if (type === 'AbnormalStock') {
				if (this.dashboardStats.AbnormalStock > 0) {
					uni.navigateTo({
						url: '/pages/position-manage/position-manage?filter=AbnormalStock'
					});
				}
			} else if (type === 'AbnormalExpiry') {
				if (this.dashboardStats.AbnormalExpiry > 0) {
					uni.navigateTo({
						url: '/pages/position-manage/position-manage?filter=AbnormalExpiry'
					});
				}
			}
		},
		closeTimelineModal() {
			this.showTimelineModal = false;
		},
		formatTime(timeStr) {
			if (!timeStr) return '';
			// Assuming ISO format or similar, you might need 'formatDate' util
			const date = new Date(timeStr);
			if (isNaN(date.getTime())) return timeStr;
			// Simple format: YYYY-MM-DD HH:mm
			const Y = date.getFullYear();
			const M = (date.getMonth() + 1).toString().padStart(2, '0');
			const D = date.getDate().toString().padStart(2, '0');
			const h = date.getHours().toString().padStart(2, '0');
			const m = date.getMinutes().toString().padStart(2, '0');
			return `${Y}-${M}-${D} ${h}:${m}`;
		},
		getStatusText(status) {
			// Reuse NotifyStatus if applicable, or generic mapping
			if (status == 200) return '已完成';
			if (status == 210) return '已取消';
			if (status == 220) return '已忽略';
			return status;
		}
	}
}
</script>

<style lang="scss" scoped>
page {
	background-color: $sull-color-background;
	height: 100%;
	font-family: $font-family-main;
}

.container {
	display: flex;
	flex-direction: column;
	height: 100vh;
}

/* Header - Optimized for compactness and scrolling */
.header {
	padding: 0px 24px 10px 24px;
	/* Reduced padding */
	display: flex;
	justify-content: space-between;
	align-items: center;
	background-color: $sull-color-background;
	/* Match page bg so it blends when scrolling */
}

.header-right {
	display: flex;
	gap: 12px;
}

.greeting {
	font-size: $font-size-body-lg;
	/* Reduced from h5 */
	font-weight: $font-weight-bold;
	color: $sull-text-main;
	display: block;
}

.subtitle {
	font-size: $font-size-body-sm;
	/* Reduced from md */
	color: $sull-text-secondary;
}

.icon-btn {
	position: relative;
	width: 32px;
	/* Smaller button */
	height: 32px;
	background: $sull-color-white;
	border: 1px solid $sull-color-line;
	border-radius: $radius-circle;
	display: flex;
	align-items: center;
	justify-content: center;
}

.icon {
	font-size: $font-size-body-lg;
}

.badge {
	position: absolute;
	top: -4px;
	right: -4px;
	background: $sull-color-error;
	color: $sull-color-white;
	font-size: 8px;
	padding: 1px 4px;
	border-radius: $radius-md;
	border: 1px solid $sull-color-white;
}

.main-scroll {
	flex: 1;
	height: 0;
}

.scroll-content {
	padding: 10px 24px 160px 24px;
	/* Increased bottom padding to clear nav bar */
}

/* Overview Card */
.overview-card {
	border-radius: 20px;
	padding: 16px;
	margin-bottom: 20px;
	transition: all 0.3s ease;
}

.overview-card.normal {
	background: #F0F9EB;
	border: 1px solid #67C23A;
}

.overview-card.warning {
	background: #FDF6EC;
	border: 1px solid #E6A23C;
}

.overview-card.abnormal {
	background: #FEF0F0;
	border: 2px solid #F56C6C;
	border: 2px solid #F56C6C;
	/* Thicker border for glaring effect */
}

.overview-card.empty-frosted {
	background: rgba(255, 255, 255, 0.3);
	backdrop-filter: blur(10px);
	-webkit-backdrop-filter: blur(10px);
	border: 1px solid rgba(255, 255, 255, 0.5);
	box-shadow: 0 4px 16px rgba(0, 0, 0, 0.05);
	display: flex;
	align-items: center;
	justify-content: center;
	min-height: 130px;
	/* Preserve height */
}

/* Text Colors for Card */
/* Removed dynamic text colors as per request */

/* Tag Backgrounds */
.tag-normal-bg {
	background: rgba(103, 194, 58, 0.1);
}

.tag-warning-bg {
	background: rgba(230, 162, 60, 0.1);
}

.tag-abnormal-bg {
	background: rgba(245, 108, 108, 0.1);
}

.overview-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-bottom: 12px;
}

.overview-title {
	font-size: $font-size-body-lg;
	font-weight: $font-weight-semibold;
	opacity: 0.9;
	color: $sull-text-main;
	/* Ensure dark text */
}

.status-tag {
	padding: 4px 12px;
	border-radius: 20px;
}

.status-text {
	font-size: $font-size-body-sm;
	font-weight: $font-weight-semibold;
	color: $sull-text-main;
}

.stats-row {
	display: flex;
	justify-content: space-between;
}

.stat-item {
	display: flex;
	flex-direction: column;
	align-items: center;
}

.stat-num {
	font-size: $font-size-h4;
	font-weight: $font-weight-bold;
	color: $sull-text-main;
	line-height: 1;
}

.stat-value-box {
	display: flex;
	align-items: baseline;
	justify-content: center;
	gap: 2px;
	margin-bottom: 4px;
}

.stat-unit {
	font-size: 12px;
	color: $sull-text-secondary;
	font-weight: normal;

	&.warning {
		color: #E6A23C;
	}

	&.danger {
		color: #F56C6C;
	}
}

/* Updated for light background contrast */
.stat-num.warning {
	color: #E6A23C;
}

.stat-num.danger {
	color: #F56C6C;
}

.stat-label {
	font-size: $font-size-body-sm;
	opacity: 0.8;
	color: $sull-text-secondary;
}

/* Section Headers */
.section-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-bottom: 16px;
}

.section-title {
	font-size: $font-size-body-lg;
	font-weight: $font-weight-bold;
	color: $sull-text-main;
}

.see-all {
	font-size: $font-size-body-md;
	color: $sull-color-primary;
}

/* Quick Actions Grid */
.action-swiper {
	height: 230px;
	margin-bottom: 0px;
}

.action-grid {
	display: flex;
	flex-wrap: wrap;
	gap: 16px;
	/* margin-bottom removed as it is handled by swiper container */
	padding: 10px 4px 0 4px;
	/* Increased top padding to prevent bell clipping */
}

.action-item {
	width: calc(25% - 12px);
	display: flex;
	flex-direction: column;
	align-items: center;
}

.action-icon-box {
	position: relative;
	/* Added for reminder positioning */
	width: 56px;
	height: 56px;
	border-radius: $radius-lg;
	display: flex;
	align-items: center;
	justify-content: center;
	margin-bottom: 8px;
	background-color: $sull-color-white;
	box-shadow: 0 2px 8px rgba(0, 0, 0, 0.03);
}

.reminder-bell {
	position: absolute;
	top: -8px;
	right: -8px;
	width: 20px;
	height: 20px;
	background-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='%23F56C6C'%3E%3Cpath d='M12 22c1.1 0 2-.9 2-2h-4c0 1.1.9 2 2 2zm6-6v-5c0-3.07-1.63-5.64-4.5-6.32V4c0-.83-.67-1.5-1.5-1.5s-1.5.67-1.5 1.5v.68C7.64 5.36 6 7.92 6 11v5l-2 2v1h16v-1l-2-2z'/%3E%3C/svg%3E");
	background-size: contain;
	background-repeat: no-repeat;
	animation: bell-shake 1.2s infinite ease-in-out;
	z-index: 10;
}

@keyframes bell-shake {
	0% {
		transform: rotate(0) scale(1);
	}

	10% {
		transform: rotate(25deg) scale(1.1);
	}

	20% {
		transform: rotate(-20deg) scale(1.1);
	}

	30% {
		transform: rotate(15deg) scale(1.2);
	}

	/* Max magnification */
	40% {
		transform: rotate(-10deg) scale(1.2);
	}

	50% {
		transform: rotate(5deg) scale(1.1);
	}

	60% {
		transform: rotate(-5deg) scale(1.1);
	}

	70% {
		transform: rotate(0) scale(1);
	}

	100% {
		transform: rotate(0) scale(1);
	}
}

.action-icon {
	font-size: 24px;
}

.action-label {
	font-size: $font-size-body-sm;
	color: $sull-text-secondary;
	text-align: center;
}

/* Guide State */
.guide-state {
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	padding: 0;
	height: 100%;
}

.guide-icon-box {
	width: 64px;
	height: 64px;
	border-radius: 50%;
	/* 3D Soft Gradient - Orange */
	background: linear-gradient(135deg, #FFD666, #E6A23C);
	/* Depth Shadows */
	box-shadow:
		0 10px 20px rgba(230, 162, 60, 0.25),
		inset 2px 2px 5px rgba(255, 255, 255, 0.5),
		inset -2px -2px 5px rgba(0, 0, 0, 0.1);
	display: flex;
	align-items: center;
	justify-content: center;
	margin-bottom: 12px;
	position: relative;
	overflow: hidden;
	z-index: 10;
}

.guide-icon-box::after {
	content: '';
	position: absolute;
	top: -50%;
	left: -50%;
	width: 200%;
	height: 200%;
	background: linear-gradient(to bottom right,
			rgba(255, 255, 255, 0) 0%,
			rgba(255, 255, 255, 0) 40%,
			rgba(255, 255, 255, 0.6) 50%,
			rgba(255, 255, 255, 0) 60%,
			rgba(255, 255, 255, 0) 100%);
	transform: rotate(45deg);
	animation: shimmer 3s infinite;
}

@keyframes shimmer {
	0% {
		transform: translateY(-100%) rotate(45deg);
	}

	100% {
		transform: translateY(100%) rotate(45deg);
	}
}

.guide-icon {
	font-size: 32px;
	color: #FFFFFF;
	font-weight: bold;
	line-height: 1;
	margin-top: -4px;
}

.guide-title {
	font-size: 18px;
	font-weight: bold;
	color: #333;
	margin-bottom: 6px;
}

.guide-desc {
	font-size: 14px;
	color: #666;
	margin-bottom: 0;
}

/* Alert List */
.alert-list {
	display: flex;
	flex-direction: column;
	gap: 12px;
}

.alert-card {
	background: $sull-color-white;
	border-radius: $radius-lg;
	padding: 16px;
	display: flex;
	align-items: center;
	box-shadow: 0 2px 8px rgba(0, 0, 0, 0.02);
}

.alert-left {
	margin-right: 12px;
}

.alert-type-tag {
	padding: 4px 8px;
	border-radius: 4px;
	font-size: 11px;
	font-weight: bold;
}

.tag-danger {
	background: #FEF0F0;
	color: #F56C6C;
}

.tag-warning {
	background: #FDF6EC;
	color: #E6A23C;
}

.tag-info {
	background: #F4F4F5;
	color: #909399;
}

.alert-content {
	flex: 1;
	display: flex;
	flex-direction: column;
}

.alert-msg {
	font-size: $font-size-body-md;
	color: $sull-text-main;
	margin-bottom: 4px;
	line-height: 1.4;

	overflow: hidden;
	text-overflow: ellipsis;
	display: -webkit-box;
	-webkit-line-clamp: 2;
	line-clamp: 2;
	-webkit-box-orient: vertical;
}

.alert-time {
	font-size: 11px;
	color: $sull-text-secondary;
}

.alert-action {
	margin-left: 12px;
	display: flex;
	gap: 8px;
}

.icon-action-btn {
	width: 32px;
	height: 32px;
	border-radius: 50%;
	display: flex;
	align-items: center;
	justify-content: center;
	transition: opacity 0.2s;

	&:active {
		opacity: 0.7;
	}

	&.process-btn {
		background: #EAF2FF;
		border: 1px solid #D0E7FF;
	}

	&.ignore-btn {
		background: #F4F4F5;
		border: 1px solid #E9E9EB;
	}
}

.action-icon-small {
	font-size: 16px;
	line-height: 1;
}

/* Simple Empty State */
.empty-state-simple {
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	padding: 30px 0;
}

.empty-icon-simple {
	width: 48px;
	height: 48px;
	background-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='%23DCDFE6'%3E%3Cpath d='M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm-5 14H7v-2h7v2zm3-4H7v-2h10v2zm0-4H7V7h10v2z'/%3E%3C/svg%3E");
	background-size: contain;
	background-repeat: no-repeat;
	margin-bottom: 8px;
	opacity: 0.5;
}

.empty-text-simple {
	font-size: 13px;
	color: $sull-text-secondary;
}

.bottom-spacer {
	height: 60px;
}

/* Modal Styles (Global or Scoped Root) */
.modal-mask {
	position: fixed;
	top: 0;
	left: 0;
	right: 0;
	bottom: 0;
	background: rgba(0, 0, 0, 0.5);
	z-index: 1000;
	display: flex;
	align-items: center;
	justify-content: center;
}

.modal-content {
	width: 80%;
	background: #fff;
	border-radius: 12px;
	overflow: hidden;
	animation: modal-pop 0.3s ease-out;
}

@keyframes modal-pop {
	0% {
		transform: scale(0.9);
		opacity: 0;
	}

	100% {
		transform: scale(1);
		opacity: 1;
	}
}

.modal-header {
	padding: 16px;
	border-bottom: 1px solid #EBEEF5;
	display: flex;
	justify-content: space-between;
	align-items: center;

	.modal-title {
		font-size: 16px;
		font-weight: bold;
		color: #303133;
	}

	.close-btn {
		font-size: 20px;
		color: #909399;
		padding: 4px;
	}
}

.modal-body {
	padding: 20px 16px;
}

.modal-input {
	height: 48px;
	background: #F5F7FA;
	border-radius: 8px;
	padding: 0 12px;
	font-size: 14px;
	margin-bottom: 16px;

	&:last-child {
		margin-bottom: 0;
	}
}

.modal-footer {
	display: flex;
	padding: 0 16px 16px 16px;
	gap: 12px;
}

.btn {
	flex: 1;
	font-size: 14px;
	border-radius: 20px;

	&::after {
		border: none;
	}
}

.btn-primary {
	background: #409EFF;
	color: #fff;
}

.btn-secondary {
	background: #F4F4F5;
	color: #909399;
}

/* Timeline Styles */
.timeline-modal {
	height: 60vh;
	display: flex;
	flex-direction: column;
}

.timeline-body {
	flex: 1;
	padding: 20px;
	box-sizing: border-box;
}

.timeline-list {
	padding-left: 10px;
}

.timeline-item {
	position: relative;
	padding-left: 20px;
	padding-bottom: 24px;
}

.timeline-item:last-child {
	padding-bottom: 0;
}

.timeline-node {
	position: absolute;
	left: 0;
	top: 0;
	width: 20px;
	height: 100%;
	display: flex;
	justify-content: center;
}

.timeline-dot {
	width: 8px;
	height: 8px;
	border-radius: 50%;
	background-color: $sull-color-primary;
	margin-top: 16px;
	/* Align with content top approx */
	z-index: 1;
}

.node-icon {
	width: 16px;
	height: 16px;
	margin-top: 12px;
	z-index: 1;
	background-color: $sull-color-primary;
	-webkit-mask-size: contain;
	mask-size: contain;
	-webkit-mask-repeat: no-repeat;
	mask-repeat: no-repeat;
	-webkit-mask-position: center;
	mask-position: center;
}

/* Play/Start Icon */
.icon-start {
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M8 5v14l11-7z'/%3E%3C/svg%3E");
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M8 5v14l11-7z'/%3E%3C/svg%3E");
}

/* Check/End Icon */
.icon-end {
	background-color: $sull-color-success;
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41z'/%3E%3C/svg%3E");
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41z'/%3E%3C/svg%3E");
}

.timeline-line {
	position: absolute;
	left: 9px;
	top: 24px;
	/* Start below first node/dot */
	bottom: -10px;
	/* Extend to next */
	width: 2px;
	background-color: #E4E7ED;
}

.timeline-item:last-child .timeline-line {
	display: none;
}

/* Adjust line for start/end icons which are larger */
.timeline-node.is-start~.timeline-line {
	top: 28px;
}

/* If end node is present, the previous line needs to connect to it. 
   css logic for 'previous line' is hard in pure css without complex selectors if height varies.
   But strictly, the line belongs to the item. 
   So item N's line goes to item N+1.
*/

.timeline-content {
	background: #F8F9FB;
	padding: 12px;
	border-radius: 8px;
	margin-left: 10px;
	/* Space from dot layout */
}

.timeline-header {
	display: flex;
	justify-content: space-between;
	margin-bottom: 6px;
	font-size: 12px;
	color: #909399;
}

.timeline-desc {
	font-size: 14px;
	color: #303133;
	margin-bottom: 6px;
	line-height: 1.4;
}

.timeline-footer {
	font-size: 12px;
	color: #606266;
	display: flex;
	flex-wrap: wrap;
	gap: 8px;
}

.timeline-status {
	color: $sull-color-primary;
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

/* Icons */
.icon-home {
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M10 20v-6h4v6h5v-8h3L12 3 2 12h3v8z'/%3E%3C/svg%3E");
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M10 20v-6h4v6h5v-8h3L12 3 2 12h3v8z'/%3E%3C/svg%3E");
}

.icon-screen {
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M21 2H3c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h7v2H8v2h8v-2h-2v-2h7c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2zm0 14H3V4h18v12z'/%3E%3C/svg%3E");
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M21 2H3c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h7v2H8v2h8v-2h-2v-2h7c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2zm0 14H3V4h18v12z'/%3E%3C/svg%3E");
}

.icon-scan {
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Ccircle cx='12' cy='12' r='3.2'/%3E%3Cpath d='M9 2L7.17 4H4c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2h-3.17L15 2H9zm3 15c-2.76 0-5-2.24-5-5s2.24-5 5-5 5 2.24 5 5-2.24 5-5 5z'/%3E%3C/svg%3E");
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Ccircle cx='12' cy='12' r='3.2'/%3E%3Cpath d='M9 2L7.17 4H4c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2h-3.17L15 2H9zm3 15c-2.76 0-5-2.24-5-5s2.24-5 5-5 5 2.24 5 5-2.24 5-5 5z'/%3E%3C/svg%3E");
}

.icon-stats {
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zM9 17H7v-7h2v7zm4 0h-2V7h2v10zm4 0h-2v-4h2v4z'/%3E%3C/svg%3E");
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zM9 17H7v-7h2v7zm4 0h-2V7h2v10zm4 0h-2v-4h2v4z'/%3E%3C/svg%3E");
}

.icon-user {
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z'/%3E%3C/svg%3E");
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z'/%3E%3C/svg%3E");
}

/* Simple Empty State */
.empty-state-simple {
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	padding: 40px 0;
}

.empty-text-simple {
	font-size: 14px;
	color: #999;
	font-weight: normal;
	letter-spacing: 0.5px;
}

.empty-icon-simple {
	width: 48px;
	height: 48px;
	min-width: 48px;
	min-height: 48px;
	flex-shrink: 0;
	margin-bottom: 8px;
	background-color: #E0E0E0;
	/* Slightly lighter for large icon */
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z'/%3E%3C/svg%3E");
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z'/%3E%3C/svg%3E");
	-webkit-mask-size: 100% 100%;
	-webkit-mask-repeat: no-repeat;
	-webkit-mask-position: center;
	mask-size: 100% 100%;
	mask-repeat: no-repeat;
	mask-position: center;
}

/* Todo Tabs */
.todo-tabs {
	display: flex;
	background: #fff;
	border-radius: 25px;
	padding: 4px;
	margin-bottom: 16px;
	box-shadow: 0 2px 8px rgba(0, 0, 0, 0.02);
}

.todo-tab-item {
	flex: 1;
	height: 42px;
	display: flex;
	align-items: center;
	justify-content: center;
	font-size: 14px;
	color: $sull-text-secondary;
	font-weight: 500;
	border-radius: 21px;
	transition: all 0.3s;
}

.todo-tab-item.active {
	background: $sull-color-primary;
	color: #fff;
	font-weight: 600;
	box-shadow: 0 2px 6px rgba(149, 202, 255, 0.4);
}

/* Modal Styles (Global or Scoped Root) */
.modal-mask {
	position: fixed;
	top: 0;
	left: 0;
	right: 0;
	bottom: 0;
	background: rgba(0, 0, 0, 0.5);
	z-index: 9999;
	display: flex;
	align-items: center;
	justify-content: center;
}

.modal-content {
	background: #fff;
	width: 80%;
	border-radius: 12px;
	padding: 20px;
}

.modal-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-bottom: 20px;
}

.modal-title {
	font-size: 18px;
	font-weight: bold;
}

.close-btn {
	font-size: 24px;
	color: #999;
}

.modal-body {
	margin-bottom: 20px;
}

.modal-input {
	border: 1px solid #eee;
	border-radius: 8px;
	padding: 10px;
	margin-bottom: 12px;
	font-size: 14px;
	display: block;
	width: 100%;
	box-sizing: border-box;
}

.modal-footer {
	display: flex;
	justify-content: flex-end;
	gap: 12px;
}

.icon-action-btn {
	width: 32px;
	height: 32px;
	border-radius: 50%;
	display: flex;
	align-items: center;
	justify-content: center;
	transition: opacity 0.2s;

	&:active {
		opacity: 0.7;
	}

	&.process-btn {
		background: #EAF2FF;
		border: 1px solid #D0E7FF;
	}

	&.ignore-btn {
		background: #F4F4F5;
		border: 1px solid #E9E9EB;
	}
}

.action-icon-svg {
	width: 16px;
	height: 16px;
	background-repeat: no-repeat;
	background-position: center;
	background-size: contain;
}

/* Curved Up-Right Arrow */
.icon-process {
	background-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='%23409EFF' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Cpath d='M4 17C4 17 4 9 12 9H19' /%3E%3Cpath d='M15 5L19 9L15 13' /%3E%3C/svg%3E");
}

/* Eye Off */
.icon-ignore {
	background-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='%23909399' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Cpath d='M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24'/%3E%3Cline x1='1' y1='1' x2='23' y2='23'/%3E%3C/svg%3E");
}
</style>
}

<template>
	<view class="container">
		<!-- Fixed Header -->
		<view class="header-fixed" :style="{ paddingTop: statusBarHeight * 2 + 'rpx' }">
			<view class="header-content">
				<view class="header-left">
					<text class="greeting">{{ greetingText }}</text>
					<text class="subtitle">{{ userOrgInfo.Department ? userOrgInfo.Department.Name : $t('index.dept')
					}}</text>
				</view>
				<view class="header-right">
					<view class="icon-btn search-btn" @click="onSearch">
						<image class="icon-img" src="/static/imgs/search.png" mode="aspectFit"></image>
					</view>
					<!-- Message Icon -->
					<view class="icon-btn" @click="navTo('/pages/todo-list/todo-list')">
						<text class="icon">🔔</text>
						<view class="badge" v-if="dashboardStats.TotalPending > 0">{{ dashboardStats.TotalPending > 99
							? '99+' : dashboardStats.TotalPending }}</view>
					</view>
					<!-- One-click Rescue -->
					<view class="icon-btn rescue-btn" :class="{ 'rescuing': isRescuing }" @click="handleRescueClick">
						<image class="icon-img" src="/static/imgs/First-aid.png" mode="aspectFit"
							style="width: 44rpx; height: 44rpx;"></image>
					</view>
				</view>
			</view>
		</view>

		<scroll-view scroll-y class="main-scroll" :scroll-top="scrollTopValue" @scroll="onScroll"
			refresher-enabled="true" :refresher-triggered="isRefreshing" :refresher-threshold="60"
			@refresherrefresh="onRefresh" @refresherpulling="onPulling" @refresherrestore="onRestore"
			@refresherabort="onRestore" refresher-default-style="none" refresher-background="#f0f5ff"
			:style="{ height: `calc(100vh - ${statusBarHeight * 2}rpx - 88rpx)`, marginTop: `calc(${statusBarHeight * 2}rpx + 88rpx)` }">
			<CustomRefresher slot="refresher" :isRefreshing="isRefreshing" :isPulling="isPulling" />

			<view class="scroll-content" style="padding-top: 20rpx;">
				<!-- Status Overview (Data Big Screen Concept) -->
				<view class="overview-card"
					:class="[isRescuing ? 'status-rescuing' : statusConfig.class, { 'empty-frosted': rescueCars.length === 0 }]">
					<template v-if="rescueCars.length > 0">
						<view class="overview-header">
							<view class="status-left">
								<picker v-if="rescueCars.length > 1" @change="onRescueCarChange"
									:value="currentRescueCarIndex" :range="rescueCars" range-key="DisplayName">
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
							<view class="status-container" v-if="isRescuing">
								<text class="rescue-timer">{{ rescueDuration }}</text>
								<view class="rescuing-tag">抢救中</view>
							</view>
							<view class="status-tag" v-else @click="showStatusRemark">
								<text class="status-text">{{ rescueCars[currentRescueCarIndex] ?
									getRescueCarStatusText(rescueCars[currentRescueCarIndex].Status) : '' }}</text>
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
									<image class="action-icon-img" :src="action.iconPath" mode="aspectFit"></image>
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
				<CapsuleTabs :tabs="todoTabs" v-model:current="currentTodoTab"
					container-style="padding: 16rpx 0 32rpx 0;" />

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
						<text>加载中.</text>
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
									<text class="timeline-status" v-if="item.Status">状态 {{ getStatusText(item.Status)
									}}</text>
									<text class="timeline-assign" v-if="item.AssignUser">处理人{{ item.AssignUser
									}}</text>
									<text class="timeline-remark" v-if="item.Remark">备注: {{ item.Remark }}</text>
								</view>
							</view>
						</view>
					</view>
				</scroll-view>
			</view>
		</view>
		<!-- Unified Warning/Choice Modal -->
		<WarningModal v-model:visible="warningModal.visible" :title="warningModal.title" :message="warningModal.message"
			:buttonText="warningModal.buttonText" :cancelText="warningModal.cancelText"
			:showCancel="warningModal.showCancel" :showClose="warningModal.showClose" :icon="warningModal.icon"
			@confirm="onWarningConfirm" @cancel="onWarningCancel" @close="onWarningClose" />
	</view>
</template>

<script>
import BackToTop from '@/components/BackToTop/BackToTop.vue';
import FooterInfo from '@/components/FooterInfo/FooterInfo.vue';
import BottomNav from '@/components/BottomNav/BottomNav.vue';
import WarningModal from '@/components/WarningModal/WarningModal.vue';
import CapsuleTabs from '@/components/CapsuleTabs/CapsuleTabs.vue';
import CustomRefresher from '@/components/CustomRefresher/CustomRefresher.vue';
import config from '@/config.js';
import request from '@/utils/request.js';
import { storage } from '@/utils/storage.js';
import dayjs from 'dayjs';
import { formatMessage } from '@/utils/i18n.js';
import { getRescueCarStatusText } from '@/utils/rescueCarUtils.js';


const NotifyStatus = {
	Completed: 200,
	Canceled: 210,
	Ignored: 220
};

export default {
	components: {
		BackToTop,
		FooterInfo,
		BottomNav,
		WarningModal,
		CapsuleTabs,
		CustomRefresher
	},
	data() {
		return {
			positionData: [
				{
					"title": "分管院长", "level": 50, "group": 50, menus: ['index.actions.handover', 'index.actions.stockIn', 'index.actions.itemOutbound',
						'index.actions.virtualPos', 'index.actions.sealing', 'index.actions.records', 'index.actions.itemManage',
						'index.actions.schedule'
					]
				},
				{
					"title": "护理部主任", "level": 40, "group": 40, menus: ['index.actions.handover', 'index.actions.stockIn', 'index.actions.itemOutbound',
						'index.actions.virtualPos', 'index.actions.sealing', 'index.actions.records', 'index.actions.itemManage',
						'index.actions.schedule']
				},
				{
					"title": "科主任", "level": 31, "group": 30, menus: ['index.actions.handover', 'index.actions.stockIn', 'index.actions.itemOutbound',
						'index.actions.virtualPos', 'index.actions.sealing', 'index.actions.records', 'index.actions.itemManage',
						'index.actions.schedule']
				},
				{
					"title": "护士长", "level": 30, "group": 30, menus: ['index.actions.handover', 'index.actions.stockIn', 'index.actions.itemOutbound',
						'index.actions.virtualPos', 'index.actions.sealing', 'index.actions.records', 'index.actions.itemManage',
						'index.actions.schedule']
				},
				{ "title": "医生", "level": 22, "group": 20, menus: ['index.actions.virtualPos', 'index.actions.records', 'index.actions.schedule'] },
				{ "title": "护士", "level": 20, "group": 20, menus: ['index.actions.handover', 'index.actions.sealing', 'index.actions.stockIn', 'index.actions.itemOutbound', 'index.actions.virtualPos', 'index.actions.records', 'index.actions.itemManage', 'index.actions.schedule'] },
				{ "title": "技师", "level": 21, "group": 20, menus: ['index.actions.handover', 'index.actions.sealing', 'index.actions.stockIn', 'index.actions.itemOutbound', 'index.actions.virtualPos', 'index.actions.records', 'index.actions.itemManage', 'index.actions.schedule'] },
				{ "title": "护工", "level": 11, "group": 10, menus: ['index.actions.cleaning'] },
				{ "title": "后勤", "level": 10, "group": 10, menus: ['index.actions.cleaning'] },
				{
					"title": "其他", "level": 1, "group": 1, menus: ['index.actions.schedule']
				}
			],
			statusBarHeight: 20, // Default fallback
			scrollTop: 0,
			scrollTopValue: 0, // Controlled scroll position
			isRefreshing: false, // Pull-to-refresh state
			isPulling: false,
			// currentStatus removed, using computed logic
			version: config.version,
			companyName: config.company,
			currentYear: new Date().getFullYear(),
			// Actions - Unified background
			allActions: [
				{ labelKey: 'index.actions.handover', iconPath: '/static/imgs/handover.png', reminder: true },
				{ labelKey: 'index.actions.stockIn', iconPath: '/static/imgs/inbound.png' },
				{ labelKey: 'index.actions.itemOutbound', iconPath: '/static/imgs/outbound.png' },
				{ labelKey: 'index.actions.virtualPos', iconPath: '/static/imgs/position.png' },
				{ labelKey: 'index.actions.sealing', iconPath: '/static/imgs/seal.png' },
				// { labelKey: 'index.actions.inventory', iconPath: '/static/imgs/item.png', reminder: true },
				{ labelKey: 'index.actions.records', iconPath: '/static/imgs/rescue.png' },
				{ labelKey: 'index.actions.schedule', iconPath: '/static/imgs/schedule.png' },
				// { labelKey: 'index.actions.report', icon: '🔧' },
				// { labelKey: 'index.actions.learning', icon: '📖' },
				{ labelKey: 'index.actions.itemManage', iconPath: '/static/imgs/item.png' },
				{ labelKey: 'index.actions.cleaning', iconPath: '/static/imgs/clean.png' },
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
			warningModal: {
				visible: false,
				title: '',
				message: '',
				buttonText: '',
				cancelText: '',
				showCancel: false,
				showClose: false,
				icon: '💡',
				type: ''
			},
			currentRescueInfo: null, // Store latest rescue info
			rescueDuration: '', // Formatted duration string
			rescueTimer: null
		};
	},
	onUnload() {
		if (this.rescueTimer) clearInterval(this.rescueTimer);
	},
	onLoad() {
		const sysInfo = uni.getSystemInfoSync();
		this.statusBarHeight = sysInfo.statusBarHeight || 20;
	},
	onShow() {
		// Check if user is logged in
		const token = storage.getToken();
		if (!token) {
			// Redirect to profile page if not logged in
			uni.reLaunch({
				url: '/pages/profile/profile'
			});
			return;
		}

		// #ifdef APP-PLUS
		if (uni.getSystemInfoSync().platform === 'android') {
			plus.navigator.setStatusBarBackground('transparent');
		}
		// #endif

		this.userInfo = storage.get('userInfo') || {};
		this.userOrgInfo = storage.get('userOrgInfo') || {};
		this.fetchRescueCars();
		this.fetchTodoData();

		// Check for mandatory updates if not checked yet
		// #ifdef APP-PLUS
		getApp().checkUpdate();
		// #endif

	},
	onBackPress(options) {
		// #ifdef APP-PLUS
		if (options.from === 'navigateBack') {
			return false;
		}
		const now = Date.now();
		if (!this.lastBackPressTime || now - this.lastBackPressTime > 2000) {
			this.lastBackPressTime = now;
			uni.showToast({
				title: '再按一次退出应用',
				icon: 'none'
			});
		} else {
			uni.showModal({
				title: '提示',
				content: '确定要退出程序吗？',
				success: (res) => {
					if (res.confirm) {
						plus.runtime.quit();
					}
				}
			});
		}
		return true;
		// #endif
	},
	computed: {
		actions() {
			const userOrgInfo = storage.get('userOrgInfo');
			const title = (userOrgInfo && userOrgInfo.User && userOrgInfo.User.Title) || (userOrgInfo && userOrgInfo.Title) || '';
			const position = this.positionData.find(p => p.title === title);
			if (!position || !position.menus) {
				return [{ labelKey: 'index.actions.schedule', iconPath: '/static/imgs/schedule.png' },];
			}
			return this.allActions.filter(action => position.menus.includes(action.labelKey));
		},
		actionPages() {
			const pages = [];
			const pageSize = 8;
			for (let i = 0; i < this.actions.length; i += pageSize) {
				pages.push(this.actions.slice(i, i + pageSize));
			}
			return pages;
		},
		currentRescueCar() {
			if (this.rescueCars && this.rescueCars.length > 0) {
				return this.rescueCars[this.currentRescueCarIndex];
			}
			return null;
		},
		statusConfig() {
			const car = this.currentRescueCar;
			const status = car ? car.Status : 0; // Default to normal if no car or unknown

			// Configuration Map
			// 0: Normal, 10: Abnormal, 11: Confirming, 20: Dirty, 21: Cleaning, 30: Repair, 31: Fixing
			const map = {
				0: { class: 'status-normal', text: this.$t('bigScreen.rescueCar.statusNormal') || '正常' },
				10: { class: 'status-abnormal', text: this.$t('bigScreen.rescueCar.statusAbnormal') || '异常' },
				11: { class: 'status-confirming', text: this.$t('bigScreen.rescueCar.statusConfirming') || '异常待确认' },
				20: { class: 'status-dirty', text: this.$t('bigScreen.rescueCar.statusDirty') || '脏污' },
				21: { class: 'status-cleaning', text: this.$t('bigScreen.rescueCar.statusCleaning') || '清洁中' },
				30: { class: 'status-repair', text: this.$t('bigScreen.rescueCar.statusRepair') || '报修' },
				31: { class: 'status-fixing', text: this.$t('bigScreen.rescueCar.statusFixing') || '维修中' }
			};

			return map[status] || map[0];
		},
		statusTagClass() {
			// Deprecated usage in template, mapped via CSS selectors now
			return '';
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
			const userInfo = storage.get('userInfo');
			const name = (userInfo && userInfo.userTrueName) || '';
			// #ifdef H5
			return this.$t('index.greeting', { name: name });
			// #endif
			// #ifndef H5
			const template = this.$t('index.greeting');
			return formatMessage(template, { name: name });
			// #endif
		},
		isRescuing() {
			if (this.currentRescueInfo && this.currentRescueInfo.Status == 0) {
				return true;
			}
			return false;
		}
	},
	methods: {
		onSearch() {
			console.log('Search clicked');
			// Navigate to search page or show search modal
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
			Promise.all([this.fetchRescueCars(), this.fetchTodoData()]).then(() => {
				setTimeout(() => {
					this.isRefreshing = false;
					this.isPulling = false;
					uni.showToast({
						title: this.$t('index.refreshed'),
						icon: 'none'
					});
				}, 800);
			}).catch(() => {
				this.isRefreshing = false;
				this.isPulling = false;
			});
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
			uni.navigateTo({
				url: page
			});
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
			} else if (action.labelKey === 'index.actions.schedule') {
				uni.navigateTo({
					url: '/pages/schedule/schedule'
				});
			} else if (action.labelKey === 'index.actions.records') {
				let carId = '';
				if (this.rescueCars && this.rescueCars.length > 0) {
					const car = this.rescueCars[this.currentRescueCarIndex];
					carId = car.Id;
				}
				uni.navigateTo({
					url: `/pages/rescue/rescue-list?carId=${carId}`
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
					this.dashboardStats.TotalPending = res.total || 0; // Update for new badge
				})
				.catch(e => {
					console.error("Fetch Todo Failed", e);
					this.alerts = [];
					this.todoTotal = 0;
					this.dashboardStats.TotalPending = 0; // Update for new badge
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
					// Restore last selected car
					const lastId = storage.get('lastSelectedRescueCarId');
					if (lastId) {
						// Use loose comparison to handle potential string/number mismatch
						const idx = this.rescueCars.findIndex(c => c.Id == lastId);
						if (idx > -1) {
							this.currentRescueCarIndex = idx;
						} else {
							this.currentRescueCarIndex = 0;
						}
					} else {
						this.currentRescueCarIndex = 0;
					}
					// Fetch stats
					// Fetch stats
					this.fetchDashboardStats();
					this.fetchRescueInfo();
					// Save current car info to cache
					const currentCar = this.rescueCars[this.currentRescueCarIndex];
					if (currentCar) {
						storage.set('currentRescueCarInfo', {
							Id: currentCar.Id,
							Name: currentCar.DisplayName
						});
					}
				} else {
					// Guide mode handled in template
				}
			}).catch(e => console.error(e));
		},
		onRescueCarChange(e) {
			this.currentRescueCarIndex = e.detail.value;
			// Save selection
			const car = this.rescueCars[this.currentRescueCarIndex];
			if (car) {
				storage.set('lastSelectedRescueCarId', car.Id);
				storage.set('currentRescueCarInfo', {
					Id: car.Id,
					Name: car.DisplayName
				});
			}
			this.fetchDashboardStats();
			this.fetchRescueInfo();
		},



		openAddRescueCar() {
			if (!this.userOrgInfo.Department || !this.userOrgInfo.Department.Id) {
				this.warningModal = {
					visible: true,
					title: this.$t('common.tip'),
					message: this.$t('profile.please_join_dept_first'),
					buttonText: this.$t('common.confirm')
				};
				return;
			}
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
		showStatusRemark() {
			const car = this.rescueCars[this.currentRescueCarIndex];
			if (car && car.Remark) {
				uni.showModal({
					title: '当前状态：' + this.getRescueCarStatusText(car.Status),
					content: car.Remark,
					showCancel: false,
					confirmText: this.$t('common.confirm') || '确定'
				});
			}
		},
		fetchDashboardStats() {
			if (this.rescueCars.length === 0) return;
			const currentCar = this.rescueCars[this.currentRescueCarIndex];
			if (!currentCar) return;
			request.get(`api/ssbh/Position/Statistic?warehouseId=${currentCar.Id}`)
				.then(res => {
					if (res) {
						let price = (res.PriceCounter || 0) / 10000 / 100; // Cent to Yuan (100), then to 10k (10000) => / 1000000? 
						// Request: "单位为分，需要转化为万元，保留两位小
						// 1  100 
						// 1 万元 = 10000  1000000 
						price = (res.PriceCounter || 0) / 1000000;
						this.dashboardStats = {
							ItemCounter: res.ItemCounter || 0,
							PriceCounter: price.toFixed(2),
							AbnormalStock: res.AbnormalStock || 0,
							AbnormalExpiry: res.AbnormalExpiry || 0,
							TotalPending: this.dashboardStats.TotalPending || 0 // Keep existing TotalPending
						};
					}
				})
				.catch(err => {
					console.error('Fetch Dashboard Stats Failed', err);
				});
		},
		fetchRescueInfo() {
			if (this.rescueCars.length === 0) return;
			const currentCar = this.rescueCars[this.currentRescueCarIndex];
			if (!currentCar) return;

			// Fetch latest rescue info for this car
			request.post('api/ssbh/Rescue/GetPageData', {
				page: 1,
				rows: 1,
				sort: 'Id',
				order: 'desc',
				wheres: JSON.stringify([{ name: 'CartId', value: currentCar.Id }])
			}).then(res => {
				if (res && res.rows && res.rows.length > 0) {
					this.currentRescueInfo = res.rows[0];
					// Cache the current task for potential edit
					storage.set('currentRescueTask', this.currentRescueInfo);
					this.startRescueTimer();
				} else {
					this.currentRescueInfo = null;
					storage.remove('currentRescueTask');
					this.stopRescueTimer();
				}
			}).catch(e => {
				console.error("Fetch Rescue Info Failed", e);
				this.currentRescueInfo = null;
				storage.remove('currentRescueTask');
				this.stopRescueTimer();
			});
		},
		startRescueTimer() {
			this.stopRescueTimer();
			if (this.currentRescueInfo && this.currentRescueInfo.StartTime) {
				this.updateRescueDuration();
				this.rescueTimer = setInterval(this.updateRescueDuration, 33); // High frequency update
			}
		},
		updateRescueDuration() {
			if (!this.currentRescueInfo || !this.currentRescueInfo.StartTime) return;
			const start = dayjs(this.currentRescueInfo.StartTime);
			const now = dayjs();
			const diff = now.diff(start); // Difference in milliseconds

			const days = Math.floor(diff / 86400000);
			const remainderDay = diff % 86400000;
			const hours = Math.floor(remainderDay / 3600000);
			const remainderHour = remainderDay % 3600000;
			const minutes = Math.floor(remainderHour / 60000);
			const remainderMinute = remainderHour % 60000;
			const seconds = Math.floor(remainderMinute / 1000);
			const millis = remainderMinute % 1000;

			const pad = (n, width = 2) => n.toString().padStart(width, '0');
			const msPad = (n) => n.toString().padStart(3, '0');

			let durationStr = `${pad(hours)}:${pad(minutes)}:${pad(seconds)}.${msPad(millis)}`;

			if (days > 0) {
				durationStr = `${days}天 ${durationStr}`;
			}

			this.rescueDuration = durationStr;
		},
		stopRescueTimer() {
			if (this.rescueTimer) {
				clearInterval(this.rescueTimer);
				this.rescueTimer = null;
			}
			this.rescueDuration = '';
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
			const date = dayjs(timeStr);
			if (!date.isValid()) return timeStr;
			return date.format('YYYY-MM-DD HH:mm');
		},
		getStatusText(status) {
			// Reuse NotifyStatus if applicable, or generic mapping
			if (status == 200) return '已完成';
			if (status == 210) return '已取消';
			if (status == 220) return '已忽略';
			return status;
		},
		getRescueCarStatusText(status) {
			return getRescueCarStatusText(status, this.$t);
		},
		handleRescueClick() {
			if (this.isRescuing) {
				this.warningModal = {
					visible: true,
					title: this.$t('rescue.end_confirm_title'),
					message: this.$t('rescue.end_confirm_msg'),
					buttonText: this.$t('rescue.end_btn'),
					showCancel: true,
					showClose: false,
					cancelText: this.$t('rescue.wait_btn'),
					icon: '🏥',
					type: 'endRescue'
				};
			} else {
				this.warningModal = {
					visible: true,
					title: this.$t('rescue.start_confirm_title'),
					message: this.$t('rescue.start_confirm_msg'),
					buttonText: this.$t('rescue.start_btn'),
					cancelText: this.$t('rescue.supplement_btn'),
					showCancel: true,
					showClose: true,
					icon: '⚡',
					type: 'startRescue'
				};
			}
		},
		onWarningConfirm() {
			if (this.warningModal.type === 'endRescue') {
				this.endRescue();
			} else if (this.warningModal.type === 'addCar') {
				this.navTo('/pages/position-manage/position-manage');
			} else if (this.warningModal.type === 'startRescue') {
				this.startRescueImmediately();
			}
			this.warningModal.visible = false;
		},
		onWarningCancel() {
			if (this.warningModal.type === 'startRescue') {
				this.handleSupplementInfo();
			}
			this.warningModal.visible = false;
		},
		onWarningClose() {
			this.warningModal.visible = false;
		},
		async startRescueImmediately() {
			if (!this.currentRescueCar) {
				uni.showToast({ title: this.$t('rescue.select_car_tip'), icon: 'none' });
				return;
			}

			uni.showLoading({ title: this.$t('rescue.starting') });
			const now = dayjs();
			const payload = {
				Name: this.$t('rescue.unnamed'),
				StartTime: now.format('YYYY-MM-DDTHH:mm:ss'),
				CartId: this.currentRescueCar.Id,
				CartName: this.currentRescueCar.DisplayName,
				DepartmentName: (this.userOrgInfo && this.userOrgInfo.Department && this.userOrgInfo.Department.Name) || '',
				Status: 0 // Ongoing
			};

			try {
				const res = await request.post('api/ssbh/Rescue/Add', { mainData: payload });
				if (res.success || res.status) {
					uni.showToast({ title: this.$t('rescue.start_success'), icon: 'success' });
					// 刷新首页数据
					this.fetchDashboardStats();
					this.fetchRescueInfo();
					this.fetchTodoData();
				} else {
					uni.showToast({ title: res.msg || this.$t('common.fail'), icon: 'none' });
				}
			} catch (e) {
				console.error("Start Rescue Immediately Failed", e);
				uni.showToast({ title: this.$t('common.fail'), icon: 'none' });
			} finally {
				uni.hideLoading();
			}
		},
		handleSupplementInfo() {
			uni.navigateTo({
				url: '/pages/rescue/rescue-record'
			});
		},
		// Old method if used by other calls, redirected to generic handler
		closeWarningModal() {
			this.warningModal.visible = false;
		},
		endRescue() {
			if (!this.currentRescueInfo || !this.currentRescueInfo.Id) return;

			// 1. Prepare Update Data: Set Status to 10 (Finished)
			const mainData = {
				...this.currentRescueInfo,
				Status: 20,
				EndTime: dayjs().format('YYYY-MM-DD HH:mm:ss')
			};

			console.log("EndRescue: Submitting Update", mainData);

			// 2. Call Update Interface
			request.post('api/ssbh/Rescue/Update', { mainData: mainData }).then(res => {
				if (res.success || res.status) {
					uni.showToast({ title: this.$t('rescue.end_success'), icon: 'success' });

					// 3. Update Local Cache: Clear active task
					this.currentRescueInfo = null;
					storage.remove('currentRescueTask');
					this.stopRescueTimer();

					// 4. Refresh Dashboard
					this.fetchDashboardStats();
					this.fetchRescueInfo();
				} else {
					uni.showToast({ title: res.msg || '操作失败', icon: 'none' });
				}
			}).catch(err => {
				console.error("EndRescue Failed", err);
				uni.showToast({ title: '操作失败', icon: 'none' });
			});
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
	background-color: $sull-color-primary;
}

.scroll-content {
	background-color: #f8f9fb;
	min-height: 100%;
	padding-bottom: 40rpx;
	border-radius: 32rpx 32rpx 0 0;
	/* Optional: smooth transition */
}

/* Header - Optimized for compactness and scrolling */
.header-fixed {
	position: fixed;
	top: 0;
	left: 0;
	width: 100%;
	z-index: 100;
	background-color: $sull-color-primary;
	/* Match page bg so it blends when scrolling */
}

.header-content {
	height: 88rpx;
	padding: 0 48rpx 20rpx 48rpx;
	display: flex;
	justify-content: space-between;
	align-items: center;
}

.header-right {
	display: flex;
	gap: 24rpx;
}

.greeting {
	font-size: $font-size-body-lg;
	/* Reduced from h5 */
	font-weight: $font-weight-bold;
	color: #ffffff;
	display: block;
}

.subtitle {
	font-size: $font-size-body-sm;
	/* Reduced from md */
	color: rgba(255, 255, 255, 0.9);
}

.icon-btn {
	position: relative;
	width: 64rpx;
	/* Smaller button */
	height: 64rpx;
	background: $sull-color-white;
	border: 2rpx solid $sull-color-line;
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
	top: -8rpx;
	right: -8rpx;
	background: $sull-color-error;
	color: $sull-color-white;
	font-size: 16rpx;
	padding: 2rpx 8rpx;
	border-radius: $radius-md;
	border: 2rpx solid $sull-color-white;
}



.main-scroll {
	flex: 1;
	height: 0;
}

.scroll-content {
	padding: 20rpx 48rpx 320rpx 48rpx;
	/* Increased bottom padding to clear nav bar */
}

/* Overview Card */
.overview-card {
	border-radius: 40rpx;
	padding: 32rpx;
	margin-top: 40rpx;
	margin-bottom: 40rpx;
	transition: all 0.3s ease;
}

.overview-card.normal {
	background: #F0F9EB;
	border: 2rpx solid #E89310;
}

.overview-card.warning {
	background: #FDF6EC;
	border: 2rpx solid #E89310;
}

.overview-card.abnormal {
	background: #FEF0F0;
	border: 4rpx solid #F56C6C;
	/* Thicker border for glaring effect */
}

/* Status-Based Colors */
/* Normal #2E7D32 #E8F5E9 */
.status-normal {
	background: #E8F5E9;
	border: 2rpx solid #2E7D32;

	.overview-title,
	.status-text,
	.arrow-down {
		color: #2E7D32;
	}

	.status-tag {
		background: rgba(46, 125, 50, 0.1);
	}
}

/* Confirming #F9A825 #FFF8E1 */
.status-confirming {
	background: #FFF8E1;
	border: 2rpx solid #F9A825;

	.overview-title,
	.status-text,
	.arrow-down {
		color: #F9A825;
	}

	.status-tag {
		background: rgba(249, 168, 37, 0.1);
	}
}

/* Dirty #C0A16B #FAF7F0 */
.status-dirty {
	background: #FAF7F0;
	border: 2rpx solid #C0A16B;

	.overview-title,
	.status-text,
	.arrow-down {
		color: #C0A16B;
	}

	.status-tag {
		background: rgba(192, 161, 107, 0.1);
	}
}

/* Abnormal #C62828 #FDECEA */
.status-abnormal {
	background: #FDECEA;
	border: 2rpx solid #C62828;

	.overview-title,
	.status-text,
	.arrow-down {
		color: #C62828;
	}

	.status-tag {
		background: rgba(198, 40, 40, 0.1);
	}
}

/* Repair #B71C1C #F9E5E5 */
.status-repair {
	background: #F9E5E5;
	border: 2rpx solid #B71C1C;

	.overview-title,
	.status-text,
	.arrow-down {
		color: #B71C1C;
	}

	.status-tag {
		background: rgba(183, 28, 28, 0.1);
	}
}

/* Cleaning #1976D2 #E3F2FD */
.status-cleaning {
	background: #E3F2FD;
	border: 2rpx solid #1976D2;

	.overview-title,
	.status-text,
	.arrow-down {
		color: #1976D2;
	}

	.status-tag {
		background: rgba(25, 118, 210, 0.1);
	}
}

/* Fixing #546E7A #ECEFF1 */
.status-fixing {
	background: #ECEFF1;
	border: 2rpx solid #546E7A;

	.overview-title,
	.status-text,
	.arrow-down {
		color: #546E7A;
	}

	.status-tag {
		background: rgba(84, 110, 122, 0.1);
	}
}

.overview-card.empty-frosted {
	background: rgba(255, 255, 255, 0.3);
	backdrop-filter: blur(20rpx);
	-webkit-backdrop-filter: blur(20rpx);
	border: 2rpx solid rgba(255, 255, 255, 0.5);
	box-shadow: 0 8rpx 32rpx rgba(0, 0, 0, 0.05);
	display: flex;
	align-items: center;
	justify-content: center;
	min-height: 260rpx;
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
	margin-bottom: 24rpx;
}

.overview-title {
	/* font-size: $font-size-body-lg; */
	font-size: 32rpx;
	font-weight: $font-weight-bold;
	/* Color handled by status class */
	/* color: $sull-text-main; */
}

.status-tag {
	padding: 8rpx 24rpx;
	border-radius: 40rpx;
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
	font-size: 36rpx;
	font-weight: $font-weight-bold;
	color: $sull-text-main;
	line-height: 1;
}

.stat-value-box {
	display: flex;
	align-items: baseline;
	justify-content: center;
	gap: 4rpx;
	margin-bottom: 8rpx;
}

.stat-unit {
	font-size: 24rpx;
	color: $sull-text-secondary;
	font-weight: normal;

	&.warning {
		color: #E89310;
	}

	&.danger {
		color: #F56C6C;
	}
}

/* Updated for light background contrast */
.stat-num.warning {
	color: #E89310;
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
	margin-bottom: 32rpx;
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
	height: 400rpx;
	/* Adjusted height for 2 rows */
	margin-bottom: 20rpx;
}

.action-grid {
	width: 100%;
	display: flex;
	flex-wrap: wrap;
	padding: 10rpx 0;
	box-sizing: border-box;
}

.action-item {
	/* Strict 25% width with no padding/borders to prevent wrapping */
	width: 25%;
	flex: 0 0 25%;
	display: flex;
	flex-direction: column;
	align-items: center;
	margin-bottom: 24rpx;
}

.action-icon-box {
	position: relative;
	/* Added for reminder positioning */
	width: 112rpx;
	height: 112rpx;
	border-radius: $radius-lg;
	display: flex;
	align-items: center;
	justify-content: center;
	margin-bottom: 16rpx;
	background-color: $sull-color-white;
	box-shadow: 0 4rpx 16rpx rgba(0, 0, 0, 0.03);
}

.reminder-bell {
	position: absolute;
	top: -16rpx;
	right: -16rpx;
	width: 40rpx;
	height: 40rpx;
	background-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='%23E89310'%3E%3Cpath d='M12 22c1.1 0 2-.9 2-2h-4c0 1.1.9 2 2 2zm6-6v-5c0-3.07-1.63-5.64-4.5-6.32V4c0-.83-.67-1.5-1.5-1.5s-1.5.67-1.5 1.5v.68C7.64 5.36 6 7.92 6 11v5l-2 2v1h16v-1l-2-2z'/%3E%3C/svg%3E");
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
	font-size: 48rpx;
}

.action-icon-img {
	width: 56rpx;
	height: 56rpx;
	margin-bottom: 8rpx;
}

.icon-img {
	width: 32rpx;
	height: 32rpx;
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
	width: 128rpx;
	height: 128rpx;
	border-radius: 50%;
	/* 3D Soft Gradient - Orange */
	background: linear-gradient(135deg, #FFD666, #E89310);
	/* Depth Shadows */
	box-shadow:
		0 20rpx 40rpx rgba(230, 162, 60, 0.25),
		inset 4rpx 4rpx 10rpx rgba(255, 255, 255, 0.5),
		inset -4rpx -4rpx 10rpx rgba(0, 0, 0, 0.1);
	display: flex;
	align-items: center;
	justify-content: center;
	margin-bottom: 24rpx;
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
	font-size: 64rpx;
	color: #FFFFFF;
	font-weight: bold;
	line-height: 1;
	margin-top: -8rpx;
}

.guide-title {
	font-size: 36rpx;
	font-weight: bold;
	color: #333;
	margin-bottom: 12rpx;
}

.guide-desc {
	font-size: 28rpx;
	color: #666;
	margin-bottom: 0;
}

.alert-list {
	display: flex;
	flex-direction: column;
	gap: 24rpx;
}

.alert-card {
	background: $sull-color-white;
	border-radius: $radius-lg;
	padding: 32rpx;
	display: flex;
	align-items: center;
	box-shadow: 0 4rpx 16rpx rgba(0, 0, 0, 0.02);
}

.alert-left {
	margin-right: 24rpx;
}

.alert-type-tag {
	padding: 8rpx 16rpx;
	border-radius: 8rpx;
	font-size: 22rpx;
	font-weight: bold;
}

.tag-danger {
	background: #FEF0F0;
	color: #F56C6C;
}

.tag-warning {
	background: #FDF6EC;
	color: #E89310;
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
	margin-bottom: 8rpx;
	line-height: 1.4;

	overflow: hidden;
	text-overflow: ellipsis;
	display: -webkit-box;
	-webkit-line-clamp: 2;
	line-clamp: 2;
	-webkit-box-orient: vertical;
}

.alert-time {
	font-size: 22rpx;
	color: $sull-text-secondary;
}

.alert-action {
	margin-left: 24rpx;
	display: flex;
	gap: 16rpx;
}

.icon-action-btn {
	width: 64rpx;
	height: 64rpx;
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
		border: 2rpx solid #D0E7FF;
	}

	&.ignore-btn {
		background: #F4F4F5;
		border: 2rpx solid #E9E9EB;
	}
}

.action-icon-small {
	font-size: 32rpx;
	line-height: 1;
}

/* Simple Empty State */
.empty-state-simple {
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	padding: 60rpx 0;
}

.empty-icon-simple {
	width: 160rpx;
	height: 160rpx;
	margin-bottom: 8rpx;
	background-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='%23DCDFE6'%3E%3Cpath d='M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm-5 14H7v-2h7v2zm3-4H7v-2h10v2zm0-4H7V7h10v2z'/%3E%3C/svg%3E");
	background-size: contain;
	background-repeat: no-repeat;
	opacity: 0.5;
}

.empty-text-simple {
	font-size: 26rpx;
	color: $sull-text-secondary;
}

.bottom-spacer {
	height: 120rpx;
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
	border-radius: 24rpx;
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
	padding: 32rpx;
	border-bottom: 2rpx solid #EBEEF5;
	display: flex;
	justify-content: space-between;
	align-items: center;

	.modal-title {
		font-size: 32rpx;
		font-weight: bold;
		color: #303133;
	}

	.close-btn {
		font-size: 40rpx;
		color: #909399;
		padding: 8rpx;
	}
}

.modal-body {
	padding: 40rpx 32rpx;
}

.modal-input {
	height: 96rpx;
	background: #F5F7FA;
	border-radius: 16rpx;
	padding: 0 24rpx;
	font-size: 28rpx;
	margin-bottom: 32rpx;

	&:last-child {
		margin-bottom: 0;
	}
}

.modal-footer {
	display: flex;
	padding: 0 32rpx 32rpx 32rpx;
	gap: 24rpx;
}

.btn {
	flex: 1;
	font-size: 28rpx;
	border-radius: 40rpx;

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
	padding: 40rpx;
	box-sizing: border-box;
}

.timeline-list {
	padding-left: 20rpx;
}

.timeline-item {
	position: relative;
	padding-left: 40rpx;
	padding-bottom: 48rpx;
}

.timeline-item:last-child {
	padding-bottom: 0;
}

.timeline-node {
	position: absolute;
	left: 0;
	top: 0;
	width: 40rpx;
	height: 100%;
	display: flex;
	justify-content: center;
}

.timeline-dot {
	width: 16rpx;
	height: 16rpx;
	border-radius: 50%;
	background-color: $sull-color-primary;
	margin-top: 32rpx;
	/* Align with content top approx */
	z-index: 1;
}

.node-icon {
	width: 32rpx;
	height: 32rpx;
	margin-top: 24rpx;
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
	left: 18rpx;
	top: 48rpx;
	/* Start below first node/dot */
	bottom: -20rpx;
	/* Extend to next */
	width: 4rpx;
	background-color: #E4E7ED;
}

.timeline-item:last-child .timeline-line {
	display: none;
}

/* Adjust line for start/end icons which are larger */
.timeline-node.is-start~.timeline-line {
	top: 56rpx;
}

/* If end node is present, the previous line needs to connect to it. 
   css logic for 'previous line' is hard in pure css without complex selectors if height varies.
   But strictly, the line belongs to the item. 
   So item N's line goes to item N+1.
*/

.timeline-content {
	background: #F8F9FB;
	padding: 24rpx;
	border-radius: 16rpx;
	margin-left: 20rpx;
	/* Space from dot layout */
}

.timeline-header {
	display: flex;
	justify-content: space-between;
	margin-bottom: 12rpx;
	font-size: 24rpx;
	color: #909399;
}

.timeline-desc {
	font-size: 28rpx;
	color: #303133;
	margin-bottom: 12rpx;
	line-height: 1.4;
}

.timeline-footer {
	font-size: 24rpx;
	color: #606266;
	display: flex;
	flex-wrap: wrap;
	gap: 16rpx;
}

.timeline-status {
	color: $sull-color-primary;
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
	padding: 80rpx 0;
}

.empty-text-simple {
	font-size: 28rpx;
	color: #999;
	font-weight: normal;
	letter-spacing: 1rpx;
}

.empty-icon-simple {
	width: 96rpx;
	height: 96rpx;
	min-width: 96rpx;
	min-height: 96rpx;
	flex-shrink: 0;
	margin-bottom: 16rpx;
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

/* Todo Tabs Styling integrated via component */

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
	border-radius: 24rpx;
	padding: 40rpx;
}

.modal-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-bottom: 40rpx;
}

.modal-title {
	font-size: 36rpx;
	font-weight: bold;
}

.close-btn {
	font-size: 48rpx;
	color: #999;
}

.modal-body {
	margin-bottom: 40rpx;
}

.modal-input {
	border: 2rpx solid #eee;
	border-radius: 16rpx;
	padding: 20rpx;
	margin-bottom: 24rpx;
	font-size: 28rpx;
	display: block;
	width: 100%;
	box-sizing: border-box;
}

.modal-footer {
	display: flex;
	justify-content: flex-end;
	gap: 24rpx;
}

.icon-action-btn {
	width: 64rpx;
	height: 64rpx;
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
		border: 2rpx solid #D0E7FF;
	}

	&.ignore-btn {
		background: #F4F4F5;
		border: 2rpx solid #E9E9EB;
	}
}

.action-icon-svg {
	width: 32rpx;
	height: 32rpx;
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

/* Emergency Logic */
.header-fixed.header-emergency {
	background-color: #ffeaea;
	/* Light red bg */
	border-bottom: 2rpx solid #ff4d4f;
	overflow: hidden;
}

.flash-effect {
	position: absolute;
	top: 0;
	left: 0;
	width: 100%;
	height: 100%;
	background: linear-gradient(135deg, rgba(255, 255, 255, 0) 0%, rgba(255, 255, 255, 0.8) 50%, rgba(255, 255, 255, 0) 100%);
	transform: translate(-100%, -100%);
	animation: flash 2s infinite;
	pointer-events: none;
	z-index: 1;
}

@keyframes flash {
	0% {
		transform: translate(-100%, -100%);
	}

	100% {
		transform: translate(100%, 100%);
	}
}

/* Rescue btn animation disabled
.rescue-btn.rescuing {
	animation: pulse-rotate 1s infinite;
	box-shadow: 0 0 16rpx rgba(255, 0, 0, 0.8);
	background-color: #fff0f0;
	border: 2rpx solid #ff4d4f;
}

@keyframes pulse-rotate {
	0% {
		transform: scale(1) rotate(0deg);
		box-shadow: 0 0 0 rgba(255, 77, 79, 0.7);
	}

	50% {
		transform: scale(1.1) rotate(10deg);
		box-shadow: 0 0 20rpx rgba(255, 77, 79, 1);
	}

	100% {
		transform: scale(1) rotate(0deg);
		box-shadow: 0 0 0 rgba(255, 77, 79, 0.7);
	}
}
*/

.overview-card.status-rescuing {
	background: linear-gradient(135deg, #fff0f0 0%, #ffeaea 100%);
	box-shadow: 0 0 0 rgba(255, 77, 79, 0);
	border: 2rpx solid #ff4d4f;
	animation: heartbeat 1.5s infinite;
}

.overview-card.status-rescuing .status-text {
	color: #ff4d4f !important;
	font-weight: bold;
}

@keyframes heartbeat {
	0% {
		box-shadow: 0 0 0 0 rgba(255, 77, 79, 0.4);
	}

	15% {
		box-shadow: 0 0 24rpx 8rpx rgba(255, 77, 79, 0.8);
	}

	30% {
		box-shadow: 0 0 4rpx 0 rgba(255, 77, 79, 0.4);
	}

	45% {
		box-shadow: 0 0 48rpx 12rpx rgba(255, 77, 79, 0.9);
		/* Larger and stronger second beat */
	}

	70% {
		box-shadow: 0 0 0 0 rgba(255, 77, 79, 0);
	}

	100% {
		box-shadow: 0 0 0 0 rgba(255, 77, 79, 0);
	}
}

.status-container {
	display: flex;
	align-items: center;
	gap: 16rpx;
}

.rescuing-tag {
	padding: 4rpx 16rpx;
	border: 2rpx solid #ff4d4f;
	border-radius: 8rpx;
	color: #ff4d4f;
	font-size: 24rpx;
	font-weight: bold;
	background: rgba(255, 77, 79, 0.1);
}

.rescue-timer {
	font-family: monospace;
	/* Monospaced for stable digits */
	color: #ff4d4f;
	font-weight: bold;
	font-size: 28rpx;
}
</style>

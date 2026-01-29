<template>
	<view class="page-container" @click="closeAllMenus">
		<!-- 主要内容 -->
		<scroll-view class="scroll-content" scroll-y :enable-back-to-top="true" :refresher-enabled="true"
			:refresher-triggered="isRefreshing" :refresher-threshold="60" @refresherrefresh="onRefresh"
			@refresherpulling="onPulling" @refresherrestore="onRestore" @refresherabort="onRestore"
			@scrolltolower="loadMore" @scroll="onScroll" :scroll-top="scrollTopVal" refresher-default-style="none"
			refresher-background="#f0f5ff">

			<CustomRefresher slot="refresher" :isRefreshing="isRefreshing" :isPulling="isPulling" />

			<!-- 自定义导航栏 (现在在滚动视图内) -->
			<view class="custom-nav" :style="{ paddingTop: statusBarHeight * 2 + 'rpx' }">
				<view class="nav-content">
					<view class="nav-left">
						<BackButton class="nav-back-btn" @click="goBack" />
					</view>
					<text class="nav-title">{{ $t('sealing.title') }}</text>
					<view class="nav-right"></view>
				</view>
			</view>

			<view class="content-wrapper">
				<!-- 统计卡片 -->
				<view class="stats-card overview-card">
					<view class="stats-row">
						<view class="stat-item">
							<text class="stat-num text-primary">{{ stats.today }}</text>
							<text class="stat-label">{{ $t('sealing.today') }}</text>
						</view>
						<view class="stat-divider"></view>
						<view class="stat-item">
							<text class="stat-num text-success">{{ stats.sealed }}</text>
							<text class="stat-label">{{ $t('sealing.sealed') }}</text>
						</view>
						<view class="stat-divider"></view>
						<view class="stat-item">
							<text class="stat-num text-warning">{{ stats.unsealed }}</text>
							<text class="stat-label">{{ $t('sealing.unsealed') }}</text>
						</view>
						<view class="stat-divider"></view>
						<view class="stat-item">
							<text class="stat-num text-danger">{{ stats.total }}</text>
							<text class="stat-label">{{ $t('sealing.total') }}</text>
						</view>
					</view>
				</view>

				<!-- 日历选择 -->
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
						<view class="new-seal-btn" @click="openNewSealModal">
							<text class="plus-icon">+</text>
							<text class="btn-text">{{ $t('sealing.new') }}</text>
						</view>
					</view>
					<view class="end-controls">
						<view class="icon-btn" @click="handleShare"><text>📤</text></view>
						<view class="icon-btn" @click="handleExport"><text>📑</text></view>
						<view class="icon-btn" @click="handlePrint"><text>🖨️</text></view>
					</view>
				</view>

				<!-- 封存记录列表 -->
				<view class="list-section">
					<view v-if="isLoading" class="loading-state">
						<text class="loading-spinner">↻</text>
						<text>{{ $t('sealing.loading') }}</text>
					</view>

					<view v-else-if="sealList.length === 0" class="empty-state">
						<text class="empty-icon">🔒</text>
						<text class="empty-text">{{ $t('sealing.empty') }}</text>
					</view>

					<view v-else class="seal-list">
						<view v-for="(item, index) in sealList" :key="item.Id" class="seal-item"
							:class="{ 'new-item': item.isNew }">
							<!-- 行1: 时间 | 操作类型、状态、操作 -->
							<view class="item-header">
								<text class="item-time">{{ formatTime(item.CreationTime) }}</text>
								<view class="item-meta-right">
									<view class="status-tag" :class="getCategoryClass(item.Category)">
										<text class="status-text">{{ getCategoryText(item.Category) }}</text>
									</view>
									<view class="status-tag" :class="getStatusClass(item.Status)">
										<text class="status-text">{{ getStatusText(item.Status) }}</text>
									</view>
									<view class="action-menu" @click.stop="toggleActionMenu(item)">
										<text class="menu-icon">⋯</text>
										<!-- 操作菜单 -->
										<view class="action-dropdown"
											v-if="getRecordId(activeActionItemId) === getRecordId(item)">
											<view class="action-item" @click.stop="confirmSeal(item)"
												v-if="getRecordStatus(item) === 0">
												<text class="action-icon">✅</text>
												<text class="action-text">{{ $t('sealing.confirm') }}</text>
											</view>
											<view class="action-item" @click.stop="editSeal(item)">
												<text class="action-icon">✏️</text>
												<text class="action-text">{{ $t('sealing.edit') }}</text>
											</view>
											<view class="action-item delete-item" @click.stop="deleteSeal(item)"
												v-if="getRecordStatus(item) === 0">
												<text class="action-icon">🗑️</text>
												<text class="action-text">{{ $t('sealing.delete') }}</text>
											</view>
										</view>
									</view>
								</view>
							</view>

							<!-- 行2: 编码、确认人、备注 -->
							<view class="item-content">
								<text class="item-code">{{ item.Code || $t('sealing.noCode') }}</text>
								<text class="item-assign" v-if="item.AssignUser">{{ $t('sealing.confirmer') }}: {{
									item.AssignUser }}</text>
								<text class="item-remark" v-if="item.Remark">{{ item.Remark }}</text>
							</view>

							<!-- 行3: 操作人 -->
							<view class="item-footer">
								<text class="item-creator">{{ $t('sealing.operator') }}: {{ item.Creator ||
									item.CreatorUser || item.CreateUser || item.UserName || '-' }}</text>
							</view>

						</view>
					</view>
				</view>

				<!-- 底部版权信息 -->
				<FooterInfo :showFinished="sealList.length > 0" />
				<view class="bottom-spacer"></view>
			</view>
		</scroll-view>

		<!-- 回到顶部按钮 -->
		<BackToTop :scroll-top="scrollTop" @click="scrollToTop" />

		<!-- 新增封存模态框 -->
		<view class="modal-overlay" v-if="showNewSealModal" @click="closeNewSealModal"
			@touchmove.stop.prevent="doNothing">
			<view class="modal-content" @click.stop>
				<view class="modal-header">
					<text class="modal-title">{{ relatedName || '' }}{{ $t('sealing.newSeal') }}</text>
					<text class="close-btn" @click="closeNewSealModal">✖️</text>
				</view>
				<view class="modal-body">
					<view class="form-item">
						<text class="label">部门</text>
						<view class="value disabled">
							<text class="text-main">{{ userOrgInfo.Department ? userOrgInfo.Department.Name : '-'
							}}</text>
						</view>
					</view>
					<view class="form-item">
						<text class="label">{{ $t('sealing.status') }}</text>
						<view class="shift-selector">
							<view v-for="(status, index) in sealStatusOptions" :key="index" class="shift-chip"
								:class="{ active: sealForm.StatusIndex === index }" @click="selectStatus(index)">
								{{ status.text }}
							</view>
						</view>
					</view>
					<view class="form-item">
						<text class="label">{{ $t('sealing.operator') }}</text>
						<view class="value disabled">
							<text class="text-main">{{ currentUserDisplayName }}</text>
						</view>
					</view>
					<view class="form-item">
						<text class="label">{{ $t('sealing.code') }}</text>
						<view class="value">
							<input v-model="sealForm.Code" :placeholder="$t('sealing.codePlaceholder')"
								class="value-input" />
						</view>
					</view>
					<view class="form-item">
						<text class="label">{{ $t('sealing.confirmer') }}</text>
						<view class="value disabled" @click="openMemberSelector">
							<text :class="selectedConfirmer ? 'text-main' : 'text-placeholder'">
								{{ selectedConfirmer || $t('sealing.selectConfirmer') }}
							</text>
							<text class="picker-arrow">▼</text>
						</view>
						<text v-if="showConfirmerError" class="error-msg">{{ $t('sealing.selectConfirmerError')
						}}</text>
					</view>
					<view class="form-item">
						<text class="label">{{ $t('sealing.remark') }}</text>
						<view class="value">
							<textarea v-model="sealForm.Remark" :placeholder="$t('sealing.remarkPlaceholder')"
								class="value-textarea" auto-height></textarea>
						</view>
					</view>
				</view>
				<view class="modal-footer">
					<button class="modal-btn cancel" @click="closeNewSealModal">{{ $t('common.cancel') }}</button>
					<button class="modal-btn confirm" @click="submitNewSeal">{{ $t('common.confirm') }}</button>
				</view>
			</view>
		</view>
		<view class="member-overlay" v-if="showMemberSelector" @click.stop="doNothing"
			@touchmove.stop.prevent="doNothing">
			<view class="member-modal" @click.stop="doNothing">
				<view class="member-header">
					<view class="search-box">
						<input class="member-search-input" v-model="memberSearchKey"
							:placeholder="$t('sealing.searchMemberPlaceholder')" />
						<text v-if="memberSearchKey" class="clear-icon" @click="clearSearch">✖</text>
					</view>
					<text class="close-btn" @click="closeMemberSelector">✖️</text>
				</view>
				<scroll-view scroll-y class="member-list">
					<view class="member-item" v-for="(m, i) in filteredConfirmers" :key="i" @click="selectConfirmer(m)">
						<text class="member-name">{{ m.UserTrueName }}</text>
					</view>
					<view v-if="filteredConfirmers.length === 0" class="empty-state-small">
						<text>No Result</text>
					</view>
				</scroll-view>
			</view>
		</view>
		<WarningModal v-model:visible="warningModal.visible" :title="warningModal.title" :message="warningModal.message"
			:buttonText="warningModal.buttonText" @confirm="onWarningConfirm" />
	</view>
</template>

<script>
import BackButton from '@/components/BackButton/BackButton.vue';
import BackToTop from '@/components/BackToTop/BackToTop.vue';
import FooterInfo from '@/components/FooterInfo/FooterInfo.vue';
import WarningModal from '@/components/WarningModal/WarningModal.vue';
import config from '@/config.js';
import request from '@/utils/request.js';
import { storage } from '@/utils/storage.js';
import dayjs from 'dayjs';
import CustomRefresher from '@/components/CustomRefresher/CustomRefresher.vue';

export default {
	components: {
		BackButton,
		BackToTop,
		FooterInfo,
		WarningModal,
		CustomRefresher
	},
	data() {
		return {
			statusBarHeight: 0,
			scrollTop: 0,
			scrollTopVal: 0,
			isRefreshing: false,
			isPulling: false,
			isLoading: false,
			userOrgInfo: {},
			selectedDate: '',
			currentMonthYear: '',
			weekDays: [],
			stats: {
				today: 0,
				sealed: 0,
				unsealed: 0,
				today: 0,
				sealed: 0,
				unsealed: 0,
				total: 0
			},
			todaySealCount: -1,
			activeActionItemId: null,
			sealList: [],
			showNewSealModal: false,
			sealForm: {
				StatusIndex: 0,
				ConfirmerIndex: 0,
				Code: '',
				Remark: ''
			},
			sealStatusOptions: [],
			confirmerOptions: [],
			showMemberSelector: false,
			memberSearchKey: '',
			showConfirmerError: false,
			selectedConfirmer: '',
			relatedId: '',
			relatedName: '',
			todoListId: '',
			userInfo: {},
			warningModal: {
				visible: false,
				title: '',
				message: '',
				buttonText: ''
			}
		}
	},
	onLoad(options) {
		this.relatedId = options.relatedId || '';
		this.relatedName = options.relatedName || '';
		this.todoListId = options.todoListId || '';
		if (this.todoListId != '') {
			const systemInfo = uni.getSystemInfoSync();
			this.statusBarHeight = systemInfo.statusBarHeight || 20;

			// 清空日期，以便显示相关的封存记录（不限制为当天）
			this.selectedDate = '';
			this.initCalendar();
			this.initSealStatusOptions();
			this.confirmSeal({ Id: this.relatedId });
		} else {
			this.initPage();
		}
	},
	onShow() {
		const token = storage.getToken();
		if (!token) {
			uni.reLaunch({
				url: '/pages/profile/profile'
			});
			return;
		}
		this.userOrgInfo = storage.get('userOrgInfo') || {};
		this.userInfo = storage.get('userInfo') || {};
		this.fetchSealData();
		this.checkTodayStatus();
	},
	computed: {
		filteredConfirmers() {
			if (!this.memberSearchKey) {
				return this.confirmerOptions;
			}
			const searchKey = this.memberSearchKey.toLowerCase();
			return this.confirmerOptions.filter(member =>
				member.UserTrueName && member.UserTrueName.toLowerCase().includes(searchKey)
			);
		},
		currentUserDisplayName() {
			const user = this.userInfo || {};
			const orgUser = this.userOrgInfo?.User || {};
			return user.userTrueName || user.UserTrueName || orgUser.UserTrueName || orgUser.UserName || user.userName || '-';
		}
	},
	methods: {
		doNothing() {
			// 空方法，防止事件冒泡和默认行为
		},
		initPage() {
			const systemInfo = uni.getSystemInfoSync();
			this.statusBarHeight = systemInfo.statusBarHeight || 20;

			// 设置当前选中日期 (使用 dayjs 确保本地时间一致)
			this.selectedDate = dayjs().format('YYYY-MM-DD');
			this.initCalendar();
			this.initSealStatusOptions();
		},
		initCalendar() {
			const today = dayjs();
			this.currentMonthYear = today.format('YYYY年M月');

			const weekDays = [];
			const dayNames = ['日', '一', '二', '三', '四', '五', '六'];
			const startOfWeek = today.startOf('week');

			for (let i = 0; i < 7; i++) {
				const date = startOfWeek.add(i, 'day');
				const fullDate = date.format('YYYY-MM-DD');
				weekDays.push({
					name: dayNames[i],
					date: date.date(),
					fullDate: fullDate,
					hasData: false // 初始设为 false，由后续 fetch/check 更新
				});
			}
			this.weekDays = weekDays;
		},
		updateWeekDaysDot() {
			const todayStr = dayjs().format('YYYY-MM-DD');
			this.weekDays.forEach(day => {
				day.hasData = (day.fullDate === todayStr && this.todaySealCount > 0);
			});
		},
		initSealStatusOptions() {
			this.sealStatusOptions = [
				{ value: 0, text: this.$t('sealing.statusSealed') },
				{ value: 1, text: this.$t('sealing.statusUnsealed') }
			];
			this.fetchConfirmers();
		},
		fetchConfirmers() {
			// 获取可选择的确认人列表
			request.get('api/ssbh/OrganizationUser/DepartmentUser').then(res => {
				this.confirmerOptions = (res.rows || []).map(item => ({
					Id: item.UserId,
					UserTrueName: item.UserName
				})).filter(item => item.Id !== this.userOrgInfo.User?.UserId);
			}).catch(err => {
				console.error('获取确认人列表失败:', err);
				// 如果获取失败，使用当前用户作为确认人
				this.confirmerOptions = [
					{
						Id: this.userOrgInfo.User?.UserId,
						UserTrueName: this.userOrgInfo.User?.UserTrueName
					}
				];
			});
		},
		onDateChange(e) {
			this.selectedDate = e.detail.value;
			this.fetchSealData();
		},
		selectDate(date) {
			this.selectedDate = date;
			this.fetchSealData();
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
			this.fetchSealData();
		},
		onScroll(e) {
			this.scrollTop = e.detail.scrollTop;
		},
		scrollToTop() {
			this.scrollTopVal = this.scrollTop;
			this.$nextTick(() => {
				this.scrollTopVal = 0;
			});
		},
		loadMore() {
			// 加载更多数据
		},
		goBack() {
			uni.navigateBack();
		},
		handleShare() {
			uni.showToast({
				title: this.$t('sealing.shared'),
				icon: 'none'
			});
		},
		handleExport() {
			uni.showToast({
				title: this.$t('sealing.exported'),
				icon: 'none'
			});
		},
		handlePrint() {
			uni.showToast({
				title: this.$t('sealing.printing'),
				icon: 'none'
			});
		},
		fetchSealData() {
			this.isLoading = true;

			// 构建筛选条件
			const wheres = [];
			if (this.relatedId) {
				wheres.push({ name: 'RelatedId', value: this.relatedId });
			}

			// 日期筛选 - 获取当天的开始和结束时间
			if (this.selectedDate) {
				const startDate = this.selectedDate + ' 00:00:00';
				const endDate = this.selectedDate + ' 23:59:59';
				wheres.push({
					name: 'CreationTime',
					value: startDate,
					displayType: 'thanorequal'
				});
				wheres.push({
					name: 'CreationTime',
					value: endDate,
					displayType: 'lt'
				});
			}

			const params = {
				page: 1,
				rows: 50,
				sort: 'CreationTime',
				order: 'desc',
				wheres: JSON.stringify(wheres)
			};

			request.post('api/ssbh/SealHistory/GetPageData', params)
				.then(res => {
					this.sealList = (res.rows || []).map(item => ({
						...item
					}));

					// 如果是已选日期是今天，更新todaySealCount
					const today = dayjs().format('YYYY-MM-DD');
					if (this.selectedDate === today) {
						this.todaySealCount = res.total !== undefined ? res.total : this.sealList.length;
						this.updateWeekDaysDot();
					}

					this.updateStats();
				})
				.catch(err => {
					console.error('获取封存数据失败:', err);
					uni.showToast({
						title: this.$t('sealing.fetchError'),
						icon: 'none'
					});
				})
				.finally(() => {
					this.isLoading = false;
					setTimeout(() => {
						this.isRefreshing = false;
						this.isPulling = false;
						uni.showToast({ title: '已刷新', icon: 'none' });
					}, 800);
				});
		},
		checkTodayStatus() {
			const today = dayjs().format('YYYY-MM-DD');
			if (this.selectedDate === today) return;

			const tmr = dayjs().add(1, 'day').format('YYYY-MM-DD');

			const wheres = [
				{ name: 'RelatedId', value: this.relatedId },
				{ name: 'CreationTime', value: today + ' 00:00:00', displayType: 'thanorequal' },
				{ name: 'CreationTime', value: tmr + ' 00:00:00', displayType: 'lt' }
			];

			const params = {
				page: 1,
				rows: 1,
				wheres: JSON.stringify(wheres)
			};

			request.post('api/ssbh/SealHistory/GetPageData', params).then(res => {
				const count = res.total !== undefined ? res.total : (res.rows ? res.rows.length : 0);
				this.todaySealCount = count;
				// updateStats中的today是从sealList计算的，这里主要为了红点
				// this.stats.today 依赖于当前列表，如果是今天列表会自动更新，如果不是今天列表，stats.today显示的是列表数据的今天数量吗？
				// 原代码 updateStats: const todayList = this.sealList.filter(...)
				// 所以如果现在看的是以前的日期，stats.today会是0(因为列表里没有今天的)。
				// 保持原样即可，这里只更新红点
				this.updateWeekDaysDot();
			});
		},
		updateStats() {
			const today = dayjs().format('YYYY-MM-DD');
			const todayList = this.sealList.filter(item =>
				item.CreationTime && item.CreationTime.startsWith(today)
			);

			this.stats = {
				today: todayList.length,
				sealed: this.sealList.filter(item => item.Status === 0).length,
				unsealed: this.sealList.filter(item => item.Status === 1).length,
				total: this.sealList.length
			};
		},
		openNewSealModal() {
			this.showNewSealModal = true;
			this.resetForm();
		},
		closeNewSealModal() {
			this.showNewSealModal = false;
		},
		resetForm() {
			this.sealForm = {
				StatusIndex: 0,
				ConfirmerIndex: 0,
				Code: '',
				Remark: ''
			};
			this.selectedConfirmer = '';
			this.showConfirmerError = false;
			this.memberSearchKey = '';
		},
		selectStatus(index) {
			this.sealForm.StatusIndex = index;
		},
		openMemberSelector() {
			this.showMemberSelector = true;
		},
		closeMemberSelector() {
			this.showMemberSelector = false;
			this.memberSearchKey = '';
		},
		clearSearch() {
			this.memberSearchKey = '';
		},
		selectConfirmer(member) {
			this.selectedConfirmer = member.UserTrueName;
			this.sealForm.ConfirmerIndex = this.confirmerOptions.findIndex(m => m.Id === member.Id);
			this.showConfirmerError = false;
			this.closeMemberSelector();
		},
		submitNewSeal() {
			if (!this.validateForm()) {
				return;
			}

			const selectedStatus = this.sealStatusOptions[this.sealForm.StatusIndex];
			const selectedConfirmerObj = this.confirmerOptions[this.sealForm.ConfirmerIndex] || {};

			const data = {
				RelatedId: this.relatedId,
				Category: selectedStatus.value, // 封存类型
				AssignUserId: selectedConfirmerObj.Id || null,
				AssignUser: selectedConfirmerObj.UserTrueName || this.selectedConfirmer,
				Code: this.sealForm.Code.trim(),
				Remark: this.sealForm.Remark.trim()
			};

			uni.showLoading({ title: this.$t('sealing.submitting'), mask: true });

			request.post('api/ssbh/SealHistory/Add', { mainData: data })
				.then(res => {
					// Extremely flexible success detection
					let isSuccess = false;
					if (res) {
						if (res === '保存成功' || res.status === true || res.success === true) {
							isSuccess = true;
						} else if (typeof res === 'object' && !res.message && (res.Id || res.id)) {
							isSuccess = true;
						}
					}

					if (isSuccess) {
						uni.showToast({
							title: this.$t('sealing.submitSuccess'),
							icon: 'success'
						});
						this.closeNewSealModal();
						this.fetchSealData();
						// Update stats/dot
						const todayStr = dayjs().format('YYYY-MM-DD');
						this.todaySealCount++;
						this.updateWeekDaysDot();
					} else {
						const msg = res.message || res.msg || (typeof res === 'string' ? res : '');
						throw new Error(msg || '接口返回异常');
					}
				})
				.catch(err => {
					console.error('提交封存记录失败:', err);
					uni.showModal({
						title: this.$t('common.fail'),
						content: err.message || this.$t('sealing.submitError'),
						showCancel: false
					});
				})
				.finally(() => {
					uni.hideLoading();
				});
		},
		validateForm() {
			// 0. Check Related Context
			if (!this.relatedId) {
				console.error('Validation Failed: relatedId is missing');
				this.showWarning(this.$t('common.fail'));
				return false;
			}

			// 1. Check Code
			const code = (this.sealForm.Code || '').toString().trim();
			if (!code) {
				console.warn('Validation Failed: Code is empty');
				this.showWarning(this.$t('sealing.codeRequired'));
				return false;
			}

			// 2. Check Confirmer
			if (!this.selectedConfirmer) {
				console.warn('Validation Failed: Confirmer is not selected');
				this.showConfirmerError = true;
				this.showWarning(this.$t('sealing.selectConfirmerError'));
				return false;
			}

			this.showConfirmerError = false;
			return true;
		},
		showWarning(msg) {
			this.warningModal = {
				visible: true,
				title: this.$t('modal.warning_title'),
				message: msg,
				buttonText: this.$t('modal.confirm_btn')
			};
		},
		onWarningConfirm() {
			this.warningModal.visible = false;
		},
		toggleActionMenu(item) {
			const id = this.getRecordId(item);
			if (this.activeActionItemId === id) {
				this.activeActionItemId = null;
			} else {
				this.activeActionItemId = id;
			}
		},
		getRecordId(item) {
			if (!item) return null;
			if (typeof item === 'string' || typeof item === 'number') return item;
			return item.Id !== undefined ? item.Id : item.id;
		},
		getRecordStatus(item) {
			if (!item) return -1;
			return item.Status !== undefined ? item.Status : (item.status !== undefined ? item.status : -1);
		},
		closeAllMenus() {
			this.activeActionItemId = null;
		},
		getCategoryClass(category) {
			switch (category) {
				case 0: return 'status-sealed';     // 封存
				case 1: return 'status-unsealed';   // 启封
				default: return 'status-unknown';
			}
		},
		getCategoryText(category) {
			switch (category) {
				case 0: return this.$t('sealing.statusSealed');   // 封存
				case 1: return this.$t('sealing.statusUnsealed'); // 启封
				default: return this.$t('sealing.statusUnknown');
			}
		},
		getStatusClass(status) {
			switch (status) {
				case 0: return 'status-pending';    // 待确认
				case 1: return 'status-confirmed';  // 已确认
				default: return 'status-unknown';
			}
		},
		getStatusText(status) {
			switch (status) {
				case 0: return this.$t('sealing.statusPending');    // 待确认
				case 1: return this.$t('sealing.statusConfirmed');  // 已确认
				default: return this.$t('sealing.statusUnknown');
			}
		},
		formatTime(timeStr) {
			if (!timeStr) return '';
			const date = dayjs(timeStr);
			if (!date.isValid()) return timeStr;
			return date.format('HH:mm');
		},
		confirmSeal(item) {
			uni.showModal({
				title: this.$t('sealing.confirmTitle'),
				content: this.$t('sealing.confirmContent'),
				success: (res) => {
					if (res.confirm) {
						this.doConfirmSeal(item);
					} else {
						this.activeActionItemId = null;
					}
				}
			});
		},
		doConfirmSeal(item) {
			uni.showLoading({ title: this.$t('sealing.submitting') });

			// 1. Update Seal Status to 1 (Assuming Update API takes object with Id and fields to update)
			// Wait, if no Update API, we might need a specific endpoint. 
			// Assuming generic Update method exists or provided Add works for update if ID exists (common in some frameworks)
			// But user said "Add a method... update Status to 1".
			// Trying generic 'Update' endpoint first.
			const updateData = {
				Id: item.Id,
				Status: 1
			};
			request.post('api/ssbh/SealHistory/Update', { mainData: updateData })
				.then(res => {
					let isSuccess = false;
					if (res) {
						if (res === '修改成功' || res.status === true || res.success === true) {
							isSuccess = true;
						} else if (typeof res === 'object' && !res.message) {
							isSuccess = true;
						}
					}

					if (isSuccess) {
						if (this.todoListId) {
							this.completeTodoList(item);
						} else {
							uni.showToast({ title: this.$t('sealing.submitSuccess') });
							this.fetchSealData();
							this.activeActionItemId = null;
						}
					} else {
						const msg = res.message || res.msg || (typeof res === 'string' ? res : '');
						throw new Error(msg || 'Update failed');
					}
				})
				.catch(err => {
					console.error("Confirm seal failed", err);
					uni.showModal({
						title: this.$t('common.fail'),
						content: err.message || this.$t('sealing.submitError'),
						showCancel: false
					});
				})
				.finally(() => {
					if (!this.todoListId) uni.hideLoading(); // If todolist logic runs, it will handle hideLoading
				});
		},
		completeTodoList(item) {
			const todoData = {
				TodoListId: this.todoListId,
				Status: 200, // Completed
				Description: '确认申请'
			};

			request.post('api/ssbh/TodoListDetail/Add', { mainData: todoData })
				.then(res => {
					if (res.status) {
						uni.showToast({ title: this.$t('sealing.submitSuccess') });
						setTimeout(() => {
							uni.navigateBack(); // Go back to task list? Or stay? Usually go back if entered from task.
						}, 1500);
					} else {
						// Seal updated but task failed... acceptable state?
						uni.showToast({ title: 'Seal confirmed but task update failed', icon: 'none' });
						this.fetchSealData();
					}
				})
				.catch(err => {
					console.error("TodoTask complete failed", err);
					uni.showToast({ title: 'Task update error', icon: 'none' });
					this.fetchSealData();
				})
				.finally(() => {
					uni.hideLoading();
					this.activeActionItemId = null;
				});
		},
		editSeal(item) {
			// 编辑功能（可选实现）

			this.activeActionItemId = null;
		},
		deleteSeal(item) {
			// 根据需求：Status>0时允许删除，Status=0时不允许删除
			if (item.Status === 0) { // 已修改为Status===0允许删除
				uni.showModal({
					title: this.$t('sealing.deleteConfirm'),
					content: this.$t('sealing.deleteWarning'),
					success: (res) => {
						if (res.confirm) {
							this.confirmDelete(item);
						} else {
							this.activeActionItemId = null;
						}
					}
				});
			} else {
				uni.showToast({
					title: this.$t('sealing.deleteNotAllowed'),
					icon: 'none'
				});
				this.activeActionItemId = null;
			}
		},
		confirmDelete(item) {
			uni.showLoading({ title: this.$t('sealing.deleting') });

			request.post('api/ssbh/SealHistory/Del', [item.Id])
				.then(res => {
					if (res.status) {
						uni.showToast({
							title: this.$t('sealing.deleteSuccess'),
							icon: 'success'
						});
						this.fetchSealData();
					} else {
						throw new Error(res.message || '删除失败');
					}
				})
				.catch(err => {
					console.error('删除封存记录失败:', err);
					uni.showToast({
						title: this.$t('sealing.deleteError'),
						icon: 'none'
					});
				})
				.finally(() => {
					uni.hideLoading();
				});
		}
	}
}
</script>

<style lang="scss" scoped>
@import '@/sull.scss';

page {
	background-color: $sull-color-background;
	height: 100%;
	font-family: $font-family-main;
}

.page-container {
	display: flex;
	flex-direction: column;
	height: 100vh;
}

.custom-nav {
	/* Removed fixed position */
	/* position: fixed; */
	/* top: 0; */
	/* left: 0; */
	/* right: 0; */
	background: $sull-color-white;
	border-bottom: 2rpx solid $sull-color-line;
	z-index: 999;
}

.nav-content {
	display: flex;
	align-items: center;
	justify-content: space-between;
	height: 88rpx;
	padding: 0 32rpx;
}

// ... existing styles ...

.bottom-spacer {
	height: 40rpx;
}




.nav-left,
.nav-right {
	width: 120rpx;
	display: flex;
	align-items: center;
}

.nav-right {
	justify-content: flex-end;
}

.nav-title {
	font-size: 34rpx;
	font-weight: $font-weight-semibold;
	color: $sull-text-main;
}

.scroll-content {
	flex: 1;
	height: 0;
}

.content-wrapper {
	padding: 32rpx 48rpx;
}

/* 统计卡片 */
.stats-card {
	border-radius: $radius-lg;
	padding: 32rpx;
	margin-bottom: 32rpx;
	background: $sull-color-white;
	box-shadow: 0 4rpx 16rpx rgba(0, 0, 0, 0.02);
}

.stats-row {
	display: flex;
	justify-content: space-between;
	align-items: center;
}

.stat-item {
	display: flex;
	flex-direction: column;
	align-items: center;
	flex: 1;
}

.stat-num {
	font-size: $font-size-h4;
	font-weight: $font-weight-bold;
	margin-bottom: 8rpx;
}

.stat-label {
	font-size: $font-size-body-sm;
	color: $sull-text-secondary;
}

.stat-divider {
	width: 2rpx;
	height: 80rpx;
	background-color: $sull-color-line;
	margin: 0 32rpx;
}

/* 日历部分 */
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
			border: none;
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
				/* Remove absolute positioning to match flex layout in handover */
				position: static;
			}
		}
	}
}

/* 3. Controls Row */
.controls-row {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-bottom: 32rpx;

	.new-seal-btn {
		display: flex;
		align-items: center;
		justify-content: center;
		background: $sull-color-primary;
		color: $sull-color-white;
		font-size: $font-size-body-md;
		padding: 0 32rpx;
		height: 72rpx;
		border-radius: 36rpx;
		box-shadow: 0 8rpx 16rpx rgba(149, 202, 255, 0.4);
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

/* 列表部分 */
.list-section {
	margin-bottom: 40rpx;
}

.loading-state,
.empty-state {
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	padding: 120rpx 40rpx;
}

.loading-spinner {
	font-size: 48rpx;
	margin-bottom: 24rpx;
	animation: spin 1s linear infinite;
}

@keyframes spin {
	from {
		transform: rotate(0deg);
	}

	to {
		transform: rotate(360deg);
	}
}

.empty-icon {
	font-size: 96rpx;
	margin-bottom: 24rpx;
	opacity: 0.5;
}

.empty-text {
	font-size: $font-size-body-md;
	color: $sull-text-secondary;
}

.seal-list {
	display: flex;
	flex-direction: column;
	gap: 24rpx;
}

.seal-item {
	background: $sull-color-white;
	border-radius: $radius-lg;
	padding: 32rpx;
	border: 2rpx solid $sull-color-line;
	position: relative;
	transition: all 0.3s ease;
}

.seal-item.new-item {
	border-color: $sull-color-primary;
	box-shadow: 0 0 0 4rpx rgba(64, 158, 255, 0.1);
}

.item-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-bottom: 16rpx;
}

.item-time {
	font-size: $font-size-body-md;
	color: $sull-text-main;
	font-weight: $font-weight-medium;
}

.item-meta-right {
	display: flex;
	align-items: center;
	gap: 24rpx;
}

.status-tag {
	padding: 8rpx 16rpx;
	border-radius: $radius-sm;
	font-size: $font-size-body-xs;
	font-weight: $font-weight-medium;
}

.status-sealed {
	background: #E6F7FF;
	color: #1890FF;
}

.status-unsealed {
	background: #F6FFED;
	color: #52C41A;
}

.status-pending {
	background: #FFF7E6;
	color: #FA8C16;
}

.status-confirmed {
	background: #F6FFED;
	color: #52C41A;
}

.status-unknown {
	background: #FFF7E6;
	color: #FA8C16;
}

.action-menu {
	width: 64rpx;
	height: 64rpx;
	display: flex;
	align-items: center;
	justify-content: center;
	border-radius: $radius-md;
	cursor: pointer;
	transition: background-color 0.2s ease;
	position: relative;
}

.action-menu:hover {
	background: $sull-gray-10;
}

.menu-icon {
	font-size: 36rpx;
	color: $sull-text-secondary;
}

.item-content {
	margin-bottom: 16rpx;
}

.item-code {
	font-size: $font-size-body-md;
	color: $sull-text-main;
	font-weight: $font-weight-medium;
	margin-bottom: 8rpx;
	display: block;
}

.item-remark {
	font-size: $font-size-body-sm;
	color: $sull-text-secondary;
	display: block;
	line-height: 1.4;
}

.item-footer {
	display: flex;
	justify-content: space-between;
	align-items: center;
}

.item-creator,
.item-assign {
	font-size: $font-size-body-xs;
	color: $sull-text-secondary;
}

/* 操作下拉菜单 */
.action-dropdown {
	position: absolute;
	top: 100%;
	right: 0;
	background: $sull-color-white;
	border-radius: $radius-md;
	box-shadow: 0 8rpx 32rpx rgba(0, 0, 0, 0.15);
	border: 2rpx solid $sull-color-line;
	z-index: 100;
	min-width: 240rpx;
	overflow: hidden;
	margin-top: 8rpx;
	/* Space between button and menu */
}

.action-item {
	display: flex;
	align-items: center;
	padding: 24rpx 32rpx;
	cursor: pointer;
	transition: background-color 0.2s ease;
}

.action-item:hover {
	background: $sull-gray-10;
}

.delete-item:hover {
	background: #FFF2F0;
}

.action-icon {
	margin-right: 16rpx;
	font-size: 32rpx;
}

.action-text {
	font-size: $font-size-body-sm;
	color: $sull-text-main;
}

.delete-item .action-text {
	color: $sull-color-error;
}

/* Modal Styles - 完全参照交接班 */
.modal-overlay {
	position: fixed;
	top: 0;
	left: 0;
	right: 0;
	bottom: 0;
	background: rgba(0, 0, 0, 0.5);
	backdrop-filter: blur(4rpx);
	z-index: 2000;
	display: flex;
	align-items: center;
	justify-content: center;
}

.modal-content {
	width: 85%;
	background: $sull-color-white;
	border-radius: $radius-lg;
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
		overflow-y: auto;

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
				display: flex;
				flex-direction: column;
				gap: 8rpx;
			}

			.value.disabled {
				color: $sull-text-secondary;
				background: #f9f9f9;
				padding-left: 16rpx;
				border-radius: 8rpx;
				border-bottom: none;
				flex-direction: row;
				justify-content: space-between;
				align-items: center;
				padding-right: 12rpx;
			}

			.value.disabled .text-main {
				color: $sull-text-secondary;
				/* Match parent/disabled color */
				font-weight: 500;
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
						background: #EAF2FF;
						color: $sull-color-primary;
						border-color: $sull-color-primary;
						font-weight: $font-weight-medium;
					}
				}
			}

			.value-input {
				width: 100%;
				border: none;
				outline: none;
				font-size: $font-size-body-md;
				font-weight: 500;
				color: $sull-text-main;
				background: transparent;
				padding: 0;
				margin: 0;
			}

			.value-input::placeholder {
				color: $sull-text-placeholder;
				font-weight: 400;
			}

			.value-textarea {
				width: 100%;
				min-height: 60rpx;
				border: none;
				outline: none;
				font-size: $font-size-body-md;
				line-height: 40rpx;
				font-weight: 500;
				color: $sull-text-main;
				background: transparent;
				padding: 10rpx 0;
				margin: 0;
				resize: none;
			}

			.value-textarea::placeholder {
				color: $sull-text-placeholder;
				font-weight: 400;
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

@keyframes slideUp {
	0% {
		transform: translateY(40rpx);
		opacity: 0;
	}

	100% {
		transform: translateY(0);
		opacity: 1;
	}
}

/* Member Selector Styles - 完全参照交接班 */
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
	backdrop-filter: blur(4rpx);
}

.member-modal {
	width: 100%;
	background: $sull-color-white;
	border-radius: 40rpx 40rpx 0 0;
	padding: 40rpx;
	padding-bottom: 80rpx;
	display: flex;
	flex-direction: column;
	max-height: 60vh;
	animation: slideUp 0.3s ease;
}

.member-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-bottom: 32rpx;
	gap: 24rpx;
}

.member-search-input {
	flex: 1;
	height: 80rpx;
	background: #F5F7FA;
	border-radius: 40rpx;
	padding: 0 72rpx 0 32rpx;
	/* Right padding for clear icon */
	font-size: 28rpx;
}

.search-box {
	flex: 1;
	position: relative;
	display: flex;
	align-items: center;
}

.clear-icon {
	position: absolute;
	right: 24rpx;
	color: #999;
	font-size: 28rpx;
	padding: 8rpx;
	z-index: 10;
}

.member-list {
	flex: 1;
	height: 0;
	/* Important for flex scroll */
	min-height: 400rpx;
}

.member-item {
	padding: 32rpx;
	border-bottom: 2rpx solid #eee;
	font-size: 32rpx;
	color: $sull-text-main;

	&:active {
		background: #f9f9f9;
	}

	.member-name {
		font-size: 32rpx;
		color: $sull-text-main;
	}
}

.empty-state-small {
	padding: 40rpx;
	text-align: center;
	color: $sull-text-secondary;
}

.bottom-spacer {
	height: 40rpx;
}
</style>
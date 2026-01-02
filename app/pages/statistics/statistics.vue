<template>
	<view class="container">
		<!-- Main Content Scroll -->
		<scroll-view scroll-y class="main-content" @scrolltolower="loadMore" refresher-enabled="true"
			:refresher-triggered="isRefreshing" :refresher-threshold="60" @refresherrefresh="onRefresh"
			@refresherpulling="onPulling" @refresherrestore="onRestore" @refresherabort="onRestore"
			refresher-default-style="none" refresher-background="#f0f5ff">
			<view slot="refresher" class="custom-refresher"
				:style="{ transform: (isPulling || isRefreshing) ? 'translateY(120rpx)' : 'translateY(0)' }">
				<view class="refresher-content">
					<view class="refresher-icon" :class="{ 'animating': isRefreshing }"></view>
					<text class="refresher-text">{{ isRefreshing ? $t('index.refreshing') : $t('index.pullToRefresh')
					}}</text>
				</view>
			</view>

			<!-- Header (Inside Scroll) -->
			<view class="custom-header" :style="{ paddingTop: statusBarHeight * 2 + 'rpx' }">
				<view class="nav-left" @click="toggleDrawer">
					<text class="menu-icon">☰</text>
				</view>
				<text class="header-title">{{ activeCategory.title || $t('index.nav.statistics') }}</text>
				<view class="nav-right"></view>
			</view>
			<view class="content-wrapper">

				<!-- Row 2: Conditions / Filters -->
				<view class="row-filters card">
					<!-- Time Range Condition -->
					<view class="filter-row" v-if="activeCategory.filters && activeCategory.filters.time">
						<text class="filter-label-fixed">统计时间</text>
						<view class="filter-control-group">
							<picker mode="date" class="date-picker" :value="filter.startDate"
								@change="onStartDateChange">
								<view class="date-input">
									<text>{{ filter.startDate || '开始日期' }}</text>
								</view>
							</picker>
							<text class="date-divider">至</text>
							<picker mode="date" class="date-picker" :value="filter.endDate" @change="onEndDateChange">
								<view class="date-input">
									<text>{{ filter.endDate || '结束日期' }}</text>
								</view>
							</picker>
						</view>
					</view>

					<!-- Item Name Condition -->
					<view class="filter-row" v-if="activeCategory.filters && activeCategory.filters.itemName">
						<text class="filter-label-fixed">物品名称</text>
						<view class="filter-control-box" style="background: #F5F7FA; border: 2rpx solid #E9E9ED;">
							<input class="search-input" v-model="filter.itemName" placeholder="请输入物品名称"
								style="padding-left: 0; background: transparent; height: 100%;" />
						</view>
					</view>

					<!-- Operators Condition (Multi-select) -->
					<view class="filter-row" v-if="activeCategory.filters && activeCategory.filters.operator">
						<text class="filter-label-fixed">操作人员</text>
						<view class="filter-control-box" @click="openOperatorSelector">
							<text class="control-value" :class="{ placeholder: !filter.operators.length }">
								{{ filter.operators.length > 0 ? `已选 ${filter.operators.join(', ')}` : '请选择操作人员' }}
							</text>
							<text class="control-icon">▼</text>
						</view>
					</view>

					<!-- Status Condition (Checkboxes) -->
					<view class="filter-row" v-if="activeCategory.filters && activeCategory.filters.status">
						<text class="filter-label-fixed">状态</text>
						<view class="checkbox-group">
							<view class="checkbox-item" v-for="(item, index) in statusOptions" :key="index"
								@click="toggleStatus(item)">
								<view class="checkbox-box" :class="{ checked: filter.status.includes(item.value) }">
									<text v-if="filter.status.includes(item.value)">✓</text>
								</view>
								<text class="checkbox-label">{{ item.label }}</text>
							</view>
						</view>
					</view>
				</view>

				<!-- Row 3: Actions -->
				<view class="row-actions">
					<view class="action-btn btn-query" @click="handleQuery">
						<text class="btn-text">查询</text>
					</view>
					<view v-if="false" class="action-btn btn-share" @click="handleShare">
						<text class="btn-text">分享</text>
					</view>
					<view class="action-btn btn-export" @click="handleExport">
						<text class="btn-text">导出</text>
					</view>
					<view class="action-btn btn-settings" @click="openColumnSettings"
						style="margin-left: auto; background: transparent; border: none; padding: 0;">
						<text class="btn-text" style="font-size: 40rpx;">⚙️</text>
					</view>
				</view>

				<!-- Row 4: Results Table -->
				<view class="row-results card">
					<!-- Loading -->
					<view v-if="loading" class="loading-state">
						<text>Loading...</text>
					</view>

					<!-- Empty State (No Data) -->
					<view v-else-if="results.length === 0" class="empty-state">
						<image class="empty-image" src="/static/imgs/empty.png" mode="aspectFit"></image>
						<text class="empty-title">暂无统计数据</text>
						<text class="empty-subtitle">请调整查询条件或先录入相关数据</text>
					</view>

					<!-- Scroll Wrapper for Table (Only if results exist) -->
					<scroll-view v-else scroll-x class="table-scroll-x" :enable-flex="true">
						<view class="table-content" :style="{ width: tableWidth }">
							<!-- Header -->
							<view class="table-header">
								<view v-for="(col, index) in activeCategory.columns" :key="index" class="th"
									:class="{ 'sticky-col': col.frozen }"
									:style="{ width: col.width + 'rpx', flex: 'none', left: col.left || 0, zIndex: col.frozen ? 10 : 1 }">
									{{ col.label }}
								</view>
							</view>

							<!-- Body -->
							<view class="table-body-container">
								<view class="table-body">
									<view v-for="(row, rIndex) in results" :key="rIndex" class="tr">
										<view v-for="(col, cIndex) in activeCategory.columns" :key="cIndex" class="td"
											:class="{ 'text-red': col.key === 'count' && row[col.key] < 0, 'sticky-col': col.frozen }"
											:style="{ width: col.width + 'rpx', flex: 'none', left: col.left || 0, zIndex: col.frozen ? 5 : 0 }">
											{{ row[col.key] }}
										</view>
									</view>
									<view class="list-footer">
										<view v-if="loadingMore" class="loading-more">
											<view class="loading-icon-spin"></view>
											<text>加载中...</text>
										</view>
										<view v-else-if="pagination.noMore" class="no-more">
											<view class="line"></view>
											<text class="no-more-text">没有更多数据了</text>
											<view class="line"></view>
										</view>
									</view>
								</view>
							</view>
						</view>
					</scroll-view>
				</view>
			</view>

			<view class="spacer"></view>
			<FooterInfo />
		</scroll-view>

		<!-- Left Drawer -->
		<view class="drawer-mask" :class="{ show: showDrawer }" @click="toggleDrawer"></view>
		<view class="drawer-panel" :class="{ show: showDrawer }">
			<view class="drawer-header">
				<text class="drawer-title">统计菜单</text>
				<text class="close-btn" @click="toggleDrawer">✖</text>
			</view>
			<scroll-view scroll-y class="drawer-list">
				<view v-for="(cat, index) in categories" :key="index" class="drawer-item"
					:class="{ active: activeCategory.id === cat.id }" @click="selectCategory(cat)">
					<text class="item-text">{{ cat.title }}</text>
					<text v-if="activeCategory.id === cat.id" class="active-indicator">✓</text>
				</view>
			</scroll-view>
		</view>

		<!-- Operator Selection Modal (Bottom Sheet Style) -->
		<view class="operator-overlay" v-if="showOperatorModal" @click="closeOperatorModal">
			<view class="operator-modal" @click.stop>
				<view class="operator-header">
					<view class="search-box">
						<input class="search-input" v-model="searchOperatorKey" placeholder="搜索人员" />
						<text v-if="searchOperatorKey" class="clear-icon" @click="clearSearch">✖</text>
					</view>
					<text class="close-btn" @click="closeOperatorModal">✖️</text>
				</view>
				<scroll-view scroll-y class="operator-list">
					<view v-for="(op, index) in filteredOperatorList" :key="op.id" class="operator-item"
						:class="{ active: filter.operators.includes(op.name) }" @click="toggleOperator(op.name)">
						<text class="item-text">{{ op.name }}</text>
						<text v-if="filter.operators.includes(op.name)" class="check-icon">✓</text>
					</view>
					<view v-if="filteredOperatorList.length === 0" class="empty-state-small">
						<text>数据为空</text>
					</view>
				</scroll-view>
				<view class="modal-footer-simple">
					<button class="confirm-btn-full" @click="closeOperatorModal">确定</button>
				</view>
			</view>
		</view>

		<!-- Column Settings Modal -->
		<view class="operator-overlay" v-if="showColumnSettingsModal" @click="closeColumnSettings">
			<view class="operator-modal" @click.stop>
				<view class="operator-header">
					<text class="modal-title">列设置 (冻结)</text>
					<text class="close-btn" @click="closeColumnSettings">✖️</text>
				</view>
				<scroll-view scroll-y class="operator-list">
					<view v-for="(col, index) in activeCategory.columns" :key="index" class="operator-item"
						@click="toggleColumnFreeze(index)">
						<text class="item-text">{{ col.label }}</text>
						<view class="checkbox-box" :class="{ checked: col.frozen }">
							<text v-if="col.frozen">✓</text>
						</view>
					</view>
				</scroll-view>
				<view class="modal-footer-simple">
					<button class="confirm-btn-full" @click="closeColumnSettings">确定</button>
				</view>
			</view>
		</view>

		<BottomNav activePage="statistics" />
	</view>
</template>

<script>
import BottomNav from '@/components/BottomNav/BottomNav.vue';
import FooterInfo from '@/components/FooterInfo/FooterInfo.vue';
import dayjs from 'dayjs';
import request from '@/utils/request.js';
import storage from '@/utils/storage.js';
import * as XLSX from 'xlsx';

export default {
	components: {
		BottomNav,
		FooterInfo
	},
	data() {
		return {
			statusBarHeight: 20,
			showDrawer: false,
			loading: false,
			isRefreshing: false,
			isPulling: false,
			filter: {
				startDate: '',
				endDate: '',
				operators: [],
				status: ['normal'], // Default selected
				itemName: ''
			},
			showColumnSettingsModal: false,
			operatorList: [],
			showOperatorModal: false,
			searchOperatorKey: '',
			statusOptions: [
				{ label: '正常', value: 'normal' },
				{ label: '异常', value: 'abnormal' },
				{ label: '处理中', value: 'processing' }
			],
			// Column widths now in px for horizontal scroll
			categories: [
				{
					id: 'consumption',
					title: '物品使用统计',
					filters: { time: true, operator: true, status: false, itemName: true },
					columns: [
						{ label: '物品名称', key: 'name', width: 280 },
						{ label: '规格', key: 'spec', width: 160 },
						{ label: '批号', key: 'batch', width: 200 },
						{ label: '有效期', key: 'expiry', width: 200 },
						{ label: '使用量', key: 'count', width: 140 },
						{ label: '库存', key: 'stock', width: 140 },
						{ label: '操作时间', key: 'time', width: 280 }
					]
				},
				{
					id: 'handover',
					title: '交接班统计',
					filters: { time: true, operator: true, status: false },
					columns: [
						{ label: '日期', key: 'date', width: 240 },
						{ label: '交接时间', key: 'time', width: 320 },
						{ label: '班次', key: 'shift', width: 160 },
						{ label: '交班人', key: 'giver', width: 200 },
						{ label: '接班人', key: 'receiver', width: 200 },
						{ label: '状态', key: 'status', width: 160 }
					]
				},
				{
					id: 'stock',
					title: '库存统计',
					filters: { time: false, operator: false, status: false, itemName: true },
					columns: [
						{ label: '物品名称', key: 'name', width: 280 },
						{ label: '当前库存', key: 'current', width: 160 },
						{ label: '基数', key: 'safe', width: 160 },
						{ label: '批号数', key: 'batchCount', width: 160 },
						{ label: '最近过期', key: 'nearExpiry', width: 200 },
						{ label: '上次使用', key: 'lastUse', width: 280 }
					]
				},
				{
					id: 'sealing',
					title: '封存记录',
					filters: { time: true, operator: false, status: false, itemName: false },
					columns: [
						{ label: '操作日期', key: 'time', width: 200 },
						{ label: '锁扣编号', key: 'code', width: 220 },
						{ label: '封存类型', key: 'type', width: 160 },
						{ label: '状态', key: 'status', width: 160 },
						{ label: '操作人', key: 'operator', width: 180 },
						{ label: '确认人', key: 'confirmer', width: 180 },
						{ label: '备注', key: 'remark', width: 300 }
					]
				},
				{
					id: 'rescue',
					title: '抢救记录',
					filters: { time: true, operator: true, status: false, itemName: false },
					columns: [
						{ label: '抢救日期', key: 'date', width: 220 },
						{ label: '患者姓名', key: 'patient', width: 180 },
						{ label: '性别', key: 'gender', width: 100 },
						{ label: '年龄', key: 'age', width: 100 },
						{ label: '开始时间', key: 'startTime', width: 200 },
						{ label: '结束时间', key: 'endTime', width: 200 },
						{ label: '抢救效果', key: 'effect', width: 200 },
						{ label: '状态', key: 'status', width: 160 },
						{ label: '操作人', key: 'operator', width: 180 }
					]
				}
			],
			activeCategory: {},
			results: [],
			userOrgInfo: {},
			pagination: {
				page: 1,
				rows: 50,
				total: 0,
				noMore: false
			},
			loadingMore: false
		};
	},
	computed: {
		filteredOperatorList() {
			if (!this.searchOperatorKey) return this.operatorList;
			const key = this.searchOperatorKey.toLowerCase();
			return this.operatorList.filter(op => op.name.toLowerCase().includes(key));
		},
		tableWidth() {
			if (!this.activeCategory.columns) return '100%';
			const total = this.activeCategory.columns.reduce((sum, col) => sum + (col.width || 0), 0);
			// ensure at least 100% width
			return total > 0 ? total + 'rpx' : '100%';
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
	onLoad(options) {
		const sysInfo = uni.getSystemInfoSync();
		this.statusBarHeight = sysInfo.statusBarHeight || 20;

		// Init dates
		const now = dayjs();
		this.filter.startDate = now.format('YYYY-MM-DD');
		this.filter.endDate = now.add(1, 'day').format('YYYY-MM-DD');

		// Load User Info using standard storage utility
		this.userOrgInfo = storage.get('userOrgInfo') || {};

		// Restore last selected category or use from options
		const lastCatId = (options && options.category) || storage.get('STATISTICS_LAST_CATEGORY');
		if (lastCatId) {
			const found = this.categories.find(c => c.id === lastCatId);
			this.activeCategory = found || this.categories[0];
		} else {
			this.activeCategory = this.categories[0];
		}
		this.loadColumnSettings();

		if (options && options.itemName) {
			this.filter.itemName = decodeURIComponent(options.itemName);
			// Auto trigger query if coming from a specific item
			this.$nextTick(() => {
				this.handleQuery();
			});
		}
	},
	methods: {
		toggleDrawer() {
			this.showDrawer = !this.showDrawer;
		},
		selectCategory(cat) {
			this.activeCategory = cat;
			storage.set('STATISTICS_LAST_CATEGORY', cat.id);
			this.showDrawer = false;
			this.results = []; // Clear old results
			this.loadColumnSettings();
		},
		loadColumnSettings() {
			if (!this.activeCategory || !this.activeCategory.id) return;
			const saved = storage.get(`STATISTICS_COL_SETTINGS_${this.activeCategory.id}`);
			if (saved && Array.isArray(saved)) {
				// Restore frozen state
				this.activeCategory.columns.forEach(col => {
					col.frozen = saved.includes(col.key);
				});
			}
			this.calculateColumnStyles();
		},
		saveColumnSettings() {
			if (!this.activeCategory || !this.activeCategory.id) return;
			const frozenKeys = this.activeCategory.columns.filter(c => c.frozen).map(c => c.key);
			storage.set(`STATISTICS_COL_SETTINGS_${this.activeCategory.id}`, frozenKeys);
		},
		calculateColumnStyles() {
			let currentLeft = 0;
			this.activeCategory.columns.forEach(col => {
				if (col.frozen) {
					col.left = currentLeft + 'rpx';
					currentLeft += (col.width || 0);
				} else {
					col.left = 'auto';
				}
			});
			// Force update array to trigger reactivity if needed, though property mutation might work in Vue 2
			this.activeCategory.columns = [...this.activeCategory.columns];
		},
		openColumnSettings() {
			this.showColumnSettingsModal = true;
		},
		closeColumnSettings() {
			this.showColumnSettingsModal = false;
			this.saveColumnSettings();
			this.calculateColumnStyles();
		},
		toggleColumnFreeze(index) {
			const col = this.activeCategory.columns[index];
			col.frozen = !col.frozen;
			// allow immediate visual feedback in modal, but recalc styles on close or here?
			// Recalc here to support live preview if we wanted, but close is fine.
			this.$forceUpdate();
		},
		onStartDateChange(e) {
			this.filter.startDate = e.detail.value;
		},
		onEndDateChange(e) {
			this.filter.endDate = e.detail.value;
		},
		openOperatorSelector() {
			this.showOperatorModal = true;
			if (this.operatorList.length === 0) {
				this.fetchOperators();
			}
		},
		closeOperatorModal() {
			this.showOperatorModal = false;
		},
		toggleOperator(name) {
			const index = this.filter.operators.indexOf(name);
			if (index > -1) {
				this.filter.operators.splice(index, 1);
			} else {
				this.filter.operators.push(name);
			}
		},
		clearSearch() {
			this.searchOperatorKey = '';
		},
		fetchOperators() {
			request.get('api/ssbh/OrganizationUser/DepartmentUser')
				.then(res => {
					this.operatorList = (res.rows || []).map(u => ({
						name: u.UserTrueName || u.userTrueName || u.UserName || u.userName,
						id: u.UserId || u.userId
					}));
				})
				.catch(err => {
					console.error("Fetch Operators Failed", err);
				});
		},
		toggleStatus(item) {
			const index = this.filter.status.indexOf(item.value);
			if (index > -1) {
				this.filter.status.splice(index, 1);
			} else {
				this.filter.status.push(item.value);
			}
		},
		handleQuery() {
			this.pagination.page = 1;
			this.pagination.noMore = false;
			this.results = []; // Clear for new query
			this.fetchData();
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
			this.pagination.page = 1;
			this.pagination.noMore = false;
			// this.results = []; // Keep results while refreshing to avoid flicker? Or clear? 
			// Usually clear if page=1.
			this.fetchData().finally(() => {
				setTimeout(() => {
					this.isRefreshing = false;
					this.isPulling = false;
					uni.showToast({ title: '已刷新', icon: 'none' });
				}, 800);
			});
		},
		loadMore() {
			if (this.loading || this.loadingMore || this.pagination.noMore) return;
			this.pagination.page++;
			this.fetchData(true);
		},
		fetchData(isLoadMore = false) {
			if (isLoadMore) {
				this.loadingMore = true;
			} else {
				this.loading = true;
			}

			const type = this.activeCategory.id;

			// Common Org/Dept filters
			const orgDeptWheres = [];
			if (this.userOrgInfo.Organization?.Id) {
				orgDeptWheres.push({ name: 'OrganizationId', value: this.userOrgInfo.Organization.Id, displayType: '=' });
			}
			if (this.userOrgInfo.Department?.Id) {
				orgDeptWheres.push({ name: 'DepartmentId', value: this.userOrgInfo.Department.Id, displayType: '=' });
			}

			let reqPromise = null;

			if (type === 'consumption') {
				const wheres = [...orgDeptWheres];
				// if (this.filter.startDate) {
				// 	wheres.push({ name: 'CreationTime', value: this.filter.startDate + ' 00:00:00', displayType: 'thanorequal' });
				// }
				// if (this.filter.endDate) {
				// 	wheres.push({ name: 'CreationTime', value: this.filter.endDate + ' 00:00:00', displayType: 'lt' });
				// }
				if (this.filter.itemName) {
					wheres.push({ name: 'Name', value: this.filter.itemName, displayType: 'like' });
				}
				if (this.filter.startDate) {
					wheres.push({ name: 'CreationTime', value: this.filter.startDate + ' 00:00:00', displayType: 'thanorequal' });
				}
				if (this.filter.endDate) {
					wheres.push({ name: 'CreationTime', value: this.filter.endDate + ' 23:59:59', displayType: 'lt' });
				}

				reqPromise = request.post('api/ssbh/ItemHistory/Statistics', {
					page: this.pagination.page,
					rows: this.pagination.rows,
					wheres: JSON.stringify(wheres)
				}).then(res => {
					const list = (res.data.rows || []).map(r => ({
						name: r.itemName,
						spec: r.specification || '-',
						batch: r.registerCode || '-',
						expiry: (r.expirationDate || '').substring(0, 10),
						count: r.changeCounter || 0,
						stock: r.stock || 0,
						time: (r.creationTime || '').substring(0, 16).replace('T', ' ')
					}));
					return { list, total: res.data.total };
				});

			} else if (type === 'handover') {
				const handoverWheres = [...orgDeptWheres];
				if (this.filter.startDate) {
					handoverWheres.push({ name: 'ShiftTime', value: this.filter.startDate + ' 00:00:00', displayType: 'thanorequal' });
				}
				if (this.filter.endDate) {
					handoverWheres.push({ name: 'ShiftTime', value: this.filter.endDate + ' 23:59:59', displayType: 'lt' });
				}

				reqPromise = request.post('api/ssbh/Handover/GetPageData', {
					page: this.pagination.page,
					rows: this.pagination.rows,
					wheres: JSON.stringify(handoverWheres),
					sort: 'ShiftTime', order: 'desc'
				}).then(res => {
					const list = (res.rows || []).map(r => ({
						date: (r.ShiftTime || '').substring(0, 10),
						time: (r.ShiftTime || '').replace('T', ' '),
						shift: r.ShiftName,
						giver: r.Creator,
						receiver: r.ReceiverName,
						status: r.AuditStatus === 20 ? '已审核' : (r.AuditStatus === 10 ? '已完成' : '进行中')
					}));
					return { list, total: res.total };
				});

			} else if (type === 'stock') {
				const stockWheres = [...orgDeptWheres];
				if (this.filter.itemName) {
					stockWheres.push({ name: 'Name', value: this.filter.itemName, displayType: 'like' });
				}

				reqPromise = request.post('api/ssbh/Item/GetPageData', {
					page: this.pagination.page,
					rows: this.pagination.rows,
					wheres: JSON.stringify(stockWheres)
				}).then(res => {
					const list = (res.rows || []).map(r => ({
						name: r.Name,
						current: r.AvailableStock,
						safe: r.Cardinality,
						batchCount: r.BatchCounter || 0,
						nearExpiry: (r.ExpirationDate || '').substring(0, 10),
						lastUse: (r.LastUseTime || '').substring(0, 16).replace('T', ' ')
					}));
					return { list, total: res.total };
				});
			} else if (type === 'sealing') {
				const sealWheres = [...orgDeptWheres];
				if (this.filter.startDate) {
					sealWheres.push({ name: 'CreationTime', value: this.filter.startDate + ' 00:00:00', displayType: 'thanorequal' });
				}
				if (this.filter.endDate) {
					sealWheres.push({ name: 'CreationTime', value: this.filter.endDate + ' 23:59:59', displayType: 'lt' });
				}

				reqPromise = request.post('api/ssbh/SealHistory/GetPageData', {
					page: this.pagination.page,
					rows: this.pagination.rows,
					wheres: JSON.stringify(sealWheres),
					sort: 'CreationTime', order: 'desc'
				}).then(res => {
					const list = (res.rows || []).map(r => ({
						code: r.Code || '-',
						type: r.Category === 1 ? '启封' : '封存',
						status: r.Status === 1 ? '已确认' : '待确认',
						operator: r.Creator || '-',
						confirmer: r.AssignUser || '-',
						time: (r.CreationTime || '').substring(0, 10),
						remark: r.Remark || '-'
					}));
					return { list, total: res.total };
				});
			} else if (type === 'rescue') {
				const rescueWheres = [...orgDeptWheres];
				if (this.filter.startDate) {
					rescueWheres.push({ name: 'StartTime', value: this.filter.startDate + ' 00:00:00', displayType: 'thanorequal' });
				}
				if (this.filter.endDate) {
					rescueWheres.push({ name: 'StartTime', value: this.filter.endDate + ' 23:59:59', displayType: 'lt' });
				}

				reqPromise = request.post('api/ssbh/Rescue/GetPageData', {
					page: this.pagination.page,
					rows: this.pagination.rows,
					wheres: JSON.stringify(rescueWheres),
					sort: 'StartTime', order: 'desc'
				}).then(res => {
					const getGender = (g) => {
						if (g === 1) return '男';
						if (g === 2) return '女';
						return '未知';
					};
					const getStatus = (s) => {
						const map = {
							0: '抢救中',
							10: '已取消',
							20: '已结束',
							100: '已审核'
						};
						return map[s] || '未知';
					};

					const list = (res.rows || []).map(r => ({
						date: (r.StartTime || '').substring(0, 10),
						patient: r.Name || '-',
						gender: getGender(r.Gender),
						age: r.Age ? r.Age + '岁' : '-',
						startTime: (r.StartTime || '').substring(11, 16) || '-',
						endTime: (r.EndTime || '').substring(11, 16) || '-',
						effect: r.Effect || '-',
						status: getStatus(r.Status),
						operator: r.CreatorUser || '-'
					}));
					return { list, total: res.total };
				});
			}

			if (reqPromise) {
				return reqPromise.then(data => {
					if (isLoadMore) {
						this.results = [...this.results, ...data.list];
					} else {
						this.results = data.list;
					}
					this.pagination.total = data.total;
					// Check if more
					if (this.results.length >= data.total || data.list.length < this.pagination.rows) {
						this.pagination.noMore = true;
					}
				}).catch(err => {
					console.error(err);
					uni.showToast({ title: '查询失败', icon: 'none' });
				}).finally(() => {
					this.loading = false;
					this.loadingMore = false;
				});
			} else {
				this.loading = false;
				this.loadingMore = false;
			}
		},
		handleShare() {
			uni.showToast({ title: '分享功能开发中', icon: 'none' });
		},
		async handleExport() {
			if (this.results.length === 0) {
				uni.showToast({ title: '无数据可导出', icon: 'none' });
				return;
			}

			// #ifdef APP-PLUS
			if (uni.getSystemInfoSync().platform === 'android') {
				const permission = 'android.permission.WRITE_EXTERNAL_STORAGE';
				const hasPermission = await new Promise((resolve) => {
					plus.android.requestPermissions([permission], (res) => {
						if (res.granted.length > 0) resolve(true);
						else resolve(false);
					}, (err) => resolve(false));
				});

				if (!hasPermission) {
					uni.showModal({
						title: '权限申请',
						content: '需要存储空间写入权限才能导出Excel文件，请在设置中开启',
						confirmText: '去设置',
						success: (res) => {
							if (res.confirm) {
								plus.runtime.openAppSettings();
							}
						}
					});
					return;
				}
			}
			// #endif

			uni.showLoading({ title: '正在导出...', mask: true });

			try {
				// 1. Prepare Data
				const headers = this.activeCategory.columns.map(c => c.label);
				const keys = this.activeCategory.columns.map(c => c.key);

				const data = this.results.map(row => {
					return keys.map(k => row[k]);
				});

				const wb = XLSX.utils.book_new();
				const ws = XLSX.utils.aoa_to_sheet([headers, ...data]);

				// Optional: Set column width
				const colWidths = this.activeCategory.columns.map(c => ({ wch: (c.width || 100) / 7 }));
				ws['!cols'] = colWidths;

				XLSX.utils.book_append_sheet(wb, ws, "Sheet1");

				const fileName = `${this.activeCategory.title}_${this.filter.startDate || 'export'}.xlsx`;

				// #ifdef H5
				XLSX.writeFile(wb, fileName);
				uni.hideLoading();
				// #endif

				// #ifdef MP-WEIXIN
				const wbout_wx = XLSX.write(wb, {
					bookType: 'xlsx',
					type: 'array'
				});
				const fs = wx.getFileSystemManager();
				const filePath_wx = `${wx.env.USER_DATA_PATH}/${fileName}`;
				fs.writeFile({
					filePath: filePath_wx,
					data: wbout_wx,
					encoding: 'binary',
					success: () => {
						uni.hideLoading();
						uni.openDocument({
							filePath: filePath_wx,
							showMenu: true,
							fail: () => uni.showToast({
								title: '打开文件失败',
								icon: 'none'
							})
						});
					},
					fail: (err) => {
						uni.hideLoading();
						uni.showToast({
							title: '写入失败',
							icon: 'none'
						});
					}
				});
				// #endif

				// #ifdef APP-PLUS
				const wbout_app = XLSX.write(wb, {
					bookType: 'xlsx',
					type: 'base64'
				});

				if (uni.getSystemInfoSync().platform === 'android') {
					try {
						const File = plus.android.importClass("java.io.File");
						const FileOutputStream = plus.android.importClass("java.io.FileOutputStream");
						const Base64 = plus.android.importClass("android.util.Base64");
						const Environment = plus.android.importClass("android.os.Environment");

						const publicDir = Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DOWNLOADS);
						let downloadDir = "/storage/emulated/0/Download";
						if (publicDir) {
							downloadDir = publicDir.getAbsolutePath();
						}

						const filePath = downloadDir + "/" + fileName;

						const file = new File(filePath);
						if (file.exists()) {
							file.delete();
						}
						const stream = new FileOutputStream(file);
						const bytes = Base64.decode(wbout_app, 0);
						stream.write(bytes);
						stream.flush();
						stream.close();

						// Verification check 
						if (!file.exists() || file.length() === 0) {
							throw new Error("文件物理创建失败，请检查存储空间或权限");
						}

						// Notify system to scan the file so it appears in file managers (crucial for modern Android)
						const context = plus.android.runtimeMainActivity().getContext();
						const MediaScannerConnection = plus.android.importClass("android.media.MediaScannerConnection");
						MediaScannerConnection.scanFile(context, [filePath], null, null);

						uni.hideLoading();
						uni.showModal({
							title: '导出成功',
							content: `文件已保存至：\n${filePath}\n\n是否立即打开查看？`,
							confirmText: '打开',
							cancelText: '取消',
							success: (modalRes) => {
								if (modalRes.confirm) {
									plus.runtime.openFile(filePath, {}, (err) => {
										uni.showToast({ title: '无法打开此类型文件', icon: 'none' });
									});
								}
							}
						});
					} catch (e) {
						uni.hideLoading();
						console.error('Export error:', e);
						uni.showModal({
							title: '保存失败',
							content: '无法访问存储目录，请确保已授予存储权限',
							showCancel: false
						});
					}
				} else {
					// iOS logic
					const path_ios = '_doc/' + fileName;
					plus.io.requestFileSystem(plus.io.PRIVATE_DOC, (fs) => {
						fs.root.getFile(fileName, { create: true }, (entry) => {
							entry.createWriter((writer) => {
								writer.onwrite = () => {
									uni.hideLoading();
									uni.showModal({
										title: '导出完成',
										content: '文件已保存到应用文档目录',
										showCancel: false
									});
								};
								writer.onerror = () => {
									uni.hideLoading();
									uni.showToast({ title: '保存失败', icon: 'none' });
								};
								writer.write(wbout_app);
							});
						});
					});
				}
				// #endif

				// #ifndef H5 || MP-WEIXIN || APP-PLUS
				uni.hideLoading();
				uni.showToast({
					title: '平台暂不支持',
					icon: 'none'
				});
				// #endif

			} catch (e) {
				uni.hideLoading();
				console.error(e);
				uni.showToast({ title: '导出发生错误', icon: 'none' });
			}
		},
	}
};
</script>

<style lang="scss" scoped>
.container {
	display: flex;
	flex-direction: column;
	height: 100vh;
	width: 100%;
	background-color: #ffffff;
}

.custom-header {
	height: 88rpx;
	background: linear-gradient(135deg, $sull-color-primary 0%, lighten($sull-color-primary, 10%) 100%);
	display: flex;
	align-items: center;
	justify-content: space-between;
	padding-left: 32rpx;
	padding-right: 32rpx;
	box-shadow: 0 2rpx 8rpx rgba(0, 0, 0, 0.05);

	.header-title {
		font-size: 34rpx;
		font-weight: bold;
		color: #fff;
	}

	.menu-icon {
		color: #fff;
		font-size: 36rpx;
	}

	.nav-left,
	.nav-right {
		width: 80rpx;
		display: flex;
		align-items: center;
	}

	.menu-icon {
		font-size: 48rpx;
		color: $sull-text-main;
	}
}

.main-content {
	flex: 1;
	height: 0;
	width: 100%;
}

.content-wrapper {
	display: flex;
	flex-direction: column;
	gap: 32rpx;
	padding: 32rpx;
}



/* Row 2: Filters */
.row-filters {
	display: flex;
	flex-direction: column;
	padding: 32rpx !important;
	gap: 32rpx;

	.filter-row {
		display: flex;
		/* Horizontal row */
		align-items: center;
		gap: 24rpx;
	}

	.filter-label-fixed {
		font-size: 28rpx;
		color: $sull-text-main;
		font-weight: bold;
		width: 140rpx;
		/* Fixed width as requested */
		flex-shrink: 0;
	}

	/* Time Range */
	.filter-control-group {
		flex: 1;
		display: flex;
		align-items: center;
		gap: 16rpx;

		.date-picker {
			flex: 1;
		}

		.date-input {
			height: 72rpx;
			background: #F5F7FA;
			border-radius: 12rpx;
			display: flex;
			align-items: center;
			justify-content: center;
			font-size: 26rpx;
			color: $sull-text-main;
			border: 2rpx solid $sull-color-line;
		}

		.date-divider {
			color: $sull-text-secondary;
			font-size: 26rpx;
		}
	}

	/* Operator Selector */
	.filter-control-box {
		flex: 1;
		min-height: 72rpx;
		background: #F5F7FA;
		border-radius: 12rpx;
		display: flex;
		align-items: center;
		justify-content: space-between;
		padding: 0 20rpx;
		border: 2rpx solid $sull-color-line;

		.control-value {
			font-size: 28rpx;
			color: $sull-text-main;
		}

		.control-value.placeholder {
			color: $sull-text-placeholder;
		}

		.control-icon {
			font-size: 20rpx;
			color: $sull-text-secondary;
		}
	}

	/* Status Checkboxes */
	.checkbox-group {
		flex: 1;
		display: flex;
		flex-wrap: wrap;
		gap: 24rpx;
	}

	.checkbox-item {
		display: flex;
		align-items: center;
		gap: 12rpx;
		cursor: pointer;
	}

	.checkbox-box {
		width: 36rpx;
		height: 36rpx;
		border: 2rpx solid #ccc;
		border-radius: 8rpx;
		display: flex;
		align-items: center;
		justify-content: center;
		font-size: 24rpx;
		color: #fff;
		background: #fff;

		&.checked {
			background: $sull-color-primary;
			border-color: $sull-color-primary;
		}
	}

	.checkbox-label {
		font-size: 28rpx;
		color: $sull-text-main;
	}
}

.list-footer {
	padding: 20rpx;
	text-align: center;
	font-size: 24rpx;
	color: $sull-text-secondary;
}


/* Row 3: Actions */
.row-actions {
	display: flex;
	gap: 20rpx;
	justify-content: flex-start;

	.action-btn {
		height: 64rpx;
		border-radius: 32rpx;
		display: flex;
		align-items: center;
		justify-content: center;
		padding: 0 32rpx;
		font-size: 26rpx;
		font-weight: 500;
		transition: opacity 0.2s;

		&:active {
			opacity: 0.8;
		}
	}

	.btn-query {
		background: $sull-color-primary;
		color: #fff;
	}

	.btn-share {
		background: #E8F3FF;
		color: $sull-color-primary;
	}

	.btn-export {
		background: #fff;
		border: 2rpx solid $sull-color-line;
		color: $sull-text-main;
	}
}

/* Row 4: Results */
.row-results {
	min-height: 400rpx;
	padding: 0 !important;
	overflow: hidden;
}

.table-scroll-x {
	width: 100%;
}

.table-content {
	display: flex;
	flex-direction: column;
	min-width: 100%;
	border-left: 2rpx solid $sull-color-line;
	border-right: 2rpx solid $sull-color-line;
}

.table-header {
	background: #F5F7FA;
	display: flex;
	border-bottom: 2rpx solid $sull-color-line;
	border-top: 2rpx solid $sull-color-line;

	.th {
		font-size: 26rpx;
		color: $sull-text-secondary;
		font-weight: bold;
		text-align: center;
		padding: 20rpx 16rpx;
		box-sizing: border-box;
		border-right: 2rpx solid $sull-color-line;
		display: flex;
		align-items: center;
		justify-content: center;

		&:last-child {
			border-right: none;
		}
	}
}

.table-body-container {
	width: 100%;
}

.table-body {
	display: flex;
	flex-direction: column;

	.tr {
		display: flex;
		border-bottom: 2rpx solid $sull-color-line;
		align-items: stretch;

		/* Zebra Striping */
		&:nth-child(even) {
			background-color: #F8F9FA;
		}

		.td {
			font-size: 28rpx;
			color: $sull-text-main;
			text-align: center;
			white-space: nowrap;
			overflow: hidden;
			text-overflow: ellipsis;
			padding: 24rpx 16rpx;
			box-sizing: border-box;
			border-right: 2rpx solid $sull-color-line;
			display: flex;
			align-items: center;
			justify-content: center;

			&:last-child {
				border-right: none;
			}

			&.text-red {
				color: #FF4D4F;
			}
		}
	}
}

.list-footer {
	padding: 48rpx 0;
	display: flex;
	justify-content: center;
	align-items: center;
	width: 100%;
}

.no-more {
	display: flex;
	align-items: center;
	gap: 24rpx;
	color: #B2B2B2;
	font-size: 24rpx;

	.line {
		width: 80rpx;
		height: 2rpx;
		background: #EBEBEB;
	}

	.no-more-text {
		letter-spacing: 2rpx;
	}
}

.loading-more {
	display: flex;
	align-items: center;
	gap: 16rpx;
	color: $sull-text-secondary;
	font-size: 26rpx;

	.loading-icon-spin {
		width: 28rpx;
		height: 28rpx;
		border: 3rpx solid #e0e0e0;
		border-top-color: $sull-color-primary;
		border-radius: 50%;
		animation: spin 0.8s linear infinite;
	}
}

@keyframes spin {
	to {
		transform: rotate(360deg);
	}
}

/* Shared Card Style */
.card {
	background: #fff;
	border-radius: 24rpx;
	padding: 32rpx;
	box-shadow: 0 4rpx 16rpx rgba(0, 0, 0, 0.02);
}

.loading-state,
.empty-state {
	padding: 100rpx 0;
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
}

.empty-image {
	width: 180rpx;
	height: 180rpx;
	margin-bottom: 8rpx;
}

.empty-title {
	font-size: 32rpx;
	font-weight: bold;
	color: $sull-text-main;
	margin-bottom: 4rpx;
}

.empty-subtitle {
	font-size: 26rpx;
	color: $sull-text-secondary;
}

.spacer {
	height: 200rpx;
}

/* Custom Drawer */
.drawer-mask {
	position: fixed;
	top: 0;
	left: 0;
	width: 100%;
	height: 100%;
	background: rgba(0, 0, 0, 0.5);
	z-index: 900;
	opacity: 0;
	pointer-events: none;
	transition: opacity 0.3s;

	&.show {
		opacity: 1;
		pointer-events: auto;
	}
}

.drawer-panel {
	position: fixed;
	top: 0;
	left: 0;
	width: 500rpx;
	height: 100%;
	background: #fff;
	z-index: 901;
	transform: translateX(-100%);
	transition: transform 0.3s cubic-bezier(0.25, 1, 0.5, 1);
	display: flex;
	flex-direction: column;

	&.show {
		transform: translateX(0);
	}
}

.drawer-header {
	height: 160rpx;
	background: linear-gradient(135deg, $sull-color-primary 0%, lighten($sull-color-primary, 10%) 100%);
	display: flex;
	align-items: flex-end;
	justify-content: space-between;
	padding: 32rpx;

	.drawer-title {
		font-size: 40rpx;
		font-weight: bold;
		color: #fff;
		margin-bottom: 8rpx;
	}

	.close-btn {
		color: #fff;
		font-size: 40rpx;
		margin-bottom: 12rpx;
	}
}

.drawer-list {
	flex: 1;
	padding: 32rpx 0;
}

.drawer-item {
	display: flex;
	align-items: center;
	padding: 32rpx 48rpx;
	cursor: pointer;
	position: relative;
	border-bottom: 2rpx solid #f9f9f9;

	&:active {
		background: #f5f5f5;
	}

	&.active {
		background: #E8F3FF;

		.item-text {
			color: $sull-color-primary;
			font-weight: bold;
		}
	}

	.item-text {
		font-size: 30rpx;
		color: $sull-text-main;
	}

	.active-indicator {
		position: absolute;
		right: 48rpx;
		color: $sull-color-primary;
		font-weight: bold;
	}
}

/* Operator Modal Styles (Bottom Sheet) */
.operator-overlay {
	position: fixed;
	top: 0;
	left: 0;
	width: 100%;
	height: 100%;
	background: rgba(0, 0, 0, 0.5);
	z-index: 900;
	display: flex;
	align-items: flex-end;
	justify-content: center;
	backdrop-filter: blur(4rpx);
}

.operator-modal {
	width: 100%;
	background: #fff;
	border-radius: 40rpx 40rpx 0 0;
	padding: 40rpx;
	padding-bottom: 80rpx;
	display: flex;
	flex-direction: column;
	max-height: 70vh;
	animation: slideUp 0.3s ease;
}

.operator-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-bottom: 32rpx;
	gap: 24rpx;
}

.search-box {
	flex: 1;
	position: relative;
	display: flex;
	align-items: center;
}

.search-input {
	flex: 1;
	height: 80rpx;
	background: #F5F7FA;
	border-radius: 40rpx;
	padding: 0 72rpx 0 32rpx;
	font-size: 28rpx;
}

.clear-icon {
	position: absolute;
	right: 24rpx;
	color: #999;
	font-size: 28rpx;
	padding: 8rpx;
	z-index: 10;
}

.operator-list {
	flex: 1;
	height: 0;
	min-height: 600rpx;
}

.operator-item {
	padding: 32rpx;
	border-bottom: 2rpx solid #f9f9f9;
	display: flex;
	justify-content: space-between;
	align-items: center;

	&:active {
		background: #f5f5f5;
	}

	&.active {
		background: #E8F3FF;

		.item-text {
			color: $sull-color-primary;
			font-weight: bold;
		}
	}
}

.item-text {
	font-size: 30rpx;
	color: $sull-text-main;
}

.check-icon {
	color: $sull-color-primary;
	font-weight: bold;
}

.modal-footer-simple {
	margin-top: 20rpx;
	padding-top: 20rpx;
	border-top: 2rpx solid #eee;
}

.confirm-btn-full {
	width: 100%;
	height: 88rpx;
	background: $sull-color-primary;
	color: #fff;
	border-radius: 44rpx;
	font-size: 32rpx;
	font-weight: 500;
	display: flex;
	align-items: center;
	justify-content: center;
}

@keyframes slideUp {
	from {
		transform: translateY(100%);
	}

	to {
		transform: translateY(0);
	}
}

/* Column Settings */
.sticky-col {
	position: sticky;
	background-color: #fff;
	box-shadow: 4rpx 0 8rpx rgba(0, 0, 0, 0.05);
}

/* Ensure header sticky col matches header background */
.table-header .sticky-col {
	background-color: #F5F7FA;
}

.checkbox-box {
	width: 40rpx;
	height: 40rpx;
	border: 2rpx solid #dcdfe6;
	border-radius: 8rpx;
	display: flex;
	align-items: center;
	justify-content: center;
	transition: all 0.2s;

	&.checked {
		background-color: $sull-color-primary;
		border-color: $sull-color-primary;
		color: #fff;
		font-size: 28rpx;
	}
}
</style>

<style lang="scss" scoped>
/* Custom Refresher */
.custom-refresher {
	height: 120rpx;
	width: 100%;
	display: flex;
	align-items: center;
	justify-content: center;
	position: absolute;
	top: -120rpx;
	left: 0;
	transition: transform 0.2s linear;
}

.refresher-content {
	display: flex;
	flex-direction: column;
	align-items: center;
	gap: 8rpx;
}

.refresher-icon {
	width: 64rpx;
	height: 64rpx;
	background-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='%23409EFF'%3E%3Cpath d='M19 14h-1V8h-3V6h-2v2h-2V6H9v2H7v6H6c-1.1 0-2 .9-2 2v3c0 1.1.9 2 2 2h1c0 1.66 1.34 3 3 3s3-1.34 3-3h4c0 1.66 1.34 3 3 3s3-1.34 3-3h1c1.1 0 2-.9 2-2v-3c0-1.1-.9-2-2-2zM8 19c-.55 0-1-.45-1-1s.45-1 1-1 1 .45 1 1-.45 1-1 1zm10 0c-.55 0-1-.45-1-1s.45-1 1-1 1 .45 1 1-.45 1-1 1zM8 8h2v2H8V8zm0 4h2v2H8v-2zm4 4h-2v-2h2v2zm0-4h-2V8h2v4zm4 4h-2v-2h2v2zm0-4h-2V8h2v4z'/%3E%3Cpath d='M4 8h2v2H4zm0 4h2v2H4z' fill-opacity='.3'/%3E%3C/svg%3E");
	background-size: contain;
	background-repeat: no-repeat;
	transition: transform 0.3s ease;
}

.refresher-icon.animating {
	animation: ambulance-bounce 0.8s infinite ease-in-out;
}

.refresher-text {
	font-size: 20rpx;
	color: $sull-color-primary;
	opacity: 0.8;
}

@keyframes ambulance-bounce {

	0%,
	100% {
		transform: translateY(0);
	}

	50% {
		transform: translateY(-12rpx);
	}
}
</style>

<template>
	<view class="container">
		<!-- Header -->
		<view class="custom-header">
			<view class="nav-left" @click="toggleDrawer">
				<text class="menu-icon">☰</text>
			</view>
			<text class="header-title">{{ $t('index.nav.statistics') }}</text>
			<view class="nav-right"></view>
		</view>

		<!-- Main Content Scroll -->
		<scroll-view scroll-y class="main-content">
			<view class="content-wrapper">

				<!-- Row 1: Title -->
				<view class="row-title">
					<view class="title-decoration"></view>
					<text class="page-title">{{ activeCategory.title }}</text>
				</view>

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
						<view class="filter-control-box" style="background: #F5F7FA; border: 1px solid #E9E9ED;">
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
				</view>

				<!-- Row 4: Results Table -->
				<view class="row-results card">
					<!-- Scroll Wrapper for Table -->
					<scroll-view scroll-x class="table-scroll-x" :enable-flex="true">
						<view class="table-content" :style="{ width: tableWidth }">
							<!-- Header -->
							<view class="table-header">
								<view v-for="(col, index) in activeCategory.columns" :key="index" class="th"
									:style="{ width: col.width + 'px', flex: 'none' }">
									{{ col.label }}
								</view>
							</view>

							<!-- Loading / Empty -->
							<view v-if="loading" class="loading-state">
								<text>Loading...</text>
							</view>
							<view v-else-if="results.length === 0" class="empty-state">
								<text class="empty-icon">📭</text>
								<text class="empty-text">No Data</text>
							</view>

							<!-- Body -->
							<scroll-view v-else scroll-y class="table-body-scroll" style="max-height: 400px;"
								@scrolltolower="loadMore">
								<view class="table-body">
									<view v-for="(row, rIndex) in results" :key="rIndex" class="tr">
										<view v-for="(col, cIndex) in activeCategory.columns" :key="cIndex" class="td"
											:class="{ 'text-red': col.key === 'count' && row[col.key] < 0 }"
											:style="{ width: col.width + 'px', flex: 'none' }">
											{{ row[col.key] }}
										</view>
									</view>
									<view class="list-footer">
										<text v-if="loadingMore">加载中...</text>
										<text v-else-if="pagination.noMore">没有更多数据了</text>
									</view>
								</view>
							</scroll-view>
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

		<BottomNav activePage="statistics" />
	</view>
</template>

<script>
import BottomNav from '@/components/BottomNav/BottomNav.vue';
import FooterInfo from '@/components/FooterInfo/FooterInfo.vue';
import dayjs from 'dayjs';
import request from '@/utils/request.js';
import * as XLSX from 'xlsx';

export default {
	components: {
		BottomNav,
		FooterInfo
	},
	data() {
		return {
			showDrawer: false,
			loading: false,
			filter: {
				startDate: '',
				endDate: '',
				operators: [],
				status: ['normal'], // Default selected
				itemName: ''
			},
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
						{ label: '物品名称', key: 'name', width: 140 },
						{ label: '规格', key: 'spec', width: 80 },
						{ label: '批号', key: 'batch', width: 100 },
						{ label: '有效期', key: 'expiry', width: 100 },
						{ label: '使用量', key: 'count', width: 70 },
						{ label: '库存', key: 'stock', width: 70 },
						{ label: '操作时间', key: 'time', width: 140 }
					]
				},
				{
					id: 'handover',
					title: '交接班统计',
					filters: { time: true, operator: true, status: false },
					columns: [
						{ label: '日期', key: 'date', width: 120 },
						{ label: '交接时间', key: 'time', width: 160 },
						{ label: '班次', key: 'shift', width: 80 },
						{ label: '交班人', key: 'giver', width: 100 },
						{ label: '接班人', key: 'receiver', width: 100 },
						{ label: '状态', key: 'status', width: 80 }
					]
				},
				{
					id: 'stock',
					title: '库存统计',
					filters: { time: false, operator: false, status: false, itemName: true },
					columns: [
						{ label: '物品名称', key: 'name', width: 140 },
						{ label: '当前库存', key: 'current', width: 80 },
						{ label: '基数', key: 'safe', width: 80 },
						{ label: '批号数', key: 'batchCount', width: 80 },
						{ label: '最近过期', key: 'nearExpiry', width: 100 },
						{ label: '上次使用', key: 'lastUse', width: 140 }
					]
				}
			],
			activeCategory: {},
			results: [],
			userOrgInfo: {
				OrganizationId: '',
				DepartmentId: ''
			},
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
			return total > 0 ? total + 'px' : '100%';
		}
	},
	onLoad() {
		// Init dates
		const now = dayjs();
		this.filter.startDate = now.format('YYYY-MM-DD');
		this.filter.endDate = now.add(1, 'day').format('YYYY-MM-DD');

		// Load User Info
		const uInfo = uni.getStorageSync('userOrgInfo');
		if (uInfo && uInfo.User) {
			this.userOrgInfo.OrganizationId = uInfo.User.OrganizationId;
			this.userOrgInfo.DepartmentId = uInfo.User.DepartmentId;
		}

		// Restore last selected category
		const lastCatId = uni.getStorageSync('STATISTICS_LAST_CATEGORY');
		if (lastCatId) {
			const found = this.categories.find(c => c.id === lastCatId);
			this.activeCategory = found || this.categories[0];
		} else {
			this.activeCategory = this.categories[0];
		}
	},
	methods: {
		toggleDrawer() {
			this.showDrawer = !this.showDrawer;
		},
		selectCategory(cat) {
			this.activeCategory = cat;
			uni.setStorageSync('STATISTICS_LAST_CATEGORY', cat.id);
			this.showDrawer = false;
			this.results = []; // Clear old results
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
			if (this.userOrgInfo.OrganizationId) {
				orgDeptWheres.push({ name: 'OrganizationId', value: this.userOrgInfo.OrganizationId, displayType: '=' });
			}
			if (this.userOrgInfo.DepartmentId) {
				orgDeptWheres.push({ name: 'DepartmentId', value: this.userOrgInfo.DepartmentId, displayType: '=' });
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
					wheres.push({ name: 'CreationTime', value: this.filter.startDate + ' 00:00:00,' + this.filter.endDate + ' 00:00:00' });
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
			}

			if (reqPromise) {
				reqPromise.then(data => {
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
		handleExport() {
			if (this.results.length === 0) {
				uni.showToast({ title: '无数据可导出', icon: 'none' });
				return;
			}

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
				const wbout = XLSX.write(wb, { bookType: 'xlsx', type: 'array' });
				const fs = wx.getFileSystemManager();
				const filePath = `${wx.env.USER_DATA_PATH}/${fileName}`;

				fs.writeFile({
					filePath: filePath,
					data: wbout,
					encoding: 'binary',
					success: () => {
						uni.hideLoading();
						uni.openDocument({
							filePath: filePath,
							showMenu: true,
							success: function () {
								console.log('Open Document Success');
							},
							fail: function (err) {
								uni.showToast({ title: '打开文件失败', icon: 'none' });
							}
						});
					},
					fail: (err) => {
						uni.hideLoading();
						console.error(err);
						uni.showToast({ title: '写入文件失败', icon: 'none' });
					}
				});
				// #endif

				// #ifndef H5 || MP-WEIXIN
				// Other platforms not fully implemented yet
				// uni.hideLoading();
				// uni.showToast({ title: '请在网页端或小程序使用导出', icon: 'none' });
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
	background-color: $sull-color-background;
}

.custom-header {
	height: 44px;
	padding-top: 40px;
	/* Status bar spacer */
	background: #fff;
	display: flex;
	align-items: center;
	justify-content: space-between;
	padding-left: 16px;
	padding-right: 16px;
	box-shadow: 0 1px 4px rgba(0, 0, 0, 0.05);

	.header-title {
		font-size: 18px;
		font-weight: bold;
		color: $sull-text-main;
	}

	.nav-left,
	.nav-right {
		width: 40px;
		display: flex;
		align-items: center;
	}

	.menu-icon {
		font-size: 24px;
		color: $sull-text-main;
	}
}

.main-content {
	flex: 1;
	height: 0;
	padding: 16px;
	box-sizing: border-box;
}

.content-wrapper {
	display: flex;
	flex-direction: column;
	gap: 16px;
}

/* Row 1: Title */
.row-title {
	display: flex;
	align-items: center;
	gap: 8px;
	margin-bottom: 4px;

	.title-decoration {
		width: 4px;
		height: 18px;
		background: $sull-color-primary;
		border-radius: 2px;
	}

	.page-title {
		font-size: 20px;
		font-weight: bold;
		color: $sull-text-main;
	}
}

/* Row 2: Filters */
.row-filters {
	display: flex;
	flex-direction: column;
	padding: 16px !important;
	gap: 16px;

	.filter-row {
		display: flex;
		/* Horizontal row */
		align-items: center;
		gap: 12px;
	}

	.filter-label-fixed {
		font-size: 14px;
		color: $sull-text-main;
		font-weight: bold;
		width: 70px;
		/* Fixed width as requested */
		flex-shrink: 0;
	}

	/* Time Range */
	.filter-control-group {
		flex: 1;
		display: flex;
		align-items: center;
		gap: 8px;

		.date-picker {
			flex: 1;
		}

		.date-input {
			height: 36px;
			background: #F5F7FA;
			border-radius: 6px;
			display: flex;
			align-items: center;
			justify-content: center;
			font-size: 13px;
			color: $sull-text-main;
			border: 1px solid $sull-color-line;
		}

		.date-divider {
			color: $sull-text-secondary;
			font-size: 13px;
		}
	}

	/* Operator Selector */
	.filter-control-box {
		flex: 1;
		min-height: 36px;
		background: #F5F7FA;
		border-radius: 6px;
		display: flex;
		align-items: center;
		justify-content: space-between;
		padding: 0 10px;
		border: 1px solid $sull-color-line;

		.control-value {
			font-size: 14px;
			color: $sull-text-main;
		}

		.control-value.placeholder {
			color: $sull-text-placeholder;
		}

		.control-icon {
			font-size: 10px;
			color: $sull-text-secondary;
		}
	}

	/* Status Checkboxes */
	.checkbox-group {
		flex: 1;
		display: flex;
		flex-wrap: wrap;
		gap: 12px;
	}

	.checkbox-item {
		display: flex;
		align-items: center;
		gap: 6px;
		cursor: pointer;
	}

	.checkbox-box {
		width: 18px;
		height: 18px;
		border: 1px solid #ccc;
		border-radius: 4px;
		display: flex;
		align-items: center;
		justify-content: center;
		font-size: 12px;
		color: #fff;
		background: #fff;

		&.checked {
			background: $sull-color-primary;
			border-color: $sull-color-primary;
		}
	}

	.checkbox-label {
		font-size: 14px;
		color: $sull-text-main;
	}
}

.list-footer {
	padding: 10px;
	text-align: center;
	font-size: 12px;
	color: $sull-text-secondary;
}


/* Row 3: Actions */
.row-actions {
	display: flex;
	gap: 10px;
	justify-content: flex-start;

	.action-btn {
		height: 32px;
		border-radius: 16px;
		display: flex;
		align-items: center;
		justify-content: center;
		padding: 0 16px;
		font-size: 13px;
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
		border: 1px solid $sull-color-line;
		color: $sull-text-main;
	}
}

/* Row 4: Results */
.row-results {
	min-height: 200px;
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
	border-left: 1px solid $sull-color-line;
	border-right: 1px solid $sull-color-line;
}

.table-header {
	background: #F5F7FA;
	display: flex;
	border-bottom: 1px solid $sull-color-line;
	border-top: 1px solid $sull-color-line;

	.th {
		font-size: 13px;
		color: $sull-text-secondary;
		font-weight: bold;
		text-align: center;
		padding: 10px 8px;
		box-sizing: border-box;
		border-right: 1px solid $sull-color-line;
		display: flex;
		align-items: center;
		justify-content: center;

		&:last-child {
			border-right: none;
		}
	}
}

.table-body-scroll {
	width: 100%;
}

.table-body {
	display: flex;
	flex-direction: column;

	.tr {
		display: flex;
		border-bottom: 1px solid $sull-color-line;
		align-items: stretch;

		/* Zebra Striping */
		&:nth-child(even) {
			background-color: #F8F9FA;
		}

		.td {
			font-size: 14px;
			color: $sull-text-main;
			text-align: center;
			white-space: nowrap;
			overflow: hidden;
			text-overflow: ellipsis;
			padding: 12px 8px;
			box-sizing: border-box;
			border-right: 1px solid $sull-color-line;
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

/* Shared Card Style */
.card {
	background: #fff;
	border-radius: 12px;
	padding: 16px;
	box-shadow: 0 2px 8px rgba(0, 0, 0, 0.02);
}

.loading-state,
.empty-state {
	padding: 40px;
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	gap: 10px;
	color: $sull-text-secondary;

	.empty-icon {
		font-size: 40px;
	}
}

.spacer {
	height: 100px;
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
	width: 250px;
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
	height: 80px;
	background: linear-gradient(135deg, $sull-color-primary 0%, lighten($sull-color-primary, 10%) 100%);
	display: flex;
	align-items: flex-end;
	justify-content: space-between;
	padding: 16px;

	.drawer-title {
		font-size: 20px;
		font-weight: bold;
		color: #fff;
		margin-bottom: 4px;
	}

	.close-btn {
		color: #fff;
		font-size: 20px;
		margin-bottom: 6px;
	}
}

.drawer-list {
	flex: 1;
	padding: 16px 0;
}

.drawer-item {
	display: flex;
	align-items: center;
	padding: 16px 24px;
	cursor: pointer;
	position: relative;
	border-bottom: 1px solid #f9f9f9;

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
		font-size: 15px;
		color: $sull-text-main;
	}

	.active-indicator {
		position: absolute;
		right: 24px;
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
	backdrop-filter: blur(2px);
}

.operator-modal {
	width: 100%;
	background: #fff;
	border-radius: 20px 20px 0 0;
	padding: 20px;
	padding-bottom: 40px;
	display: flex;
	flex-direction: column;
	max-height: 70vh;
	animation: slideUp 0.3s ease;
}

.operator-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-bottom: 16px;
	gap: 12px;
}

.search-box {
	flex: 1;
	position: relative;
	display: flex;
	align-items: center;
}

.search-input {
	flex: 1;
	height: 40px;
	background: #F5F7FA;
	border-radius: 20px;
	padding: 0 36px 0 16px;
	font-size: 14px;
}

.clear-icon {
	position: absolute;
	right: 12px;
	color: #999;
	font-size: 14px;
	padding: 4px;
	z-index: 10;
}

.operator-list {
	flex: 1;
	height: 0;
	min-height: 300px;
}

.operator-item {
	padding: 16px;
	border-bottom: 1px solid #f9f9f9;
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
	font-size: 15px;
	color: $sull-text-main;
}

.check-icon {
	color: $sull-color-primary;
	font-weight: bold;
}

.modal-footer-simple {
	margin-top: 10px;
	padding-top: 10px;
	border-top: 1px solid #eee;
}

.confirm-btn-full {
	width: 100%;
	height: 44px;
	background: $sull-color-primary;
	color: #fff;
	border-radius: 22px;
	font-size: 16px;
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
</style>

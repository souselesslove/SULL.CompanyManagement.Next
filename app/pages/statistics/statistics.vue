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
					<view class="filter-row">
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

					<!-- Operators Condition (Multi-select Mock) -->
					<view class="filter-row">
						<text class="filter-label-fixed">操作人员</text>
						<view class="filter-control-box" @click="openOperatorSelector">
							<text class="control-value" :class="{ placeholder: !filter.operators.length }">
								{{ filter.operators.length > 0 ? `已选 ${filter.operators.join(', ')}` : '请选择操作人员' }}
							</text>
							<text class="control-icon">▼</text>
						</view>
					</view>

					<!-- Status Condition (Checkboxes) -->
					<view class="filter-row">
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
					<view class="action-btn btn-share" @click="handleShare">
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
								<text v-for="(col, index) in activeCategory.columns" :key="index" class="th"
									:style="{ width: col.width + 'px', flex: 'none' }">
									{{ col.label }}
								</text>
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
							<scroll-view v-else scroll-y class="table-body-scroll" style="max-height: 400px;">
								<view class="table-body">
									<view v-for="(row, rIndex) in results" :key="rIndex" class="tr">
										<text v-for="(col, cIndex) in activeCategory.columns" :key="cIndex" class="td"
											:style="{ width: col.width + 'px', flex: 'none' }">
											{{ row[col.key] }}
										</text>
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

		<BottomNav activePage="statistics" />
	</view>
</template>

<script>
import BottomNav from '@/components/BottomNav/BottomNav.vue';
import FooterInfo from '@/components/FooterInfo/FooterInfo.vue';
import dayjs from 'dayjs';

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
				status: ['normal'] // Default selected
			},
			statusOptions: [
				{ label: '正常', value: 'normal' },
				{ label: '异常', value: 'abnormal' },
				{ label: '处理中', value: 'processing' }
			],
			// Column widths now in px for horizontal scroll
			categories: [
				{
					id: 'consumption',
					title: '物品消耗统计',
					columns: [
						{ label: '物品名称', key: 'name', width: 140 },
						{ label: '规格', key: 'spec', width: 100 },
						{ label: '消耗量', key: 'count', width: 80 },
						{ label: '单位', key: 'unit', width: 60 }
					]
				},
				{
					id: 'handover',
					title: '交接班统计',
					columns: [
						{ label: '日期', key: 'date', width: 120 },
						{ label: '班次', key: 'shift', width: 80 },
						{ label: '交班人', key: 'giver', width: 100 },
						{ label: '接班人', key: 'receiver', width: 100 },
						{ label: '状态', key: 'status', width: 80 }
					]
				},
				{
					id: 'stock',
					title: '库存统计',
					columns: [
						{ label: '物品名称', key: 'name', width: 140 },
						{ label: '当前库存', key: 'current', width: 100 },
						{ label: '安全库存', key: 'safe', width: 100 },
						{ label: '状态', key: 'status', width: 90 }
					]
				}
			],
			activeCategory: {},
			results: []
		};
	},
	computed: {
		tableWidth() {
			if (!this.activeCategory.columns) return '100%';
			const total = this.activeCategory.columns.reduce((sum, col) => sum + (col.width || 0), 0);
			// ensure at least 100% width
			return total > 0 ? total + 'px' : '100%';
		}
	},
	onLoad() {
		// Init dates
		const end = dayjs();
		const start = end.subtract(30, 'day');
		this.filter.startDate = start.format('YYYY-MM-DD');
		this.filter.endDate = end.format('YYYY-MM-DD');

		// Default active
		this.activeCategory = this.categories[0];
		this.handleQuery();
	},
	methods: {
		toggleDrawer() {
			this.showDrawer = !this.showDrawer;
		},
		selectCategory(cat) {
			this.activeCategory = cat;
			this.showDrawer = false;
			this.results = []; // Clear old results
			this.handleQuery(); // Auto query on switch? or wait for user? Let's auto query for better UX.
		},
		onStartDateChange(e) {
			this.filter.startDate = e.detail.value;
		},
		onEndDateChange(e) {
			this.filter.endDate = e.detail.value;
		},
		openOperatorSelector() {
			// Mock selector, simply toggle a dummy user for demo
			if (this.filter.operators.includes('张三')) {
				this.filter.operators = [];
			} else {
				this.filter.operators = ['张三', '李四'];
			}
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
			this.loading = true;
			// Mock API delay
			setTimeout(() => {
				this.results = this.getMockData(this.activeCategory.id);
				this.loading = false;
				uni.showToast({ title: '查询成功', icon: 'success' });
			}, 500);
		},
		handleShare() {
			uni.showToast({ title: '分享功能开发中', icon: 'none' });
		},
		handleExport() {
			uni.showToast({ title: '导出已请求', icon: 'success' });
		},
		getMockData(type) {
			if (type === 'consumption') {
				return [
					{ name: '肾上腺素注射液', spec: '1ml:1mg', count: 45, unit: '支' },
					{ name: '一次性注射器', spec: '5ml', count: 120, unit: '具' },
					{ name: '生理盐水', spec: '250ml', count: 30, unit: '瓶' },
					{ name: '碘伏消毒液', spec: '500ml', count: 5, unit: '瓶' },
					{ name: '医用棉签', spec: '50支/包', count: 10, unit: '包' }
				];
			} else if (type === 'handover') {
				return [
					{ date: '2025-12-01', shift: '早班', giver: '张三', receiver: '李四', status: '完成' },
					{ date: '2025-12-01', shift: '中班', giver: '李四', receiver: '王五', status: '完成' },
					{ date: '2025-12-01', shift: '晚班', giver: '王五', receiver: '赵六', status: '完成' },
					{ date: '2025-12-02', shift: '早班', giver: '赵六', receiver: '张三', status: '异常' }
				];
			} else if (type === 'stock') {
				return [
					{ name: '阿托品注射液', current: 15, safe: 10, status: '正常' },
					{ name: '多巴胺注射液', current: 5, safe: 8, status: '不足' },
					{ name: '利多卡因', current: 20, safe: 10, status: '正常' },
					{ name: '地塞米松', current: 2, safe: 5, status: '严重不足' }
				];
			}
			return [];
		}
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
}

.table-header {
	background: #F5F7FA;
	display: flex;
	padding: 10px 0;
	border-bottom: 1px solid $sull-color-line;

	.th {
		font-size: 13px;
		color: $sull-text-secondary;
		font-weight: bold;
		text-align: center;
		padding: 0 8px;
		box-sizing: border-box;
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
		padding: 12px 0;
		border-bottom: 1px solid #f9f9f9;
		align-items: center;

		&:last-child {
			border-bottom: none;
		}

		.td {
			font-size: 14px;
			color: $sull-text-main;
			text-align: center;
			white-space: nowrap;
			overflow: hidden;
			text-overflow: ellipsis;
			padding: 0 8px;
			box-sizing: border-box;
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
</style>

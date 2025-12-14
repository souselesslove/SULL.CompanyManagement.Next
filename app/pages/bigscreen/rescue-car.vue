<template>
	<view class="container">
		<scroll-view scroll-y class="main-scroll">
			<!-- Header -->
			<view class="header">
				<text class="title">{{ $t('bigScreen.rescueCar.title') }}</text>
			</view>

			<view class="scroll-content">
				<view class="info-section">
					<view class="card-header">
						<view class="header-left">
							<view class="header-icon">🏥</view>
							<text class="header-title">{{ rescueCar.DisplayName || rescueCar.Name || '骨伤一科' }}</text>
						</view>
						<view class="header-right">
							<text class="manager-label">{{ $t('bigScreen.rescueCar.manager') }}:</text>
							<text class="manager-value">{{ rescueCar.Manager || '李秋水' }}</text>
						</view>
					</view>
					<!-- Info Carousel -->
					<swiper class="info-swiper" :autoplay="false" :interval="5000" :duration="500" :circular="true"
						:indicator-dots="false" indicator-active-color="#1890ff" indicator-color="rgba(0,0,0,0.1)">
						<!-- Slide 1: Status Overview -->
						<swiper-item>
							<view class="info-grid">
								<view class="info-item">
									<view class="item-icon">💊</view>
									<view class="item-content">
										<text class="item-label">{{ $t('bigScreen.rescueCar.itemTypes') }}</text>
										<view class="value-unit-row">
											<text class="item-value">{{ stats.itemTypes }}</text>
											<text class="item-unit">{{ $t('index.unit.type') }}</text>
										</view>
									</view>
								</view>
								<view class="info-item">
									<view class="item-icon">💰</view>
									<view class="item-content">
										<text class="item-label">{{ $t('bigScreen.rescueCar.itemValue') }}</text>
										<view class="value-unit-row">
											<text class="item-value">{{ stats.itemValue }}</text>
											<text class="item-unit">{{ $t('index.unit.tenThousand') }}</text>
										</view>
									</view>
								</view>
								<view class="info-item warning">
									<view class="item-icon">⚠️</view>
									<view class="item-content">
										<text class="item-label">{{ $t('bigScreen.rescueCar.stockWarning') }}</text>
										<view class="value-unit-row">
											<text class="item-value warning-text">{{ stats.stockWarning }}</text>
											<text class="item-unit warning-text">{{ $t('index.unit.type') }}</text>
										</view>
									</view>
								</view>
								<view class="info-item warning">
									<view class="item-icon">⏰</view>
									<view class="item-content">
										<text class="item-label">{{ $t('bigScreen.rescueCar.expiryWarning') }}</text>
										<view class="value-unit-row">
											<text class="item-value warning-text">{{ stats.expiryWarning }}</text>
											<text class="item-unit warning-text">{{ $t('index.unit.type') }}</text>
										</view>
									</view>
								</view>
							</view>
						</swiper-item>

						<!-- Slide 2: Tasks Overview -->
						<swiper-item>
							<view class="tasks-grid-2x2">
								<view class="task-item-compact">
									<view class="task-header">
										<view class="task-icon-small">🔄</view>
										<text class="task-label-inline">{{ $t('bigScreen.rescueCar.handover') }}</text>
										<text class="task-status normal">{{ $t('bigScreen.rescueCar.statusNormal')
										}}</text>
									</view>
									<view class="task-times">
										<text class="time-row"><text class="time-label">{{
											$t('bigScreen.rescueCar.lastTime') }}</text><text
												class="time-last">2025-12-03 08:00</text></text>
										<text class="time-row"><text class="time-label">{{
											$t('bigScreen.rescueCar.nextTime') }}</text><text
												class="time-next">2025-12-04 08:00</text></text>
									</view>
								</view>
								<view class="task-item-compact">
									<view class="task-header">
										<view class="task-icon-small">✨</view>
										<text class="task-label-inline">{{ $t('bigScreen.rescueCar.cleaning') }}</text>
										<text class="task-status overdue">{{ $t('bigScreen.rescueCar.statusOverdue')
										}}</text>
									</view>
									<view class="task-times">
										<text class="time-row"><text class="time-label">{{
											$t('bigScreen.rescueCar.lastTime') }}</text><text
												class="time-last">2025-11-30 10:00</text></text>
										<text class="time-row"><text class="time-label">{{
											$t('bigScreen.rescueCar.nextTime') }}</text><text
												class="time-next">2025-12-04 10:00</text></text>
									</view>
								</view>
								<view class="task-item-compact">
									<view class="task-header">
										<view class="task-icon-small">📝</view>
										<text class="task-label-inline">{{ $t('bigScreen.rescueCar.check') }}</text>
										<text class="task-status normal">{{ $t('bigScreen.rescueCar.statusNormal')
										}}</text>
									</view>
									<view class="task-times">
										<text class="time-row"><text class="time-label">{{
											$t('bigScreen.rescueCar.lastTime') }}</text><text
												class="time-last">2025-12-04 14:00</text></text>
										<text class="time-row"><text class="time-label">{{
											$t('bigScreen.rescueCar.nextTime') }}</text><text
												class="time-next">2025-12-05 14:00</text></text>
									</view>
								</view>
								<view class="task-item-compact">
									<view class="task-header">
										<view class="task-icon-small">🔒</view>
										<text class="task-label-inline">{{ $t('bigScreen.rescueCar.sealing') }}</text>
										<text class="task-status open">{{ $t('bigScreen.rescueCar.statusOpen') }}</text>
									</view>
									<view class="task-times">
										<text class="time-row"><text class="time-label">{{
											$t('bigScreen.rescueCar.lastTime') }}</text><text
												class="time-last">2025-12-03 16:30</text></text>
										<text class="time-row"><text class="time-label">{{
											$t('bigScreen.rescueCar.nextTime') }}</text><text
												class="time-next">2025-12-04 08:00</text></text>
									</view>
								</view>
							</view>
						</swiper-item>

						<!-- Slide 3: Handover Stats -->
						<swiper-item>
							<view class="stats-slide">
								<view class="stats-left">
									<text class="stats-title">{{ $t('bigScreen.rescueCar.handoverRate') }}</text>
									<text class="stats-value">98%</text>
									<view class="stats-trend up">
										<text>▲ 2%</text>
										<text class="trend-label">vs last month</text>
									</view>
								</view>
								<view class="stats-right">
									<SimpleLineChart :data="[92, 95, 98]" color="#52c41a" :height="80" />
								</view>
							</view>
						</swiper-item>

						<!-- Slide 4: Expiry Stats -->
						<swiper-item>
							<view class="stats-slide">
								<view class="stats-left">
									<text class="stats-title">{{ $t('bigScreen.rescueCar.expiryIssueRate') }}</text>
									<text class="stats-value">0.5%</text>
									<view class="stats-trend down">
										<text>▼ 0.2%</text>
										<text class="trend-label">vs last month</text>
									</view>
								</view>
								<view class="stats-right">
									<SimpleLineChart :data="[1.2, 0.8, 0.5]" color="#faad14" :height="80" />
								</view>
							</view>
						</swiper-item>

						<!-- Slide 5: Cleaning Stats -->
						<swiper-item>
							<view class="stats-slide">
								<view class="stats-left">
									<text class="stats-title">{{ $t('bigScreen.rescueCar.cleaningRate') }}</text>
									<text class="stats-value">100%</text>
									<view class="stats-trend same">
										<text>- 0%</text>
										<text class="trend-label">vs last month</text>
									</view>
								</view>
								<view class="stats-right">
									<SimpleLineChart :data="[99, 100, 100]" color="#1890ff" :height="80" />
								</view>
							</view>
						</swiper-item>

						<!-- Slide 6: Abnormality Stats -->
						<swiper-item>
							<view class="stats-slide">
								<view class="stats-left">
									<text class="stats-title">{{ $t('bigScreen.rescueCar.abnormalRate') }}</text>
									<text class="stats-value">1.2%</text>
									<view class="stats-trend down">
										<text>▼ 0.5%</text>
										<text class="trend-label">vs last month</text>
									</view>
								</view>
								<view class="stats-right">
									<SimpleLineChart :data="[2.5, 1.8, 1.2]" color="#f5222d" :height="80" />
								</view>
							</view>
						</swiper-item>
					</swiper>
				</view>

				<!-- 2. Items Section -->
				<view class="items-section">
					<!-- Layers -->
					<view class="layers-container">
						<view class="layer-item" v-for="(layer, index) in layers" :key="index">
							<!-- Layer Header -->
							<view class="layer-header" @click="toggleLayer(index)">
								<text class="layer-title">{{ layer.name }}</text>
								<view class="layer-info">
									<view class="abnormal-icons" v-if="hasAbnormal(layer)">
										<view v-if="hasExpired(layer)" class="icon-btn"
											@click.stop="setFilter(index, 'expired')"
											:class="{ 'active': layer.filterType === 'expired' }">
											<text class="info-icon">🚫</text>
										</view>
										<view v-if="hasNearExpiry(layer)" class="icon-btn"
											@click.stop="setFilter(index, 'nearExpiry')"
											:class="{ 'active': layer.filterType === 'nearExpiry' }">
											<text class="info-icon">⏰</text>
										</view>
									</view>
									<view class="info-badge" @click.stop="setFilter(index, 'all')"
										:class="{ 'active': !layer.filterType || layer.filterType === 'all' }">
										<text class="info-icon">📦</text>
										<text class="info-num">{{ layer.items.length }}</text>
									</view>
								</view>
							</view>

							<!-- Layer Content -->
							<view class="layer-content" v-if="layer.expanded">
								<view class="list-view">
									<view class="list-card-wrapper" v-for="(item, i) in getFilteredItems(layer)"
										:key="item.id">
										<!-- Active Item Card -->
										<view class="list-card" v-if="!item.isEmpty">
											<view class="card-row top-row">
												<view class="item-name-wrapper">
													<text class="item-name-text"
														:class="{ 'scrolling': item.name.length > 6 }">{{ i + 1 }}.
														{{ item.name }}</text>
												</view>
												<view v-if="item.days <= 30" class="status-icon"
													:class="{ 'icon-warn': item.days < 30 && item.days >= 0, 'shaking': item.days < 0 }">
													🔔</view>
											</view>
											<view class="card-row middle-row">
												<view class="spec-qty-container">
													<text class="item-spec-text">{{ item.spec }}</text>
													<!-- Use value-unit-row for qty if needed, keeping simple for now -->
													<text class="item-qty-text">{{ item.qty }} {{ item.unit
													}}</text>
												</view>
												<view class="days-info">
													<view class="days-badge"
														:class="{ 'badge-expired': item.days < 0, 'badge-danger': item.days >= 0 && item.days < 30, 'badge-warning': item.days >= 30 && item.days <= 60, 'badge-normal': item.days > 60 }">
														<text v-if="item.days < 0" class="days-prefix">过期</text>
														<text class="days-num">{{ Math.abs(item.days) }}</text>
													</view>
													<text class="days-label">天</text>
												</view>
											</view>
											<view class="card-row bottom-row">
												<view class="progress-container"
													:class="{ 'shimmer': item.days < 30 && item.days >= 0 }"
													:style="{ backgroundColor: getProgressColor(item.days) }">
													<view class="progress-green"
														:style="{ width: getGreenProgress(item.days) + '%' }">
													</view>
												</view>
											</view>
										</view>

										<!-- Empty Slot Card -->
										<view class="list-card empty-card" v-else>
											<text class="empty-index">{{ i + 1 }}</text>
											<view class="empty-icon-center">
												<view class="empty-icon"></view>
											</view>
										</view>
									</view>
								</view>
							</view>
						</view>
					</view>
				</view>
			</view>

			<FooterInfo />
		</scroll-view>

		<BottomNav activePage="bigscreen" />
	</view>
</template>

<script>
import FooterInfo from '@/components/FooterInfo/FooterInfo.vue';
import SimpleLineChart from '@/components/SimpleLineChart/SimpleLineChart.vue';
import request from '@/utils/request.js';

export default {
	components: {
		FooterInfo,
		SimpleLineChart
	},
	data() {
		return {
			userOrgInfo: uni.getStorageSync('userOrgInfo') || {},
			selectedRescueCarId: null,
			rescueCar: {},
			layers: [],
			serialList: [],
			itemsMap: {},
			stats: {
				itemTypes: 0,
				stockWarning: 0,
				expiryWarning: 0,
				itemValue: 0
			},
			initialFilterType: null
		}
	},
	onLoad(options) {
		if (options && options.type) {
			this.initialFilterType = parseInt(options.type);
		}
		this.fetchRescueCars();
	},
	methods: {
		async fetchRescueCars() {
			try {
				if (!this.userOrgInfo.Organization || !this.userOrgInfo.Department) return;

				const res = await request.post('api/ssbh/Position/GetPageData', {
					page: 1, rows: 100,
					wheres: JSON.stringify([
						{ name: 'Category', value: 10 }, // Only fetch Warehouses (Rescue Cars)
						{ name: 'OrganizationId', value: this.userOrgInfo.Organization.Id },
						{ name: 'DepartmentId', value: this.userOrgInfo.Department.Id }
					]),
					sort: 'OrderBy', order: 'asc'
				});
				const cars = res.rows || [];
				if (cars.length > 0) {
					this.rescueCar = cars[0];
					this.selectedRescueCarId = this.rescueCar.Id;
					this.fetchDashboardStats(this.selectedRescueCarId);
					// Fetch floors/layers detailed view
					this.fetchFloors(this.selectedRescueCarId);
				} else {
					this.rescueCar = {};
					this.selectedRescueCarId = null;
					this.layers = [];
				}
			} catch (e) {
				console.error(e);
			}
		},
		async fetchFloors(parentId) {
			if (!parentId) return;
			try {
				const res = await request.post('api/ssbh/Position/GetPageData', {
					page: 1, rows: 100,
					wheres: JSON.stringify([
						{ name: 'Category', value: 100 },
						{ name: 'WarehouseId', value: parentId },
						{ name: 'OrganizationId', value: this.userOrgInfo.Organization.Id },
						{ name: 'DepartmentId', value: this.userOrgInfo.Department.Id }
					]),
					sort: 'OrderBy', order: 'asc'
				});
				const floors = res.rows || [];
				console.log('Floors:', floors);
				if (floors.length > 0) {
					// In position-manage, it filters locally, here we query. 
					// We need to fetch serials for ALL floors found.
					const floorIds = floors.map(f => f.Id);
					await this.fetchSerialsByFloors(floorIds, floors);
				} else {
					this.layers = [];
				}
			} catch (e) { console.error(e); }
		},
		async fetchSerialsByFloors(floorIds, floors) {
			if (!floorIds || floorIds.length === 0) return;
			try {
				// Use 'ParentId' with array of floorIds, assuming backend supports 'value' as array or we need to check backend behavior. 
				// position-manage sends array: { name: 'ParentId', value: floorIds }
				const res = await request.post('api/ssbh/Position/GetPageData', {
					page: 1, rows: 500,
					wheres: JSON.stringify([
						{ name: 'Category', value: 110 },
						{ name: 'ParentId', value: floorIds.join(','), DisplayType: 'in' }
					]),
					sort: 'OrderBy', order: 'asc'
				});
				this.serialList = res.rows || [];

				const itemIds = this.serialList.filter(s => s.ItemId > 0).map(s => s.ItemId);
				if (itemIds.length > 0) {
					await this.fetchItemDetails([...new Set(itemIds)]);
				}

				this.transformToLayers(floors, this.serialList);
			} catch (e) { console.error(e); }
		},
		async fetchDashboardStats(warehouseId) {
			try {
				const res = await request.get(`api/ssbh/Position/Statistic?warehouseId=${warehouseId}`);
				if (res) {
					// Price conversion: Cent to Yuan (100)
					let price = (res.PriceCounter || 0) / 100;

					this.stats = {
						itemTypes: res.ItemCounter || 0,
						stockWarning: res.AbnormalStock || 0,
						expiryWarning: res.AbnormalExpiry || 0,
						itemValue: price.toFixed(0) // Display as whole number for Yuan
					};
				}
			} catch (e) {
				console.error('Fetch Dashboard Stats Failed', e);
			}
		},
		async fetchItemDetails(ids) {
			const distinctIds = [...new Set(ids)];
			// We can try fetching all at once if API supports IN, but let's loop or fetch batch if possible
			// Given previous code usage, we might be fetching individually or in small batches.
			// Ideally we fetch all relevant items. For now, let's assume one by one or small batch isn't too slow for this demo,
			// OR use a bulk fetch if available. Previous code used loop.
			// Let's optimize slightly by just looping efficiently
			const promises = distinctIds.map(id => {
				if (this.itemsMap[id]) return Promise.resolve();
				return request.post('api/ssbh/Item/GetPageData', {
					page: 1, rows: 1,
					wheres: JSON.stringify([
						{ name: 'Id', value: id },
						// Secure fetch with Org/Dept
						{ name: 'OrganizationId', value: this.userOrgInfo.Organization.Id },
						{ name: 'DepartmentId', value: this.userOrgInfo.Department.Id }
					])
				}).then(res => {
					if (res.rows && res.rows.length > 0) {
						this.$set(this.itemsMap, id, res.rows[0]);
					}
				});
			});
			await Promise.all(promises);
		},
		getDaysUntil(dateStr) {
			if (!dateStr) return '999';
			const now = new Date();
			const target = new Date(dateStr);
			return Math.ceil((target.getTime() - now.getTime()) / (1000 * 3600 * 24));
		},
		transformToLayers(floors, serials) {
			this.layers = floors.map(floor => {
				const floorSerials = serials.filter(s => s.FloorId === floor.Id);
				// Map serials to dashboard items
				// We need to show ALL serials, even if empty.
				const items = floorSerials.map((s, index) => {
					if (s.ItemId > 0 && this.itemsMap[s.ItemId]) {
						const itemDetail = this.itemsMap[s.ItemId];
						let expiryDate = itemDetail.LatestExpirationDate || itemDetail.ExpiryDate;
						return {
							id: s.Id,
							name: itemDetail.Name,
							spec: itemDetail.Specification,
							qty: s.Stock || itemDetail.AvailableStock,
							unit: itemDetail.Unit,
							expiry: expiryDate,
							days: this.getDaysUntil(expiryDate),
							row: 1, col: index + 1,
							isEmpty: false
						};
					}
					// Empty/Unbound Serial
					return {
						id: s.Id,
						name: '空闲',
						row: 1, col: index + 1,
						isEmpty: true
					};
				}); // No filtering

				return {
					name: floor.Name || floor.DisplayName,
					count: floor.SerialId || 0, // Assuming SerialId on floor is the total count
					expanded: items.length > 0 && items.some(i => !i.isEmpty), // Expand if there are actual items? Or just items.length > 0
					items: items
				};
			});

			// Handle initial filter if any
			if (this.initialFilterType === 10) {
				this.layers.forEach((layer, index) => {
					if (this.hasExpired(layer)) this.setFilter(index, 'expired');
					else if (this.hasNearExpiry(layer)) this.setFilter(index, 'nearExpiry');
				});
			}
		},

		navToHome() {
			uni.reLaunch({
				url: '/pages/index/index'
			});
		},
		navTo(page) {
			let url = '';
			if (page === 'statistics') url = '/pages/statistics/statistics';
			if (page === 'profile') url = '/pages/profile/profile';

			if (url) {
				uni.reLaunch({ url });
			}
		},
		toggleLayer(index) {
			this.layers[index].expanded = !this.layers[index].expanded;
		},
		getProgress(days) {
			return Math.min(100, (days / 365) * 100);
		},
		getGreenProgress(days) {
			if (days < 0) return 0;
			if (days >= 90) return 100;
			return (days / 90) * 100;
		},
		getProgressColor(days) {
			if (days < 0) return '#000000';
			if (days < 30) return '#F56C6C';
			if (days <= 60) return '#E6A23C';
			return '#67C23A';
		},
		getLayerAbnormalCount(layer) {
			if (!layer || !layer.items) return 0;
			return layer.items.filter(item => item.days <= 30).length;
		},
		hasAbnormal(layer) {
			return this.getLayerAbnormalCount(layer) > 0;
		},
		hasExpired(layer) {
			return layer.items && layer.items.some(item => item.days < 0);
		},
		hasNearExpiry(layer) {
			return layer.items && layer.items.some(item => item.days >= 0 && item.days <= 30);
		},
		setFilter(index, type) {
			this.layers[index].filterType = type;
			if (type !== 'all' && !this.layers[index].expanded) {
				this.layers[index].expanded = true;
			}
		},
		getFilteredItems(layer) {
			const filter = layer.filterType || 'all';
			if (filter === 'all') return layer.items;
			if (filter === 'expired') return layer.items.filter(item => item.days < 0);
			if (filter === 'nearExpiry') return layer.items.filter(item => item.days >= 0 && item.days <= 30);
			return layer.items;
		}
	}
}
</script>

<style lang="scss" scoped>
@import '@/sull.scss';

/* Apply border-box selectively to prevent overflow */
.container,
.content,
.info-section,
.layers-container,
.layer-item,
.layer-content,
.list-header,
.list-row,
.col,
.footer,
.car-photo,
.car-info,
.info-row {
	box-sizing: border-box;
}

page {
	background-color: $sull-color-background;
	height: 100%;
	width: 100%;
	overflow-x: hidden;
}

.container {
	display: flex;
	flex-direction: column;
	height: 100vh;
	width: 100%;
	max-width: 100vw;
	overflow-x: hidden;
}

.header {
	padding: 30px 24px 10px 24px;
	display: flex;
	justify-content: center;
	align-items: center;
	background-color: $sull-color-background;
}

.main-scroll {
	flex: 1;
	height: 0;
}

.scroll-content {
	padding: 10px 24px 160px 24px;
}

/* Info Section - Premium Design */
.info-section {
	margin-bottom: 20px;
	background: linear-gradient(135deg, #ffffff 0%, #f8fbff 100%);
	padding: 16px;
	border-radius: 16px;
	box-shadow:
		0 4px 12px rgba(24, 144, 255, 0.06),
		0 1px 4px rgba(0, 0, 0, 0.04);
	max-width: 100%;
	position: relative;
	border: 1px solid rgba(24, 144, 255, 0.1);
	overflow: hidden;
}

.info-section::before {
	content: '';
	position: absolute;
	top: 0;
	left: 0;
	right: 0;
	height: 3px;
	background: linear-gradient(90deg, #1890ff 0%, #36cfc9 100%);
	border-radius: 16px 16px 0 0;
}

.card-header {
	display: flex;
	align-items: center;
	justify-content: space-between;
	margin-bottom: 12px;
	padding-bottom: 10px;
	border-bottom: 1px solid rgba(0, 0, 0, 0.06);
}

.header-left {
	display: flex;
	align-items: center;
	gap: 8px;
}

.header-right {
	display: flex;
	align-items: center;
	gap: 4px;
}

.header-icon {
	font-size: 20px;
	line-height: 1;
}

.header-title {
	font-size: $font-size-body-md;
	font-weight: $font-weight-bold;
	color: $sull-text-main;
}

.manager-label {
	font-size: $font-size-body-xs;
	color: $sull-text-secondary;
}

.manager-value {
	font-size: $font-size-body-sm;
	font-weight: $font-weight-semibold;
	color: $sull-color-primary;
}

.info-grid {
	display: grid;
	grid-template-columns: 1fr 1fr;
	gap: 8px;
	align-content: center;
	height: 100%;
}

.info-swiper {
	height: 140px;
}

.info-item {
	display: flex;
	align-items: center;
	gap: 8px;
	padding: 8px;
	background: rgba(255, 255, 255, 0.6);
	border-radius: 10px;
	transition: all 0.3s ease;
	border: 1px solid rgba(0, 0, 0, 0.04);
}

.item-icon {
	width: 24px;
	height: 24px;
	display: flex;
	align-items: center;
	justify-content: center;
}

.bg-icon {
	width: 100%;
	height: 100%;
	background-repeat: no-repeat;
	background-position: center;
	background-size: contain;
}

.icon-money {
	background-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='%23faad14'%3E%3Cpath d='M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1.41 16.09V20h-2.67v-1.93c-1.71-.36-3.16-1.46-3.27-3.4h1.96c.1 1.05 1.06 1.91 2.57 1.91 1.74 0 2.5-.96 2.5-1.87 0-1.77-2.35-2.22-4.22-2.68-2.18-.51-3.66-1.57-3.66-3.65 0-1.78 1.4-3.18 3.12-3.53V3h2.67v1.93c1.71.36 3.16 1.46 3.27 3.4h-1.96c-.1-1.05-1.06-1.91-2.57-1.91-1.74 0-2.5.96-2.5 1.87 0 1.77 2.35 2.22 4.22 2.68 2.18.51 3.66 1.57 3.66 3.65 0 1.78-1.4 3.18-3.12 3.53z'/%3E%3C/svg%3E");
}

.info-item.warning {
	background: rgba(255, 241, 240, 0.6);
	border-color: rgba(255, 77, 79, 0.2);
}

.warning-text {
	color: #ff4d4f !important;
}

.value-unit-row {
	display: flex;
	align-items: baseline;
	gap: 2px;
}

.item-unit {
	font-size: 10px;
	color: $sull-text-secondary;
	font-weight: normal;
}

.item-unit.warning-text {
	color: #ff4d4f !important;
}

.stats-slide {
	display: flex;
	align-items: center;
	justify-content: space-between;
	height: 100%;
	padding: 0 10px 20px 10px;
}

.stats-left {
	display: flex;
	flex-direction: column;
	gap: 4px;
}

.stats-right {
	width: 192px;
}

.stats-title {
	font-size: 14px;
	color: $sull-text-main;
	font-weight: bold;
	margin-bottom: 20px;
}

.stats-value {
	font-size: 24px;
	font-weight: bold;
	color: $sull-text-main;
	line-height: 1.2;
}

.stats-trend {
	display: flex;
	align-items: center;
	gap: 6px;
	font-size: 10px;
}

.stats-trend.up {
	color: #52c41a;
}

.stats-trend.down {
	color: #ff4d4f;
}

.stats-trend.same {
	color: $sull-text-secondary;
}

.trend-label {
	color: $sull-text-secondary;
	opacity: 0.8;
}

/* Tasks Grid - Single Row Layout */
.tasks-grid {
	display: flex;
	gap: 8px;
	height: 100%;
	align-items: center;
}

/* Tasks Grid - 2x2 Layout */
.tasks-grid-2x2 {
	display: grid;
	grid-template-columns: 1fr 1fr;
	grid-template-rows: 1fr 1fr;
	gap: 8px;
	height: 100%;
	align-content: center;
}

.task-item {
	flex: 1;
	display: flex;
	flex-direction: column;
	align-items: center;
	gap: 6px;
	padding: 8px;
	background: rgba(255, 255, 255, 0.6);
	border-radius: 10px;
	border: 1px solid rgba(0, 0, 0, 0.04);
}

.task-icon {
	font-size: 20px;
	line-height: 1;
}

.task-content {
	display: flex;
	flex-direction: column;
	align-items: center;
	gap: 2px;
}

.task-label {
	font-size: 10px;
	color: $sull-text-secondary;
	text-align: center;
}

.task-value {
	font-size: $font-size-body-sm;
	font-weight: $font-weight-bold;
	color: $sull-color-primary;
	text-align: center;
}

/* Compact Task Item for 2x2 Grid */
.task-item-compact {
	display: flex;
	flex-direction: column;
	gap: 4px;
	padding: 8px;
	background: rgba(255, 255, 255, 0.6);
	border-radius: 10px;
	border: 1px solid rgba(0, 0, 0, 0.04);
}

.task-header {
	display: flex;
	align-items: center;
	gap: 4px;
	flex-wrap: nowrap;
}

.task-icon-small {
	font-size: 16px;
	line-height: 1;
	flex-shrink: 0;
}

.task-label-inline {
	font-size: 14px;
	color: $sull-text-main;
	font-weight: bold;
	flex-shrink: 1;
	min-width: 0;
}

.task-time-block {
	font-size: 10px;
	color: $sull-text-secondary;
	margin-left: 20px;
}

.task-times {
	display: flex;
	flex-direction: column;
	gap: 2px;
	margin-left: 20px;
}

.time-row {
	font-size: 10px;
	line-height: 1.4;
}

.time-label {
	color: $sull-text-secondary;
	margin-right: 4px;
}

.time-last {
	color: $sull-text-secondary;
}

.time-next {
	color: $sull-color-primary;
	font-weight: $font-weight-semibold;
}

.task-status {
	font-size: 10px;
	font-weight: $font-weight-semibold;
	text-align: center;
	padding: 2px 6px;
	border-radius: 4px;
}

.task-status.normal {
	color: #52c41a;
	background: rgba(82, 196, 26, 0.1);
}

.task-status.overdue {
	color: #ff4d4f;
	background: rgba(255, 77, 79, 0.1);
}

.task-status.open {
	color: #1890ff;
	background: rgba(24, 144, 255, 0.1);
}

.task-status.sealed {
	color: #8c8c8c;
	background: rgba(140, 140, 140, 0.1);
}

.task-time {
	font-size: 10px;
	color: $sull-text-secondary;
	text-align: center;
}

.info-item:active {
	background: rgba(24, 144, 255, 0.05);
	transform: scale(0.98);
}

.item-icon {
	font-size: 18px;
	line-height: 1;
	width: 32px;
	height: 32px;
	display: flex;
	align-items: center;
	justify-content: center;
	background: linear-gradient(135deg, #e6f7ff 0%, #f0f9ff 100%);
	border-radius: 8px;
	flex-shrink: 0;
}

.item-content {
	flex: 1;
	display: flex;
	flex-direction: column;
	gap: 2px;
	min-width: 0;
}

.item-label {
	font-size: 14px;
	color: $sull-text-main;
	font-weight: bold;
	white-space: nowrap;
	overflow: hidden;
	text-overflow: ellipsis;
}

.item-value {
	font-size: $font-size-body-sm;
	font-weight: $font-weight-bold;
	color: $sull-color-primary;
	white-space: nowrap;
	overflow: hidden;
	text-overflow: ellipsis;
}



/* Layers */
.layers-container {
	display: flex;
	flex-direction: column;
	gap: 20px;
	padding: 4px;
}

.layer-item {
	background: rgba(255, 255, 255, 0.65);
	backdrop-filter: blur(20px);
	-webkit-backdrop-filter: blur(20px);
	border: 1px solid rgba(255, 255, 255, 0.6);
	border-radius: 24px;
	box-shadow: 0 10px 40px rgba(0, 0, 0, 0.04);
	overflow: hidden;
	transition: all 0.3s ease;
}

.layer-header {
	padding: 10px 24px;
	display: flex;
	justify-content: space-between;
	align-items: center;
	background: linear-gradient(90deg, rgba(255, 255, 255, 0.5) 0%, rgba(255, 255, 255, 0) 100%);
	border-bottom: 1px solid rgba(255, 255, 255, 0.3);
}

.layer-title {
	font-weight: $font-weight-bold;
	color: $sull-text-main;
}

.layer-info {
	display: flex;
	align-items: center;
	gap: 8px;
}

.info-badge {
	display: flex;
	align-items: center;
	gap: 4px;
	background: rgba(255, 255, 255, 0.5);
	padding: 4px 10px;
	border-radius: 20px;
	border: 1px solid rgba(255, 255, 255, 0.8);
	transition: all 0.3s ease;
}

.info-badge.active {
	background: #e6f7ff;
	border-color: #1890ff;
	box-shadow: 0 0 8px rgba(24, 144, 255, 0.2);
}

.info-num {
	font-size: 14px;
	font-weight: bold;
	color: $sull-text-main;
}

.abnormal-icons {
	display: flex;
	align-items: center;
	gap: 6px;
	margin-left: 4px;
}

.icon-btn {
	padding: 4px;
	border-radius: 50%;
	transition: all 0.3s ease;
	display: flex;
	align-items: center;
	justify-content: center;
}

.icon-btn.active {
	background: rgba(255, 255, 255, 0.8);
	box-shadow: 0 0 8px rgba(0, 0, 0, 0.1);
	transform: scale(1.1);
}

.info-icon {
	font-size: 16px;
	line-height: 1;
}

.layer-content {
	padding: 12px;
	border-top: none;
	max-width: 100%;
}

/* List View */
.list-card-wrapper {
	margin-bottom: 8px;
}

.list-card-wrapper:last-child {
	margin-bottom: 0px;
}

/* List View - Card Style */
.list-card {
	background: rgba(255, 255, 255, 0.8);
	border-radius: 12px;
	padding: 8px 16px;
	box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
	border: 1px solid rgba(255, 255, 255, 0.6);
}

.card-row {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-bottom: 8px;
}

.top-row {
	margin-bottom: 4px;
}



.item-name-wrapper {
	width: 100%;
	height: 24px;
	overflow: hidden;
	white-space: nowrap;
	margin-bottom: 4px;
	position: relative;
}

.item-name-text {
	display: block;
	font-size: 16px;
	font-weight: bold;
	color: $sull-text-main;
	white-space: nowrap;
	overflow: hidden;
	text-overflow: ellipsis;
	width: 100%;
}

.item-name-text.scrolling {
	display: inline-block;
	width: auto;
	padding-left: 100%;
	animation: marquee 8s linear infinite;
	text-overflow: clip;
}

@keyframes marquee {
	0% {
		transform: translateX(0);
	}

	100% {
		transform: translateX(-100%);
	}
}

.empty-card {
	background: #F5F7FA;
	border: 1px dashed #dcdfe6;
	border-radius: 12px;
	padding: 8px 16px;
	height: 32px;
	position: relative;
	display: flex;
	align-items: center;
}

.empty-index {
	font-size: 16px;
	font-weight: bold;
	color: #C0C4CC;
}

.empty-icon-center {
	position: absolute;
	top: 0;
	left: 0;
	width: 100%;
	height: 100%;
	display: flex;
	justify-content: center;
	align-items: center;
}

.empty-icon {
	width: 24px;
	height: 24px;
	background-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='%23C0C4CC' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Cpath d='M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z'%3E%3C/path%3E%3Cpolyline points='3.27 6.96 12 12.01 20.73 6.96'%3E%3C/polyline%3E%3Cline x1='12' y1='22.08' x2='12' y2='12'%3E%3C/line%3E%3C/svg%3E");
	background-size: contain;
	background-repeat: no-repeat;
	/* Remove text content in template */
}

.status-icon {
	font-size: 18px;
	color: #52c41a;
}

.status-icon.icon-warn {
	color: #faad14;
}

.status-icon.shaking {
	color: #000000;
	animation: bell-shake 1.2s infinite ease-in-out;
}

.middle-row {
	margin-bottom: 12px;
}

.spec-qty-container {
	display: flex;
	align-items: center;
	gap: 8px;
}

.item-spec-text {
	font-size: 13px;
	color: $sull-text-secondary;
	background: rgba(0, 0, 0, 0.04);
	padding: 2px 8px;
	border-radius: 4px;
}

.item-qty-text {
	font-size: 14px;
	color: $sull-text-secondary;
}

.days-info {
	display: flex;
	align-items: center;
	gap: 4px;
}

.days-badge {
	background: #333;
	border-radius: 4px;
	padding: 2px 6px;
	min-width: 30px;
	text-align: center;
	display: flex;
	align-items: center;
	justify-content: center;
	gap: 2px;
}

.days-badge.badge-normal {
	background: #ffffff;
	border: 1px solid #dcdfe6;
}

.days-badge.badge-normal .days-num {
	color: #303133;
}

.days-badge.badge-danger {
	background: #ff4d4f;
}

.days-badge.badge-warning {
	background: #faad14;
}

.days-badge.badge-expired {
	background: #000000;
}

.days-prefix {
	font-size: 12px;
	color: #fff;
	margin-right: 2px;
}

.days-num {
	color: #fff;
	font-size: 14px;
	font-weight: bold;
}

.days-label {
	font-size: 14px;
	color: $sull-text-secondary;
}

.bottom-row {
	margin-bottom: 0;
}

.progress-container {
	width: 100%;
	height: 10px;
	background: rgba(103, 194, 58, 0.3);
	border-radius: 5px;
	overflow: hidden;
	position: relative;
}

.progress-green {
	height: 100%;
	background-color: #67C23A;
	transition: width 0.3s ease;
	position: absolute;
	right: 0;
	top: 0;
	transform: skewX(-45deg);
	transform-origin: bottom right;
}



.progress-container.shimmer::after {
	content: '';
	position: absolute;
	top: 0;
	left: 0;
	width: 100%;
	height: 100%;
	background: linear-gradient(90deg, rgba(255, 255, 255, 0) 0%, rgba(255, 255, 255, 0.6) 50%, rgba(255, 255, 255, 0) 100%);
	transform: translateX(-100%);
	animation: shimmer 1.5s infinite;
	z-index: 2;
}

@keyframes shimmer {
	100% {
		transform: translateX(100%);
	}
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

.days-text {
	font-size: 10px;
	color: $sull-text-secondary;
	font-weight: 500;
}

.text-danger {
	color: #ff4d4f;
}

.text-warning {
	color: #faad14;
}



/* Bottom Navigation */
.bottom-nav {
	position: fixed;
	bottom: 0;
	left: 0;
	width: 100%;
	height: 90px;
	background: $sull-color-white;
	display: flex;
	justify-content: space-around;
	align-items: center;
	padding-bottom: calc(20px + constant(safe-area-inset-bottom));
	padding-bottom: calc(20px + env(safe-area-inset-bottom));
	box-shadow: 0 -4px 20px rgba(0, 0, 0, 0.05);
	border-radius: 24px 24px 0 0;
}

.nav-item {
	display: flex;
	flex-direction: column;
	align-items: center;
	width: 60px;
}

.nav-icon {
	width: 24px;
	height: 24px;
	margin-bottom: 4px;
	background-color: $sull-gray-50;
	-webkit-mask-size: contain;
	-webkit-mask-repeat: no-repeat;
	-webkit-mask-position: center;
	mask-size: contain;
	mask-repeat: no-repeat;
	mask-position: center;
}

.nav-label {
	font-size: $font-size-body-xs;
	color: $sull-gray-50;
}

.nav-item.active .nav-icon {
	background-color: $sull-color-primary;
}

.nav-item.active .nav-label {
	color: $sull-color-primary;
}

.center-item {
	position: relative;
	top: -30px;
	display: flex;
	flex-direction: column;
	align-items: center;
}

.center-btn {
	width: 60px;
	height: 60px;
	background: $sull-color-primary;
	border-radius: $radius-circle;
	display: flex;
	align-items: center;
	justify-content: center;
	box-shadow: 0 10px 20px rgba(149, 202, 255, 0.4);
	border: 4px solid $sull-color-white;
	margin-bottom: 4px;
}

.center-icon {
	width: 32px;
	height: 32px;
	background-color: #fff;
	-webkit-mask-size: contain;
	-webkit-mask-repeat: no-repeat;
	-webkit-mask-position: center;
	mask-size: contain;
	mask-repeat: no-repeat;
	mask-position: center;
}

/* Navigation Icons */
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
</style>

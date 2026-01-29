<template>
	<view class="container">
		<scroll-view scroll-y class="main-scroll" refresher-enabled="true" :refresher-triggered="isRefreshing"
			:refresher-threshold="60" @refresherrefresh="onRefresh" @refresherpulling="onPulling"
			@refresherrestore="onRestore" @refresherabort="onRestore" refresher-default-style="none"
			refresher-background="#f0f5ff">
			<CustomRefresher slot="refresher" :isRefreshing="isRefreshing" :isPulling="isPulling" />
			<!-- Header -->
			<view class="header">
				<text class="title">{{ $t('bigScreen.rescueCar.title') }}</text>
			</view>

			<view class="scroll-content">
				<view class="info-section">
					<view class="card-header">
						<view class="header-left" @click="openStatusModal">
							<view class="header-icon">🚑</view>
							<text class="header-title">{{ rescueCar.DisplayName || rescueCar.Name || '' }}</text>
							<text class="status-tag" :class="statusConfig.class">
								{{ statusConfig.text }}
							</text>
						</view>
						<view class="header-right">
							<text class="manager-label">{{ $t('bigScreen.rescueCar.manager') }}:</text>
							<text class="manager-value">{{ rescueCar.DutyUser || '未指定' }}</text>
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
						<!-- <swiper-item>
							<view class="tasks-grid-2x2">
								<view class="task-item-compact">
									<view class="task-header">
										<view class="task-icon-small">📋</view>
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
										<view class="task-icon-small">✅</view>
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
						</swiper-item> -->

						<!-- Slide 3: Handover Stats -->
						<!-- <swiper-item>
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
						</swiper-item> -->

						<!-- Slide 4: Expiry Stats -->
						<!-- <swiper-item>
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
						</swiper-item> -->

						<!-- Slide 5: Cleaning Stats -->
						<!-- <swiper-item>
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
						</swiper-item> -->

						<!-- Slide 6: Abnormality Stats -->
						<!-- <swiper-item>
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
						</swiper-item> -->

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
											<text class="info-icon">⚠️</text>
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
														:class="{ 'scrolling': $getStringLen(item.name) > 20 }">{{ i + 1
														}}. {{
															item.name }}</text>
												</view>
												<view v-if="item.days <= 30" class="status-icon"
													:class="{ 'icon-warn': item.days < 30 && item.days >= 0, 'shaking': item.days < 0 }">
													⚠️</view>
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
													:style="{ background: getProgressGradient(item.days) }">
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

		<!-- Status Selection Modal (Handover Style) -->
		<view class="modal-overlay" v-if="showStatusModal" @click.stop="doNothing" @touchmove.stop.prevent="doNothing">
			<view class="modal-content" @click.stop="doNothing">
				<view class="modal-header">
					<text class="modal-title">{{ $t('bigScreen.rescueCar.statusTitle') || 'Update Status' }}</text>
					<text class="close-btn" @click="closeStatusModal">✖️</text>
				</view>
				<view class="modal-body">
					<!-- Status Select -->
					<view class="form-item">
						<text class="label">状态</text>
						<view class="status-selector">
							<view v-for="(opt, i) in statusOptions" :key="i" class="status-chip"
								:class="{ active: statusForm.status === opt.value }" @click="selectStatus(opt)">
								{{ opt.label }}
							</view>
						</view>
					</view>
					<!-- Remark -->
					<view class="form-item">
						<text class="label">备注</text>
						<textarea class="remark-textarea" v-model="statusForm.remark"
							placeholder="请输入备注信息..."></textarea>
					</view>
				</view>
				<view class="modal-footer">
					<button class="modal-btn cancel" @click="closeStatusModal">取消</button>
					<button class="modal-btn confirm" @click="submitStatusUpdate">确定</button>
				</view>
			</view>
		</view>
	</view>
</template>

<script>
import FooterInfo from '@/components/FooterInfo/FooterInfo.vue';
import SimpleLineChart from '@/components/SimpleLineChart/SimpleLineChart.vue';
import request from '@/utils/request.js';
import { storage } from '@/utils/storage.js';
import { getRescueCarStatusText } from '@/utils/rescueCarUtils.js';
import CustomRefresher from '@/components/CustomRefresher/CustomRefresher.vue';
import dayjs from 'dayjs';

export default {
	components: {
		FooterInfo,
		SimpleLineChart,
		CustomRefresher
	},
	data() {
		return {
			userOrgInfo: storage.get('userOrgInfo') || {},
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
			initialFilterType: null,
			isRefreshing: false,
			isPulling: false,
			showStatusModal: false,
			statusOptions: [],
			statusForm: {
				status: null,
				remark: ''
			}
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
		if (options && options.type) {
			this.initialFilterType = parseInt(options.type);
		}
		if (options && options.id) {
			this.selectedRescueCarId = options.id;
		}
		this.fetchData();
	},
	computed: {
		statusConfig() {
			const status = this.rescueCar.Status || 0;
			const map = {
				0: { class: 'tag-normal', text: this.$t('bigScreen.rescueCar.statusNormal') || '正常' },
				10: { class: 'tag-abnormal', text: this.$t('bigScreen.rescueCar.statusAbnormal') || '异常' },
				11: { class: 'tag-confirming', text: this.$t('bigScreen.rescueCar.statusConfirming') || '确认中' },
				20: { class: 'tag-dirty', text: this.$t('bigScreen.rescueCar.statusDirty') || '脏污' },
				21: { class: 'tag-cleaning', text: this.$t('bigScreen.rescueCar.statusCleaning') || '清洁中' },
				30: { class: 'tag-repair', text: this.$t('bigScreen.rescueCar.statusRepair') || '报修' },
				31: { class: 'tag-fixing', text: this.$t('bigScreen.rescueCar.statusFixing') || '维修中' }
			};
			return map[status] || map[0];
		}
	},
	methods: {
		onPulling() {
			this.isPulling = true;
		},
		onRestore() {
			this.isPulling = false;
		},
		onRefresh() {
			this.isRefreshing = true;
			this.isPulling = true;
			this.fetchData().then(() => {
				setTimeout(() => {
					this.isRefreshing = false;
					this.isPulling = false;
					uni.showToast({ title: '已刷新', icon: 'none' });
				}, 800);
			});
		},
		async fetchData() {
			await this.fetchRescueCars();
		},
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
					const lastId = this.selectedRescueCarId || storage.get('lastSelectedRescueCarId');
					let targetCar = cars[0];
					if (lastId) {
						// Use loose comparison to handle potential string/number mismatch
						const found = cars.find(c => c.Id == lastId);
						if (found) targetCar = found;
					}
					this.rescueCar = targetCar;
					this.selectedRescueCarId = this.rescueCar.Id;

					// Ensure storage is synced with the current selection
					storage.set('lastSelectedRescueCarId', this.selectedRescueCarId);
					storage.set('currentRescueCarInfo', {
						Id: this.rescueCar.Id,
						Name: this.rescueCar.DisplayName || this.rescueCar.Name
					});

					this.fetchDashboardStats(this.selectedRescueCarId);
					// Combined fetch for floors and serials
					this.fetchRescueCarPositions(this.selectedRescueCarId);
				} else {
					this.rescueCar = {};
					this.selectedRescueCarId = null;
					this.layers = [];
				}
			} catch (e) {
				console.error(e);
			}
		},
		async fetchRescueCarPositions(warehouseId) {
			if (!warehouseId) return;
			try {
				const res = await request.post('api/ssbh/Position/GetPageData', {
					page: 1, rows: 1000,
					wheres: JSON.stringify([
						{ name: 'WarehouseId', value: warehouseId },
						{ name: 'Category', value: '100,110', DisplayType: 'in' },
						{ name: 'OrganizationId', value: this.userOrgInfo.Organization.Id },
						{ name: 'DepartmentId', value: this.userOrgInfo.Department.Id }
					]),
					sort: 'OrderBy,SerialId', order: 'asc'
				});
				const allItems = res.rows || [];
				const floors = allItems.filter(p => p.Category === 100);
				const serials = allItems.filter(p => p.Category === 110);

				if (floors.length > 0) {
					const itemIds = serials.filter(s => s.ItemId > 0).map(s => s.ItemId);
					if (itemIds.length > 0) {
						await this.fetchItemDetails([...new Set(itemIds)]);
					}
					this.transformToLayers(floors, serials);
				} else {
					this.layers = [];
				}
			} catch (e) {
				console.error("Fetch Rescue Car Positions Error", e);
			}
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
			const distinctIds = ids.filter(id => !this.itemsMap[id]);
			if (distinctIds.length === 0) return;

			try {
				const res = await request.post('api/ssbh/Item/GetPageData', {
					page: 1, rows: 500,
					wheres: JSON.stringify([
						{ name: 'Id', value: distinctIds.join(','), DisplayType: 'in' },
						// Secure fetch with Org/Dept
						{ name: 'OrganizationId', value: this.userOrgInfo.Organization.Id },
						{ name: 'DepartmentId', value: this.userOrgInfo.Department.Id }
					])
				});

				if (res.rows && res.rows.length > 0) {
					res.rows.forEach(item => {
						this.$set(this.itemsMap, item.Id, item);
					});
				}
			} catch (e) {
				console.error("Batch fetch item details failed", e);
			}
		},
		getDaysUntil(dateStr) {
			if (!dateStr) return '999';
			const now = new Date();
			const target = dayjs(dateStr).toDate();
			return Math.ceil((target.getTime() - now.getTime()) / (1000 * 3600 * 24));
		},
		transformToLayers(floors, serials) {
			this.layers = floors.map(floor => {
				const floorSerials = serials.filter(s => s.FloorId === floor.Id);
				// Sort by SerialId as requested
				floorSerials.sort((a, b) => (a.SerialId || 0) - (b.SerialId || 0));

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
			if (page === 'statistics') url = '/pages_user/statistics/statistics';
			if (page === 'profile') url = '/pages/profile/profile';

			if (url) {
				uni.reLaunch({ url });
			}
		},
		toggleLayer(index) {
			this.layers[index].expanded = !this.layers[index].expanded;
		},
		getDangerProgress(days) {
			if (days < 0) return 100;
			if (days >= 90) return 0;
			return ((90 - days) / 90) * 100;
		},
		getProgressGradient(days) {
			if (days < 0) return '#000000';
			if (days >= 90) return '#67C23A';
			const p = this.getDangerProgress(days);
			return `linear-gradient(110deg, #F56C6C ${p}%, #67C23A ${p}%)`;
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
		},
		getStatusText(status) {
			return getRescueCarStatusText(status, this.$t);
		},
		openStatusModal() {
			if (!this.rescueCar || !this.rescueCar.Id) return;

			const currentStatus = this.rescueCar.Status || 0;
			let options = [];

			// Logic based on current status
			if (currentStatus === 0) {
				options = [
					{ label: this.$t('bigScreen.rescueCar.statusAbnormal') || '异常', value: 10 },
					{ label: this.$t('bigScreen.rescueCar.statusDirty') || '脏污', value: 20 },
					{ label: this.$t('bigScreen.rescueCar.statusRepair') || '报修', value: 30 }
				];
			} else if (currentStatus === 10) {
				options = [
					{ label: this.$t('bigScreen.rescueCar.statusConfirming') || '确认中', value: 11 },
					{ label: this.$t('bigScreen.rescueCar.statusNormal') || '正常', value: 0 }
				];
			} else if (currentStatus === 20) {
				options = [
					{ label: this.$t('bigScreen.rescueCar.statusCleaning') || '清洁中', value: 21 },
					{ label: this.$t('bigScreen.rescueCar.statusNormal') || '正常', value: 0 }
				];
			} else if (currentStatus === 30) {
				options = [
					{ label: this.$t('bigScreen.rescueCar.statusFixing') || '维修中', value: 31 },
					{ label: this.$t('bigScreen.rescueCar.statusNormal') || '正常', value: 0 }
				];
			} else if ([11, 21, 31].includes(currentStatus)) {
				options = [
					{ label: this.$t('bigScreen.rescueCar.statusNormal') || '正常', value: 0 }
				];
			} else {
				options = [
					{ label: this.$t('bigScreen.rescueCar.statusNormal') || '正常', value: 0 }
				];
			}

			this.statusOptions = options;
			this.statusForm.status = options.length > 0 ? options[0].value : null;
			this.statusForm.remark = '';
			this.showStatusModal = true;
		},
		closeStatusModal() {
			this.showStatusModal = false;
		},
		selectStatus(option) {
			this.statusForm.status = option.value;
		},
		submitStatusUpdate() {
			if (this.statusForm.status === null) {
				uni.showToast({ title: '请选择状态', icon: 'none' });
				return;
			}
			this.updateStatus(this.statusForm.status, this.statusForm.remark);
		},
		updateStatus(newStatus, remark) {
			uni.showLoading({ title: 'Updating...' });
			const payload = {
				mainData: {
					OrganizationId: this.userOrgInfo.Organization.Id,
					DepartmentId: this.userOrgInfo.Department.Id,
					ItemId: this.rescueCar.Id,
					Status: newStatus,
					Remark: remark,
					Category: 20 // Rescue Car Type
				}
			};

			request.post('api/ssbh/ItemStatusHistory/Add', payload).then(res => {
				uni.hideLoading();
				if (res.success || res.status) {
					uni.showToast({ title: '成功' });
					this.rescueCar.Status = newStatus;
					this.closeStatusModal();
					this.fetchDashboardStats(this.rescueCar.Id); // Refresh stats too
				} else {
					uni.showToast({ title: res.msg || '失败', icon: 'none' });
				}
			}).catch(err => {
				uni.hideLoading();
				console.error(err);
				uni.showToast({ title: 'Error', icon: 'none' });
			});
		},
		doNothing() { }
	}
}
</script>

<style lang="scss" scoped>
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
	background: linear-gradient(135deg, $sull-color-primary 0%, lighten($sull-color-primary, 10%) 100%);

	.title {
		color: #fff;
		font-size: 36rpx;
		font-weight: bold;
	}
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

.status-tag {
	padding: 4rpx 12rpx;
	border-radius: 40rpx;
	font-size: 20rpx;
	font-weight: 600;
}

/* Status-Based Colors */
/* Normal #2E7D32 */
.tag-normal {
	background: rgba(46, 125, 50, 0.1);
	color: #2E7D32;
	border: 1px solid rgba(46, 125, 50, 0.2);
}

/* Confirming #F9A825 */
.tag-confirming {
	background: rgba(249, 168, 37, 0.1);
	color: #F9A825;
	border: 1px solid rgba(249, 168, 37, 0.2);
}

/* Dirty #C0A16B */
.tag-dirty {
	background: rgba(192, 161, 107, 0.1);
	color: #C0A16B;
	border: 1px solid rgba(192, 161, 107, 0.2);
}

/* Abnormal #C62828 */
.tag-abnormal {
	background: rgba(198, 40, 40, 0.1);
	color: #C62828;
	border: 1px solid rgba(198, 40, 40, 0.2);
}

/* Repair #B71C1C */
.tag-repair {
	background: rgba(183, 28, 28, 0.1);
	color: #B71C1C;
	border: 1px solid rgba(183, 28, 28, 0.2);
}

/* Cleaning #1976D2 */
.tag-cleaning {
	background: rgba(25, 118, 210, 0.1);
	color: #1976D2;
	border: 1px solid rgba(25, 118, 210, 0.2);
}

/* Fixing #546E7A */
.tag-fixing {
	background: rgba(84, 110, 122, 0.1);
	color: #546E7A;
	border: 1px solid rgba(84, 110, 122, 0.2);
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
	font-size: 30rpx;
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
	font-size: 32rpx;
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
	flex: 1;
	height: 32px;
	overflow: hidden;
	white-space: nowrap;
	margin-bottom: 4px;
	position: relative;
	display: flex;
	align-items: center;
}

.item-name-text {
	display: inline-block;
	font-size: 32rpx;
	line-height: 32px;
	font-weight: bold;
	color: $sull-text-main;
	white-space: nowrap;
	overflow: hidden;
	text-overflow: ellipsis;
	width: 100%;
	padding-left: 8rpx;
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

.status-tag {
	font-size: 24rpx;
	padding: 4rpx 12rpx;
	border-radius: 8rpx;
	margin-left: 16rpx;
	font-weight: normal;

	&.status-0 {
		background: rgba(82, 196, 26, 0.1);
		color: #52c41a;
		border: 1px solid rgba(82, 196, 26, 0.2);
	}

	&.status-10 {
		background: rgba(245, 34, 45, 0.1);
		color: #f5222d;
		border: 1px solid rgba(245, 34, 45, 0.2);
	}

	&.status-11 {
		background: rgba(250, 173, 20, 0.1);
		color: #faad14;
		border: 1px solid rgba(250, 173, 20, 0.2);
	}

	&.status-20 {
		background: rgba(114, 46, 209, 0.1);
		color: #722ed1;
		border: 1px solid rgba(114, 46, 209, 0.2);
	}

	&.status-21 {
		background: rgba(19, 194, 194, 0.1);
		color: #13c2c2;
		border: 1px solid rgba(19, 194, 194, 0.2);
	}

	&.status-30 {
		background: rgba(235, 47, 150, 0.1);
		color: #eb2f96;
		border: 1px solid rgba(235, 47, 150, 0.2);
	}

	&.status-31 {
		background: rgba(24, 144, 255, 0.1);
		color: #1890ff;
		border: 1px solid rgba(24, 144, 255, 0.2);
	}
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

/* Modal Styles */
.member-overlay {
	position: fixed;
	top: 0;
	left: 0;
	width: 100%;
	height: 100%;
	background: rgba(0, 0, 0, 0.5);
	z-index: 3000;
	display: flex;
	align-items: flex-end;
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

	.modal-title {
		font-size: 32rpx;
		font-weight: bold;
		color: $sull-text-main;
	}

	.close-btn {
		font-size: 36rpx;
		color: $sull-text-secondary;
		padding: 8rpx;
	}
}

.member-list {
	flex: 1;
	height: 0;
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

@keyframes slideUp {
	from {
		transform: translateY(100%);
	}

	to {
		transform: translateY(0);
	}
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
	display: none;
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

/* Modal Base - Same as handover.vue for consistency */
.modal-overlay {
	position: fixed;
	top: 0;
	left: 0;
	right: 0;
	bottom: 0;
	background: rgba(0, 0, 0, 0.6);
	backdrop-filter: blur(5px);
	z-index: 1000;
	display: flex;
	align-items: center;
	justify-content: center;
	padding: 40rpx;
}

.modal-content {
	width: 100%;
	background: #fff;
	border-radius: 40rpx;
	overflow: hidden;
	animation: modal-zoom 0.3s cubic-bezier(0.18, 0.89, 0.32, 1.28);
	box-shadow: 0 20px 60px rgba(0, 0, 0, 0.2);
}

@keyframes modal-zoom {
	from {
		transform: scale(0.8);
		opacity: 0;
	}

	to {
		transform: scale(1);
		opacity: 1;
	}
}

.modal-header {
	padding: 40rpx;
	display: flex;
	justify-content: space-between;
	align-items: center;
	border-bottom: 2rpx solid #f5f5f5;
}

.modal-title {
	font-size: 34rpx;
	font-weight: bold;
	color: #333;
}

.close-btn {
	font-size: 40rpx;
	color: #999;
	padding: 10rpx;
}

.modal-body {
	padding: 40rpx;
}

.form-item {
	margin-bottom: 40rpx;

	&:last-child {
		margin-bottom: 0;
	}

	.label {
		display: block;
		font-size: 28rpx;
		color: #999;
		margin-bottom: 20rpx;
		font-weight: 500;
	}

	.value {
		font-size: 30rpx;
		color: #333;
		font-weight: bold;
		min-height: 48rpx;
		display: flex;
		align-items: center;

		&.disabled {
			background: #f9f9f9;
			padding: 24rpx 32rpx;
			border-radius: 16rpx;
			border: 2rpx solid #f0f0f0;
		}

		&.text-placeholder {
			color: #ccc;
		}
	}
}

/* Status Selector */
.status-selector {
	display: flex;
	flex-wrap: wrap;
	gap: 20rpx;
}

.status-chip {
	padding: 16rpx 32rpx;
	border-radius: 40rpx;
	background: #f5f5f5;
	font-size: 26rpx;
	color: #666;
	border: 2rpx solid #f0f0f0;
	transition: all 0.2s;
}

.status-chip.active {
	background: #e6f7ff;
	color: #1890ff;
	border-color: #91d5ff;
	font-weight: bold;
}

/* Textarea */
.remark-textarea {
	width: 100%;
	height: 160rpx;
	background: #f9f9f9;
	border-radius: 16rpx;
	padding: 24rpx;
	box-sizing: border-box;
	font-size: 28rpx;
	color: #333;
	border: 2rpx solid #f0f0f0;
}

.modal-footer {
	padding: 0 40rpx 40rpx;
	display: flex;
	gap: 24rpx;
}

.modal-btn {
	flex: 1;
	height: 90rpx;
	border-radius: 45rpx;
	display: flex;
	align-items: center;
	justify-content: center;
	font-size: 30rpx;
	font-weight: bold;

	&::after {
		border: none;
	}

	&.cancel {
		background: #f5f5f5;
		color: #999;
	}

	&.confirm {
		background: linear-gradient(90deg, #1890ff 0%, #36cfc9 100%);
		color: #fff;
		box-shadow: 0 10rpx 20rpx rgba(24, 144, 255, 0.2);
	}
}
</style>

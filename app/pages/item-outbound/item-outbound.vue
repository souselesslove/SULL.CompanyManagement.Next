<template>
	<view class="container">
		<scroll-view class="main-content" scroll-y :enable-back-to-top="true" :refresher-enabled="true"
			:refresher-triggered="isRefreshing" :refresher-threshold="60" @refresherrefresh="onRefresh"
			@refresherpulling="onPulling" @refresherrestore="onRestore" @refresherabort="onRestore" @scroll="onScroll"
			:scroll-top="scrollTopVal" refresher-default-style="none" refresher-background="#f0f5ff">

			<view slot="refresher" class="custom-refresher"
				:style="{ transform: (isPulling || isRefreshing) ? 'translateY(120rpx)' : 'translateY(0)' }">
				<view class="refresher-content">
					<view class="refresher-icon" :class="{ 'animating': isRefreshing }"></view>
					<text class="refresher-text">{{ isRefreshing ? $t('index.refreshing') : $t('index.pullToRefresh')
					}}</text>
				</view>
			</view>

			<!-- Header Gradient Section matching Position Manage -->
			<view class="header-section" :style="{ paddingTop: statusBarHeight * 2 + 'rpx' }">
				<view class="nav-bar">
					<BackButton @click="goBack" />
					<text class="page-title">{{ $t('itemOutbound.title') }}</text>
				</view>
				<view class="header-content">

					<!-- Search Row -->
					<view class="search-row">
						<view class="search-box">
							<text class="search-icon">🔍</text>
							<input class="search-input" v-model="searchText" confirm-type="search" @confirm="onSearch"
								placeholder="搜索物品或位置" />
							<view class="clear-icon" v-if="searchText" @click="clearSearch">
								<text>✖</text>
							</view>
						</view>
						<view class="scan-btn" @click="onScan">
							<view class="scan-icon"></view>
						</view>
					</view>
				</view>
			</view>

			<!-- Main Content Panel -->
			<view class="content-panel">
				<!-- Rescue Car Switcher -->
				<view class="horizontal-section" v-if="rescueCarList.length > 0">
					<scroll-view scroll-x class="horizontal-scroll" :show-scrollbar="false"
						:scroll-into-view="scrollIntoViewId" scroll-with-animation>
						<view class="scroll-inner">
							<view class="h-card" v-for="(item, index) in rescueCarList" :key="item.Id"
								:id="'car-' + item.Id" :class="{ active: selectedRescueCarId === item.Id }"
								@click="selectRescueCar(item.Id)">
								<text class="card-name">{{ item.DisplayName }}</text>
							</view>
						</view>
					</scroll-view>
				</view>
				<view class="empty-placeholder" v-else>
					暂无抢救车数据
				</view>

				<!-- Floor List (Refactored) -->
				<StorageLayers :floorList="floorList" :serialsMap="floorSerialsMap" :itemsMap="itemsMap"
					:highlightedSerialIds="highlightedSerialIds" @toggle-floor="toggleFloor"
					@serial-click="onSerialClick" />
				<view style="height: 80rpx;"></view>
			</view>

			<view class="bottom-spacer"></view>
			<FooterInfo />
		</scroll-view>

		<BackToTop :scroll-top="scrollTop" @click="goTop" />

		<!-- Outbound Modal -->
		<view class="modal-mask" v-if="showModal" @click="closeModal">
			<view class="modal-content" @click.stop>
				<view class="modal-header">
					<view class="title-section">
						<text class="modal-title">{{ selectedItem?.Name }} - 出库</text>
					</view>
					<view class="close-btn" @click="closeModal">✕</view>
				</view>

				<view class="modal-body">
					<!-- Batch Selection -->
					<view class="form-group" v-if="itemDetails.length > 0">
						<text class="form-label">{{ $t('itemOutbound.batch_select') }}</text>
						<scroll-view scroll-x class="batch-scroll">
							<view class="batch-list-h">
								<view class="batch-item" v-for="(batch, idx) in itemDetails" :key="idx"
									:class="{ active: selectedBatchId === batch.Id, disabled: batch.Stock <= 0, 'stock-warning': (!batch.Stock || batch.Stock <= 0), 'stock-insufficient': (batch.Stock > 0 && batch.Stock < (selectedItem.Cardinality || 0)) }"
									@click="batch.Stock > 0 && selectBatch(batch)">
									<view class="batch-row">
										<text class="batch-no">批号: {{ batch.RegisterCode || '-' }}</text>
									</view>
									<view class="batch-row">
										<text class="batch-expiry">效期: {{ formatDate(batch.ExpirationDate) }}</text>
									</view>
									<view class="batch-row">
										<text class="batch-stock"
											:class="{ 'stock-zero': (!batch.Stock || batch.Stock <= 0), 'stock-insufficient-text': (batch.Stock > 0 && batch.Stock < (selectedItem.Cardinality || 0)) }">库存:
											{{ batch.Stock }}</text>
									</view>
								</view>
							</view>
						</scroll-view>
					</view>

					<!-- Quantity Display -->
					<!-- Quantity Control Row -->
					<view class="qty-control-row">
						<view class="q-btn-circle sub" @click.stop="quickAction('sub')">-1</view>

						<view class="qty-value-display" :class="{ error: outQty > maxQty }">
							<text class="stock-warning-label" v-if="maxQty <= 0">库存不足</text>
							<text class="label" v-else>出库数量</text>
							<view class="qty-val-group">
								<text class="qty-text">{{ outQty }}</text>
								<text class="unit-text">{{ selectedItem?.Unit }}</text>
							</view>
							<text class="stock-hint">余: {{ maxQty }}</text>
						</view>

						<view class="q-btn-circle add" @click.stop="quickAction('add')">+1</view>
					</view>

					<!-- Keypad -->
					<view class="keypad-wrapper">
						<NumericKeypad @input="onKeypadInput" @delete="onKeypadDelete" />
					</view>

					<!-- Clear Button (Full Width) -->
					<view class="clear-action-bar">
						<view class="q-btn-full clear" @click.stop="quickAction('clear')">清空</view>
					</view>

				</view>

				<view class="modal-footer">
					<button class="submit-btn" :disabled="!canSubmit" @click="submitOutbound">
						{{ $t('itemOutbound.confirm') }}
					</button>
				</view>
			</view>
		</view>
	</view>
</template>

<script>
import BackButton from '@/components/BackButton/BackButton.vue';
import FooterInfo from '@/components/FooterInfo/FooterInfo.vue';
import BackToTop from '@/components/BackToTop/BackToTop.vue';
import NumericKeypad from '@/components/NumericKeypad/NumericKeypad.vue';
import StorageLayers from '@/components/StorageLayers/StorageLayers.vue';
import request from '@/utils/request.js';
import { storage } from '@/utils/storage.js';
import dayjs from 'dayjs';

export default {
	components: { BackButton, FooterInfo, BackToTop, NumericKeypad, StorageLayers },
	data() {
		return {
			statusBarHeight: 20,
			scrollTop: 0,
			oldScrollTop: 0,
			isRefreshing: false,
			isPulling: false,
			searchText: '',
			scrollTopVal: 0,

			// Data Lists (Same as Position Manage)
			rescueCarList: [],
			floorList: [],
			serialList: [],
			selectedRescueCarId: null,
			itemsMap: {},
			userOrgInfo: {},

			// Search Highlight
			highlightedSerialIds: [], // Array of IDs
			scrollIntoViewId: '',

			// Outbound Modal Data
			showModal: false,
			selectedItem: null,
			itemDetails: [], // Batches
			selectedBatchId: null,
			selectedBatch: null,
			outQtyInput: '1', // String for keypad input
			remark: ''
		};
	},
	computed: {
		outQty() {
			return parseInt(this.outQtyInput) || 0;
		},
		maxQty() {
			let stock = 0;
			if (this.selectedBatch) {
				stock = this.selectedBatch.Stock;
			} else if (this.selectedItem) {
				stock = this.selectedItem.AvailableStock;
			}
			return parseInt(stock) || 0;
		},
		canSubmit() {
			return this.selectedBatchId && this.outQty > 0 && this.outQty <= this.maxQty;
		},
		floorSerialsMap() {
			const map = {};
			this.floorList.forEach(f => map[f.Id] = []);
			this.serialList.forEach(s => {
				if (!map[s.FloorId]) map[s.FloorId] = [];
				map[s.FloorId].push(s);
			});
			return map;
		}
	},
	onLoad() {
		const sysInfo = uni.getSystemInfoSync();
		this.statusBarHeight = sysInfo.statusBarHeight || 20;
		this.userOrgInfo = storage.get('userOrgInfo') || {};
		this.fetchData();
	},
	methods: {
		goBack() { uni.navigateBack(); },
		goTop() {
			this.scrollTopVal = this.scrollTop;
			this.$nextTick(() => {
				this.scrollTopVal = 0;
			});
		},
		onScroll(e) {
			this.scrollTop = e.detail.scrollTop;
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
			this.fetchData().then(() => {
				setTimeout(() => {
					this.isRefreshing = false;
					this.isPulling = false;
					uni.showToast({ title: '已刷新', icon: 'none' });
				}, 800);
			});
		},

		// Data Fetching Logic (Mirrors Position Manage)
		async fetchData() {
			await this.fetchRescueCars();
		},
		async fetchRescueCars() {
			try {
				if (!this.userOrgInfo.Organization || !this.userOrgInfo.Department) return;

				const res = await request.post('api/ssbh/Position/GetPageData', {
					page: 1, rows: 100,
					wheres: JSON.stringify([
						{ name: 'Category', value: 10 },
						{ name: 'OrganizationId', value: this.userOrgInfo.Organization.Id },
						{ name: 'DepartmentId', value: this.userOrgInfo.Department.Id }
					]),
					sort: 'OrderBy', order: 'asc'
				});
				this.rescueCarList = (res.rows || []).filter(w => w.Category === 10);

				if (this.rescueCarList.length > 0) {
					let targetId = this.selectedRescueCarId;

					// Try to load from storage if no specific selection
					if (!targetId) {
						const lastId = storage.get('lastSelectedRescueCarId');
						// Use loose comparison
						if (lastId && this.rescueCarList.find(w => w.Id == lastId)) {
							targetId = lastId;
						}
					}

					if (!targetId || !this.rescueCarList.find(w => w.Id == targetId)) {
						this.selectRescueCar(this.rescueCarList[0].Id);
					} else {
						this.selectRescueCar(targetId);
					}
				}
			} catch (e) { console.error(e); }
		},
		selectRescueCar(id) {
			this.selectedRescueCarId = id;
			storage.set('lastSelectedRescueCarId', id);
			this.fetchRescueCarData(id);
			this.scrollIntoViewId = '';
			this.$nextTick(() => {
				this.scrollIntoViewId = 'car-' + id;
			});
		},
		async fetchRescueCarData(warehouseId) {
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
				this.floorList = allItems.filter(p => p.Category === 100).sort((a, b) => (a.OrderBy || 0) - (b.OrderBy || 0));
				this.serialList = allItems.filter(p => p.Category === 110);

				const itemIds = this.serialList.filter(s => s.ItemId > 0).map(s => s.ItemId);
				if (itemIds.length > 0) {
					this.fetchItemDetailsForList([...new Set(itemIds)]);
				}
			} catch (e) {
				console.error("Fetch Rescue Car Data Error", e);
			}
		},
		async fetchItemDetailsForList(ids) {
			const distinctIds = ids.filter(id => !this.itemsMap[id]);
			if (distinctIds.length === 0) return;

			try {
				const res = await request.post('api/ssbh/Item/GetPageData', {
					page: 1, rows: 500,
					wheres: JSON.stringify([
						{ name: 'Id', value: distinctIds.join(','), DisplayType: 'in' },
						{ name: 'OrganizationId', value: this.userOrgInfo.Organization.Id },
						{ name: 'DepartmentId', value: this.userOrgInfo.Department.Id }
					])
				});

				if (res.rows && res.rows.length > 0) {
					res.rows.forEach(item => {
						const dateToUse = item.LatestExpirationDate || item.ExpiryDate;
						const days = this.getDaysUntil(dateToUse);
						item.FormattedDate = this.formatDate(dateToUse);
						item.ExpiryClass = this.getExpiryClass(dateToUse);
						const stock = Number(item.AvailableStock || 0);
						const base = Number(item.Cardinality || 0);
						item.IsStockInsufficient = stock <= 0 || (base > 0 && stock < base);
						item.IsExpiryWarning = days < 30;

						this.$set(this.itemsMap, item.Id, item);
					});
				}
			} catch (e) {
				console.error("Batch fetch item details failed", e);
			}
		},
		// Display helpers moved to StorageLayers component
		toggleFloor(floor) {
			this.$set(floor, 'expanded', !floor.expanded);
		},
		async refreshFloorSerials(floor) {
			uni.showLoading({ title: '加载中', mask: true });
			try {
				const res = await request.post('api/ssbh/Position/GetPageData', {
					page: 1, rows: 500,
					wheres: JSON.stringify([
						{ name: 'Category', value: 110 },
						{ name: 'OrganizationId', value: this.userOrgInfo.Organization.Id },
						{ name: 'DepartmentId', value: this.userOrgInfo.Department.Id },
						{ name: 'WarehouseId', value: floor.WarehouseId },
						{ name: 'FloorId', value: floor.Id }
					]),
					sort: 'SerialId', order: 'asc'
				});
				const newSerials = res.rows || [];
				const otherSerials = this.serialList.filter(s => s.FloorId !== floor.Id);
				this.serialList = [...otherSerials, ...newSerials];
				const itemIds = newSerials.filter(s => s.ItemId > 0).map(s => s.ItemId);
				if (itemIds.length > 0) {
					await this.fetchItemDetailsForList([...new Set(itemIds)]);
				}
				this.$set(floor, 'expanded', true);
			} catch (e) {
				console.error("Refresh Serials Error", e);
				uni.showToast({ title: '加载失败', icon: 'none' });
			} finally {
				uni.hideLoading();
			}
		},

		// Interaction Logic
		onSerialClick(serial) {
			// interact to clear highlight
			this.highlightedSerialIds = [];

			// interact to clear highlight
			this.highlightedSerialIds = [];

			if (serial.ItemId && serial.ItemId > 0) {
				const item = this.itemsMap[serial.ItemId];
				if (item) {
					this.openOutboundModal(item);
				}
			} else {
				uni.showToast({ title: '该位置为空', icon: 'none' });
			}
		},

		// Modal & Outbound Logic
		openOutboundModal(item) {
			this.selectedItem = item;
			this.selectedBatchId = null;
			this.selectedBatch = null;
			this.outQtyInput = '1';
			this.itemDetails = [];
			this.showModal = true;
			this.fetchBatchDetails(item.Id);
		},
		fetchBatchDetails(itemId) {
			request.post('api/ssbh/ItemDetail/GetPageData', {
				page: 1, rows: 100, sort: 'ExpirationDate', order: 'asc',
				wheres: JSON.stringify([
					{ name: 'ItemId', value: itemId },
					{ name: 'Stock', value: 0, displayType: '>' }
				])
			}).then(res => {
				if (res && res.rows) {
					this.itemDetails = res.rows;
					if (this.itemDetails.length > 0) {
						this.selectBatch(this.itemDetails[0]);
					}
				}
			});
		},
		selectBatch(batch) {
			this.selectedBatchId = batch.Id;
			this.selectedBatch = batch;
			this.outQtyInput = '1';
		},
		closeModal() { this.showModal = false; },

		// Keypad Logic
		onKeypadInput(val) {
			if (this.outQtyInput === '0') this.outQtyInput = val; // Replace leading zero
			// Limit length
			else if (this.outQtyInput.length < 5) {
				this.outQtyInput += val;
			}
		},
		onKeypadDelete() {
			if (this.outQtyInput.length > 1) {
				this.outQtyInput = this.outQtyInput.slice(0, -1);
			} else {
				this.outQtyInput = '0'; // Don't make it empty
			}
		},
		quickAction(type) {
			let val = this.outQty;
			const max = this.maxQty; // Safe Number from computed

			if (type === 'add') {
				if (val < max) {
					val++;
					this.outQtyInput = val.toString();
				} else {
					uni.showToast({ title: '库存不足', icon: 'none' });
				}
			} else if (type === 'sub') {
				if (val > 1) {
					val--;
					this.outQtyInput = val.toString();
				} else {
					this.outQtyInput = '0';
				}
			} else if (type === 'clear') {
				this.outQtyInput = '0';
			}
		},

		submitOutbound() {
			if (!this.canSubmit) return;
			const payload = {
				ItemId: this.selectedItem.Id,
				ItemDetailId: this.selectedBatchId,
				ChangeCounter: -this.outQty,
				Stock: this.selectedItem.AvailableStock - this.outQty,
				Remark: 'Quick Outbound',
				CartId: this.selectedRescueCarId
			};

			uni.showLoading({ title: '提交中' });
			request.post('api/ssbh/ItemHistory/Add', { mainData: payload }).then(res => {
				uni.hideLoading();
				if (res.success || res.status) {
					uni.showToast({ title: '出库成功', icon: 'success' });

					// Local Update Logic
					const qty = this.outQty;

					// Update Item Stock & Warning Flags
					if (this.selectedItem) {
						const oldStock = Number(this.selectedItem.AvailableStock) || 0;
						const newStock = oldStock - qty;

						// Update in itemsMap (main source of truth for StorageLayers)
						const itemInMap = this.itemsMap[this.selectedItem.Id];
						if (itemInMap) {
							this.$set(itemInMap, 'AvailableStock', newStock);

							// Recalculate Stock Warning
							const base = Number(itemInMap.Cardinality || 0);
							this.$set(itemInMap, 'IsStockInsufficient', newStock <= 0 || (base > 0 && newStock < base));

							// Update selectedItem reference
							this.selectedItem.AvailableStock = newStock;
							this.selectedItem.IsStockInsufficient = itemInMap.IsStockInsufficient;
						}
					}

					// Update Batch Stock
					if (this.selectedBatchId) {
						const batch = this.itemDetails.find(b => b.Id === this.selectedBatchId);
						if (batch) {
							const oldBatchStock = Number(batch.Stock) || 0;
							const newBatchStock = oldBatchStock - qty;
							this.$set(batch, 'Stock', newBatchStock);
						}
					}

					this.closeModal();
				} else {
					uni.showToast({ title: res.msg || '失败', icon: 'none' });
				}
			}).catch(e => {
				uni.hideLoading();
				console.error(e);
				uni.showToast({ title: '提交失败', icon: 'none' });
			});
		},

		// Search & Scan
		async onScan() {
			uni.scanCode({
				success: (res) => {
					if (res.result) {
						this.searchText = res.result;
						this.onSearch();
					}
				}
			});
		},
		clearSearch() {
			this.searchText = '';
			this.highlightedSerialIds = [];
		},
		async onSearch() {
			if (!this.searchText) {
				this.highlightedSerialIds = [];
				return;
			}
			if (!this.selectedRescueCarId) return uni.showToast({ title: '请先选择抢救车', icon: 'none' });

			uni.showLoading({ title: '搜索中...' });
			this.highlightedSerialIds = []; // Reset

			try {
				const res = await request.post('api/ssbh/Position/GetPageData', {
					page: 1, rows: 100,
					wheres: JSON.stringify([
						{ name: 'Category', value: 110 },
						{ name: 'WarehouseId', value: this.selectedRescueCarId },
						{ name: 'OrganizationId', value: this.userOrgInfo.Organization.Id },
						{ name: 'DepartmentId', value: this.userOrgInfo.Department.Id },
						{ name: 'ItemName', value: this.searchText, displayType: 'like' }
					])
				});

				if (res.rows && res.rows.length > 0) {
					const foundSerials = res.rows;
					const floorIds = [...new Set(foundSerials.map(s => s.FloorId))];

					// Ensure floors are expanded
					for (let fid of floorIds) {
						const floor = this.floorList.find(f => f.Id === fid);
						if (floor) {
							this.$set(floor, 'expanded', true);
						}
					}

					// Highlight matches
					this.highlightedSerialIds = foundSerials.map(s => s.Id);

					// Scroll to first match (Centered)
					if (this.highlightedSerialIds.length > 0) {
						const firstId = this.highlightedSerialIds[0];
						this.$nextTick(() => {
							const query = uni.createSelectorQuery().in(this);
							query.select('#serial-' + firstId).boundingClientRect(data => {
								if (data) {
									const sysInfo = uni.getSystemInfoSync();
									const windowHeight = sysInfo.windowHeight;

									// Calculate delta to move element to center
									// data.top is relative to viewport
									// We want data.top to be windowHeight / 2
									// currentScrollTop + (data.top - windowHeight/2 + height/2)

									let scrollDiff = data.top - (windowHeight / 2) + (data.height / 2);
									let targetTop = this.scrollTop + scrollDiff;

									// Ensure we trigger a change
									this.scrollTop = this.scrollTop === targetTop ? targetTop + 1 : targetTop;
								}
							}).exec();
						});
					}

					uni.showToast({ title: `找到 ${foundSerials.length} 个物品`, icon: 'none' });
				} else {
					uni.showToast({ title: '未找到相关物品', icon: 'none' });
				}
			} catch (e) { console.error(e); uni.showToast({ title: '搜索失败', icon: 'none' }); }
			finally { uni.hideLoading(); }
		},


		// Utils
		formatDate(dateStr) {
			if (!dateStr) return '';
			return dayjs(dateStr).format('YYYY-MM-DD');
		},
		getDaysUntil(dateStr) {
			if (!dateStr) return 9999;
			const now = new Date();
			const target = dayjs(dateStr).toDate();
			return Math.ceil((target.getTime() - now.getTime()) / (1000 * 3600 * 24));
		},
		getExpiryClass(dateStr) {
			const days = this.getDaysUntil(dateStr);
			if (days < 0) return 'text-black';
			if (days < 30) return 'text-red';
			if (days <= 90) return 'text-yellow';
			return 'text-normal';
		},
		// Helper methods moved to StorageLayers or unused
	}
}
</script>

<style lang="scss" scoped>
// Layout & structure from position-manage.vue
.container {
	background-color: #F7F8FA;
	height: 100vh;
	display: flex;
	flex-direction: column;
}

.header-section {
	height: 400rpx;
	background: linear-gradient(135deg, #FF9A9E 0%, #FECFEF 100%);
	/* Different Gradient for Outbound? Pink/Red tone */
	padding: 0 48rpx;
	display: flex;
	flex-direction: column;
	box-sizing: border-box;
}

.nav-bar {
	display: flex;
	align-items: center;
}

.header-content {
	margin-top: 0;
	padding-bottom: 40rpx;
}

.page-title {
	font-size: 44rpx;
	font-weight: bold;
	color: #fff;
	margin-left: 24rpx;
	text-shadow: 0 4rpx 8rpx rgba(0, 0, 0, 0.1);
}

.main-scroll {
	flex: 1;
	height: 100%;
}


.content-panel {
	background-color: #fff;
	border-top-left-radius: 60rpx;
	border-top-right-radius: 60rpx;
	margin-top: -80rpx;
	padding: 48rpx 32rpx;
	min-height: 1000rpx;
	position: relative;
	z-index: 10;
}

// Search
.search-row {
	margin-top: 24rpx;
	display: flex;
	align-items: center;
}

.search-box {
	flex: 1;
	height: 72rpx;
	background: rgba(255, 255, 255, 0.2);
	border-radius: 36rpx;
	display: flex;
	align-items: center;
	padding: 0 24rpx;
	backdrop-filter: blur(10rpx);
}

.search-icon {
	font-size: 28rpx;
	margin-right: 16rpx;
	color: #fff;
	opacity: 0.8;
}

.search-input {
	flex: 1;
	font-size: 28rpx;
	color: #fff;
}

.clear-icon {
	margin-left: 16rpx;
	color: rgba(255, 255, 255, 0.8);
}

.scan-btn {
	width: 72rpx;
	height: 72rpx;
	background: rgba(255, 255, 255, 0.2);
	border-radius: 50%;
	margin-left: 24rpx;
	display: flex;
	align-items: center;
	justify-content: center;
	backdrop-filter: blur(10rpx);
}

.scan-icon {
	width: 40rpx;
	height: 40rpx;
	background-color: #fff;
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M3 5v4h2V5h4V3H5c-1.1 0-2 .9-2 2zm2 10H3v4c0 1.1.9 2 2 2h4v-2H5v-4zm14 4h-4v2h4c1.1 0 2-.9 2-2v-4h-2v4zm0-16h-4v2h4v4h2V5c0-1.1-.9-2-2-2z'/%3E%3C/svg%3E");
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M3 5v4h2V5h4V3H5c-1.1 0-2 .9-2 2zm2 10H3v4c0 1.1.9 2 2 2h4v-2H5v-4zm14 4h-4v2h4c1.1 0 2-.9 2-2v-4h-2v4zm0-16h-4v2h4v4h2V5c0-1.1-.9-2-2-2z'/%3E%3C/svg%3E");
	-webkit-mask-size: contain;
	mask-size: contain;
	-webkit-mask-repeat: no-repeat;
	mask-repeat: no-repeat;
	-webkit-mask-position: center;
	mask-position: center;
}


// Horizontal Switcher
.horizontal-section {
	display: flex;
	align-items: center;
	margin-bottom: 48rpx;
}

.horizontal-scroll {
	flex: 1;
	white-space: nowrap;
	width: 0;
}

.scroll-inner {
	display: flex;
}

.h-card {
	display: inline-flex;
	align-items: center;
	justify-content: center;
	height: 96rpx;
	padding: 0 40rpx;
	background: #F7F8FA;
	border-radius: 48rpx;
	margin-right: 24rpx;
	border: 2rpx solid transparent;
	border: 2rpx solid transparent;
	transition: all 0.3s;
	min-width: auto;
	flex-shrink: 0;

	.card-name {
		font-size: 28rpx;
		font-weight: 500;
		color: #555;
	}

	&.active {
		background: #FFF0F0;
		color: #FF4D4F;
		border: 2rpx solid #FF4D4F;

		.card-name {
			color: #FF4D4F;
			font-weight: bold;
		}
	}
}

.empty-placeholder {
	color: #999;
	font-size: 28rpx;
	text-align: center;
	margin-top: 40rpx;
}

// Floors & Serials (Copied & simplified)
/* Floor Styles Removed (Moved to StorageLayers) */

// Inbound/Outbound Modal Specifics
.modal-mask {
	position: fixed;
	top: 0;
	left: 0;
	width: 100%;
	height: 100%;
	background: rgba(0, 0, 0, 0.5);
	z-index: 1000;
	display: flex;
	align-items: flex-end;
}

.modal-content {
	width: 100%;
	background: #fff;
	border-radius: 48rpx 48rpx 0 0;
	display: flex;
	flex-direction: column;
	animation: slideUp 0.3s ease;
	max-height: 95vh;
}

.modal-header {
	padding: 32rpx 48rpx;
	display: flex;
	justify-content: space-between;
	align-items: center;
	border-bottom: 2rpx solid #eee;
}

.title-section {
	display: flex;
	flex-direction: column;
}

.stock-warning {
	font-size: 24rpx;
	margin-top: 8rpx;
}

.stock-warning-label {
	font-size: 28rpx;
	color: #FF4D4F;
	font-weight: bold;
	animation: flash 1s infinite alternate;
}

@keyframes flash {
	from {
		opacity: 0.6;
	}

	to {
		opacity: 1;
	}
}

.modal-title {
	font-size: 36rpx;
	font-weight: bold;
}

.close-btn {
	font-size: 40rpx;
	color: #999;
	padding: 16rpx;
}

.modal-body {
	padding: 48rpx;
}

.form-label {
	font-size: 28rpx;
	color: #666;
	margin-bottom: 16rpx;
	display: block;
}

.batch-scroll {
	width: 100%;
	white-space: nowrap;
	margin-bottom: 40rpx;
}

.batch-list-h {
	display: flex;
	gap: 20rpx;
	padding-top: 20rpx; // Make space for the top badge
	padding-right: 20rpx; // Make space for the right badge
}

.batch-item {
	padding: 20rpx;
	border: 2rpx solid #eee;
	border-radius: 16rpx;
	min-width: 240rpx;
	transition: all 0.2s;
	position: relative; // For checkmark

	&.active {
		// Keep warning border if present, relies on checkmark for selection indication
		background: #FFF0F0; // Or whatever background was appropriate, or Keep original logic
		// But let's match the inbound style closer if we want consistency
		// Inbound used #E6F7FF but also handled warning overrides.
		// Outbound uses red theme.
		background: #FFF1F0;
		border-color: #FF4D4F; // Keep red border for outbound theme
	}

	&.active::after {
		content: '✓';
		position: absolute;
		top: -16rpx;
		right: -16rpx;
		background: #FF4D4F; // Red for outbound context
		color: #fff;
		width: 40rpx;
		height: 40rpx;
		border-radius: 50%;
		font-size: 24rpx;
		display: flex;
		align-items: center;
		justify-content: center;
		border: 4rpx solid #fff;
		box-shadow: 0 4rpx 8rpx rgba(0, 0, 0, 0.1);
	}

	&.disabled {
		opacity: 0.5;
	}

	&.stock-warning {
		border: 4rpx solid #F56C6C !important;
		background-color: #FFF0F0;
	}

	&.stock-insufficient {
		border: 4rpx solid #E6A23C !important;
		background-color: #FDF6EC;
	}
}

.stock-zero {
	color: #F56C6C;
	font-weight: bold;
}

.stock-insufficient-text {
	color: #E6A23C;
	font-weight: bold;
}

.batch-row {
	font-size: 24rpx;
	color: #333;
	margin-bottom: 4rpx;
}

.batch-no {
	font-size: 24rpx;
	font-weight: bold;
	color: #333;
}

.batch-expiry {
	font-size: 22rpx;
	color: #666;
}

.batch-stock {
	font-size: 22rpx;
	color: #666;
	font-weight: bold;
}

/* Qty Control Redesign */
.qty-control-row {
	display: flex;
	align-items: center;
	justify-content: center;
	padding: 32rpx 24rpx;
	margin-bottom: 16rpx;
	gap: 40rpx;
}

.qty-value-display {
	display: flex;
	flex-direction: column;
	align-items: center;
	min-width: 240rpx;
	/* Ensure visual stability */
}

.qty-val-group {
	display: flex;
	align-items: baseline;
	margin: 8rpx 0;
}

.label {
	font-size: 24rpx;
	color: #606266;
}

.qty-text {
	font-size: 64rpx;
	font-weight: bold;
	color: #303133;
	line-height: 1;
}

.unit-text {
	font-size: 24rpx;
	color: #909399;
	margin-left: 8rpx;
}

.stock-hint {
	font-size: 24rpx;
	color: #909399;
}

.qty-value-display.error .qty-text {
	color: #FF4D4F;
}

.q-btn-circle {
	width: 96rpx;
	height: 96rpx;
	border-radius: 50%;
	display: flex;
	align-items: center;
	justify-content: center;
	font-size: 32rpx;
	font-weight: bold;
	background: #F2F3F5;
	color: #606266;

	&:active {
		opacity: 0.8;
		transform: scale(0.95);
	}
}

.q-btn-circle.sub {
	background: #FFF1F0;
	color: #FF4D4F;
}

.q-btn-circle.add {
	background: #E6F7FF;
	color: #1890FF;
}

/* Clear Full Width - Match Keypad Width */
.clear-action-bar {
	margin-top: 20rpx;
	display: flex;
}

.q-btn-full {
	flex: 1;
	height: 96rpx;
	border-radius: 16rpx;
	background: #F5F7FA;
	color: #909399;
	font-size: 32rpx;
	display: flex;
	align-items: center;
	justify-content: center;
	font-weight: 500;
	margin-top: 0;

	&:active {
		background: #e6e8eb;
	}
}

.keypad-wrapper {
	margin-top: 20rpx;
}

/* Old Quick Actions styles removed */

.modal-footer {
	padding: 32rpx 48rpx calc(32rpx + env(safe-area-inset-bottom)) 48rpx;
	border-top: 2rpx solid #eee;
}

.submit-btn {
	background: #FF4D4F;
	color: #fff;
	border-radius: 48rpx;
	height: 96rpx;
	font-size: 32rpx;
	font-weight: bold;

	&[disabled] {
		opacity: 0.5;
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

@keyframes fadeIn {
	from {
		opacity: 0;
	}

	to {
		opacity: 1;
	}
}

.bottom-spacer {
	height: 80rpx;
}


// Colors
.text-red {
	color: #ff4d4f;
}

.text-yellow {
	color: #faad14;
}

.text-normal {
	color: #999;
}

.text-black {
	color: #000;
	font-weight: bold;
}

/* Alerts */
.text-red-bold {
	color: #F56C6C !important;
	font-weight: bold;
}

.floor-status-icons {
	display: flex;
	align-items: center;
	margin-right: 16rpx;
}

.status-icon {
	width: 48rpx;
	height: 48rpx;
	border-radius: 50%;
	display: flex;
	align-items: center;
	justify-content: center;
	margin-left: 8rpx;
	background: #f5f7fa;
}

.status-icon.warning {
	background: #FEF0F0;
	border: 2rpx solid #F56C6C;
}

.status-icon.danger {
	background: #FDF6EC;
	border: 2rpx solid #E89310;
}

.status-bg-icon {
	width: 32rpx;
	height: 32rpx;
	background-size: contain;
	background-repeat: no-repeat;
	background-position: center;
}

.icon-clock {
	/* Red Clock */
	background-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='%23F56C6C'%3E%3Cpath d='M11.99 2C6.47 2 2 6.48 2 12s4.47 10 9.99 10C17.52 22 22 17.52 22 12S17.52 2 11.99 2zM12 20c-4.42 0-8-3.58-8-8s3.58-8 8-8 8 3.58 8 8-3.58 8 8-8zm.5-13H11v6l5.25 3.15.75-1.23-4.5-2.67z'/%3E%3C/svg%3E");
}

.icon-siren {
	/* Orange Siren */
	background-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='%23E6A23C'%3E%3Cpath d='M12 4c-3.3 0-6 2.7-6 6v4h12v-4c0-3.3-2.7-6-6-6zm-1 3h2v4h-2V7z'/%3E%3Cpath d='M5 16v3c0 1.1 0.9 2 2 2h10c1.1 0 2-0.9 2-2v-3H5z'/%3E%3Cpath d='M12 1.5l1.5 1.5-1.5 1.5-1.5-1.5L12 1.5zM4.5 5.5l2 2-1.5 1.5-2-2 1.5-1.5zM19.5 5.5l-2 2 1.5 1.5 2-2-1.5-1.5z'/%3E%3C/svg%3E");
}

.alert-bell {
	width: 36rpx;
	height: 36rpx;
	background-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='%23F56C6C'%3E%3Cpath d='M12 22c1.1 0 2-.9 2-2h-4c0 1.1.9 2 2 2zm6-6v-5c0-3.07-1.63-5.64-4.5-6.32V4c0-.83-.67-1.5-1.5-1.5s-1.5.67-1.5 1.5v.68C7.64 5.36 6 7.92 6 11v5l-2 2v1h16v-1l-2-2z'/%3E%3C/svg%3E");
	background-size: contain;
	background-repeat: no-repeat;
	animation: bell-shake 1.2s infinite ease-in-out;
	margin-right: 8rpx;
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

/* Page Scroll Fix */
page {
	height: 100vh;
	overflow: hidden;
	background-color: $sull-color-background;
}

.container {
	display: flex;
	flex-direction: column;
	height: 100vh;
	width: 100%;
	overflow: hidden;
	background-color: $sull-color-background;
}

.main-content {
	flex: 1;
	height: 0;
	width: 100%;
}

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
		transform: translateY(-6px);
	}
}
</style>

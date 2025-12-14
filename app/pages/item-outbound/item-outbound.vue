<template>
	<view class="container">
		<scroll-view class="main-scroll" scroll-y @scroll="onScroll" :scroll-top="scrollTop" lower-threshold="50"
			@scrolltolower="onLoadMore" refresher-enabled @refresherrefresh="onRefresh"
			:refresher-triggered="isRefreshing" scroll-with-animation>

			<!-- Header Gradient Section matching Position Manage -->
			<view class="header-section" :style="{ paddingTop: (statusBarHeight + 10) + 'px' }">
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
					<scroll-view scroll-x class="horizontal-scroll" :show-scrollbar="false">
						<view class="scroll-inner">
							<view class="h-card" v-for="(item, index) in rescueCarList" :key="item.Id"
								:class="{ active: selectedRescueCarId === item.Id }" @click="selectRescueCar(item.Id)">
								<text class="card-name">{{ item.DisplayName }}</text>
							</view>
						</view>
					</scroll-view>
				</view>
				<view class="empty-placeholder" v-else>
					暂无抢救车数据
				</view>

				<!-- Floor List -->
				<view class="floor-list">
					<view class="floor-wrapper" v-for="floor in floorList" :key="floor.Id">
						<view class="floor-card" @click="toggleFloor(floor)">
							<!-- Icon -->
							<view class="floor-icon">
								<view class="icon-circle" :class="getFloorStatusClass(floor)"></view>
							</view>

							<!-- Info -->
							<view class="floor-info">
								<text class="floor-name">{{ floor.DisplayName || floor.Name }}</text>
								<text class="floor-sub">已用 {{ getOccupiedCount(floor) }} / 共 {{ floor.SerialId || 0 }}
									格</text>
							</view>
							<!-- Status Icons -->
							<view class="floor-status-icons">
								<view class="status-icon warning" v-if="hasExpiryProblem(floor)">
									<view class="status-bg-icon icon-clock"></view>
								</view>
								<view class="status-icon danger" v-if="hasStockProblem(floor)">
									<view class="status-bg-icon icon-siren"></view>
								</view>
							</view>
						</view>

						<!-- Expanded Details (Serials) -->
						<view class="serial-grid-wrapper" v-if="floor.expanded">
							<view class="serial-grid">
								<view class="serial-item" v-for="serial in getSerialsByFloor(floor.Id)" :key="serial.Id"
									:id="'serial-' + serial.Id"
									:class="{ 'highlight-glow': highlightedSerialIds.includes(serial.Id), 'expiry-warning': serial.ItemId > 0 && itemsMap[serial.ItemId] && isExpiryWarning(itemsMap[serial.ItemId]) }"
									@click="onSerialClick(serial)">
									<view class="serial-header">
										<view class="alert-bell"
											v-if="serial.ItemId > 0 && itemsMap[serial.ItemId] && shouldShowAlert(itemsMap[serial.ItemId])">
										</view>
										<text class="serial-no">{{ serial.SerialId || serial.OrderBy }}</text>
									</view>

									<!-- Occupied Content -->
									<view class="item-detail" v-if="serial.ItemId > 0 && itemsMap[serial.ItemId]">
										<view class="item-name-wrapper">
											<text class="item-name-text"
												:class="{ scrolling: (itemsMap[serial.ItemId].Name || '').length > 8 }">{{
													itemsMap[serial.ItemId].Name }}</text>
										</view>
										<view class="item-info-row">
											<text class="item-spec">{{ itemsMap[serial.ItemId].Specification || '-'
											}}</text>
											<view class="item-stock"
												:class="{ 'text-red-bold': isStockInsufficient(itemsMap[serial.ItemId]) }">
												<text>x {{ itemsMap[serial.ItemId].AvailableStock || 0 }}</text>
												<text class="item-cardinality"
													v-if="itemsMap[serial.ItemId].Cardinality"> / {{
														itemsMap[serial.ItemId].Cardinality }}</text>
												<text class="item-unit" v-if="itemsMap[serial.ItemId].Unit">{{
													itemsMap[serial.ItemId].Unit }}</text>
											</view>
										</view>
										<view class="item-expiry">
											<text
												:class="getExpiryClass(itemsMap[serial.ItemId].LatestExpirationDate || itemsMap[serial.ItemId].ExpiryDate)">
												{{ formatDate(itemsMap[serial.ItemId].LatestExpirationDate ||
													itemsMap[serial.ItemId].ExpiryDate) }}
											</text>
										</view>
									</view>

									<!-- Empty Content -->
									<view class="empty-slot" v-else>
										<text class="plus-icon">+</text>
									</view>
								</view>
							</view>
						</view>
					</view>
				</view>
				<view style="height: 40px;"></view>
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
									:class="{ active: selectedBatchId === batch.Id, disabled: batch.Stock <= 0 }"
									@click="batch.Stock > 0 && selectBatch(batch)">
									<view class="batch-row">
										<text class="batch-no">批号: {{ batch.BatchNo || '-' }}</text>
									</view>
									<view class="batch-row">
										<text class="batch-expiry">效期: {{ formatDate(batch.ExpirationDate) }}</text>
									</view>
									<view class="batch-row">
										<text class="batch-stock">库存: {{ batch.Stock }}</text>
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
import request from '@/utils/request.js';
import dayjs from 'dayjs';

export default {
	components: { BackButton, FooterInfo, BackToTop, NumericKeypad },
	data() {
		return {
			statusBarHeight: 20,
			scrollTop: 0,
			oldScrollTop: 0,
			isRefreshing: false,
			searchText: '',

			// Data Lists (Same as Position Manage)
			rescueCarList: [],
			allFloorList: [],
			floorList: [],
			serialList: [],
			selectedRescueCarId: null,
			itemsMap: {},
			userOrgInfo: {},

			// Search Highlight
			highlightedSerialIds: [], // Array of IDs

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
		}
	},
	onLoad() {
		const sysInfo = uni.getSystemInfoSync();
		this.statusBarHeight = sysInfo.statusBarHeight || 20;
		this.userOrgInfo = uni.getStorageSync('userOrgInfo')?.User || {};
		this.fetchData();
	},
	methods: {
		goBack() { uni.navigateBack(); },
		goTop() {
			this.scrollTop = this.oldScrollTop;
			this.$nextTick(() => { this.scrollTop = 0; });
		},
		onScroll(e) {
			this.scrollTop = e.detail.scrollTop;
			this.oldScrollTop = e.detail.scrollTop;
		},
		onRefresh() {
			this.isRefreshing = true;
			this.fetchData().then(() => {
				this.isRefreshing = false;
				uni.showToast({ title: '已刷新', icon: 'none' });
			});
		},

		// Data Fetching Logic (Mirrors Position Manage)
		async fetchData() {
			await this.fetchRescueCars();
		},
		async fetchRescueCars() {
			try {
				if (!this.userOrgInfo.OrganizationId || !this.userOrgInfo.DepartmentId) return;

				const res = await request.post('api/ssbh/Position/GetPageData', {
					page: 1, rows: 100,
					wheres: JSON.stringify([
						{ name: 'Category', value: "10,100", DisplayType: 'in' },
						{ name: 'OrganizationId', value: this.userOrgInfo.OrganizationId },
						{ name: 'DepartmentId', value: this.userOrgInfo.DepartmentId }
					]),
					sort: 'OrderBy', order: 'asc'
				});
				this.rescueCarList = (res.rows || []).filter(w => w.Category === 10);
				this.allFloorList = (res.rows || []).filter(w => w.Category === 100);
				if (this.rescueCarList.length > 0) {
					if (!this.selectedRescueCarId || !this.rescueCarList.find(w => w.Id === this.selectedRescueCarId)) {
						this.selectRescueCar(this.rescueCarList[0].Id);
					} else {
						this.selectRescueCar(this.selectedRescueCarId);
					}
				}
			} catch (e) { console.error(e); }
		},
		selectRescueCar(id) {
			this.selectedRescueCarId = id;
			this.floorList = this.allFloorList.filter(f => f.WarehouseId === id);
			if (this.floorList.length > 0) {
				const floorIds = this.floorList.map(f => f.Id);
				this.fetchSerialsByFloors(floorIds);
			} else {
				this.serialList = [];
			}
		},
		async fetchSerialsByFloors(floorIds) {
			if (!floorIds || floorIds.length === 0) return;
			try {
				const res = await request.post('api/ssbh/Position/GetPageData', {
					page: 1, rows: 500,
					wheres: JSON.stringify([
						{ name: 'Category', value: 110 },
						{ name: 'ParentId', value: floorIds.join(','), DisplayType: 'in' }
					]),
					sort: 'SerialId', order: 'asc'
				});
				this.serialList = res.rows || [];
				const itemIds = this.serialList.filter(s => s.ItemId > 0).map(s => s.ItemId);
				if (itemIds.length > 0) {
					this.fetchItemDetailsForList([...new Set(itemIds)]);
				}
			} catch (e) { console.error(e); }
		},
		async fetchItemDetailsForList(ids) {
			const distinctIds = [...new Set(ids)];
			for (let id of distinctIds) {
				if (this.itemsMap[id]) continue;
				try {
					const res = await request.post('api/ssbh/Item/GetPageData', {
						page: 1, rows: 1,
						wheres: JSON.stringify([{ name: 'Id', value: id }, { name: 'OrganizationId', value: this.userOrgInfo.OrganizationId },
						{ name: 'DepartmentId', value: this.userOrgInfo.DepartmentId }])
					});
					if (res.rows && res.rows.length > 0) {
						this.$set(this.itemsMap, id, res.rows[0]);
					}
				} catch (e) { }
			}
		},
		getSerialsByFloor(floorId) {
			return this.serialList.filter(s => s.FloorId === floorId);
		},
		getOccupiedCount(floor) {
			const serials = this.getSerialsByFloor(floor.Id);
			return serials.filter(s => s.ItemId && s.ItemId > 0).length;
		},
		getFloorStatusClass(floor) {
			// Simplified status for Outbound (maybe just show stock status)
			// Copy logic from position-manage basically
			const serials = this.getSerialsByFloor(floor.Id);
			if (!serials || serials.length === 0) return 'blink-green';
			let hasExpiryWarning = false;
			let hasStockWarning = false;
			for (let s of serials) {
				if (s.ItemId > 0 && this.itemsMap[s.ItemId]) {
					const item = this.itemsMap[s.ItemId];
					const days = this.getDaysUntil(item.ExpiryDate);
					if (days < 30) { hasExpiryWarning = true; break; }
					if (item.AvailableStock < item.Cardinality || item.AvailableStock <= 0) { hasStockWarning = true; }
				}
			}
			if (hasExpiryWarning) return 'blink-red';
			if (hasStockWarning) return 'blink-orange';
			return 'blink-green';
		},
		async toggleFloor(floor) {
			if (floor.expanded) {
				this.$set(floor, 'expanded', false);
				return;
			}
			await this.refreshFloorSerials(floor);
		},
		async refreshFloorSerials(floor) {
			uni.showLoading({ title: '加载中', mask: true });
			try {
				const res = await request.post('api/ssbh/Position/GetPageData', {
					page: 1, rows: 500,
					wheres: JSON.stringify([
						{ name: 'Category', value: 110 },
						{ name: 'OrganizationId', value: this.userOrgInfo.OrganizationId },
						{ name: 'DepartmentId', value: this.userOrgInfo.DepartmentId },
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
				CreatorId: this.userOrgInfo.UserId || 0,
				Creator: this.userOrgInfo.UserName || 'User'
			};

			uni.showLoading({ title: '提交中' });
			request.post('api/ssbh/ItemHistory/Add', { mainData: payload }).then(res => {
				uni.hideLoading();
				if (res.success || res.status) {
					uni.showToast({ title: '出库成功', icon: 'success' });

					// Local Update Logic
					const qty = this.outQty;

					// Update Item Stock
					if (this.selectedItem) {
						this.selectedItem.AvailableStock = (this.selectedItem.AvailableStock || 0) - qty;
						if (this.itemsMap[this.selectedItem.Id]) {
							this.itemsMap[this.selectedItem.Id].AvailableStock = this.selectedItem.AvailableStock;
						}
					}

					// Update Batch Stock
					if (this.selectedBatchId) {
						const batch = this.itemDetails.find(b => b.Id === this.selectedBatchId);
						if (batch) {
							batch.Stock = (batch.Stock || 0) - qty;
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
						{ name: 'OrganizationId', value: this.userOrgInfo.OrganizationId },
						{ name: 'DepartmentId', value: this.userOrgInfo.DepartmentId },
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
							if (!floor.expanded) {
								await this.refreshFloorSerials(floor);
							}
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
			const target = new Date(dateStr);
			return Math.ceil((target.getTime() - now.getTime()) / (1000 * 3600 * 24));
		},
		getExpiryClass(dateStr) {
			const days = this.getDaysUntil(dateStr);
			if (days < 0) return 'text-black';
			if (days < 30) return 'text-red';
			if (days <= 90) return 'text-yellow-bold';
			return 'text-normal';
		},
		hasExpiryProblem(floor) {
			return this.serialList.some(s => {
				if (s.FloorId !== floor.Id) return false;
				if (!s.ItemId || !this.itemsMap[s.ItemId]) return false;
				const item = this.itemsMap[s.ItemId];
				const days = this.getDaysUntil(item.LatestExpirationDate || item.ExpiryDate);
				return days < 30;
			});
		},
		hasStockProblem(floor) {
			return this.serialList.some(s => {
				if (s.FloorId !== floor.Id) return false;
				if (!s.ItemId || !this.itemsMap[s.ItemId]) return false;
				const item = this.itemsMap[s.ItemId];
				return this.isStockInsufficient(item);
			});
		},
		isStockInsufficient(item) {
			if (!item) return false;
			const stock = item.AvailableStock || 0;
			const base = item.BaseStock || 0; // Assuming BaseStock exists
			return stock < base || stock === 0;
		},
		isExpiryWarning(item) {
			// Prefer LatestExpirationDate, allow ExpiryDate fallback if needed, or stick to Latest
			const date = item.LatestExpirationDate || item.ExpiryDate;
			if (!date) return false;
			const days = this.getDaysUntil(date);
			return days < 30;
		},
		shouldShowAlert(item) {
			if (!item) return false;
			if (this.isStockInsufficient(item)) return true;
			if (this.isExpiryWarning(item)) return true;
			return false;
		}
	}
}
</script>

<style lang="scss" scoped>
@import '@/sull.scss';

// Layout & structure from position-manage.vue
.container {
	background-color: #F7F8FA;
	height: 100vh;
	display: flex;
	flex-direction: column;
}

.header-section {
	height: 200px;
	background: linear-gradient(135deg, #FF9A9E 0%, #FECFEF 100%);
	/* Different Gradient for Outbound? Pink/Red tone */
	padding: 0 24px;
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
	padding-bottom: 20px;
}

.page-title {
	font-size: 22px;
	font-weight: bold;
	color: #fff;
	margin-left: 12px;
	text-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.main-scroll {
	flex: 1;
	height: 100%;
}


.content-panel {
	background-color: #fff;
	border-top-left-radius: 30px;
	border-top-right-radius: 30px;
	margin-top: -40px;
	padding: 24px 16px;
	min-height: 500px;
	position: relative;
	z-index: 10;
}

// Search
.search-row {
	margin-top: 12px;
	display: flex;
	align-items: center;
}

.search-box {
	flex: 1;
	height: 36px;
	background: rgba(255, 255, 255, 0.2);
	border-radius: 18px;
	display: flex;
	align-items: center;
	padding: 0 12px;
	backdrop-filter: blur(5px);
}

.search-icon {
	font-size: 14px;
	margin-right: 8px;
	color: #fff;
	opacity: 0.8;
}

.search-input {
	flex: 1;
	font-size: 14px;
	color: #fff;
}

.clear-icon {
	margin-left: 8px;
	color: rgba(255, 255, 255, 0.8);
}

.scan-btn {
	width: 36px;
	height: 36px;
	background: rgba(255, 255, 255, 0.2);
	border-radius: 50%;
	margin-left: 12px;
	display: flex;
	align-items: center;
	justify-content: center;
	backdrop-filter: blur(5px);
}

.scan-icon {
	width: 20px;
	height: 20px;
	background-color: #fff;
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M3 5v4h2V5h4V3H5c-1.1 0-2 .9-2 2zm2 10H3v4c0 1.1.9 2 2 2h4v-2H5v-4zm14 4h-4v2h4c1.1 0 2-.9 2-2v-4h-2v4zm0-16h-4v2h4v4h2V5c0-1.1-.9-2-2-2z'/%3E%3C/svg%3E");
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
	margin-bottom: 24px;
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
	height: 48px;
	padding: 0 20px;
	background: #F7F8FA;
	border-radius: 24px;
	margin-right: 12px;
	border: 1px solid transparent;
	transition: all 0.3s;

	.card-name {
		font-size: 14px;
		font-weight: 500;
		color: #555;
	}

	&.active {
		background: #FFF0F0;
		color: #FF4D4F;
		border: 1px solid #FF4D4F;

		.card-name {
			color: #FF4D4F;
			font-weight: bold;
		}
	}
}

.empty-placeholder {
	color: #999;
	font-size: 14px;
	text-align: center;
	margin-top: 20px;
}

// Floors & Serials (Copied & simplified)
.floor-list {
	display: flex;
	flex-direction: column;
	gap: 12px;
}

.floor-card {
	background: #fff;
	border-radius: 12px;
	padding: 16px;
	box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
	display: flex;
	align-items: center;
	min-height: 60px;
	border: 1px solid #f0f0f0;

	&:active {
		background-color: #f9f9f9;
	}
}

.icon-circle {
	width: 8px;
	height: 32px;
	border-radius: 4px;
	background-color: #F7F8FA;
	transition: all 0.3s;
	margin-right: 8px;
}

.blink-red {
	background-color: #ff4d4f;
	border: 1px solid #ff4d4f;
	animation: strong-pulse-red 0.8s infinite;
}

.blink-orange {
	background-color: #faad14;
	border: 1px solid #faad14;
	animation: strong-pulse-orange 1s infinite;
}

.blink-green {
	background-color: #F0F9EB;
	border: 1px solid #E1F3D8;
	animation: pulse-green 3s infinite;
}

@keyframes strong-pulse-red {
	0% {
		box-shadow: 0 0 0 0 rgba(255, 77, 79, 0.7);
		transform: scale(0.95);
	}

	50% {
		transform: scale(1.05);
		box-shadow: 0 0 0 12px rgba(255, 77, 79, 0);
	}

	100% {
		transform: scale(0.95);
		box-shadow: 0 0 0 0 rgba(255, 77, 79, 0);
	}
}

@keyframes strong-pulse-orange {
	0% {
		box-shadow: 0 0 0 0 rgba(250, 173, 20, 0.7);
		transform: scale(0.95);
	}

	50% {
		transform: scale(1.05);
		box-shadow: 0 0 0 12px rgba(250, 173, 20, 0);
	}

	100% {
		transform: scale(0.95);
		box-shadow: 0 0 0 0 rgba(250, 173, 20, 0);
	}
}

@keyframes pulse-green {
	0% {
		box-shadow: 0 0 0 0 rgba(103, 194, 58, 0.4);
	}

	70% {
		box-shadow: 0 0 0 6px rgba(103, 194, 58, 0);
	}

	100% {
		box-shadow: 0 0 0 0 rgba(103, 194, 58, 0);
	}
}

.floor-info {
	flex: 1;
	display: flex;
	flex-direction: column;
}

.floor-name {
	font-size: 16px;
	font-weight: 600;
	color: #333;
}

.floor-sub {
	font-size: 11px;
	color: #999;
}

.serial-grid-wrapper {
	margin-top: 12px;
	padding: 0 4px;
	animation: fadeIn 0.3s ease;
}

.serial-grid {
	display: grid;
	grid-template-columns: repeat(3, minmax(0, 1fr));
	gap: 8px;
}

.serial-item {
	width: 100%;
	box-sizing: border-box;
	overflow: hidden;
	background: #F5F7FA;
	border-radius: 8px;
	padding: 8px;
	height: 110px;
	display: flex;
	flex-direction: column;
	box-shadow: inset 0 2px 4px rgba(0, 0, 0, 0.06);
	border: 2px solid #E4E7ED;
	/* Use border here to avoid layout shift when highlighting */
	transition: all 0.3s;
	position: relative;

	&.expiry-warning {
		border: 1px solid #F56C6C !important;
		background-color: #FFF0F0;
	}
}

.highlight-glow {
	border-color: #FF4D4F;
	box-shadow: 0 0 15px rgba(255, 77, 79, 0.5);
	animation: pulse-glow 1.5s infinite;
	z-index: 10;
	position: relative;
	background: #fff;
}

@keyframes pulse-glow {
	0% {
		box-shadow: 0 0 5px rgba(255, 77, 79, 0.4);
		transform: scale(1);
	}

	50% {
		box-shadow: 0 0 20px rgba(255, 77, 79, 0.8), 0 0 10px rgba(255, 77, 79, 0.4);
		transform: scale(1.02);
	}

	100% {
		box-shadow: 0 0 5px rgba(255, 77, 79, 0.4);
		transform: scale(1);
	}
}

.serial-header {
	display: flex;
	justify-content: flex-start;
	align-items: center;
	margin-bottom: 4px;
}

.serial-no {
	font-size: 14px;
	font-weight: 900;
	color: #303133;
}

.item-detail {
	flex: 1;
	display: flex;
	flex-direction: column;
	justify-content: center;
}

.item-name-wrapper {
	width: 100%;
	height: 20px;
	overflow: hidden;
	white-space: nowrap;
	margin-bottom: 4px;
	position: relative;
}

.item-name-text {
	display: block;
	font-size: 13px;
	font-weight: 600;
	color: #303133;
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

.item-info-row {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-bottom: 2px;
}

.item-spec {
	font-size: 11px;
	color: #909399;
}

.item-unit {
	font-size: 10px;
	color: inherit;
	margin-left: 2px;
	font-weight: normal;
}

.item-stock {
	font-size: 11px;
	font-weight: bold;
	color: #666;
	padding: 0 4px;
	display: flex;
	align-items: baseline;
}

.item-cardinality {
	font-size: inherit;
	color: inherit;
	opacity: 0.8;
	margin-left: 2px;
	font-weight: normal;
}

.item-expiry {
	text-align: right;
	font-size: 10px;
}

.empty-slot {
	flex: 1;
	display: flex;
	align-items: center;
	justify-content: center;
}

.plus-icon {
	font-size: 28px;
	color: #DCDFE6;
}

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
	border-radius: 24px 24px 0 0;
	display: flex;
	flex-direction: column;
	animation: slideUp 0.3s ease;
	max-height: 85vh;
}

.modal-header {
	padding: 16px 24px;
	display: flex;
	justify-content: space-between;
	align-items: center;
	border-bottom: 1px solid #eee;
}

.title-section {
	display: flex;
	flex-direction: column;
}

.stock-warning {
	font-size: 12px;
	color: #FF4D4F;
	margin-top: 4px;
}

.stock-warning-label {
	font-size: 14px;
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
	font-size: 18px;
	font-weight: bold;
}

.close-btn {
	font-size: 20px;
	color: #999;
	padding: 8px;
}

.modal-body {
	padding: 24px;
}

.form-label {
	font-size: 14px;
	color: #666;
	margin-bottom: 8px;
	display: block;
}

.batch-scroll {
	width: 100%;
	white-space: nowrap;
	margin-bottom: 20px;
}

.batch-list-h {
	display: flex;
	gap: 10px;
}

.batch-item {
	padding: 10px;
	border: 1px solid #eee;
	border-radius: 8px;
	min-width: 120px;
	transition: all 0.2s;

	&.active {
		border-color: #FF4D4F;
		background: #FFF0F0;
	}

	&.disabled {
		opacity: 0.5;
	}
}

.batch-row {
	font-size: 12px;
	color: #333;
	margin-bottom: 2px;
}

.batch-no {
	font-size: 12px;
	font-weight: bold;
	color: #333;
}

.batch-expiry {
	font-size: 11px;
	color: #666;
}

.batch-stock {
	font-size: 11px;
	color: #666;
	font-weight: bold;
}

/* Qty Control Redesign */
.qty-control-row {
	display: flex;
	align-items: center;
	justify-content: center;
	padding: 16px 12px;
	margin-bottom: 8px;
	gap: 20px;
}

.qty-value-display {
	display: flex;
	flex-direction: column;
	align-items: center;
	min-width: 120px;
	/* Ensure visual stability */
}

.qty-val-group {
	display: flex;
	align-items: baseline;
	margin: 4px 0;
}

.label {
	font-size: 12px;
	color: #606266;
}

.qty-text {
	font-size: 32px;
	font-weight: bold;
	color: #303133;
	line-height: 1;
}

.unit-text {
	font-size: 12px;
	color: #909399;
	margin-left: 4px;
}

.stock-hint {
	font-size: 12px;
	color: #909399;
}

.qty-value-display.error .qty-text {
	color: #FF4D4F;
}

.q-btn-circle {
	width: 48px;
	height: 48px;
	border-radius: 50%;
	display: flex;
	align-items: center;
	justify-content: center;
	font-size: 16px;
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
	margin-top: 10px;
	display: flex;
}

.q-btn-full {
	flex: 1;
	height: 48px;
	border-radius: 8px;
	background: #F5F7FA;
	color: #909399;
	font-size: 16px;
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
	margin-top: 10px;
}

/* Old Quick Actions styles removed */

.modal-footer {
	padding: 16px 24px calc(16px + env(safe-area-inset-bottom)) 24px;
	border-top: 1px solid #eee;
}

.submit-btn {
	background: #FF4D4F;
	color: #fff;
	border-radius: 24px;
	height: 48px;
	font-size: 16px;
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
	height: 40px;
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
	margin-right: 8px;
}

.status-icon {
	width: 24px;
	height: 24px;
	border-radius: 50%;
	display: flex;
	align-items: center;
	justify-content: center;
	margin-left: 4px;
	background: #f5f7fa;
}

.status-icon.warning {
	background: #FEF0F0;
	border: 1px solid #F56C6C;
}

.status-icon.danger {
	background: #FDF6EC;
	border: 1px solid #E6A23C;
}

.status-bg-icon {
	width: 16px;
	height: 16px;
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
	width: 18px;
	height: 18px;
	background-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='%23F56C6C'%3E%3Cpath d='M12 22c1.1 0 2-.9 2-2h-4c0 1.1.9 2 2 2zm6-6v-5c0-3.07-1.63-5.64-4.5-6.32V4c0-.83-.67-1.5-1.5-1.5s-1.5.67-1.5 1.5v.68C7.64 5.36 6 7.92 6 11v5l-2 2v1h16v-1l-2-2z'/%3E%3C/svg%3E");
	background-size: contain;
	background-repeat: no-repeat;
	animation: bell-shake 1.2s infinite ease-in-out;
	margin-right: 4px;
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
</style>

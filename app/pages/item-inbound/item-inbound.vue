<template>
    <view class="container">
        <view class="main-content">

            <!-- Header Gradient Section matching Position Manage (Blue for Inbound) -->
            <view class="header-section" :style="{ paddingTop: (statusBarHeight + 10) + 'px' }">
                <view class="nav-bar">
                    <BackButton @click="goBack" />
                    <text class="page-title">{{ $t('itemInbound.title') || '物品入库' }}</text>
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
                                            <text :class="itemsMap[serial.ItemId].ExpiryClass">
                                                {{ itemsMap[serial.ItemId].FormattedDate }}
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
        </view>

        <BackToTop :scroll-top="scrollTop" @click="goTop" />

        <!-- Inbound Modal -->
        <view class="modal-mask" v-if="showModal" @click="closeModal">
            <view class="modal-content" @click.stop>
                <view class="modal-header">
                    <view class="title-section">
                        <text class="modal-title">{{ selectedItem?.Name }} - 入库</text>
                    </view>
                    <view class="close-btn" @click="closeModal">✕</view>
                </view>

                <view class="modal-body">
                    <!-- Tabs -->
                    <view class="tab-header">
                        <view class="tab-item" :class="{ active: inboundTab === 'inbound' }"
                            @click="inboundTab = 'inbound'">
                            <text class="tab-text">入库</text>
                            <view class="active-indicator" v-if="inboundTab === 'inbound'"></view>
                        </view>
                        <view class="tab-item" :class="{ active: inboundTab === 'newBatch' }"
                            @click="inboundTab = 'newBatch'">
                            <text class="tab-text">新增批次</text>
                            <view class="active-indicator" v-if="inboundTab === 'newBatch'"></view>
                        </view>
                    </view>

                    <!-- Inbound Tab Content -->
                    <view v-if="inboundTab === 'inbound'" class="tab-content">
                        <!-- Batch Selection -->
                        <view class="form-group" v-if="itemDetails.length > 0">
                            <text class="form-label">{{ $t('itemOutbound.batch_select') }}</text>
                            <scroll-view scroll-x class="batch-scroll">
                                <view class="batch-list-h">
                                    <view class="batch-item" v-for="(batch, idx) in itemDetails" :key="idx"
                                        :class="{ active: selectedBatchId === batch.Id, 'stock-warning': (!batch.Stock || batch.Stock <= 0), 'stock-insufficient': (batch.Stock > 0 && batch.Stock < (selectedItem.BaseStock || 0)) }"
                                        @click="selectBatch(batch)">
                                        <view class="batch-row">
                                            <text class="batch-no">批号: {{ batch.RegisterCode || '-' }}</text>
                                        </view>
                                        <view class="batch-row">
                                            <text class="batch-expiry">效期: {{ formatDate(batch.ExpirationDate)
                                                }}</text>
                                        </view>
                                        <view class="batch-row">
                                            <text class="batch-stock"
                                                :class="{ 'stock-zero': (!batch.Stock || batch.Stock <= 0), 'stock-insufficient-text': (batch.Stock > 0 && batch.Stock < (selectedItem.BaseStock || 0)) }">库存:
                                                {{ batch.Stock || 0 }}</text>
                                        </view>
                                    </view>
                                </view>
                            </scroll-view>
                        </view>

                        <view class="form-group" v-else>
                            <text class="form-label" style="color:#999; text-align: center;">无现有批次可选，将直接入库</text>
                        </view>

                        <!-- Quantity Control Row -->
                        <view class="qty-control-row">
                            <view class="q-btn-circle sub" @click.stop="quickAction('sub')">-1</view>

                            <view class="qty-value-display">
                                <text class="label">入库数量</text>
                                <view class="qty-val-group">
                                    <text class="qty-text">{{ inQty }}</text>
                                    <text class="unit-text">{{ selectedItem?.Unit }}</text>
                                </view>
                                <!-- Show current stock just as info -->
                                <text class="stock-hint">当前: {{ currentStock }}</text>
                            </view>

                            <view class="q-btn-circle add" @click.stop="quickAction('add')">+1</view>
                        </view>

                        <!-- Clear Button (Full Width) -->
                        <view class="clear-action-bar">
                            <view class="q-btn-full clear" @click.stop="quickAction('clear')">清空</view>
                        </view>

                        <!-- Keypad -->
                        <view class="keypad-wrapper">
                            <NumericKeypad @input="onKeypadInput" @delete="onKeypadDelete" />
                        </view>
                    </view>

                    <!-- New Batch Tab Content -->
                    <view v-else-if="inboundTab === 'newBatch'" class="tab-content form-content">
                        <view class="input-group">
                            <text class="input-label">批号 <text class="required">*</text></text>
                            <input class="input-field" v-model="newBatchForm.BatchNo" placeholder="请输入批号" />
                        </view>
                        <view class="input-group">
                            <text class="input-label">条码</text>
                            <view class="input-with-icon">
                                <input class="input-field" v-model="newBatchForm.Barcode" placeholder="请输入或扫描条码" />
                                <view class="scan-icon" @click="scanBarcode">
                                    {/* Using a simple text icon for now or an image if available */}
                                    <text style="font-size: 20px;">📷</text>
                                </view>
                            </view>
                        </view>
                        <view class="input-group">
                            <text class="input-label">过期日期 <text class="required">*</text></text>
                            <picker mode="date" :value="newBatchForm.ExpirationDate"
                                @change="onDateChange($event, 'ExpirationDate')">
                                <view class="picker-display" :class="{ empty: !newBatchForm.ExpirationDate }">
                                    <text>{{ newBatchForm.ExpirationDate || '请选择过期日期' }}</text>
                                    <view class="picker-clear-btn" v-if="newBatchForm.ExpirationDate"
                                        @click.stop="clearDate('ExpirationDate')">✕</view>
                                </view>
                            </picker>
                        </view>
                        <view class="input-group">
                            <text class="input-label">生产日期</text>
                            <picker mode="date" :value="newBatchForm.ProductionDate"
                                @change="onDateChange($event, 'ProductionDate')">
                                <view class="picker-display" :class="{ empty: !newBatchForm.ProductionDate }">
                                    <text>{{ newBatchForm.ProductionDate || '请选择生产日期 (可选)' }}</text>
                                    <view class="picker-clear-btn" v-if="newBatchForm.ProductionDate"
                                        @click.stop="clearDate('ProductionDate')">✕</view>
                                </view>
                            </picker>
                        </view>
                    </view>

                </view>

                <view class="modal-footer">
                    <button class="submit-btn" :disabled="!canSubmit"
                        @click="inboundTab === 'inbound' ? submitInbound() : submitNewBatch()">
                        {{ inboundTab === 'inbound' ? $t('itemInbound.confirm') : '确认新增批次' }}
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

            // Inbound Modal Data
            showModal: false,
            selectedItem: null,
            itemDetails: [], // Batches
            selectedBatchId: null,
            selectedBatch: null,
            inQtyInput: '1', // String for keypad input
            remark: '',

            // Tabs
            inboundTab: 'inbound', // 'inbound' or 'newBatch'

            // New Batch Form
            newBatchForm: {
                BatchNo: '',
                ProductionDate: '',
                ExpirationDate: '',
                Barcode: ''
            }
        };
    },
    computed: {
        inQty() {
            return parseInt(this.inQtyInput) || 0;
        },
        currentStock() {
            let stock = 0;
            if (this.selectedBatch) {
                stock = this.selectedBatch.Stock;
            } else if (this.selectedItem) {
                stock = this.selectedItem.AvailableStock;
            }
            return parseInt(stock) || 0;
        },
        canSubmit() {
            if (this.inboundTab === 'inbound') {
                // ID required if batches exist, but for pure inbound without batch selection (if empty), maybe allowed?
                // Current logic: qty > 0.
                return this.inQty > 0;
            } else {
                // New Batch
                return this.newBatchForm.BatchNo && this.newBatchForm.ExpirationDate;
            }
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
    onPageScroll(e) {
        this.scrollTop = e.scrollTop;
    },
    onPullDownRefresh() {
        this.isRefreshing = true;
        this.fetchData().then(() => {
            this.isRefreshing = false;
            uni.showToast({ title: '已刷新', icon: 'none' });
            uni.stopPullDownRefresh();
        });
    },
    onReachBottom() {
        // Placeholder logic if needed
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
            uni.pageScrollTo({
                scrollTop: 0,
                duration: 300
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
                        const item = res.rows[0];
                        // Pre-calculate properties for performance
                        const days = this.getDaysUntil(item.LatestExpirationDate);
                        item.FormattedDate = this.formatDate(item.LatestExpirationDate);
                        item.ExpiryClass = this.getExpiryClass(item.LatestExpirationDate);
                        item.IsStockInsufficient = ((item.AvailableStock || 0) < (item.Cardinality || 0)) || (item.AvailableStock || 0) <= 0;
                        item.IsExpiryWarning = days < 30;

                        this.$set(this.itemsMap, id, item);
                    }
                } catch (e) { }
            }
        },
        getSerialsByFloor(floorId) {
            return this.floorSerialsMap[floorId] || [];
        },
        getOccupiedCount(floor) {
            const serials = this.getSerialsByFloor(floor.Id);
            return serials.filter(s => s.ItemId && s.ItemId > 0).length;
        },
        getFloorStatusClass(floor) {
            // Simplified status
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

            if (serial.ItemId && serial.ItemId > 0) {
                const item = this.itemsMap[serial.ItemId];
                if (item) {
                    this.openInboundModal(item);
                }
            } else {
                uni.showToast({ title: '请选择已绑定物品的位置进行入库', icon: 'none' });
            }
        },

        // Modal & Inbound Logic
        openInboundModal(item) {
            this.selectedItem = item;
            this.selectedBatchId = null;
            this.selectedBatch = null;
            this.inQtyInput = '1';
            this.itemDetails = [];
            this.inboundTab = 'inbound';
            this.newBatchForm = { BatchNo: '', ProductionDate: '', ExpirationDate: '', Barcode: '' };
            this.showModal = true;
            this.fetchBatchDetails(item.Id);
        },
        fetchBatchDetails(itemId) {
            request.post('api/ssbh/ItemDetail/GetPageData', {
                page: 1, rows: 100, sort: 'ExpirationDate', order: 'asc',
                wheres: JSON.stringify([
                    { name: 'ItemId', value: itemId }
                    // Do NOT filter by Stock > 0 for inbound, we can add to empty batches? 
                    // Or usually we assume batches with 0 stock might be hidden?
                    // Let's show all batches to allow refill.
                ])
            }).then(res => {
                if (res && res.rows) {
                    this.itemDetails = res.rows;
                    if (this.itemDetails.length > 0) {
                        // Default select one? Maybe last one or first?
                        // Outbound selected first (nearest expiry). Inbound maybe best to select first too.
                        this.selectBatch(this.itemDetails[0]);
                    }
                }
            });
        },
        selectBatch(batch) {
            this.selectedBatchId = batch.Id;
            this.selectedBatch = batch;
            this.inQtyInput = '1';
        },
        closeModal() { this.showModal = false; },

        // Keypad Logic
        onKeypadInput(val) {
            if (this.inQtyInput === '0') this.inQtyInput = val; // Replace leading zero
            // Limit length
            else if (this.inQtyInput.length < 5) {
                this.inQtyInput += val;
            }
        },
        onKeypadDelete() {
            if (this.inQtyInput.length > 1) {
                this.inQtyInput = this.inQtyInput.slice(0, -1);
            } else {
                this.inQtyInput = '0'; // Don't make it empty
            }
        },
        quickAction(type) {
            let val = this.inQty;
            // No max limit for inbound

            if (type === 'add') {
                val++;
                this.inQtyInput = val.toString();
            } else if (type === 'sub') {
                if (val > 1) {
                    val--;
                    this.inQtyInput = val.toString();
                } else {
                    this.inQtyInput = '0';
                }
            } else if (type === 'clear') {
                this.inQtyInput = '0';
            }
        },

        submitInbound() {
            if (!this.canSubmit) return;
            const payload = {
                ItemId: this.selectedItem.Id,
                ItemDetailId: this.selectedBatchId,
                ChangeCounter: this.inQty, // Positive for Inbound
                Stock: this.selectedItem.AvailableStock + this.inQty,
                Remark: 'Quick Inbound',
                CreatorId: this.userOrgInfo.UserId || 0,
                Creator: this.userOrgInfo.UserName || 'User'
            };

            uni.showLoading({ title: '提交中' });
            request.post('api/ssbh/ItemHistory/Add', { mainData: payload }).then(res => {
                uni.hideLoading();
                if (res.success || res.status) {
                    uni.showToast({ title: '入库成功', icon: 'success' });

                    // Local Update Logic
                    const qty = this.inQty;

                    // Update Item Stock
                    if (this.selectedItem) {
                        this.selectedItem.AvailableStock = (this.selectedItem.AvailableStock || 0) + qty;
                        // Update in Map
                        if (this.itemsMap[this.selectedItem.Id]) {
                            this.itemsMap[this.selectedItem.Id].AvailableStock = this.selectedItem.AvailableStock;
                        }
                    }

                    // Update Batch Stock
                    if (this.selectedBatchId) {
                        const batch = this.itemDetails.find(b => b.Id === this.selectedBatchId);
                        if (batch) {
                            batch.Stock = (batch.Stock || 0) + qty;

                            // Update Item Expiry if this batch matches or is earlier
                            if (batch.ExpirationDate) {
                                const batchExpiry = new Date(batch.ExpirationDate).getTime();
                                const currentLatest = this.selectedItem.LatestExpirationDate ? new Date(this.selectedItem.LatestExpirationDate).getTime() : null;

                                if (!currentLatest || batchExpiry < currentLatest) {
                                    this.selectedItem.LatestExpirationDate = batch.ExpirationDate;
                                    if (this.itemsMap[this.selectedItem.Id]) {
                                        this.itemsMap[this.selectedItem.Id].LatestExpirationDate = batch.ExpirationDate;
                                    }
                                }
                            }
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
                                    let scrollDiff = data.top - (windowHeight / 2) + (data.height / 2);
                                    let targetTop = this.scrollTop + scrollDiff;
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
        onDateChange(e, field) {
            this.newBatchForm[field] = e.detail.value;
        },
        clearDate(field) {
            this.newBatchForm[field] = '';
        },
        scanBarcode() {
            uni.scanCode({
                success: (res) => {
                    if (res && res.result) {
                        this.newBatchForm.Barcode = res.result;
                    }
                },
                fail: (err) => {
                    console.error('Scan failed', err);
                    // Handle cancellation or error silently or toast
                }
            });
        },
        async submitNewBatch() {
            if (!this.canSubmit) return;
            if (!this.selectedItem) return;

            uni.showLoading({ title: '保存中' });
            const payload = {
                ItemId: this.selectedItem.Id,
                RegisterCode: this.newBatchForm.BatchNo,
                Barcode: this.newBatchForm.Barcode,
                StartDate: this.newBatchForm.ProductionDate ? this.newBatchForm.ProductionDate : null,
                ExpirationDate: this.newBatchForm.ExpirationDate,
                Stock: 0 // Initial stock 0, will inbound next
            };
            // Assuming ItemDetail/Add exists. The user did not specify exact API, but ItemDetail represents a batch.
            // If Add not available, might be Save with Id=0.
            try {
                const res = await request.post('api/ssbh/ItemDetail/Add', { mainData: payload });
                uni.hideLoading();
                if (res.success || res.status) {
                    uni.showToast({ title: '添加批次成功', icon: 'success' });
                    // Refresh batch list
                    await this.fetchBatchDetails(this.selectedItem.Id);
                    // If we got an ID back, select it? 
                    // Usually Create returns the ID or the object.
                    // Assuming we refresh and select the newest one (or matching BatchNo).
                    // For simplicity, just switch tab and user selects (or auto select if we implemented that).
                    this.inboundTab = 'inbound';
                    // Clear form
                    this.newBatchForm = { BatchNo: '', ProductionDate: '', ExpirationDate: '' };
                } else {
                    uni.showToast({ title: res.msg || '保存失败', icon: 'none' });
                }
            } catch (e) {
                uni.hideLoading();
                console.error(e);
                uni.showToast({ title: '保存异常', icon: 'none' });
            }
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
            if (days <= 90) return 'text-yellow';
            return 'text-normal';
        },
        isStockInsufficient(item) {
            if (!item) return false;
            // Use cached property if available
            if (typeof item.IsStockInsufficient !== 'undefined') return item.IsStockInsufficient;

            const stock = item.AvailableStock || 0;
            const base = item.Cardinality || 0;
            return stock < base || stock === 0;
        },
        hasExpiryProblem(floor) {
            const serials = this.getSerialsByFloor(floor.Id); // Uses cached map
            return serials.some(s => {
                if (!s.ItemId) return false;
                const item = this.itemsMap[s.ItemId];
                return item && this.isExpiryWarning(item);
            });
        },
        hasStockProblem(floor) {
            const serials = this.getSerialsByFloor(floor.Id); // Uses cached map
            return serials.some(s => {
                if (!s.ItemId) return false;
                const item = this.itemsMap[s.ItemId];
                return item && this.isStockInsufficient(item);
            });
        },
        isExpiryWarning(item) {
            if (!item) return false;
            if (typeof item.IsExpiryWarning !== 'undefined') return item.IsExpiryWarning;

            if (!item.LatestExpirationDate) return false;
            const days = this.getDaysUntil(item.LatestExpirationDate);
            return days < 30;
        },
        shouldShowAlert(item) {
            if (!item) return false;
            // 1. Stock Insufficient
            if (this.isStockInsufficient(item)) return true;
            // 2. Expiry < 30 days
            if (this.isExpiryWarning(item)) return true;
            return false;
        }
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
    height: 200px;
    // Blue Gradient for Inbound
    background: linear-gradient(135deg, #A1C4FD 0%, #C2E9FB 100%);
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
        background: #E6F7FF;
        /* Blue for Inbound */
        color: #1890FF;
        border: 1px solid #1890FF;

        .card-name {
            color: #1890FF;
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

.floor-icon {
    display: flex;
    align-items: center;
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
    border: 1px solid #E4E7ED;
    transition: all 0.3s;
    position: relative;
    /* Added for alert bell positioning */

    &.highlight-glow {
        border-color: #1890FF;
        box-shadow: 0 0 10px rgba(24, 144, 255, 0.5);
        animation: glow 1.5s infinite alternate;
    }

    &.expiry-warning {
        border: 1px solid #F56C6C !important;
        background-color: #FFF0F0;
    }
}

@keyframes glow {
    from {
        box-shadow: 0 0 5px rgba(24, 144, 255, 0.3);
    }

    to {
        box-shadow: 0 0 15px rgba(24, 144, 255, 0.7);
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
    position: relative;
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

.item-info-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 2px;
}

.item-spec {
    font-size: 11px;
    color: $sull-text-secondary;
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
    color: $sull-text-secondary;
    padding: 0 4px;

    display: flex;
    align-items: baseline;
}

.text-red-bold {
    color: $sull-color-error !important;
    background: rgba(245, 108, 108, 0.1) !important;
}

.item-cardinality {
    font-size: inherit;
    color: inherit;
    opacity: 0.8;
    margin-left: 2px;
    font-weight: normal;
}

.item-expiry {
    font-size: 11px;
    text-align: right;
}

// Colors
.text-red {
    color: $sull-color-error;
}

.text-yellow {
    color: $sull-color-warning;
}

.text-normal {
    color: $sull-text-secondary;
}

.text-black {
    color: #000;
    font-weight: bold;
}

.empty-slot {
    flex: 1;
    display: flex;
    align-items: center;
    justify-content: center;
}

.plus-icon {
    color: #CCD0D6;
    font-size: 24px;
    font-weight: 300;
}

// Modal Styles
.modal-mask {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.5);
    z-index: 900;
    display: flex;
    align-items: flex-end;
}

.modal-content {
    width: 100%;
    background: $sull-color-white;
    border-radius: 24px 24px 0 0;
    display: flex;
    flex-direction: column;
    animation: slideUp 0.3s ease;
    max-height: 95vh;
}

.modal-header {
    padding: 16px 24px;
    display: flex;
    justify-content: space-between;
    align-items: center;
    border-bottom: 1px solid $sull-color-line;
}

.title-section {
    display: flex;
    flex-direction: column;
}

.modal-title {
    font-size: 18px;
    font-weight: bold;
}

.close-btn {
    font-size: 20px;
    color: $sull-text-secondary;
    padding: 8px;
}

.modal-body {
    padding: 0px 24px;
}

.form-label {
    font-size: 14px;
    color: $sull-text-secondary;
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
    border: 1px solid $sull-color-line;
    border-radius: 8px;
    min-width: 120px;
    transition: all 0.2s;

    &.active {
        border-color: $sull-color-primary;
        background: #E6F7FF;
    }

    &.disabled {
        opacity: 0.5;
    }
}

.batch-row {
    font-size: 12px;
    color: $sull-text-main;
    margin-bottom: 2px;
}

.batch-no {
    font-size: 12px;
    font-weight: bold;
    color: $sull-text-main;
}

.batch-expiry {
    font-size: 11px;
    color: $sull-text-secondary;
}

.batch-stock {
    font-size: 11px;
    color: $sull-text-secondary;
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
}

.qty-val-group {
    display: flex;
    align-items: baseline;
    margin: 4px 0;
}

.label {
    font-size: 12px;
    color: $sull-text-secondary;
}

.qty-text {
    font-size: 32px;
    font-weight: bold;
    color: $sull-text-main;
    line-height: 1;
}

.unit-text {
    font-size: 12px;
    color: $sull-text-secondary;
    margin-left: 4px;
}

.stock-hint {
    font-size: 12px;
    color: $sull-text-secondary;
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
    color: $sull-text-secondary;

    &:active {
        opacity: 0.8;
        transform: scale(0.95);
    }
}

.q-btn-circle.sub {
    background: #FFF1F0;
    color: $sull-color-error;
}

.q-btn-circle.add {
    background: #E6F7FF;
    color: $sull-color-primary;
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
    color: $sull-text-secondary;
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


.modal-footer {
    padding: 16px 24px calc(16px + env(safe-area-inset-bottom)) 24px;
    border-top: 1px solid $sull-color-line;
}

.submit-btn {
    background: $sull-color-primary;
    /* Blue for Inbound */
    color: $sull-color-white;
    font-size: 16px;
    height: 44px;
    line-height: 44px;
    border-radius: 22px;
    font-weight: 600;

    &:active {
        opacity: 0.9;
    }

    &[disabled] {
        background: #ccc;
        color: $sull-color-white;
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
        transform: translateY(-10px);
    }

    to {
        opacity: 1;
        transform: translateY(0);
    }
}

@keyframes strong-pulse-red {
    0% {
        box-shadow: 0 0 0 0 rgba(255, 77, 79, 0.4);
    }

    70% {
        box-shadow: 0 0 0 6px rgba(255, 77, 79, 0);
    }

    100% {
        box-shadow: 0 0 0 0 rgba(255, 77, 79, 0);
    }
}

/* Tabs - Mirrored from Index Page */
.tab-header {
    background: #FAFAFA;
    border-radius: 25px;
    padding: 4px;
    margin-bottom: 20px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.02);
    display: flex;
    justify-content: space-between;
    border-bottom: none;
    /* Remove old border */
}

.tab-item {
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
    padding: 0;
    /* Override old padding */
    position: static;
    /* Remove relative */
}

.tab-item.active {
    background: $sull-color-primary;
    color: $sull-color-white;
    font-weight: 600;
    box-shadow: 0 2px 6px rgba(24, 144, 255, 0.4);
}

.active-indicator {
    display: none;
    /* Remove old underline indicator */
}

/* Form Styles */
.form-content {
    padding: 0 10px;
}

.input-group {
    margin-bottom: 20px;
    display: flex;
    flex-direction: column;
}

.input-label {
    font-size: 14px;
    color: $sull-text-secondary;
    margin-bottom: 8px;
}

.input-with-icon {
    position: relative;
    display: flex;
    align-items: center;
}

.input-with-icon .input-field {
    flex: 1;
    padding-right: 40px;
    /* Make space for icon */
}

.scan-icon {
    position: absolute;
    right: 10px;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 10;
}

.required {
    color: $sull-color-error;
    margin-left: 4px;
}

/* Stock Warning Styles */
.batch-item.stock-warning {
    border: 2px solid $sull-color-error !important;
    /* Zero - Red */
    background-color: #FFF0F0;
}

.batch-item.stock-insufficient {
    border: 2px solid $sull-color-warning !important;
    /* Insufficient - Orange */
    background-color: #FDF6EC;
}

.stock-zero {
    color: $sull-color-error;
    font-weight: bold;
}

.stock-insufficient-text {
    color: $sull-color-warning;
    font-weight: bold;
}

.input-field {
    height: 44px;
    background: #F5F7FA;
    border-radius: 8px;
    padding: 0 12px;
    font-size: 14px;
    color: $sull-text-main;
    border: 1px solid transparent;
}

.input-field:focus {
    border-color: $sull-color-primary;
    background: $sull-color-white;
}

.picker-display {
    height: 44px;
    background: #F5F7FA;
    border-radius: 8px;
    padding: 0 12px;
    font-size: 14px;
    color: $sull-text-main;
    display: flex;
    align-items: center;
    justify-content: space-between;
    /* Adjusted */
}

.picker-display.empty {
    color: $sull-text-secondary;
}

.picker-clear-btn {
    padding: 4px 8px;
    color: $sull-text-secondary;
    font-size: 14px;
    display: flex;
    align-items: center;
    justify-content: center;
}

/* Alerts */
.text-red-bold {
    color: $sull-color-error !important;
    font-weight: bold;
}

.alert-bell {
    /* position: absolute; removed */
    /* top: -5px; removed */
    /* right: -5px; removed */
    width: 18px;
    height: 18px;
    background-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='%23F56C6C'%3E%3Cpath d='M12 22c1.1 0 2-.9 2-2h-4c0 1.1.9 2 2 2zm6-6v-5c0-3.07-1.63-5.64-4.5-6.32V4c0-.83-.67-1.5-1.5-1.5s-1.5.67-1.5 1.5v.68C7.64 5.36 6 7.92 6 11v5l-2 2v1h16v-1l-2-2z'/%3E%3C/svg%3E");
    background-size: contain;
    background-repeat: no-repeat;
    animation: bell-shake 1.2s infinite ease-in-out;
    margin-right: 4px;
    /* Added spacing */
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
</style>

/* Page Scroll Fix */
page {
min-height: 100%;
height: auto;
overflow-y: auto;
background-color: $sull-color-background;
}

.container {
display: flex;
flex-direction: column;
min-height: 100vh;
height: auto;
width: 100%;
overflow: visible;
background-color: $sull-color-background;
}

.main-content {
flex: 1;
width: 100%;
}

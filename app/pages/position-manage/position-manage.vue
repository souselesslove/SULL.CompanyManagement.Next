<template>
    <view class="container">
        <!-- Navigation Bar Removed - Using Header Section -->

        <scroll-view class="main-scroll" scroll-y @scroll="onScroll" :scroll-top="scrollTop" lower-threshold="50"
            @scrolltolower="onLoadMore" refresher-enabled @refresherrefresh="onRefresh"
            :refresher-triggered="isRefreshing">

            <!-- Header Gradient Section matching Add Car page -->
            <view class="header-section" :style="{ paddingTop: (statusBarHeight + 10) + 'px' }">
                <view class="nav-bar">
                    <BackButton @click="goBack" />
                    <text class="page-title">抢救车管理</text>
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
                            <view class="fixed-add-btn" @click="goAdd(10)">
                                <text class="add-icon">+</text>
                            </view>

                            <view class="h-card" v-for="(item, index) in rescueCarList" :key="item.Id"
                                :class="{ active: selectedRescueCarId === item.Id }" @click="selectRescueCar(item.Id)">
                                <text class="card-name">{{ item.DisplayName }}</text>
                            </view>
                        </view>
                    </scroll-view>
                </view>
                <view class="empty-placeholder" v-else>
                    暂无抢救车，请添加
                    <view class="fixed-add-btn" @click="goAdd(10)">
                        <text class="add-icon">+</text>
                    </view>
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

                            <!-- Action Button -->
                            <view class="floor-action" @click.stop="showFloorActions(floor)">
                                <text class="action-icon">⋮</text>
                            </view>
                        </view>

                        <!-- Expanded Details (Serials) -->
                        <view class="serial-grid-wrapper" v-if="floor.expanded">
                            <view class="serial-grid">
                                <view class="serial-item" v-for="serial in getSerialsByFloor(floor.Id)" :key="serial.Id"
                                    :class="{ 'expiry-warning': serial.ItemId > 0 && itemsMap[serial.ItemId] && isExpiryWarning(itemsMap[serial.ItemId]) }"
                                    @click="editSerial(serial)">
                                    <view class="serial-header">
                                        <text class="serial-no">{{ serial.SerialId || serial.OrderBy }}</text>
                                        <view class="serial-action" @click.stop="showSerialActions(serial)">
                                            <text>⋮</text>
                                        </view>
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
                                            <text :class="getExpiryClass(itemsMap[serial.ItemId].LatestExpirationDate)">
                                                {{ formatDate(itemsMap[serial.ItemId].LatestExpirationDate) }}
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
                    <!-- Add Button Module -->
                    <view v-if="false" class="add-floor-btn" @click="goAdd(20)">
                        <text class="add-text">+ 添加分层</text>
                    </view>
                </view>
                <view style="height: 40px;"></view>
            </view>

            <view class="bottom-spacer"></view>
            <FooterInfo />
        </scroll-view>

        <BackToTop :scroll-top="scrollTop" @click="goTop" />

        <!-- Custom Action Sheet (Floors) -->
        <view class="action-sheet-overlay" v-if="showActionSheetModal" @click="closeActionSheet">
            <view class="action-sheet-panel" @click.stop>
                <view class="action-item" @click="handleActionSheetItem(0)">修改名称</view>
                <view class="action-item" @click="handleActionSheetItem(1)">修改分隔</view>
                <view class="action-item" @click="handleActionSheetItem(2)">一键补库</view>
                <view class="action-cancel-gap"></view>
                <view class="action-item cancel" @click="closeActionSheet">取消</view>
            </view>
        </view>

        <!-- Custom Action Sheet (Serials) -->
        <view class="action-sheet-overlay" v-if="showSerialActionSheetModal" @click="closeSerialActionSheet">
            <view class="action-sheet-panel" @click.stop>
                <view class="action-item" @click="handleSerialActionItem(0)">扫码绑定</view>
                <view class="action-item" @click="handleSerialActionItem(1)">手工绑定</view>
                <view class="action-item" @click="handleSerialActionItem(2)">取消绑定</view>
                <view class="action-item" @click="handleSerialActionItem(3)">说明书</view>
                <view class="action-cancel-gap"></view>
                <view class="action-item cancel" @click="closeSerialActionSheet">取消</view>
            </view>
        </view>

        <!-- Custom Rename Modal -->
        <view class="modal-overlay" v-if="showRenameModalDialog" @click.self="closeRenameDialog">
            <view class="modal-content">
                <view class="modal-header">
                    <text class="modal-title">修改名称</text>
                    <text class="close-btn" @click="closeRenameDialog">✖️</text>
                </view>
                <view class="modal-body">
                    <view class="form-item">
                        <text class="label">新的名称</text>
                        <view class="input-wrapper">
                            <input class="input-field" v-model="tempNewName" placeholder="请输入新的分层名称" />
                            <view class="clear-icon" v-if="tempNewName" @click="clearTempName">
                                <text>✖</text>
                            </view>
                        </view>
                    </view>
                </view>
                <view class="modal-footer">
                    <button class="modal-btn cancel" @click="closeRenameDialog">取消</button>
                    <button class="modal-btn confirm" @click="confirmRename">确定</button>
                </view>
            </view>
        </view>

        <!-- Custom Split Modal -->
        <view class="modal-overlay" v-if="showSplitModalDialog" @click.self="closeSplitDialog">
            <view class="modal-content">
                <view class="modal-header">
                    <text class="modal-title">修改分隔</text>
                    <text class="close-btn" @click="closeSplitDialog">✖️</text>
                </view>
                <view class="modal-body">
                    <view class="form-item">
                        <text class="label">分隔数量</text>
                        <view class="input-wrapper">
                            <input class="input-field" type="number" v-model="tempSplitCount" placeholder="请输入分隔数量" />
                            <view class="clear-icon" v-if="tempSplitCount" @click="tempSplitCount = ''">
                                <text>✖</text>
                            </view>
                        </view>
                    </view>
                </view>
                <view class="modal-footer">
                    <button class="modal-btn cancel" @click="closeSplitDialog">取消</button>
                    <button class="modal-btn confirm" @click="confirmSplit">确定</button>
                </view>
            </view>
        </view>

        <!-- Custom Manual Modal -->
        <view class="modal-overlay" v-if="showManualDialog" @click.self="closeManualDialog">
            <view class="modal-content manual-modal">
                <view class="modal-header">
                    <text class="modal-title">物品说明书</text>
                    <text class="close-btn" @click="closeManualDialog">✖️</text>
                </view>
                <scroll-view scroll-y class="modal-body manual-body">
                    <text class="manual-text">{{ itemRemarkContent }}</text>
                </scroll-view>
                <view class="modal-footer">
                    <button class="modal-btn confirm" @click="closeManualDialog" style="border-radius:0;">关闭</button>
                </view>
            </view>
        </view>
    </view>
</template>

<script>
import BackButton from '@/components/BackButton/BackButton.vue';
import FooterInfo from '@/components/FooterInfo/FooterInfo.vue';
import BackToTop from '@/components/BackToTop/BackToTop.vue';
import request from '@/utils/request.js';

export default {
    components: { BackButton, FooterInfo, BackToTop },
    data() {
        return {
            statusBarHeight: 20,
            scrollTop: 0,
            oldScrollTop: 0,
            isRefreshing: false,

            // Data Lists
            rescueCarList: [],
            allFloorList: [],
            floorList: [],
            serialList: [],

            // Selections
            selectedRescueCarId: null,

            // Items Cache
            itemsMap: {},
            userOrgInfo: {},

            // Custom Modals
            showActionSheetModal: false,
            showSerialActionSheetModal: false,
            showRenameModalDialog: false,
            showSplitModalDialog: false,
            tempFloor: null,
            tempSerial: null, // Current serial for action
            tempNewName: '',
            tempSplitCount: '',

            // Search
            searchText: '',

            // Item Manual
            showManualDialog: false,
            itemRemarkContent: '',

            // Filter
            currentFilter: null
        };
    },
    onLoad(options) {
        const sysInfo = uni.getSystemInfoSync();
        this.statusBarHeight = sysInfo.statusBarHeight || 20;
        this.userOrgInfo = uni.getStorageSync('userOrgInfo') || {};

        if (options && options.filter) {
            this.currentFilter = options.filter; // 'AbnormalStock' or 'AbnormalExpiry'
        }

        this.fetchData();
    },
    methods: {
        goBack() {
            uni.navigateBack();
        },
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
        async fetchData() {
            await this.fetchRescueCars();
        },

        // API Calls
        // Category 0: Warehouse
        async fetchRescueCars() {
            try {
                if (!this.userOrgInfo.Organization || !this.userOrgInfo.Department) return;

                const res = await request.post('api/ssbh/Position/GetPageData', {
                    page: 1, rows: 100,
                    wheres: JSON.stringify([
                        { name: 'Category', value: "10,100", DisplayType: 'in' },
                        { name: 'OrganizationId', value: this.userOrgInfo.Organization.Id },
                        { name: 'DepartmentId', value: this.userOrgInfo.Department.Id }
                    ]),
                    sort: 'OrderBy', order: 'asc'
                });
                this.rescueCarList = (res.rows || []).filter(w => w.Category === 10);
                this.allFloorList = (res.rows || []).filter(w => w.Category === 100);
                if (this.rescueCarList.length > 0) {
                    // Default select first
                    if (!this.selectedRescueCarId || !this.rescueCarList.find(w => w.Id === this.selectedRescueCarId)) {
                        this.selectRescueCar(this.rescueCarList[0].Id);
                    } else {
                        // Refresh current selection
                        this.selectRescueCar(this.selectedRescueCarId);
                    }
                } else {
                    this.rescueCarList = [];
                    this.allFloorList = [];
                    this.floorList = [];
                    this.selectedRescueCarId = null;
                }
            } catch (e) {
                console.error(e);
            }
        },

        selectRescueCar(id) {
            this.selectedRescueCarId = id;
            this.fetchFloors(id);
        },

        // Category 20: Floor (Filter by ParentId which is RescueCarId)
        async fetchFloors(parentId) {
            if (!parentId) return;
            try {
                this.floorList = this.allFloorList.filter(f => f.WarehouseId === parentId);
                if (this.floorList.length > 0) {
                    // Gather Floor IDs and fetch serials
                    const floorIds = this.floorList.map(f => f.Id);

                    if (this.currentFilter) {
                        await this.fetchSerialsByFloors(floorIds);
                        // Expand all floors if filtering
                        this.floorList.forEach(f => this.$set(f, 'expanded', true));
                    } else {
                        this.fetchSerialsByFloors(floorIds);
                    }
                } else {
                    this.serialList = [];
                }
            } catch (e) { console.error(e); }
        },

        async fetchSerialsByFloors(floorIds) {
            if (!floorIds || floorIds.length === 0) return;
            try {
                // If backend supports IN for wheres name/value... usually value array works.
                const res = await request.post('api/ssbh/Position/GetPageData', {
                    page: 1, rows: 500,
                    wheres: JSON.stringify([
                        { name: 'Category', value: 110 },
                        { name: 'ParentId', value: floorIds.join(','), DisplayType: 'in' }
                    ]),
                    sort: 'OrderBy', order: 'asc'
                });
                this.serialList = res.rows || [];

                // Collect ItemIds
                const itemIds = this.serialList.filter(s => s.ItemId > 0).map(s => s.ItemId);
                if (itemIds.length > 0) {
                    this.fetchItemDetails([...new Set(itemIds)]);
                }
            } catch (e) { console.error(e); }
        },

        getSerialsByFloor(floorId) {
            let list = this.serialList.filter(s => s.FloorId === floorId);

            if (this.currentFilter === 'AbnormalStock') {
                list = list.filter(s => {
                    const item = this.itemsMap[s.ItemId];
                    // Stock Warning: stock < cardinality or stock <= 0
                    return item && this.isStockInsufficient(item);
                });
            } else if (this.currentFilter === 'AbnormalExpiry') {
                list = list.filter(s => {
                    const item = this.itemsMap[s.ItemId];
                    if (!item) return false;
                    const days = this.getDaysUntil(item.LatestExpirationDate);
                    return days < 30;
                });
            }

            return list;
        },

        async fetchItemDetails(ids) {
            const distinctIds = [...new Set(ids)];
            for (let id of distinctIds) {
                if (this.itemsMap[id]) continue; // Skip if cached
                try {
                    const res = await request.post('api/ssbh/Item/GetPageData', {
                        page: 1, rows: 1,
                        wheres: JSON.stringify([{ name: 'Id', value: id }, { name: 'OrganizationId', value: this.userOrgInfo.Organization.Id },
                        { name: 'DepartmentId', value: this.userOrgInfo.Department.Id }])
                    });
                    if (res.rows && res.rows.length > 0) {
                        this.$set(this.itemsMap, id, res.rows[0]);
                    }
                } catch (e) { }
            }
        },

        goAdd(category) {
            let parentId = 0;
            // category 10 = rescue car, 100 = floor
            if (category === 100) {
                if (!this.selectedRescueCarId) return uni.showToast({ title: '请选择抢救车', icon: 'none' });
                parentId = this.selectedRescueCarId;
            }

            uni.navigateTo({
                url: `/pages/position-manage/position-add?category=${category}&parentId=${parentId}`
            });
        },
        editSerial(serial) {
            // Navigate to edit serial or add item to serial
            // user didn't specify interaction, just display.
            // But probably want to edit.
        },

        formatDate(dateStr) {
            if (!dateStr) return '';
            return dateStr.substring(0, 10);
        },
        getDaysUntil(dateStr) {
            if (!dateStr) return 9999;
            const now = new Date();
            const target = new Date(dateStr);
            const diff = target.getTime() - now.getTime();
            return Math.ceil(diff / (1000 * 3600 * 24));
        },
        getExpiryClass(dateStr) {
            const days = this.getDaysUntil(dateStr);
            if (days < 0) return 'text-black';
            if (days < 30) return 'text-red';
            if (days <= 90) return 'text-yellow';
            return 'text-normal';
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
                        { name: 'OrganizationId', value: this.userOrgInfo.Organization.Id },
                        { name: 'DepartmentId', value: this.userOrgInfo.Department.Id },
                        { name: 'WarehouseId', value: floor.WarehouseId }, // Assuming WarehouseId is present on Floor object
                        { name: 'FloorId', value: floor.Id }
                    ]),
                    sort: 'SerialId', order: 'asc'
                });

                const newSerials = res.rows || [];

                // Merge serials: Remove old ones for this floor and add new ones
                const otherSerials = this.serialList.filter(s => s.FloorId !== floor.Id);
                this.serialList = [...otherSerials, ...newSerials];

                // Fetch item details
                const itemIds = newSerials.filter(s => s.ItemId > 0).map(s => s.ItemId);
                if (itemIds.length > 0) {
                    await this.fetchItemDetails([...new Set(itemIds)]);
                }

                this.$set(floor, 'expanded', true);
            } catch (e) {
                console.error("Refresh Floor Serials Error", e);
                uni.showToast({ title: '加载失败', icon: 'none' });
            } finally {
                uni.hideLoading();
            }
        },
        getFloorStatusClass(floor) {
            const serials = this.getSerialsByFloor(floor.Id);
            if (!serials || serials.length === 0) return 'blink-green'; // Default green if empty? Or gray? User implies normal is green.

            let hasExpiryWarning = false;
            let hasStockWarning = false;

            for (let s of serials) {
                if (s.ItemId > 0 && this.itemsMap[s.ItemId]) {
                    const item = this.itemsMap[s.ItemId];
                    // Check Expiry (Red)
                    const days = this.getDaysUntil(item.ExpiryDate);
                    if (days < 30) {
                        hasExpiryWarning = true;
                        break; // Highest priority
                    }
                    // Check Stock (Orange)
                    if (item.AvailableStock < item.Cardinality || item.AvailableStock <= 0) { // Assuming MinStock exists or just 0
                        hasStockWarning = true;
                    }
                }
            }

            if (hasExpiryWarning) return 'blink-red';
            if (hasStockWarning) return 'blink-orange';
            return 'blink-green';
        },
        showFloorActions(floor) {
            this.tempFloor = floor;
            this.showActionSheetModal = true;
        },
        closeActionSheet() {
            this.showActionSheetModal = false;
        },
        handleActionSheetItem(index) {
            this.closeActionSheet();
            if (index === 0) {
                this.openRenameDialog();
            } else if (index === 1) {
                this.editFloorLayout(this.tempFloor);
            } else if (index === 2) {
                this.restockFloor(this.tempFloor);
            }
        },
        openRenameDialog() {
            this.tempNewName = this.tempFloor.DisplayName || this.tempFloor.Name;
            this.showRenameModalDialog = true;
        },
        clearTempName() {
            this.tempNewName = '';
        },
        closeRenameDialog() {
            this.showRenameModalDialog = false;
        },
        confirmRename() {
            if (this.tempNewName && this.tempFloor) {
                this.updateFloorName(this.tempFloor.Id, this.tempNewName);
            }
            this.closeRenameDialog();
        },
        async updateFloorName(id, newName) {
            try {
                // Generic Update API call
                await request.post('api/ssbh/Position/Update', {
                    mainData: { Id: id, Name: newName, DisplayName: newName }
                });

                // Update Local Data Immediate Reflection
                const floorInAll = this.allFloorList.find(f => f.Id === id);
                if (floorInAll) {
                    floorInAll.Name = newName;
                    floorInAll.DisplayName = newName;
                }
                const floorInList = this.floorList.find(f => f.Id === id);
                if (floorInList) {
                    floorInList.Name = newName;
                    floorInList.DisplayName = newName;
                }

                uni.showToast({ title: '修改成功' });
            } catch (e) { console.error(e); }
        },
        editFloorLayout(floor) {
            this.tempFloor = floor;
            this.tempSplitCount = floor.SerialId || ''; // Default to current SerialId
            this.showSplitModalDialog = true;
        },
        closeSplitDialog() {
            this.showSplitModalDialog = false;
        },
        async confirmSplit() {
            const count = parseInt(this.tempSplitCount);
            if (isNaN(count) || count < 0) {
                return uni.showToast({ title: '请输入有效数量', icon: 'none' });
            }

            try {
                await request.post('api/ssbh/Position/Update', {
                    mainData: { Id: this.tempFloor.Id, SerialId: count }
                });

                // Update Local
                const floorInAll = this.allFloorList.find(f => f.Id === this.tempFloor.Id);
                if (floorInAll) floorInAll.SerialId = count;

                const floorInList = this.floorList.find(f => f.Id === this.tempFloor.Id);
                if (floorInList) floorInList.SerialId = count;

                uni.showToast({ title: '修改成功' });
                this.closeSplitDialog();

                // Refresh serials to reflect new split count
                await this.refreshFloorSerials(this.tempFloor);
            } catch (e) {
                console.error(e);
                uni.showToast({ title: '修改失败', icon: 'none' });
            }
        },
        getOccupiedCount(floor) {
            const serials = this.getSerialsByFloor(floor.Id);
            return serials.filter(s => s.ItemId && s.ItemId > 0).length;
        },
        async restockFloor(floor) {
            uni.showToast({ title: '已触发一键补库', icon: 'success' });
            // Call API placeholder
        },
        showSerialActions(serial) {
            this.tempSerial = serial;
            this.showSerialActionSheetModal = true;
        },
        closeSerialActionSheet() {
            this.showSerialActionSheetModal = false;
        },
        handleSerialActionItem(index) {
            this.closeSerialActionSheet();
            if (index === 1) {
                // Bind Item
                this.bindItemToSerial();
            } else if (index === 2) {
                // Unbind Item
                this.unbindItemFromSerial();
            } else if (index === 3) {
                // Manual
                this.showItemManual();
            }
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
        },
        async onSearch() {
            if (!this.searchText) return;
            if (!this.selectedRescueCarId) return uni.showToast({ title: '请先选择抢救车', icon: 'none' });

            uni.showLoading({ title: '搜索中...' });
            try {
                // Search Serials in current Rescue Car (which is a Warehouse)
                // We need to find Serials where Floor.WarehouseId == selectedRescueCarId AND (ItemName like text OR Name like text)
                // This requires a complex query or filtering locally if we had all data.
                // We don't have all data.
                // Let's try to fetch all serials for this rescue car that match.

                const res = await request.post('api/ssbh/Position/GetPageData', {
                    page: 1, rows: 100,
                    wheres: JSON.stringify([
                        { name: 'Category', value: 110 },
                        // Can't easily filter by Grandparent (WarehouseId) directly on Position usually unless there's a field.
                        // Fortunately we modified GetPageData earlier to accept WarehouseId for serials?
                        // Actually, in `toggleFloor` we used WarehouseId in wheres!
                        // So we can search by WarehouseId + ItemName/Name
                        { name: 'WarehouseId', value: this.selectedRescueCarId },
                        { name: 'ItemName', value: this.searchText, displayType: 'like' }
                    ])
                });

                if (res.rows && res.rows.length > 0) {
                    // Found items.
                    // Expand the floors they belong to.
                    const floorIds = [...new Set(res.rows.map(s => s.FloorId))];

                    // Find these floors in our list
                    for (let fid of floorIds) {
                        const floor = this.floorList.find(f => f.Id === fid);
                        if (floor) {
                            if (!floor.expanded) {
                                await this.refreshFloorSerials(floor);
                            }
                        }
                    }
                    uni.showToast({ title: `找到 ${res.rows.length} 个物品`, icon: 'none' });
                } else {
                    uni.showToast({ title: '未找到相关物品', icon: 'none' });
                }

            } catch (e) {
                console.error(e);
                uni.showToast({ title: '搜索失败', icon: 'none' });
            } finally {
                uni.hideLoading();
            }
        },
        bindItemToSerial() {
            // Remove any existing listener to prevent duplicates
            uni.$off('itemSelected');

            // Listen for selection
            uni.$on('itemSelected', async (item) => {
                // Ensure we have a target
                if (!this.tempSerial) return;

                try {
                    uni.showLoading({ title: '绑定中...' });
                    await request.post('api/ssbh/Position/Update', {
                        mainData: { Id: this.tempSerial.Id, ItemId: item.Id, ItemName: item.Name }
                    });

                    uni.showToast({ title: '绑定成功', icon: 'success' });

                    // Refresh
                    const floor = this.floorList.find(f => f.Id === this.tempSerial.FloorId);
                    if (floor) {
                        await this.refreshFloorSerials(floor);
                    }
                } catch (e) {
                    console.error(e);
                    uni.showToast({ title: '绑定失败', icon: 'none' });
                } finally {
                    uni.hideLoading();
                    // Clean up
                    uni.$off('itemSelected');
                }
            });

            uni.navigateTo({
                url: '/pages/item-manage/item-manage?mode=select'
            });
        },
        async unbindItemFromSerial() {
            if (!this.tempSerial || !this.tempSerial.ItemId) {
                return uni.showToast({ title: '当前未绑定物品', icon: 'none' });
            }

            try {
                uni.showLoading({ title: '解绑中...' });
                await request.post('api/ssbh/Position/Update', {
                    mainData: { Id: this.tempSerial.Id, ItemId: 0, ItemName: '' } // 0 or null
                });

                uni.showToast({ title: '解绑成功', icon: 'success' });
                const floor = this.floorList.find(f => f.Id === this.tempSerial.FloorId);
                if (floor) {
                    await this.refreshFloorSerials(floor);
                }
            } catch (e) {
                console.error(e);
                uni.showToast({ title: '解绑失败', icon: 'none' });
            } finally {
                uni.hideLoading();
            }
        },
        showItemManual() {
            if (!this.tempSerial || !this.tempSerial.ItemId) {
                return uni.showToast({ title: '当前未绑定物品', icon: 'none' });
            }
            const item = this.itemsMap[this.tempSerial.ItemId];
            if (!item) {
                return uni.showToast({ title: '未找到物品信息', icon: 'none' });
            }

            this.itemRemarkContent = item.Remark || '暂无说明书内容';
            this.showManualDialog = true;
        },
        closeManualDialog() {
            this.showManualDialog = false;
        },
        onLoadMore() {
            // Placeholder
        },
        getDaysUntil(dateStr) {
            if (!dateStr) return 9999;
            const now = new Date();
            const target = new Date(dateStr);
            return Math.ceil((target.getTime() - now.getTime()) / (1000 * 3600 * 24));
        },
        getExpiryClass(dateStr) {
            const days = this.getDaysUntil(dateStr);
            if (days < 0) return 'text-black'; // Expired
            if (days < 30) return 'text-red-bold'; // Urgent warning
            if (days <= 90) return 'text-yellow-bold'; // Warning
            return 'text-normal';
        },
        isStockInsufficient(item) {
            if (!item) return false;
            const stock = item.AvailableStock || 0;
            const base = item.Cardinality || 0;
            return stock < base || stock === 0;
        },
        isExpiryWarning(item) {
            if (!item || !item.LatestExpirationDate) return false;
            const days = this.getDaysUntil(item.LatestExpirationDate);
            return days < 30; // Matches warning logic
        },
        hasExpiryProblem(floor) {
            // Check full list for this floor, ignoring current filter
            // Logic: Expiry < 30 days (User defined "AbnormalExpiry" as < 30)
            return this.serialList.some(s => {
                if (s.FloorId !== floor.Id) return false;
                if (!s.ItemId || !this.itemsMap[s.ItemId]) return false;
                const item = this.itemsMap[s.ItemId];
                const days = this.getDaysUntil(item.LatestExpirationDate);
                return days < 30;
            });
        },
        hasStockProblem(floor) {
            // Logic: Stock < Cardinality
            return this.serialList.some(s => {
                if (s.FloorId !== floor.Id) return false;
                if (!s.ItemId || !this.itemsMap[s.ItemId]) return false;
                const item = this.itemsMap[s.ItemId];
                return this.isStockInsufficient(item);
            });
        }
    }
}
</script>

<style lang="scss" scoped>
@import '@/sull.scss';

.container {
    background-color: #F7F8FA;
    height: 100vh;
    display: flex;
    flex-direction: column;
}

.header-section {
    height: 200px;
    background: linear-gradient(135deg, $sull-color-primary 0%, lighten($sull-color-primary, 20%) 100%);
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
    margin-top: 10px;
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
    mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M3 5v4h2V5h4V3H5c-1.1 0-2 .9-2 2zm2 10H3v4c0 1.1.9 2 2 2h4v-2H5v-4zm14 4h-4v2h4c1.1 0 2-.9 2-2v-4h-2v4zm0-16h-4v2h4v4h2V5c0-1.1-.9-2-2-2z'/%3E%3C/svg%3E");
    -webkit-mask-size: contain;
    mask-size: contain;
    -webkit-mask-repeat: no-repeat;
    mask-repeat: no-repeat;
    -webkit-mask-position: center;
    mask-position: center;
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

/* Horizontal Car Switcher */
.horizontal-section {
    display: flex;
    align-items: center;
    margin-bottom: 24px;
}

.fixed-add-btn {
    width: 48px;
    height: 48px;
    background: #F2F4F8;
    border-radius: 12px;
    display: flex;
    align-items: center;
    justify-content: center;
    margin-right: 12px;
    flex-shrink: 0;

    .add-icon {
        font-size: 24px;
        color: $sull-color-primary;
        font-weight: bold;
    }

    &:active {
        background: #e0e0e0;
    }
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

    .floor-count {
        font-size: 10px;
        color: #999;
        margin-left: 4px;
    }

    &.active {
        background: #E6F7FF;
        color: $sull-color-primary;
        border: 1px solid $sull-color-primary;

        .card-name {
            color: $sull-color-primary;
            font-weight: bold;
        }

        .floor-count {
            color: $sull-color-primary;
        }
    }
}

/* Floor List */
.floor-list {
    display: flex;
    flex-direction: column;
    gap: 12px;
}

.add-floor-btn {
    height: 48px;
    /** 56px in Add Car, but 48px is fine here */
    background: $sull-color-primary;
    border-radius: 28px;
    display: flex;
    align-items: center;
    justify-content: center;
    box-shadow: 0 4px 16px rgba(149, 202, 255, 0.5);
    margin-top: 16px;
}

.add-floor-btn:active {
    opacity: 0.9;
    transform: scale(0.98);
}

.add-text {
    font-size: 16px;
    color: #fff;
    font-weight: 500;
}

.floor-wrapper {
    margin-bottom: 0;
}

.floor-card {
    background: #fff;
    border-radius: 12px;
    padding: 16px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08), 0 1px 4px rgba(0, 0, 0, 0.04);
    display: flex;
    align-items: center;
    min-height: 60px;
    border: 1px solid #f0f0f0;
}

.floor-card:active {
    background-color: #f9f9f9;
}

.floor-icon {
    margin-right: 8px;
}

.icon-circle {
    width: 8px;
    height: 32px;
    border-radius: 4px;
    background-color: #F7F8FA;
    transition: all 0.3s;
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
    justify-content: center;
}

.floor-action {
    width: 32px;
    height: 32px;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: 50%;
    margin-left: 8px;
}

.floor-action:active {
    background-color: #f0f0f0;
}

.action-icon {
    font-size: 20px;
    color: #999;
    font-weight: bold;
}

.floor-name {
    font-size: 16px;
    font-weight: 600;
    color: #333;
    margin-bottom: 4px;
}

.floor-sub {
    font-size: 13px;
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
    position: relative;
    /* 3D Box Effect - Inset look */
    box-shadow:
        inset 0 2px 4px rgba(0, 0, 0, 0.06),
        0 1px 2px rgba(255, 255, 255, 0.8),
        0 0 0 1px rgba(0, 0, 0, 0.03);
    border: 1px solid #E4E7ED;

    &.expiry-warning {
        border: 1px solid #F56C6C !important;
        background-color: #FFF0F0;
    }
}

.serial-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 4px;
}

.serial-no {
    font-size: 14px;
    color: #303133;
    font-weight: 900;
    /* Bold */
    font-family: inherit;
}

.serial-action {
    width: 24px;
    height: 24px;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #909399;
    font-weight: bold;
    font-size: 18px;
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

.item-stock {
    font-size: 11px;
    font-weight: bold;
    color: #666;
    padding: 0 4px;
    display: flex;
    align-items: baseline;
}

.text-red-bold {
    color: #F56C6C !important;
    background: rgba(245, 108, 108, 0.1) !important;
}

.text-yellow-bold {
    color: #E6A23C !important;
}

.text-black {
    color: #303133 !important;
    font-weight: bold !important;
}

.item-cardinality {
    font-size: inherit;
    color: inherit;
    opacity: 0.8;
    margin-left: 2px;
    font-weight: normal;
}

.item-unit {
    font-size: 10px;
    color: inherit;
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
    font-weight: 300;
}

@keyframes fadeIn {
    from {
        opacity: 0;
        transform: translateY(-5px);
    }

    to {
        opacity: 1;
        transform: translateY(0);
    }
}

.bottom-spacer {
    height: 40px;
}

.empty-placeholder {
    color: #999;
    font-size: 14px;
    text-align: center;
    margin-top: 20px;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 10px;
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

/* Custom Action Sheet */
.action-sheet-overlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.5);
    z-index: 2000;
    display: flex;
    flex-direction: column;
    justify-content: flex-end;
}

.action-sheet-panel {
    background: #F7F7F7;
    border-top-left-radius: 16px;
    border-top-right-radius: 16px;
    overflow: hidden;
    animation: slideUp 0.3s ease;
}

.action-item {
    background: #fff;
    padding: 16px;
    text-align: center;
    font-size: 16px;
    color: #171725;
    border-bottom: 1px solid #f0f0f0;
}

.action-item:last-child {
    border-bottom: none;
}

.action-item:active {
    background: #f9f9f9;
}

.action-cancel-gap {
    height: 8px;
    background: #F7F7F7;
}

.action-item.cancel {
    color: #171725;
}

/* Custom Modal */
.modal-overlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.5);
    z-index: 2001;
    display: flex;
    align-items: center;
    justify-content: center;
    backdrop-filter: blur(2px);

    .modal-content {
        width: 85%;
        background: #fff;
        border-radius: 16px;
        display: flex;
        flex-direction: column;
        animation: slideUp 0.3s ease;

        .modal-header {
            padding: 16px;
            border-bottom: 1px solid #E9E9ED;
            display: flex;
            justify-content: space-between;
            align-items: center;

            .modal-title {
                font-size: 18px;
                font-weight: bold;
                color: #171725;
            }

            .close-btn {
                font-size: 18px;
                color: #78828A;
                padding: 4px;
            }
        }

        .modal-body {
            padding: 20px;

            .form-item {
                display: flex;
                flex-direction: column;

                .label {
                    font-size: 12px;
                    color: #78828A;
                    margin-bottom: 8px;
                }
            }

            .input-wrapper {
                background-color: #FDFDFD;
                border: 1px solid #E9E9ED;
                border-radius: 12px;
                padding: 0 12px;
                display: flex;
                align-items: center;
                height: 48px;
                box-sizing: border-box;
            }

            .input-field {
                flex: 1;
                font-size: 16px;
                color: #171725;
                height: 100%;
                border: none;
                background: transparent;
                padding: 0;
            }

            .clear-icon {
                width: 24px;
                height: 24px;
                display: flex;
                align-items: center;
                justify-content: center;
                color: #999;
                font-size: 14px;
            }
        }

        .modal-footer {
            display: flex;
            border-top: 1px solid #E9E9ED;

            .modal-btn {
                flex: 1;
                border: none;
                background: #fff;
                padding: 14px 0;
                font-size: 14px;
                line-height: normal;
                border-radius: 0;
            }

            .modal-btn.cancel {
                color: #78828A;
                border-right: 1px solid #E9E9ED;
                border-bottom-left-radius: 16px;
            }

            .modal-btn.confirm {
                color: #95CAFF;
                font-weight: 500;
                border-bottom-right-radius: 16px;
            }

            .modal-btn:active {
                background: #F9F9F9;
            }
        }
    }
}

@keyframes slideUp {
    from {
        transform: translateY(20px);
        opacity: 0;
    }

    to {
        transform: translateY(0);
        opacity: 1;
    }
}

.manual-modal {
    max-height: 80vh;
    display: flex;
    flex-direction: column;
}

.manual-body {
    padding: 24px;
    max-height: 60vh;
    box-sizing: border-box;
}

.manual-text {
    font-size: 15px;
    color: #333;
    line-height: 1.8;
    white-space: pre-wrap;
    text-align: justify;
}
</style>

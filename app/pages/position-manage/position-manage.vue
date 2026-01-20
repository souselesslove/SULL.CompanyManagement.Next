<template>
    <view class="container">
        <!-- Navigation Bar Removed - Using Header Section -->


        <scroll-view scroll-y class="main-content" :scroll-top="scrollTopVal" refresher-enabled="true"
            :refresher-triggered="isRefreshing" :refresher-threshold="60" @refresherrefresh="onRefresh"
            @refresherpulling="onPulling" @refresherrestore="onRestore" @refresherabort="onRestore"
            refresher-default-style="none" refresher-background="#f0f5ff" @scroll="onScroll">
            <CustomRefresher slot="refresher" :isRefreshing="isRefreshing" :isPulling="isPulling" />

            <!-- Header Gradient Section matching Add Car page -->
            <view class="header-section" :style="{ paddingTop: statusBarHeight * 2 + 'rpx' }">
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
                    <scroll-view scroll-x class="horizontal-scroll" :show-scrollbar="false"
                        :scroll-into-view="scrollIntoViewId" scroll-with-animation>
                        <view class="scroll-inner">
                            <view class="fixed-add-btn" @click="goAdd(10)">
                                <text class="add-icon">+</text>
                            </view>

                            <view class="h-card" v-for="(item, index) in rescueCarList" :key="item.Id"
                                :id="'car-' + item.Id" :class="{ active: selectedRescueCarId === item.Id }"
                                @click="selectRescueCar(item.Id)" @longpress="showDutyUserModal(item)">
                                <text class="card-name">{{ item.DisplayName }}</text>
                                <text class="card-duty" v-if="item.DutyUser">{{ item.DutyUser }}</text>
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

                <!-- Floor List (Refactored) -->
                <StorageLayers :floorList="floorList" :serialsMap="floorSerialsMap" :itemsMap="itemsMap"
                    :showFloorAction="true" :showSerialAction="true" :highlightedSerialIds="highlightedSerialIds"
                    @toggle-floor="toggleFloor" @serial-click="editSerial" @floor-action="showFloorActions"
                    @serial-action="showSerialActions">
                    <template #add-floor>
                        <!-- Add Button Module -->
                        <view v-if="false" class="add-floor-btn" @click="goAdd(20)">
                            <text class="add-text">+ 添加分层</text>
                        </view>
                    </template>
                </StorageLayers>
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
                <view class="action-item" @click="handleSerialActionItem(4)">插入绑定</view>
                <view class="action-item" @click="handleSerialActionItem(2)">取消绑定</view>
                <view class="action-item" @click="handleSerialActionItem(3)">说明书</view>
                <view class="action-cancel-gap"></view>
                <view class="action-item cancel" @click="closeSerialActionSheet">取消</view>
            </view>
        </view>

        <!-- Custom Rename Modal -->
        <view class="modal-overlay" v-if="showRenameModalDialog" @click="closeRenameDialog">
            <view class="modal-content" @click.stop>
                <view class="modal-header">
                    <text class="modal-title">修改名称</text>
                    <text class="close-btn" @click="closeRenameDialog">×</text>
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
        <view class="modal-overlay" v-if="showSplitModalDialog" @click="closeSplitDialog">
            <view class="modal-content" @click.stop>
                <view class="modal-header">
                    <text class="modal-title">修改分隔</text>
                    <text class="close-btn" @click="closeSplitDialog">×</text>
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
        <view class="modal-overlay" v-if="showManualDialog" @click="closeManualDialog">
            <view class="modal-content manual-modal" @click.stop>
                <view class="modal-header">
                    <text class="modal-title">{{ itemManualName }} 说明书</text>
                    <text class="close-btn" @click="closeManualDialog">×</text>
                </view>
                <scroll-view scroll-y class="modal-body manual-body">
                    <text class="manual-text">{{ itemRemarkContent }}</text>
                </scroll-view>
                <view class="modal-footer">
                    <button class="modal-btn confirm full-width" @click="closeManualDialog">关闭</button>
                </view>
            </view>
        </view>
    </view>

    <!-- Member Selector Modal (Bottom Sheet Style) -->
    <view class="member-overlay" v-if="showMemberSelector" @click="closeMemberSelector">
        <view class="member-modal" @click.stop>
            <view class="member-top-bar">
                <text class="member-modal-title">设置抢救车责任人</text>
                <text class="close-btn" @click="closeMemberSelector">✖️</text>
            </view>
            <view class="member-search-bar">
                <view class="search-box">
                    <input class="search-input" v-model="searchMemberKey" placeholder="搜索人员" />
                    <text v-if="searchMemberKey" class="clear-icon" @click="clearSearchMember">✖</text>
                </view>
            </view>
            <scroll-view scroll-y class="member-list">
                <view v-for="(m, index) in filteredMemberList" :key="m.id" class="member-item" @click="selectMember(m)">
                    <text class="item-text">{{ m.name }}</text>
                </view>
                <view v-if="filteredMemberList.length === 0" class="empty-state-small">
                    <text>无搜索结果</text>
                </view>
            </scroll-view>
        </view>
    </view>

    <!-- Warning Modal -->
    <WarningModal v-model:visible="warningModal.visible" :title="warningModal.title" :message="warningModal.message"
        :buttonText="warningModal.buttonText" />
</template>

<script>
import { globalScan } from '@/utils/scan.js';
import BackButton from '@/components/BackButton/BackButton.vue';
import FooterInfo from '@/components/FooterInfo/FooterInfo.vue';
import BackToTop from '@/components/BackToTop/BackToTop.vue';
import WarningModal from '@/components/WarningModal/WarningModal.vue';
import StorageLayers from '@/components/StorageLayers/StorageLayers.vue';
import CustomRefresher from '@/components/CustomRefresher/CustomRefresher.vue';
import request from '@/utils/request.js';
import { storage } from '@/utils/storage.js';
import dayjs from 'dayjs';

export default {
    components: { BackButton, FooterInfo, BackToTop, WarningModal, StorageLayers, CustomRefresher },
    data() {
        return {
            statusBarHeight: 20,
            scrollTop: 0,
            isPulling: false,
            isRefreshing: false,
            scrollTopVal: 0,

            // Data Lists
            rescueCarList: [],
            floorList: [],
            serialList: [],

            // Selections
            selectedRescueCarId: null,
            scrollIntoViewId: '',
            highlightedSerialIds: [],

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
            itemManualName: '',
            itemRemarkContent: '',

            // Filter
            currentFilter: null,

            // Duty User Selection
            showMemberSelector: false,
            memberList: [],
            searchMemberKey: '',
            currentEditingCar: null,
            warningModal: {
                visible: false,
                title: '',
                message: '',
                buttonText: ''
            }
        };
    },
    onLoad(options) {
        const sysInfo = uni.getSystemInfoSync();
        this.statusBarHeight = sysInfo.statusBarHeight || 20;
        this.userOrgInfo = storage.get('userOrgInfo') || {};

        if (options && options.filter) {
            this.currentFilter = options.filter; // 'AbnormalStock' or 'AbnormalExpiry'
        }

        this.fetchData();
        this.fetchMembers();
    },
    computed: {
        floorSerialsMap() {
            const map = {};
            this.floorList.forEach(f => map[f.Id] = []);

            const rawMap = {};
            this.serialList.forEach(s => {
                if (!rawMap[s.FloorId]) rawMap[s.FloorId] = [];
                rawMap[s.FloorId].push(s);
            });

            for (const fid in rawMap) {
                let list = rawMap[fid];
                if (this.currentFilter === 'AbnormalStock') {
                    list = list.filter(s => {
                        const item = this.itemsMap[s.ItemId];
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
                map[fid] = list;
            }
            return map;
        },
        filteredMemberList() {
            if (!this.searchMemberKey) return this.memberList;
            const key = this.searchMemberKey.toLowerCase();
            return this.memberList.filter(m => m.name.toLowerCase().includes(key));
        }
    },
    methods: {
        goBack() {
            uni.navigateBack();
        },
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
                        { name: 'Category', value: 10 },
                        { name: 'OrganizationId', value: this.userOrgInfo.Organization.Id },
                        { name: 'DepartmentId', value: this.userOrgInfo.Department.Id }
                    ]),
                    sort: 'OrderBy', order: 'asc'
                });
                this.rescueCarList = (res.rows || []).filter(w => w.Category === 10);

                if (this.rescueCarList.length > 0) {
                    let targetId = this.selectedRescueCarId;

                    // Try to load from storage
                    if (!targetId) {
                        const lastId = storage.get('lastSelectedRescueCarId');
                        // Use loose comparison to handle potential string/number mismatch
                        if (lastId && this.rescueCarList.find(w => w.Id == lastId)) {
                            targetId = lastId;
                        }
                    }

                    if (!targetId || !this.rescueCarList.find(w => w.Id == targetId)) {
                        this.selectRescueCar(this.rescueCarList[0].Id);
                    } else {
                        this.selectRescueCar(targetId);
                    }
                } else {
                    this.rescueCarList = [];
                    this.floorList = [];
                    this.selectedRescueCarId = null;
                }
            } catch (e) {
                console.error(e);
            }
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
                // Background refresh: preserve states
                const expandedIds = this.floorList.filter(f => f.expanded).map(f => f.Id);

                const res = await request.post('api/ssbh/Position/GetPageData', {
                    page: 1, rows: 1000,
                    wheres: JSON.stringify([
                        { name: 'WarehouseId', value: warehouseId },
                        { name: 'Category', value: '100,110', DisplayType: 'in' }
                    ]),
                    sort: 'SerialId,OrderBy', order: 'asc'
                });

                const allItems = res.rows || [];
                const nextFloorList = allItems.filter(p => p.Category === 100).sort((a, b) => (a.OrderBy || 0) - (b.OrderBy || 0));
                const nextSerialList = allItems.filter(p => p.Category === 110);

                // Restore expanded states
                nextFloorList.forEach(f => {
                    if (expandedIds.includes(f.Id) || (this.currentFilter)) {
                        this.$set(f, 'expanded', true);
                    }
                });

                // Pre-fetch items BEFORE updating list to avoid "empty slots" flash
                const itemIds = nextSerialList.filter(s => s.ItemId > 0).map(s => s.ItemId);
                if (itemIds.length > 0) {
                    await this.fetchItemDetails([...new Set(itemIds)]);
                }

                // Atomic update to prevent flashing
                this.floorList = nextFloorList;
                this.serialList = nextSerialList;
            } catch (e) {
                console.error("Fetch Rescue Car Data Error", e);
            }
        },

        // Display helpers moved to StorageLayers component

        async fetchItemDetails(ids, force = false) {
            const distinctIds = force ? ids : ids.filter(id => !this.itemsMap[id]);
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

                        // Pre-calculate properties for StorageLayers
                        item.IsExpiryWarning = days < 30;
                        const stock = Number(item.AvailableStock || 0);
                        const base = Number(item.Cardinality || 0);
                        item.IsStockInsufficient = stock <= 0 || (base > 0 && stock < base);
                        item.FormattedDate = this.formatDate(dateToUse);
                        item.ExpiryClass = this.getExpiryClass(dateToUse);

                        this.$set(this.itemsMap, item.Id, item);
                    });
                }
            } catch (e) {
                console.error("Batch fetch item details failed", e);
            }
        },

        goAdd(category) {
            // category 10 = rescue car, 100 = floor
            if (category === 10) {
                if (!this.userOrgInfo.Department || !this.userOrgInfo.Department.Id) {
                    this.warningModal = {
                        visible: true,
                        title: this.$t('common.tip'),
                        message: this.$t('profile.please_join_dept_first'),
                        buttonText: this.$t('common.confirm')
                    };
                    return;
                }
                uni.navigateTo({
                    url: `/pages/position-manage/position-add-car`
                });
            }
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
            const target = dayjs(dateStr).toDate();
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
        toggleFloor(floor) {
            this.$set(floor, 'expanded', !floor.expanded);
        },
        async refreshFloorSerials(floor, force = false) {
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
                    await this.fetchItemDetails([...new Set(itemIds)], force);
                }

                this.$set(floor, 'expanded', true);
            } catch (e) {
                console.error("Refresh Floor Serials Error", e);
                uni.showToast({ title: '加载失败', icon: 'none' });
            } finally {
                uni.hideLoading();
            }
        },
        // Status Logic moved to component
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
        // Occupied Count Logic moved to component
        async restockFloor(floor) {
            try {
                uni.showLoading({ title: '补库中...', mask: true });
                // One-click restock: Set all stock for the warehouse (car) or floor?
                // User said "SetAllStock, need to pass WarehouseId". 
                // Context is "Action Sheet Item 2: One Key Restock", which is triggered from a Floor card 'showFloorActions'.
                // If it really means WarehouseId, it might restock the WHOLE car.
                // Assuming it means for this floor's scope, but API takes WarehouseId.
                // If the user wants to restock the whole car, we pass floor.WarehouseId.

                await request.post('api/ssbh/ItemHistory/SetAllStock', {
                    mainData: { WarehouseId: floor.WarehouseId }
                });

                uni.showToast({ title: '补库成功', icon: 'success' });

                // Refresh to show updated stock
                await this.refreshFloorSerials(floor, true);

            } catch (e) {
                console.error(e);
                uni.showToast({ title: '补库失败', icon: 'none' });
            } finally {
                uni.hideLoading();
            }
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
            if (index === 0) {
                // Scan Bind (Not implemented yet, but keeping structure)
                uni.showToast({ title: '扫码绑定开发中', icon: 'none' });
            } else if (index === 1) {
                // Bind Item
                this.bindItemToSerial(false);
            } else if (index === 4) {
                // Insert Bind
                this.bindItemToSerial(true);
            } else if (index === 2) {
                // Unbind Item
                this.unbindItemFromSerial();
            } else if (index === 3) {
                // Manual
                this.showItemManual();
            }
        },
        async onScan() {
            globalScan((res) => {
                if (res.data) {
                    this.searchText = res.data;
                    this.onSearch();
                }
            });
        },
        clearSearch() {
            this.searchText = '';
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
                        { name: 'OrganizationId', value: this.userOrgInfo.Organization?.Id },
                        { name: 'DepartmentId', value: this.userOrgInfo.Department?.Id },
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
                            // Need to select from inside the StorageLayers component or global
                            // Using .in(this) should work if there's only one instance or if IDs are unique.
                            query.select('#serial-' + firstId).boundingClientRect(data => {
                                if (data) {
                                    const sysInfo = uni.getSystemInfoSync();
                                    const windowHeight = sysInfo.windowHeight;

                                    // Calculate delta to move element to center
                                    let scrollDiff = data.top - (windowHeight / 2) + (data.height / 2);
                                    let targetTop = this.scrollTop + scrollDiff;

                                    // Ensure we trigger a change
                                    this.scrollTopVal = this.scrollTop;
                                    this.$nextTick(() => {
                                        this.scrollTopVal = targetTop;
                                    });
                                }
                            }).exec();
                        });
                    }

                    uni.showToast({ title: `找到 ${foundSerials.length} 个物品`, icon: 'none' });
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
        bindItemToSerial(isInsert = false) {
            // Remove any existing listener to prevent duplicates
            uni.$off('itemSelected');

            // Listen for selection
            uni.$on('itemSelected', async (item) => {
                // Ensure we have a target
                if (!this.tempSerial) return;

                // Stop if same item selected (Only for non-insert)
                if (!isInsert && this.tempSerial.ItemId === item.Id) return;

                try {
                    uni.showLoading({ title: isInsert ? '插入中...' : '绑定中...' });
                    const mainData = { Id: this.tempSerial.Id, ItemId: item.Id, ItemName: item.Name };
                    if (isInsert) {
                        mainData.INS = 1;
                    }

                    await request.post('api/ssbh/Position/Update', { mainData });

                    uni.showToast({ title: isInsert ? '插入成功' : '绑定成功', icon: 'success' });

                    if (isInsert) {
                        // Refresh everything on insert as positions might shift
                        this.fetchRescueCarData(this.selectedRescueCarId);
                    } else {
                        // Update Local State for direct bind
                        if (this.tempSerial) {
                            this.tempSerial.ItemId = item.Id;
                            this.tempSerial.ItemName = item.Name;
                            // Pre-fetch item details for UI
                            await this.fetchItemDetails([item.Id]);
                        }
                        const floor = this.floorList.find(f => f.Id === this.tempSerial.FloorId);
                        if (floor) {
                            this.$set(floor, 'expanded', true);
                        }
                    }
                } catch (e) {
                    console.error(e);
                    uni.showToast({ title: isInsert ? '插入失败' : '绑定失败', icon: 'none' });
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
                // Update Local State
                if (this.tempSerial) {
                    this.tempSerial.ItemId = 0;
                    this.tempSerial.ItemName = '';
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
            this.itemManualName = item.Name || '物品';
            this.itemRemarkContent = item.Remark || '暂无说明书内容';
            this.showManualDialog = true;
        },
        // Duty User Logic
        showDutyUserModal(car) {
            this.currentEditingCar = car;
            this.showMemberSelector = true;
            this.searchMemberKey = '';
        },
        closeMemberSelector() {
            this.showMemberSelector = false;
        },
        clearSearchMember() {
            this.searchMemberKey = '';
        },
        fetchMembers() {
            request.get('api/ssbh/OrganizationUser/DepartmentUser')
                .then(res => {
                    this.memberList = (res.rows || []).map(u => ({
                        name: u.UserTrueName || u.userTrueName || u.UserName || u.userName,
                        id: u.UserId || u.userId
                    }));
                })
                .catch(err => {
                    console.error("Fetch Members Failed", err);
                });
        },
        selectMember(member) {
            console.log(member);
            if (!this.currentEditingCar) return;
            const car = this.currentEditingCar;

            uni.showLoading({ title: '更新中...' });

            const payload = {
                ...car,
                DutyUser: member.name,
                DutyUserId: member.id,
            };

            // Using standard update
            request.post('api/ssbh/Position/Update', {
                mainData: payload
            }).then(res => {
                uni.hideLoading();
                if (res.success !== false) { // Handle variable response structure
                    uni.showToast({ title: '已更新责任人' });
                    // Update local list
                    car.DutyUser = member.name;
                    this.closeMemberSelector();
                } else {
                    uni.showToast({ title: res.msg || '更新失败', icon: 'none' });
                }
            }).catch(e => {
                uni.hideLoading();
                console.error(e);
                uni.showToast({ title: '网络错误', icon: 'none' });
            });
        },
        closeManualDialog() {
            this.showManualDialog = false;
        },

        getDaysUntil(dateStr) {
            if (!dateStr) return 9999;
            const now = new Date();
            const target = dayjs(dateStr).toDate();
            return Math.ceil((target.getTime() - now.getTime()) / (1000 * 3600 * 24));
        },
        getExpiryClass(dateStr) {
            const days = this.getDaysUntil(dateStr);
            if (days < 0) return 'text-black'; // Expired
            if (days < 30) return 'text-red'; // Urgent warning
            if (days <= 90) return 'text-yellow'; // Warning
            return 'text-normal';
        },
        isStockInsufficient(item) {
            if (!item) return false;
            if (typeof item.IsStockInsufficient === 'boolean') return item.IsStockInsufficient;

            const stock = Number(item.AvailableStock || 0);
            const base = Number(item.Cardinality || 0);
            return stock <= 0 || (base > 0 && stock < base);
        },
        isExpiryWarning(item) {
            if (!item || !item.LatestExpirationDate) return false;
            const days = this.getDaysUntil(item.LatestExpirationDate);
            return days < 30; // Matches warning logic
        },
        hasExpiryProblem(floor) {
            // Check full list for this floor, ignoring current filter
            return this.serialList.some(s => {
                if (s.FloorId !== floor.Id) return false;
                if (!s.ItemId || !this.itemsMap[s.ItemId]) return false;
                const item = this.itemsMap[s.ItemId];
                return item.IsExpiryWarning;
            });
        },
        hasStockProblem(floor) {
            // Logic: Stock < Cardinality
            return this.serialList.some(s => {
                if (s.FloorId !== floor.Id) return false;
                if (!s.ItemId || !this.itemsMap[s.ItemId]) return false;
                const item = this.itemsMap[s.ItemId];
                return item && item.IsStockInsufficient;
            });
        }
    }
}
</script>

<style lang="scss" scoped>
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

.header-section {
    height: 400rpx;
    background: linear-gradient(135deg, $sull-color-primary 0%, lighten($sull-color-primary, 20%) 100%);
    padding: 0 48rpx;
    display: flex;
    flex-direction: column;
    box-sizing: border-box;
}

.main-content {
    flex: 1;
    height: 0;
    width: 100%;
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
    backdrop-filter: blur(5px);
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
    backdrop-filter: blur(5px);
    position: relative;
    z-index: 10;
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

.content-panel {
    background-color: #fff;
    border-top-left-radius: 60rpx;
    border-top-right-radius: 60rpx;
    margin-top: -80rpx;
    padding: 48rpx 32rpx;
}

/* Horizontal Car Switcher */
.horizontal-section {
    display: flex;
    align-items: center;
    margin-bottom: 48rpx;
}

.fixed-add-btn {
    width: 96rpx;
    height: 96rpx;
    background: #F2F4F8;
    border-radius: 24rpx;
    display: flex;
    align-items: center;
    justify-content: center;
    margin-right: 24rpx;
    flex-shrink: 0;

    .add-icon {
        font-size: 48rpx;
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
    flex-direction: column;
    align-items: center;
    justify-content: center;
    height: 96rpx;
    padding: 0 24rpx;
    min-width: 120rpx;
    background: #F7F8FA;
    border-radius: 48rpx;
    margin-right: 24rpx;
    border: 2rpx solid transparent;
    margin-right: 24rpx;
    border: 2rpx solid transparent;
    transition: all 0.3s;
    min-width: auto;
    flex-shrink: 0;

    .card-name {
        font-size: 28rpx;
        font-weight: 500;
        color: #555;
        line-height: 1.2;
    }

    .card-duty {
        font-size: 20rpx;
        color: #999;
        margin-top: 4rpx;
        line-height: 1;
    }

    .floor-count {
        font-size: 20rpx;
        color: #999;
        margin-left: 8rpx;
    }

    &.active {
        background: #E6F7FF;
        color: $sull-color-primary;
        border: 2rpx solid $sull-color-primary;

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
    gap: 24rpx;
}

.add-floor-btn {
    height: 96rpx;
    /** 56px in Add Car, but 48px is fine here */
    background: $sull-color-primary;
    border-radius: 56rpx;
    display: flex;
    align-items: center;
    justify-content: center;
    box-shadow: 0 8rpx 32rpx rgba(149, 202, 255, 0.5);
    margin-top: 32rpx;
}

.add-floor-btn:active {
    opacity: 0.9;
    transform: scale(0.98);
}

.add-text {
    font-size: 32rpx;
    color: #fff;
    font-weight: 500;
}

/* Floor Styles Removed (Moved to StorageLayers) */

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
    border-top-left-radius: 32rpx;
    border-top-right-radius: 32rpx;
    overflow: hidden;
    animation: slideUp 0.3s ease;
}

.action-item {
    background: #fff;
    padding: 32rpx;
    text-align: center;
    font-size: 32rpx;
    color: #171725;
    border-bottom: 2rpx solid #f0f0f0;
}

.action-item:last-child {
    border-bottom: none;
}

.action-item:active {
    background: #f9f9f9;
}

.action-cancel-gap {
    height: 16rpx;
    background: #F7F7F7;
}

.action-item.cancel {
    color: #171725;
}

/* Custom Modal */
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
        width: 80%;
        background: #fff;
        border-radius: 40rpx;
        display: flex;
        flex-direction: column;
        animation: slideUp 0.3s ease;
        overflow: hidden;

        .modal-header {
            padding: 32rpx;
            border-bottom: 2rpx solid $sull-color-line;
            display: flex;
            justify-content: space-between;
            align-items: center;

            .modal-title {
                font-size: 36rpx;
                font-weight: bold;
                color: $sull-text-main;
            }

            .close-btn {
                font-size: 40rpx;
                color: $sull-text-secondary;
                padding: 8rpx;
                line-height: 1;
            }
        }

        .modal-body {
            padding: 48rpx 40rpx;

            .form-item {
                display: flex;
                flex-direction: column;

                .label {
                    font-size: 28rpx;
                    color: $sull-text-secondary;
                    margin-bottom: 16rpx;
                }
            }

            .input-wrapper {
                background-color: $sull-color-background;
                border: 2rpx solid $sull-color-line;
                border-radius: 20rpx;
                padding: 0 24rpx;
                display: flex;
                align-items: center;
                height: 96rpx;
                box-sizing: border-box;
            }

            .input-field {
                flex: 1;
                font-size: 30rpx;
                color: $sull-text-main;
                height: 100%;
                border: none;
                background: transparent;
                padding: 0;
            }

            .clear-icon {
                width: 48rpx;
                height: 48rpx;
                display: flex;
                align-items: center;
                justify-content: center;
                color: $sull-text-disabled;
                font-size: 28rpx;
            }
        }

        .modal-footer {
            display: flex;
            border-top: 2rpx solid $sull-color-line;

            .modal-btn {
                flex: 1;
                border: none;
                background: #fff;
                padding: 28rpx 0;
                font-size: 30rpx;
                line-height: normal;
                border-radius: 0;

                &::after {
                    border: none;
                }
            }

            .modal-btn.cancel {
                color: $sull-text-secondary;
                border-right: 2rpx solid $sull-color-line;
            }

            .modal-btn.confirm {
                color: $sull-color-primary;
                font-weight: 500;
            }

            .modal-btn.full-width {
                width: 100%;
                border-right: none;
            }

            .modal-btn:active {
                background: $sull-color-background;
            }
        }
    }
}

@keyframes slideUp {
    from {
        transform: scale(0.9);
        opacity: 0;
    }

    to {
        transform: scale(1);
        opacity: 1;
    }
}

.manual-modal {
    max-height: 80vh;
}

.manual-body {
    padding: 48rpx;
    max-height: 50vh;
    box-sizing: border-box;
}

.manual-text {
    font-size: 28rpx;
    color: $sull-text-main;
    line-height: 1.6;
    white-space: pre-wrap;
    text-align: justify;
}

/* Member Selector Modal (Bottom Sheet) */
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
    backdrop-filter: blur(2px);
}

.member-modal {
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

.member-top-bar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 32rpx;
}

.member-modal-title {
    font-size: 36rpx;
    font-weight: bold;
    color: #171725;
}

.member-search-bar {
    display: flex;
    align-items: center;
    margin-bottom: 24rpx;

    .search-box {
        flex: 1;
        position: relative;
        display: flex;
        align-items: center;
        background: transparent; // Reset
        height: auto; // Reset
        border-radius: 0; // Reset
        backdrop-filter: none; // Reset
        padding: 0; // Reset
    }

    .search-input {
        flex: 1;
        height: 80rpx;
        background: #F5F7FA;
        border-radius: 40rpx;
        padding: 0 72rpx 0 32rpx;
        font-size: 28rpx;
        color: #333; // Dark text for member search
    }
}

.member-list {
    flex: 1;
    height: 0;
    min-height: 600rpx;
}

.member-item {
    padding: 32rpx;
    border-bottom: 2rpx solid #f9f9f9;
    display: flex;
    align-items: center;

    &:active {
        background: #f5f5f5;
    }
}

.item-text {
    font-size: 30rpx;
    color: #171725;
}

.empty-state-small {
    padding: 40rpx;
    text-align: center;
    color: #999;
    font-size: 28rpx;
}
</style>

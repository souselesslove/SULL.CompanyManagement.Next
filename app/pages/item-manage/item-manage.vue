<template>
    <view class="container" @click="hideAllMenus">
        <!-- Navigation Bar -->
        <!-- Navigation Bar -->
        <view class="header-section" :style="{ paddingTop: statusBarHeight + 'px' }">
            <view class="header-content" :style="{ height: navHeight + 'px' }">
                <view class="nav-left">
                    <BackButton class="header-back-btn" @click="goBack" />
                </view>
                <text class="page-title">{{ $t('itemManage.title') }}</text>
                <view class="nav-right"></view>
            </view>
        </view>



        <!-- List -->
        <scroll-view class="main-scroll" scroll-y :enable-back-to-top="true" :refresher-enabled="true"
            :refresher-triggered="isRefreshing" @refresherrefresh="onRefresh" @refresherpulling="onPulling"
            @refresherrestore="onRestore" @scrolltolower="onLoadMore" @scroll="onScroll" :scroll-top="scrollTop"
            refresher-default-style="none" refresher-background="#f0f5ff"
            :style="{ marginTop: (statusBarHeight + navHeight) + 'px', height: `calc(100vh - ${statusBarHeight + navHeight}px)` }">

            <CustomRefresher slot="refresher" :isRefreshing="isRefreshing" :isPulling="isPulling" />

            <!-- Search Bar -->
            <view class="content">
                <!-- Search Bar -->
                <view class="search-section">
                    <view class="search-box">
                        <text class="search-icon">🔍</text>
                        <input class="search-input" type="text" v-model="searchText"
                            :placeholder="$t('itemOutbound.search_placeholder')" confirm-type="search"
                            @confirm="onSearch" />
                        <view class="icon-btn" v-if="searchText" @click="clearSearch">
                            <text class="clear-icon">✕</text>
                        </view>
                    </view>
                </view>
                <view class="category-tabs">
                    <view v-for="(cat, index) in categories" :key="index" class="tab-item"
                        :class="{ active: currentCategory === cat.value }" @click="selectCategory(cat.value)">
                        <text>{{ cat.label }}</text>
                    </view>
                </view>
                <view class="list-content" :class="{ 'with-footer': selectMode }">
                    <view class="item-card" v-for="(item, index) in itemList" :key="index"
                        @click="handleItemClick(item)"
                        :class="{ 'selected': selectMode && selectedItem && selectedItem.Id === item.Id }">
                        <view class="card-row top-row">
                            <view class="name-section">
                                <text class="item-name">{{ item.Name }}</text>
                                <text class="item-code text-gray" v-if="item.InterfaceCode">({{ item.InterfaceCode
                                }})</text>
                            </view>
                            <view class="card-actions">
                                <view class="status-tag" :class="getExpiryStatus(item).class"
                                    v-if="getExpiryStatus(item)">
                                    <text>{{ getExpiryStatus(item).text }}</text>
                                </view>
                                <view class="action-btn" @click.stop="toggleMoreMenu(item)">
                                    <text class="action-icon">⋮</text>
                                </view>

                                <!-- 下拉菜单 -->
                                <view class="more-menu" v-if="activeMenuId === item.Id" @click.stop>
                                    <view class="menu-item" @click="goToBatchManage(item)">
                                        <text class="menu-icon-small">📦</text>
                                        <text class="menu-text">{{ $t('itemManage.batch_manage') }}</text>
                                    </view>
                                    <view class="menu-divider"></view>
                                    <view class="menu-item" @click="goToUsageRecords(item)">
                                        <text class="menu-icon-small">📋</text>
                                        <text class="menu-text">{{ $t('itemManage.usage_records') }}</text>
                                    </view>
                                    <view class="menu-divider"></view>
                                    <view class="menu-item" @click="handleRefresh(item.Id)">
                                        <text class="menu-icon-small">🔄</text>
                                        <text class="menu-text">{{ $t('common.refresh') || '同步刷新' }}</text>
                                    </view>
                                    <view class="menu-divider"></view>
                                    <view class="menu-item delete-item" @click="confirmDeleteItem(item)">
                                        <text class="menu-icon-small">🗑️</text>
                                        <text class="menu-text danger">{{ $t('common.delete') }}</text>
                                    </view>
                                </view>
                            </view>
                        </view>

                        <view class="card-row mid-row">
                            <text class="spec-text" v-if="item.Specification">{{ item.Specification }}</text>
                            <text class="info-divider" v-if="item.Specification">|</text>
                            <text class="factory-text">{{ item.Factory || '-' }}</text>
                        </view>

                        <view class="card-row bot-row">
                            <view class="stock-box">
                                <text class="label">{{ $t('itemManage.stock_label') }}</text>
                                <text class="stock-val"
                                    :class="{ 'warning': (item.AvailableStock || 0) < (item.Cardinality || 0) }">{{
                                        item.AvailableStock || 0 }}</text>
                                <text class="stock-divider" v-if="item.Cardinality">/</text>
                                <text class="base-val" v-if="item.Cardinality">{{ item.Cardinality }}</text>
                                <text class="unit" v-if="item.Unit">{{ item.Unit }}</text>
                            </view>
                            <text class="expiry-date">{{ $t('itemManage.expiry_date_label') }}: {{
                                getExpiryDateDisplay(item) }}</text>
                        </view>
                    </view>

                    <view v-if="itemList.length === 0 && !loading" class="empty-state">
                        <view class="empty-icon-bg">
                            <text class="empty-icon">📦</text>
                        </view>
                        <text class="empty-title">{{ $t('handover.empty') }}</text>
                        <text class="empty-desc">{{ $t('itemManage.empty_desc') || '暂无物品，请点击右下角添加' }}</text>
                    </view>
                </view>
            </view>
            <view class="bottom-spacer"></view>
            <FooterInfo />
        </scroll-view>

        <!-- Expanded Actions -->
        <view class="fab-actions" v-if="showActions" @click.stop>
            <view class="action-item" @click="handleRefresh" :style="{ animationDelay: '0ms' }">
                <text class="action-text">{{ $t('common.refresh') || '同步刷新' }}</text>
                <view class="action-btn refresh-btn">
                    <text class="action-icon">🔄</text>
                </view>
            </view>
            <view class="action-item" @click="goToPage('/pages/item-manage/item-import')"
                :style="{ animationDelay: '50ms' }">
                <text class="action-text">{{ $t('itemManage.import') }}</text>
                <view class="action-btn import-btn">
                    <text class="action-icon">📥</text>
                </view>
            </view>
            <view class="action-item" @click="goToPage('/pages/item-manage/item-add')"
                :style="{ animationDelay: '100ms' }">
                <text class="action-text">{{ $t('itemManage.add') }}</text>
                <view class="action-btn add-btn">
                    <text class="action-icon">+</text>
                </view>
            </view>
        </view>

        <!-- Floating Action Button -->
        <view class="fab-btn" @click.stop="toggleActions" :class="{ active: showActions }" v-if="!selectMode">
            <text class="main-icon">+</text>
        </view>

        <!-- Selection Footer -->
        <view class="selection-footer" v-if="selectMode">
            <button class="confirm-btn" :disabled="!selectedItem" @click="confirmSelection">
                {{ $t('itemManage.confirm_selection') }} {{ selectedItem ? `(${selectedItem.Name})` : '' }}
            </button>
        </view>

        <!-- Back To Top -->
        <BackToTop :scroll-top="scrollTop" @click="goTop" />

        <!-- Confirm Modal -->
        <ConfirmModal v-model:visible="showDeleteConfirm" :title="$t('common.tip')"
            :message="`${$t('handover.delete_confirm')} (${itemToDelete?.Name})`" @confirm="handleDeleteConfirm" />
    </view>
</template>

<script>
import FooterInfo from '@/components/FooterInfo/FooterInfo.vue';
import BackButton from '@/components/BackButton/BackButton.vue';
import BackToTop from '@/components/BackToTop/BackToTop.vue';
import request from '@/utils/request.js';
import { storage } from '@/utils/storage.js';
import dayjs from 'dayjs';
import CustomRefresher from '@/components/CustomRefresher/CustomRefresher.vue';
import ConfirmModal from '@/components/ConfirmModal/ConfirmModal.vue';

export default {
    components: {
        FooterInfo,
        BackButton,
        BackToTop,
        CustomRefresher,
        ConfirmModal
    },
    data() {
        return {
            statusBarHeight: 20,
            searchText: '',
            itemList: [],
            page: 1,
            rows: 10,
            total: 0,
            loading: false,
            isRefreshing: false,
            showActions: false,
            userOrgInfo: {},
            scrollTop: 0,
            oldScrollTop: 0,
            selectMode: false,
            selectedItem: null,
            currentCategory: null,
            activeMenuId: null,
            categories: [
                { label: this.$t('itemManage.all'), value: null },
                { label: this.$t('itemManage.medicine'), value: 10 },
                { label: this.$t('itemManage.consumable'), value: 20 },
                { label: this.$t('itemManage.equipment'), value: 30 },
                { label: this.$t('itemManage.other'), value: 0 }
            ],
            showDeleteConfirm: false,
            itemToDelete: null,
            navHeight: 44 // default in px for header content
        };
    },
    onLoad(options) {
        const sysInfo = uni.getSystemInfoSync();
        this.statusBarHeight = sysInfo.statusBarHeight || 20;

        // #ifdef MP-WEIXIN
        const menuButton = uni.getMenuButtonBoundingClientRect();
        this.navHeight = (menuButton.bottom - sysInfo.statusBarHeight) + 8;
        // #endif

        if (options && options.mode === 'select') {
            this.selectMode = true;
        }

        const uInfo = storage.get('userOrgInfo');
        if (uInfo) {
            this.userOrgInfo = uInfo;
        }

        this.fetchItems();
    },
    methods: {
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
        getExpiryStatus(item) {
            const dateStr = item.LatestExpirationDate || item.ExpiryDate;
            if (!dateStr) return null;
            const days = this.getDaysUntil(dateStr);
            if (days < 0) return { text: this.$t('itemManage.expired'), class: 'tag-expired' };
            if (days < 30) return { text: this.$t('itemManage.days_left', { days: days }), class: 'tag-urgent' };
            if (days <= 90) return { text: this.$t('itemManage.days_left', { days: days }), class: 'tag-warning' };
            return null;
        },
        getExpiryDateDisplay(item) {
            const dateStr = item.LatestExpirationDate || item.ExpiryDate;
            return dateStr ? this.formatDate(dateStr) : '-';
        },
        goBack() {
            uni.navigateBack();
        },
        onSearch() {
            this.page = 1;
            this.itemList = [];
            this.fetchItems();
        },
        clearSearch() {
            this.searchText = '';
            this.onSearch();
        },
        selectCategory(val) {
            this.currentCategory = val;
            this.onSearch();
        },
        onPulling() {
            // Can be used for visual feedback
        },
        onRestore() {
            this.isRefreshing = false;
        },
        onRefresh() {
            this.isRefreshing = true;
            this.page = 1;
            return this.fetchItems().then(() => {
                setTimeout(() => {
                    this.isRefreshing = false;
                }, 800);
            });
        },
        onLoadMore() {
            if (this.itemList.length < this.total) {
                this.page++;
                this.fetchItems();
            }
        },
        editItem(item) {
            uni.navigateTo({
                url: `/pages/item-manage/item-add?item=${encodeURIComponent(JSON.stringify(item))}`
            });
        },
        toggleMoreMenu(item) {
            if (this.activeMenuId === item.Id) {
                this.activeMenuId = null;
            } else {
                this.activeMenuId = item.Id;
            }
        },
        hideAllMenus() {
            this.activeMenuId = null;
            this.showActions = false;
        },
        handleItemClick(item) {
            if (this.selectMode) {
                this.selectedItem = item;
            } else {
                this.editItem(item);
            }
        },
        confirmSelection() {
            if (!this.selectedItem) return;
            uni.$emit('itemSelected', this.selectedItem);
            uni.navigateBack();
        },
        fetchItems() {
            if (this.loading) return Promise.resolve();
            this.loading = true;

            const wheres = [
            ];

            if (this.userOrgInfo && this.userOrgInfo.Organization && this.userOrgInfo.Organization.Id) {
                wheres.push({ name: 'OrganizationId', value: this.userOrgInfo.Organization.Id });
            }
            if (this.userOrgInfo && this.userOrgInfo.Department && this.userOrgInfo.Department.Id) {
                wheres.push({ name: 'DepartmentId', value: this.userOrgInfo.Department.Id });
            }

            if (this.searchText) {
                wheres.push({ name: 'Name', value: this.searchText, displayType: 'like' });
            }

            if (this.currentCategory !== null) {
                wheres.push({ name: 'Category', value: this.currentCategory });
            }

            return request.post('api/ssbh/Item/GetPageData', {
                page: this.page,
                rows: this.rows,
                sort: 'Id',
                order: 'desc',
                wheres: JSON.stringify(wheres)
            }).then(res => {
                if (res && res.rows) {
                    if (this.page === 1) {
                        this.itemList = res.rows;
                    } else {
                        this.itemList = [...this.itemList, ...res.rows];
                    }
                    this.total = res.total;
                }
                this.loading = false;
            }).catch(e => {
                console.error("Fetch Items Failed", e);
                this.loading = false;
            });
        },
        toggleActions() {
            this.showActions = !this.showActions;
        },
        onScroll(e) {
            this.scrollTop = e.detail.scrollTop;
            this.oldScrollTop = e.detail.scrollTop;
        },
        goTop() {
            this.scrollTop = this.oldScrollTop;
            this.$nextTick(() => {
                this.scrollTop = 0;
            });
        },

        goToPage(url) {
            this.hideAllMenus();
            uni.navigateTo({ url });
        },
        async handleRefresh(id) {
            this.hideAllMenus();
            try {
                uni.showLoading({ title: this.$t('common.syncing') });
                let url = 'api/ssbh/Item/Refresh';
                if (id) {
                    url += `?id=${id}`;
                }
                await request.post(url, {});
                uni.showToast({ title: this.$t('common.sync_success'), icon: 'success' });
                this.onRefresh();
            } catch (e) {
                console.error("Refresh Failed", e);
                uni.showToast({ title: this.$t('common.sync_fail'), icon: 'none' });
            } finally {
                uni.hideLoading();
            }
        },
        goToBatchManage(item) {
            this.hideAllMenus();
            uni.navigateTo({
                url: `/pages/item-manage/item-batch?itemId=${item.Id}&itemName=${encodeURIComponent(item.Name)}`
            });
        },
        goToUsageRecords(item) {
            this.hideAllMenus();
            uni.navigateTo({
                url: `/pages/statistics/statistics?category=consumption&itemName=${encodeURIComponent(item.Name)}&itemId=${item.Id}`
            });
        },
        confirmDeleteItem(item) {
            this.hideAllMenus();
            this.itemToDelete = item;
            this.showDeleteConfirm = true;
        },
        handleDeleteConfirm() {
            if (this.itemToDelete) {
                this.deleteItem(this.itemToDelete.Id);
            }
        },
        async deleteItem(id) {
            try {
                uni.showLoading({ title: this.$t('common.deleting') });
                await request.post('api/ssbh/Item/Del', [id]);
                uni.showToast({ title: this.$t('common.success'), icon: 'success' });
                this.page = 1;
                this.fetchItems();
            } catch (e) {
                console.error("Delete Failed", e);
                uni.showToast({ title: this.$t('common.fail'), icon: 'none' });
            } finally {
                uni.hideLoading();
            }
        }
    }
}
</script>

<style lang="scss" scoped>
// ... imported styles

// Previous styles...
.container {
    background-color: $sull-color-background;
    height: 100vh;
    display: flex;
    flex-direction: column;
}

.header-section {
    background-color: $sull-color-white;
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    z-index: 999;
    box-shadow: 0 1px 4px rgba(0, 0, 0, 0.03);

    .header-content {
        height: 44px;
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: 0 16px;

        .nav-left {
            width: 100rpx;
            display: flex;
            align-items: center;
        }

        .page-title {
            flex: 1;
            text-align: center;
            font-size: 34rpx;
            font-weight: 600;
            color: $sull-text-main;
        }

        .nav-right {
            width: 100rpx;
            height: 44px;
            position: relative;
            display: flex;
            justify-content: flex-end;
            align-items: center;

            .header-btn {
                width: 72rpx;
                height: 72rpx;
                background: #f0f7ff;
                border-radius: 12rpx;
                display: flex;
                align-items: center;
                justify-content: center;

                .header-icon {
                    font-size: 36rpx;
                }

                &:active {
                    opacity: 0.7;
                }
            }
        }
    }
}

.item-card {
    .card-actions {
        display: flex;
        align-items: center;
        gap: 16rpx;
        position: relative;

        .action-btn {
            width: 60rpx;
            height: 60rpx;
            display: flex;
            align-items: center;
            justify-content: center;
            border-radius: 50%;
            transition: background-color 0.2s;

            &:active {
                background-color: rgba(0, 0, 0, 0.05);
            }

            .action-icon {
                font-size: 36rpx;
                color: #999;
                font-weight: bold;
            }
        }

        .more-menu {
            position: absolute;
            top: 70rpx;
            right: 0;
            width: 240rpx;
            background: #ffffff;
            border-radius: 12rpx;
            box-shadow: 0 4rpx 24rpx rgba(0, 0, 0, 0.15);
            z-index: 100;
            padding: 8rpx 0;
            overflow: hidden;
            animation: fadeInDown 0.2s ease-out;

            .menu-item {
                display: flex;
                align-items: center;
                padding: 24rpx 32rpx;
                gap: 16rpx;

                &:active {
                    background-color: #f5f7fa;
                }

                .menu-icon-small {
                    font-size: 28rpx;
                }

                .menu-text {
                    font-size: 28rpx;
                    color: #333;
                    font-weight: 500;
                }
            }

            .menu-divider {
                height: 1rpx;
                background-color: #eee;
                margin: 0 24rpx;
            }

            .danger {
                color: #ff4d4f !important;
            }
        }
    }
}

.search-section {
    padding: 10px 16px;
    background: #fff;
    border-radius: 12px;
}

.search-box {
    background: $sull-color-background;
    border-radius: 999px;
    height: 36px;
    display: flex;
    align-items: center;
    padding: 0 12px;
}

.search-icon {
    font-size: 14px;
    margin-right: 8px;
    color: $sull-text-secondary;
}

@keyframes fadeInDown {
    from {
        opacity: 0;
        transform: translateY(-10rpx);
    }

    to {
        opacity: 1;
        transform: translateY(0);
    }
}

.search-input {
    flex: 1;
    font-size: 28rpx;
}

.clear-icon {
    color: $sull-text-secondary;
    font-size: 24rpx;
}

.category-tabs {
    display: flex;
    background: #fff;
    border-radius: 50rpx;
    padding: 8rpx;
    box-shadow: 0 4rpx 16rpx rgba(0, 0, 0, 0.02);
    margin: 0;
    overflow: visible;

    /* Hide scrollbar */
    &::-webkit-scrollbar {
        display: none;
    }

    .tab-item {
        flex: 1;
        height: 64rpx;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 24rpx;
        color: $sull-text-secondary;
        font-weight: 500;
        border-radius: 32rpx;
        transition: all 0.3s;

        // Reset previous card styles
        background: transparent;
        border: none;
        padding: 0;
        box-shadow: none;

        &.active {
            background: $sull-color-primary;
            color: #fff;
            font-weight: 600;
            box-shadow: 0 4rpx 12rpx rgba(0, 154, 151, 0.3);
            border: none;
        }
    }
}

.content {
    display: flex;
    flex-direction: column;
    padding: 32rpx;
    gap: 32rpx;
}

.main-scroll {
    flex: 1;
    height: 0;
}

.list-content {
    /* Padding removed as .content already handles it */
    min-height: 600rpx;
    display: flex;
    flex-direction: column;
    gap: 24rpx;
}

.item-card {
    background: $sull-color-white;
    border-radius: $radius-lg;
    padding: 24rpx 32rpx;
    margin-bottom: 24rpx;
    margin-bottom: 24rpx;
    box-shadow: 0 4rpx 16rpx rgba(0, 0, 0, 0.02);
    transition: all 0.2s;
    border: 4rpx solid transparent;
}

.item-card.selected {
    border-color: $sull-color-primary;
    background-color: #f0f7ff;
}

.list-content.with-footer {
    padding-bottom: 200rpx;
    /* Space for footer */
}

.card-row {
    display: flex;
    align-items: center;
}

.top-row {
    justify-content: space-between;
    margin-bottom: 12rpx;
}

.mid-row {
    margin-bottom: 16rpx;
    align-items: baseline;
}

.bot-row {
    justify-content: space-between;
    align-items: flex-end;
}

.name-section {
    flex: 1;
    display: flex;
    align-items: baseline;
    padding-right: 16rpx;
    overflow: hidden;
}

.item-name {
    font-size: 32rpx;
    font-weight: bold;
    color: $sull-text-main;
    margin-right: 12rpx;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    max-width: 70%;
}

.item-code {
    font-size: 24rpx;
    color: #999;
    flex-shrink: 0;
}

.status-tag {
    padding: 4rpx 16rpx;
    border-radius: 8rpx;
    font-size: 22rpx;
    font-weight: 500;
    flex-shrink: 0;
}

.tag-expired {
    background: #333;
    color: #fff;
}

.tag-urgent {
    background: #ff4d4f;
    color: #fff;
}

.tag-warning {
    background: #faad14;
    color: #fff;
}

.tag-normal {
    background: #f0f9eb;
    color: #E89310;
}

.spec-text {
    font-size: 24rpx;
    font-weight: bold;
    color: $sull-text-main;
    margin-right: 8rpx;
    max-width: 45%;
    overflow: hidden;
    white-space: nowrap;
    text-overflow: ellipsis;
}

.info-divider {
    margin: 0 8rpx;
    color: #eee;
}

.factory-text {
    flex: 1;
    font-size: 24rpx;
    color: #999;
    overflow: hidden;
    white-space: nowrap;
    text-overflow: ellipsis;
}

.stock-box {
    display: flex;
    align-items: baseline;
}

.stock-box .label {
    font-size: 24rpx;
    color: #999;
    margin-right: 8rpx;
}

.stock-box .stock-val {
    font-size: 36rpx;
    font-weight: bold;
    color: $sull-text-main;
    font-family: 'Roboto', sans-serif;

    &.warning {
        color: #ff4d4f;
    }
}

.stock-box .stock-divider {
    font-size: 24rpx;
    color: #ccc;
    margin: 0 4rpx;
}

.stock-box .base-val {
    font-size: 28rpx;
    color: #999;
    font-weight: 500;
}

.stock-box .unit {
    font-size: 24rpx;
    color: #606266;
    margin-left: 4rpx;
    font-weight: normal;
}

.expiry-date {
    font-size: 24rpx;
    color: #999;
}

.text-gray {
    color: #999;
}

.empty-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding-top: 120rpx;
}

.empty-icon-bg {
    width: 240rpx;
    height: 240rpx;
    background-color: #FFF5EE;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    margin-bottom: 48rpx;
}

.empty-icon {
    font-size: 120rpx;
}

.empty-title {
    font-size: 36rpx;
    font-weight: bold;
    color: $sull-text-main;
    margin-bottom: 16rpx;
}

.empty-desc {
    font-size: 28rpx;
    color: $sull-text-secondary;
}

.bottom-spacer {
    height: 160rpx;
}

/* FAB & Actions */
.fab-actions {
    position: fixed;
    bottom: calc(146px + env(safe-area-inset-bottom));
    /* 80px base + 66px offset */
    right: 24px;
    z-index: 1000;
    display: flex;
    flex-direction: column;
    align-items: flex-end;
    gap: 32rpx;
}

.action-item {
    display: flex;
    align-items: center;
    opacity: 0;
    transform: translateY(40rpx);
    animation: slideUp 0.3s forwards;
}

.action-text {
    background: rgba(0, 0, 0, 0.7);
    color: #fff;
    padding: 8rpx 16rpx;
    border-radius: 8rpx;
    font-size: 24rpx;
    margin-right: 16rpx;
    opacity: 0.8;
}

.action-btn {
    width: 96rpx;
    height: 96rpx;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    box-shadow: 0 8rpx 20rpx rgba(0, 0, 0, 0.2);

    .action-icon {
        font-size: 48rpx;
    }
}

.refresh-btn {
    background: #FF9F0A;
    color: #fff;
}

.add-btn {
    background: #52c41a;
    color: #fff;
}

.import-btn {
    background: #1890ff;
    color: #fff;
}

.fab-btn {
    position: fixed;
    bottom: calc(80px + env(safe-area-inset-bottom));
    right: 24px;
    width: 112rpx;
    height: 112rpx;
    background: $sull-color-primary;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    box-shadow: 0 8rpx 24rpx rgba(0, 122, 255, 0.3);
    z-index: 1000;
    transition: transform 0.3s;

    .main-icon {
        color: #fff;
        font-size: 56rpx;
    }

    &.active {
        transform: rotate(45deg);
    }

    &:active {
        transform: scale(0.95);
    }
}

@keyframes slideUp {
    to {
        opacity: 1;
        transform: translateY(0);
    }
}


.selection-footer {
    position: fixed;
    bottom: 0;
    left: 0;
    width: 100%;
    background: #fff;
    padding: 32rpx 48rpx;
    box-shadow: 0 -4rpx 20rpx rgba(0, 0, 0, 0.05);
    z-index: 1001;
    box-sizing: border-box;
    padding-bottom: calc(32rpx + env(safe-area-inset-bottom));
}

.confirm-btn {
    background: $sull-color-primary;
    color: #fff;
    height: 96rpx;
    border-radius: 48rpx;
    font-size: 32rpx;
    font-weight: bold;
    display: flex;
    align-items: center;
    justify-content: center;
    box-shadow: 0 8rpx 24rpx rgba(64, 158, 255, 0.4);

    &[disabled] {
        background: #ccc;
        box-shadow: none;
    }

    &:active {
        opacity: 0.9;
        transform: scale(0.98);
    }
}
</style>

<template>
    <view class="container">
        <!-- Navigation Bar -->
        <!-- Navigation Bar -->
        <view class="header-section" :style="{ paddingTop: statusBarHeight + 'px' }">
            <view class="header-content">
                <view class="nav-left">
                    <BackButton class="header-back-btn" @click="goBack" />
                </view>
                <text class="page-title">{{ $t('itemManage.title') }}</text>
                <view class="nav-right"></view>
            </view>
        </view>



        <!-- List -->
        <scroll-view class="main-scroll" scroll-y :enable-back-to-top="true" :refresher-enabled="true"
            :refresher-triggered="isRefreshing" @refresherrefresh="onRefresh" @scrolltolower="onLoadMore"
            @scroll="onScroll" :scroll-top="scrollTop" refresher-default-style="none" refresher-background="#f0f5ff"
            :style="{ marginTop: (statusBarHeight + 44) + 'px' }">

            <view slot="refresher" class="custom-refresher">
                <view class="refresher-content">
                    <view class="refresher-icon" :class="{ 'animating': isRefreshing }"></view>
                    <text class="refresher-text">{{ isRefreshing ? $t('index.refreshing') : $t('index.pullToRefresh')
                        }}</text>
                </view>
            </view>

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
                            <view class="status-tag" :class="getExpiryStatus(item.ExpiryDate).class"
                                v-if="item.ExpiryDate && getExpiryStatus(item.ExpiryDate)">
                                <text>{{ getExpiryStatus(item.ExpiryDate).text }}</text>
                            </view>
                        </view>

                        <view class="card-row mid-row">
                            <text class="spec-text" v-if="item.Specification">{{ item.Specification }}</text>
                            <text class="info-divider" v-if="item.Specification">|</text>
                            <text class="factory-text">{{ item.Factory || '-' }}</text>
                        </view>

                        <view class="card-row bot-row">
                            <view class="stock-box">
                                <text class="label">库存</text>
                                <text class="stock-val">{{ item.AvailableStock || 0 }}</text>
                                <text class="unit" v-if="item.Unit">{{ item.Unit }}</text>
                            </view>
                            <text class="expiry-date">过期日期: {{ item.ExpiryDate ? formatDate(item.ExpiryDate) : '-'
                                }}</text>
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
        <view class="fab-actions" v-if="showActions">
            <view class="action-item" @click="goToPage('/pages/item-manage/item-import')"
                :style="{ animationDelay: '0ms' }">
                <text class="action-text">{{ $t('itemManage.import') }}</text>
                <view class="action-btn import-btn">
                    <text class="action-icon">📥</text>
                </view>
            </view>
            <view class="action-item" @click="goToPage('/pages/item-manage/item-add')"
                :style="{ animationDelay: '50ms' }">
                <text class="action-text">{{ $t('itemManage.add') }}</text>
                <view class="action-btn add-btn">
                    <text class="action-icon">➕</text>
                </view>
            </view>
        </view>

        <!-- Floating Action Button -->
        <view class="fab-btn" @click="toggleActions" :class="{ active: showActions }" v-if="!selectMode">
            <text class="main-icon">➕</text>
        </view>

        <!-- Selection Footer -->
        <view class="selection-footer" v-if="selectMode">
            <button class="confirm-btn" :disabled="!selectedItem" @click="confirmSelection">
                确认选择 {{ selectedItem ? `(${selectedItem.Name})` : '' }}
            </button>
        </view>

        <!-- Back To Top -->
        <BackToTop :scroll-top="scrollTop" @click="goTop" />
    </view>
</template>

<script>
import FooterInfo from '@/components/FooterInfo/FooterInfo.vue';
import BackButton from '@/components/BackButton/BackButton.vue';
import BackToTop from '@/components/BackToTop/BackToTop.vue';
import request from '@/utils/request.js';

export default {
    components: {
        FooterInfo,
        BackButton,
        BackToTop
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
            categories: [
                { label: '全部', value: null },
                { label: '药品', value: 10 },
                { label: '耗材', value: 20 },
                { label: '设备', value: 30 },
                { label: '其他', value: 0 }
            ]
        };
    },
    onLoad(options) {
        const sysInfo = uni.getSystemInfoSync();
        this.statusBarHeight = sysInfo.statusBarHeight || 20;

        if (options && options.mode === 'select') {
            this.selectMode = true;
            // Optional: set title
            uni.setNavigationBarTitle({ title: '选择物品' });
        }

        const uInfo = uni.getStorageSync('userOrgInfo');
        if (uInfo) {
            this.userOrgInfo = uInfo.User;
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
        onRefresh() {
            this.isRefreshing = true;
            this.page = 1;
            this.fetchItems().then(() => {
                this.isRefreshing = false;
            });
        },
        onLoadMore() {
            if (this.itemList.length < this.total) {
                this.page++;
                this.fetchItems();
            }
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
        getExpiryStatus(dateStr) {
            const days = this.getDaysUntil(dateStr);
            if (days < 0) return { text: '已过期', class: 'tag-expired' };
            if (days < 30) return { text: `剩${days}天`, class: 'tag-urgent' };
            if (days <= 90) return { text: `剩${days}天`, class: 'tag-warning' };
            return null;
        },
        editItem(item) {
            uni.navigateTo({
                url: `/pages/item-manage/item-add?item=${encodeURIComponent(JSON.stringify(item))}`
            });
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

            if (this.userOrgInfo && this.userOrgInfo.OrganizationId) {
                wheres.push({ name: 'OrganizationId', value: this.userOrgInfo.OrganizationId });
            }
            if (this.userOrgInfo && this.userOrgInfo.DepartmentId) {
                wheres.push({ name: 'DepartmentId', value: this.userOrgInfo.DepartmentId });
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
            // Must toggle value to trigger scroll view update if it's already 0 (rare but safe)
            this.scrollTop = this.oldScrollTop;
            this.$nextTick(() => {
                this.scrollTop = 0;
            });
        },
        goToPage(url) {
            this.showActions = false;
            uni.navigateTo({ url });
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
            width: 40px;
        }

        .page-title {
            font-size: $font-size-h6;
            font-weight: $font-weight-semibold;
            color: $sull-text-main;
        }

        .nav-right {
            width: 40px;
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

.search-input {
    flex: 1;
    font-size: 14px;
}

.clear-icon {
    color: $sull-text-secondary;
    font-size: 12px;
}

.category-tabs {
    display: flex;
    background: #fff;
    border-radius: 25px;
    padding: 4px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.02);
    margin: 0;
    overflow: visible;

    /* Hide scrollbar */
    &::-webkit-scrollbar {
        display: none;
    }

    .tab-item {
        flex: 1;
        height: 32px;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 12px;
        color: $sull-text-secondary;
        font-weight: 500;
        border-radius: 16px;
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
            box-shadow: 0 2px 6px rgba(0, 154, 151, 0.3);
            border: none;
        }
    }
}

.content {
    display: flex;
    flex-direction: column;
    padding: 16px;
    gap: 16px;
}

.main-scroll {
    flex: 1;
    height: 0;
}

.list-content {
    /* Padding removed as .content already handles it */
    min-height: 300px;
    display: flex;
    flex-direction: column;
    gap: 12px;
}

.item-card {
    background: $sull-color-white;
    border-radius: $radius-lg;
    padding: 12px 16px;
    margin-bottom: 12px;
    margin-bottom: 12px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.02);
    transition: all 0.2s;
    border: 2px solid transparent;
}

.item-card.selected {
    border-color: $sull-color-primary;
    background-color: #f0f7ff;
}

.list-content.with-footer {
    padding-bottom: 100px;
    /* Space for footer */
}

.card-row {
    display: flex;
    align-items: center;
}

.top-row {
    justify-content: space-between;
    margin-bottom: 6px;
}

.mid-row {
    margin-bottom: 8px;
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
    padding-right: 8px;
    overflow: hidden;
}

.item-name {
    font-size: 16px;
    font-weight: bold;
    color: $sull-text-main;
    margin-right: 6px;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    max-width: 70%;
}

.item-code {
    font-size: 12px;
    color: #999;
    flex-shrink: 0;
}

.status-tag {
    padding: 2px 8px;
    border-radius: 4px;
    font-size: 11px;
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
    color: #67c23a;
}

.spec-text {
    font-size: 12px;
    font-weight: bold;
    color: $sull-text-main;
    margin-right: 4px;
    max-width: 45%;
    overflow: hidden;
    white-space: nowrap;
    text-overflow: ellipsis;
}

.info-divider {
    margin: 0 4px;
    color: #eee;
}

.factory-text {
    flex: 1;
    font-size: 12px;
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
    font-size: 12px;
    color: #999;
    margin-right: 4px;
}

.stock-box .stock-val {
    font-size: 18px;
    font-weight: bold;
    color: $sull-text-main;
    font-family: 'Roboto', sans-serif;
}

.stock-box .unit {
    font-size: 12px;
    color: #606266;
    margin-left: 2px;
    font-weight: normal;
}

.expiry-date {
    font-size: 12px;
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
    padding-top: 60px;
}

.empty-icon-bg {
    width: 120px;
    height: 120px;
    background-color: #FFF5EE;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    margin-bottom: 24px;
}

.empty-icon {
    font-size: 60px;
}

.empty-title {
    font-size: 18px;
    font-weight: bold;
    color: $sull-text-main;
    margin-bottom: 8px;
}

.empty-desc {
    font-size: 14px;
    color: $sull-text-secondary;
}

.bottom-spacer {
    height: 80px;
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
    gap: 16px;
}

.action-item {
    display: flex;
    align-items: center;
    opacity: 0;
    transform: translateY(20px);
    animation: slideUp 0.3s forwards;
}

.action-text {
    background: rgba(0, 0, 0, 0.7);
    color: #fff;
    padding: 4px 8px;
    border-radius: 4px;
    font-size: 12px;
    margin-right: 8px;
    opacity: 0.8;
}

.action-btn {
    width: 48px;
    height: 48px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.2);

    .action-icon {
        font-size: 24px;
    }
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
    width: 56px;
    height: 56px;
    background: $sull-color-primary;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    box-shadow: 0 4px 12px rgba(0, 122, 255, 0.3);
    z-index: 1000;
    transition: transform 0.3s;

    .main-icon {
        color: #fff;
        font-size: 28px;
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

/* Refresher */
/* Refresher */
.custom-refresher {
    height: 60px;
    width: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
}

.refresher-content {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 4px;
}

.refresher-icon {
    width: 32px;
    height: 32px;
    background-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='%23409EFF'%3E%3Cpath d='M19 14h-1V8h-3V6h-2v2h-2V6H9v2H7v6H6c-1.1 0-2 .9-2 2v3c0 1.1.9 2 2 2h1c0 1.66 1.34 3 3 3s3-1.34 3-3h4c0 1.66 1.34 3 3 3s3-1.34 3-3h1c1.1 0 2-.9 2-2v-3c0-1.1-.9-2-2-2zM8 19c-.55 0-1-.45-1-1s.45-1 1-1 1 .45 1 1-.45 1-1 1zm10 0c-.55 0-1-.45-1-1s.45-1 1-1 1 .45 1 1-.45 1-1 1zM8 8h2v2H8V8zm0 4h2v2H8v-2zm4 4h-2v-2h2v2zm0-4h-2V8h2v4zm4 4h-2v-2h2v2zm0-4h-2V8h2v4z'/%3E%3Cpath d='M4 8h2v2H4zm0 4h2v2H4z' fill-opacity='.3'/%3E%3C/svg%3E");
    background-size: contain;
    background-repeat: no-repeat;
    transition: transform 0.3s ease;
}

.refresher-icon.animating {
    animation: ambulance-bounce 0.8s infinite ease-in-out;
}

.refresher-text {
    font-size: 10px;
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

.selection-footer {
    position: fixed;
    bottom: 0;
    left: 0;
    width: 100%;
    background: #fff;
    padding: 16px 24px;
    box-shadow: 0 -2px 10px rgba(0, 0, 0, 0.05);
    z-index: 1001;
    box-sizing: border-box;
    padding-bottom: calc(16px + env(safe-area-inset-bottom));
}

.confirm-btn {
    background: $sull-color-primary;
    color: #fff;
    height: 48px;
    border-radius: 24px;
    font-size: 16px;
    font-weight: bold;
    display: flex;
    align-items: center;
    justify-content: center;
    box-shadow: 0 4px 12px rgba(64, 158, 255, 0.4);

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

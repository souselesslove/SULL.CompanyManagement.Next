<template>
    <view class="container">
        <!-- Custom Header -->
        <scroll-view class="main-scroll" scroll-y :enable-back-to-top="true" :refresher-enabled="true"
            :refresher-triggered="isRefreshing" :refresher-threshold="60" @refresherrefresh="onRefresh"
            @refresherpulling="onPulling" @refresherrestore="onRestore" @refresherabort="onRestore"
            @scrolltolower="onLoadMore" @scroll="onScroll" :scroll-top="scrollTopValue" refresher-default-style="none"
            refresher-background="#f0f5ff">
            <CustomRefresher slot="refresher" :isRefreshing="isRefreshing" :isPulling="isPulling" />

            <!-- Custom Header (Inside Scroll) -->
            <view class="custom-nav" :style="{ paddingTop: statusBarHeight * 2 + 'rpx' }">
                <view class="header-content">
                    <view class="nav-left">
                        <BackButton class="header-back-btn" @click="goBack" />
                    </view>
                    <text class="page-title">{{ $t('index.pendingTasks') }}</text>
                    <view class="nav-right"></view>
                </view>
            </view>

            <view class="content">
                <!-- Row 1: Search -->
                <view class="search-section">
                    <view class="search-box">
                        <text class="search-icon">🔍</text>
                        <input class="search-input" type="text" v-model="searchKeyword"
                            :placeholder="$t('common.search')" confirm-type="search" @confirm="handleSearch" />
                        <view class="icon-btn" v-if="searchKeyword" @click="clearSearch">
                            <text class="clear-icon">✕</text>
                        </view>
                    </view>
                </view>

                <!-- Row 2: Stats Grid -->
                <view class="stats-grid">
                    <view class="stat-card" :class="{ active: currentStatFilter === 'all' }"
                        @click="filterByStat('all')">
                        <text class="stat-value">{{ stats.total }}</text>
                        <text class="stat-label">{{ $t('index.todo.all') }}</text>
                    </view>
                    <view class="stat-card" :class="{ active: currentStatFilter === 'pending' }"
                        @click="filterByStat('pending')">
                        <text class="stat-value warning">{{ stats.pending }}</text>
                        <text class="stat-label">{{ $t('index.todo.pending') }}</text>
                    </view>
                    <view class="stat-card" :class="{ active: currentStatFilter === 'ignored' }"
                        @click="filterByStat('ignored')">
                        <text class="stat-value info">{{ stats.ignored }}</text>
                        <text class="stat-label">{{ $t('index.todo.ignored') }}</text>
                    </view>
                    <view class="stat-card" :class="{ active: currentStatFilter === 'processed' }"
                        @click="filterByStat('processed')">
                        <text class="stat-value success">{{ stats.processed }}</text>
                        <text class="stat-label">{{ $t('index.todo.processed') }}</text>
                    </view>
                </view>

                <!-- Row 3: Category Tabs -->
                <view class="category-tabs">
                    <view class="tab-item" v-for="(tab, index) in todoTabs" :key="index"
                        :class="{ active: currentCategoryTab === tab.key }" @click="onTabClick(tab.key)">
                        <text>{{ $t(tab.label) }}</text>
                    </view>
                </view>

                <!-- Row 4: List -->
                <view class="alert-list">
                    <view class="alert-card" v-for="(alert, index) in filteredList" :key="index"
                        @click="showTaskTimeline(alert)">
                        <view class="alert-left">
                            <view class="alert-type-tag" :class="getParamsClass(alert.Category)">
                                <text class="alert-type-text">{{ getCategoryText(alert.Category) }}</text>
                            </view>
                        </view>
                        <view class="alert-content">
                            <text class="alert-msg">{{ alert.Description }}</text>
                            <text class="alert-time">{{ alert.CreationTime }}</text>
                        </view>
                        <view class="alert-action"
                            v-if="alert.LatestAssignUserId == userInfo.UserId && alert.IsFinished == false">
                            <view class="icon-action-btn process-btn" @click.stop="handleProcess(alert)">
                                <view class="action-icon-svg icon-process"></view>
                            </view>
                            <view class="icon-action-btn ignore-btn" @click.stop="handleIgnore(alert)">
                                <view class="action-icon-svg icon-ignore"></view>
                            </view>
                        </view>
                    </view>

                    <view v-if="filteredList.length === 0" class="empty-state-simple">
                        <view class="empty-icon-simple"></view>
                        <text class="empty-text-simple">{{ $t('index.noTasks') }}</text>
                    </view>
                </view>

                <!-- Footer -->
                <FooterInfo :showFinished="allTodoList.length > 0 && allTodoList.length >= total" />
                <view class="bottom-spacer"></view>
            </view>
        </scroll-view>

        <BackToTop :scroll-top="scrollTop" @click="scrollToTop" />

        <!-- Timeline Modal -->
        <view class="modal-mask" v-if="showTimelineModal" @click="closeTimelineModal">
            <view class="modal-content timeline-modal" @click.stop>
                <view class="modal-header">
                    <text class="modal-title">{{ $t('index.timeline.title') }}</text>
                    <text class="close-btn" @click="closeTimelineModal">×</text>
                </view>
                <scroll-view scroll-y class="modal-body timeline-body">
                    <view v-if="timelineLoading" class="loading-state">
                        <text>{{ $t('index.timeline.loading') }}</text>
                    </view>
                    <view v-else-if="timelineData.length === 0" class="empty-state-simple">
                        <text class="empty-text-simple">{{ $t('index.timeline.empty') }}</text>
                    </view>
                    <view v-else class="timeline-list">
                        <view class="timeline-item" v-for="(item, index) in timelineData" :key="index">
                            <view class="timeline-node" :class="{
                                'is-start': index === 0 && !(index === timelineData.length - 1 && currentTimelineTask.IsFinished),
                                'is-end': index === timelineData.length - 1 && currentTimelineTask.IsFinished
                            }">
                                <view v-if="index === timelineData.length - 1 && currentTimelineTask.IsFinished"
                                    class="node-icon icon-end"></view>
                                <view v-else-if="index === 0" class="node-icon icon-start"></view>
                                <view v-else class="timeline-dot"></view>
                            </view>
                            <view class="timeline-line" v-if="index < timelineData.length - 1"></view>
                            <view class="timeline-content">
                                <view class="timeline-header">
                                    <text class="timeline-time">{{ formatTime(item.CreationTime) }}</text>
                                    <text class="timeline-creator">{{ item.Creator || $t('index.timeline.unknown')
                                        }}</text>
                                </view>
                                <view class="timeline-desc">{{ item.Description }}</view>
                                <view class="timeline-footer">
                                    <text class="timeline-status" v-if="item.Status">{{ $t('index.timeline.status') }}:
                                        {{ getStatusText(item.Status)
                                        }}</text>
                                    <text class="timeline-assign" v-if="item.AssignUser">{{
                                        $t('index.timeline.assignUser') }}: {{ item.AssignUser
                                        }}</text>
                                    <text class="timeline-remark" v-if="item.Remark">{{ $t('index.timeline.remark') }}:
                                        {{ item.Remark }}</text>
                                </view>
                            </view>
                        </view>
                    </view>
                </scroll-view>
            </view>
        </view>
    </view>
</template>

<script>
import BackButton from '@/components/BackButton/BackButton.vue';
import BackToTop from '@/components/BackToTop/BackToTop.vue';
import FooterInfo from '@/components/FooterInfo/FooterInfo.vue';
import request from '@/utils/request.js';
import { storage } from '@/utils/storage.js';
import dayjs from 'dayjs';
import CustomRefresher from '@/components/CustomRefresher/CustomRefresher.vue';

const NotifyStatus = {
    Completed: 200,
    Canceled: 210,
    Ignored: 220
};

export default {
    components: {
        BackButton,
        BackToTop,
        FooterInfo,
        CustomRefresher
    },
    data() {
        return {
            statusBarHeight: 20,
            scrollTop: 0,
            scrollTopValue: 0,
            isRefreshing: false,
            isPulling: false,
            loading: false, // Loading more state
            searchKeyword: '',
            userInfo: {},

            // Data
            allTodoList: [],
            filteredList: [], // Kept for template compatibility, will alias to allTodoList in new logic or used if mixed

            // Pagination
            page: 1,
            rows: 10,
            total: 0,

            // Filters
            currentStatFilter: 'all', // all, pending, ignored, processed
            currentCategoryTab: 'all',

            // Stats
            stats: {
                total: 0,
                pending: 0,
                ignored: 0,
                processed: 0
            },
            // ...

            // Config
            todoTabs: [
                { key: 'all', label: 'index.todo.all' },
                { key: 'expiry', label: 'index.todo.expiry' },
                { key: 'stock', label: 'index.todo.stock' },
                { key: 'abnormal', label: 'index.todo.abnormal' },
                { key: 'other', label: 'index.todo.other' }
            ],

            // Timeline
            showTimelineModal: false,
            timelineLoading: false,
            timelineData: [],
            currentTimelineTask: {}
        }
    },
    onLoad(options) {
        const sysInfo = uni.getSystemInfoSync();
        this.statusBarHeight = sysInfo.statusBarHeight || 20;
        this.userInfo = (storage.get('userOrgInfo') || {}).User || {};

        // Optional: pre-filter if passed from home e.g. ?filter=expired
        if (options.filter) {
            this.currentStatFilter = options.filter;
        }
        console.log(this.userInfo);
        this.fetchData();
        this.fetchStats();
    },
    methods: {
        goBack() {
            uni.navigateBack();
        },
        onScroll(e) {
            this.scrollTop = e.detail.scrollTop;
        },
        scrollToTop() {
            this.scrollTopValue = this.scrollTop;
            this.$nextTick(() => {
                this.scrollTopValue = 0;
            });
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
            this.page = 1;
            this.fetchData(() => {
                this.isRefreshing = false;
                this.isPulling = false;
                uni.showToast({ title: this.$t('index.refreshed'), icon: 'none' });
            });
        },
        onLoadMore() {
            if (this.allTodoList.length < this.total && !this.loading) {
                this.page++;
                this.fetchData();
            }
        },
        fetchData(callback) {
            if (this.loading && this.page > 1) return; // Allow refresh (page 1) to proceed even if loading
            this.loading = true;

            // Build Wheres
            const wheres = [];

            // 1. Search (Description or AssignUser)
            // Note: detailed OR logic might need backend support. For now assuming Name/Description search.
            if (this.searchKeyword) {
                // If backend supports complex search, use it. Otherwise standard 'Description' like.
                wheres.push({ name: 'Description', value: this.searchKeyword, displayType: 'like' });
            }

            // 2. Stat Filter
            // pending: IsFinished=false & Status!=220
            // ignored: Status=220
            // processed: IsFinished=true & Status!=220
            if (this.currentStatFilter === 'pending') {
                wheres.push({ name: 'IsFinished', value: false });
            } else if (this.currentStatFilter === 'ignored') {
                wheres.push({ name: 'LatestStatus', value: '220' });
            } else if (this.currentStatFilter === 'processed') {
                wheres.push({ name: 'IsFinished', value: true });
            }

            // 3. Category Filter
            if (this.currentCategoryTab !== 'all') {
                const categoryMap = {
                    'expiry': 10,
                    'stock': 20,
                    'abnormal': 30
                };
                const targetCat = categoryMap[this.currentCategoryTab];
                if (targetCat) {
                    wheres.push({ name: 'Category', value: targetCat });
                } else if (this.currentCategoryTab === 'other') {
                    // This is tricky with simple key-value wheres (0 OR 40). 
                    // If backend supports 'in', use it. Or just filter 0 for now as 'other' usually means 0.
                    // Let's try 'in' if supported, otherwise just ignore for 'other' or assume 0.
                    wheres.push({ name: 'Category', value: '0' }); // Simplifying to 0 for 'Other' tab safety
                }
            }

            request.post('api/ssbh/TodoList/GetPageData', {
                page: this.page,
                rows: this.rows,
                sort: 'CreationTime',
                order: 'desc',
                wheres: JSON.stringify(wheres)
            }).then(res => {
                const list = res.rows || [];
                if (this.page === 1) {
                    this.allTodoList = list;
                } else {
                    this.allTodoList = [...this.allTodoList, ...list];
                }
                this.filteredList = this.allTodoList; // In server-side pagination, filtered list IS the list
                this.total = res.total;

                // Note: Stats calculation here will only reflect the LOADED items, which is expected behavior for infinite scroll unless we have a separate stats API.
                // this.calculateStats(this.allTodoList);

                if (callback) callback();
                this.loading = false;
            }).catch(e => {
                console.error(e);
                if (callback) callback();
                this.loading = false;
            });
        },
        fetchStats() {
            request.get('api/ssbh/TodoList/Statistic', {}).then(res => {
                if (res) {
                    this.stats = {
                        total: res.AllCount || 0,
                        pending: res.PendingCount || 0,
                        ignored: res.IgnoredCount || 0,
                        processed: res.ProcessedCount || 0
                    };
                }
            }).catch(err => {
                console.error('Fetch Stats Failed', err);
            });
        },
        handleSearch() {
            this.page = 1;
            this.fetchData();
        },
        clearSearch() {
            this.searchKeyword = '';
            this.handleSearch();
        },
        filterByStat(statKey) {
            this.currentStatFilter = statKey;
            this.page = 1;
            this.fetchData();
        },
        onTabClick(key) {
            this.currentCategoryTab = key;
            this.page = 1;
            this.fetchData();
        },
        applyFilters() {
            // Deprecated: Filtering is now done on server
            this.filteredList = this.allTodoList;
        },
        // Helper Methods copied from index.vue
        getParamsClass(category) {
            switch (category) {
                case 10: return 'tag-danger';
                case 20: return 'tag-warning';
                case 30: return 'tag-danger';
                case 40: return 'tag-info';
                default: return 'tag-info';
            }
        },
        getCategoryText(category) {
            switch (category) {
                case 10: return this.$t('index.todo.expiry');
                case 20: return this.$t('index.todo.stock');
                case 30: return this.$t('index.todo.abnormal');
                case 40: return '通知';
                default: return this.$t('index.todo.other');
            }
        },
        handleIgnore(item) {
            uni.showModal({
                title: this.$t('index.timeline.ignoreTitle'),
                content: this.$t('index.timeline.ignoreContent'),
                success: (res) => {
                    if (res.confirm) {
                        const payload = {
                            TodoListId: item.Id,
                            Description: '已选择忽略',
                            Status: NotifyStatus.Ignored
                        };
                        request.post('api/ssbh/TodoListDetail/Add', {
                            mainData: payload
                        }).then(res => {
                            if (res.success || res.status) {
                                uni.showToast({ title: this.$t('index.timeline.ignoreSuccess'), icon: 'none' });
                                // Update local status instead of removing, since we have 'Ignored' filter now
                                item.Status = 220;
                                item.IsFinished = true; // Usually ignored is considered handled/finished
                                this.fetchStats(); // Update stats from server after action
                                // this.applyFilters(); // Filter is server side, might need refresh if currently viewing 'pending'
                                if (this.currentStatFilter === 'pending') {
                                    // Remove item locally or refresh
                                    const idx = this.allTodoList.indexOf(item);
                                    if (idx > -1) this.allTodoList.splice(idx, 1);
                                }
                            } else {
                                uni.showToast({ title: this.$t('index.timeline.fail'), icon: 'none' });
                            }
                        });
                    }
                }
            });
        },
        handleProcess(item) {
            // Reuse navigation logic
            const category = item.Category;
            const relatedId = item.RelatedId;
            switch (category) {
                case 10:
                case 20:
                    uni.reLaunch({
                        url: `/pages_rescue/bigscreen/rescue-car?type=${category}&id=${relatedId}`
                    });
                    break;
                case 30:
                case 40:
                    uni.navigateTo({
                        url: `/pages_handover/handover-check/handover-item-check?id=${relatedId}&todoListId=${item.Id}`
                    });
                    break;
                default:
                    uni.showToast({ title: 'Feature not available', icon: 'none' });
                    break;
            }
        },
        showTaskTimeline(alert) {
            this.currentTimelineTask = alert;
            this.showTimelineModal = true;
            this.timelineLoading = true;
            this.timelineData = [];

            const wheres = [{ name: 'TodoListId', value: alert.Id, displayType: 'numeric' }];
            request.post('api/ssbh/TodoListDetail/GetPageData', {
                page: 1, rows: 100, wheres: JSON.stringify(wheres), sort: 'CreationTime', order: 'asc'
            }).then(res => {
                this.timelineLoading = false;
                if (res.status === 0) {
                    this.timelineData = res.rows || [];
                } else {
                    uni.showToast({ title: 'Fetch Timeline Failed', icon: 'none' });
                }
            }).catch(err => {
                console.error(err);
                this.timelineLoading = false;
            });
        },
        closeTimelineModal() {
            this.showTimelineModal = false;
        },
        formatTime(timeStr) {
            if (!timeStr) return '';
            const date = dayjs(timeStr);
            if (!date.isValid()) return timeStr;
            return date.format('YYYY-MM-DD HH:mm');
        },
        getStatusText(status) {
            if (status == 200) return this.$t('index.timeline.completed');
            if (status == 210) return this.$t('index.timeline.canceled');
            if (status == 220) return this.$t('index.timeline.ignoreSuccess');
            return status;
        }
    }
}
</script>

<style lang="scss" scoped>
.container {
    height: 100vh;
    display: flex;
    flex-direction: column;
    background-color: $sull-color-background;
}

.header-section {
    /* Removed fixed positioning */
    background: #fff;
    box-shadow: 0 2rpx 4rpx rgba(0, 0, 0, 0.03);
}

.header-content {
    height: 88rpx;
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 0 32rpx;
}

.nav-left,
.nav-right {
    width: 80rpx;
    display: flex;
    align-items: center;
}

.header-back-btn {
    /* Reset absolute positioning */
    position: static;
}

.page-title {
    font-size: 34rpx;
    font-weight: bold;
    color: $sull-text-main;
}

.main-scroll {
    flex: 1;
    height: 0;
}

.content {
    padding: 32rpx;
    display: flex;
    flex-direction: column;
    gap: 32rpx;
}

/* Search */
.search-section {
    padding: 20rpx 32rpx;
    background: #fff;
}

.search-box {
    background: $sull-color-background;
    border-radius: 999px;
    height: 72rpx;
    display: flex;
    align-items: center;
    padding: 0 24rpx;
}

.search-icon {
    font-size: 28rpx;
    margin-right: 16rpx;
    color: $sull-text-secondary;
}

.search-input {
    flex: 1;
    font-size: 28rpx;
}

.clear-icon {
    color: $sull-text-secondary;
    font-size: 24rpx;
}

/* Stats */
.stats-grid {
    display: flex;
    justify-content: space-between;
    gap: 20rpx;
}

.stat-card {
    flex: 1;
    background: #fff;
    border-radius: 24rpx;
    padding: 24rpx 16rpx;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    box-shadow: 0 4rpx 8rpx rgba(0, 0, 0, 0.02);
    gap: 8rpx;
    transition: all 0.2s;
    border: 2rpx solid transparent;
}

.stat-card.active {
    border-color: $sull-color-primary;
    background: rgba($sull-color-primary, 0.05);
}

.stat-value {
    font-size: 40rpx;
    font-weight: bold;
    color: $sull-text-main;
}

.stat-label {
    font-size: 22rpx;
    color: $sull-text-secondary;
}

.stat-value.warning {
    color: $sull-color-warning;
}

.stat-value.info {
    color: $sull-text-secondary;
}

.stat-value.error {
    color: $sull-color-error;
}

.stat-value.success {
    color: $sull-color-success;
}

/* Category Tabs */
.category-tabs {
    display: flex;
    background: #fff;
    border-radius: 50rpx;
    padding: 8rpx;
    box-shadow: 0 4rpx 16rpx rgba(0, 0, 0, 0.02);
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
}

.tab-item.active {
    background: $sull-color-primary;
    color: #fff;
    font-weight: 600;
    box-shadow: 0 4rpx 12rpx rgba(0, 154, 151, 0.3);
}

/* Alert List (Reused from index but scoped here) */
.alert-list {
    display: flex;
    flex-direction: column;
    gap: 24rpx;
}

.alert-card {
    background: #fff;
    border-radius: 24rpx;
    padding: 32rpx;
    display: flex;
    align-items: center;
    box-shadow: 0 4rpx 16rpx rgba(0, 0, 0, 0.02);
}

.alert-left {
    margin-right: 24rpx;
}

.alert-type-tag {
    padding: 8rpx 16rpx;
    border-radius: 8rpx;
    font-size: 22rpx;
    font-weight: bold;
}

.tag-danger {
    background: #FEF0F0;
    color: #F56C6C;
}

.tag-warning {
    background: #FDF6EC;
    color: #E89310;
}

.tag-info {
    background: #F4F4F5;
    color: #909399;
}

.alert-content {
    flex: 1;
    display: flex;
    flex-direction: column;
}

.alert-msg {
    font-size: 28rpx;
    color: $sull-text-main;
    margin-bottom: 8rpx;
    line-height: 1.4;
    overflow: hidden;
    text-overflow: ellipsis;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    line-clamp: 2;
    -webkit-box-orient: vertical;
}

.alert-time {
    font-size: 22rpx;
    color: $sull-text-secondary;
}

.alert-action {
    margin-left: 24rpx;
    display: flex;
    gap: 16rpx;
}

.icon-action-btn {
    width: 64rpx;
    height: 64rpx;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
}

.process-btn {
    background: #EAF2FF;
    border: 2rpx solid #D0E7FF;
}

.ignore-btn {
    background: #F4F4F5;
    border: 2rpx solid #E9E9EB;
}

.action-icon-svg {
    width: 32rpx;
    height: 32rpx;
    background-color: $sull-color-primary;
    -webkit-mask-size: contain;
    mask-size: contain;
    -webkit-mask-repeat: no-repeat;
    mask-repeat: no-repeat;
    -webkit-mask-position: center;
    mask-position: center;
}

.icon-process {
    -webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M9 16.2L4.8 12l-1.4 1.4L9 19 21 7l-1.4-1.4L9 16.2z'/%3E%3C/svg%3E");
    mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M9 16.2L4.8 12l-1.4 1.4L9 19 21 7l-1.4-1.4L9 16.2z'/%3E%3C/svg%3E");
}

.icon-ignore {
    background-color: #909399;
    -webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M12 4.5C7 4.5 2.73 7.61 1 12c1.73 4.39 6 7.5 11 7.5s9.27-3.11 11-7.5c-1.73-4.39-6-7.5-11-7.5zM12 17c-2.76 0-5-2.24-5-5s2.24-5 5-5 5 2.24 5 5-2.24 5-5 5zm0-8c-1.66 0-3 1.34-3 3s1.34 3 3 3 3-1.34 3-3-1.34-3-3-3z'/%3E%3C/svg%3E");
    mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M12 4.5C7 4.5 2.73 7.61 1 12c1.73 4.39 6 7.5 11 7.5s9.27-3.11 11-7.5c-1.73-4.39-6-7.5-11-7.5zM12 17c-2.76 0-5-2.24-5-5s2.24-5 5-5 5 2.24 5 5-2.24 5-5 5zm0-8c-1.66 0-3 1.34-3 3s1.34 3 3 3 3-1.34 3-3-1.34-3-3-3z'/%3E%3C/svg%3E");
}

/* Simple Empty State */
.empty-state-simple {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 80rpx 0;
}

.empty-icon-simple {
    width: 96rpx;
    height: 96rpx;
    background-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='%23DCDFE6'%3E%3Cpath d='M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm-5 14H7v-2h7v2zm3-4H7v-2h10v2zm0-4H7V7h10v2z'/%3E%3C/svg%3E");
    background-size: contain;
    background-repeat: no-repeat;
    margin-bottom: 16rpx;
    opacity: 0.5;
}

.empty-text-simple {
    font-size: 28rpx;
    color: $sull-text-secondary;
}

.bottom-spacer {
    height: 80rpx;
}

/* Timeline Reused Styles */
.timeline-modal {
    height: 60vh;
    display: flex;
    flex-direction: column;
}

.modal-mask {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0, 0, 0, 0.5);
    z-index: 1000;
    display: flex;
    align-items: center;
    justify-content: center;
}

.modal-content {
    width: 80%;
    background: #fff;
    border-radius: 24rpx;
    overflow: hidden;
}

.modal-header {
    padding: 32rpx;
    border-bottom: 2rpx solid #EBEEF5;
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.modal-title {
    font-size: 32rpx;
    font-weight: bold;
}

.close-btn {
    font-size: 40rpx;
    color: #999;
}

.timeline-body {
    flex: 1;
    padding: 40rpx;
    box-sizing: border-box;
}

.timeline-list {
    padding-left: 20rpx;
}

.timeline-item {
    position: relative;
    padding-left: 40rpx;
    padding-bottom: 48rpx;
}

.timeline-item:last-child {
    padding-bottom: 0;
}

.timeline-node {
    position: absolute;
    left: 0;
    top: 0;
    width: 40rpx;
    height: 100%;
    display: flex;
    justify-content: center;
}

.timeline-dot {
    width: 16rpx;
    height: 16rpx;
    border-radius: 50%;
    background-color: $sull-color-primary;
    margin-top: 32rpx;
    z-index: 1;
}

.node-icon {
    width: 32rpx;
    height: 32rpx;
    margin-top: 24rpx;
    z-index: 1;
    background-color: $sull-color-primary;
    -webkit-mask-size: contain;
    mask-size: contain;
    -webkit-mask-repeat: no-repeat;
    mask-repeat: no-repeat;
    -webkit-mask-position: center;
    mask-position: center;
}

.icon-start {
    -webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M8 5v14l11-7z'/%3E%3C/svg%3E");
    mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M8 5v14l11-7z'/%3E%3C/svg%3E");
}

.icon-end {
    background-color: $sull-color-success;
    -webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41z'/%3E%3C/svg%3E");
    mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41z'/%3E%3C/svg%3E");
}

.timeline-line {
    position: absolute;
    left: 18rpx;
    top: 48rpx;
    bottom: -20rpx;
    width: 4rpx;
    background-color: #E4E7ED;
}

.timeline-item:last-child .timeline-line {
    display: none;
}

.timeline-node.is-start~.timeline-line {
    top: 56rpx;
}

.timeline-content {
    background: #F8F9FB;
    padding: 24rpx;
    border-radius: 16rpx;
    margin-left: 20rpx;
}

.timeline-header {
    display: flex;
    justify-content: space-between;
    margin-bottom: 12rpx;
    font-size: 24rpx;
    color: #909399;
}

.timeline-desc {
    font-size: 28rpx;
    color: #303133;
    margin-bottom: 12rpx;
    line-height: 1.4;
}

.timeline-footer {
    font-size: 24rpx;
    color: #606266;
    display: flex;
    flex-wrap: wrap;
    gap: 16rpx;
}

.timeline-status {
    color: $sull-color-primary;
}

.loading-state {
    text-align: center;
    padding: 40rpx;
    color: #999;
}
</style>

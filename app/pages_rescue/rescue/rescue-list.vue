<template>
    <view class="container">
        <!-- Navigation Bar -->
        <view class="custom-nav" :style="{ paddingTop: statusBarHeight * 2 + 'rpx' }">
            <view class="nav-content">
                <view class="nav-left">
                    <BackButton class="nav-back-btn" @click="goBack" />
                </view>
                <text class="nav-title">{{ $t('rescue.title') }}</text>
                <view class="nav-right"></view>
            </view>
        </view>

        <view class="content" :style="{ marginTop: (statusBarHeight * 2 + 88) + 'rpx' }">
            <!-- Header Info -->
            <view class="header-info">
                <text class="car-name">{{ $t('rescue.current_car') }}: {{ rescueCarName || $t('common.unknown')
                }}</text>
            </view>

            <!-- Statistics Card -->
            <view class="stats-card">
                <view class="stat-item">
                    <text class="stat-value">{{ stats.total || 0 }}</text>
                    <text class="stat-label">{{ $t('rescue.stats.total') }}</text>
                </view>
                <view class="stat-item">
                    <text class="stat-value">{{ stats.monthCount || 0 }}</text>
                    <text class="stat-label">{{ $t('rescue.stats.month') }}</text>
                </view>
                <view class="stat-item">
                    <text class="stat-value">{{ stats.lastTime || '-' }}</text>
                    <text class="stat-label">{{ $t('rescue.stats.last') }}</text>
                </view>
            </view>

            <!-- List -->
            <scroll-view class="list-container" scroll-y @scrolltolower="loadMore" :refresher-enabled="true"
                :refresher-triggered="refreshing" :refresher-threshold="60" @refresherrefresh="onRefresh"
                @refresherpulling="onPulling" @refresherrestore="onRestore" @refresherabort="onRestore"
                refresher-default-style="none" refresher-background="#f5f7fa">
                <CustomRefresher slot="refresher" :isRefreshing="refreshing" :isPulling="isPulling" />
                <view class="list-content">
                    <view class="rescue-card" v-for="(item, index) in list" :key="index" @click="goDetail(item)">
                        <view class="card-header">
                            <view class="header-patient">
                                <text class="header-name">{{ item.Name || $t('common.unknown') }}</text>
                                <text class="header-detail">{{ getGenderText(item.Gender) }} {{ item.Age ? item.Age +
                                    $t('rescue.age_unit') : '' }}</text>
                            </view>
                            <view class="status-badge" :class="getStatusClass(item.Status)">
                                {{ getStatusText(item.Status) }}
                            </view>
                        </view>
                        <view class="card-body">
                            <!-- Info Grid -->
                            <view class="info-grid">
                                <view class="info-row">
                                    <text class="label">{{ $t('rescue.start_time') }}:</text>
                                    <text class="value">{{ formatTime(item.StartTime) }}</text>
                                </view>
                                <view class="info-row">
                                    <text class="label">{{ $t('rescue.end_time') }}:</text>
                                    <text class="value">{{ formatTime(item.EndTime) || '-' }}</text>
                                </view>
                                <view class="info-row">
                                    <text class="label">{{ $t('rescue.duration') }}:</text>
                                    <text class="value">{{ calculateDuration(item.StartTime, item.EndTime) }}</text>
                                </view>
                                <view class="info-row">
                                    <text class="label">{{ $t('rescue.participants_count') }}:</text>
                                    <text class="value">{{ item.Participants ? item.Participants.split(',').length : 0
                                    }}{{ $t('rescue.person_unit') }}</text>
                                </view>
                                <view class="info-row">
                                    <text class="label">{{ $t('rescue.creator') }}:</text>
                                    <text class="value">{{ item.CreatorUser || '-' }}</text>
                                </view>
                                <view class="info-row">
                                    <text class="label">{{ $t('rescue.auditor') }}:</text>
                                    <text class="value">{{ item.AuditUser || '-' }}</text>
                                </view>
                                <view class="info-row full-width">
                                    <text class="label">{{ $t('rescue.effect') }}:</text>
                                    <text class="value highlight">{{ item.Effect || '-' }}</text>
                                </view>
                            </view>
                        </view>
                        <view class="card-footer" v-if="showActions(item)">
                            <view class="btn-group" @click.stop>
                                <button class="action-btn complete" v-if="item.Status === 0"
                                    @click="handleAction(item, 'complete')">{{ $t('rescue.actions.complete') }}</button>
                                <button class="action-btn cancel" v-if="item.Status === 0"
                                    @click="handleAction(item, 'cancel')">{{ $t('rescue.actions.cancel') }}</button>
                                <button class="action-btn audit" v-if="item.Status === 20"
                                    @click="handleAction(item, 'audit')">{{ $t('rescue.actions.audit') }}</button>
                            </view>
                        </view>
                    </view>
                    <view class="loading-more" v-if="loading">{{ $t('handover.loading') }}</view>
                    <view class="no-more" v-if="!hasMore && list.length > 0">{{ $t('index.noMoreData') }}</view>
                    <view class="empty-state" v-if="!loading && list.length === 0">
                        <text class="empty-text">{{ $t('rescue.no_records') }}</text>
                    </view>
                </view>
            </scroll-view>
        </view>

        <FooterInfo />
        <WarningModal v-model:visible="warningModal.visible" :title="warningModal.title" :message="warningModal.message"
            :buttonText="warningModal.buttonText" :showCancel="warningModal.showCancel" @confirm="onWarningConfirm"
            @cancel="onWarningCancel" />
    </view>
</template>

<script>
import BackButton from '@/components/BackButton/BackButton.vue';
import FooterInfo from '@/components/FooterInfo/FooterInfo.vue';
import WarningModal from '@/components/WarningModal/WarningModal.vue';
import CustomRefresher from '@/components/CustomRefresher/CustomRefresher.vue';
import request from '@/utils/request.js';
import { storage } from '@/utils/storage.js';
import dayjs from 'dayjs';

export default {
    components: {
        BackButton,
        FooterInfo,
        WarningModal,
        CustomRefresher
    },
    data() {
        return {
            statusBarHeight: 20,
            carId: '',
            rescueCarName: '',
            stats: {
                total: 0,
                monthCount: 0,
                lastTime: '-'
            },
            list: [],
            page: 1,
            pageSize: 5,
            total: 0,
            loading: false,
            refreshing: false,
            isPulling: false,
            hasMore: true,
            warningModal: {
                visible: false,
                title: '',
                message: '',
                buttonText: '',
                showCancel: false,
                type: null,
                targetItem: null
            }
        };
    },
    onLoad(options) {
        const sysInfo = uni.getSystemInfoSync();
        this.statusBarHeight = sysInfo.statusBarHeight || 20;

        this.carId = options.carId || options.id || '';
        if (!this.carId) {
            const lastId = storage.get('lastSelectedRescueCarId');
            if (lastId) this.carId = lastId;
        }

        // Try to get name from storage
        const currentCar = storage.get('currentRescueCarInfo');
        // Use loose comparison to handle potential string/number mismatch
        if (currentCar && currentCar.Id == this.carId) {
            this.rescueCarName = currentCar.Name || currentCar.DisplayName;
        } else if (this.carId) {
            // If we have carId but no name (maybe first visit to this page from URL), 
            // the name will be "Unknown" until we fetch detailed info if we wanted to
            // But for now, let's at least try to load it from a car list if we had one
        }

        this.fetchStats();
        this.loadList(true);
    },
    methods: {
        goBack() {
            uni.navigateBack();
        },
        formatTime(str) {
            if (!str) return '';
            return dayjs(str).format('YYYY-MM-DD HH:mm');
        },
        calculateDuration(start, end) {
            if (!start) return '-';
            const s = dayjs(start);
            const e = end ? dayjs(end) : dayjs();
            const diff = e.diff(s, 'minute');
            const h = Math.floor(diff / 60);
            const m = diff % 60;
            if (h > 0) return `${h}${this.$t('index.unit.hour') || 'h'}${m}${this.$t('index.unit.minute') || 'm'}`;
            return `${m}${this.$t('index.unit.minute') || 'm'}`;
        },
        getStatusText(status) {
            const map = {
                0: this.$t('rescue.status.active'),
                20: this.$t('rescue.status.finished'),
                10: this.$t('rescue.status.cancelled'),
                100: this.$t('handover.audited'),
            };
            return map[status] || this.$t('common.unknown');
        },
        getGenderText(gender) {
            if (gender === 1) return this.$t('profile.male');
            if (gender === 2) return this.$t('profile.female');
            return this.$t('common.unknown');
        },
        getStatusClass(status) {
            if (status === 0) return 'status-active';
            if (status === 100) return 'status-success';
            if (status === 20) return 'status-warning';
            if (status === 10) return 'status-gray';
            return 'status-primary';
        },
        showActions(item) {
            return item.Status === 0 || item.Status === 20;
        },
        fetchStats() {
            if (!this.carId) return;
            const query = {
                wheres: JSON.stringify([{ name: 'CartId', value: this.carId }])
            };
            request.post('api/ssbh/Rescue/Statistics', query).then(res => {
                const data = res.row || res.data || res;
                if (data) {
                    this.stats = {
                        total: data.Total || 0,
                        monthCount: data.MonthCount || 0,
                        lastTime: data.lastTime ? dayjs(data.lastTime).format('YYYY-MM-DD') : '-'
                    };
                }
            }).catch(e => {
                console.warn("Stats fetch failed", e);
            });
        },
        loadList(reset = false) {
            if (reset) {
                this.page = 1;
                this.hasMore = true;
                this.loading = true;
            }
            // Ensure car name is set if carId is available but name isn't (e.g., direct link)
            if (this.carId && !this.rescueCarName) {
                const currentCar = storage.get('currentRescueCarInfo');
                if (currentCar && currentCar.Id == this.carId) {
                    this.rescueCarName = currentCar.Name || currentCar.DisplayName;
                }
            }
            const query = {
                page: this.page,
                rows: this.pageSize,
                sort: 'StartTime',
                order: 'desc',
                wheres: JSON.stringify([{ name: 'CartId', value: this.carId }])
            };

            request.post('api/ssbh/Rescue/GetPageData', query).then(res => {
                const rows = res.rows || [];
                if (reset) this.list = rows;
                else this.list = [...this.list, ...rows];

                this.total = res.total || 0;
                this.hasMore = this.list.length < this.total;
                this.loading = false;
                this.refreshing = false;
                this.isPulling = false;

                // If stats endpoint fails, maybe update total here
                if (this.stats.total === 0 && this.total > 0) this.stats.total = this.total;
            }).catch(e => {
                this.loading = false;
                this.refreshing = false;
                this.isPulling = false;
                uni.showToast({ title: this.$t('common.load_fail'), icon: 'none' });
            });
        },
        loadMore() {
            if (this.hasMore && !this.loading) {
                this.page++;
                this.loading = true;
                this.loadList(false);
            }
        },
        onRefresh() {
            this.refreshing = true;
            this.isPulling = true;
            this.fetchStats();
            this.loadList(true);
        },
        onPulling() {
            this.isPulling = true;
        },
        onRestore() {
            this.isPulling = false;
        },
        handleAction(item, type) {
            let title = '';
            let msg = '';
            // 0: status active -> 10 (complete) or 40 (cancel)
            if (type === 'complete') {
                title = this.$t('rescue.actions.complete_confirm_title');
                msg = this.$t('rescue.actions.complete_confirm_msg');
            } else if (type === 'cancel') {
                title = this.$t('rescue.actions.cancel_confirm_title');
                msg = this.$t('rescue.actions.cancel_confirm_msg');
            } else if (type === 'audit') {
                title = this.$t('rescue.actions.audit_confirm_title');
                msg = this.$t('rescue.actions.audit_confirm_msg');
            }

            this.warningModal = {
                visible: true,
                title: title,
                message: msg,
                buttonText: this.$t('common.confirm'),
                showCancel: true,
                type: type,
                targetItem: item
            };
        },
        onWarningConfirm() {
            const { type, targetItem } = this.warningModal;
            if (!targetItem) return;

            let newStatus = targetItem.Status;
            if (type === 'complete') newStatus = 20;
            if (type === 'cancel') newStatus = 10;
            if (type === 'audit') newStatus = 100;

            // Call Update
            const mainData = { ...targetItem, Status: newStatus };
            // If completing, maybe update EndTime if not set?
            if (type === 'complete' && !mainData.EndTime) {
                mainData.EndTime = dayjs().format('YYYY-MM-DD HH:mm:ss');
            }

            request.post('api/ssbh/Rescue/Update', { mainData }).then(res => {
                if (res.success || res.status) {
                    uni.showToast({ title: this.$t('common.success'), icon: 'success' });
                    this.warningModal.visible = false;
                    this.onRefresh();
                } else {
                    uni.showToast({ title: res.msg || this.$t('common.fail'), icon: 'none' });
                }
            });
        },
        onWarningCancel() {
            this.warningModal.visible = false;
        },
        goDetail(item) {
            storage.set('currentRescueTask', item);
            uni.navigateTo({
                url: `/pages_rescue/rescue/rescue-record?id=${item.Id}`
            });
        }
    }
}
</script>

<style lang="scss" scoped>
page {
    background-color: #F5F7FA;
}

.container {
    padding-bottom: 200rpx;
}

.custom-nav {
    background: #fff;
    position: fixed;
    top: 0;
    width: 100%;
    z-index: 999;
}

.nav-content {
    height: 88rpx;
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 0 30rpx;
}

.nav-title {
    font-size: 36rpx;
    font-weight: bold;
    color: #333;
}

.header-info {
    padding: 20rpx 30rpx 0 30rpx;
}

.car-name {
    font-size: 32rpx;
    font-weight: bold;
    color: #333;
    padding-left: 20rpx;
    border-left: 8rpx solid $sull-color-primary;
}

.stats-card {
    display: flex;
    justify-content: space-around;
    background: #fff;
    margin: 20rpx 30rpx;
    padding: 40rpx 0;
    border-radius: 20rpx;
    box-shadow: 0 4rpx 16rpx rgba(0, 0, 0, 0.05);
}

.stat-item {
    display: flex;
    flex-direction: column;
    align-items: center;
}

.stat-value {
    font-size: 40rpx;
    font-weight: bold;
    color: $sull-color-primary;
    margin-bottom: 10rpx;
}

.stat-label {
    font-size: 26rpx;
    color: #909399;
}

.list-container {
    height: calc(100vh - 360rpx);
    width: 100%;
}

.list-content {
    padding: 0 30rpx;
}

.rescue-card {
    background: #fff;
    border-radius: 20rpx;
    padding: 30rpx;
    margin-bottom: 24rpx;
    box-shadow: 0 2rpx 12rpx rgba(0, 0, 0, 0.03);
}

.card-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding-bottom: 20rpx;
    border-bottom: 1rpx solid #EBEEF5;
    margin-bottom: 20rpx;
}

.header-patient {
    display: flex;
    align-items: center;
}

.header-name {
    font-size: 34rpx;
    font-weight: bold;
    color: #333;
    margin-right: 20rpx;
}

.header-detail {
    font-size: 26rpx;
    color: #666;
}

.status-badge {
    padding: 4rpx 12rpx;
    border-radius: 8rpx;
    font-size: 24rpx;
}

.status-active {
    background: #E6F7FF;
    color: #1890FF;
}

/* Blue */
.status-success {
    background: #F0F9EB;
    color: #67C23A;
}

/* Green */
.status-warning {
    background: #FDF6EC;
    color: #E6A23C;
}

/* Orange */
.status-gray {
    background: #F4F4F5;
    color: #909399;
}

/* Gray */

.card-body {
    margin-bottom: 20rpx;
}

.patient-info {
    display: flex;
    align-items: center;
    margin-bottom: 20rpx;
    padding-bottom: 20rpx;
    border-bottom: 1rpx dashed #EBEEF5;
}

.patient-name {
    font-size: 32rpx;
    font-weight: bold;
    color: #333;
    margin-right: 20rpx;
}

.patient-detail {
    font-size: 26rpx;
    color: #666;
}

.info-grid {
    display: flex;
    flex-wrap: wrap;
    margin-left: -10rpx;
    margin-right: -10rpx;
}

.info-row {
    width: 50%;
    /* Two columns */
    padding: 0 10rpx;
    box-sizing: border-box;
    display: flex;
    margin-bottom: 16rpx;
    font-size: 26rpx;
}

.info-row.full-width {
    width: 100%;
}

.label {
    color: #909399;
    flex-shrink: 0;
    margin-right: 10rpx;
}

.value {
    color: #606266;
    flex: 1;
}

.highlight {
    color: #303133;
    font-weight: 500;
}

.card-footer {
    border-top: 1rpx solid #EBEEF5;
    padding-top: 20rpx;
    display: flex;
    justify-content: flex-end;
}

.btn-group {
    display: flex;
    gap: 20rpx;
}

.action-btn {
    font-size: 26rpx;
    padding: 10rpx 30rpx;
    border-radius: 30rpx;
    border: 1rpx solid #DCDFE6;
    background: #fff;
    line-height: normal;
}

.action-btn.complete {
    color: #1890FF;
    border-color: #1890FF;
    background: #E6F7FF;
}

.action-btn.cancel {
    color: #909399;
    border-color: #DCDFE6;
}

.action-btn.audit {
    color: #E6A23C;
    border-color: #E6A23C;
    background: #FDF6EC;
}

.loading-more,
.no-more,
.empty-state {
    text-align: center;
    padding: 30rpx;
    color: #909399;
    font-size: 26rpx;
}
</style>

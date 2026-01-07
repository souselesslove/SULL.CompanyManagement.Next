<template>
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
                    <text class="floor-sub">已用 {{ getOccupiedCount(floor) }} / 共 {{ floor.SerialId || 0 }} 格</text>
                </view>
                <!-- Status Icons -->
                <view class="floor-status-icons">
                    <view class="status-icon bg-red-light" v-if="hasExpiryProblem(floor)">
                        <view class="status-bg-icon icon-clock"></view>
                    </view>
                    <view class="status-icon bg-orange-light" v-if="hasStockProblem(floor)">
                        <view class="status-bg-icon icon-siren"></view>
                    </view>
                </view>

                <!-- Action Button (Optional) -->
                <view class="floor-action" v-if="showFloorAction" @click.stop="handleFloorAction(floor)">
                    <text class="action-icon">⋮</text>
                </view>
            </view>

            <!-- Expanded Details (Serials) -->
            <view class="serial-grid-wrapper" v-if="floor.expanded">
                <view class="serial-grid">
                    <view class="serial-item" v-for="(serial, index) in getSerialsByFloor(floor.Id)" :key="serial.Id"
                        :id="'serial-' + serial.Id"
                        :class="{ 'highlight-glow': highlightedSerialIds.includes(serial.Id), 'expiry-warning': isSerialExpiryWarning(serial) }"
                        @click="handleSerialClick(serial)">
                        <view class="serial-header">
                            <view class="serial-no-wrapper">
                                <view class="alert-bell" v-if="shouldShowAlert(serial)"
                                    :class="getAlertDotClass(serial)"></view>
                                <text class="serial-no">{{ serial.SerialId || serial.OrderBy || (index + 1) }}</text>
                            </view>
                            <view class="serial-action" v-if="showSerialAction"
                                @click.stop="handleSerialAction(serial)">
                                <text>⋮</text>
                            </view>
                        </view>

                        <!-- Occupied Content -->
                        <view class="item-detail" v-if="serial.ItemId > 0 && itemsMap[serial.ItemId]">
                            <view class="item-name-wrapper">
                                <text class="item-name-text"
                                    :class="[{ scrolling: $getStringLen(itemsMap[serial.ItemId].Name || '') > 12 }, getItemNameColorClass(itemsMap[serial.ItemId])]">{{
                                        itemsMap[serial.ItemId].Name }}</text>
                            </view>
                            <view class="item-spec-row">
                                <text class="item-spec">{{ itemsMap[serial.ItemId].Specification || '-' }}</text>
                            </view>
                            <view class="item-footer-row">
                                <view class="item-stock-simple"
                                    :class="{ 'text-orange-bold': isStockInsufficient(itemsMap[serial.ItemId]) }">
                                    <text class="current-num">{{ itemsMap[serial.ItemId].AvailableStock || 0 }}</text>
                                    <text class="total-num">/{{ itemsMap[serial.ItemId].Cardinality || 0 }}</text>
                                </view>
                                <view class="item-expiry">
                                    <text :class="getItemExpiryClass(itemsMap[serial.ItemId])">
                                        {{ getItemFormattedDate(itemsMap[serial.ItemId]) }}
                                    </text>
                                </view>
                            </view>
                        </view>

                        <!-- Empty Content -->
                        <view class="empty-slot" v-else>
                            <image src="/static/imgs/emptyPositionW.png" class="empty-img" mode="aspectFit"></image>
                        </view>

                        <!-- Expired Watermark -->
                        <view class="expired-watermark"
                            v-if="serial.ItemId > 0 && itemsMap[serial.ItemId] && isExpired(itemsMap[serial.ItemId])">
                            <text class="watermark-text">已过期</text>
                        </view>
                    </view>
                </view>
            </view>
        </view>
        <!-- Optional Add Floor Slot or component -->
        <slot name="add-floor"></slot>
    </view>
</template>

<script>
import dayjs from 'dayjs';

export default {
    props: {
        floorList: {
            type: Array,
            default: () => []
        },
        serialsMap: {
            type: Object,
            default: () => ({})
        },
        itemsMap: {
            type: Object,
            default: () => ({}) // ItemDetail objects mapped by ItemId
        },
        highlightedSerialIds: {
            type: Array,
            default: () => []
        },
        showFloorAction: {
            type: Boolean,
            default: false
        },
        showSerialAction: {
            type: Boolean,
            default: false
        }
    },
    methods: {
        getAlertDotClass(serial) {
            if (!serial || !serial.ItemId) return '';
            const item = this.itemsMap[serial.ItemId];
            if (!item) return '';
            if (this.isSerialExpiryWarning(serial)) return 'bg-red';
            if (this.isStockInsufficient(item)) return 'bg-orange';
            return '';
        },
        isExpired(item) {
            if (!item) return false;
            const d = item.LatestExpirationDate || item.ExpiryDate;
            if (!d) return false;
            return this.getDaysUntil(d) < 0;
        },
        getItemNameColorClass(item) {
            if (!item) return '';
            const expiryClass = this.getItemExpiryClass(item);
            if (expiryClass.includes('text-red')) return 'text-red';
            if (this.isStockInsufficient(item)) return 'text-orange';
            if (expiryClass.includes('text-yellow')) return 'text-yellow';
            return '';
        },
        getSerialsByFloor(floorId) {
            return this.serialsMap[floorId] || [];
        },
        getOccupiedCount(floor) {
            //TODO 默认显示数量错误，点击后显示正常
            const serials = this.getSerialsByFloor(floor.Id);
            return serials.filter(s => s.ItemId && s.ItemId > 0).length;
        },
        toggleFloor(floor) {
            this.$emit('toggle-floor', floor);
        },
        handleFloorAction(floor) {
            this.$emit('floor-action', floor);
        },
        handleSerialClick(serial) {
            this.$emit('serial-click', serial);
        },
        handleSerialAction(serial) {
            this.$emit('serial-action', serial);
        },
        // helpers
        getDaysUntil(dateStr) {
            if (!dateStr) return 999;
            const now = dayjs();
            const target = dayjs(dateStr);
            return target.diff(now, 'day');
        },
        getItemFormattedDate(item) {
            if (!item) return '';
            // Assuming FormattedDate is pre-calculated by parent, or we calculate it here.
            // The parent logic pre-calculates it. But let's fallback.
            if (item.FormattedDate) return item.FormattedDate;
            const d = item.LatestExpirationDate || item.ExpiryDate;
            return d ? dayjs(d).format('YYYY-MM-DD') : '-';
        },
        getItemExpiryClass(item) {
            if (!item) return '';
            if (item.ExpiryClass) return item.ExpiryClass;
            const d = item.LatestExpirationDate || item.ExpiryDate;
            if (!d) return 'text-normal';
            const days = this.getDaysUntil(d);
            if (days < 0) return 'text-red font-bold';
            if (days < 30) return 'text-red';
            if (days < 90) return 'text-yellow';
            return 'text-normal';
        },
        isStockInsufficient(item) {
            if (!item) return false;
            // Respect pre-calculated value if provided by parent, but ensure it's not bypassed by logic errors
            if (item.IsStockInsufficient !== undefined && typeof item.IsStockInsufficient === 'boolean') {
                return item.IsStockInsufficient;
            }
            const stock = Number(item.AvailableStock || 0);
            const base = Number(item.Cardinality || 0);
            // Warning if stock is 0 or less, OR if a positive base is set and stock falls below it
            return stock <= 0 || (base > 0 && stock < base);
        },
        isSerialExpiryWarning(serial) {
            if (!serial || !serial.ItemId) return false;
            const item = this.itemsMap[serial.ItemId];
            if (!item) return false;
            if (item.IsExpiryWarning !== undefined) return item.IsExpiryWarning;
            const d = item.LatestExpirationDate || item.ExpiryDate;
            if (!d) return false;
            return this.getDaysUntil(d) < 30;
        },
        shouldShowAlert(serial) {
            // This was used in item-inbound specifically
            return this.isSerialExpiryWarning(serial) || (this.itemsMap[serial.ItemId] && this.isStockInsufficient(this.itemsMap[serial.ItemId]));
        },
        hasExpiryProblem(floor) {
            const serials = this.getSerialsByFloor(floor.Id);
            if (!serials) return false;
            return serials.some(s => this.isSerialExpiryWarning(s));
        },
        hasStockProblem(floor) {
            const serials = this.getSerialsByFloor(floor.Id);
            if (!serials) return false;
            return serials.some(s => {
                if (!s.ItemId) return false;
                const item = this.itemsMap[s.ItemId];
                return item && this.isStockInsufficient(item);
            });
        },
        getFloorStatusClass(floor) {
            const serials = this.getSerialsByFloor(floor.Id);
            if (!serials || serials.length === 0) return 'blink-green';
            if (this.hasExpiryProblem(floor)) return 'blink-red';
            if (this.hasStockProblem(floor)) return 'blink-orange';
            return 'blink-green';
        }
    }
}
</script>

<style lang="scss" scoped>
.floor-list {
    width: 100%;
}

.floor-wrapper {
    margin-bottom: 24rpx;
    animation: slideUp 0.3s ease;
}

.floor-card {
    background: #fff;
    border-radius: 24rpx;
    padding: 24rpx;
    display: flex;
    align-items: center;
    position: relative;
    box-shadow: 0 4rpx 12rpx rgba(0, 0, 0, 0.04);
    transition: all 0.2s;

    &:active {
        transform: scale(0.99);
        background: #FAFAFA;
    }
}

.floor-icon {
    width: 80rpx;
    height: 80rpx;
    background: #F5F7FA;
    border-radius: 20rpx;
    display: flex;
    align-items: center;
    justify-content: center;
    margin-right: 24rpx;
}

.icon-circle {
    width: 24rpx;
    height: 24rpx;
    border-radius: 50%;
    background: #DCDFE6;
}

/* Status Blink Animations */
.blink-green {
    background: #67C23A;
    box-shadow: 0 0 10rpx #67C23A;
}

.blink-orange {
    background: #FF9800;
    animation: blink 2s infinite;
}

.blink-red {
    background: #F56C6C;
    animation: blink-fast 1s infinite;
}

@keyframes blink {

    0%,
    100% {
        opacity: 1;
        transform: scale(1);
    }

    50% {
        opacity: 0.6;
        transform: scale(0.8);
    }
}

@keyframes blink-fast {

    0%,
    100% {
        opacity: 1;
        transform: scale(1);
    }

    50% {
        opacity: 0.4;
        transform: scale(0.8);
    }
}

.floor-info {
    flex: 1;
    display: flex;
    flex-direction: column;
}

.floor-name {
    font-size: 30rpx;
    font-weight: bold;
    color: $sull-text-main;
    margin-bottom: 6rpx;
}

.floor-sub {
    font-size: 22rpx;
    color: #999;
}

.floor-status-icons {
    display: flex;
    gap: 12rpx;
    margin-right: 12rpx;
}

.status-icon {
    width: 36rpx;
    height: 36rpx;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;

    &.bg-red-light {
        background: rgba(245, 108, 108, 0.1);
    }

    &.bg-orange-light {
        background: rgba(255, 152, 0, 0.1);
    }
}

.status-bg-icon {
    width: 24rpx;
    height: 24rpx;
    background-size: contain;
    background-repeat: no-repeat;
    background-position: center;
}

.icon-clock {
    background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='%23F56C6C'%3E%3Cpath d='M11.99 2C6.47 2 2 6.48 2 12s4.47 10 9.99 10C17.52 22 22 17.52 22 12S17.52 2 11.99 2zM12 20c-4.42 0-8-4.42-8-8s3.58-8 8-8 8 3.58 8 8-3.58 8-8 8zm.5-13H11v6l5.25 3.15.75-1.23-4.5-2.67z'/%3E%3C/svg%3E");
}

.icon-siren {
    background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='%23FF9800'%3E%3Cpath d='M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-2h2v2zm0-4h-2V7h2v6z'/%3E%3C/svg%3E");
}

.floor-action,
.serial-action {
    width: 60rpx;
    height: 60rpx;
    display: flex;
    align-items: center;
    justify-content: center;
}

.action-icon {
    font-size: 36rpx;
    color: #999;
}


/* Serial Grid */
.serial-grid-wrapper {
    margin-top: 24rpx;
    padding: 0 8rpx;
    animation: fadeIn 0.3s ease;
}

.serial-grid {
    display: grid;
    grid-template-columns: repeat(3, minmax(0, 1fr));
    gap: 16rpx;
}

.serial-item {
    width: 100%;
    box-sizing: border-box;
    overflow: hidden;
    background: #F5F7FA;
    border-radius: 16rpx;
    padding: 16rpx;
    height: 220rpx;
    display: flex;
    flex-direction: column;
    box-shadow:
        inset 0 4rpx 8rpx rgba(0, 0, 0, 0.06),
        0 2rpx 4rpx rgba(255, 255, 255, 0.8),
        0 0 0 2rpx rgba(0, 0, 0, 0.03);
    border: 2rpx solid #E4E7ED;
    transition: all 0.3s;
    position: relative;

    &.highlight-glow {
        border-color: #1890FF;
        box-shadow: 0 0 20rpx rgba(24, 144, 255, 0.5);
        animation: glow 1.5s infinite alternate;
    }

    &.expiry-warning {
        border: 2rpx solid #F56C6C !important;
        background-color: #FFF0F0;
    }
}

@keyframes glow {
    from {
        box-shadow: 0 0 10rpx rgba(24, 144, 255, 0.3);
    }

    to {
        box-shadow: 0 0 30rpx rgba(24, 144, 255, 0.7);
    }
}

.serial-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 8rpx;
}

.serial-no-wrapper {
    display: flex;
    align-items: center;
}

.serial-no {
    font-size: 28rpx;
    font-weight: 900;
    color: #303133;
}

.alert-bell {
    width: 14rpx;
    height: 14rpx;
    background: #F56C6C;
    border-radius: 50%;
    margin-right: 6rpx;

    &.bg-red {
        background: $sull-color-error;
    }

    &.bg-orange {
        background: #FF9800;
    }
}

/* Item Detail */
.item-detail {
    flex: 1;
    display: flex;
    flex-direction: column;
    justify-content: center;
    position: relative;
}

.item-name-wrapper {
    width: 100%;
    height: 40rpx;
    overflow: hidden;
    white-space: nowrap;
    margin-bottom: 8rpx;
    position: relative;
}

.item-name-text {
    display: block;
    font-size: 26rpx;
    font-weight: 600;
    color: $sull-text-main;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    width: 100%;

    &.scrolling {
        display: inline-block;
        width: auto;
        padding-left: 100%;
        animation: marquee 10s linear infinite;
        text-overflow: clip;
    }
}

@keyframes marquee {
    0% {
        transform: translateX(0);
    }

    100% {
        transform: translateX(-100%);
    }
}

.item-spec-row {
    width: 100%;
    overflow: hidden;
    white-space: nowrap;
    margin-bottom: 4rpx;
}

.item-spec {
    font-size: 20rpx;
    color: $sull-text-secondary;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    display: block;
}

.item-footer-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-top: 4rpx;
}

.item-stock-simple {
    display: flex;
    align-items: baseline;
    font-size: 20rpx;
    color: $sull-text-secondary;

    .current-num {
        font-size: 26rpx;
        font-weight: bold;
        color: $sull-text-main;
        margin-right: 2rpx;
    }

    .total-num {
        opacity: 0.7;
    }

    &.text-red-bold {

        .current-num,
        .total-num {
            color: $sull-color-error !important;
            opacity: 1;
        }
    }
}

.item-expiry {
    font-size: 20rpx;
}

/* Text Colors */
.text-red {
    color: $sull-color-error !important;
    font-weight: bold;
}

.text-orange {
    color: #FF9800 !important;
}

.text-orange-bold {

    .current-num,
    .total-num {
        color: #FF9800 !important;
        opacity: 1;
    }
}

.text-yellow {
    color: $sull-color-warning !important;
    font-weight: bold;
}

.text-black {
    color: #000;
    font-weight: bold;
}

.font-bold {
    font-weight: bold;
}

.expired-watermark {
    position: absolute;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%) rotate(-30deg);
    border: 4rpx solid rgba(0, 0, 0, 0.25);
    border-radius: 8rpx;
    padding: 2rpx 12rpx;
    pointer-events: none;
    z-index: 10;
    white-space: nowrap;
    display: flex;
    align-items: center;
    justify-content: center;

    .watermark-text {
        font-size: 30rpx;
        font-weight: 900;
        color: rgba(0, 0, 0, 0.25);
        letter-spacing: 2rpx;
        white-space: nowrap;
    }
}

.empty-slot {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    pointer-events: none;
    /* Allow clicks to pass through if needed, though card handles it */
}

.empty-img {
    width: 64rpx;
    height: 64rpx;
    opacity: 0.8;
}
</style>

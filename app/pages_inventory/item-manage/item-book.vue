<template>
    <view class="container">
        <!-- 顶部导航栏 -->
        <view class="header" :style="{ paddingTop: statusBarHeight * 2 + 'rpx' }">
            <view class="nav-bar">
                <BackButton @click="goBack" />
                <text class="page-title">{{ $t('itemBook.title') }}</text>
                <view class="placeholder"></view>
            </view>
        </view>

        <!-- 内容滚动区 -->
        <scroll-view scroll-y class="content-scroll" :scroll-top="scrollTopVal" @scroll="onScroll"
            scroll-with-animation>
            <view v-if="loading" class="loading-state">
                <view class="loading-icon"></view>
                <text>{{ $t('itemBook.searching') }}</text>
            </view>

            <view v-else-if="error" class="error-state">
                <icon type="warn" size="48" color="#E74C3C"></icon>
                <text class="error-text">{{ error }}</text>
                <button class="retry-btn" @click="fetchData">{{ $t('itemBook.retry') }}</button>
            </view>

            <view v-else class="item-content">
                <view class="item-card">
                    <!-- 药品基本信息 -->
                    <view class="item-header">
                        <text class="item-name">{{ drugData.name || $t('itemBook.unknownDrug') }}</text>
                    </view>

                    <view class="item-meta">
                        <view class="meta-item">
                            <text class="label">{{ $t('itemBook.spec') }}</text>
                            <text class="value">{{ drugData.form || '-' }}</text>
                        </view>
                        <view class="meta-item">
                            <text class="label">{{ $t('itemBook.stock') }}</text>
                            <text class="value" :class="{ 'warning-orange': drugData.availableStock <= 0 }">
                                {{ drugData.availableStock ?? 0 }} {{ drugData.unit || $t('itemBook.unit') }}
                            </text>
                        </view>
                        <view class="meta-item">
                            <text class="label">{{ $t('itemBook.expiryDate') }}</text>
                            <text class="value" :class="{ 'warning-red': isExpWarn }">
                                {{ formatDate(drugData.latestExpirationDate) }}
                            </text>
                        </view>
                    </view>

                    <!-- 正文内容 (原始排版展示) -->
                    <view class="instruction-body">
                        <block v-for="(section, index) in parsedSections" :key="index">
                            <view class="section-title">
                                <view class="title-indicator"></view>
                                <text>{{ section.title }}</text>
                            </view>

                            <view class="section-content">
                                <text class="raw-text">{{ section.content }}</text>
                            </view>
                        </block>

                        <view v-if="!parsedSections.length && drugData.remark" class="raw-content">
                            <text class="raw-text">{{ drugData.remark }}</text>
                        </view>

                        <view v-if="!parsedSections.length && !drugData.remark" class="empty-content">
                            <text>{{ $t('itemBook.emptyContent') }}</text>
                        </view>
                    </view>
                </view>
                <!-- 底部间距 -->
                <view class="bottom-spacer"></view>
                <!-- 版权信息 -->
                <FooterInfo />
            </view>
        </scroll-view>

        <!-- 回到顶部按钮 -->
        <BackToTop :scroll-top="scrollTop" @click="goTop" />
    </view>
</template>

<script>
import BackButton from '@/components/BackButton/BackButton.vue';
import FooterInfo from '@/components/FooterInfo/FooterInfo.vue';
import BackToTop from '@/components/BackToTop/BackToTop.vue';
import request from '@/utils/request.js';
import dayjs from 'dayjs';

export default {
    components: { BackButton, FooterInfo, BackToTop },
    data() {
        return {
            statusBarHeight: 20,
            scrollTop: 0,
            scrollTopVal: 0,
            code: '',
            loading: true,
            error: '',
            drugData: {},
            parsedSections: []
        };
    },
    computed: {
        isExpWarn() {
            if (!this.drugData.latestExpirationDate) return false;
            const expireDate = dayjs(this.drugData.latestExpirationDate);
            const diffDays = expireDate.diff(dayjs(), 'day');
            return diffDays < 30;
        }
    },
    onLoad(options) {
        const sysInfo = uni.getSystemInfoSync();
        this.statusBarHeight = sysInfo.statusBarHeight || 20;
        if (options.code) {
            // Use decodeURIComponent to handle potential double encoding issues on some platforms (like Android)
            this.code = decodeURIComponent(options.code);
            console.log(this.code);
            this.fetchData();
        } else {
            this.error = this.$t('itemBook.errorMissingCode');
            this.loading = false;
        }
    },
    methods: {
        goBack() {
            uni.navigateBack();
        },
        onScroll(e) {
            this.scrollTop = e.detail.scrollTop;
        },
        goTop() {
            this.scrollTopVal = this.scrollTop;
            this.$nextTick(() => {
                this.scrollTopVal = 0;
            });
        },
        formatDate(dateStr) {
            if (!dateStr) return this.$t('itemBook.noDate');
            return dayjs(dateStr).format('YYYY-MM-DD');
        },
        async fetchData() {
            this.loading = true;
            this.error = '';

            try {
                const combinedId = this.code.replace('379630044868680', '386908427251781');
                const res = await request.get('api/ssbh/Item/Remark', { combinedId: combinedId });

                if (res.status && res.data && res.data.name) {
                    this.drugData = res.data;
                    this.parseDrugInfo(res.data.remark);
                } else {
                    this.error = res.message || this.$t('itemBook.fetchFail');
                }
            } catch (e) {
                console.error('Fetch drug info failed', e);
                this.error = this.$t('itemBook.networkError');
            } finally {
                this.loading = false;
            }
        },
        parseDrugInfo(text) {
            if (!text) {
                this.parsedSections = [];
                return;
            }

            const sections = [];
            const rawSections = text.split(/【([^】]+)】/g).filter(Boolean);

            for (let i = 0; i < rawSections.length; i += 2) {
                if (i + 1 < rawSections.length) {
                    sections.push({
                        title: rawSections[i].trim(),
                        content: rawSections[i + 1].trim()
                    });
                }
            }
            this.parsedSections = sections;
        }
    }
};
</script>

<style lang="scss" scoped>
.container {
    background-color: #f5f5f5;
    /* Updated background color */
    min-height: 100vh;
    display: flex;
    flex-direction: column;
}

.header {
    background: linear-gradient(135deg, #009A97 0%, #007A78 100%);
    padding: 0 40rpx 40rpx 40rpx;
    color: #fff;
}

.nav-bar {
    display: flex;
    align-items: center;
    justify-content: space-between;
    height: 88rpx;
}

.page-title {
    font-size: 34rpx;
    font-weight: bold;
}

.placeholder {
    width: 60rpx;
}

.content-scroll {
    flex: 1;
    height: 0;
}

.loading-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding-top: 200rpx;
    color: #666;

    .loading-icon {
        width: 60rpx;
        height: 60rpx;
        border: 4rpx solid #eee;
        border-top-color: #009A97;
        border-radius: 50%;
        animation: spin 1s linear infinite;
        margin-bottom: 20rpx;
    }
}

@keyframes spin {
    to {
        transform: rotate(360deg);
    }
}

.error-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    padding: 100rpx 60rpx;

    .error-text {
        margin-top: 30rpx;
        color: #333;
        font-size: 30rpx;
        text-align: center;
        line-height: 1.5;
    }

    .retry-btn {
        margin-top: 50rpx;
        background-color: #009A97;
        color: #fff;
        border-radius: 45rpx;
        padding: 0 80rpx;
        height: 90rpx;
        line-height: 90rpx;
        font-size: 30rpx;
    }
}

.item-content {
    padding: 30rpx;
}

.item-card {
    background-color: #fff;
    border-radius: 32rpx;
    box-shadow: 0 8rpx 32rpx rgba(0, 0, 0, 0.04);
    padding: 40rpx;
}

.item-name {
    font-size: 48rpx;
    font-weight: bold;
    color: #009A97;
    line-height: 1.3;
    margin-bottom: 30rpx;
    display: block;
}

.item-meta {
    padding: 30rpx 0;
    border-top: 2rpx solid #F0F2F5;
    border-bottom: 2rpx solid #F0F2F5;
    margin-bottom: 40rpx;

    .meta-item {
        display: flex;
        margin-bottom: 16rpx;
        font-size: 32rpx;
        /* 字体加大 */

        &:last-child {
            margin-bottom: 0;
        }

        .label {
            color: #88909B;
            width: 160rpx;
        }

        .value {
            color: #333;
            font-weight: 500;
        }

        .warning-orange {
            color: #E89310;
            font-weight: bold;
        }

        .warning-red {
            color: #E74C3C;
            font-weight: bold;
        }
    }
}

.section-title {
    display: flex;
    align-items: center;
    margin: 40rpx 0 20rpx 0;

    .title-indicator {
        width: 8rpx;
        height: 36rpx;
        background-color: #009A97;
        border-radius: 4rpx;
        margin-right: 20rpx;
    }

    text {
        font-size: 36rpx;
        font-weight: bold;
        color: #009A97;
    }
}

.section-content {
    padding: 0 10rpx;
}

.raw-text {
    font-size: 34rpx;
    color: #333;
    font-family: 'Arial', 'Helvetica', sans-serif;
    /* Updated font family */
    line-height: 1.6;
    /* Updated line height */
    text-align: justify;
    white-space: pre-wrap;
    /* 保留原生换行和缩进 */
    word-break: break-all;
}

.empty-content {
    text-align: center;
    padding: 60rpx 0;
    color: #999;
    font-size: 28rpx;
}

.bottom-spacer {
    height: 60rpx;
}
</style>

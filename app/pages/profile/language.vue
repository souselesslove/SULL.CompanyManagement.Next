<template>
    <view class="container">
        <scroll-view scroll-y class="main-scroll" :scroll-top="scrollTopValue" @scroll="onScroll"
            refresher-enabled="true" :refresher-triggered="isRefreshing" @refresherrefresh="onRefresh"
            refresher-default-style="none" refresher-background="#f0f5ff">
            <view slot="refresher" class="custom-refresher">
                <view class="refresher-content">
                    <view class="refresher-icon" :class="{ 'animating': isRefreshing }"></view>
                    <text class="refresher-text">{{ isRefreshing ? $t('index.refreshing') : $t('index.pullToRefresh')
                        }}</text>
                </view>
            </view>

            <view class="scroll-content">
                <!-- Header -->
                <view class="page-header">
                    <view class="back-btn" @click="goBack">
                        <view class="icon-arrow-left"></view>
                    </view>
                    <text class="page-title">{{ $t('profile.language.title') }}</text>
                    <view class="placeholder-right"></view>
                </view>

                <!-- Language List -->
                <view class="language-list">
                    <view class="language-item" v-for="(item, index) in languages" :key="index"
                        @click="changeLanguage(item.code)" :class="{ active: currentLang === item.code }">
                        <text class="lang-name">{{ item.name }}</text>
                        <view class="check-icon" v-if="currentLang === item.code">
                            <text class="icon-check">✓</text>
                        </view>
                    </view>
                </view>

                <!-- Footer -->
                <FooterInfo />

                <view class="bottom-spacer"></view>
            </view>
        </scroll-view>

        <BackToTop :scroll-top="scrollTop" @click="scrollToTop" />
    </view>
</template>

<script>
import BackToTop from '@/components/BackToTop/BackToTop.vue';
import FooterInfo from '@/components/FooterInfo/FooterInfo.vue';

export default {
    components: {
        BackToTop,
        FooterInfo
    },
    data() {
        return {
            scrollTop: 0,
            scrollTopValue: 0,
            isRefreshing: false,
            languages: [{
                code: 'zh-Hans',
                name: '简体中文'
            },
            {
                code: 'en',
                name: 'English'
            }
            ],
            currentLang: ''
        }
    },
    onShow() {
        this.currentLang = this.$i18n.locale;
    },
    methods: {
        changeLanguage(lang) {
            if (this.currentLang === lang) return;

            this.$i18n.locale = lang;
            this.currentLang = lang;
            uni.setLocale(lang);

            uni.showToast({
                title: this.$t('common.success'),
                icon: 'success'
            });

            setTimeout(() => {
                uni.navigateBack();
            }, 500);
        },
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
        onRefresh() {
            this.isRefreshing = true;
            setTimeout(() => {
                this.isRefreshing = false;
                uni.showToast({
                    title: this.$t('index.refreshed'),
                    icon: 'none'
                });
            }, 1000);
        }
    }
}
</script>

<style lang="scss" scoped>


page {
    background-color: $sull-color-background;
    height: 100%;
    font-family: $font-family-main;
}

.container {
    display: flex;
    flex-direction: column;
    height: 100vh;
    width: 100%;
    background-color: #F8F8F8;
}

.main-scroll {
    flex: 1;
    height: 0;
}

.scroll-content {
    padding: 0 24px;
    min-height: 100%;
    display: flex;
    flex-direction: column;
}

/* Header */
.page-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 40px 0 20px 0;
    /* Adjust top padding for status bar if needed */
    height: 44px;
}

.back-btn {
    width: 40px;
    height: 40px;
    display: flex;
    align-items: center;
    justify-content: flex-start;
}

.icon-arrow-left {
    width: 24px;
    height: 24px;
    background-color: $sull-text-main;
    -webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M15.41 7.41L14 6l-6 6 6 6 1.41-1.41L10.83 12z'/%3E%3C/svg%3E");
    -webkit-mask-size: contain;
    mask-size: contain;
    -webkit-mask-repeat: no-repeat;
    mask-repeat: no-repeat;
    -webkit-mask-position: center;
    mask-position: center;
    mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M15.41 7.41L14 6l-6 6 6 6 1.41-1.41L10.83 12z'/%3E%3C/svg%3E");
}

.page-title {
    font-size: 18px;
    font-weight: 600;
    color: $sull-text-main;
}

.placeholder-right {
    width: 40px;
}

/* Language List */
.language-list {
    margin-top: 20px;
    background: #FFFFFF;
    border-radius: 16px;
    padding: 0 16px;
    flex: 1;
}

.language-item {
    display: flex;
    align-items: center;
    justify-content: space-between;
    height: 60px;
    border-bottom: 1px solid $sull-color-line;
}

.language-item:last-child {
    border-bottom: none;
}

.lang-name {
    font-size: 16px;
    color: $sull-text-main;
}

.active .lang-name {
    color: $sull-color-primary;
    font-weight: 600;
}

.check-icon {
    color: $sull-color-primary;
    font-size: 18px;
    font-weight: bold;
}

.bottom-spacer {
    height: 40px;
}
</style>

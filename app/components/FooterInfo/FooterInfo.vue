<template>
	<view class="footer">
		<text v-if="showFinished" class="footer-text">{{ $t('index.noMoreData') }}</text>
		<text class="footer-info">v{{ version }}</text>
		<text class="footer-copyright">{{ copyrightText }}</text>
	</view>
</template>

<script>
import config from '@/config.js';

import { formatMessage } from '@/utils/i18n.js';

export default {
	name: 'FooterInfo',
	props: {
		showFinished: {
			type: Boolean,
			default: false
		}
	},
	data() {
		const sysInfo = uni.getSystemInfoSync();
		return {
			version: sysInfo.appVersion || config.version,
			companyName: config.company,
			currentYear: new Date().getFullYear()
		}
	},
	computed: {
		copyrightText() {
			const values = {
				year: this.currentYear,
				company: this.companyName
			};
			// #ifdef H5
			return this.$t('index.copyright', values);
			// #endif
			// #ifndef H5
			const raw = this.$t('index.copyright');
			return formatMessage(raw, values);
			// #endif
		}
	}
}
</script>

<style lang="scss">
.footer {
	display: flex;
	flex-direction: column;
	align-items: center;
	padding-top: 60rpx;
	gap: 16rpx;
	width: 100%;
}

.footer-text {
	font-size: $font-size-body-xs;
	color: $sull-text-secondary;
	opacity: 0.6;
	display: flex;
	align-items: center;
	gap: 16rpx;
}

.footer-text::before,
.footer-text::after {
	content: "";
	display: block;
	width: 40rpx;
	height: 2rpx;
	background-color: $sull-color-line;
}

.footer-info {
	font-size: 20rpx;
	color: $sull-gray-50;
	font-family: monospace;
	background: rgba(0, 0, 0, 0.03);
	padding: 4rpx 12rpx;
	border-radius: 8rpx;
}

.footer-copyright {
	font-size: 20rpx;
	color: $sull-gray-50;
	opacity: 0.8;
	transform: scale(0.9);
}
</style>

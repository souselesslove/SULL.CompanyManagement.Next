<template>
	<view class="footer">
		<text class="footer-text">{{ $t('index.noMoreData') }}</text>
		<text class="footer-info">v{{ version }}</text>
		<text class="footer-copyright">{{ $t('index.copyright', { year: currentYear, company: companyName }) }}</text>
	</view>
</template>

<script>
	import config from '@/config.js';

	export default {
		name: 'FooterInfo',
		data() {
			return {
				version: config.version,
				companyName: config.company,
				currentYear: new Date().getFullYear()
			}
		}
	}
</script>

<style lang="scss">
	

	.footer {
		display: flex;
		flex-direction: column;
		align-items: center;
		padding-top: 30px;
		gap: 8px;
		width: 100%;
	}

	.footer-text {
		font-size: $font-size-body-xs;
		color: $sull-text-secondary;
		opacity: 0.6;
		display: flex;
		align-items: center;
		gap: 8px;
	}
	
	.footer-text::before,
	.footer-text::after {
		content: "";
		display: block;
		width: 20px;
		height: 1px;
		background-color: $sull-color-line;
	}

	.footer-info {
		font-size: 10px;
		color: $sull-gray-50;
		font-family: monospace;
		background: rgba(0,0,0,0.03);
		padding: 2px 6px;
		border-radius: 4px;
	}

	.footer-copyright {
		font-size: 10px;
		color: $sull-gray-50;
		opacity: 0.8;
		transform: scale(0.9);
	}
</style>

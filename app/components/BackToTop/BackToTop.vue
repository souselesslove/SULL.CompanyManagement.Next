<template>
	<view class="back-to-top" :class="{ 'show': show }" @click="onClick">
		<view class="icon"></view>
	</view>
</template>

<script>
export default {
	name: 'BackToTop',
	props: {
		scrollTop: {
			type: Number,
			default: 0
		},
		threshold: {
			type: Number,
			default: 200 // Show after scrolling 200px
		}
	},
	data() {
		return {
			show: false
		}
	},
	watch: {
		scrollTop(val) {
			this.show = val > this.threshold;
		}
	},
	methods: {
		onClick() {
			this.$emit('click');
		}
	}
}
</script>

<style lang="scss" scoped>
.back-to-top {
	position: fixed;
	bottom: 120rpx;
	right: 40rpx;
	width: 80rpx;
	height: 80rpx;
	background-color: rgba(255, 255, 255, 0.85);
	backdrop-filter: blur(10rpx);
	-webkit-backdrop-filter: blur(10rpx);
	border: 1rpx solid rgba(0, 0, 0, 0.05);
	border-radius: 50%;
	display: flex;
	align-items: center;
	justify-content: center;
	opacity: 0;
	visibility: hidden;
	transform: translateY(20rpx);
	transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
	z-index: 999;
	box-shadow: 0 8rpx 24rpx rgba(0, 0, 0, 0.12);
}

.back-to-top.show {
	opacity: 1;
	visibility: visible;
	transform: translateY(0);
}

.icon {
	width: 40rpx;
	height: 40rpx;
	background-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' width='24' height='24' viewBox='0 0 24 24' fill='none' stroke='%23333333' stroke-width='2.5' stroke-linecap='round' stroke-linejoin='round'%3E%3Cpath d='M18 15l-6-6-6 6'/%3E%3C/svg%3E");
	background-size: contain;
	background-repeat: no-repeat;
	background-position: center;
}
</style>

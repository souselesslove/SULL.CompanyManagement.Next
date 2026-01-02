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
			default: 300 // Show after scrolling 300px
		}
	},
	data() {
		return {
			show: false,
			timer: null
		}
	},
	watch: {
		scrollTop(val) {
			if (val > this.threshold) {
				this.show = true;
				this.resetTimer();
			} else {
				this.show = false;
				this.clearTimer();
			}
		}
	},
	methods: {
		resetTimer() {
			this.clearTimer();
			this.timer = setTimeout(() => {
				this.show = false;
			}, 3000);
		},
		clearTimer() {
			if (this.timer) clearTimeout(this.timer);
		},
		onClick() {
			this.$emit('click');
		}
	},
	beforeUnmount() {
		this.clearTimer();
	}
}
</script>

<style lang="scss" scoped>
.back-to-top {
	position: fixed;
	bottom: 300rpx;
	left: 50%;
	transform: translateX(-50%) translateY(40rpx);
	width: 88rpx;
	/* Slightly larger for glass effect */
	height: 88rpx;
	background-color: rgba(255, 255, 255, 0.6);
	/* White glass */
	backdrop-filter: blur(16rpx);
	/* Frosted effect */
	-webkit-backdrop-filter: blur(16rpx);
	border: 2rpx solid rgba(255, 255, 255, 0.8);
	border-radius: 50%;
	display: flex;
	align-items: center;
	justify-content: center;
	opacity: 0;
	visibility: hidden;
	transition: all 0.3s ease;
	z-index: 999;
	pointer-events: none;
	box-shadow: 0 16rpx 48rpx rgba(0, 0, 0, 0.1);
}

.back-to-top.show {
	opacity: 1;
	visibility: visible;
	transform: translateX(-50%) translateY(0);
	pointer-events: auto;
}

.icon {
	width: 48rpx;
	height: 48rpx;
	/* SVG: Line at top (M5 6h14), Chevron Up (M6 14l6-6 6 6) */
	background-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' width='24' height='24' viewBox='0 0 24 24' fill='none' stroke='%23333333' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Cpath d='M5 6h14'/%3E%3Cpath d='M6 15l6-6 6 6'/%3E%3C/svg%3E");
	background-size: contain;
	background-repeat: no-repeat;
	background-position: center;
}
</style>

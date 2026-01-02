<template>
	<view class="back-btn-container" @click.stop="handleClick">
		<view class="icon-arrow-left"></view>
	</view>
</template>

<script>
export default {
	name: 'BackButton',
	props: {
		// If true, automatically navigate back when clicked
		autoBack: {
			type: Boolean,
			default: false
		}
	},
	methods: {
		handleClick() {
			this.$emit('click');
			if (this.autoBack) {
				uni.navigateBack();
			}
		}
	}
}
</script>

<style lang="scss" scoped>
.back-btn-container {
	width: 80rpx;
	height: 80rpx;
	background: #F6F8FA;
	border-radius: 50%;
	display: flex;
	align-items: center;
	justify-content: center;
	/* Default positioning usually handled by parent or utility classes, 
		   but to match department exactly, we can leave position to parent 
		   or include it if it's always top-left.
		   For now, just the button style, let parent position it. 
		*/
}

.icon-arrow-left {
	width: 40rpx;
	height: 40rpx;
	background-color: #000;
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M20 11H7.83l5.59-5.59L12 4l-8 8 8 8 1.41-1.41L7.83 13H20v-2z'/%3E%3C/svg%3E");
	-webkit-mask-size: contain;
	mask-size: contain;
	-webkit-mask-repeat: no-repeat;
	mask-repeat: no-repeat;
	-webkit-mask-position: center;
	mask-position: center;
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M20 11H7.83l5.59-5.59L12 4l-8 8 8 8 1.41-1.41L7.83 13H20v-2z'/%3E%3C/svg%3E");
}
</style>

<template>
	<view class="bottom-nav">
		<view class="nav-item" :class="{ active: activePage === 'home' }" @click="navTo('home')">
			<view class="nav-icon icon-home"></view>
			<text class="nav-label">{{ $t('index.nav.home') }}</text>
		</view>
		<view class="nav-item" :class="{ active: activePage === 'bigscreen' }" @click="navTo('bigscreen')">
			<view class="nav-icon icon-screen"></view>
			<text class="nav-label">{{ $t('index.nav.bigScreen') }}</text>
		</view>
		<view class="nav-item center-item">
			<view class="center-btn">
				<view class="center-icon icon-scan"></view>
			</view>
		</view>
		<view class="nav-item" :class="{ active: activePage === 'statistics' }" @click="navTo('statistics')">
			<view class="nav-icon icon-stats"></view>
			<text class="nav-label">{{ $t('index.nav.statistics') }}</text>
		</view>
		<view class="nav-item" :class="{ active: activePage === 'profile' }" @click="navTo('profile')">
			<view class="nav-icon icon-user"></view>
			<text class="nav-label">{{ $t('index.nav.profile') }}</text>
		</view>
	</view>
</template>

<script>
export default {
	name: 'BottomNav',
	props: {
		activePage: {
			type: String,
			default: 'home'
		}
	},
	methods: {
		navTo(page) {
			if (page === this.activePage) return;

			let url = '';
			if (page === 'home') url = '/pages/index/index';
			if (page === 'bigscreen') url = '/pages/bigscreen/rescue-car';
			if (page === 'statistics') url = '/pages/statistics/statistics';
			if (page === 'profile') url = '/pages/profile/profile';

			if (url) {
				uni.reLaunch({ url });
			}
		}
	}
}
</script>

<style lang="scss" scoped>
.bottom-nav {
	position: fixed;
	bottom: 0;
	left: 0;
	width: 100%;
	height: calc(60px + env(safe-area-inset-bottom));
	background: transparent;
	display: flex;
	justify-content: space-around;
	align-items: center;
	padding-bottom: env(safe-area-inset-bottom);
	filter: drop-shadow(0 -4px 10px rgba(0, 0, 0, 0.05));
	z-index: 100;
}

.bottom-nav::before {
	content: '';
	position: absolute;
	bottom: 0;
	left: 0;
	width: 100%;
	height: 100%;
	background: #ffffff;
	border-radius: 24px 24px 0 0;
	z-index: -1;
}

.bottom-nav::after {
	content: '';
	position: absolute;
	top: -20px;
	left: 50%;
	transform: translateX(-50%);
	width: 100px;
	height: 90px;
	border-radius: 50% 50% 0 0 / 100% 100% 0 0;
	background: #ffffff;
	z-index: -1;
}

.nav-item {
	display: flex;
	flex-direction: column;
	align-items: center;
	width: 60px;
}

.nav-icon {
	width: 24px;
	height: 24px;
	margin-bottom: 4px;
	background-color: $sull-gray-50;
	-webkit-mask-size: contain;
	-webkit-mask-repeat: no-repeat;
	-webkit-mask-position: center;
	mask-size: contain;
	mask-repeat: no-repeat;
	mask-position: center;
}

.nav-label {
	font-size: $font-size-body-xs;
	color: $sull-gray-50;
}

.nav-item.active .nav-icon {
	background-color: $sull-color-primary;
}

.nav-item.active .nav-label {
	color: $sull-color-primary;
}

.center-item {
	position: relative;
	top: -20px;
	display: flex;
	flex-direction: column;
	align-items: center;
}

.center-btn {
	width: 40px;
	height: 40px;
	background: $sull-color-primary;
	border-radius: $radius-circle;
	display: flex;
	align-items: center;
	justify-content: center;
	box-shadow: 0 10px 20px rgba(149, 202, 255, 0.4);
	border: 4px solid $sull-color-white;
	margin-bottom: 4px;
}

.center-icon {
	width: 32px;
	height: 32px;
	background-color: #fff;
	-webkit-mask-size: contain;
	-webkit-mask-repeat: no-repeat;
	-webkit-mask-position: center;
	mask-size: contain;
	mask-repeat: no-repeat;
	mask-position: center;
}

/* Navigation Icons */
.icon-home {
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M10 20v-6h4v6h5v-8h3L12 3 2 12h3v8z'/%3E%3C/svg%3E");
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M10 20v-6h4v6h5v-8h3L12 3 2 12h3v8z'/%3E%3C/svg%3E");
}

.icon-screen {
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M21 2H3c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h7v2H8v2h8v-2h-2v-2h7c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2zm0 14H3V4h18v12z'/%3E%3C/svg%3E");
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M21 2H3c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h7v2H8v2h8v-2h-2v-2h7c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2zm0 14H3V4h18v12z'/%3E%3C/svg%3E");
}

.icon-scan {
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Ccircle cx='12' cy='12' r='3.2'/%3E%3Cpath d='M9 2L7.17 4H4c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2h-3.17L15 2H9zm3 15c-2.76 0-5-2.24-5-5s2.24-5 5-5 5 2.24 5 5-2.24 5-5 5z'/%3E%3C/svg%3E");
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Ccircle cx='12' cy='12' r='3.2'/%3E%3Cpath d='M9 2L7.17 4H4c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2h-3.17L15 2H9zm3 15c-2.76 0-5-2.24-5-5s2.24-5 5-5 5 2.24 5 5-2.24 5-5 5z'/%3E%3C/svg%3E");
}

.icon-stats {
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zM9 17H7v-7h2v7zm4 0h-2V7h2v10zm4 0h-2v-4h2v4z'/%3E%3C/svg%3E");
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zM9 17H7v-7h2v7zm4 0h-2V7h2v10zm4 0h-2v-4h2v4z'/%3E%3C/svg%3E");
}

.icon-user {
	-webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z'/%3E%3C/svg%3E");
	mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z'/%3E%3C/svg%3E");
}
</style>

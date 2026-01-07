<script>
import request from '@/utils/request.js';
import { storage } from '@/utils/storage.js';

export default {
	globalData: {
		hasCheckedUpdate: false
	},
	onLaunch: function () {
		// #ifdef APP-PLUS
		// Unified listening for push messages (including foreground received, background received, clicking notification to open App)
		uni.onPushMessage((res) => {
			console.log('【uniPush】收到推送消息:', res);

			// res.data 结构示例
			const data = res.data || res;

			if (data) {
				// 写入本地缓存
				storage.addNotification(data);
				// 发递全局事件通知页面更新
				uni.$emit('pushMessageReceived', data);
			}
			// 前台时弹窗提示
			// uni.showModal({
			//     title: data.title || '新通知',
			//     content: data.content || '您有新消息',
			//     showCancel: false
			// });

			// 处理自定义 payload（如跳转页面）
			// if (data.payload && data.payload.url) {
			//     uni.navigateTo({
			//         url: '/' + data.payload.url
			//     });
			// }
			// 可选：播放声音或振动
			plus.device.vibrate();
		});
		// #endif
	},
	onShow: function () {
		console.log('App Show')
	},
	onHide: function () {
		console.log('App Hide')
	},
	methods: {
		checkUpdate(options = {}) {
			const isManual = options === true || options.manual === true;
			const isForce = options === true || options.force === true;

			if (this.$options.globalData.hasCheckedUpdate && !isForce && !isManual) return;

			if (isManual) {
				uni.showLoading({
					title: options.title || '...'
				});
			}

			const sysInfo = uni.getSystemInfoSync();
			console.log(sysInfo);
			// Use appVersionCode if available, otherwise fallback to manifest version
			const currentVersionCode = parseInt(sysInfo.appVersionCode || '9999');

			return request.get('version?v=' + new Date().getTime()).then(data => {
				this.$options.globalData.hasCheckedUpdate = true;
				if (isManual) uni.hideLoading();

				if (data) {
					// Backend updated to PascalCase: Version
					const serverVersion = parseInt(data.Version || '0');
					console.log('Update Check:', currentVersionCode, '->', serverVersion);

					if (serverVersion > currentVersionCode) {
						// Force update navigation
						uni.reLaunch({
							url: '/pages/common/update?data=' + encodeURIComponent(JSON.stringify(data))
						});
						return true;
					} else if (isManual) {
						uni.showToast({
							title: options.latestMessage || 'Latest Version',
							icon: 'success'
						});
						return false;
					}
				}
				return false;
			}).catch(err => {
				if (isManual) uni.hideLoading();
				console.error('Check version failed', err);
				throw err;
			});
		}
	}
}
</script>

<style>
/*每个页面公共css */
.uni-input-placeholder,
.input-placeholder {
	color: #C0C4CC !important;
	font-size: 24rpx !important;
}
</style>
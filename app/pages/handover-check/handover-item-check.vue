<template>
	<view class="container">
		<!-- Navigation Bar -->
		<view class="custom-nav" :style="{ paddingTop: statusBarHeight + 'px' }">
			<view class="nav-content">
				<view class="nav-left">
					<BackButton class="nav-back-btn" @click="goBack" />
				</view>
				<text class="nav-title">{{ $t('handoverCheck.itemTitle') }}</text>
				<view class="nav-right"></view>
			</view>
		</view>

		<scroll-view class="main-scroll" scroll-y :enable-back-to-top="true" :refresher-enabled="true"
			:refresher-triggered="isRefreshing" @refresherrefresh="onRefresh" refresher-default-style="none"
			refresher-background="#f0f5ff" :scroll-top="scrollTopValue" @scroll="onScroll"
			:style="{ marginTop: (statusBarHeight + 44) + 'px' }">

			<view slot="refresher" class="custom-refresher">
				<view class="refresher-content">
					<view class="refresher-icon" :class="{ 'animating': isRefreshing }"></view>
					<text class="refresher-text">{{ isRefreshing ? $t('index.refreshing') : $t('index.pullToRefresh')
					}}</text>
				</view>
			</view>

			<!-- Warning Section for Expiring Items -->
			<view class="warning-section" v-if="warningItems.length > 0">
				<view class="section-header">
					<text class="warning-icon">⚠️</text>
					<text class="warning-title">{{ $t('handoverCheck.warningTitle', { count: warningItems.length })
					}}</text>
				</view>
				<view class="warning-list">
					<view class="warning-card" v-for="(item, index) in warningItems" :key="'warn-' + index"
						@click="editItem(item)">
						<view class="card-row top">
							<text class="item-name">{{ item.ItemName }}</text>
							<text class="expiry-tag" :class="getExpiryClass(item.ExpirationDate)">
								{{ getExpiryText(item.ExpirationDate) }}
							</text>
						</view>
						<view class="card-row bottom">
							<text class="info-text">{{ $t('handoverCheck.systemQty') }}: {{ item.ItemCounter }}</text>
							<text class="info-text">{{ $t('handoverCheck.colExpiry') }}: {{
								formatDate(item.ExpirationDate) }}</text>
						</view>
					</view>
				</view>
			</view>

			<!-- Main List -->
			<view class="list-content">
				<view class="list-header" v-if="normalItems.length > 0">
					<text class="list-title">{{ $t('handoverCheck.allList') }}</text>
				</view>

				<view class="item-card" v-for="(item, index) in normalItems" :key="item.Id"
					:class="{ 'abnormal-bg': isAbnormal(item) }">
					<view class="card-row top-row">
						<view class="name-section">
							<text class="item-name">{{ item.ItemName }}</text>
						</view>
						<view class="modify-btn" v-if="!isAuditMode" @click="editItem(item)">
							<text>{{ $t('handoverCheck.modify') }}</text>
						</view>
					</view>

					<view class="card-row mid-row">
						<view class="info-group">
							<text class="label">{{ $t('handoverCheck.systemQty') }}:</text>
							<text class="value">{{ item.ItemCounter }}</text>
						</view>
						<view class="info-group">
							<text class="label">{{ $t('handoverCheck.colExpiry') }}:</text>
							<text class="value" :class="getExpiryClass(item.ExpirationDate)">
								{{ formatDate(item.ExpirationDate) }}
							</text>
						</view>
					</view>

					<view class="card-row bot-row">
						<view class="info-group">
							<text class="label">{{ $t('handoverCheck.actualQty') }}:</text>
							<text class="value highlight">{{ item.ItemRealCounter }}</text>
						</view>
						<view class="info-group remark-group">
							<text class="label">{{ $t('handoverCheck.remark') }}:</text>
							<text class="value remark-text">{{ item.Remark || '-' }}</text>
						</view>
					</view>
				</view>

				<view v-if="itemList.length === 0 && !loading" class="empty-state">
					<view class="empty-icon-bg">
						<text class="empty-icon">📦</text>
					</view>
					<text class="empty-title">{{ $t('handover.empty') }}</text>
				</view>
			</view>

			<view class="bottom-spacer"></view>
			<FooterInfo />
		</scroll-view>

		<!-- Bottom Action Bar -->
		<view class="bottom-action-bar">
			<button v-if="!isAuditMode" class="confirm-btn" @click="onConfirmCheck">{{ $t('handoverCheck.confirmItems')
			}}</button>
			<button v-else class="confirm-btn" @click="onAuditNext">{{ $t('common.confirm') || '确认' }}</button>
		</view>

		<!-- Edit Modal -->
		<view class="modal-mask" v-if="showEditModal" @click="closeEditModal">
			<view class="modal-content" @click.stop>
				<view class="modal-header">
					<text class="modal-title">{{ currentItem.ItemName }} - {{ $t('handoverCheck.modify') }}</text>
					<text class="close-btn" @click="closeEditModal">×</text>
				</view>
				<view class="modal-body">
					<view class="form-item">
						<text class="label">{{ $t('handoverCheck.actualQty') }}</text>
						<view class="input-wrapper">
							<view class="qty-btn" @click="adjustQty(-1)">-</view>
							<input type="number" class="qty-input" v-model.number="editForm.ItemRealCounter" />
							<view class="qty-btn" @click="adjustQty(1)">+</view>
						</view>
					</view>
					<view class="form-item">
						<text class="label">{{ $t('handoverCheck.remark') }}</text>
						<textarea class="textarea" v-model="editForm.Remark"
							:placeholder="$t('handoverCheck.enterRemark')"></textarea>
					</view>
				</view>
				<view class="modal-footer">
					<button class="btn cancel" @click="closeEditModal">{{ $t('handover.cancel') }}</button>
					<button class="btn confirm" @click="saveItemEdit">{{ $t('handover.confirm') }}</button>
				</view>
			</view>
		</view>

		<BackToTop :scroll-top="scrollTop" @click="scrollToTop" />
	</view>
</template>

<script>
import FooterInfo from '@/components/FooterInfo/FooterInfo.vue';
import BackButton from '@/components/BackButton/BackButton.vue';
import BackToTop from '@/components/BackToTop/BackToTop.vue';
import request from '@/utils/request.js';
import dayjs from 'dayjs';

export default {
	components: {
		FooterInfo,
		BackButton,
		BackToTop
	},
	data() {
		return {
			statusBarHeight: 20,
			isRefreshing: false,
			loading: false,
			scrollTop: 0,
			scrollTopValue: 0,
			handoverData: null,
			handoverId: null,
			itemList: [],
			showEditModal: false,
			currentItem: {},
			editForm: {
				ItemRealCounter: 0,
				Remark: ''
			},
			todoListId: 0
		};
	},
	computed: {
		warningItems() {
			return this.itemList.filter(item => {
				const days = this.getDaysUntil(item.ExpirationDate);
				return days <= 30;
			});
		},
		normalItems() {
			return this.itemList;
		},
		isAuditMode() {
			return this.todoListId > 0;
		}
	},
	onLoad(options) {
		const sysInfo = uni.getSystemInfoSync();
		this.statusBarHeight = sysInfo.statusBarHeight || 20;

		if (options.id) {
			this.handoverId = options.id;
			this.fetchItemList();
		}
		if (options.todoListId) {
			this.todoListId = parseInt(options.todoListId) || 0;
			if (!this.handoverData && this.handoverId) {
				this.fetchHandoverInfo();
			}
		}

		// Listen for data from opener
		const eventChannel = this.getOpenerEventChannel();
		eventChannel.on('acceptHandoverData', (data) => {
			this.handoverData = data.data;
			if (this.handoverData && this.handoverData.id) {
				// Only fetch if we haven't already fetched for this ID
				if (this.handoverId != this.handoverData.id) {
					this.handoverId = this.handoverData.id;
					this.fetchItemList();
				}
			}
		});
	},
	methods: {
		goBack() {
			uni.navigateBack();
		},
		onRefresh() {
			this.isRefreshing = true;
			this.fetchItemList().then(() => {
				this.isRefreshing = false;
			});
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
		fetchHandoverInfo() {
			if (!this.handoverId) return;

			const searchOptions = {
				page: 1,
				rows: 1,
				wheres: JSON.stringify([
					{ name: 'Id', value: this.handoverId }
				])
			};

			request.post('api/ssbh/Handover/GetPageData', searchOptions)
				.then(res => {
					if (res && res.rows && res.rows.length > 0) {
						this.handoverData = res.rows[0];
					}
				})
				.catch(err => {
					console.error("Fetch Handover Info Failed", err);
				});
		},
		fetchItemList() {
			if (!this.handoverId) return Promise.resolve();
			this.loading = true;
			const searchOptions = {
				page: 1,
				rows: 1000,
				wheres: JSON.stringify([{
					name: 'HandoverId',
					value: this.handoverId
				}]),
				sort: 'ExpirationDate',
				order: 'asc'
			};

			return request.post('api/ssbh/HandoverItemCheck/GetPageData', searchOptions)
				.then(res => {
					if (res && res.rows) {
						this.itemList = res.rows;
					}
					this.loading = false;
				})
				.catch(err => {
					console.error("Fetch Item List Failed", err);
					uni.showToast({
						title: this.$t('handoverCheck.loadFail'),
						icon: 'none'
					});
					this.loading = false;
				});
		},
		formatDate(dateStr) {
			if (!dateStr) return '-';
			return dayjs(dateStr).format('YYYY-MM-DD');
		},
		getDaysUntil(dateStr) {
			if (!dateStr) return 9999;
			const now = new Date();
			const target = new Date(dateStr);
			return Math.ceil((target.getTime() - now.getTime()) / (1000 * 3600 * 24));
		},
		getExpiryClass(dateStr) {
			const days = this.getDaysUntil(dateStr);
			if (days < 0) return 'text-black'; // Expired
			if (days <= 30) return 'text-red'; // Warning
			if (days <= 90) return 'text-yellow'; // Near
			return 'text-normal';
		},
		getExpiryText(dateStr) {
			const days = this.getDaysUntil(dateStr);
			if (days < 0) return this.$t('handoverCheck.expired');
			return this.$t('handoverCheck.daysLeft', { days: days });
		},
		isAbnormal(item) {
			return item.ItemRealCounter !== item.ItemCounter;
		},
		editItem(item) {
			this.currentItem = item;
			this.editForm.ItemRealCounter = item.ItemRealCounter; // Default to current real
			this.editForm.Remark = item.Remark;
			this.showEditModal = true;
		},
		closeEditModal() {
			this.showEditModal = false;
		},
		adjustQty(delta) {
			let val = parseInt(this.editForm.ItemRealCounter) || 0;
			val += delta;
			if (val < 0) val = 0;
			this.editForm.ItemRealCounter = val;
		},
		saveItemEdit() {
			const payload = {
				...this.currentItem,
				ItemRealCounter: this.editForm.ItemRealCounter,
				Remark: this.editForm.Remark
			};

			request.post('api/ssbh/HandoverItemCheck/Update', {
				mainData: payload
			})
				.then(res => {
					if (res.success || res.status) {
						uni.showToast({
							title: this.$t('handoverCheck.saveSuccess'),
							icon: 'success'
						});

						// Update local list
						this.currentItem.ItemRealCounter = this.editForm.ItemRealCounter;
						this.currentItem.Remark = this.editForm.Remark;

						this.closeEditModal();
					} else {
						uni.showToast({
							title: res.msg || this.$t('handoverCheck.saveFail'),
							icon: 'none'
						});
					}
				})
				.catch(e => {
					console.error(e);
					uni.showToast({
						title: this.$t('handoverCheck.saveFail'),
						icon: 'none'
					});
				});
		},
		onConfirmCheck() {
			uni.navigateTo({
				url: '/pages/handover-check/handover-check?id=' + this.handoverId + '&status=' + (this.handoverData?.AuditStatus || 0),
				success: (res) => {
					res.eventChannel.emit('acceptHandoverData', {
						data: this.handoverData
					});
				}
			});
		},
		onAuditNext() {
			uni.navigateTo({
				url: `/pages/handover-check/handover-check?id=${this.handoverId}&todoListId=${this.todoListId}&status=${this.handoverData?.AuditStatus || 0}`,
				success: (res) => {
					if (this.handoverData) {
						res.eventChannel.emit('acceptHandoverData', {
							data: this.handoverData
						});
					}
				}
			});
		}
	}
}
</script>

<style lang="scss" scoped>
.container {
	background-color: $sull-color-background;
	min-height: 100vh;
	display: flex;
	flex-direction: column;
	font-family: $font-family-main;
}

.custom-nav {
	background-color: $sull-color-white;
	position: fixed;
	top: 0;
	left: 0;
	width: 100%;
	z-index: 999;
	box-shadow: 0 1px 4px rgba(0, 0, 0, 0.03);

	.nav-content {
		height: 44px;
		display: flex;
		align-items: center;
		justify-content: space-between;
		padding: 0 16px;

		.nav-left {
			width: 40px;
		}

		.nav-title {
			font-size: $font-size-h6;
			font-weight: $font-weight-semibold;
			color: $sull-text-main;
		}

		.nav-right {
			width: 40px;
		}
	}
}

.main-scroll {
	flex: 1;
	height: 0;
}

.warning-section {
	margin: 16px;
	background: #FFF0F0;
	border: 1px solid #FFCDCD;
	border-radius: $radius-lg;
	padding: 12px;

	.section-header {
		display: flex;
		align-items: center;
		margin-bottom: 8px;

		.warning-icon {
			font-size: 18px;
			margin-right: 6px;
		}

		.warning-title {
			font-size: 14px;
			font-weight: bold;
			color: $sull-color-error;
		}
	}

	.warning-list {
		display: flex;
		flex-direction: column;
		gap: 8px;
	}

	.warning-card {
		background: #fff;
		border-radius: 8px;
		padding: 8px 12px;
		box-shadow: 0 1px 4px rgba(0, 0, 0, 0.05);

		.card-row {
			display: flex;
			justify-content: space-between;
			align-items: center;

			&.top {
				margin-bottom: 4px;
			}
		}

		.item-name {
			font-weight: bold;
			color: $sull-text-main;
			font-size: 14px;
		}

		.expiry-tag {
			font-size: 12px;
			font-weight: bold;
		}

		.info-text {
			font-size: 12px;
			color: #666;
		}
	}
}

.list-content {
	padding: 16px;
	display: flex;
	flex-direction: column;
	gap: 12px;
}

.list-header {
	margin-bottom: 4px;

	.list-title {
		font-size: 14px;
		font-weight: bold;
		color: $sull-text-secondary;
	}
}

.item-card {
	background: $sull-color-white;
	border-radius: $radius-lg;
	padding: 16px;
	box-shadow: 0 2px 8px rgba(0, 0, 0, 0.02);
	display: flex;
	flex-direction: column;
	transition: background 0.3s;

	&.abnormal-bg {
		background-color: #FEF0F0; // Light red bg for abnormal
		border: 1px solid #FFCDCD;
	}
}

.card-row {
	display: flex;
	align-items: center;
	justify-content: space-between;
}

.name-section {
	flex: 1;

	.item-name {
		font-size: 16px;
		font-weight: bold;
		color: $sull-text-main;
	}
}

.modify-btn {
	/* Small button style */
	padding: 4px 12px;
	background: #E8F3FF;
	color: $sull-color-primary;
	border-radius: 14px;
	font-size: 12px;
	font-weight: bold;

	&:active {
		background: #D0E7FF;
	}
}

.mid-row,
.bot-row {
	justify-content: flex-start;
	gap: 24px;
}

.info-group {
	display: flex;
	align-items: center;
	gap: 6px;

	.label {
		font-size: 13px;
		color: #999;
	}

	.value {
		font-size: 14px;
		color: $sull-text-main;
		font-weight: 500;
	}

	.value.highlight {
		font-weight: bold;
		font-size: 15px;
	}

	&.remark-group {
		flex: 1;

		.remark-text {
			white-space: nowrap;
			overflow: hidden;
			text-overflow: ellipsis;
			max-width: 150px;
		}
	}
}

.text-red {
	color: $sull-color-error;
}

.text-yellow {
	color: $sull-color-warning;
}

.text-black {
	color: #000;
	font-weight: bold;
}

.text-normal {
	color: $sull-text-main;
}

.empty-state {
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	padding: 40px;

	.empty-icon-bg {
		width: 100px;
		height: 100px;
		background: #f9f9f9;
		border-radius: 50%;
		display: flex;
		align-items: center;
		justify-content: center;
		margin-bottom: 20px;
	}

	.empty-icon {
		font-size: 40px;
	}

	.empty-title {
		font-size: 16px;
		color: #999;
	}
}

.bottom-spacer {
	height: 80px;
}

.bottom-action-bar {
	position: fixed;
	bottom: 0;
	left: 0;
	width: 100%;
	background: $sull-color-white;
	padding: 16px 24px;
	padding-bottom: calc(16px + env(safe-area-inset-bottom));
	box-shadow: 0 -2px 10px rgba(0, 0, 0, 0.05);
	z-index: 99;
	box-sizing: border-box;
}

.confirm-btn {
	background: $sull-color-primary;
	color: #fff;
	border-radius: $radius-pill;
	font-size: 16px;
	font-weight: bold;
	height: 48px;
	line-height: 48px;
	border: none;

	&:active {
		opacity: 0.9;
	}
}

/* Modal Styles */
.modal-mask {
	position: fixed;
	top: 0;
	left: 0;
	right: 0;
	bottom: 0;
	background: rgba(0, 0, 0, 0.5);
	z-index: 1000;
	display: flex;
	align-items: center;
	justify-content: center;
}

.modal-content {
	width: 80%;
	background: #fff;
	border-radius: 16px;
	overflow: hidden;
}

.modal-header {
	padding: 16px;
	border-bottom: 1px solid $sull-color-line;
	display: flex;
	justify-content: space-between;
	align-items: center;

	.modal-title {
		font-size: 18px;
		font-weight: bold;
	}

	.close-btn {
		font-size: 24px;
		color: #999;
		padding: 4px;
	}
}

.modal-body {
	padding: 20px;

	.form-item {
		margin-bottom: 20px;

		.label {
			display: block;
			margin-bottom: 8px;
			color: #666;
			font-size: 14px;
		}

		.input-wrapper {
			display: flex;
			align-items: center;
			gap: 12px;

			.qty-btn {
				width: 40px;
				height: 40px;
				background: #f5f5f5;
				border-radius: 8px;
				display: flex;
				align-items: center;
				justify-content: center;
				font-size: 20px;
				color: #666;

				&:active {
					background: #e0e0e0;
				}
			}

			.qty-input {
				flex: 1;
				height: 40px;
				border: 1px solid #ddd;
				border-radius: 8px;
				text-align: center;
				font-size: 18px;
				font-weight: bold;
			}
		}

		.textarea {
			width: 100%;
			height: 80px;
			border: 1px solid #ddd;
			border-radius: 8px;
			padding: 8px;
			box-sizing: border-box;
			font-size: 14px;
		}
	}
}

.modal-footer {
	display: flex;
	border-top: 1px solid $sull-color-line;

	.btn {
		flex: 1;
		background: #fff;
		border: none;
		height: 50px;
		font-size: 16px;
		border-radius: 0;

		&.confirm {
			color: $sull-color-primary;
			border-left: 1px solid $sull-color-line;
		}

		&.cancel {
			color: #666;
		}

		&:active {
			background: #f5f5f5;
		}
	}
}

/* Refresher */
.custom-refresher {
	height: 60px;
	width: 100%;
	display: flex;
	align-items: center;
	justify-content: center;
}

.refresher-content {
	display: flex;
	flex-direction: column;
	align-items: center;
	gap: 4px;
}

.refresher-icon {
	width: 32px;
	height: 32px;
	background-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='%23409EFF'%3E%3Cpath d='M19 14h-1V8h-3V6h-2v2h-2V6H9v2H7v6H6c-1.1 0-2 .9-2 2v3c0 1.1.9 2 2 2h1c0 1.66 1.34 3 3 3s3-1.34 3-3h4c0 1.66 1.34 3 3 3s3-1.34 3-3h1c1.1 0 2-.9 2-2v-3c0-1.1-.9-2-2-2zM8 19c-.55 0-1-.45-1-1s.45-1 1-1 1 .45 1 1-.45 1-1 1zm10 0c-.55 0-1-.45-1-1s.45-1 1-1 1 .45 1 1-.45 1-1 1zM8 8h2v2H8V8zm0 4h2v2H8v-2zm4 4h-2v-2h2v2zm0-4h-2V8h2v4zm4 4h-2v-2h2v2zm0-4h-2V8h2v4z'/%3E%3Cpath d='M4 8h2v2H4zm0 4h2v2H4z' fill-opacity='.3'/%3E%3C/svg%3E");
	background-size: contain;
	background-repeat: no-repeat;
	transition: transform 0.3s ease;
}

.refresher-icon.animating {
	animation: ambulance-bounce 0.8s infinite ease-in-out;
}

.refresher-text {
	font-size: 10px;
	color: $sull-color-primary;
	opacity: 0.8;
}

@keyframes ambulance-bounce {

	0%,
	100% {
		transform: translateY(0);
	}

	50% {
		transform: translateY(-6px);
	}
}
</style>
```

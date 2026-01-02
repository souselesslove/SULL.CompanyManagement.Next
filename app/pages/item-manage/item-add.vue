<template>
	<view class="container">
		<!-- Navigation Bar -->
		<view class="custom-nav" :style="{ paddingTop: statusBarHeight * 2 + 'rpx' }">
			<view class="nav-content">
				<view class="nav-left">
					<BackButton class="nav-back-btn" @click="goBack" />
				</view>
				<text class="nav-title">{{ isEdit ? '编辑物品' : $t('itemManage.add') }}</text>
				<view class="nav-right"></view>
			</view>
		</view>

		<scroll-view class="form-content" scroll-y :style="{ marginTop: (statusBarHeight * 2 + 88) + 'rpx' }">
			<view class="qr-section" v-if="isEdit && qrCodeValue">
				<uv-qrcode :value="qrCodeValue" size="200px" ref="qrcode"></uv-qrcode>
				<text class="qr-hint">物品二维码</text>
			</view>
			<view class="form-card">
				<view class="form-item">
					<text class="label required">{{ $t('itemManage.name') }}</text>
					<input class="input" v-model="formData.Name" :placeholder="$t('itemManage.name')" />
				</view>
				<view class="form-item">
					<text class="label">{{ $t('itemManage.full_name') }}</text>
					<input class="input" v-model="formData.FullName" :placeholder="$t('itemManage.full_name')" />
				</view>
				<view class="form-item">
					<text class="label">{{ $t('itemManage.code') }}</text>
					<input class="input" v-model="formData.InterfaceCode" :placeholder="$t('itemManage.code')" />
				</view>
				<view class="form-item">
					<text class="label">类型</text>
					<picker :range="categories" range-key="text" @change="onCategoryChange" :value="categoryIndex">
						<view class="input">
							{{ categoryText || '请选择' }}
						</view>
					</picker>
				</view>
				<view class="form-item">
					<text class="label">{{ $t('itemManage.spec') }}</text>
					<input class="input" v-model="formData.Specification" :placeholder="$t('itemManage.spec')" />
				</view>
				<view class="form-item">
					<text class="label">{{ $t('itemManage.unit') }}</text>
					<input class="input" v-model="formData.Unit" :placeholder="$t('itemManage.unit')" />
				</view>
				<view class="form-item">
					<text class="label">{{ $t('itemManage.form') }}</text>
					<input class="input" v-model="formData.Form" :placeholder="$t('itemManage.form')" />
				</view>
				<view class="form-item">
					<text class="label">批准文号</text>
					<input class="input" v-model="formData.RegisterCode" placeholder="批准文号" />
				</view>
				<view class="form-item">
					<text class="label">{{ $t('itemManage.factory') }}</text>
					<input class="input" v-model="formData.Factory" :placeholder="$t('itemManage.factory')" />
				</view>
				<view class="form-item">
					<text class="label">{{ $t('itemManage.cardinality') }}</text>
					<input class="input" type="number" v-model.number="formData.Cardinality"
						:placeholder="$t('itemManage.cardinality')" />
				</view>
				<view class="form-item">
					<text class="label">{{ $t('itemManage.barcode') }}</text>
					<input class="input" v-model="formData.Barcode" :placeholder="$t('itemManage.barcode')" />
				</view>
				<view class="form-item">
					<text class="label">排序</text>
					<input class="input" type="number" v-model.number="formData.OrderBy" placeholder="排序" />
				</view>
				<view class="form-item">
					<text class="label">检索码</text>
					<input class="input" v-model="formData.SearchCode" placeholder="检索码" />
				</view>
				<view class="form-item">
					<text class="label">备注</text>
					<input class="input" v-model="formData.Remark" placeholder="备注" />
				</view>
			</view>

			<view class="submit-section">
				<button class="submit-btn" :disabled="!isValid" @click="submitForm">{{ $t('handover.confirm')
				}}</button>
			</view>
		</scroll-view>
	</view>
</template>

<script>
import BackButton from '@/components/BackButton/BackButton.vue';
import UvQrcode from '@climblee/uv-ui/components/uv-qrcode/uv-qrcode.vue';
import request from '@/utils/request.js';
import { storage } from '@/utils/storage.js';

export default {
	components: { BackButton, UvQrcode },
	data() {
		return {
			statusBarHeight: 20,
			formData: {
				Name: '',
				FullName: '',
				InterfaceCode: '',
				Category: 0,
				Specification: '',
				Unit: '',
				Form: '',
				RegisterCode: '',
				Factory: '',
				Barcode: '',
				Cardinality: 1,
				OrderBy: 0,
				SearchCode: '',
				Remark: '',
				IsDeleted: 0,
				Status: 1
			},
			categories: [
				{ text: '未分类', value: 0 },
				{ text: '药品', value: 10 },
				{ text: '耗材', value: 20 },
				{ text: '设备', value: 30 }
			],
			userInfo: {},
			isEdit: false
		};
	},
	computed: {
		qrCodeValue() {
			if (this.formData.DepartmentId && this.formData.InterfaceCode) {
				return `item.html?code=${this.formData.DepartmentId}%2b${this.formData.InterfaceCode}`;
			}
			return '';
		},
		isValid() {
			// At least Name or FullName
			return this.formData.Name || this.formData.FullName;
		},
		categoryText() {
			const cat = this.categories.find(c => c.value === this.formData.Category);
			return cat ? cat.text : '';
		},
		categoryIndex() {
			return this.categories.findIndex(c => c.value === this.formData.Category);
		}
	},
	onLoad(options) {
		const sysInfo = uni.getSystemInfoSync();
		this.statusBarHeight = sysInfo.statusBarHeight || 20;
		const uInfo = storage.get('userOrgInfo');
		if (uInfo && uInfo.User) {
			this.userInfo = uInfo.User;
		}

		if (options.item) {
			try {
				let jsonStr = options.item;
				try {
					jsonStr = decodeURIComponent(options.item);
				} catch (e) {
					console.warn('Decode item failed, trying raw', e);
				}
				const item = JSON.parse(jsonStr);
				this.formData = { ...this.formData, ...item };
				this.isEdit = true;
			} catch (e) {
				console.error('Failed to parse item', e);
			}
		}
	},
	methods: {
		goBack() {
			uni.navigateBack();
		},
		onCategoryChange(e) {
			const index = e.detail.value;
			this.formData.Category = this.categories[index].value;
		},
		submitForm() {
			if (!this.isValid) return;

			// Add Creator info
			const payload = {
				...this.formData,
				CreatorId: this.userInfo.UserId,
				Creator: this.userInfo.UserName,
				OrganizationId: this.userInfo.OrganizationId || 0,
				DepartmentId: this.userInfo.DepartmentId || 0
			};

			// Fallback Name/FullName
			if (!payload.Name && payload.FullName) payload.Name = payload.FullName;
			if (!payload.FullName && payload.Name) payload.FullName = payload.Name;

			const url = this.isEdit ? 'api/ssbh/Item/Update' : 'api/ssbh/Item/Add';

			request.post(url, { mainData: payload })
				.then(res => {
					if (res.success || res.status) {
						uni.showToast({ title: this.$t('itemManage.save_success'), icon: 'success' });
						setTimeout(() => {
							uni.navigateBack();
						}, 1500);
					} else {
						uni.showToast({ title: res.msg || '保存失败', icon: 'none' });
					}
				})
				.catch(e => {
					console.error(e);
					uni.showToast({ title: '保存失败', icon: 'none' });
				});
		}
	}
}
</script>

<style lang="scss" scoped>
.container {
	background-color: $sull-color-background;
	height: 100vh;
	display: flex;
	flex-direction: column;
}

.custom-nav {
	background-color: $sull-color-white;
	position: fixed;
	top: 0;
	left: 0;
	width: 100%;
	z-index: 999;
	box-shadow: 0 2rpx 8rpx rgba(0, 0, 0, 0.03);

	.nav-content {
		height: 88rpx;
		display: flex;
		align-items: center;
		justify-content: space-between;
		padding: 0 32rpx;

		.nav-left {
			width: 80rpx;
		}

		.nav-title {
			font-size: 32rpx;
			font-weight: 600;
		}

		.nav-right {
			width: 80rpx;
		}
	}
}

.form-content {
	flex: 1;
	padding: 32rpx;
	width: 100%;
	box-sizing: border-box;
}

.form-card {
	background: #fff;
	border-radius: 24rpx;
	padding: 0 32rpx;
	margin-bottom: 48rpx;
}

.form-item {
	display: flex;
	align-items: center;
	padding: 32rpx 0;
	border-bottom: 2rpx solid #eee;

	&:last-child {
		border-bottom: none;
	}

	.label {
		width: 160rpx;
		font-size: 28rpx;
		color: #333;
	}

	.label.required::after {
		content: '*';
		color: red;
		margin-left: 4rpx;
	}

	.input {
		flex: 1;
		font-size: 28rpx;
		color: #333;
		text-align: right;
	}

	picker {
		flex: 1;
		text-align: right;
	}
}

.submit-section {
	padding: 32rpx;
}

.submit-btn {
	background: $sull-color-primary;
	color: #fff;
	border-radius: 48rpx;
	font-size: 32rpx;
	font-weight: bold;
	height: 96rpx;
	line-height: 96rpx;
	box-shadow: 0 8rpx 24rpx rgba(64, 158, 255, 0.3);
}

.submit-btn[disabled] {
	opacity: 0.6;
	box-shadow: none;
}

.qr-section {
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	padding: 32rpx;
	background: #fff;
	border-radius: 24rpx;
	margin-bottom: 20rpx;
}

.qr-hint {
	margin-top: 16rpx;
	font-size: 24rpx;
	color: #999;
}
</style>

<template>
	<view class="page-container">
		<!-- Custom Navigation Bar -->
		<view class="custom-nav" :style="{ paddingTop: statusBarHeight + 'px' }">
			<view class="nav-content">
				<view class="nav-left">
					<BackButton class="nav-back-btn" @click="goBack" />
				</view>
				<text class="nav-title">{{ $t('handoverCheck.title') }}</text>
				<view class="nav-right"></view>
			</view>
		</view>

		<scroll-view class="scroll-content" scroll-y :enable-back-to-top="true" :refresher-enabled="true"
			:refresher-triggered="isRefreshing" @refresherrefresh="onRefresh" refresher-default-style="none"
			refresher-background="#f0f5ff" :scroll-top="scrollTopValue" @scroll="onScroll"
			:style="{ paddingTop: (statusBarHeight + 44) + 'px' }">

			<view slot="refresher" class="custom-refresher">
				<view class="refresher-content">
					<view class="refresher-icon" :class="{ 'animating': isRefreshing }"></view>
					<text class="refresher-text">{{ isRefreshing ? $t('index.refreshing') : $t('index.pullToRefresh')
					}}</text>
				</view>
			</view>

			<view class="content-wrapper">

				<!-- Read-Only List Mode (Non-Creator) -->
				<view v-if="isReadOnly" class="read-only-list">
					<view class="check-card" v-for="(item, index) in checkList" :key="index">
						<view class="check-header">
							<text class="check-title">{{ item.index + '. ' + item.title }}</text>
						</view>

						<view class="check-options no-pointer">
							<!-- Display Selected Options Only or Value Text -->
							<text class="result-text">{{ getResultText(item) }}</text>
						</view>

						<view class="check-input" v-if="item.remark">
							<text class="input-label">{{ $t('handoverCheck.remark') }}:</text>
							<text class="remark-text">{{ item.remark }}</text>
						</view>
					</view>

					<!-- Action Buttons for ReadOnly Mode -->
					<view class="nav-buttons" v-if="showConfirmBtn || showAuditBtn">
						<button v-if="showConfirmBtn" class="nav-btn finish-btn" @click="onConfirmHandover">
							{{ $t('handoverCheck.confirm') || '确认交接' }}
						</button>
						<button v-if="showAuditBtn" class="nav-btn finish-btn" @click="onAuditHandover">
							{{ $t('handoverCheck.audit') || '审核' }}
						</button>
					</view>
				</view>

				<!-- Editable Wizard Mode (Creator) -->
				<block v-else>
					<!-- 1. Progress Info -->
					<view class="progress-card">
						<view class="progress-text">
							<text class="current-step">{{ $t('handoverCheck.step', {
								current: currentStepIndex + 1,
								total: checkList.length
							}) }}</text>
							<text class="remaining-text">{{ $t('handoverCheck.remaining', { count: remainingCount })
							}}</text>
						</view>
						<progress :percent="progressPercent" border-radius="4" stroke-width="6" activeColor="#007AFF"
							backgroundColor="#E9E9ED" />
					</view>

					<!-- 2. Main Content Card -->
					<view class="check-card" v-if="currentItem">
						<view class="check-header">
							<text class="check-title">{{ currentItem.index + '. ' + currentItem.title }}</text>
							<view class="help-btn" @click="showHelp(currentItem)">
								<text class="help-icon">?</text>
							</view>
						</view>

						<view class="check-desc">
							<text>{{ currentItem.description }}</text>
						</view>

						<view class="check-options">
							<!-- Single Choice -->
							<block v-if="currentItem.type === 'single'">
								<view v-for="(opt, idx) in currentItem.options" :key="idx" class="option-item"
									:class="{ active: currentItem.value === opt.value, disabled: isReadOnly }"
									@click="!isReadOnly && selectOption(opt.value)">
									<text class="radio-icon">{{ currentItem.value === opt.value ? '⚫' : '⚪' }}</text>
									<text class="option-label">{{ opt.label }}</text>
								</view>
							</block>

							<!-- Multiple Choice -->
							<block v-else-if="currentItem.type === 'multiple'">
								<view v-for="(opt, idx) in currentItem.options" :key="idx" class="option-item"
									:class="{ active: isSelected(opt.value), disabled: isReadOnly }"
									@click="!isReadOnly && toggleOption(opt.value)">
									<text class="checkbox-icon">{{ isSelected(opt.value) ? '✅' : '⬜' }}</text>
									<text class="option-label">{{ opt.label }}</text>
								</view>
							</block>
						</view>

						<view class="check-input">
							<text class="input-label">{{ $t('handoverCheck.remark') }}</text>
							<textarea class="remark-input" :disabled="isReadOnly" v-model="currentItem.remark"
								:placeholder="$t('handoverCheck.remarkPlaceholder')" auto-height />
						</view>
					</view>

					<!-- 3. Navigation Buttons -->
					<view class="nav-buttons">
						<button v-if="!isFirstStep" class="nav-btn prev-btn" @click="prevStep">
							{{ $t('handoverCheck.prev') }}
						</button>

						<!-- Middle button for Quick Finish if not last step -->
						<button v-if="!isLastStep" class="nav-btn quick-finish-btn" @click="confirmQuickFinish">
							{{ $t('handoverCheck.quickFinish') || '一键确认' }}
						</button>

						<button v-if="!isLastStep" class="nav-btn next-btn" @click="nextStep">
							{{ $t('handoverCheck.next') }}
						</button>

						<button v-if="isLastStep" class="nav-btn finish-btn" @click="finishCheck">
							{{ $t('handoverCheck.finish') }}
						</button>
					</view>
				</block>

				<FooterInfo />
				<view class="safe-area-bottom"></view>
			</view>
		</scroll-view>

		<!-- Back to Top -->
		<BackToTop :scroll-top="scrollTop" @click="scrollToTop" />
	</view>
</template>

<script>
import FooterInfo from '@/components/FooterInfo/FooterInfo.vue';
import BackButton from '@/components/BackButton/BackButton.vue';
import BackToTop from '@/components/BackToTop/BackToTop.vue';
import request from '@/utils/request.js';

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
			scrollTop: 0,
			scrollTopValue: 0,
			currentStepIndex: 0,
			handoverData: null,
			handoverId: null,
			handoverStatus: 0,
			handoverStatus: 0,
			isReadOnly: false,
			handoverCreatorId: '',
			currentUser: '',
			userInfo: {},
			checkList: [],
			todoListId: 0,
			isSubmitting: false
		};
	},
	onLoad(option) {
		// Get User Info for Org/Dept
		const uInfo = uni.getStorageSync('userOrgInfo');
		if (uInfo && uInfo.User) {
			this.userInfo = uInfo.User;
			this.currentUser = this.userInfo.UserId;
		}

		if (option.id) {
			this.handoverId = option.id;
		}
		if (option.todoListId) {
			this.todoListId = parseInt(option.todoListId) || 0;
		}
		if (option.status) {
			this.handoverStatus = parseInt(option.status) || 0;
		}

		// Listen for data from opener
		const eventChannel = this.getOpenerEventChannel();
		eventChannel.on('acceptHandoverData', (data) => {
			this.handoverData = data.data;
			if (this.handoverData) {
				if (!this.handoverId && this.handoverData.id) {
					this.handoverId = this.handoverData.id;
				}
				// Map status from AuditStatus if present, else fallback to Status or status lowercase
				if (this.handoverData.AuditStatus !== undefined) {
					this.handoverStatus = parseInt(this.handoverData.AuditStatus) || 0;
				} else if (this.handoverData.Status !== undefined) {
					this.handoverStatus = parseInt(this.handoverData.Status) || 0;
				} else if (this.handoverData.status !== undefined) {
					this.handoverStatus = parseInt(this.handoverData.status) || 0;
				}
				// Giver/Creator check
				// Use CreateId primarily for reliable ID comparison
				this.handoverCreatorId = this.handoverData.CreateId || this.handoverData.createId || this.handoverData.CreatorId || this.handoverData.GiverId || this.handoverData.giverId;
			}
			this.checkReadOnly();
			// If we get full data here, we might need to fetch results if checks are loaded, or wait for checks to load
			if (this.checkList.length > 0) {
				this.fetchCheckResults();
			}
		});

		if (this.handoverId && !this.handoverData) {
			// If we only have ID and no data from opener, fetch it.
			// This method will handle fetching check results after getting info.
			this.fetchHandoverInfo();
		} else if (this.handoverData) {
			// If we have data (e.g. from new creation), we might need to ensure check results are fetched if it's an existing edit
			// But typically new creation doesn't have results yet.
			// If getting data from list click (but via channel), just ensure consistent status logic

		}

		this.checkReadOnly();
	},
	computed: {
		currentItem() {
			return this.checkList[this.currentStepIndex];
		},
		progressPercent() {
			if (this.checkList.length === 0) return 0;
			return ((this.currentStepIndex + 1) / this.checkList.length) * 100;
		},
		remainingCount() {
			const count = this.checkList.length - (this.currentStepIndex + 1);
			return count > 0 ? count : 0;
		},
		isFirstStep() {
			return this.currentStepIndex === 0;
		},
		isLastStep() {
			return this.checkList.length > 0 && this.currentStepIndex === this.checkList.length - 1;
		},
		showConfirmBtn() {
			// Status 1 (Pending Confirm) AND CurrentUser != Creator -> Show Confirm
			return this.isReadOnly && this.handoverStatus === 1 && this.currentUser && this.handoverCreatorId && String(this.currentUser) !== String(this.handoverCreatorId);
		},
		showAuditBtn() {
			// Status 10 (Pending Audit) -> Show Audit
			// Assuming no specific user restriction for audit provided in requirement, or controlled by backend permission implicitly? 
			// Requirement said: "If status is 10... show Audit button".
			return this.isReadOnly && this.handoverStatus === 10;
		}
	},
	mounted() {
		const sysInfo = uni.getSystemInfoSync();
		this.statusBarHeight = sysInfo.statusBarHeight || 20;
		this.fetchCheckList();
	},
	methods: {
		fetchCheckList() {
			// 1. Name=Handover
			// 2. Category=G or Category=U
			// Using client-side filtering for Category to ensure "OR" logic is correct unless API supports IN/OR strictly.

			const searchOptions = {
				page: 1,
				rows: 100,
				sort: 'Id',
				order: 'asc',
				wheres: JSON.stringify([
					{ name: 'Name', value: 'Handover' }
				])
			};

			request.post('api/ssbh/DbConfig/GetPageData', searchOptions)
				.then(res => {
					if (res && res.rows) {
						const rows = res.rows;
						// Priority: U > D > O > G
						let target = rows.find(r => r.Category === 'U');
						if (!target) target = rows.find(r => r.Category === 'D');
						if (!target) target = rows.find(r => r.Category === 'O');
						if (!target) target = rows.find(r => r.Category === 'G');

						if (target) {
							let jsonStr = target.Value;
							if (!jsonStr || jsonStr.trim() === '') {
								jsonStr = target.DefaultValue;
							}

							try {
								if (jsonStr) {
									const parsedList = JSON.parse(jsonStr);
									if (Array.isArray(parsedList)) {
										let filteredList = parsedList.filter(item => {
											const active = item.isActive !== undefined ? item.isActive : item.IsActive;
											return active === true || active === 'true' || active === 1;
										});

										this.checkList = filteredList.map((item, index) => {
											const type = item.componentType === 'RadioGroup' ? 'single' : (item.componentType === 'CheckBoxGroup' ? 'multiple' : 'singale');
											const defVal = item.props.defaultValue;

											// Ensure required fields for UI
											return {
												id: item.id || item.Id || (index + 1),
												index: index + 1,
												title: item.title || item.Title || 'Check Item',
												description: item.description || item.Description || '',
												type: type,
												options: (item.props.options || item.props.Options || [
													{ label: this.$t('handoverCheck.normal'), value: 'normal' },
													{ label: this.$t('handoverCheck.abnormal'), value: 'abnormal' }
												]).map(opt => ({
													label: opt.label || opt.Label,
													value: opt.value || opt.Value
												})),
												value: defVal || (type === 'multiple' ? [] : ''),
												remark: item.remark || item.Remark || ''
											};
										});
									}

									// Fetch Existing Results after configured list is loaded
									this.fetchCheckResults();
								}
							} catch (e) {
								console.error("Parse CheckList JSON Failed", e);
								uni.showToast({ title: '配置解析失败', icon: 'none' });
							}
						}
					}
				})
				.catch(err => {
					console.error("Fetch CheckList Failed", err);
					uni.showToast({ title: '加载检查项失败', icon: 'none' });
				});
		},
		fetchCheckResults() {
			if (!this.handoverId) return;

			const searchOptions = {
				page: 1,
				rows: 1000,
				wheres: JSON.stringify([
					{ name: 'HandoverId', value: this.handoverId }
				])
			};

			request.post('api/ssbh/HandoverCheck/GetPageData', searchOptions)
				.then(res => {
					if (res && res.rows && res.rows.length > 0) {
						// Map results to checkList
						res.rows.forEach(row => {
							const targetItem = this.checkList.find(c => c.id == row.CheckId); // Use loose eq for string/number id match
							if (targetItem) {
								if (targetItem.type === 'multiple') {
									targetItem.value = row.ValueResult ? row.ValueResult.split(',') : [];
								} else {
									targetItem.value = row.ValueResult || '';
								}
								targetItem.remark = row.Remark || '';
							}
						});
					}
				})
				.catch(err => {
					console.error("Fetch Check Results Failed", err);
				});
		},
		checkReadOnly() {
			// ReadOnly if:
			// ReadOnly if:
			// 1. Status > 0 (Pending Confirmation / Finished / Audited)
			// 2. OR Current User != Creator
			const isFinished = this.handoverStatus > 1;
			// Use strict or loose equality as IDs can be string or int. 
			const isNotCreator = this.handoverCreatorId && this.currentUser && String(this.handoverCreatorId) !== String(this.currentUser);
			this.isReadOnly = isFinished || isNotCreator;
		},
		getResultText(item) {
			if (item.type === 'single') {
				const opt = item.options.find(o => o.value === item.value);
				return opt ? opt.label : item.value;
			} else if (item.type === 'multiple') {
				const labels = item.options
					.filter(o => item.value.includes(o.value))
					.map(o => o.label);
				return labels.join(', ');
			}
			return item.value;
		},
		goBack() {
			uni.navigateBack();
		},
		onRefresh() {
			this.isRefreshing = true;
			this.fetchCheckList();
			setTimeout(() => {
				this.currentStepIndex = 0;
				this.isRefreshing = false;
			}, 1000);
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
		showHelp(item) {
			// Use description or tooltip
			const msg = item.description || item.title;
			uni.showToast({ title: msg, icon: 'none' });
		},
		selectOption(val) {
			this.currentItem.value = val;
		},
		isSelected(val) {
			return this.currentItem.value.includes(val);
		},
		toggleOption(val) {
			const idx = this.currentItem.value.indexOf(val);
			if (idx > -1) {
				this.currentItem.value.splice(idx, 1);
			} else {
				this.currentItem.value.push(val);
			}
		},
		prevStep() {
			if (this.currentStepIndex > 0) {
				this.currentStepIndex--;
			}
		},
		nextStep() {
			// Validation
			if ((this.currentItem.type === 'single' && !this.currentItem.value) ||
				(this.currentItem.type === 'multiple' && this.currentItem.value.length === 0)) {
				uni.showToast({ title: '请选择一项', icon: 'none' });
				return;
			}

			// Auto Save
			this.saveCheckResult(this.currentItem);

			if (this.currentStepIndex < this.checkList.length - 1) {
				this.currentStepIndex++;
			}
		},
		saveCheckResult(item) {
			if (!this.handoverId || this.isReadOnly) return;

			let textRes = '';
			if (item.type === 'single') {
				const opt = item.options.find(o => o.value === item.value);
				if (opt) textRes = opt.label;
			} else if (item.type === 'multiple') {
				const labels = item.options
					.filter(o => item.value.includes(o.value))
					.map(o => o.label);
				textRes = labels.join(',');
			}

			const payload = {
				OrganizationId: this.userInfo.OrganizationId,
				DepartmentId: this.userInfo.DepartmentId,
				HandoverId: this.handoverId,
				CheckId: item.id,
				CheckContent: item.title,
				ValueResult: Array.isArray(item.value) ? item.value.join(',') : item.value,
				TextResult: textRes,
				Remark: item.remark,
				Counter: item.index === this.checkList.length ? item.index : 0 //TODO 待完善
			};

			// Request without blocking UI significantly
			request.post('api/ssbh/HandoverCheck/Add', {
				mainData: payload
			}).then(res => {
				// Silent success
			}).catch(e => {
				console.error('Save check failed', e);
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
						this.handoverStatus = this.handoverData.AuditStatus || 0;
						// Use CreateId primarily for reliable ID comparison
						this.handoverCreatorId = this.handoverData.CreateId || this.handoverData.createId || this.handoverData.CreatorId || this.handoverData.GiverId || this.handoverData.giverId;
						// If Creator field is a name, we might need CreateId. Assuming CreateId is standard.
						if (this.handoverData.CreateId) {
							this.handoverCreatorId = this.handoverData.CreateId;
						}

						this.checkReadOnly();
						// Refresh checks after knowing the handover exists
						// Removed duplicate fetchCheckResults call to improve efficiency
					}
				})
				.catch(err => {
					console.error("Fetch Handover Info Failed", err);
				});
		},
		onConfirmHandover() {
			this.updateHandoverStatus(10, '交接确认成功');
		},
		onAuditHandover() {
			this.updateHandoverStatus(20, '审核成功');
		},
		updateHandoverStatus(newStatus, successMsg) {
			if (!this.handoverData) return;

			const payload = {
				...this.handoverData,
				AuditStatus: newStatus
			};
			if (this.todoListId > 0) {
				payload.todoListId = this.todoListId;
			}
			// Using Update API - Assuming basic update structure
			// If the API requires specific fields for update, this might need adjustment.
			// Commonly: mainData: { ... }
			request.post('api/ssbh/Handover/Update', { mainData: payload })
				.then(res => {
					if (res.success || res.status) { // Check for API success signal
						uni.showToast({ title: successMsg, icon: 'success' });
						this.handoverStatus = newStatus;
						this.handoverData.AuditStatus = newStatus;
						this.checkReadOnly();

						// Delay back
						setTimeout(() => {
							// Return to home page
							uni.reLaunch({
								url: '/pages/index/index'
							});
						}, 1500);
					} else {
						uni.showToast({ title: res.msg || '操作失败', icon: 'none' });
					}
				})
				.catch(e => {
					uni.showToast({ title: '请求失败', icon: 'none' });
					console.error(e);
				});
		},
		finishCheck() {
			if ((this.currentItem.type === 'single' && !this.currentItem.value) ||
				(this.currentItem.type === 'multiple' && this.currentItem.value.length === 0)) {
				uni.showToast({ title: '请选择一项', icon: 'none' });
				return;
			}

			// Save last item
			this.saveCheckResult(this.currentItem);

			uni.showToast({ title: '检查完成', icon: 'success' });

			// Pass data back to create the record
			const eventChannel = this.getOpenerEventChannel();
			eventChannel.emit('handoverFinished', {
				...this.handoverData,
				checks: this.checkList
			});

			setTimeout(() => {
				uni.navigateBack({
					delta: 2
				});
			}, 1000);
		},
		confirmQuickFinish() {
			uni.showModal({
				title: '一键确认',
				content: '确定要使用默认值(正常)提交剩余所有检查项吗？',
				success: (res) => {
					if (res.confirm) {
						this.quickFinish();
					}
				}
			});
		},
		async quickFinish() {
			if (this.isSubmitting) return;
			this.isSubmitting = true;
			uni.showLoading({
				title: '提交中...'
			});

			try {
				// Ensure current item is saved if modified
				this.saveCheckResult(this.currentItem);

				// Iterate from NEXT item to END
				const startIndex = this.currentStepIndex + 1;
				const promises = [];

				for (let i = startIndex; i < this.checkList.length; i++) {
					const item = this.checkList[i];
					// Set default value if empty
					if (!item.value || (Array.isArray(item.value) && item.value.length === 0)) {
						if (item.type === 'single') {
							// Default 'normal' or first option
							item.value = 'normal';
						} else {
							item.value = ['normal'];
						}
					}
					// Construct payload for this item to save to server immediately
					// Reuse save logic but promisified
					promises.push(new Promise((resolve) => {
						let textRes = '';
						if (item.type === 'single') {
							const opt = item.options.find(o => o.value === item.value);
							if (opt) textRes = opt.label;
						} else if (item.type === 'multiple') {
							const labels = item.options
								.filter(o => item.value.includes(o.value))
								.map(o => o.label);
							textRes = labels.join(',');
						}

						const payload = {
							OrganizationId: this.userInfo.OrganizationId,
							DepartmentId: this.userInfo.DepartmentId,
							HandoverId: this.handoverId,
							CheckId: item.id,
							CheckContent: item.title,
							ValueResult: Array.isArray(item.value) ? item.value.join(',') : item.value,
							TextResult: textRes,
							Remark: item.remark,
							Counter: this.checkList.length
						};

						request.post('api/ssbh/HandoverCheck/Add', {
							mainData: payload
						}).then(() => resolve()).catch(() => resolve());
					}));
				}

				await Promise.all(promises);

				uni.hideLoading();
				uni.showToast({
					title: '全部完成',
					icon: 'success'
				});

				const eventChannel = this.getOpenerEventChannel();
				eventChannel.emit('handoverFinished', {
					...this.handoverData,
					checks: this.checkList
				});

				setTimeout(() => {
					uni.navigateBack({
						delta: 2
					});
				}, 1000);
			} catch (e) {
				console.error(e);
				uni.hideLoading();
				uni.showToast({
					title: '提交失败，请重试',
					icon: 'none'
				});
				this.isSubmitting = false;
			}
		}
	}
}
</script>

<style lang="scss" scoped>
.page-container {
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
			display: flex;
			align-items: center;
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

.scroll-content {
	flex: 1;
	height: 100vh;
}

.content-wrapper {
	padding: 16px;
	padding-bottom: 40px;
}

// 1. Progress Card
.progress-card {
	background: $sull-color-white;
	border-radius: $radius-lg;
	padding: 16px;
	margin-bottom: 20px;
	box-shadow: 0 2px 8px rgba(0, 0, 0, 0.02);

	.progress-text {
		display: flex;
		justify-content: space-between;
		margin-bottom: 8px;

		.current-step {
			font-size: $font-size-body-lg;
			font-weight: $font-weight-bold;
			color: $sull-color-primary;
		}

		.remaining-text {
			font-size: $font-size-body-sm;
			color: $sull-text-secondary;
		}
	}
}

// 2. Check Card
.check-card {
	background: $sull-color-white;
	border-radius: $radius-lg;
	padding: 24px;
	margin-bottom: 24px;
	box-shadow: 0 4px 12px rgba(0, 0, 0, 0.04);

	.check-header {
		display: flex;
		justify-content: space-between;
		align-items: flex-start;
		margin-bottom: 12px;

		.check-title {
			font-size: $font-size-h5;
			font-weight: $font-weight-bold;
			color: $sull-text-main;
		}

		.help-btn {
			width: 20px;
			height: 20px;
			border: 1px solid $sull-text-secondary; // Circle border
			border-radius: 50%;
			display: flex;
			align-items: center;
			justify-content: center;
			margin-left: 8px;
			flex-shrink: 0;
			margin-top: 4px;
		}

		.help-icon {
			font-size: 14px;
			color: $sull-text-secondary;
			font-weight: bold;
			line-height: 1;
		}
	}

	.check-desc {
		font-size: $font-size-body-md;
		color: $sull-text-secondary;
		margin-bottom: 20px;
		line-height: 1.5;
	}

	.check-options {
		margin-bottom: 20px;

		.option-item {
			display: flex;
			align-items: center;
			padding: 12px 16px;
			margin-bottom: 10px;
			background: $sull-color-background;
			border-radius: $radius-md;
			border: 1px solid transparent;
			transition: all 0.2s;

			&.disabled {
				opacity: 0.6;
				pointer-events: none;
			}

			&.active {
				background: #EAF2FF;
				border-color: $sull-color-primary;

				.option-label {
					color: $sull-color-primary;
					font-weight: $font-weight-medium;
				}
			}

			.radio-icon,
			.checkbox-icon {
				font-size: 18px;
				margin-right: 10px;
			}

			.option-label {
				font-size: $font-size-body-md;
				color: $sull-text-main;
			}
		}
	}

	.check-input {
		display: flex;
		flex-direction: column;

		.input-label {
			font-size: $font-size-body-sm;
			color: $sull-text-secondary;
			margin-bottom: 8px;
		}

		.remark-input {
			width: 100%;
			box-sizing: border-box;
			background: $sull-color-background;
			border-radius: $radius-md;
			padding: 12px;
			font-size: $font-size-body-md;
			color: $sull-text-main;
			min-height: 80px;
		}
	}
}

// 3. Nav Buttons
.nav-buttons {
	display: flex;
	gap: 16px;

	.nav-btn {
		flex: 1;
		height: 48px;
		border-radius: $radius-pill;
		border: none;
		font-size: $font-size-body-lg;
		font-weight: $font-weight-bold;
		display: flex;
		align-items: center;
		justify-content: center;
		transition: opacity 0.2s;

		&:active {
			opacity: 0.9;
		}

		&.prev-btn {
			background: $sull-color-white;
			color: $sull-text-secondary;
			border: 1px solid $sull-color-line;

			&.disabled {
				opacity: 0.5;
				background: $sull-color-background;
			}
		}

		&.next-btn {
			background: $sull-color-primary;
			color: $sull-color-white;
			box-shadow: 0 4px 10px rgba(0, 122, 255, 0.3);
		}

		&.finish-btn {
			background: $sull-color-success;
			color: $sull-color-white;
			box-shadow: 0 4px 10px rgba(76, 217, 100, 0.3);
		}

		&.quick-finish-btn {
			background: #ff4d4f;
			color: #fff;
			border: none;
			box-shadow: 0 4px 10px rgba(255, 77, 79, 0.3);
		}
	}
}

.safe-area-bottom {
	height: env(safe-area-inset-bottom);
}

/* Custom Refresher */
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

.read-only-list {
	.check-card {
		margin-bottom: 16px;
		padding: 16px;

		.check-header {
			margin-bottom: 8px;
		}

		.check-title {
			font-size: 16px;
		}

		.result-text {
			font-size: 15px;
			color: $sull-color-primary;
			font-weight: 500;
		}

		.remark-text {
			font-size: 14px;
			color: $sull-text-secondary;
			margin-left: 8px;
		}

		.check-input {
			margin-top: 8px;
			flex-direction: row;
			align-items: baseline;
		}
	}
}
</style>

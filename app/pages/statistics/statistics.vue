<template>
	<view class="container">
		<scroll-view scroll-y class="main-scroll">
			<!-- Header -->
			<view class="header">
				<text class="header-title">{{ $t('index.nav.statistics') }}</text>
			</view>

			<view class="content">
				<!-- Overview Cards -->
				<view class="stats-grid">
					<view class="stats-card primary">
						<view class="card-icon">📊</view>
						<text class="card-label">Total Items</text>
						<text class="card-value">1,234</text>
						<text class="card-trend up">+12% vs last month</text>
					</view>
					<view class="stats-card warning">
						<view class="card-icon">⚠️</view>
						<text class="card-label">Abnormal</text>
						<text class="card-value">23</text>
						<text class="card-trend down">-5% vs last month</text>
					</view>
				</view>

				<!-- Chart Section -->
				<view class="chart-section">
					<view class="section-header">
						<text class="section-title">Monthly Usage</text>
						<view class="filter-btn">Last 6 Months</view>
					</view>
					<view class="chart-placeholder">
						<!-- Placeholder for a real chart -->
						<view class="bar" style="height: 40%"></view>
						<view class="bar" style="height: 60%"></view>
						<view class="bar" style="height: 30%"></view>
						<view class="bar" style="height: 80%"></view>
						<view class="bar" style="height: 50%"></view>
						<view class="bar" style="height: 70%"></view>
					</view>
					<view class="chart-labels">
						<text>Jan</text><text>Feb</text><text>Mar</text><text>Apr</text><text>May</text><text>Jun</text>
					</view>
				</view>

				<!-- Recent Activity -->
				<view class="activity-section">
					<text class="section-title">Recent Activity</text>
					<view class="activity-list">
						<view class="activity-item">
							<view class="activity-icon check">✓</view>
							<view class="activity-info">
								<text class="activity-name">Daily Check Completed</text>
								<text class="activity-time">Today, 09:00 AM</text>
							</view>
						</view>
						<view class="activity-item">
							<view class="activity-icon warn">!</view>
							<view class="activity-info">
								<text class="activity-name">Stock Warning: Epinephrine</text>
								<text class="activity-time">Yesterday, 04:30 PM</text>
							</view>
						</view>
						<view class="activity-item">
							<view class="activity-icon update">↻</view>
							<view class="activity-info">
								<text class="activity-name">System Updated</text>
								<text class="activity-time">Dec 01, 10:00 AM</text>
							</view>
						</view>
					</view>
				</view>
			</view>
		</scroll-view>

		<!-- Bottom Navigation -->
		<BottomNav activePage="statistics" />
	</view>

</template>

<script>
	export default {
		methods: {
			navTo(page) {
				let url = '';
				if (page === 'home') url = '/pages/index/index';
				if (page === 'bigscreen') url = '/pages/bigscreen/rescue-car';
				if (page === 'profile') url = '/pages/profile/profile';
				
				if (url) {
					uni.reLaunch({ url });
				}
			}
		}
	}
</script>

<style lang="scss" scoped>
	@import '@/sull.scss';

	.container {
		display: flex;
		flex-direction: column;
		height: 100vh;
		width: 100%;
		background-color: #ffffff;
	}

	.main-scroll {
		flex: 1;
		height: 0;
	}

	.header {
		padding: 44px 24px 20px;
		display: flex;
		justify-content: center;
		align-items: center;
	}

	.header-title {
		font-size: 18px;
		font-weight: bold;
		color: $sull-text-main;
	}

	.content {
		padding: 0 24px 120px;
	}

	.stats-grid {
		display: grid;
		grid-template-columns: 1fr 1fr;
		gap: 16px;
		margin-bottom: 32px;
	}

	.stats-card {
		padding: 16px;
		border-radius: 16px;
		display: flex;
		flex-direction: column;
		gap: 8px;
	}

	.stats-card.primary {
		background: #e6f7ff;
		border: 1px solid #91d5ff;
	}

	.stats-card.warning {
		background: #fff1f0;
		border: 1px solid #ffa39e;
	}

	.card-icon {
		font-size: 24px;
		margin-bottom: 4px;
	}

	.card-label {
		font-size: 14px;
		color: $sull-text-secondary;
	}

	.card-value {
		font-size: 24px;
		font-weight: bold;
		color: $sull-text-main;
	}

	.card-trend {
		font-size: 12px;
		font-weight: 500;
	}

	.card-trend.up { color: #52c41a; }
	.card-trend.down { color: #ff4d4f; }

	.chart-section {
		margin-bottom: 32px;
	}

	.section-header {
		display: flex;
		justify-content: space-between;
		align-items: center;
		margin-bottom: 16px;
	}

	.section-title {
		font-size: 18px;
		font-weight: bold;
		color: $sull-text-main;
	}

	.filter-btn {
		font-size: 12px;
		color: $sull-color-primary;
		background: #e6f7ff;
		padding: 4px 12px;
		border-radius: 12px;
	}

	.chart-placeholder {
		height: 200px;
		display: flex;
		align-items: flex-end;
		justify-content: space-between;
		padding: 0 10px;
		border-bottom: 1px solid #f0f0f0;
	}

	.bar {
		width: 30px;
		background: $sull-color-primary;
		border-radius: 4px 4px 0 0;
		opacity: 0.8;
	}

	.chart-labels {
		display: flex;
		justify-content: space-between;
		padding: 8px 10px 0;
		font-size: 12px;
		color: $sull-text-secondary;
	}

	.activity-list {
		display: flex;
		flex-direction: column;
		gap: 16px;
	}

	.activity-item {
		display: flex;
		align-items: center;
		gap: 16px;
		padding: 12px;
		background: #fafafa;
		border-radius: 12px;
	}

	.activity-icon {
		width: 40px;
		height: 40px;
		border-radius: 50%;
		display: flex;
		align-items: center;
		justify-content: center;
		font-size: 20px;
		font-weight: bold;
	}

	.activity-icon.check { background: #f6ffed; color: #52c41a; }
	.activity-icon.warn { background: #fffbe6; color: #faad14; }
	.activity-icon.update { background: #e6f7ff; color: #1890ff; }

	.activity-info {
		display: flex;
		flex-direction: column;
		gap: 4px;
	}

	.activity-name {
		font-size: 14px;
		font-weight: 500;
		color: $sull-text-main;
	}

	.activity-time {
		font-size: 12px;
		color: $sull-text-secondary;
	}

	/* Bottom Navigation styles moved to component */
</style>

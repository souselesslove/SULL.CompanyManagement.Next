<template>
	<view class="chart-container" :style="{ height: height + 'px' }">
		<svg :width="width" :height="height" viewBox="0 0 100 50" preserveAspectRatio="none">
			<!-- Gradient Definition -->
			<defs>
				<linearGradient :id="'gradient-' + chartId" x1="0" x2="0" y1="0" y2="1">
					<stop offset="0%" stop-color="#1890ff" stop-opacity="0.3" />
					<stop offset="100%" stop-color="#1890ff" stop-opacity="0.05" />
				</linearGradient>
			</defs>

			<!-- Area Path -->
			<path :d="areaPath" :fill="'url(#gradient-' + chartId + ')'" />

			<!-- Line Path -->
			<path :d="linePath" fill="none" :stroke="color" stroke-width="2" stroke-linecap="round"
				stroke-linejoin="round" />

			<!-- Data Points -->
			<circle v-for="(point, index) in points" :key="index" :cx="point.x" :cy="point.y" r="2" :fill="color" />
		</svg>
	</view>
</template>

<script>
export default {
	name: 'SimpleLineChart',
	props: {
		data: {
			type: Array,
			default: () => []
		},
		color: {
			type: String,
			default: '#1890ff'
		},
		height: {
			type: Number,
			default: 60
		},
		width: {
			type: [Number, String],
			default: '100%'
		}
	},
	data() {
		return {
			viewWidth: 100,
			viewHeight: 50,
			chartId: 'chart-' + Math.random().toString(36).substr(2, 9)
		}
	},
	computed: {
		points() {
			if (!this.data || this.data.length === 0) return [];

			const max = Math.max(...this.data, 1);
			const min = Math.min(...this.data, 0);
			const range = max - min || 1;

			const stepX = this.viewWidth / (this.data.length - 1);

			return this.data.map((val, index) => {
				const x = index * stepX;
				const normalizedVal = (val - min) / range;
				const y = this.viewHeight - (normalizedVal * (this.viewHeight * 0.8) + (this.viewHeight * 0.1));
				return { x, y };
			});
		},
		linePath() {
			if (this.points.length === 0) return '';
			if (this.points.length === 1) return `M${this.points[0].x},${this.points[0].y}`;

			// Use smooth Bezier curves for rounded lines
			let path = `M${this.points[0].x},${this.points[0].y}`;

			for (let i = 0; i < this.points.length - 1; i++) {
				const current = this.points[i];
				const next = this.points[i + 1];
				const controlX = (current.x + next.x) / 2;

				// Quadratic Bezier curve for smooth transition
				path += ` Q${controlX},${current.y} ${controlX},${(current.y + next.y) / 2}`;
				path += ` Q${controlX},${next.y} ${next.x},${next.y}`;
			}

			return path;
		},
		areaPath() {
			if (this.points.length === 0) return '';

			const first = this.points[0];
			const last = this.points[this.points.length - 1];

			// Start from bottom-left, follow curved line, close at bottom-right
			return `M${first.x},${this.viewHeight} L${first.x},${first.y} ${this.linePath.substring(1)} L${last.x},${this.viewHeight} Z`;
		}
	}
}
</script>

<style scoped>
.chart-container {
	width: 100%;
	overflow: hidden;
}
</style>

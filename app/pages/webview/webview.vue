<template>
    <view class="container">
        <web-view :src="url"></web-view>
    </view>
</template>

<script>
import request from '@/utils/request.js';
import { storage } from '@/utils/storage.js';

export default {
    data() {
        return {
            url: ''
        }
    },
    onLoad(options) {
        let rawUrl = '';
        if (options.url) {
            try {
                rawUrl = decodeURIComponent(options.url);
            } catch (e) {
                console.warn('Decode URL failed', e);
                rawUrl = options.url || '';
            }
        } else {
            // Check storage for complex URLs
            rawUrl = storage.get('TEMP_WEBVIEW_URL') || '';
            storage.remove('TEMP_WEBVIEW_URL');
        }

        if (rawUrl) {
            if (rawUrl.indexOf('http') !== 0) {
                rawUrl = request.config.RESOURCE_URL + rawUrl;
            }
            this.url = rawUrl;
            console.log('Final WebView URL:', this.url);
        }
    }
}
</script>

<style scoped>
.container {
    width: 100%;
    height: 100vh;
}
</style>

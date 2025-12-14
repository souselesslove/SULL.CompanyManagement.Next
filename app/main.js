import App from './App'
import { createI18n } from 'vue-i18n'
import en from './locale/en.json'
import zhHans from './locale/zh-Hans.json'

// #ifndef VUE3
import Vue from 'vue'
import './uni.promisify.adaptor'
Vue.config.productionTip = false
App.mpType = 'app'
const app = new Vue({
    ...App
})
const i18n = createI18n({
    locale: 'zh-Hans',
    messages: {
        en,
        'zh-Hans': zhHans
    }
})

app.use(i18n);
app.$mount()
// #endif

// #ifdef VUE3
import { createSSRApp } from 'vue'

const i18n = createI18n({
    locale: 'zh-Hans',
    messages: {
        en,
        'zh-Hans': zhHans
    }
})

export function createApp() {
    const app = createSSRApp(App)
    app.use(i18n)
    return {
        app
    }
}
// #endif

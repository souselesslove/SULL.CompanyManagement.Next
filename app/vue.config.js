const path = require('path')
const webpack = require('webpack')

module.exports = {
  css: {
    preprocessorOptions: {
      scss: {
        additionalData: `
          @import "@/sull.scss";
        `
      }
    }
  },
  configureWebpack: {
    resolve: {
      alias: {
        '@': path.resolve(__dirname, './src')
      }
    }
  },
  chainWebpack: (config) => {
    config.plugin('define').tap((args) => {
      args[0]['__VUE_I18N_FULL_INSTALL__'] = JSON.stringify(true)
      args[0]['__VUE_I18N_LEGACY_API__'] = JSON.stringify(false)
      args[0]['__INTLIFY_PROD_DEVTOOLS__'] = JSON.stringify(false)
      return args
    })
  }
}
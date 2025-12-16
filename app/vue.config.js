const path = require('path')

module.exports = {
  css: {
    preprocessorOptions: {
      scss: {
        // 注意：这里使用 additionalData 而不是 prependData
        // 在 sass-loader v10+ 中需要用 additionalData
        additionalData: `
          @import "@/sull.scss";
        `
      }
    }
  },
  // 配置路径别名
  configureWebpack: {
    resolve: {
      alias: {
        '@': path.resolve(__dirname, './src')
      }
    }
  }
}
const path = require('path');

module.exports = {
  publicPath: '/',

  chainWebpack: config =>
  {
    config
      .plugin('html')
      .tap(args => {
        args[0].title = 'CodeSniffer';
        return args;
    });

    config.performance
      .hints(false);    

      config.module
      .rule('i18n-resource')
        .test(/\.(json5?|ya?ml)$/)
          .include.add(path.resolve(__dirname, './src/locales'))
          .end()
        .type('javascript/auto')
        .use('i18n-resource')
          .loader('@intlify/vue-i18n-loader')
    config.module
      .rule('i18n')
        .resourceQuery(/blockType=i18n/)
        .type('javascript/auto')
        .use('i18n')
          .loader('@intlify/vue-i18n-loader')
  },

  devServer: {
    allowedHosts: 'all',
    proxy: {
      '^/api': {
        target: 'http://localhost:7042',
        changeOrigin: true
      } 
    }
  }  
};
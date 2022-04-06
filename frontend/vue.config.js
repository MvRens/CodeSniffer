module.exports = {
  publicPath: '/',

  chainWebpack: config =>
  {
    config
      .plugin('html')
      .tap(args => {
        args[0].title = 'CodeSniffer Frontend';
        return args;
    });

    config.performance
      .hints(false);    
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
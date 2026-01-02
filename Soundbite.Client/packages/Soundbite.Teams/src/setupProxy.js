// This file is picked up by the development server and is not compiled into the final application
const { createProxyMiddleware } = require('http-proxy-middleware');

module.exports = function (app) {
    app.use(
        '/api',
        createProxyMiddleware({
            target: 'http://localhost:52313',
            changeOrigin: true,
            pathRewrite: {
                '^/api':'/'
            }
        })
    );
};
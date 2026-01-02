const webpackMerge = require('webpack-merge');
const commonConfig = require('./webpack.common.js');

module.exports = ({ env, watch }) => {
  const envConfig = require(`./webpack.${env}.js`);
  // merge default configuration with a chosen mode configuration
  let webPackConfig = webpackMerge(commonConfig, envConfig);
  if (watch && watch.toLowerCase() === "true") {
    console.log("Watch flag is enabled");
    webPackConfig.watch = true;
  }
  return webPackConfig;
};

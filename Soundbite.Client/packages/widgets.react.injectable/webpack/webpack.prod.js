const { CleanWebpackPlugin } = require("clean-webpack-plugin");
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const CssMinimizerPlugin = require("css-minimizer-webpack-plugin");
const TerserPlugin = require("terser-webpack-plugin");

console.log("Using webpack.prod.js");

module.exports = {
  mode: 'production',
  // source-map - A full SourceMap is emitted as a separate file.
  // It adds a reference comment to the bundle so development tools know where to find it.
  devtool: 'source-map',
  optimization: {
    minimizer: [
      new CssMinimizerPlugin(),       // Minimizes CSS
      new TerserPlugin()              // Minimize JS
    ]
  },
  plugins: [
    new MiniCssExtractPlugin({filename:"soundbite-widgets-react-browser.css"}), // Helps extract CSS into a single file
    new CleanWebpackPlugin()                                                    // Cleans up the build folder (output.path)
  ],
  module: {
    rules: [
      {
        test: /\.css/,
        use: [
          // NOTE: these run in reverse order so plan accordingly
          MiniCssExtractPlugin.loader,  // 2. Extracts css into files
          "css-loader"                  // 1. Turns css into commonjs
        ]
      }
    ]
  }
}
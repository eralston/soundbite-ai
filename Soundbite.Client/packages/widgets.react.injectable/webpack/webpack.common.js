const path = require('path')

module.exports = {
  entry: "./src/index.ts",
  output: {
    path: path.resolve(__dirname, "../dist"),
    filename: "soundbite-widgets-react-injectable.js",
    libraryTarget: "umd",
    library: "soundbite"
  },
  module: {
    rules: [
      {
        test: /\.tsx?$/,
        use: [
          // NOTE: these run in reverse order so plan accordingly
          "babel-loader",
          "ts-loader"
        ],
        exclude: /node_modules/,
      }
    ]
  },
  resolve: {
    extensions: [".tsx", ".ts", ".js", ".jsx"],
    alias: {
      Components: path.resolve(__dirname, '../src/components/'),
      Models: path.resolve(__dirname, '../src/models/'),
      Services: path.resolve(__dirname, '../src/services/'),
      Widgets: path.resolve(__dirname, '../src/widgets/')
    }
  },
  externals: {
    /*
    "@soundbite/api": {
      amd: "@soundbite/api",
      commonjs: "@soundbite/api",
      commonjs2: "@soundbite/api",
      root: "soundbiteapi"
    },
    "@soundbite/widgets-react": {
      amd: "@soundbite/widgets-react",
      commonjs: "@soundbite/widgets-react",
      commonjs2: "@soundbite/widgets-react",
      root: "soundbitewidgetsreact"
    },
    */    
    "mobx-react-lite": {
      amd: "mobx-react-lite",
      commonjs: "mobx-react-lite",
      commonjs2: "mobx-react-lite",
      root: "mobxReactLite"
    },
    react: {
      amd: "react",
      commonjs: "react",
      commonjs2: "react",
      root: "React" 
    },
    "react-dom": {
      amd: "react-dom",
      commonjs: "react-dom",
      commonjs2: "react-dom",
      root: "ReactDOM"
    }
  }
}
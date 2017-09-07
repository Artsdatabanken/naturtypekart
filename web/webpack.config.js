var Webpack = require('webpack');
var path = require('path');
var nodeModulesPath = path.resolve(__dirname, 'node_modules');

var config = {
  xdevtool: 'eval',
  entry: [
    // For hot style updates
    //'webpack/hot/dev-server',

    // The script refreshing the browser on none hot updates
    //'webpack-dev-server/client?http://localhost:8080',

    path.resolve(__dirname, 'app', 'main.js') ],  
	resolve: {
        modulesDirectories: ['lib', 'node_modules'],
        alias: {
			'durandal': 'durandal/js',
			'plugins': 'durandal/js/plugins',
			"knockout": "knockout/dist/knockout.js",
			"jquery": "jquery/jquery.js"
       }           
    },
	output: {
      path: path.resolve(__dirname, 'dist'),
      filename: 'bundle.js',

      // Everything related to Webpack should go through a build path,
      // localhost:3000/build. That makes proxying easier to handle
      publicPath: '/dist/'
    },
    module: {
	  loaders: [
	    // Let us also add the style-loader and css-loader, which you can
	    // expand with less-loader etc.
	    {
	      test: /\.css$/,
	      loader: 'style!css',
	  resolveLoader: { root: path.join(__dirname, "node_modules") },
    	}
      ]
    }
};

module.exports = config;
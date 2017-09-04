// Karma configuration
// Generated on Thu Jan 01 2015 15:29:47 GMT+0100 (W. Europe Standard Time)

module.exports = function(config) {
  config.set({

    // base path that will be used to resolve all patterns (eg. files, exclude)
    basePath: '',


    // frameworks to use
    // available frameworks: https://npmjs.org/browse/keyword/karma-adapter
    frameworks: ['jasmine', 'requirejs'],


    // list of files / patterns to load in the browser
    files: [
        'test-main.js',
        { pattern: 'test/*.js', included: false },
        { pattern: 'wwwroot/app/**/*.js', included: false },
        { pattern: 'wwwroot/lib/knockout/dist/knockout.js', included: false },
        { pattern: 'wwwroot/lib/durandal/js/*.js', included: false },
        { pattern: 'wwwroot/lib/jquery/jquery.js', included: false },
        { pattern: 'wwwroot/lib/lodash/lodash.js', included: false },
        { pattern: 'wwwroot/lib/bootstrap-treeview/src/js/*.js', included: false }
    ],


    // list of files to exclude
    exclude: [
    ],

    // preprocess matching files before serving them to the browser
    // available preprocessors: https://npmjs.org/browse/keyword/karma-preprocessor
    preprocessors: {
    },

    reporters: ['progress', 'junit'],
	junitReporter: {
		outputDir: '../../',
		outputFile: 'uitest-result.xml',
		useBrowserName: false
	},

    // web server port
    port: 9876,


    // enable / disable colors in the output (reporters and logs)
    colors: true,


    // level of logging
    // possible values: config.LOG_DISABLE || config.LOG_ERROR || config.LOG_WARN || config.LOG_INFO || config.LOG_DEBUG
    logLevel: config.LOG_DEBUG,


    // enable / disable watching file and executing tests whenever any file changes
    autoWatch: true,


    // start these browsers
    // available browser launchers: https://npmjs.org/browse/keyword/karma-launcher
    //browsers: ['Chrome', 'Firefox', 'IE'],
    browsers: ['Chrome'],


    // Continuous Integration mode
    // if true, Karma captures browsers, runs the tests and exits
    singleRun: true
  });
};

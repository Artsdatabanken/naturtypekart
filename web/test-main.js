var allTestFiles = [];
var TEST_REGEXP = /(spec|test)\.js$/i;

var pathToModule = function(path) {
  return path.replace(/^\/base\//, '').replace(/\.js$/, '');
};

Object.keys(window.__karma__.files).forEach(function(file) {
  if (TEST_REGEXP.test(file)) {
    // Normalize paths to RequireJS module names.
    allTestFiles.push(pathToModule(file));
  }
});

require.config({
  // Karma serves files under /base, which is the basePath from your config file
    baseUrl: '/base',
    paths: {
        'services/config': 'wwwroot/app/services/config',
        'application': 'wwwroot/app/services/application',
        'adbFuncs': 'wwwroot/app/services/adbFuncs',
        'knockout': 'wwwroot/lib/knockout/dist/knockout',
        'lodash': "http://cdnjs.cloudflare.com/ajax/libs/lodash.js/2.4.1/lodash.compat.min"
    },

  // dynamically load all test files
  deps: allTestFiles,

  // we have to kickoff jasmine, as it is asynchronous
  callback: window.__karma__.start
});

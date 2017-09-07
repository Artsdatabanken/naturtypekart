var gulp = require("gulp"),
    durandal = require('gulp-durandal'),
    less = require('gulp-less'),
    uglify = require('gulp-uglify'),
    concat = require('gulp-concat'),
    uncss = require('gulp-uncss'),
    minifycss = require('gulp-minify-css'),
    rename = require('gulp-rename'),
    del = require('del'),
    karma = require("karma").server,
    _ = require("lodash"),
    project = require("./project.json");


var bundleNames = { scripts: "scripts", styles: "styles" };

var paths = {
    webroot: "./" + project.webroot + "/"
};

var config = {
    base: ".",
    components: "./bower_components/",
    //debug: "src",
    release: "./dist",
    css: "css",
    lib: "./wwwroot/lib",


    // The fonts we want Gulp to process
    fonts: ["./content/fonts/*.*"],

    // The scripts we want Gulp to process - adapted from BundleConfig
    scripts: [
        // Vendor Scripts 
        "./wwwroot/lib/jquery/jquery.js",
        "./wwwroot/lib/jquery-ui/jquery-ui.js",
        "./wwwroot/lib/openlayers3-dist/ol.js",
        "./wwwroot/lib/knockout/dist/knockout.js",
        "./wwwroot/lib/bootstrap/dist/js/bootstrap.js",
        "./wwwroot/lib/bootstrap-multiselect/dist/js/bootstrap-multiselect.js",
        "./wwwroot/lib/jquery-validation/dist/jquery.validate.min.js",
        "./wwwroot/lib/jquery-validation/src/localization/messages_no.js",
        "./wwwroot/lib/requirejs/require.js"
    ],
    durandalScripts: [
      "activator.js",
      "app.js",
      "binder.js",
      "composition.js",
      "events.js",
      "system.js",
      "viewEngine.js",
      "viewLocator.js",
      "plugins/dialog.js",
      "plugins/history.js",
      "plugins/http.js",
      "plugins/router.js",
      "plugins/widget.js",
      "transitions/entrance.js"
    ],

    // The styles we want Gulp to process - adapted from BundleConfig
    styles: [
        "./wwwroot/lib/bootstrap-treeview/src/css/bootstrap-treeview.css",
        "./wwwroot/lib/bootstrap-multiselect/dist/css/bootstrap-multiselect.css",
        "./wwwroot/lib/openlayers3-dist/ol.css"
    ]};

gulp.csrc = function (prefix, glob, conf) {
  var globs =  _.map([].concat(glob), function (g) { return prefix + g; });
  return gulp.src(globs, conf);
}

// Delete the build folder
gulp.task("clean", function (cb) {
    del([config.release], cb);
});


gulp.task('styles', function () {
    return gulp.src('./style/bootstrap-build.less')
        .pipe(less())
        .pipe(gulp.dest('./wwwroot/content'));
});
gulp.task('truncatecss', function () {
    return gulp.src('./wwwroot/content/bootstrap-build.css')
        .pipe(uncss({
            html: ['./wwwroot/index.html', './wwwroot/app/views/*.html'],
            ignore: [
                ".fade",
                ".fade.in",
                ".collapse",
                ".collapse.in",
                ".collapsing",
                /\.open/
                //".menubar .form-group"
            ]
        }))
        .pipe(minifycss())
        .pipe(rename('bootstrap-build-truncated.css'))
        .pipe(gulp.dest('./wwwroot/content'));
});


gulp.task("fonts", function (cb) {
    return gulp.src(config.components + '/bootstrap/fonts/*.*')
        .pipe(gulp.dest('./wwwroot/content/fonts'));
});



gulp.task('durandal', function(){
  durandal({
    baseDir: 'wwwroot/app', //same as default, so not really required.
    main: 'main.js', //same as default, so not really required.
    output: 'durandal-main.js',
    //rjsConfigAdapter: function (rjsConfig) { // if using r.js for build ( http://stackoverflow.com/questions/28928076/durandal-optimization-with-gulp-and-gulp-durandal-not-working )
    //    rjsConfig.deps = ['text'];
    //    return rjsConfig;
    //},
    almond: true,
    minify: true
  })
  .pipe(gulp.dest('.'));
});



gulp.task("durandal-lib", [], function () {
    return gulp
        .csrc(config.components + "durandal/js/", config.durandalScripts, { base: config.components + "durandal/js"} )
        .pipe(gulp.dest(config.lib + '/durandal'));});

gulp.task("scripts-lib", [], function () {
    return gulp
        .csrc(config.components, config.scripts)
        .pipe(gulp.dest(config.lib));
});




////http://stackoverflow.com/questions/28999621/using-requirejs-optimizer-node-module-with-gulp
var gulp = require('gulp'),
    shell = require('gulp-shell');
// Run the r.js command, so simple taks :)
gulp.task('req', shell.task([
    '.\node_modules\.bin\r.js.cmd -o r.conf.js'
]));






gulp.task('karma', function (done) {
    //var config = __dirname + '/karma.conf.js';
    //var server = new Server(config, [done]);
    //server.start();
    karma.start({
        configFile: __dirname + '/karma.conf.js',
        
        singleRun: true
    }, done);
})
.task('karma:watch', function (done) {
    karma.start({
        configFile: __dirname + '/karma.conf.js'
    }, done);
});

gulp.task('watch', function () {
    gulp.watch('./style/*', ['styles']);
});

gulp.task('scripts_amund', function () {
    // Minify and copy all vendor JavaScript 
    return gulp.src(config.scripts)
        .pipe(uglify())
        .pipe(concat('all.min.js'))
      .pipe(gulp.dest('./build/js'));
});


gulp.task("default", ["scripts-lib", "durandal-lib", "styles", "fonts"]);

// Maps the files so Durandal knows where to find these.
require.config({
    wrapShim: true,
    baseUrl: 'app',
    paths: {
        'text': '../bower_components/requirejs-text/text',
        'durandal': '../bower_components/durandal/js',
        'plugins': '../bower_components/durandal/js/plugins',
        'transitions': '../bower_components/durandal/js/transitions',
        "lodash": '../bower_components/lodash/lodash',
        "bootstrap-treeview": "../bower_components/bootstrap-treeview/src/js/bootstrap-treeview",
        "d3": "../node_modules/d3/d3"
    }
});

// Durandal 2.x assumes no global libraries. It will ship expecting 
// Knockout and jQuery to be defined with requirejs. .NET 
// templates by default will set them up as standard script
// libs and then register them with require as follows: 
define('jquery', function() {
     return jQuery;
}); // of unknown reasons jQuery has to be wraped in a function!?
define('knockout', ko);

require(['durandal/app', 'durandal/viewLocator', 'durandal/system'],
    function (app, viewLocator, system) {

        // Enable debug message to show in the console 
        system.debug(true);

        app.title = 'xNaturtyper i Norge';

        app.configurePlugins({
            router: true,
            widget: true,
            dialog: true
        });

        app.start().then(function () {
            // When finding a viewmodel module, replace the viewmodel string 
            // with view to find it partner view.
            // [viewmodel]s/sessions --> [view]s/sessions.html
            // Defaults to viewmodels/views/views. 
            // Otherwise you can pass paths for modules, views, partials
            viewLocator.useConvention();

            app.on('main:resized').then(function (elem) {
                if (console.log && elem) {
                    console.log("Main div resized! - New size: height: " + elem.clientHeight + " width: " + elem.clientWidth);
                }
            });

            //Show the app by setting the root view model for our application.
            app.setRoot('viewmodels/shell', 'entrance');
        });
    });

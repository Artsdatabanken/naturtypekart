define(['services/logger', 'viewmodels/shell', 'durandal/app', 'services/resource'],
    function (logger, shell, app, resource) {
    var title = 'Selected';
    function activate() {
        return true;
    }
    var vm = {
        activate: activate,
        title: title,
        toggleview: shell.toggleleftmenu,
        res: resource.res,
    };
    app.on("showAboutPage:trigger").then(function () {
        // todo
    });

    return vm;
});

define(['services/logger', "knockout", "durandal/app", "services/cssclass", 'viewmodels/shell', 'viewmodels/nav', 'services/resource'],
    function (logger, ko, app, selector, shell, nav, resource) {
        var title = 'MainPanel',
            vm,
            mainviewSize = ko.observable(null),
            activate = function () {
                nav.activate();
                return true;
            },
            toolbar = ko.computed(function () {
                var activeView = nav.activeView();
                switch(activeView) {
                    case "map":
                        return "viewmodels/mapToolbar";
                    case "list":
                        return "viewmodels/listToolbar";
                    case "factsheet":
                        return "viewmodels/factsheetToolbar";
                    case "statistics":
                        return "viewmodels/statisticsToolbar";
                    case "import":
                        return "viewmodels/importToolbar";
                    default:
                        return undefined;
                }
            }),
            attached = function () {
                var mainview = selector.getElement("mainview");
                app.on('main:resized').then(function (elem) {
                    mainviewSize("height: " + mainview.clientHeight + " width: " + mainview.clientWidth);
                });
                app.on('mainToolbar:show').then(function (toolbar) {
                    vm.toolbar(toolbar);
                });
                app.on('mainToolbar:remove').then(function (toolbar) {
                    if (vm.toolbar() === toolbar) {
                        vm.toolbar(undefined);
                    }
                });
            };

        vm = {
            res: resource.res,
            activate: activate,
            attached: attached,
            title: title,
            mainviewSize: mainviewSize,
            toggleleftmenu: shell.toggleleftmenu,
            toolbar: toolbar,
            activeView: nav.activeView,
            navTabs: nav.navTabs,
            activeNavTab: nav.activeNavTab,
            gotoTab: nav.gotoTab

        };
        return vm;
    });

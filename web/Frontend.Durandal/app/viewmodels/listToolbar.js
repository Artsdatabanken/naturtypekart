define(['services/logger', "knockout", 'services/dataServices', 'services/application', 'viewmodels/shell', 'durandal/app'],
    function (logger, ko, dataServices, application, shell, app) {
        "use strict";

        var vm = {
            title: "List toolbar",
            activate: function() {
                application.initBookmarks();
            },
            totalListItems: ko.observable(0),
            maxExportItems: ko.observable(application.config.maxExportItems),

            showSpinner: ko.observable(false),
            exportGml: function() {
                vm.showSpinner(true);
                dataServices.exportNatureAreasAsGmlBySearchFilter(application.filter)
                    .then(function(exportFile) {
                        dataServices.downloadFile(exportFile, 'text/xml', 'data.gml', false);
                        vm.showSpinner(false);
                    });
            },
            exportXml: function() {
                vm.showSpinner(true);
                dataServices.exportNatureAreasBySearchFilter(application.filter)
                    .then(function(exportFile) {
                        dataServices.downloadFile(exportFile, 'text/xml', 'data.xml', false);
                        vm.showSpinner(false);
                    });
            },
            exportShape: function() {
                vm.showSpinner(true);
                dataServices.exportNatureAreasAsShapeBySearchFilter(application.filter);
            },
            exportExcel: function() {
                vm.showSpinner(true);
                dataServices.exportNatureAreasAsXlsxBySearchFilter(application.filter);
            },
            toggleFullscreen: shell.toggleFullscreen,

            bookmarks: application.bookmarks,

            openBookmarks: ko.observable(false),
            toggleBookmarksContainer: function() {
                vm.openBookmarks(!vm.openBookmarks());
            },
            applyFilter: function(bookmark) {
                application.applyFilter(bookmark);

                // Transform url to point to list
                var listUrl = bookmark.url;
                listUrl = listUrl.replace("#map", "#list");
                window.location.assign(listUrl);
            }
        };

        vm.tiledBookmarks = ko.computed(function () {
            var c = 0, r = [], b = application.bookmarks();
            while (c < b.length) {
                r.push(b.slice(c, c += 5));
            }
            return r;
        });

        app.on('downloadFile:done').then(function(filename) {
            vm.showSpinner(false);
        });
        vm.exportEnabled = ko.computed(function() {
            return vm.totalListItems() < application.config.maxExportItems;
        }, this);


        return vm;
    });
  

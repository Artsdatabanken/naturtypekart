define(['services/logger', "knockout", "services/application", 'viewmodels/shell', 'services/resource'],
    function (logger, ko, application, shell, resource) {
        "use strict";

        var vm = {
            title: "Statistics toolbar",
            resource: resource,
            res: resource.res,
            activate: function() {
                application.initBookmarks();
            },
            toggleFullscreen: shell.toggleFullscreen,
            bookmarks: application.bookmarks,
            showAbout: application.showAbout,
            openBookmarks: ko.observable(false),
            toggleBookmarksContainer: function() {
                vm.openBookmarks(!vm.openBookmarks());
            },
            applyFilter: function(bookmark) {
                application.applyFilter(bookmark);

                // Transform url to point to statistics
                var statUrl = bookmark.url;
                statUrl = statUrl.replace("#map", "#statistics");
                window.location.assign(statUrl);
            }
        };
        vm.tiledBookmarks = ko.computed(function () {
            var c = 0, r = [], b = application.bookmarks();
            while (c < b.length) {
                r.push(b.slice(c, c += 5));
            }
            return r;
        });

        return vm;
    });
  

define(['services/logger', "knockout", "services/adbFuncs", "durandal/app", 'plugins/router', 'services/application'],
    function (logger, ko, adbFuncs, app, router, application) {
        "use strict";
        var title = 'nav',
            viewportState = application.viewportState,
            defaultRouteParameters = '/:center/:zoom(/background/:background)(/id/:id)(/filter/:filter)',

            createRoute = function (obj) {
                return {
                    route: obj.id + defaultRouteParameters,
                    activeView: obj.id,
                    title: obj.title,
                    moduleId: obj.moduleId,
                    nav: true
                };
            },
            createTabmodel = function (obj) {
                var undifinedIsBool = function (value, bool) {
                        var b = typeof bool === 'undefined' ? true : bool; //default value of bool is true
                        return typeof value === 'undefined' ? b : value; //default value of value is true (else bool)
                    },

                    createObservable = function (value, bool) {
                        var v = undifinedIsBool(value, bool);
                        return (typeof v !== 'function') ?
                            ko.observable(!!v) :
                            ko.computed(function () {
                                var result;
                                try {
                                    result = v();
                                } catch (err) {
                                    result = false;
                                }
                                return !!result;
                            });
                    };

                return {
                    id: obj.id,
                    name: obj.name || obj.title,
                    title: obj.title,
                    link: obj.link || "#",
                    //image: "images/Spritelist-" + (obj.name || "list") + ".png",
                    classname: (obj.id || "list") + "Icon",
                    visible: createObservable(obj.visible, true),
                    enabled: createObservable(obj.enabled, true)
                };
            },
            activeView = ko.observable(),

            createUri = function (viewName) {
                var uri;
                viewName = viewName || activeView() || "map";

                // Todo: fjern tomme innslag fra lista?
                var urlFilter = application.urlFilter();
                uri = viewName + "/" + viewportState.center() + "/" + viewportState.zoom() +
                    "/background/" + viewportState.background() + (viewportState.id() ? "/id/" + viewportState.id() : "")
                    + "/filter/" + encodeURIComponent(ko.toJSON(urlFilter));
                //+"/filter/" + encodeURIComponent(ko.toJSON(application.filter));

                return uri;
            },

            navigateTo = function (fragment, options) {
                if (fragment.indexOf('undefined') === 0) {
                    logger.error("route undefined", null, title, true);
                    throw new Error("route undefined");
                }
                router.navigate(fragment, options);
            },
            restart = function () {
                navigateTo("map/" + application.config.defaultMapCenter + "/" + application.config.defaultMapZoom + "/background/" + application.config.initialBaseMapLayer);
            },

            activate = function () {
                router.map(adbFuncs.map(application.routeInfo, createRoute)).buildNavigationModel();
                router.activate();
                router.on('router:navigation:complete', function (instance, instruction) {
                    activeView(instruction.config.activeView);
                });
                if (window.location.hash === "") {
                    restart();
                }

                return true;
            },

            updateUri = function () {
                var uri = createUri();
                navigateTo(uri, false);  // only update url (does not trigger view activation)
            },
            navTabs = adbFuncs.map(application.routeInfo, createTabmodel),
            activeNavTab = ko.computed(function () {
                var activeview = activeView() || 'map';
                var result = adbFuncs.filter(navTabs, function(nt) {
                    return nt.id === activeview;
                });
                return result[0];
            }),
            gotoTab = function (tabinfo) {
                if (tabinfo.enabled() /*&& tabinfo.visible()*/) {
                    var uri = createUri(tabinfo.id);
                    navigateTo(uri); // activate view
                }
            },
            vm = {
                activate: activate,
                restart: restart,
                activeView: activeView,
                activeRouteItem: function() {
                    return router.activeItem();
                },
                title: title,
                navTabs: navTabs,
                activeNavTab: activeNavTab,
                gotoTab: gotoTab,
                navigateTo: navigateTo
            };

        application.viewportStateChanged.subscribe(function () {
            updateUri();
        });
        application.filterChanged.subscribe(function () {
            updateUri();
        });

        return vm;
    });

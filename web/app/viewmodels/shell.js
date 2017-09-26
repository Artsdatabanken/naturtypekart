define(
    ["services/cssclass", "lodash", "durandal/app"],
    function (dom, _, app) {
        "use strict";
        var resizeDebounceTimeout = 250,
            resizePollingTimeout = 1750,
            toggleleftmenu = null,
            viewleftmenu = null,
            toggleFullscreen = null,
            viewnormal = null,
            init = function() {
                var prevSize = { width: -1, height: -1 },
                    main = dom.getElement("main"),
                    leftpanel = dom.getElement("leftpanel"),
                    doRedrawMain = function() {
                        var h = main.clientHeight,
                            w = main.clientWidth;
                        if (!(w === 0 && h === 0) && !(w === prevSize.width && h === prevSize.height)) {
                            prevSize = { width: w, height: h };
                            resizeMain();
                        }
                    },
                    debouncedRedrawMain = _.debounce(doRedrawMain, resizeDebounceTimeout),
                    mainResized = function(immediate) {
                        if (immediate === true) {
                            doRedrawMain();
                        } else {
                            debouncedRedrawMain();
                        }
                    },
                    $content = dom.getElement("content"),
                    showleftmenu = function() {
                        dom.addClass($content, 'showleftMenu');
                        dom.replaceClass(dom.getElement("toggleLeftMenuButton"), 'glyphicon-menu-right', 'glyphicon-menu-left');

                        resizeMain();

                    },
                    shownormal = function() {
                        dom.removeClass($content, 'showleftMenu');
                        dom.replaceClass(dom.getElement("toggleLeftMenuButton"), 'glyphicon-menu-left', 'glyphicon-menu-right');
                        resizeMain();
                    },
                    resizeMain = function() {
                        var mainwidth = main.clientWidth;
                        var leftpanelWidth = leftpanel.clientWidth;

                        if (dom.checkForClass($content, 'showleftMenu')) {
                            $("#mainview").css("width", mainwidth-leftpanelWidth);
                        } else {
                            $("#mainview").css("width", mainwidth);
                        }
                        $("#toggleLeftMenuButton").css("top", main.clientHeight/2);

                        app.trigger('main:resized', main);
                    };

                viewleftmenu = function() {
                    if (!dom.checkForClass($content, 'showleftMenu')) {
                        showleftmenu();
                    }
                };
                viewnormal = function () {
                    if (dom.checkForClass($content, 'showleftMenu')) {
                        shownormal();
                    }
                    };
                toggleleftmenu = function() {
                    if (dom.checkForClass($content, 'showleftMenu')) {
                        shownormal();
                    } else {
                        showleftmenu();
                    }
                };
                toggleFullscreen = function() {
                    dom.toggleClass("viewport", "l-mainfullscreen");
                    mainResized(true);
                    app.trigger('app:toggleFullscreen', '');

                };
                dom.addEvent(window, "resize", mainResized);
                //dom.addEvent(dom.getElement("main"), "click", toggleFullscreen);
                window.setInterval(mainResized, resizePollingTimeout);
                mainResized();
                showleftmenu();
            },
            module = {
                attached: init,
                showleftmenu: function() {
                    if (viewleftmenu) {
                        viewleftmenu();
                    }
                },
                toggleleftmenu: function() {
                    if (toggleleftmenu) {
                        toggleleftmenu();
                    }
                },
                showNormal: function () {
                    if (viewnormal) {
                        viewnormal();
                    }
                },
                toggleFullscreen: function () {
                    if (toggleFullscreen) {
                        toggleFullscreen();
                    }
                }

    };

        return module;
    }
);

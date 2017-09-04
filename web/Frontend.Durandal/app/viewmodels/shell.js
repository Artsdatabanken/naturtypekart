define(
    ["services/cssclass", "lodash", "durandal/app"],
    function (dom, _, app) {
        "use strict";
        var resizeDebounceTimeout = 250,
            resizePollingTimeout = 1750,
            toggleleftmenu = null,
            toggleFullscreen = null,
            init = function() {
                var prevSize = { width: -1, height: -1 },
                    main = dom.getElement("main"),
                    doRedrawMain = function() {
                        var h = main.clientHeight,
                            w = main.clientWidth;
                        if (!(w === 0 && h === 0) && !(w === prevSize.width && h === prevSize.height)) {
                            prevSize = { width: w, height: h };
                            app.trigger('main:resized', main);
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
                    },
                    shownormal = function() {
                        dom.removeClass($content, 'showleftMenu');
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
            },
            module = {
                attached: init,

                toggleleftmenu: function() {
                    if (toggleleftmenu) {
                        toggleleftmenu();
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

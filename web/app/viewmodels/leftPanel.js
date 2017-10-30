define(['services/logger', 'viewmodels/shell', 'durandal/app', 'services/resource'],
    function (logger, shell, app, resource) {
    var title = 'Selected';
    function activate() {
        return true;
    }
    var vm = {
        activate: activate,
        title: title,        
        res: resource.res,
        toggleview: shell.toggleleftmenu,
        showleftmenu: shell.showleftmenu,
        showNormal: shell.showNormal,
        compositionComplete: function () {
            var sidebar = document.getElementsByClassName("l-sidebarcontent")[0];
            var togglebar = document.getElementsByClassName("toggle-full-bar")[0];
            var menubar = document.getElementsByClassName("menubar")[0];
            //var mainview = document.getElementById("mainview");
            
            // Test via a getter in the options object to see if the passive property is accessed
            var supportsPassive = false;
            try {
                var opts = Object.defineProperty({}, 'passive', {
                    get: function () {
                        supportsPassive = true;
                    }
                });
                window.addEventListener("test", null, opts);
            } catch (e) { }

            sidebar.addEventListener('touchstart', handleTouchStart, supportsPassive ? { passive: true } : false);
            sidebar.addEventListener('touchmove', handleTouchMove, supportsPassive ? { passive: true } : false);
            togglebar.addEventListener('touchstart', handleTouchStart, supportsPassive ? { passive: true } : false);
            togglebar.addEventListener('touchmove', handleTouchMove, supportsPassive ? { passive: true } : false);
            menubar.addEventListener('touchstart', handleTouchStart, supportsPassive ? { passive: true } : false);
            menubar.addEventListener('touchmove', handleTouchMove, supportsPassive ? { passive: true } : false);
            //mainview.addEventListener('touchstart', handleTouchStart, supportsPassive ? { passive: true } : false);
            //mainview.addEventListener('touchmove', handleTouchMove, supportsPassive ? { passive: true } : false);
        }
    };

    app.on("currentFeatureChanged:trigger").then(function () {
        vm.showleftmenu();
    });
    app.on("showAboutPage:trigger").then(function () {
        // todo
    });

    var xDown = null;
    var yDown = null;

    function handleTouchStart(evt) {
        xDown = evt.touches[0].clientX;
        yDown = evt.touches[0].clientY;
    }

    function handleTouchMove(evt) {
        if (!xDown || !yDown) {
            return;
        }

        var xUp = evt.touches[0].clientX;
        var yUp = evt.touches[0].clientY;

        var xDiff = xDown - xUp;
        var yDiff = yDown - yUp;

        if (Math.abs(xDiff) > Math.abs(yDiff)) {/*most significant*/
            if (xDiff > 0) {
                /* left swipe */
                vm.showNormal();
            } else {
                vm.showleftmenu();
                /* right swipe */
            }
        } else {
            if (yDiff > 0) {
                /* up swipe */
            } else {
                /* down swipe */
            }
        }
        /* reset values */
        xDown = null;
        yDown = null;
    }

    return vm;
});

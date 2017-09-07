define(['services/logger', "knockout", 'viewmodels/shell'],
    function (logger, ko, shell) {
        "use strict";

        var vm = {
            title: "Import toolbar",
            toggleFullscreen: shell.toggleFullscreen
        };

        return vm;
    });
  

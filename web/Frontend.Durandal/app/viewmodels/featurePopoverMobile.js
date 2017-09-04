define(['services/logger', "knockout", 'services/application', 'viewmodels/nav'],
    function (logger, ko, application, nav) {
        "use strict";
        var title = "FeaturePopoverMobile",
        vm = {
            compositionComplete: function () {
                if (application.currFeature.data) {
                    $("#featureInfoMobile").append($("#natureAreaInfo"));
                } else {
                    nav.restart();
                }
            },
            activate: function () {
                $("#featureInfoMobile").append($("#natureAreaInfo"));
            }
        };

        return vm;


    });
  

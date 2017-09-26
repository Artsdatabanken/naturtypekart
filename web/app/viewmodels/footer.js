define(['knockout', 'services/application', 'services/layerConfig', 'services/resource'],
    function (ko, application, layerConfig, resource) {
    "use strict";

    return {
        title: "Footer",
        res: resource.res,
        footerWarning: application.footerWarning,
        footerWarningText: application.footerWarningText,
        toggleShowWarning: function() {
            this.footerWarning(!this.footerWarning());
        },

        showTokenError: ko.observable(true),
        toggleShowToken: function() {
            this.showTokenError(!this.showTokenError());
        },

        noToken: ko.computed(function () {
            return !application.ndToken();
        }),

        noTokenForNDLayer: ko.computed(function () {
            var layer = application.currentLayer() && layerConfig.getBaseLayerFromPool(application.currentLayer());
            return layer && layer.needsToken && !application.ndToken();
        })
    };
});


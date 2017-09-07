define(['services/logger', "knockout", 'services/dataServices', 'durandal/app', 'services/application', 'viewmodels/shell', 'viewmodels/nav'],
    function (logger, ko, dataServices, app, application, shell, nav) {
        "use strict";

        var vm = {
            title: "Fact sheet toolbar",
            toggleFullscreen: shell.toggleFullscreen,
            historyList: ko.observableArray([]),
            localId: ko.observable(),
            getHistory: function() {
                vm.historyList.removeAll();
                dataServices.getExpiredMetadatasByNatureAreaLocalId(vm.localId())
                    .then(function(data) {
                        if (data.length <= 0) {
                            vm.historyList.push({ name: "", description: "Ingen tidligere versjoner" });
                            app.trigger('factsheetHistory:change', undefined);

                        } else {
                            data.forEach(function(version) {
                                vm.historyList.push({
                                    name: version.Name,
                                    description: (version.Expired
                                        ? "Utgått: " + application.formatDate(version.Expired)
                                        : "Gjeldende"),
                                    data: version
                                });
                            });
                            app.trigger('factsheetHistory:change', vm.historyList()[0].data);
                        }

                        $('#historyMulti')
                            .multiselect({
                                templates: {
                                    button:
                                        '<button type="button" class="glyph-button multiselect dropdown-toggle glyphicon glyphicon-folder-open hidden-xs" data-toggle="dropdown"></button>',
                                    ul: '<ul class="multiselect-container dropdown-menu"></ul>',
                                    li: '<li><a href="javascript:void(0);"><label></label></a></li>'
                                },
                                onChange: function(option, checked) {
                                    app.trigger('factsheetHistory:change', vm.historyList()[option.val()].data);
                                },
                                nonSelectedText: "Vis tidligere versjoner"
                            });

                    });
            },
            compositionComplete: function() {
                if (application.currFeature.data) {
                    vm.localId(application.currFeature.data.UniqueId.LocalId);
                    vm.getHistory();
                } else {
                    nav.restart();
                }
            },
            popup: function(data) {
                var mywindow = window.open('', 'faktaark', 'height=400,width=600');
                mywindow.document.write('<html><head><title>Faktaark</title>');
                mywindow.document
                    .write('<link rel="stylesheet" href="./content/bootstrap-build.css" type="text/css" />');
                //mywindow.document.write('<link rel="stylesheet" href="./lib/openlayers3-dist/ol.css" type="text/css" />');
                mywindow.document.write('</head><body >');
                mywindow.document.write(data);
                mywindow.document.write('</body></html>');

                mywindow.document.close(); // necessary for IE >= 10
                mywindow.focus(); // necessary for IE >= 10
                setTimeout(function() {
                        mywindow.print();
                        mywindow.close();
                    },
                    500); // Chrome seems to need a little time to think before activating print

                return true;
            },
            printElem: function(elem) {
                vm.popup($(elem).html());
            },
            printPreview: function() {
                vm.printElem($("#mainview"));
            }
        };

        app.on("currentFeatureChanged:trigger").then(function () {
            if (nav.activeView() === 'factsheet') {
                vm.localId(application.currFeature.data.UniqueId.LocalId);
                vm.getHistory();
            }
        });


        return vm;
    });
  

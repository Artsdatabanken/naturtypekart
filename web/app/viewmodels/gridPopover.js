define(['services/logger', 'knockout', 'durandal/app', 'services/application', 'services/config', 'services/dataServices'],
    function (logger, ko, app, application, config, dataServices) {
        "use strict";

        var findFileExtension = function(filename) {
            var re = /(?:\.([^.]+))?$/;
            var fileExt = re.exec(filename)[1];
            return fileExt;
        };

        var openInNewWindow = function(fileExt) {
            var newWindowFileTypes = ['pdf', 'jpg', 'png', 'gif', 'bmp', 'tif', 'jpeg', 'tiff'];
            return ($.inArray(fileExt, newWindowFileTypes) > -1);
        };

        var title = "GridPopover",

            vm = {
                tag: ko.observable("N/A"),

                code: ko.observable("N/A"),
                codeDescription: ko.observable("N/A"),
                hasCodeUrl: ko.observable(false),
                codeUrl: ko.observable("N/A"),
                value: application.grid.GridValue,

                minValue: ko.observable("N/A"),
                maxValue: ko.observable("N/A"),

                name: ko.observable("N/A"),

                hasId: ko.observable(false),
                id: application.grid.GridCellName,

                description: ko.observable("N/A"),

                gridOwnerCompany: ko.observable("N/A"),
                gridOwnerContactPerson: ko.observable("N/A"),
                gridOwnerEmail: ko.observable("N/A"),
                gridOwnerPhone: ko.observable("N/A"),

                gridDocuments: ko.observableArray(),

                docGuid: ko.observable("N/A"),

                exportSpinner: ko.observable(false),

                exportGrid: function () {
                    vm.exportSpinner(true);
                    dataServices.getFile(vm.docGuid(), 'text/xml', 'grid.xml');
                },

                compositionComplete: function () {
                    app.on('downloadFile:done').then(function () {
                        vm.exportSpinner(false);
                    });

                    app.on("gridChanged:trigger").then(function () {
                        var node = application.grid.Grid().node;

                        if (node) {
                            vm.tag(application.grid.Grid().tags[0]);

                            if (vm.tag() === 'Fylke' || vm.tag() === 'Kommune') {
                                vm.hasId(false);
                            } else {
                                vm.hasId(true);
                            }

                            vm.code(node.code);
                            vm.codeDescription(node.codeDescription);

                            vm.codeUrl(node.codeUrl);

                            if (node.codeUrl !== null) {
                                vm.hasCodeUrl(true);
                            }

                            vm.minValue(node.minValue);
                            vm.maxValue(node.maxValue);

                            vm.name(node.name);

                            vm.description(node.description);

                            vm.gridOwnerCompany(node.owner.company);
                            vm.gridOwnerContactPerson(node.owner.contactPerson);
                            vm.gridOwnerEmail(node.owner.email);
                            vm.gridOwnerPhone(node.owner.phone);

                            vm.gridDocuments([]);
                            for (var i = 0; i < node.documents.length; ++i) {

                                var title;
                                if (node.documents[i].title) title = node.documents[i].title;
                                else title = node.documents[i].fileName;

                                var tooltip = "";
                                if (node.documents[i].description) tooltip = node.documents[i].description;

                                var document = {
                                    "title": title,
                                    "url": config.dataDeliveryApiUrl + 'DownloadDocument/' + node.documents[i].guid,
                                    "tooltip": tooltip,
                                    "filename": node.documents[i].fileName,
                                    "newWindow": openInNewWindow(findFileExtension(node.documents[i].fileName))
                                };

                                vm.gridDocuments.push(document);
                            }

                            vm.docGuid(node.docGuid);
                        }
                    });
                }
            };

        return vm;
    });

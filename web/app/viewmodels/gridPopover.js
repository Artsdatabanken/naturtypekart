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

                            vm.code(node.Code);
                            vm.codeDescription(node.CodeDescription);

                            vm.codeUrl(node.CodeUrl);

                            if (node.CodeUrl !== null) {
                                vm.hasCodeUrl(true);
                            }

                            vm.minValue(node.MinValue);
                            vm.maxValue(node.MaxValue);

                            vm.name(node.Name);

                            vm.description(node.Description);

                            vm.gridOwnerCompany(node.Owner.Company);
                            vm.gridOwnerContactPerson(node.Owner.ContactPerson);
                            vm.gridOwnerEmail(node.Owner.Email);
                            vm.gridOwnerPhone(node.Owner.Phone);

                            vm.gridDocuments([]);
                            for (var i = 0; i < node.Documents.length; ++i) {

                                var title;
                                if (node.Documents[i].Title) title = node.Documents[i].Title;
                                else title = node.Documents[i].FileName;

                                var tooltip = "";
                                if (node.Documents[i].Description) tooltip = node.Documents[i].Description;

                                var document = {
                                    "title": title,
                                    "url": config.dataDeliveryApiUrl + 'DownloadDocument/' + node.Documents[i].Guid,
                                    "tooltip": tooltip,
                                    "filename": node.Documents[i].FileName,
                                    "newWindow": openInNewWindow(findFileExtension(node.Documents[i].FileName))
                                };

                                vm.gridDocuments.push(document);
                            }

                            vm.docGuid(node.DocGuid);
                        }
                    });
                }
            };

        return vm;
    });

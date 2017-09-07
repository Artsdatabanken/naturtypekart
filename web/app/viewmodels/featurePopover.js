define(['services/logger', "knockout", 'durandal/app', 'services/application', 'services/config', 'services/codeLists', 'services/pieChart', 'services/dataServices', 'viewmodels/nav'],
    function (logger, ko, app, application, config, codelists, pieChart, dataServices, nav) {
        "use strict";

        var natureTypes = [];
        var natureTypeShares = [];

        var findFileExtension = function(filename) {
            var re = /(?:\.([^.]+))?$/;
            var fileExt = re.exec(filename)[1];
            return fileExt;
        };

        var openInNewWindow = function(fileExt) {
            var newWindowFileTypes = ['pdf', 'jpg', 'png', 'gif', 'bmp', 'tif', 'jpeg', 'tiff'];
            return ($.inArray(fileExt, newWindowFileTypes) > -1);
        };

        var vm = {
            natureAreaNatureLevel: ko.observable(),
            natureAreaDescriptionVariables: ko.observableArray(),
            natureAreaId: ko.observable(),
            natureAreaVersion: ko.observable(),
            natureAreaSurveyed: ko.observable(),
            natureAreaDescription: ko.observable(),

            natureAreaSurveyerCompany: ko.observable(),
            natureAreaSurveyerContactPerson: ko.observable(),
            natureAreaSurveyerEmail: ko.observable(),
            natureAreaSurveyerPhone: ko.observable(),

            natureAreaDocuments: ko.observableArray(),

            natureAreaSurveyId: ko.observable(),
            natureAreaSurveyProgram: ko.observable(),
            natureAreaSurveyProjectName: ko.observable(),
            natureAreaSurveyProjectDescription: ko.observable(),
            natureAreaSurveyPurpose: ko.observable(),
            natureAreaSurveyFrom: ko.observable(),
            natureAreaSurveyTo: ko.observable(),
            natureAreaSurveyScale: ko.observable(),

            natureAreaSurveyContractorCompany: ko.observable(),
            natureAreaSurveyContractorContactPerson: ko.observable(),
            natureAreaSurveyContractorEmail: ko.observable(),
            natureAreaSurveyContractorPhone: ko.observable(),

            natureAreaSurveyOwnerCompany: ko.observable(),
            natureAreaSurveyOwnerContactPerson: ko.observable(),
            natureAreaSurveyOwnerEmail: ko.observable(),
            natureAreaSurveyOwnerPhone: ko.observable(),

            natureAreaSurveyDocuments: ko.observableArray(),

            makeChart: function () {
                $('#natureAreaLegends').empty();
                $('#natureAreaChart').empty();
                pieChart('#natureAreaChart', '#natureAreaLegends', natureTypes, natureTypeShares, 80);
            },

            exportOne: function () {
                dataServices.exportNatureAreasByLocalIds([vm.natureAreaId()]).then(function (exportFile) {
                    dataServices.downloadFile(exportFile, 'text/xml', 'data.xml', false);
                });
            },

            factSheet: function () {
                //window.open("factSheet.html#" + vm.natureAreaId(), "_blank"); void (0);
                nav.navigateTo("factsheet/" + application.config.defaultMapCenter + "/" + application.config.defaultMapZoom + "/background/" + application.config.initialBaseMapLayer + "/id/" + vm.natureAreaId());

            }
        };

        app.on("currentFeatureChanged:trigger").then(function () {
            var natureArea = application.currFeature.data;
            var metadata = application.currFeature.metadata;
            var i, j, title, tooltip, document;

            natureTypes = [];
            natureTypeShares = [];

            vm.natureAreaDescriptionVariables([]);

            vm.natureAreaNatureLevel("Naturnivå: " + codelists.natureLevelNames[natureArea.NatureLevel]);

            for (i = 0; i < natureArea.Parameters.length; ++i) {
                if (natureArea.Parameters[i].Share !== undefined) {
                    natureTypes.push({
                        "code": natureArea.Parameters[i].Code,
                        "description": natureArea.Parameters[i].CodeDescription
                    });

                    natureTypeShares.push({
                        "value": natureArea.Parameters[i].Share
                    });
                } else {
                    vm.natureAreaDescriptionVariables.push({
                        "codeAndValue": natureArea.Parameters[i].CodeDescription + " (" + natureArea.Parameters[i].Code + "), verdi: " + natureArea.Parameters[i].Value,
                        "description": natureArea.Parameters[i].Description,
                        "url": natureArea.Parameters[i].CodeUrl
                    });
                }
                if (natureArea.Parameters[i].AdditionalVariables) {
                    for (j = 0; j < natureArea.Parameters[i].AdditionalVariables.length; ++j) {
                        var avar = natureArea.Parameters[i].AdditionalVariables[j];
                        vm.natureAreaDescriptionVariables.push({
                            "codeAndValue": avar.CodeDescription + " (" + avar.Code + "), verdi: " + avar.Value,
                            "description": avar.Description,
                            "url": avar.CodeUrl
                        });
                    }
                }
                if (natureArea.Parameters[i].CustomVariables) {
                    for (j = 0; j < natureArea.Parameters[i].CustomVariables.length; ++j) {
                        var cvar = natureArea.Parameters[i].CustomVariables[j];
                        vm.natureAreaDescriptionVariables.push({
                            "codeAndValue": cvar.Specification + ", verdi: " + cvar.Value,
                            "description": "",
                            "url": ""
                        });
                    }
                }

            }

            vm.natureAreaId(natureArea.UniqueId.LocalId);
            vm.natureAreaVersion(natureArea.Version);
            if (natureArea.Surveyed) vm.natureAreaSurveyed(application.formatDate(natureArea.Surveyed));
            if (natureArea.Description) vm.natureAreaDescription(natureArea.Description);

            if (natureArea.Surveyer) {
                vm.natureAreaSurveyerCompany(natureArea.Surveyer.Company);
                if (natureArea.Surveyer.ContactPerson) vm.natureAreaSurveyerContactPerson(natureArea.Surveyer.ContactPerson);
                if (natureArea.Surveyer.Email) vm.natureAreaSurveyerEmail(natureArea.Surveyer.Email);
                if (natureArea.Surveyer.Phone) vm.natureAreaSurveyerPhone(natureArea.Surveyer.Phone);
            }

            vm.natureAreaSurveyId(metadata.UniqueId.LocalId);
            vm.natureAreaSurveyProgram(metadata.Program);
            vm.natureAreaSurveyProjectName(metadata.ProjectName);
            if (metadata.ProjectDescription) vm.natureAreaSurveyProjectDescription(metadata.ProjectDescription);
            if (metadata.Purpose) vm.natureAreaSurveyPurpose(metadata.Purpose);
            vm.natureAreaSurveyFrom(application.formatDate(metadata.SurveyedFrom));
            vm.natureAreaSurveyTo(application.formatDate(metadata.SurveyedTo));
            if (metadata.SurveyScale) vm.natureAreaSurveyScale(metadata.SurveyScale);

            vm.natureAreaDocuments([]);
            for (i = 0; i < natureArea.Documents.length; ++i) {

                title = "";
                if (natureArea.Documents[i].Title) title = natureArea.Documents[i].Title;
                else title = natureArea.Documents[i].FileName;

                tooltip = "";
                if (natureArea.Documents[i].Description) tooltip = natureArea.Documents[i].Description;

                document = {
                    "title": title,
                    "url": config.dataDeliveryApiUrl + 'DownloadDocument/' + natureArea.Documents[i].Guid,
                    "tooltip": tooltip,
                    "filename": natureArea.Documents[i].FileName,
                    "newWindow": openInNewWindow(findFileExtension(natureArea.Documents[i].FileName))
                };

                vm.natureAreaDocuments.push(document);
            }

            vm.natureAreaSurveyContractorCompany(metadata.Contractor.Company);
            if (metadata.Contractor.ContactPerson) vm.natureAreaSurveyContractorContactPerson(metadata.Contractor.ContactPerson);
            if (metadata.Contractor.Email) vm.natureAreaSurveyContractorEmail(metadata.Contractor.Email);
            if (metadata.Contractor.Phone) vm.natureAreaSurveyContractorPhone(metadata.Contractor.Phone);

            vm.natureAreaSurveyOwnerCompany(metadata.Owner.Company);
            if (metadata.Owner.ContactPerson) vm.natureAreaSurveyOwnerContactPerson(metadata.Owner.ContactPerson);
            if (metadata.Owner.Email) vm.natureAreaSurveyOwnerEmail(metadata.Owner.Email);
            if (metadata.Owner.Phone) vm.natureAreaSurveyOwnerPhone(metadata.Owner.Phone);

            vm.natureAreaSurveyDocuments([]);
            for (i = 0; i < metadata.Documents.length; ++i) {

                title = "";
                if (metadata.Documents[i].Title) title = metadata.Documents[i].Title;
                else title = metadata.Documents[i].FileName;

                tooltip = "";
                if (metadata.Documents[i].Description) tooltip = metadata.Documents[i].Description;

                vm.natureAreaSurveyDocuments.push({
                    "title": title,
                    "url": config.dataDeliveryApiUrl + 'DownloadDocument/' + metadata.Documents[i].Guid,
                    "tooltip": tooltip,
                    "filename": metadata.Documents[i].FileName,
                    "newWindow": openInNewWindow(findFileExtension(metadata.Documents[i].FileName))
                });
            }
            vm.makeChart();
        });

        return vm;
    });

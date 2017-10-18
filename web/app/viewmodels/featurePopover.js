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
            redlistcategories: ko.observableArray(),
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

            vm.redlistcategories([]);
            vm.natureAreaDescriptionVariables([]);

            vm.natureAreaNatureLevel("Naturnivå: " + codelists.natureLevelNames[natureArea.nivå]);

            for (i = 0; i < natureArea.parameters.length; ++i) {
                if (natureArea.parameters[i].share !== undefined) {
                    natureTypes.push({
                        "code": natureArea.parameters[i].code,
                        "description": natureArea.parameters[i].codeDescription
                    });

                    natureTypeShares.push({
                        "value": natureArea.parameters[i].share
                    });
                } else  {
                    vm.natureAreaDescriptionVariables.push({
                        "codeAndValue": natureArea.parameters[i].codeDescription + " (" + natureArea.parameters[i].code + "), verdi: " + natureArea.parameters[i].value,
                        "description": natureArea.parameters[i].description,
                        "url": natureArea.parameters[i].codeUrl
                    });
                }
                if (natureArea.parameters[i].additionalVariables) {
                    for (j = 0; j < natureArea.parameters[i].additionalVariables.length; ++j) {
                        var avar = natureArea.parameters[i].additionalVariables[j];
                        vm.natureAreaDescriptionVariables.push({
                            "codeAndValue": (avar.mainTypeDescription ? avar.mainTypeDescription + " - " : "")  + avar.codeDescription + " (" + avar.code + "), verdi: " + avar.value,
                            "description": avar.description,
                            "url": avar.codeUrl
                        });
                    }
                }
                if (natureArea.parameters[i].customVariables) {
                    for (j = 0; j < natureArea.parameters[i].customVariables.length; ++j) {
                        var cvar = natureArea.parameters[i].customVariables[j];
                        vm.natureAreaDescriptionVariables.push({
                            "codeAndValue": cvar.specification + ", verdi: " + cvar.value,
                            "description": "",
                            "url": ""
                        });
                    }
                }
            }
            if (natureArea.rødlisteKategori !== undefined) {
                vm.redlistcategories.push({
                    "category":  natureArea.rødlisteKategori.code,
                    "name": config.categoryNameMapping[natureArea.rødlisteKategori.id],
                    "vurderingsenhet": natureArea.rødlisteKategori.vurderingsenhet ? natureArea.rødlisteKategori.vurderingsenhet.code : "",
                    "tema": natureArea.rødlisteKategori.vurderingsenhet ? natureArea.rødlisteKategori.vurderingsenhet.tema.code : "",
                    "url": "https://www.artsdatabanken.no/rodlistefornaturtyper"
                });
            }
            vm.natureAreaDescriptionVariables(
                vm.natureAreaDescriptionVariables().sort(function (a, b) {
                    return a.codeAndValue === b.codeAndValue ? 0 : (a.codeAndValue < b.codeAndValue ? -1 : 1);
                })
            );
            if (natureArea.uniqueId) {
                vm.natureAreaId(natureArea.uniqueId.localId || "");
            }
            vm.natureAreaVersion(natureArea.version);
            vm.natureAreaSurveyed(natureArea.surveyed ? application.formatDate(natureArea.surveyed) : "");
            vm.natureAreaDescription(natureArea.description || "");

            if (natureArea.surveyer) {
                vm.natureAreaSurveyerCompany(natureArea.surveyer.company);
                vm.natureAreaSurveyerContactPerson(natureArea.surveyer.contactPerson || "");
                vm.natureAreaSurveyerEmail(natureArea.surveyer.email || "");
                vm.natureAreaSurveyerPhone(natureArea.surveyer.phone || "");
            }

            vm.natureAreaSurveyId(metadata.uniqueId.localId || "");
            vm.natureAreaSurveyProgram(metadata.program || "");
            vm.natureAreaSurveyProjectName(metadata.projectName || "");
            vm.natureAreaSurveyProjectDescription(metadata.projectDescription || "");
            vm.natureAreaSurveyPurpose(metadata.purpose || "");
            vm.natureAreaSurveyFrom(metadata.surveyedFrom ? application.formatDate(metadata.surveyedFrom) : "");
            vm.natureAreaSurveyTo(metadata.surveyedTo ? application.formatDate(metadata.surveyedTo) : "");
            vm.natureAreaSurveyScale(metadata.surveyScale || "");

            vm.natureAreaDocuments([]);
            for (i = 0; i < natureArea.documents.length; ++i) {

                title = natureArea.documents[i].title || natureArea.documents[i].fileName;
                tooltip = natureArea.documents[i].description || "";

                document = {
                    "title": title,
                    "url": config.dataDeliveryApiUrl + 'DownloadDocument/' + natureArea.documents[i].guid,
                    "tooltip": tooltip,
                    "filename": natureArea.documents[i].fileName,
                    "newWindow": openInNewWindow(findFileExtension(natureArea.documents[i].fileName))
                };

                vm.natureAreaDocuments.push(document);
            }

            if (metadata.contractor) {
                vm.natureAreaSurveyContractorCompany(metadata.contractor.company || "");
                vm.natureAreaSurveyContractorContactPerson(metadata.contractor.contactPerson || "");
                vm.natureAreaSurveyContractorEmail(metadata.contractor.email || "");
                vm.natureAreaSurveyContractorPhone(metadata.contractor.phone || "");
            }

            if (metadata.owner)  {
                vm.natureAreaSurveyOwnerCompany(metadata.owner.company);
                vm.natureAreaSurveyOwnerContactPerson(metadata.owner.contactPerson || "");
                vm.natureAreaSurveyOwnerEmail(metadata.owner.email || "");
                vm.natureAreaSurveyOwnerPhone(metadata.owner.phone || "");
            }

            vm.natureAreaSurveyDocuments([]);
            for (i = 0; i < metadata.documents.length; ++i) {

                title = metadata.documents[i].title || metadata.documents[i].fileName;
                tooltip = metadata.documents[i].description || "";

                vm.natureAreaSurveyDocuments.push({
                    "title": title,
                    "url": config.dataDeliveryApiUrl + 'DownloadDocument/' + metadata.documents[i].guid,
                    "tooltip": tooltip,
                    "filename": metadata.documents[i].fileName,
                    "newWindow": openInNewWindow(findFileExtension(metadata.documents[i].fileName))
                });
            }
            vm.makeChart();
        });

        return vm;
    });

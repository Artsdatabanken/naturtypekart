define(['services/logger', "knockout", 'durandal/app', 'services/application', 'services/config', 'services/codeLists', 'services/pieChart', 'viewmodels/nav', 'services/dataServices', 'services/layerConfig'],
    function (logger, ko, app, application, config, codelists, pieChart, nav, dataServices, layerConfig) {
        var title = "factsheet";

        var natureTypes = [];
        var natureTypeShares = [];

        var findFileExtension = function(filename) {
            var re = /(?:\.([^.]+))?$/;
            var fileExt = re.exec(filename)[1];
            return fileExt;
        };

        var openInNewWindow = function(fileExt) {
            var newWindowFileTypes = ['jpg', 'png', 'gif', 'bmp', 'tif', 'jpeg', 'tiff'];
            return ($.inArray(fileExt, newWindowFileTypes) > -1);
        };

        var raster = new ol.layer.Tile({
            source: new ol.source.OSM({
                attributions: [
                    new ol.Attribution({
                        html: 'All maps &copy; ' +
                        '<a href="//www.openstreetmap.org/">OpenStreetMap</a>'
                    }),
                    ol.source.OSM.ATTRIBUTION
                ],
                url: '//{a-c}.tile.openstreetmap.org/{z}/{x}/{y}.png',
                crossOrigin: 'anonymous'
            })
        });
        var view = new ol.View({
            projection: 'EPSG:32633',
            center: [0, 0],
            zoom: 1
        });
        var format = new ol.format.WKT();
        var feature, vector;

        var vm = {
            updateInProgress: ko.observable(true),
            sortDirection: ko.observable(false),

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
            natureAreaKommuner: ko.observableArray(),
            natureAreaFylker: ko.observableArray(),
            natureAreaConservationAreas: ko.observableArray(),

            natureAreaPolygon: ko.observable(),
            surveyPolygon: ko.observable(),

            showingHistoric: ko.observable(false),
            historicCreated: ko.observable(),
            historicDeliveryDate: ko.observable(),
            historicDescription: ko.observable(),
            historicExpired: ko.observable(),
            colorCode: ko.observable(),
            image: ko.observable(),
            makeChart: function () {
                $('#fsNatureAreaLegends').empty();
                $('#fsNatureAreaChart').empty();
                pieChart('#fsNatureAreaChart', '#fsNatureAreaLegends', natureTypes, natureTypeShares);
            }
        };
        vm.activate = function (center, zoom, background, id, filter) {
            vm.updateInProgress(true);

            app.trigger('factsheet:activate', id);
            if (id) {
                dataServices.getNatureAreaByLocalId(id).then(function (data) {
                    dataServices.getMetadataByNatureAreaLocalId(id).then(function (metadata) {
                        application.currentFeature(data, metadata);
                    });
                });
            }
        };
        vm.getSortOrderDirectionValue1 = function() {
            return vm.sortDirection() ? 1 : -1;
        };
        vm.getSortOrderDirectionValue2 = function () {
            return vm.sortDirection() ? -1 : 1;
        };
        vm.orderDescriptionVariablesByColumn = function (col) {
            vm.sortDirection(!vm.sortDirection());
            vm.natureAreaDescriptionVariables.sort(function (left, right) {
                return left[col] === right[col] ? 0 : left[col] < right[col] ? vm.getSortOrderDirectionValue1()
                        : vm.getSortOrderDirectionValue2();
            });
        };

        var getVector = function(wkt, style) {
            feature = format.readFeature(wkt,
            {
                dataProjection: 'EPSG:32633',
                featureProjection: 'EPSG:32633'
            });
            feature.set("ColorCode", vm.colorCode());

            vector = new ol.layer.Vector({
                source: new ol.source.Vector({
                    features: [feature]
                }),
                style: style
            });
            return vector;
        };

        var getSurveyStyle = function() {
            return new ol.style.Style({
                stroke: new ol.style.Stroke({
                    color: '#afa',
                    width: 1.0
                }),
                fill: new ol.style.Fill({
                    color: [51, 153, 204, 0.05]
                })
            });
        };

        vm.updateMap = function () {

            var naVector = getVector(vm.natureAreaPolygon(), layerConfig.natureLevelStyleFunc);
            var surveyVector = getVector(vm.surveyPolygon(), getSurveyStyle);
            vm.map.setLayerGroup(new ol.layer.Group({
                layers: [raster, surveyVector, naVector]
            }));
            vm.map.setView(view);

            //var polygon = /** @type {ol.geom.SimpleGeometry} */ (surveyVector.getSource().getFeatures()[0].getGeometry());
            var polygon = /** @type {ol.geom.SimpleGeometry} */ (naVector.getSource().getFeatures()[0].getGeometry());
            var size = /** @type {ol.Size} */ (vm.map.getSize());
            view.fit(
                polygon,
                size,
                {
                    padding: [100, 150, 100, 150], // top, right, bottom and left padding
                    nearest: true
                });
        };

        vm.compositionComplete = function () {
            vm.map = new ol.Map({
                controls: ol.control.defaults().extend([
                    new ol.control.ScaleLine({
                        units: 'metric'
                    })
                ]),
                layers: [raster],
                target: 'factsheetmap'
            });
            vm.map.on("postrender", vm.makeImage, this);
        };


        vm.updateContentFromExpired = function (data) {
            vm.updateInProgress(true);
            if (data) {
                vm.showingHistoric(true);
                var natureArea = data.metadata.natureAreas[0];
                if (data.metadata.natureAreas.length > 1 || data.metadata.natureAreas[0].parameters.length > 1) {
                    vm.colorCode("MOSAIC");
                } else {
                    vm.colorCode(data.metadata.natureAreas[0].parameters[0].code || 'UNKNOWN');
                }
                var metadata = data.metadata;

                vm.updateContent(natureArea, metadata);
                vm.natureAreaPolygon(natureArea.area);
                vm.surveyPolygon(metadata.area);

                vm.historicCreated(application.formatDate(data.created));
                vm.historicDeliveryDate(application.formatDate(data.deliveryDate));
                vm.historicDescription(data.description);
                vm.historicExpired(application.formatDate(data.expired));

                vm.updateMap();
            } else {
                // Reset to current
                vm.showingHistoric(false);
                vm.updateContent(application.currFeature.data, application.currFeature.metadata);
            }
            vm.updateInProgress(false);
        };

        vm.makeImage = function() {
            vm.image(vm.map.getViewport().children[0].toDataURL('image/png'));
            $("#factsheetmap").hide();
        };

        vm.updateContent = function (natureArea, metadata) {
            var i, title, tooltip, document; 
            natureTypes = [];
            natureTypeShares = [];

            vm.natureAreaDescriptionVariables([]);
            vm.natureAreaKommuner([]);
            vm.natureAreaFylker([]);
            vm.natureAreaConservationAreas([]);

            vm.natureAreaNatureLevel(codelists.natureLevelNames[natureArea.nivå]);

            var finnNode = function (text, code) {
                if (text !== "?") {
                    var result = $('#summarytree').treeview('search', [text, {
                        ignoreCase: false,     // case insensitive
                        exactMatch: true,    // like or equals
                        revealResults: false  // reveal matching nodes
                    }]);
                    for (var r = 0; r < result.length; r++) {
                        if (result[r].code === code) {
                            return result[r];
                        }
                    }
                }
                return undefined;
            };
            var hentNode = function (id) {
                return $('#summarytree').treeview('getNode', id);
            };

            for (i = 0; i < natureArea.parameters.length; ++i) {
                if (natureArea.parameters[i].share !== undefined) {
                    natureTypes.push({
                        "code": natureArea.parameters[i].code,
                        "description": natureArea.parameters[i].codeDescription
                    });

                    natureTypeShares.push({
                        "value": natureArea.parameters[i].share
                    });
                } else {
                    vm.natureAreaDescriptionVariables.push({
                        "codeAndDescription": natureArea.parameters[i].codeDescription + " (" + natureArea.parameters[i].code + ")",
                        "value": natureArea.parameters[i].value,
                        "mainTypeCodeAndDescription": natureArea.parameters[i].mainTypeDescription + " (" + natureArea.parameters[i].mainTypeCode + ")"
                        //,"description": natureArea.parameters[i].description
                    });
                }
                if (natureArea.parameters[i].additionalVariables) {
                    for (j = 0; j < natureArea.parameters[i].additionalVariables.length; ++j) {
                        var avar = natureArea.parameters[i].additionalVariables[j];
                        var treeNode = finnNode(avar.codeDescription, avar.code);
                        var kartleggingsEnhet = {};
                        var grunntype = {};
                        if (treeNode) {
                            kartleggingsEnhet = hentNode(treeNode.parentId);
                            if (kartleggingsEnhet) {
                                grunntype = hentNode(kartleggingsEnhet.parentId);
                            }

                        }
                        vm.natureAreaDescriptionVariables.push({
                            "codeAndDescription": avar.codeDescription + " (" + avar.code + ")",
                            "value": avar.value,
                            "mainTypeGroup": "",
                            "mainTypeCodeAndDescription": avar.mainTypeDescription + " (" + avar.mainTypeCode + ")",
                            "kartleggingsEnhet" : kartleggingsEnhet.text,
                            "grunntype" : grunntype.text
                        });
                    }
                }
                if (natureArea.parameters[i].customVariables) {
                    for (j = 0; j < natureArea.parameters[i].customVariables.length; ++j) {
                        var cvar = natureArea.parameters[i].customVariables[j];
                        vm.natureAreaDescriptionVariables.push({
                            "codeAndDescription": cvar.specification,
                            "value": cvar.value,
                            "mainTypeGroup": "",
                            "mainTypeCodeAndDescription": "",
                            "kartleggingsEnhet": "",
                            "grunntype": ""
                        });
                    }
                }
            }
            var areaObj;
            if (natureArea.areas !== undefined) {
                for (i = 0; i < natureArea.areas.length; ++i) {
                    areaObj = natureArea.areas[i];
                    if (Number(areaObj.type) === 1) {
                        vm.natureAreaKommuner.push({
                            "name": areaObj.name
                        });
                    } else if (Number(areaObj.type) === 2) {
                        vm.natureAreaFylker.push({
                            "name": areaObj.name
                        });
                    } else if (Number(areaObj.type) === 3) {
                        vm.natureAreaConservationAreas.push({
                            "name": areaObj.name
                        });
                    }
                }
            }

            vm.natureAreaId(natureArea.uniqueId.localId || "");
            vm.natureAreaVersion(natureArea.version || "");
            vm.natureAreaSurveyed(natureArea.surveyed ? application.formatDate(natureArea.surveyed) : "");
            vm.natureAreaDescription(natureArea.description || "");

            if (natureArea.surveyer) {
                vm.natureAreaSurveyerCompany(natureArea.surveyer.company);
                if (natureArea.surveyer) {
                    vm.natureAreaSurveyerContactPerson(natureArea.surveyer.contactPerson || "");
                    vm.natureAreaSurveyerEmail(natureArea.surveyer.email || "");
                    vm.natureAreaSurveyerPhone(natureArea.surveyer.phone || "");
                }
            }

            vm.natureAreaSurveyId(metadata.uniqueId.localId);
            vm.natureAreaSurveyProgram(metadata.program);
            vm.natureAreaSurveyProjectName(metadata.projectName);
            vm.natureAreaSurveyProjectDescription(metadata.projectDescription || "");
            vm.natureAreaSurveyPurpose(metadata.purpose || "");
            vm.natureAreaSurveyFrom(metadata.surveyedFrom);
            vm.natureAreaSurveyTo(metadata.surveyedTo);
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
                vm.natureAreaSurveyContractorCompany(metadata.contractor.company);
                vm.natureAreaSurveyContractorContactPerson(metadata.contractor.contactPerson || "");
                vm.natureAreaSurveyContractorEmail(metadata.contractor.email || "");
                vm.natureAreaSurveyContractorPhone(metadata.contractor.phone || "");
            }

            if (metadata.owner) {
                vm.natureAreaSurveyOwnerCompany(metadata.owner.company || "");
                vm.natureAreaSurveyOwnerContactPerson(metadata.owner.contactPerson || "");
                vm.natureAreaSurveyOwnerEmail(metadata.owner.email || "");
                vm.natureAreaSurveyOwnerPhone(metadata.owner.phone || "");
            }

            vm.natureAreaSurveyDocuments([]);
            for (i = 0; i < metadata.documents.length; ++i) {

                title = metadata.documents[i].title || metadata.documents[i].fileName;
                tooltip = metadata.documents[i].description || "";

                document = {
                    "title": title,
                    "url": config.dataDeliveryApiUrl + 'DownloadDocument/' + metadata.documents[i].guid,
                    "tooltip": tooltip,
                    "filename": metadata.documents[i].fileName,
                    "newWindow": openInNewWindow(findFileExtension(metadata.documents[i].fileName))
                };
                vm.natureAreaSurveyDocuments.push(document);
            }
            vm.natureAreaPolygon(natureArea.area);
            vm.surveyPolygon(metadata.area);
            vm.updateMap();


            vm.makeChart();
            vm.updateInProgress(false);
        };

        app.on('main:resized').then(function () {
            if (vm.map && nav.activeView() === 'factsheet') {
                vm.map.updateSize();
            }
        });

        app.on("currentFeatureChanged:trigger").then(function () {
            vm.updateContent(application.currFeature.data, application.currFeature.metadata);
        });

        app.on('factsheetHistory:change').then(function(data) {
            if (nav.activeView() === 'factsheet') {
                $("#factsheetmap").show();
                vm.updateContentFromExpired(data);
            }
        });

        return vm;
    });

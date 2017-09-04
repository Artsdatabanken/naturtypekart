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
                            '<a href="http://www.openstreetmap.org/">OpenStreetMap</a>'
                    }),
                    ol.source.OSM.ATTRIBUTION
                ],
                url: '//{a-c}.tile.openstreetmap.org/{z}/{x}/{y}.png'
            })
        });
        var view = new ol.View({
            center: [0, 0],
            zoom: 1
        });
        var format = new ol.format.WKT();
        var feature, vector;

        var vm = {
            updateInProgress: ko.observable(true),

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
        vm.activate = function (center, zoom, background, id) {
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
                var natureArea = data.Metadata.NatureAreas[0];
                if (data.Metadata.NatureAreas.length > 1 || data.Metadata.NatureAreas[0].Parameters.length > 1) {
                    vm.colorCode("MOSAIC");
                } else {
                    vm.colorCode(data.Metadata.NatureAreas[0].Parameters[0].Code || 'UNKNOWN');
                }
                var metadata = data.Metadata;

                vm.updateContent(natureArea, metadata);
                vm.natureAreaPolygon(natureArea.Area);
                vm.surveyPolygon(metadata.Area);

                vm.historicCreated(application.formatDate(data.Created));
                vm.historicDeliveryDate(application.formatDate(data.DeliveryDate));
                vm.historicDescription(data.Description);
                vm.historicExpired(application.formatDate(data.Expired));

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

            vm.natureAreaNatureLevel(codelists.natureLevelNames[natureArea.NatureLevel]);

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
                        "codeAndDescription": natureArea.Parameters[i].CodeDescription + " (" + natureArea.Parameters[i].Code + ")",
                        "value": natureArea.Parameters[i].Value,
                        "mainTypeCodeAndDescription": natureArea.Parameters[i].MainTypeDescription + " (" + natureArea.Parameters[i].MainTypeCode + ")"
                        //,"description": natureArea.Parameters[i].Description
                    });
                }
                if (natureArea.Parameters[i].AdditionalVariables) {
                    for (j = 0; j < natureArea.Parameters[i].AdditionalVariables.length; ++j) {
                        var avar = natureArea.Parameters[i].AdditionalVariables[j];
                        vm.natureAreaDescriptionVariables.push({
                            "codeAndDescription": avar.CodeDescription + " (" + avar.Code + ")",
                            "value": avar.Value,
                            "mainTypeCodeAndDescription": avar.MainTypeDescription + " (" + avar.MainTypeCode + ")"
                        });
                    }
                }
                if (natureArea.Parameters[i].CustomVariables) {
                    for (j = 0; j < natureArea.Parameters[i].CustomVariables.length; ++j) {
                        var cvar = natureArea.Parameters[i].CustomVariables[j];
                        vm.natureAreaDescriptionVariables.push({
                            "codeAndDescription": cvar.Specification,
                            "value": cvar.Value,
                            "mainTypeCodeAndDescription": ""
                        });
                    }
                }
            }
            var areaObj;
            if (natureArea.Areas !== undefined) {
                for (i = 0; i < natureArea.Areas.length; ++i) {
                    areaObj = natureArea.Areas[i];
                    if (Number(areaObj.Type) === 1) {
                        vm.natureAreaKommuner.push({
                            "name": areaObj.Name
                        });
                    } else if (Number(areaObj.Type) === 2) {
                        vm.natureAreaFylker.push({
                            "name": areaObj.Name
                        });
                    } else if (Number(areaObj.Type) === 3) {
                        vm.natureAreaConservationAreas.push({
                            "name": areaObj.Name
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
            vm.natureAreaSurveyFrom(metadata.SurveyedFrom);
            vm.natureAreaSurveyTo(metadata.SurveyedTo);
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

                document = {
                    "title": title,
                    "url": config.dataDeliveryApiUrl + 'DownloadDocument/' + metadata.Documents[i].Guid,
                    "tooltip": tooltip,
                    "filename": metadata.Documents[i].FileName,
                    "newWindow": openInNewWindow(findFileExtension(metadata.Documents[i].FileName))
                };
                vm.natureAreaSurveyDocuments.push(document);
            }

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

define(['services/logger', 'durandal/app', "knockout", 'jquery', 'viewmodels/nav', 'services/application', 'services/dataServices', 'services/layerConfig'],
    function (logger, app, ko, $, nav, application, dataServices, layerConfig) {
        "use strict";
        var title = 'Map',
            viewportState = application.viewportState,
            mapLoadedDfd = $.Deferred(),
            mapLoaded = mapLoadedDfd.promise(),
            mapViewPortSetDfd = $.Deferred(),
            mapViewPortSet = mapViewPortSetDfd.promise(),
            vm,
            selectionLayerSource = new ol.source.Vector({ wrapX: false }),
            polygonLayerSource = new ol.source.Vector({ wrapX: false }),
            polygonLayerClusterSource = new ol.source.Vector({ wrapX: false }),
            gridLayerSource = new ol.source.Vector({ wrapX: false }),
            maxClusterCircleSize = 30,
            clusterDistance = 50,
            colorScale,
            clusterColorScale = d3.scale.linear()
                            .domain([1, 50, 5000])
                            .range(["lightblue", "blue", "black"]),

            polygonLayerCluster = new ol.source.Cluster({
                distance: clusterDistance, // formula to set cluster distance
                source: polygonLayerClusterSource
            }),
            draw = new ol.interaction.Draw({
                source: selectionLayerSource,
                type: 'Polygon'
            }),

            storePopup = function() {
                // To avoid losing the popup if it is open when you switch view
                $("#myPopoverContentContainer").append($("#natureAreaInfo"));
                // We don't need to keep this around any longer.
                $("#popup").popover("destroy");
                if (vm.select && vm.select.getFeatures()) {
                    vm.select.getFeatures().clear();
                }
            },
            storeGridPopup = function () {
                // To avoid losing the popup if it is open when you switch view
                $("#myGridPopoverContentContainer").append($("#gridInfo"));
                // We don't need to keep this around any longer.
                $("#gridInfoPopup").popover("destroy");
                vm.gridPopupOverlay.setPosition(undefined);
            },

            activate = function (center, zoom, background, id) {
                //logger.log(title + ' View activate', null, title, true);
                viewportState.center(center);
                viewportState.zoom(zoom);
                viewportState.background(background);
                viewportState.id(id);
                if (vm && vm.map) {
                    // todo: find better solution to this than using setTimeout! Need to run vm.map.updateSize(); after all rendering is complete
                    // updatesize is necessary if window size has been changed while in a different view.
                    setTimeout(function() {
                         vm.map.updateSize();
                    }, 500);

                }
                app.trigger('mapview:activate', '');
            },

            compositionComplete = function () {

                var gridStyleCache = {};
                var gridStyleFunc = function(feature, resolution) {
                    var val = feature.getProperties().value;
                    var rgb = colorScale ? d3.rgb(colorScale(val)) : { r: 255, g: 255, b: 255 };
                    //console.debug(val + " - " + rgb);
                    var gridStyle = gridStyleCache[val]; // get from cache if it has already been created
                    if (!gridStyle) {
                        gridStyle = [
                            new ol.style.Style({
                                stroke: new ol.style.Stroke({
                                    color: '#555',
                                    width: 0.4
                                }),
                                fill: new ol.style.Fill({
                                    color: [rgb.r, rgb.g, rgb.b, 0.3]
                                    //color: [Number(col), Number(col), (Math.random() * 255).toFixed(0), 0.5]
                                })
                            })
                        ];
                        gridStyleCache[val] = gridStyle;
                        //console.debug("cached " + col + " " + feature.getId());
                    }

                    return gridStyle;
                };

                vm.gridLayer = new ol.layer.Vector({
                    source: gridLayerSource,
                    style: gridStyleFunc
                });

                vm.polygonLayer = new ol.layer.Vector({
                    source: polygonLayerSource,
                    style: layerConfig.natureLevelStyleFunc
                });

                var styleCache = {};
                vm.polygonClusterLayer = new ol.layer.Vector({
                    source: polygonLayerCluster,
                    style: function (feature, resolution) {
                        var nFeatures = feature.get('features').length;
                        var style = styleCache[nFeatures];   // get from cache if it has already been created

                        if (!style) {

                            style = [new ol.style.Style({
                                image: new ol.style.Circle({
                                    radius: Math.min((10 + nFeatures / 70), maxClusterCircleSize),    // formula to calculate size of circle
                                    stroke: new ol.style.Stroke({
                                        color: '#fff'
                                    }),
                                    fill: new ol.style.Fill({
                                        //color: '#3399CC'
                                        color: clusterColorScale(nFeatures) //[51, 153, 204, Math.min(0.8, 0.3 + (nFeatures / 70))] // formula to calculate transparency of circle
                                    })
                                }),
                                text: new ol.style.Text({
                                    text: nFeatures.toString(),
                                    fill: new ol.style.Fill({
                                        color: '#fff'
                                    })
                                })
                            })];
                            styleCache[nFeatures] = style;   // Cache each different style created
                        }
                        return style;
                    }
                });

                // selectionLayer: The layer we draw in / use for filtering, has only one geometry obj
                vm.selectionLayer = new ol.layer.Vector({
                    source: selectionLayerSource,
                    style: new ol.style.Style({
                        stroke: new ol.style.Stroke({
                            color: '#00f',
                            width: 1.0
                        }),
                        fill: new ol.style.Fill({
                            color: [51, 153, 204, 0.05]
                        })
                    })
                });
                application.viewportState.background.subscribe(function (value) {
                    vm.selectedBaseLayer(value);
                });
                vm.selectedBaseLayer.subscribe(function (newValue) {
                    vm.changeBaseLayer(newValue);
                });
                vm.changeBaseLayer = function (layerName) {
                    console.log("change layer", layerName);
                    if (typeof layerName === "string" && layerName.length > 0) {
                        var layer = layerConfig.getBaseLayerFromPool(layerName);
                        if (layer) {
                            if (vm.map.getLayers().getLength() > 1) {
                                vm.map.getLayers().removeAt(1);
                            }
                            vm.map.getLayers().insertAt(1, layer);
                            viewportState.background(layerName);
                        }
                    }
                };
                vm.toggleOverlayLayer = function (layerName) {
                    if (typeof layerName === "string" && layerName.length > 0) {
                        var layers = vm.map.getLayers().getArray();
                        for (var i = 0; i < layers.length; i++) {
                            if (layers[i].name === layerName) {
                                vm.map.getLayers().remove(layers[i]);
                                return;
                            }
                        }
                        var layer = layerConfig.getOverlayLayerFromPool(layerName);
                        if (layer) {
                            vm.map.getLayers().insertAt(3, layer); // Add overlay as layer nr 3, in front of background and grid
                        }
                    }
                };
                vm.applyGrid = function (node) {
                    vm.isLoadingGrid(true);
                    if (!node) {
                        vm.gridLayer.getSource().clear();
                        application.grid.Grid("");
                        vm.isLoadingGrid(false);
                        return;
                    }
                    application.grid.GridType(node.type);
                    application.grid.Grid(node);
                    if (isFinite(node.min) && isFinite(node.max)) {
                        colorScale = d3.scale.linear()
                            .domain([node.min, node.max])
                            .range(["yellow", "red"]);
                    } else {
                        // Not numbers, use scale.ordinal instead of linear
                        colorScale = d3.scale.ordinal()
                            .domain([node.min, node.max])
                            .range(["white", "yellow", "red"]);
                    }

                    if (node.code) {
                        application.grid.GridLayerTypeId(node.code);
                    } else {
                        application.grid.GridLayerTypeId("0");
                    }
                    if (node.type) {
                        var vpi = vm.getViewportInfo();
                        var gridFilter = {
                            GridType: application.grid.GridType,
                            Municipalities: application.filter.Municipalities,
                            Counties: application.filter.Counties,
                            Geometry: application.filter.Geometry,
                            BoundingBox: (vpi.zoom >= application.config.gridBoundingBoxLimit) ? vm.boundsToWkt(vpi.bounds) : "", // Add bounding box to filter if zoom > a limit
                            EpsgCode: application.filter.EpsgCode,
                            GridLayerTypeId: application.grid.GridLayerTypeId
                        };
                        dataServices.getGrid(gridFilter).then(function (geojsonObject) {
                            if (geojsonObject === "ERROR: Grid is to big") {
                                application.setFooterWarning("Kunne ikke laste et så stort miljøvariabelkart i denne oppløsningen. Prøv igjen med et mindre område!");
                            } else {
                                vm.gridLayer.getSource().clear();
                                vm.gridLayer.getSource().addFeatures(
                                    new ol.format.GeoJSON({ defaultDataProjection: "EPSG: 32633" }).
                                    readFeatures(geojsonObject, { dataProjection: "EPSG: 32633" })
                                );
                            }
                            vm.isLoadingGrid(false);
                            app.trigger("gridChanged:trigger");
                        }, function(reason) {
                            // failed
                            application.setFooterWarning("Kunne ikke laste miljøvariabelkart!");
                            vm.isLoadingGrid(false);
                        });
                    } else {
                        vm.gridLayer.getSource().clear();
                        vm.isLoadingGrid(false);
                    }
                };

                vm.select = new ol.interaction.Select({
                    wrapX: false,
                    layers: [vm.polygonLayer, vm.gridLayer]
                });

                vm.select.on('select', function (event) {
                    if (!vm.drawing()) {
                        var feat = event.target.getFeatures().getArray()[0];
                        if (feat) {
                            var type = feat.get("type");
                            var value = feat.get("value");
                            
                            switch (type) {
                                case "Municipality":
                                case "County":
                                    var name = feat.get("name");
                                    vm.select.getFeatures().clear();
                                    if (value) {
                                        vm.showPopup(event, value, name);
                                    }
                                    break;
                                default:
                                    if (value) {
                                        vm.select.getFeatures().clear();
                                        var cellId = feat.getId();
                                        vm.showPopup(event, value, cellId);
                                    } else {
                                        vm.featureSelected(event, feat);
                                    }
                            }
                        }
                    }
                });

                /**
                * Create an overlay to anchor the popup to the map.
                */
                vm.overlay = new ol.Overlay(/** @type {olx.OverlayOptions} */({
                    element: document.getElementById('popup')
                }));
                vm.gridPopupOverlay = new ol.Overlay(/** @type {olx.OverlayOptions} */({
                    element: document.getElementById('gridInfoPopup')
                }));

                vm.map = new ol.Map({
                    interactions: ol.interaction.defaults().extend([vm.select]),
                    controls: ol.control.defaults().extend([
                        new ol.control.FullScreen()
                    ]).extend([
                        new ol.control.ScaleLine({
                            units: 'metric'
                        })
                    ]),
                    // europakart alltid underst
                    layers: [layerConfig.baseLayerPool[5], layerConfig.baseLayerPool[0], vm.gridLayer, vm.selectionLayer, vm.polygonLayer, vm.polygonClusterLayer],
                    overlays: [vm.overlay, vm.gridPopupOverlay],
                    target: document.getElementById('map'),
                    view: new ol.View({
                       // projection: 'EPSG:3857',
                       projection: 'EPSG:32633',
                        maxZoom: 18,
                        //center: [1800000, 9700000],
                        center: ol.proj.fromLonLat([-74.0064, 40.7142]),
                        zoom: 5
                    })
                });
                vm.map.on("moveend", vm.changeView, this);
                vm.map.on("updatesize", vm.changeView, this);

                app.on('mapview:activate').then(function () {
                    // If filter was changed while we were in another view, reload Nature areas (polygons/centerpoints) to update mapview
                    if (vm.reloadNatureAreasOnActivate()) {
                        application.filter.ForceRefreshToggle(!application.filter.ForceRefreshToggle());
                        vm.reloadNatureAreasOnActivate(false);
                    }
                });
                app.on('main:resized').then(function (elem) {
                    if (nav.activeView() === 'map') {
                        vm.map.updateSize();
                    }
                });
                selectionLayerSource.on('addfeature', vm.selectionPolygonAdded, this);

                // translate some ol3-texts
                $(".ol-attribution > button").attr("title", "Info om kartlagene");
                $(".ol-zoom-in").attr("title", "Zoom inn");
                $(".ol-zoom-out").attr("title", "Zoom ut");
                $(".ol-full-screen-false").attr("title", "Veksle fullskjermskart");
                $(".ol-full-screen-true").attr("title", "Veksle fullskjermskart");

                vm.reloadLocations = function (keepOld, addBounds) {
                    var filter = application.filter;

                    if (keepOld && addBounds && addBounds.length > 0) {
                        filter = application.noBoundingBoxFilter();
                        filter.BoundingBox = vm.boundsToWkt(addBounds);
                    }
                    dataServices.getNatureAreasBySearchFilter(filter).then(function (geojsonObject) {

                            if (filter.CenterPoints() === false) {
                                if (keepOld && addBounds && addBounds.length > 0) {
                                    // increase record of loaded bounds
                                    var minX = addBounds[0], minY = addBounds[1], maxX = addBounds[2], maxY = addBounds[3];
                                    var lminX = vm.loadedBounds()[0], lminY = vm.loadedBounds()[1], lmaxX = vm.loadedBounds()[2], lmaxY = vm.loadedBounds()[3];

                                    vm.loadedBounds(
                                        [Math.min(minX, lminX),
                                            Math.min(minY, lminY),
                                            Math.max(maxX, lmaxX),
                                            Math.max(maxY, lmaxY)
                                        ]
                                    );
                                } else {
                                    // store bounds for fetched if correct grouping
                                    vm.loadedBounds(vm.getBounds());
                                }
                            } else {
                                vm.loadedBounds(undefined);
                            }

                            if (!keepOld) {
                                polygonLayerSource.clear();
                                polygonLayerClusterSource.clear();
                                //heatmapLayerSource.clear();
                            }

                            if (filter.CenterPoints() === false) {
                                polygonLayerSource.addFeatures(
                                    new ol.format.GeoJSON({defaultDataProjection: "EPSG: 32633"}).readFeatures(geojsonObject, {dataProjection: "EPSG: 32633"})
                                );
                            } else {
                                if (false) {

                                    var featureCollection = JSON.parse(geojsonObject);

                                    // maxHeatmapObservationCount = 0;
                                    // featureCollection.features.forEach(function (f) {
                                    //     maxHeatmapObservationCount = Math.max(maxHeatmapObservationCount, f.properties.ObservationCount);
                                    // });
                                    //
                                    // heatmapLayerSource.addFeatures(
                                    //     new ol.format.GeoJSON({defaultDataProjection: "EPSG: 32633"}).readFeatures(geojsonObject, {dataProjection: "EPSG: 32633"})
                                    // )
                                } else {
                                    polygonLayerClusterSource.addFeatures(
                                        new ol.format.GeoJSON({defaultDataProjection: "EPSG: 32633"}).readFeatures(geojsonObject, {dataProjection: "EPSG: 32633"})
                                    );
                                }
                            }
                            vm.isLoading(false);
                        }, function (reason) {
                            // failed
                            console.debug(reason.statusText);
                            application.setFooterWarning("Kunne ikke laste naturområder!");
                            vm.isLoading(false);
                        }
                    );

                };

                vm.startReloadAreas = function (keepOld, addBounds) {
                    if (nav.activeView() === 'map') {
                        vm.isLoading(true);
                        vm.reloadLocations(keepOld, addBounds);
                    } else {
                        vm.reloadNatureAreasOnActivate(true);
                    }

                };
                application.listFilterChanged.subscribe(function (value) {
                    vm.startReloadAreas();
                });

                application.filterChanged.subscribe(function (value) {
                    if (application.filter.CenterPoints() === false) {
                        if (application.filter.BoundingBox() != "" && vm.loadedBounds() != undefined) {
                            var bounds = vm.getBounds();
                            var minX = bounds[0], minY = bounds[1], maxX = bounds[2], maxY = bounds[3];
                            var lminX = vm.loadedBounds()[0], lminY = vm.loadedBounds()[1], lmaxX = vm.loadedBounds()[2], lmaxY = vm.loadedBounds()[3];
                            var addBounds = [];
                            if (minX >= lminX && minY >= lminY && maxX <= lmaxX && maxY <= lmaxY) {
                                // New bounds completely inside old bounds, do not need to fetch.
                                console.debug("Bruker cachede områder:)");
                                return;
                            } else if (minX >= lminX && minY >= lminY && maxX <= lmaxX && maxY >= lmaxY) {
                                console.debug("Laster flere områder i nord");
                                // New bounds on top+overlapping old
                                //  ..\\\..
                                //  //XXX//
                                //  ///////
                                addBounds = [lminX, lmaxY, lmaxX, maxY];
                                vm.startReloadAreas(true, addBounds);
                            } else if (minX >= lminX && minY >= lminY && maxX >= lmaxX && maxY <= lmaxY) {
                                console.debug("Laster flere observasjoner i øst");
                                // New bounds right+overlapping old
                                //  ///////..
                                //  ////XXX\\
                                //  ////XXX\\
                                //  ///////..
                                addBounds = [lmaxX, lminY, maxX, lmaxY];
                                vm.startReloadAreas(true, addBounds);
                            } else if (minX >= lminX && minY <= lminY && maxX <= lmaxX && maxY <= lmaxY) {
                                console.debug("Laster flere områder i sør");
                                // New bounds bottom+overlapping old
                                //  ///////
                                //  //XXX//
                                //  ..\\\..
                                addBounds = [lminX, minY, lmaxX, lmaxY];
                                vm.startReloadAreas(true, addBounds);
                            } else if (minX <= lminX && minY >= lminY && maxX <= lmaxX && maxY <= lmaxY) {
                                console.debug("Laster flere områder i vest");
                                // New bounds left+overlapping old
                                //  ..///////
                                //  \\XXX////
                                //  \\XXX////
                                //  ..///////
                                addBounds = [minX, lminY, lminX, lmaxY];
                                vm.startReloadAreas(true, addBounds);
                            } else if (minX >= lminX && minY <= lminY && maxX >= lmaxX && maxY <= lmaxY) {
                                console.debug("Laster flere områder i sør-øst");
                                // New bounds bottom-right corner+overlapping old
                                //  ////////.
                                //  ////////.
                                //  //////XX\
                                //  ......\\\
                                addBounds = [minX, minY, maxX, lmaxY, lmaxX, lminY];
                                vm.startReloadAreas(true, addBounds);
                            } else if (minX <= lminX && minY >= lminY && maxX <= lmaxX && maxY >= lmaxY) {
                                console.debug("Laster flere områder i nord-vest");
                                // New bounds bottom-right corner+overlapping old
                                //  \\\......
                                //  \XX//////
                                //  .////////
                                //  .////////
                                addBounds = [minX, lminY, lmaxX, maxY, lminX, lmaxY];
                                vm.startReloadAreas(true, addBounds);
                            } else if (minX >= lminX && minY >= lminY && maxX >= lmaxX && maxY >= lmaxY) {
                                console.debug("Laster flere områder i nord-øst");
                                // New bounds top-right corner+overlapping old
                                //  ......\\\
                                //  //////XX\
                                //  ////////.
                                //  ////////.
                                addBounds = [lminX, maxY, maxX, lminY, lmaxX, lmaxY];
                                vm.startReloadAreas(true, addBounds);
                            } else if (minX <= lminX && minY <= lminY && maxX <= lmaxX && maxY <= lmaxY) {
                                console.debug("Laster flere områder i sør-vest");
                                // New bounds top-right corner+overlapping old
                                //  .////////
                                //  .////////
                                //  \\XX/////
                                //  \\\\.....
                                addBounds = [minX, lmaxY, lmaxX, minY, lminX, lminY];
                                vm.startReloadAreas(true, addBounds);
                                // todo: sør-vest
                            } else {
                                // reload
                                console.debug("Laster hele utsnitt på nytt :(");
                                console.debug("minX " + (minX <= lminX ? "<=" : ">") + " lminX,\r\n" +
                                    "minY " + (minY <= lminY ? "<=" : "> ") + " lminY,\r\n" +
                                    "maxX " + (maxX <= lmaxX ? "<=" : "> ") + " lmaxX,\r\n" +
                                    "maxY " + (maxY <= lmaxY ? "<=" : "> ") + " lmaxY\r\n");
                                vm.startReloadAreas();
                            }

                        } else {
                            vm.startReloadAreas();
                            //application.filter.ForceRefreshToggle(!application.filter.ForceRefreshToggle());
                        }
                    }
                });

                mapLoadedDfd.resolve();
            };

        vm = {
            compositionComplete: compositionComplete,
            activate: activate,
            title: title,
            text: ko.observable("Map view"),
            visible: ko.observable(true),
            map: undefined,
            baseLayerVisible: ko.observable(false),
            reloadNatureAreasOnActivate: ko.observable(false),  // is set to true if filter changed when not in map view
            isLoading: ko.observable(true).extend({ rateLimit: 100 }),       // Small delay before showing loading-gif
            isLoadingGrid: ko.observable(false),
            drawing: ko.observable(false),
            selectedBaseLayer: ko.observable(),
            loadedBounds: ko.observable(),
            selectionLayer: undefined,
            polygonLayer: undefined,
            features: undefined,
            hasChanged: true,
            updateSelection: function() {
                mapViewPortSet.then(function() {
                    //logger.log(title + " updateSelection", null, title, true);
                    if (!viewportState.zoom()) {
                        viewportState.zoom(vm.map.getView().getZoom());
                    }
                });
            },

            redrawSelectionPolygon: function() {
                var gmWktPolygon = application.filter.Geometry(); 
                if (gmWktPolygon && gmWktPolygon.length > 0) {
                    if (selectionLayerSource.getFeatures() && selectionLayerSource.getFeatures().length > 0) {
                        selectionLayerSource.removeFeature(selectionLayerSource.getFeatures()[0]);
                    }
                    var wkt = new ol.format.WKT();
                    var geometry = wkt.readFeature(gmWktPolygon);
                    var feature = new ol.Feature({
                        geometry: geometry.getGeometry()
                        // { accuracy: "0", siteType: "0", siteName: "Select" });
                    });
                    feature.fid = -1;
                    selectionLayerSource.addFeatures([feature]);
                }
            },

            boundsToWkt: function (bounds) {
                var minX, minY, maxX, maxY, centerX, centerY;
                switch (bounds.length) {
                    case 4:
                        minX = bounds[0], minY = bounds[1], maxX = bounds[2], maxY = bounds[3];
                        return "POLYGON ((" +
                            minX + " " + minY + "," +
                            maxX + " " + minY + "," +
                            maxX + " " + maxY + "," +
                            minX + " " + maxY + "," +
                            minX + " " + minY + "))";
                    case 6:
                        minX = bounds[0], minY = bounds[1], maxX = bounds[2], maxY = bounds[3],
                            centerX = bounds[4], centerY = bounds[5];
                        return "POLYGON ((" +
                            minX + " " + minY + "," +
                            maxX + " " + minY + "," +
                            maxX + " " + maxY + "," +
                            centerX + " " + maxY + "," +
                            centerX + " " + centerY + "," +
                            minX + " " + centerY + "," +
                            minX + " " + minY + "))";
                    default:
                        return "POLYGON EMPTY";
                }
            },
            getBounds: function () {
                if (mapViewPortSet.state() === "resolved") {
                    var vpi = vm.getViewportInfo();
                    return vpi.bounds;
                }
                return "";
            },
            getBoundingBox: function() {
                if (mapViewPortSet.state() === "resolved") {
                    var vpi = vm.getViewportInfo();
                    return vm.boundsToWkt(vpi.bounds);
                }
                return "";
            },
            getViewportInfo: function() {
                if (!vm.map) {
                    return null;
                }
                var prop = vm.map.getView().getProperties();
                if (prop) {
                    var lon = parseFloat(prop.center[0]).toFixed(0),
                        lat = parseFloat(prop.center[1]).toFixed(0),
                        result = {
                            zoom: vm.map.getView().getZoom(),
                            lon: lon,
                            lat: lat,
                            resolution: prop.resolution,
                            rotation: prop.rotation,
                            center: lon + "," + lat,
                            bounds: vm.map.getView().calculateExtent(vm.map.getSize())
                        };
                    return result;
                }
                return null;
            },
            changeView: function() {
                var vpi = vm.getViewportInfo();
                viewportState.zoom(vpi.zoom);
                viewportState.center(vpi.center);
                if (!viewportState.background()) {
                    viewportState.background(application.config.initialBaseMapLayer);
                }
                //viewportState.background(vm.map.getLayers().getArray()[0].name);    // to get current
            },

            showPopup: function (event, value, name) {
                application.grid.GridValue(value);
                application.grid.GridCellName(name);
                var coordinate = event.mapBrowserEvent.coordinate;
                vm.gridPopupOverlay.setPosition(coordinate);
                $('#gridInfoPopup').popover({
                    content: // Retrieve content from the hidden #myGridPopoverContentContainer
                        function() {
                             return $("#gridInfo");
                        },
                    placement: "top",
                    html: true,
                    template: '<div class="popover" style="max-width: 600px;">' +
                        '<button type="button" title="Lukk" class="close" onclick="$(&quot;#myGridPopoverContentContainer&quot;).append($(&quot;#gridInfo&quot;));$(&quot;#gridInfoPopup&quot;).popover(&quot;destroy&quot;);"><span aria-hidden="true">×</span><span class="sr-only">Close</span></button>' +
                        '<div class="arrow"></div>' +
                        '<div class="popover-content" style="width: 600px; height: 320px;">' +
                        '</div>' +
                        '</div>'
                });

                // Check to avoid flickering when the popover is already shown.
                if (!$('#gridInfoPopup[aria-describedby]').length) {
                    $('#gridInfoPopup').popover('show');
                }

                $('#gridInfoPopup').on('hidden.bs.popover', function () {
                    storeGridPopup();
                });
            },

            featureSelected: function (event, feature) {
                var coordinate = event.mapBrowserEvent.coordinate;
                vm.overlay.setPosition(coordinate);

                if (feature && feature.getId()) {
                    dataServices.getNatureAreaByLocalId(feature.getId()).then(function (data) {
                        dataServices.getMetadataByNatureAreaLocalId(feature.getId()).then(function (metadata) {
                            if ($('body').width() > 481) {
                                $('#popup').popover({
                                    content: function () {
                                        // Retrieve content from the hidden #myPopoverContentContainer
                                        return $("#natureAreaInfo");
                                    },
                                    html: true,
                                    template: '<div class="popover" style="max-width: 600px;">' +
                                        '<button type="button" title="Lukk" class="close" onclick="$(&quot;#myPopoverContentContainer&quot;).append($(&quot;#natureAreaInfo&quot;));$(&quot;#popup&quot;).popover(&quot;destroy&quot;);"><span aria-hidden="true">×</span><span class="sr-only">Close</span></button>' +
                                        '<div class="arrow"></div>' +
                                        '<div class="popover-content" style="width: 600px; height: 320px;">' +
                                        '</div>' +
                                        '</div>'
                                });

                                // Check to avoid flickering when the popover is already shown.
                                if (!$('#popup[aria-describedby]').length) {
                                    $('#popup').popover('show');
                                }

                                $('#popup').on('hidden.bs.popover', function () {
                                    // Store popover-content in #myPopoverContentContainer so bindings are not lost!
                                    storePopup();
                                });

                                application.currentFeature(data, metadata);

                                app.trigger("currentFeatureChanged:trigger");
                            } else {
                                // Mobile
                                application.currentFeature(data, metadata);

                                app.trigger("currentFeatureChanged:trigger");
                                nav.navigateTo("details/" + application.viewportState.center() + "/" + application.viewportState.zoom() + "/background/" + application.viewportState.background());
                            }
                        });
                    });
                } else {
                    // Store popover-content in #myPopoverContentContainer so bindings are not lost!
                    storePopup();
                    vm.overlay.setPosition(undefined);
                }
            },

            removePolygon: function () {
                selectionLayerSource.clear();
            },
            activateDrawPolygon: function () {
                vm.map.addInteraction(draw);
                vm.drawing(true);
            },
            deactivateDrawPolygon: function () {
                vm.map.removeInteraction(draw);
                vm.drawing(false);
            },
            selectionPolygonAdded: function (e) {
                while (selectionLayerSource.getFeatures().length > 1) {
                    selectionLayerSource.removeFeature(selectionLayerSource.getFeatures()[0]);
                }

                var wkt = new ol.format.WKT();
                vm.deactivateDrawPolygon();
                app.trigger('drawPolygon:deactivate');

                var wktGeometry = wkt.writeGeometry(selectionLayerSource.getFeatures()[0].getGeometry());

                application.filter.Geometry(wktGeometry);
                app.trigger('currentSelection:add');
                // todo: do not call fit if activated by bookmark! 
                vm.fitSelectionPolygon();

            },
            fitSelectionPolygon: function () {
                vm.map.getView().fit(
                    selectionLayerSource.getFeatures()[0].getGeometry(),
                    vm.map.getSize(),
                    {
                        padding: [100, 150, 100, 150], //  top, right, bottom and left padding
                        nearest: true
                    });
            }

        };
        // These subscriptions is done in construction time to enable correct handling of early events (e.g. reload application with url filter)


        app.on('listview:activate').then(function () {
            storePopup();
        });
        app.on('factsheet:activate').then(function () {
            storePopup();
        });

        application.filter.Geometry.subscribe(function (polygon) {
            logger.log("polygon changed", null, title);
            if (!polygon) {
                vm.removePolygon();
            } else {
                vm.redrawSelectionPolygon();
            }

        });

        application.viewportStateChanged.subscribe(function (value) {
            mapLoaded.then(function () {
                var vpi = vm.getViewportInfo();
                if (!vpi || value.zoom() !== vpi.zoom || value.center() !== vpi.center) {
                    var lonlat = value.center().split(","),
                        lon = parseFloat(lonlat[0]),
                        lat = parseFloat(lonlat[1]),
                        centerPoint = [lon, lat];

                    vm.map.getView().setCenter(centerPoint);
                    vm.map.getView().setZoom(value.zoom());
                    vm.selectedBaseLayer(value.background());
                    mapViewPortSetDfd.resolve();
                }
               // vm.toggleOverlayLayer("Layer 1");
            });
            if (mapViewPortSet.state() === "resolved") {
                var vpi = vm.getViewportInfo();

                // Add bounding box to filter if zoom > a limit
                // todo: maybe increase bounding box a bit beyond the screen and check if the screen area is still inside what has been fetched before
                // todo: before changing the filter and triggering a reload. 
                if (vpi.zoom > application.config.useBoundingBoxLimit) {
                    application.filter.BoundingBox(vm.boundsToWkt(vpi.bounds));
                } else {
                    application.filter.BoundingBox("");
                }
                // Show centerpoints instead of polygons if zoom < a limit
                vm.polygonLayer.setVisible(vpi.zoom >= application.config.loadCenterPointLimit);
                vm.polygonClusterLayer.setVisible(vpi.zoom < application.config.loadCenterPointLimit);
                application.filter.CenterPoints(vpi.zoom < application.config.loadCenterPointLimit);
                //application.listAvailable(vpi.zoom >= application.config.loadCenterPointLimit);
                vm.updateSelection();
            }
        });
        return vm;
    });

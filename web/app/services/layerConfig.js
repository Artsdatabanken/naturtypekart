define(['knockout', 'services/config', 'services/application'],
    function (ko, config, application) {
        "use strict";

        // Attributions
        var kartverketBackgroundAttribution = new ol.Attribution({
            html: 'Europa-bakgrunnskart fra: <a href="//www.kartverket.no/kart">Kartverket</a>, '
        });
        var kartverketAttribution = new ol.Attribution({
            html: 'Kilde: <a href="//www.kartverket.no/kart">Kartverket</a>'
        });
        var admGrenserAttribution = new ol.Attribution({
            html: 'Administrative grenser: <a href="//kartverket.no/Geonorge/">GeoNorge</a>'
        });
        var miljodirAttribution = new ol.Attribution({
            html: '<a href="//www.miljodirektoratet.no/">Miljødirektoratet</a>'
        });

        // WMTS-layer start
        var sProjection = 'EPSG:32633';
        var sProjectionSvalbard = 'EPSG:25833';
        var speciesColorScale = undefined;
        var precisionColorScale = undefined;
        proj4.defs(sProjection, "+proj=utm +zone=33 +ellps=WGS84 +datum=WGS84 +units=m +no_defs");
        //proj4.defs("EPSG:25833", "+proj=utm +zone=33 +ellps=GRS80 +units=m +no_defs");
        //proj4.defs("EPSG:25833", "+proj=utm +zone=33 +ellps=GRS80 +towgs84=0,0,0,0,0,0,0 +units=m +no_defs");

        var projection = new ol.proj.Projection({
            code: sProjection,
            //extent: [-2000000, 3500000, 3545984, 9045984],
            extent: [-2500000, 3500000, 3045984, 9045984],
            units: "m"
        });


        // justert på øyemål, må muligens finjusteres enda mer..
        var svalbardXMin = -5118180;    // Finjuster x-posisjon her
        var svalbardYMin = 7224500;     // Finjuster y-posisjon her
        var svalbardWidth = 5545984;    // La denne være
        var svalbardHeight = 2772992;   // La denne være

        var projectionSvalbard = new ol.proj.Projection({
                code: sProjectionSvalbard,
                //extent: [-5120900, 7225108, 425084, 9998100],
                //       xmin,      ymin,   xmax,   ymax   // meter
                //extent: [-5118055, 7224258, 427929, 9997250], // justert på øyemål, må muligens finjusteres enda mer..
                extent: [svalbardXMin, svalbardYMin, svalbardXMin + svalbardWidth, svalbardYMin + svalbardHeight],
                units: "m"
            }),
            projectionExtent = projection.getExtent(),
            projectionExtentSvalbard = projectionSvalbard.getExtent(),
            size = ol.extent.getWidth(projectionExtent) / 256,
            sizeSvalbard = ol.extent.getWidth(projectionExtentSvalbard) / 256,
            numZoomLevels = 18,
            resolutions = new Array(numZoomLevels),
            matrixIds = new Array(numZoomLevels),
            nibmatrixIds = new Array(numZoomLevels),
            svalbardResolutions = new Array(numZoomLevels),
            matrixSet = sProjection,
            matrixSetSvalbard = sProjectionSvalbard;

        for (var z = 0; z < numZoomLevels; ++z) {
            resolutions[z] = size / Math.pow(2, z);
            svalbardResolutions[z] = sizeSvalbard / Math.pow(2, z);
            matrixIds[z] = matrixSet + ":" + z;
            nibmatrixIds[z] = z;
        }

        var wmtsTileGrid = new ol.tilegrid.WMTS({
                origin: ol.extent.getTopLeft(projectionExtent),
                resolutions: resolutions,
                matrixIds: matrixIds
            }),
            wmtsTileGridSvalbard = new ol.tilegrid.WMTS({
                origin: ol.extent.getTopLeft(projectionExtentSvalbard),
                resolutions: svalbardResolutions,
                matrixIds: nibmatrixIds
            }),
            wmtsTileGridNib = new ol.tilegrid.WMTS({
                origin: ol.extent.getTopLeft(projectionExtent),
                resolutions: resolutions,
                matrixIds: nibmatrixIds
            });

        ol.proj.addProjection(projection);


        //'http://opencache.statkart.no/gatekeeper/gk/gk.open_wmts?request=GetCapabilities&service=WMTS
        //'http://localhost:49912/proxy?http://opencache.statkart.no/gatekeeper/gk/gk.open_wmts?',

        //var baatUrl = config.proxyurl + '//opencache.statkart.no/gatekeeper/gk/gk.open_wmts?',
        //var baatUrl = config.proxyurl + '//gatekeeper{1-3}.geonorge.no/BaatGatekeeper/gk/gk.cache_wmts?';
        var baatUrl = '//gatekeeper{1-3}.geonorge.no/BaatGatekeeper/gk/gk.cache_wmts?';
        //if (config.dev) baatUrl = config.proxyurl + '//opencache.statkart.no/gatekeeper/gk/gk.open_wmts?';
        if (config.dev) baatUrl = '//opencache.statkart.no/gatekeeper/gk/gk.open_wmts?';

        var baatTileLoader = function (imageTile, src) {
            imageTile.getImage().src = src + "&gkt=" + application.ndToken();
        };

        var greyMap = new ol.layer.Tile({
            opacity: 1,
            extent: [-2500000, 3500000, 3045984, 9045984],
            source: new ol.source.WMTS({
                url: baatUrl,
                layer: 'topo2graatone',
                attributions: [kartverketAttribution],
                matrixSet: matrixSet,
                format: 'image/png',
                projection: projection,
                tileGrid: wmtsTileGrid,
                tileLoadFunction: baatTileLoader,
                style: 'default',
                wrapX: true,
                crossOrigin: 'anonymous'
            })
        });
        greyMap.name = "GreyMap";
        greyMap.description = "Gråtoner";

        var topo2 = new ol.layer.Tile({
            opacity: 1,
            extent: [-2500000, 3500000, 3045984, 9045984],
            source: new ol.source.WMTS({
                url: baatUrl,
                layer: 'topo2',
                attributions: [kartverketAttribution],
                matrixSet: matrixSet,
                format: 'image/png',
                projection: projection,
                tileGrid: wmtsTileGrid,
                tileLoadFunction: baatTileLoader,
                style: 'default',
                wrapX: true,
                crossOrigin: 'anonymous'
            })
        });
        topo2.name = "topo2";
        topo2.description = "Topografisk";

        var sjokart = new ol.layer.Tile({
            opacity: 1,
            extent: [-2500000, 3500000, 3045984, 9045984],
            source: new ol.source.WMTS({
                url: baatUrl,
                layer: 'sjokartraster',
                attributions: [kartverketAttribution],
                matrixSet: matrixSet,
                format: 'image/png',
                projection: projection,
                tileGrid: wmtsTileGrid,
                tileLoadFunction: baatTileLoader,
                style: 'default',
                wrapX: true,
                crossOrigin: 'anonymous'
            })
        });
        sjokart.name = "sjokart";
        sjokart.description = "Sjøkart";

        var europa = new ol.layer.Tile({
            opacity: 1,
            extent: [-2500000, 3500000, 3045984, 9045984],
            source: new ol.source.WMTS({
                url: baatUrl,
                layer: 'egk',
                attributions: [kartverketBackgroundAttribution],
                matrixSet: matrixSet,
                format: 'image/png',
                projection: projection,
                tileGrid: wmtsTileGrid,
                tileLoadFunction: baatTileLoader,
                style: 'default',
                wrapX: true,
                crossOrigin: 'anonymous'
            }),
            //minResolution: 200,
            minResolution: 100
        });
        europa.name = "europa";
        europa.description = "Europakart";

        // var greyMap2 = new ol.layer.Tile({
        //     opacity: 1,
        //     source: new ol.source.WMTS({
        //         url: baatUrl,
        //         layer: 'norges_grunnkart_graatone',
        //         attributions: [kartverketAttribution],
        //         matrixSet: matrixSet,
        //         format: 'image/png',
        //         projection: projection,
        //         tileGrid: wmtsTileGrid,
        //         tileLoadFunction: baatTileLoader,
        //         style: 'default',
        //         wrapX: true
        //     })
        // });
        // greyMap2.name = "GreyMap2";
        // greyMap2.description = "Norges grunnkart gråtone";
        //
        // var grunnkart = new ol.layer.Tile({
        //     opacity: 1,
        //     source: new ol.source.WMTS({
        //         url: baatUrl,
        //         layer: 'norges_grunnkart',
        //         attributions: [kartverketAttribution],
        //         matrixSet: matrixSet,
        //         format: 'image/png',
        //         projection: projection,
        //         tileGrid: wmtsTileGrid,
        //         tileLoadFunction: baatTileLoader,
        //         style: 'default',
        //         wrapX: true
        //     })
        // });
        // grunnkart.name = "grunnkart";
        // grunnkart.description = "Norges grunnkart";

        var terreng = new ol.layer.Tile({
            opacity: 1,
            extent: [-2500000, 3500000, 3045984, 9045984],
            source: new ol.source.WMTS({
                url: baatUrl,
                layer: 'terreng_norgeskart',
                attributions: [kartverketAttribution],
                matrixSet: matrixSet,
                format: 'image/png',
                projection: projection,
                tileGrid: wmtsTileGrid,
                tileLoadFunction: baatTileLoader,
                style: 'default',
                wrapX: true,
                crossOrigin: 'anonymous'
            })
        });
        terreng.name = "terreng";
        terreng.description = "Terreng";

        var filtersaken = "";

        var replaceUrlParameter = function (url, parameter, value) {
            var val = '';
            var tmp = '';
            var pos = url.indexOf('?' + parameter + '=');
            if (pos < 0) {
                pos = url.indexOf('&' + parameter + '=');
            }
            if (pos >= 0) {
                val = url.substr(pos);
                url = url.substr(0, pos + parameter.length + 2);
                val = val.substr(parameter.length + 2);
                tmp = val;
                pos = val.indexOf('&');
                if (pos >= 0) {
                    val = val.substr(0, pos);
                }
                tmp = value + tmp.substr(pos);
            }
            return url + tmp;
        };
        var nibTileLoader = function (imageTile, src) {
            src = replaceUrlParameter(src.toLowerCase(), 'tilematrixset', 'default028mm');
            src = replaceUrlParameter(src.toLowerCase(), 'gkt', application.ndToken());
            imageTile.getImage().src = src;

        };


        var nibUrl = '//gatekeeper{1-3}.geonorge.no/BaatGatekeeper/gk/gk.nib_utm33_wmts_v2?GKT=';
        var nibWmts =new ol.layer.Tile({
            opacity: 1,
            extent: [-2500000, 3500000, 3045984, 9045984],
            source: new ol.source.WMTS({
                url: nibUrl,
                layer: 'Nibcache_UTM33_EUREF89_v2',
                attributions: [kartverketAttribution],
                matrixSet: matrixSet,
                format: 'image/png',
                //projection: projection,
                tileGrid: wmtsTileGridNib,
                tileLoadFunction: nibTileLoader,
                style: 'default',
                wrapX: true,
                crossOrigin: null
            })

        });
        nibWmts.name = "nibwmts";
        nibWmts.description = "Flyfoto";
        // WMTS-layer end

        var svalbardTileLoader = function (imageTile, src) {
            src = replaceUrlParameter(src, 'tilematrixset', 'default028mm');
            imageTile.getImage().src = src;

        };
        // http://geodata.npolar.no/arcgis/rest/services/Basisdata/NP_Basiskart_Svalbard_WMTS_25833/MapServer/WMTS?request=getcapabilities
        var svalbardUrl = "//geodata.npolar.no/arcgis/rest/services/Basisdata/NP_Basiskart_Svalbard_WMTS_25833/MapServer/WMTS?";
        var svalbard = new ol.layer.Tile({
            opacity: 1,
            extent: [368000, 8240000, 875000, 9045984],
            source: new ol.source.WMTS({
                url: svalbardUrl,
                layer: 'Basisdata_NP_Basiskart_Svalbard_WMTS_25833',
                attributions: [kartverketAttribution],
                matrixSet: matrixSetSvalbard,
                format: 'image/jpgpng',
                projection: 'EPSG:25833', // ? må kanskje definere selv? Se også https://openlayers.org/en/latest/doc/tutorials/raster-reprojection.html
                tileGrid: wmtsTileGridSvalbard,
                tileLoadFunction: svalbardTileLoader,
                style: 'default',
                wrapX: true,
                crossOrigin: 'anonymous'
                //crossOrigin: null
            })
        });
        svalbard.name = "svalbard";
        svalbard.description = "Svalbard";
        function getToken() {
            return application.ndToken();
        }

        //// http://arcgisproxy.miljodirektoratet.no/arcgis/services/naturtyper_nin/MapServer/WMSServer?request=GetCapabilities&service=WMS
        //var wmsNin = new ol.layer.Tile({
        //    extent: [-20037508.34, -20037508.34, 20037508.34, 20037508.34],
        //    source: new ol.source.TileWMS(({
        //        url: "http://arcgisproxy.miljodirektoratet.no/arcgis/services/naturtyper_nin/MapServer/WMSServer?",
        //        params: {
        //            LAYERS: "NiN_natursystem",
        //            VERSION: "1.3.0",
        //            FORMAT: "image/png",
        //            SRS: "EPSG:32633"
        //        }//,
        //        //crossOrigin: 'anonymous'
        //    }))
        //});
        //wmsNin.name = "NiN";
        //wmsNin.description = "Naturkartlegginger";

        // http://arcgisproxy.dirnat.no/arcgis/services/nin_landskapstyper/nin_landskapstyper/MapServer/WmsServer?request=GetCapabilities&service=WMS
        //var wmsNinLandskap = new ol.layer.Tile({
        //    extent: [-20037508.34, -20037508.34, 20037508.34, 20037508.34],
        //    source: new ol.source.TileWMS(({
        //        //url: config.proxyurl + "http://arcgisproxy.dirnat.no/arcgis/services/nin_landskapstyper/nin_landskapstyper/MapServer/WmsServer? ",
        //        url: "http://arcgisproxy.dirnat.no/arcgis/services/nin_landskapstyper/nin_landskapstyper/MapServer/WmsServer? ",
        //        params: {
        //            LAYERS: "NIN_LandskapsArealEnhet",
        //            VERSION: "1.3.0",
        //            FORMAT: "image/png",
        //            SRS: "EPSG:32633"
        //        }//,
        //        //crossOrigin: 'anonymous'
        //    }))
        //});
        //wmsNinLandskap.name = "NiNLandskap";
        //wmsNinLandskap.description = "Landskapstyper";

        //http://wms.geonorge.no/skwms1/wms.abas?request=GetCapabilities&service=WMS&version=1.3.0
        var wmsAdmGrenser = new ol.layer.Tile({
            extent: [-20037508.34, -20037508.34, 20037508.34, 20037508.34],
            source: new ol.source.TileWMS({
                attributions: [admGrenserAttribution],
                //url: config.proxyurl + "http://wms.geonorge.no/skwms1/wms.abas?",
                url: "//wms.geonorge.no/skwms1/wms.abas?",
                params: {
                    LAYERS: "Kommunegrenser,Fylkesgrenser",
                    VERSION: "1.3.0",
                    FORMAT: "image/png",
                    SRS: "EPSG:32633"
                }
            })
        });
        wmsAdmGrenser.name = "AdmGrenser";
        wmsAdmGrenser.description = "Administrative grenser";

        //http://wms.miljodirektoratet.no/arcgis/services/vern/mapserver/WMSServer?request=GetCapabilities&service=WMS&version=1.3.0
        var wmsVern = new ol.layer.Tile({
            opacity:0.5,
            extent: [-20037508.34, -20037508.34, 20037508.34, 20037508.34],
            source: new ol.source.TileWMS({
                attributions: [miljodirAttribution],
                //url: config.proxyurl + "http://wms.miljodirektoratet.no/arcgis/services/vern/mapserver/WMSServer?",
                url: "//wms.miljodirektoratet.no/arcgis/services/vern/mapserver/WMSServer?",
                params: {
                    LAYERS: "foreslatt_naturvern_omrade,naturvern_klasser_omrade,naturvern_punkt",
                    VERSION: "1.3.0",
                    FORMAT: "image/png",
                    SRS: "EPSG:32633"
                },
                crossOrigin: 'anonymous'
            })
        });
        wmsVern.name = "Vern";
        wmsVern.description = "Verneområder";

        // WMS-layers end

        // WFS layers, stedsnavn:
        //http://wfs.geonorge.no/skwms1/wfs.stedsnavn50?request=GetCapabilities&service=WFS


        // var openCycleMapLayer = new ol.layer.Tile({
        //     source: new ol.source.OSM({
        //         attributions: [
        //             new ol.Attribution({
        //                 html: 'All maps &copy; ' +
        //                 '<a href="http://www.opencyclemap.org/">OpenCycleMap</a>'
        //             }),
        //             ol.source.OSM.ATTRIBUTION
        //         ],
        //         url: '//{a-c}.tile.opencyclemap.org/cycle/{z}/{x}/{y}.png'
        //     })
        // });
        // openCycleMapLayer.name = "OpenCycleMap";
        // openCycleMapLayer.description = "Open Cycle Map";

        var openStreetMapLayer = new ol.layer.Tile({
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
        openStreetMapLayer.name = "OpenStreetMap";
        openStreetMapLayer.description = "Open Street Map";

        ///---------------------
        function tileUrlFunction(tileCoord) {
            return '/ninmap/nin/{z}/{x}/{y}.geojson'
                .replace('{z}', String(tileCoord[0]))
                .replace('{x}', String(tileCoord[1]))
                .replace('{y}', String(tileCoord[2]));
        }

        function tileUrlFunctionHeat(tileCoord) {
            return '/ninmap/heat/{z}/{x}/{y}.png'
                .replace('{z}', String(tileCoord[0]))
                .replace('{x}', String(tileCoord[1]))
                .replace('{y}', String(tileCoord[2]));
        }

        var landuseStyleCache = {};
        resolutions = [];
        //        for (var z = 1; z <= 22; ++z) {
        for (z = 0; z <= 17; ++z)
            resolutions.push(156543.03392804097 / Math.pow(2, z));

        var layer1 =
            new ol.layer.VectorTile({
                projection: "EPSG:32633",
                renderMode: 'vector',
                //                preload: Infinity,
                source: new ol.source.VectorTile({
                    format: new ol.format.GeoJSON({
                        defaultDataProjection: "EPSG:32633"
                    }),
                    tileGrid: ol.tilegrid.createXYZ({minZoom: 3, maxZoom: 13}),
                    tileGrid2: new ol.tilegrid.TileGrid({
                        extent: projection.getExtent(),
                        resolutions: resolutions
                    }),
                    tileUrlFunction: tileUrlFunction,
                    crossOrigin: 'anonymous'
                }),
                visible: true,
                style: function (feature) {
                    var styleKey = feature.get('kind');
                    var style = landuseStyleCache[styleKey];
                    if (!style) {
                        var color, width;
                        color = {
                            'envelope': '#eeeeee',
                            'kommune': '#ddd',
                            'fylke': '#aaa',
                            'NR': '#DA10E7',
                            'LVO': '#76C759',
                            'LVOP': '#D58F8D',
                            'LVOD': '#3E7D28',
                            'NP': '#007E00',
                            'D': '#ff8f00',
                            'NA I1-C-1': '#404040',
                            'NA I1-C-2': '#404040',
                            'NA V1-C-1': '#404040',
                            'NA T45-E-1': '#f04040',
                            'NA T45-E-2': '#404040',
                            'NA T44': '#404040',
                            'NA V3-C-2': '#404040',
                            'NA L2-2': '#404040',
                            'T44': '#404040'
                        }[styleKey];
                        width = {
                            'envelope': 0.1,
                            'kommune': 2,
                            'fylke': 3
                        }[styleKey];
                        if (!width) width = 1;
                        var fillColor = {
                            'NR': 'rgba(155,5,5,0.2)',
                            'LVO': 'rgba(5,195,155,0.2)',
                            'LVOP': 'rgba(255,195,155,0.2)',
                            'LVOD': 'rgba(155,255,155,0.2)',
                            'NP': 'rgba(15,255,15,0.2)',
                            'D': 'rgba(255,255,0,0.2)',
                            'NA I1-C-1': 'rgba(20,100,0,0.2)',
                            'NA I1-C-2': 'rgba(20,100,100,0.2)',
                            'NA V1-C-1': 'rgba(120,00,0,0.2)',
                            'NA T45-E-1': 'rgba(190,100,120,0.2)',
                            'NA T45-E-2': 'rgba(120,100,120,0.2)',
                            'NA T44': 'rgba(20,100,180,0.2)',
                            'NA V3-C-2': 'rgba(20,20,220,0.2)',
                            'NA V4-C-2': 'rgba(120,20,220,0.2)',
                            'NA T4-C-2': 'rgba(20,20,220,0.2)',
                            'NA L2-2': 'rgba(20,220,220,0.2)'
                        }[styleKey];
                        var lineDash = {
                            'envelope': [7],
                            'kommune': [5],
                            'fylke': [14]
                        }[styleKey];
                        if (!fillColor)
                            console.log("____" + styleKey);
                        style = new ol.style.Style({
                            stroke: new ol.style.Stroke({
                                color: color,
                                lineDash: !lineDash ? undefined : lineDash,
                                width: width
                            }),
                            fill: !fillColor ? undefined : new ol.style.Fill({color: fillColor})
                        });
                        landuseStyleCache[styleKey] = style;
                    }
                    return style;
                }
            });
        layer1.name = "Layer1";
        layer1.description = "Layer1";
        ///---------------------

        var tmsHead = new ol.layer.Tile({
            preload: 1,
            source: new ol.source.TileImage({
                projection: "EPSG:32633",
                tileGrid: ol.tilegrid.createXYZ({minZoom: 0, maxZoom: 13}),
                tileGrid2: new ol.tilegrid.TileGrid({
                    origin: ol.extent.getTopLeft(projectionExtent),
                    extent: projection.getExtent(),
                    resolutions: resolutions
                }),
                tileUrlFunction: tileUrlFunctionHeat,
                crossOrigin: 'anonymous'
            })
        });
        tmsHead.name = "Heat test";
        tmsHead.description = "Heat test";

        var group = new ol.layer.Group({
            layers: [openStreetMapLayer, tmsHead]
        });
        group.name = "Heat test";

        var bingMapNames = [
            'Road',
            //'Aerial',
            'AerialWithLabels'
        ];

        var bingDescriptions = [
            'Bing Veikart',
            //'Bing Flyfoto',
            'Bing Hybrid'
        ];


        greyMap.needsToken = true;
        topo2.needsToken = true;
        sjokart.needsToken = true;
        europa.needsToken = true;
        // greyMap2.needsToken = true;
        // grunnkart.needsToken = true;
        terreng.needsToken = true;

        var baseLayerPool = ko.observableArray([]);
        var overlayLayerPool = ko.observableArray([]);
        var observationStyleMode = ko.observable("category");
        baseLayerPool.push(nibWmts, openStreetMapLayer, greyMap, terreng, sjokart, europa, topo2, svalbard);
        overlayLayerPool.push(/*wmsNin, wmsNinLandskap,*/ wmsAdmGrenser, wmsVern);

        var i, ii;
        for (i = 0, ii = bingMapNames.length; i < ii; ++i) {
            var layer =
                new ol.layer.Tile({
                    preload: Infinity,
                    source: new ol.source.BingMaps({
                        key: config.bk,
                        imagerySet: bingMapNames[i],
                        crossOrigin: 'anonymous'
                        // use maxZoom 19 to see stretched tiles instead of the BingMaps
                        // "no photos at this zoom level" tiles
                        // maxZoom: 19
                    })
                });
            layer.name = bingMapNames[i];
            layer.description = bingDescriptions[i];
            baseLayerPool.push(layer);
        }

        var getBaseLayerFromPool = function (layerName) {
            var i, layerInPool;
            for (i = 0; i < baseLayerPool().length; i++) {
                layerInPool = baseLayerPool()[i];
                if (layerInPool.name === layerName) {
                    return layerInPool;
                }
            }
            return null;
        };
        var getOverlayLayerFromPool = function (layerName) {
            var i, layerInPool;
            for (i = 0; i < overlayLayerPool().length; i++) {
                layerInPool = overlayLayerPool()[i];
                if (layerInPool.name === layerName) {
                    return layerInPool;
                }
            }
            return null;
        };

        function createSymbolRenderer(colors, size) {
            var
                // Adjust for rounding errors
                pad = 0.1,
                width = size * 2,
                height = size * 2;

            var canvas = document.createElement('canvas');
            canvas.width = width + pad;
            canvas.height = height;
            var ctx = canvas.getContext('2d');
            ctx.beginPath();
            ctx.moveTo(0, height);
            ctx.quadraticCurveTo(size, 0, width, height);
            ctx.lineWidth = 1;

            // line color
            ctx.strokeStyle = colors[0];
            ctx.stroke();

            return ctx.createPattern(canvas, 'repeat');
        }

        function createSquareRenderer(colors, squareWidth) {
            var
                // Adjust for rounding errors
                pad = 0.1,
                width = squareWidth * 2,
                height = squareWidth * 2;

            var canvas = document.createElement('canvas');
            canvas.width = width + pad;
            canvas.height = height;
            var ctx = canvas.getContext('2d');
            ctx.fillStyle = colors[0];
            ctx.fillRect(0, 0, width, height);
            ctx.fillStyle = colors[1];
            ctx.fillRect(squareWidth, 0, squareWidth, squareWidth);
            ctx.fillRect(0, squareWidth, squareWidth, squareWidth);

            return ctx.createPattern(canvas, 'repeat');
        }

        function createStripesRenderer(colors, strokeWidth, angle) {
            var
                // Adjust for rounding errors
                pad = 0.1,
                radian = angle * Math.PI / 180,
                width = strokeWidth * colors.length,
                height = strokeWidth * colors.length,
                widthR = width / Math.cos(radian),
                heightR = height / Math.sin(radian),
                i;

            var canvas = document.createElement('canvas');
            canvas.width = widthR + pad;
            canvas.height = heightR;
            var ctx = canvas.getContext('2d');
            ctx.lineWidth = strokeWidth;

            ctx.translate((widthR - width) / 2, 0);
            ctx.rotate(radian);
            for (i = -colors.length; i < colors.length * 2; i++) {
                ctx.strokeStyle = colors[(i + colors.length) % colors.length];
                ctx.beginPath();
                ctx.moveTo(strokeWidth * i + strokeWidth * 0.5, -heightR);
                ctx.lineTo(strokeWidth * i + strokeWidth * 0.5, heightR);
                ctx.stroke();
            }
            return ctx.createPattern(canvas, 'repeat');
        }

        var natureLevelStyleCache = {};
        var natureLevelStyleFunc = function (feature, resolution) {
            var val = feature.getProperties().ColorCode || 'UNKN';
            val = val.replace(' ', '_').substr(0, 4);

            var cartographyColor = config.cartographyColors[val];
            var style = natureLevelStyleCache[val]; // get from cache if it has already been created
            if (!style) {
                style = [
                    new ol.style.Style({
                        stroke: new ol.style.Stroke({
                            color: cartographyColor.strokeColor ? cartographyColor.strokeColor : '#00F',
                            width: cartographyColor.strokeWidth ? cartographyColor.strokeWidth : 0.4,
                            lineDash: cartographyColor.strokeStyle === "dash" ? cartographyColor.strokePattern : undefined
                        }),
                        fill: new ol.style.Fill({
                            color: cartographyColor.style === "solid" ? cartographyColor.colors[0] : (
                                cartographyColor.style === "stripes" ? ol.colorlike.asColorLike(createStripesRenderer(cartographyColor.colors, 3, 12)) : (
                                    cartographyColor.style === "squares" ? ol.colorlike.asColorLike(createSquareRenderer(cartographyColor.colors, 5)) : (
                                        cartographyColor.style === "symbol" ? ol.colorlike.asColorLike(createSymbolRenderer(cartographyColor.colors, 10)) :
                                            cartographyColor.colors[0])))
                        })
                    })
                ];
                natureLevelStyleCache[val] = style;
            }
            return style;
        };

        var vm = {
            baseLayerPool: baseLayerPool(),
            overlayLayerPool: overlayLayerPool(),
            getBaseLayerFromPool: getBaseLayerFromPool,
            getOverlayLayerFromPool: getOverlayLayerFromPool,
            natureLevelStyleFunc: natureLevelStyleFunc
        };

        return vm;
    });

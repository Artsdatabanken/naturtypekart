define(['knockout', 'services/config', 'services/application'],
    function (ko, config, application) {
        "use strict";

        // Attributions
        var kartverketAttribution = new ol.Attribution({
            html: 'Kilde: <a href="http://www.kartverket.no/kart">Kartverket</a>'
        });
        var admGrenserAttribution = new ol.Attribution({
            html: 'Administrative grenser: <a href="http://kartverket.no/Geonorge/">GeoNorge</a>'
        });
        var miljodirAttribution = new ol.Attribution({
            html: '<a href="http://www.miljodirektoratet.no/">Miljødirektoratet</a>'
        });

        // WMTS-layer start
        var sProjection = 'EPSG:32633';
		proj4.defs(sProjection, "+proj=utm +zone=33 +ellps=WGS84 +datum=WGS84 +units=m +no_defs");
        var projection = new ol.proj.Projection({
                code: sProjection,
//              extent: [-2000000, 3500000, 3545984, 9045984], // UTM32
                extent: [-2500000, 3500000, 3045984, 9045984] // UTM33
//                extent: [-999978, 1499999, 5499996,9500000] // SSB?
                //units: "m"
            }),
            projectionExtent = projection.getExtent(),
            size = ol.extent.getWidth(projectionExtent) / 256,
            numZoomLevels = 18,
            resolutions = new Array(numZoomLevels),
            matrixIds = new Array(numZoomLevels),
            matrixSet = sProjection,
			
            wmtsTileGrid = new ol.tilegrid.WMTS({
                origin: ol.extent.getTopLeft(projectionExtent),
                resolutions: resolutions,
                matrixIds: matrixIds
            });

        ol.proj.addProjection(projection);

        for (var z = 0; z < numZoomLevels; ++z) {
            resolutions[z] = size / Math.pow(2, z);
            matrixIds[z] = matrixSet + ":" + z;
        }
        var baatUrl = 'http://gatekeeper{1-3}.geonorge.no/BaatGatekeeper/gk/gk.cache_wmts?';

        var baatTileLoader = function (imageTile, src) {
            imageTile.getImage().src = src + "&gkt=" + application.ndToken();
        };
        var greyMap = new ol.layer.Tile({
            opacity: 1,
            source: new ol.source.WMTS({
                url: baatUrl,
                //layer: 'norges_grunnkart_graatone', //''topo2graatone',
                layer: 'topo2graatone',
                attributions: [kartverketAttribution],
                matrixSet: matrixSet,
                format: 'image/png',
                projection: projection,
                tileGrid: wmtsTileGrid,
                tileLoadFunction: baatTileLoader,
                style: 'default',
                wrapX: true
            })
        });
        greyMap.name = "GreyMap";
        greyMap.description = "Topografisk gråtonekart";

        var greyMap2 = new ol.layer.Tile({
            opacity: 1,
            source: new ol.source.WMTS({
                url: baatUrl,
                layer: 'norges_grunnkart_graatone',
                attributions: [kartverketAttribution],
                matrixSet: matrixSet,
                format: 'image/png',
                projection: projection,
                tileGrid: wmtsTileGrid,
                tileLoadFunction: baatTileLoader,
                style: 'default',
                wrapX: true
            })
        });
        greyMap2.name = "GreyMap2";
        greyMap2.description = "Norges grunnkart gråtone";

        var grunnkart = new ol.layer.Tile({
            opacity: 1,
            source: new ol.source.WMTS({
                url: baatUrl,
                layer: 'norges_grunnkart',
                attributions: [kartverketAttribution],
                matrixSet: matrixSet,
                format: 'image/png',
                projection: projection,
                tileGrid: wmtsTileGrid,
                tileLoadFunction: baatTileLoader,
                style: 'default',
                wrapX: true
            })
        });
        grunnkart.name = "grunnkart";
        grunnkart.description = "Norges grunnkart";

        var terreng = new ol.layer.Tile({
            opacity: 1,
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
                wrapX: true
            })
        });
        terreng.name = "terreng";
        terreng.description = "Terreng Norgeskart";


        // WMTS-layer end

        // WMS-layers start
        var nibUrl = 'http://gatekeeper{1-3}.geonorge.no/BaatGatekeeper/gk/gk.nib_utm33?';
        var wmsNib = new ol.layer.Tile({
            extent: [-2500000, 3500000, 3045984, 9045984],
            source: new ol.source.TileWMS(({
                url: nibUrl,
                tileLoadFunction: baatTileLoader,
                params: {
                    LAYERS: "Nibcache_UTM33_EUREF89",
                    VERSION: "1.1.1",
                    FORMAT: "image/jpeg",
                    SRS: "EPSG:32633"
                }
            }))
        });
        wmsNib.name = "NiB";
        wmsNib.description = "Norge i Bilder";

        function getToken() {
            return application.ndToken();
        }

        // http://arcgisproxy.miljodirektoratet.no/arcgis/services/naturtyper_nin/MapServer/WMSServer?request=GetCapabilities&service=WMS
        var wmsNin = new ol.layer.Tile({
            extent: [-20037508.34, -20037508.34, 20037508.34, 20037508.34],
            source: new ol.source.TileWMS(({
                url: "http://arcgisproxy.miljodirektoratet.no/arcgis/services/naturtyper_nin/MapServer/WMSServer?",
                params: {
                    LAYERS: "NiN_natursystem",
                    VERSION: "1.3.0",
                    FORMAT: "image/png",
                    SRS: "EPSG:32633"
                }
            }))
        });
        wmsNin.name = "NiN";
        wmsNin.description = "Naturkartlegginger WMS";

        // http://arcgisproxy.dirnat.no/arcgis/services/nin_landskapstyper/nin_landskapstyper/MapServer/WmsServer?request=GetCapabilities&service=WMS
        var wmsNinLandskap = new ol.layer.Tile({
            extent: [-20037508.34, -20037508.34, 20037508.34, 20037508.34],
            source: new ol.source.TileWMS(({
                url: config.proxyurl + "http://arcgisproxy.dirnat.no/arcgis/services/nin_landskapstyper/nin_landskapstyper/MapServer/WmsServer? ",
                params: {
                    LAYERS: "NIN_LandskapsArealEnhet",
                    VERSION: "1.3.0",
                    FORMAT: "image/png",
                    SRS: "EPSG:32633"
                }
            }))
        });
        wmsNinLandskap.name = "NiNLandskap";
        wmsNinLandskap.description = "Landskapstyper WMS";

        //http://wms.geonorge.no/skwms1/wms.abas?request=GetCapabilities&service=WMS&version=1.3.0
        var wmsAdmGrenser = new ol.layer.Tile({
            extent: [-20037508.34, -20037508.34, 20037508.34, 20037508.34],
            source: new ol.source.TileWMS(({
                attributions: [admGrenserAttribution],
                url: config.proxyurl + "http://wms.geonorge.no/skwms1/wms.abas?",
                params: {
                    LAYERS: "Kommunegrenser,Fylkesgrenser",
                    VERSION: "1.3.0",
                    FORMAT: "image/png",
                    SRS: "EPSG:32633"
                }
            }))
        });
        wmsAdmGrenser.name = "AdmGrenser";
        wmsAdmGrenser.description = "Administrative grenser WMS";

        //http://wms.miljodirektoratet.no/arcgis/services/vern/mapserver/WMSServer?request=GetCapabilities&service=WMS&version=1.3.0
        var wmsVern = new ol.layer.Tile({
            extent: [-20037508.34, -20037508.34, 20037508.34, 20037508.34],
            source: new ol.source.TileWMS(({
                attributions: [miljodirAttribution],
                url: config.proxyurl + "http://wms.miljodirektoratet.no/arcgis/services/vern/mapserver/WMSServer?",
                params: {
                    LAYERS: "foreslatt_naturvern_omrade,naturvern_omrade",
                    VERSION: "1.3.0",
                    FORMAT: "image/png",
                    SRS: "EPSG:32633"
                }
            }))
        });
        wmsVern.name = "Vern";
        wmsVern.description = "Verneområder";

        // WMS-layers end

        var openCycleMapLayer = new ol.layer.Tile({
            source: new ol.source.OSM({
                attributions: [
                    new ol.Attribution({
                        html: 'All maps &copy; ' +
                        '<a href="http://www.opencyclemap.org/">OpenCycleMap</a>'
                    }),
                    ol.source.OSM.ATTRIBUTION
                ],
                url: '//{a-c}.tile.opencyclemap.org/cycle/{z}/{x}/{y}.png'
            })
        });
        openCycleMapLayer.name = "OpenCycleMap";
        openCycleMapLayer.description = "Open Cycle Map";

        var openStreetMapLayer = new ol.layer.Tile({
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
        openStreetMapLayer.name = "OpenStreetMap";
        openStreetMapLayer.description = "Open Street Map";

///---------------------
        function tileUrlFunction(tileCoord) {
            return ('/ninmap/nin/{z}/{x}/{y}.geojson'
                .replace('{z}', String(tileCoord[0]))
                .replace('{x}', String(tileCoord[1]))
                .replace('{y}', String(tileCoord[2])));
        }
        function tileUrlFunctionHeat(tileCoord) {
//            return ('http://c.tiles.wmflabs.org/hillshading/{z}/{x}/{y}.png'
            //return ('/ninmap/heat/{z}/{x}/{y}.png'
            return ('http://localhost:56271/map/heat/{z}/{x}/{y}.png'

                .replace('{z}', String(tileCoord[0]))
                .replace('{x}', String(tileCoord[1]))
                .replace('{y}', String(tileCoord[2])));
        }

        var landuseStyleCache = {};
        var resolutions = [];
//        for (var z = 1; z <= 22; ++z) {
		for (var z = 0; z <= 17; ++z) 
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
                    tileGrid: ol.tilegrid.createXYZ({ minZoom: 3, maxZoom: 13 }),
                    tileGrid2: new ol.tilegrid.TileGrid({
                        extent: projection.getExtent(),
                        resolutions: resolutions
                    }),
                    tileUrlFunction: tileUrlFunction
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
							'T44': '#404040',
                        }[styleKey];
						width = {
                            'envelope': 0.1,
                            'kommune': 2,
                            'fylke': 3,
						}[styleKey];
						if(width == undefined) width = 1;
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
							'NA L2-2': 'rgba(20,220,220,0.2)',
						}[styleKey];
						var lineDash = {
                            'envelope': [7],
                            'kommune': [5],
						'fylke': [14]
						}[styleKey];
						if(fillColor == undefined)
                        console.log("____"+styleKey);
                        style = new ol.style.Style({
                            stroke: new ol.style.Stroke({
                                color: color, 
								lineDash: lineDash == undefined ? undefined : lineDash,
                                width: width 
                            }),
                            fill:  fillColor == undefined ? undefined : new ol.style.Fill({ color: fillColor })
                        });
                        landuseStyleCache[styleKey] = style;
                    }
                    return style;
                }
            });
        layer1.name = "Layer1";
        layer1.description = "Layer1";
///---------------------

      var tmsHeat = new ol.layer.Tile({
        preload: 1,
        source: new ol.source.TileImage({
            projection: "EPSG:32633",
			 tileGrid: ol.tilegrid.createXYZ({
			     minZoom: 0, maxZoom: 13,
                 extent: [-2500000, 3500000, 3045984, 9045984]
			 }),
             tileUrlFunction: tileUrlFunctionHeat
        })
    });
	tmsHeat.name="Heat test";
	tmsHeat.description="Heat test";
					
	  var group = new ol.layer.Group({
        layers: [ greyMap2, tmsHeat] });
//        layers: [ wmsNib, tmsHeat] });
        group.name = "Group test";
        group.description  = "Group test";

        var bingMapNames = [
            'Road',
            'Aerial',
            'AerialWithLabels'
        ];

        var bingDescriptions = [
            'Bing Road',
            'Bing Aerial',
            'Bing Aerial with labels'
        ];


        greyMap.needsToken = true;
        greyMap2.needsToken = true;
        grunnkart.needsToken = true;
        terreng.needsToken = true;

        var baseLayerPool = ko.observableArray([]);
        var overlayLayerPool = ko.observableArray([]);
        baseLayerPool.push(tmsHeat, group, openStreetMapLayer, openCycleMapLayer, greyMap, greyMap2, grunnkart, terreng, wmsNib);
        overlayLayerPool.push(tmsHeat, group, wmsNin, wmsNinLandskap, wmsAdmGrenser, wmsVern);
        //baseLayerPool.push(wmsNib, openStreetMapLayer, openCycleMapLayer, greyMap, greyMap2, grunnkart, terreng);
        //overlayLayerPool.push(wmsNin, wmsNinLandskap, wmsAdmGrenser, wmsVern);

        var i, ii;
        for (i = 0, ii = bingMapNames.length; i < ii; ++i) {
            var layer =
                new ol.layer.Tile({
                    preload: Infinity,
                    source: new ol.source.BingMaps({
                        key: config.bingKey,
                        imagerySet: bingMapNames[i]
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
            for (i = -colors.length; i < (colors.length * 2); i++) {
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

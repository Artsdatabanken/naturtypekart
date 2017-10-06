define([],
    function () {

        // var rooturl = 'http://localhost:59360/',
        //     dataAdmRootUrl = 'http://localhost:60031/',
        var rooturl = 'http://it-webadbtest01.it.ntnu.no/nin_master/Api/',
            dataAdmRootUrl = 'http://it-webadbtest01.it.ntnu.no/nin_master/Api/',

        dataDeliveryRootUrl = '/nindocument_vs2017/',
        //dataDeliveryRootUrl = 'http://localhost:60126/',

        proxyRootUrl = '/NinProxy/',
        asiWebRoot = '/ASIWeb/',
        t = "0.6",  // polygon fill-transparency

        config = {
            rooturl: rooturl,
            url: rooturl + 'Observation/',
            apiurl: rooturl + 'geolocation/',
            dataAdmApiUrl: dataAdmRootUrl + 'data/',
            dataDeliveryApiUrl: dataDeliveryRootUrl + 'DataDelivery/',
            logourl: rooturl + 'content/images/logoer/',
            proxyurl: proxyRootUrl + '?url=',
            newUserUrl: asiWebRoot + 'UserSystem/AnonymousUsers/NewUser.aspx',
            forgotPasswordUrl: asiWebRoot + 'UserSystem/AnonymousUsers/PasswordRecovery.aspx',
            bk: 'AkhtNitomZjCNFu9kgrrbVeaF65h94ce1lpwtbeZTuFYiY-ztrERImme_IM3vh6k', // bing-key registrert på Amund, todo: replace
            dev: true, // for nd token
            defaultMapZoom: 3,
            defaultMapCenter: '427864,7623020',
            initialBaseMapLayer: "NiB",
            maxListItems: 10,
            maxMobileListItems: 3,
            maxMobileListShowPages: 3,
            maxListShowPages: 9,
            maxExportItems: 1000,
            bookmarkLocalStorageKey: 'ADB/NBIC bookmarks',
            bookmarkThumbSize: 64,
            mapTokenStorageKey: 'ADB/NBIC ND token',

            // Show centerpoints instead of polygons if zoom < loadCenterPointLimit
            loadCenterPointLimit: 14, // todo: denne passer fint for naturområder
            //loadCenterPointLimit: 10,   // todo: denne er midlertidig siden landskapstyper brukes som naturområder, og passer bedre når polygonene er større
            // Add bounding box to filter if zoom > useBoundingBoxLimit
            gridBoundingBoxLimit: 9,
            useBoundingBoxLimit: 9,

            routeInfo: [
                { id: 'map', name: "routeMapName", title: "routeMapTitle", moduleId: 'viewmodels/mapOl3', visible: true },
                { id: 'list', name: "routeListName", title: "routeListTitle", moduleId: 'viewmodels/listView', visible: true },
                { id: 'statistics', name: "routeStatName", title: "routeStatTitle", moduleId: 'viewmodels/statistics', visible: true },
                { id: 'import', name: "routeImportName", title: "routeImportTitle", moduleId: 'viewmodels/import', visible: true },
                { id: 'details', name: "routeDetailsName", title: "routeDetailsTitle", moduleId: 'viewmodels/featurePopoverMobile', visible: false },
                { id: 'factsheet', name: "routeFactsheetName", title: "routeFactsheetTitle", moduleId: 'viewmodels/factSheet', visible: false }
                //,{ id: 'bookmark', name: "routeBookmarkName", title: "routeBookmarkTitle", moduleId: 'viewmodels/bookmark', visible: false, specialRouteParameters: bokmarkRouteParameters }
            ],

            // Polygon style definitions:
            //"colors": [array of rgba-colors],
            //"style": "solid"|"squares"|"stripes",
            //"strokeStyle": "solid|"dash",
            //"strokeColor": "rgba color for the polygon line,
            //"strokeWidth": 2  // thickness
            //"strokePattern": [4, 4], // dash pattern if dash
            cartographyColors: Object.freeze({
            UNKN: {
                "colors": ["rgba(227, 26, 28, " + t + ")"],
                "style": "solid",
                "strokeStyle": "solid"
            },
            MOSA: {
                "colors": ["rgba(255, 255, 255, " + t + ")", "rgba(0, 0, 0, " + t + ")"],
                "style": "squares",
                "strokeStyle": "solid"
            },
            NA_T: {
                "colors": ["rgba(65, 171, 93, " + t + ")"],
                "style": "solid",
                "strokeStyle": "solid"
            },
            NA_V: {
                "colors": ["rgba(255, 255, 255, " + t + ")", "rgba(66, 146, 198, " + t + ")"],
                "style": "stripes",
                "strokeStyle": "solid"
            },
            NA_L: {
                "colors": ["rgba(115, 115, 115, " + t + ")"],
                "style": "solid",
                "strokeStyle": "solid"
            },
            NA_F: {
                "colors": ["rgba(66, 146, 198, " + t + ")"],
                "style": "solid",
                "strokeStyle": "solid"
            },
            NA_M: {
                "colors": ["rgba(189, 189, 189, " + t + ")"],
                "style": "solid",
                "strokeStyle": "solid"
            },
            NA_H: {
                "colors": ["rgba(33, 113, 181, " + t + ")"],
                "style": "solid",
                "strokeStyle": "solid"
            },
            NA_I: {
                "colors": ["rgba(255,255,255," + t + ")"],
                "style": "solid",
                "strokeStyle": "dash",
                "strokeColor": "rgba(66, 146, 198, " + t + ")",
                "strokePattern": [4, 4],
                "strokeWidth": 2
            }
        })
        };

        return config;
    });

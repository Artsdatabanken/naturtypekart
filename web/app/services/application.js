define(['knockout', 'lodash', 'services/config'],
function (ko, _, conf) {
    "use strict";

    var currFeature =
    {
        data: null,
        metadata: null
    };

    var footerWarning = ko.observable(false);
    var footerWarningText = ko.observable("");
    var setFooterWarning = function(text) {
        footerWarning(text.length > 0);
        footerWarningText(text);
    };
    var showAbout = function() {
        app.trigger("showAboutPage:trigger");
    };
    var ndToken = ko.observable();
    var ndWorking = false;
    var requestNdToken = function() {
        if (ndWorking) return; 
        ndWorking = true;
            var url = '//www.norgeskart.no/ws/gkt.py';
        $.get(url, function (data) {
            var parts = data.split('"');
            var t = {
                value: parts[1],
                expires: new Date().getTime() + 30 * 60 * 1000,
                when: Date.now()
            };
            ndToken(t);
            window.localStorage.setItem(conf.mapTokenStorageKey, JSON.stringify(t));
            ndWorking = false;
        });
    };

    var getNdToken = function () {
        var now = Date.now();
        var token = JSON.parse(window.localStorage.getItem(conf.mapTokenStorageKey));
        //console.debug("token.expires:" + token.expires);
        console.log(new Date(now + 10 * 60 * 1000));
        if (token != undefined && token != null){
            if (token.value && token.expires && token.expires > (now + 10 * 60 * 1000)) {
                console.log("% Valid token loaded from localstorage: " + JSON.stringify(token));
                return token;
            }
            else if (!token.when || token.when < (now - 60 * 1000)) { // Limit retries to once every minute.
                console.log("% Token is old");
                requestNdToken();
            }
        }
        return false; // Return false, because the value will be recomputed when the ajax call is done anyway.
    };

    var ndTokenValue = ko.computed(function () {
        var token = ndToken(), now = Date.now();
        console.debug("check if we need fetching token. Token:" + token);
        if ((token == undefined || token == null) || !token.value && token.expires && token.expires < (now + 10 * 60 * 1000)) {
            console.debug("fetching token");
            requestNdToken();
//            ndToken(token);
        }
        // if(!token) console.error("No valid token");
        return token != undefined ? token.value : "";
    });
    getNdToken();

    var currentFeature = function (data, metadata) {
        currFeature.data = data;
        currFeature.metadata = metadata;
    },

    viewportState = {
        zoom: ko.observable(conf.defaultMapZoom),
        center: ko.observable(conf.defaultMapCenter),
        background: ko.observable(conf.initialBaseMapLayer),
        id: ko.observable()
    },
    filter = {
        NatureLevelCodes: ko.observableArray([]),
        CenterPoints: ko.observable(false),
        NatureAreaTypeCodes: ko.observableArray([]),
        DescriptionVariableCodes: ko.observableArray([]),
        Municipalities: ko.observableArray([]),
        Counties: ko.observableArray([]),
        ConservationAreas: ko.observableArray([]),
        Institutions: ko.observableArray([]),
        RedlistCategories: ko.observableArray([]),
        RedlistAssessmentUnits: ko.observableArray([]),
        Geometry: ko.observable(""),
        BoundingBox: ko.observable(""),
        EpsgCode: ko.observable("32633"),
        IndexFrom: ko.observable(0),
        IndexTo: ko.observable(25),
        ForceRefreshToggle: ko.observable(false)    // used to force refresh of polygons
    },

    urlFilter = function () {
        return {
            NatureLevelCodes: filter.NatureLevelCodes,
            CenterPoints: filter.CenterPoints,
            NatureAreaTypeCodes: filter.NatureAreaTypeCodes,
            DescriptionVariableCodes: filter.DescriptionVariableCodes,
            Municipalities: filter.Municipalities,
            Counties: filter.Counties,
            ConservationAreas: filter.ConservationAreas,
            Institutions: filter.Institutions,
            RedlistCategories: filter.RedlistCategories,
            RedlistAssessmentUnits: filter.RedlistAssessmentUnits,
            Geometry: filter.Geometry,
            BoundingBox: filter.BoundingBox,
            EpsgCode: filter.EpsgCode
        };
    },
    noBoundingBoxFilter = function () { // har centerpoint og ikke boundingbox
        return {
            CenterPoints: filter.CenterPoints,
            NatureLevelCodes: filter.NatureLevelCodes,
            NatureAreaTypeCodes: filter.NatureAreaTypeCodes,
            DescriptionVariableCodes: filter.DescriptionVariableCodes,
            Municipalities: filter.Municipalities,
            Counties: filter.Counties,
            ConservationAreas: filter.ConservationAreas,
            Institutions: filter.Institutions,
            RedlistCategories: filter.RedlistCategories,
            RedlistAssessmentUnits: filter.RedlistAssessmentUnits,
            Geometry: filter.Geometry,
            EpsgCode: filter.EpsgCode
        };
    },
    listFilter = function () {
        return {
            NatureLevelCodes: filter.NatureLevelCodes,
            NatureAreaTypeCodes: filter.NatureAreaTypeCodes,
            DescriptionVariableCodes: filter.DescriptionVariableCodes,
            Municipalities: filter.Municipalities,
            Counties: filter.Counties,
            ConservationAreas: filter.ConservationAreas,
            Institutions: filter.Institutions,
            Geometry: filter.Geometry,
            EpsgCode: filter.EpsgCode,
            ForceRefreshToggle: filter.ForceRefreshToggle
        };
    },
    grid = {
        Grid: ko.observable(""),
        GridType: ko.observable(""),
        GridLayerTypeId: ko.observable(""),
        GridCellName: ko.observable(""),
        GridValue: ko.observable()
    },

    removeFilter = function(type) {
        if (type === undefined) {
            return;
        }
        filter[type].removeAll();
    },
    arrContains = function (arr, s) {
        var r, i;
        r = false;
        i = arr.length;
        while (i--) {
                    if (arr[i] === s) {
                r = true;
                i = 0;
            }
        }
        return r;
    },
    updateFilterNoDupe = function (add, type, code) {
        if (type === undefined) {
            return false;
        }
                if (!arrContains(filter[type](), code)) {
            if (add === true) {
                filter[type].push(code);
                return true;
            }
        } else {
            if (add !== true) {
                filter[type](_.without(filter[type](), code));
            }
        }
        return false;
    },
    updateFilter = function (add, type, code) {
        if (type === undefined) {
            return;
        }
        if (add === true) {
            filter[type].push(code);
        } else {
            filter[type](_.without(filter[type](), code));
        }
    },
            addSeparator = function (number) {
                number += '';
                var x = number.split('.');
                var x1 = x[0];
                var x2 = x.length > 1 ? '.' + x[1] : '';
                var rgx = /(\d+)(\d{3})/;
                while (rgx.test(x1)) {
                    x1 = x1.replace(rgx, '$1' + ' ' + '$2');
                }
                return x1 + x2;
            },

    viewportStateChanged = ko.computed(function () {
        var dummy;
        _.forEach(_.keys(viewportState), function (key) {
            dummy = viewportState[key]();
        });
        return viewportState;
    }).extend({ rateLimit: 10 }),
    listFilterChanged = ko.computed(function () {
        // trigger changes in filter unless it's just BoundingBox (zoom/pan in the map)
        var dummy;
        _.forEach(_.keys(filter), function (key) {
                    if (key !== "BoundingBox") {
                dummy = filter[key]();
            }
        });
        return filter;
    }).extend({rateLimit: 100}),
    filterChanged = ko.computed(function () {
        var dummy;
        _.forEach(_.keys(filter), function (key) {
            dummy = filter[key]();
        });
        return filter;
    }).extend({ rateLimit: 100 }),

    fixDates = function () {
        $.datepicker.regional.no =
            {
                clearText: "Tøm",
                clearStatus: "",
                closeText: "Lukk",
                closeStatus: "",
                prevText: "&laquo;Forrige",
                prevStatus: "",
                prevBigText: "&#x3c;&#x3c;",
                prevBigStatus: "",
                nextText: "Neste&raquo;",
                nextStatus: "",
                nextBigText: "&#x3e;&#x3e;",
                nextBigStatus: "",
                currentText: "I dag",
                currentStatus: "",
                monthNames: ["januar", "februar", "mars", "april", "mai", "juni", "juli", "august", "september", "oktober", "november", "desember"],
                monthNamesShort: ["jan", "feb", "mar", "apr", "mai", "jun", "jul", "aug", "sep", "okt", "nov", "des"],
                monthStatus: "",
                yearStatus: "",
                weekHeader: "Uke",
                weekStatus: "",
                dayNamesShort: ["Søn", "Man", "Tir", "Ons", "Tor", "Fre", "Lør"],
                dayNames: ["Søndag", "Mandag", "Tirsdag", "Onsdag", "Torsdag", "Fredag", "Lørdag"],
                dayNamesMin: ["Sø", "Ma", "Ti", "On", "To", "Fr", "Lø"],
                dayStatus: "DD",
                dateStatus: "D, M d",
                dateFormat: "yy-mm-dd",
                firstDay: 0,
                initStatus: "",
                isRTL: false
            };
        $.datepicker.setDefaults($.datepicker.regional.no);
    },
    formatDate = function (dateString) {
        if (!dateString) {
            return "";
        }
        return $.datepicker.formatDate('d. M yy', new Date(dateString));
    },
    formatDateSorting = function (dateString) {
        return $.datepicker.formatDate('yyyy-mm-dd', new Date(dateString));
    },
    pad = function (n, width, z) {
        z = z || '0';
        n = n + '';
        return n.length >= width ? n : new Array(width - n.length + 1).join(z) + n;
    },
            isSecure = function () {
                return window.location.protocol === 'https:';
            },
    isIe = navigator.userAgent.match(/MSIE\s([\d.]+)/),
    ie11 = navigator.userAgent.match(/Trident\/7.0/) && navigator.userAgent.match(/rv:11/),
    ieEdge = navigator.userAgent.match(/Edge/g),
    ieVer = (isIe ? isIe[1] : (ie11 ? 11 : (ieEdge ? 12 : -1))),

    vm = {
        rebuildTree: ko.observable(false),
        bookmarks: ko.observableArray([]),
        ndToken: ndTokenValue,

        setFooterWarning: setFooterWarning,
        footerWarning: footerWarning,
        footerWarningText: footerWarningText,

        currFeature: currFeature,
        currentLayer: ko.observable(),
        config: conf,

        updateFilter: updateFilter,
        updateFilterNoDupe: updateFilterNoDupe,
        removeFilter: removeFilter,

                addSeparator: addSeparator,
        viewportState: viewportState,
        filter: filter,
        urlFilter: urlFilter,
        listFilter: listFilter,
        noBoundingBoxFilter: noBoundingBoxFilter,
        grid: grid,

        viewportStateChanged: viewportStateChanged,
        filterChanged: filterChanged,
        listFilterChanged: listFilterChanged,
        currentFeature: currentFeature,

        routeInfo: conf.routeInfo,
        fixDates: fixDates,
        formatDate: formatDate,
        formatDateSorting: formatDateSorting,
        showAbout: showAbout,

        pad: pad,
        isIe: isIe,
        isIe11: ie11,
        ieVer: ieVer,
		isSecure: isSecure,

        // * view-things *
        baseLayer: "",
        totalCount: ko.observable(0)
    };
    vm.parseUrlFilter = function (filterString) {
        vm.applyFilter({
            "filter": filterString
        });
    };
    vm.applyFilter = function (bookmark) {
        var bmFilter = JSON.parse(bookmark.filter);
        var bmGrid = bookmark.grid ? JSON.parse(bookmark.grid) : {};
        vm.setFilter(bmFilter, bmGrid);
    };

    vm.setFilter = function (bmFilter, bmGrid) {
        if (!_.isEqual(filter.NatureLevelCodes(), bmFilter.NatureLevelCodes)) filter.NatureLevelCodes(bmFilter.NatureLevelCodes || []);
        if (!_.isEqual(filter.CenterPoints(), bmFilter.CenterPoints)) filter.CenterPoints(bmFilter.CenterPoints || []);
        if (!_.isEqual(filter.NatureAreaTypeCodes(), bmFilter.NatureAreaTypeCodes)) filter.NatureAreaTypeCodes(bmFilter.NatureAreaTypeCodes || []);
        if (!_.isEqual(filter.DescriptionVariableCodes(), bmFilter.DescriptionVariableCodes)) filter.DescriptionVariableCodes(bmFilter.DescriptionVariableCodes || []);
        if (!_.isEqual(filter.Municipalities(), bmFilter.Municipalities)) filter.Municipalities(bmFilter.Municipalities || []);
        if (!_.isEqual(filter.Counties(), bmFilter.Counties)) filter.Counties(bmFilter.Counties || []);
        if (!_.isEqual(filter.ConservationAreas(), bmFilter.ConservationAreas)) filter.ConservationAreas(bmFilter.ConservationAreas || []);
        if (!_.isEqual(filter.Institutions(), bmFilter.Institutions)) filter.Institutions(bmFilter.Institutions || []);
        if (!_.isEqual(filter.RedlistCategories(), bmFilter.RedlistCategories)) filter.RedlistCategories(bmFilter.RedlistCategories || []);
        if (!_.isEqual(filter.RedlistAssessmentUnits(), bmFilter.RedlistAssessmentUnits)) filter.RedlistAssessmentUnits(bmFilter.RedlistAssessmentUnits || []);

        if (!_.isEqual(filter.Geometry(), bmFilter.Geometry)) filter.Geometry(bmFilter.Geometry);
        if (!_.isEqual(filter.BoundingBox(), bmFilter.BoundingBox)) filter.BoundingBox(bmFilter.BoundingBox);
        if (!_.isEqual(filter.EpsgCode(), bmFilter.EpsgCode)) filter.EpsgCode(bmFilter.EpsgCode || 0);

        if (!_.isEqual(grid.GridType(), bmGrid.GridType)) grid.GridType(bmGrid.GridType || 0);
        if (!_.isEqual(grid.GridLayerTypeId(), bmGrid.GridLayerTypeId)) grid.GridLayerTypeId(bmGrid.GridLayerTypeId || 0);
        if (!_.isEqual(grid.Grid(), bmGrid.Grid)) grid.Grid(bmGrid.Grid);

        vm.rebuildTree(true);
    };

    vm.initBookmarks = function() {
        var bookmarks;
        if ((bookmarks = JSON.parse(window.localStorage.getItem(conf.bookmarkLocalStorageKey))) && Array.isArray(bookmarks)) {
            vm.bookmarks.removeAll();
            bookmarks.forEach(function (e) {
                vm.bookmarks.push(e);
            });
        }
    };

    return vm;
});

define(['services/logger', 'jquery', 'knockout', 'services/config', 'durandal/app'],
    function (logger, $, ko, config, app) {
        var title = "dataServices",
            myPromises = {},
            promiseKeeper = function (name, promise) {
                var lastPromise = myPromises[name];
                if (lastPromise && lastPromise.state() === "pending") {
                    // Pending call exist - abort it!
                    lastPromise.abort("Ignore");
                }
                myPromises[name] = promise;
            },
            downloadFile = function download(data, contentType, fileName, isBinary) {
                var ie = navigator.userAgent.match(/MSIE\s([\d.]+)/);
                var ie11 = navigator.userAgent.match(/Trident\/7.0/) && navigator.userAgent.match(/rv:11/);
                var ieEdge = navigator.userAgent.match(/Edge/g);
                var ieVer = ie ? ie[1] : ie11 ? 11 : ieEdge ? 12 : -1;

                if (ie && ieVer < 10) {
                    alert("Export is not supported in this browser.");
                    return;
                }

                if (ieVer > -1) {
                    var dataAsBlob = new Blob([data], {
                        type: contentType
                    });
                    window.navigator.msSaveOrOpenBlob(dataAsBlob, fileName);
                } else {
                    var element = document.createElement('a');
                    element.setAttribute('download', fileName);
                    if (isBinary) {
                        element.setAttribute('href', 'data:' + contentType + ';base64,' + btoa([].reduce.call(new Uint8Array(data), function(p, c) {
                            return p + String.fromCharCode(c);
                        }, '')));
                    } else {
                        element.setAttribute('href', 'data:' + contentType + ',' + encodeURIComponent(data));
                    }
                    element.style.display = 'none';
                    document.body.appendChild(element);
                    element.click();
                    document.body.removeChild(element);
                }
                app.trigger('downloadFile:done', fileName);
            },

            getDataPromise = function(name, url, filter, type) {
                var rq = {
                    type: type || 'GET',
                    contentType: "application/json;charset=utf-8"
                };
                rq.url = url;
                if (filter !== undefined) {
                    rq.data = ko.toJS(filter);
                }
                var promise = $.ajax(rq);

                promiseKeeper(name, promise);

                return promise;
            },

            postDataPromise = function(name, url, filter, type) {
                var rq = {
                    type: type || 'POST',
                    contentType: "application/json;charset=utf-8"
                };
                rq.url = url;
                if (filter !== undefined) {
                    rq.data = ko.toJSON(filter);
                }
                var promise = $.ajax(rq);

                promiseKeeper(name, promise);
                return promise;
            },
            deleteDataPromise = function(url) {
                var rq = {
                    type: 'DELETE',
                    contentType: "application/json;charset=utf-8"
                };
                rq.url = url;

                var promise = $.ajax(rq);

                return promise;
            },
            postBinaryRequest = function (url, filter, contentType, filename) {
                var xhr = new XMLHttpRequest();
                xhr.open('POST', url, true);
                xhr.timeout = 12000; // time in milliseconds // todo: decide a sensible value
                xhr.responseType = 'arraybuffer';
                xhr.onload = function () {
                    if (this.status === 200) {
                        downloadFile(this.response, contentType, filename, true);
                    }
                };

                var data = ko.toJSON(filter);

                xhr.setRequestHeader("Content-type", "application/json;charset=utf-8");
                xhr.send(data);
            },

            postFormUploadPromise = function(url, formData) {
                var rq = {
                    type: 'POST',
                    url: url,
                    contentType: false,
                    cache: false,
                    processData: false,
                    enctype: "multipart/form-data",
                    data: formData,
                    async: true,
                    timeout: 360000 // time in milliseconds // todo: decide a sensible value
                };
                var promise = $.ajax(rq);

                return promise;
            },

            /// ----- api calls ----------
            authenticate = function (formData) {
                logger.log('authenticate', null, title, true);
                var url = config.dataDeliveryApiUrl + 'Authenticate';
                return postFormUploadPromise(url, formData);
            },
            uploadDataDelivery = function (formData) {
                logger.log('uploadDataDelivery', null, title, true);
                var url = config.dataDeliveryApiUrl + 'UploadDataDelivery';
                return postFormUploadPromise(url, formData);
            },
            uploadGrid = function (formData) {
                logger.log('uploadGrid', null, title, true);
                var url = config.dataDeliveryApiUrl + 'UploadGrid';
                return postFormUploadPromise(url, formData);
            },
            uploadGridDelivery = function (formData) {
                logger.log('uploadGridDelivery', null, title, true);
                var url = config.dataDeliveryApiUrl + 'UploadGridDelivery';
                return postFormUploadPromise(url, formData);
            },
            getListOfDataDeliveries = function (username) {
                logger.log('getListOfDataDeliveries', null, title, true);
                var url = config.dataDeliveryApiUrl + "GetListOfDataDeliveries/?username=" + username;
                return getDataPromise('getListOfDataDeliveries', url);
            },
            getListOfImportedDataDeliveries = function () {
                logger.log('getListOfImportedDataDeliveries', null, title, true);
                var url = config.dataDeliveryApiUrl + "GetListOfImportedDataDeliveries";
                return getDataPromise('getListOfImportedDataDeliveries', url);
            },
            publiserDataleveranse = function (formData) {
                logger.log('publiserDataleveranse', null, title, true);
                var url = config.dataDeliveryApiUrl + 'PubliserDataleveranse';
                return postFormUploadPromise(url, formData);
            },
            deleteGridDelivery = function (id) {
                logger.log('deleteGridDelivery', null, title, true);
                var url = config.dataDeliveryApiUrl + "DeleteGridDelivery/" + id;
                return deleteDataPromise(url);
            },
            getAllGridDeliveries = function () {
                logger.log('GetAllGridDeliveries', null, title, true);
                var url = config.dataDeliveryApiUrl + "GetAllGridDeliveries";
                return getDataPromise('getAllGridDeliveries', url);
            },
            getFile = function(docGuid, contentType, filename) {
                logger.log('DownloadGrid', null, title, true);
                var url = config.dataDeliveryApiUrl + 'DownloadDocument/' + docGuid;
                var xhr = new XMLHttpRequest();
                xhr.open("GET", url, true);
                xhr.onload = function () {
                    if (this.status === 200) {
                        downloadFile(this.response, contentType, filename, false);
                    }
                };
                xhr.send();
            },

            getLocationByTerm = function (searchTerm) {
                logger.log('getLocationByTerm', null, title, true);
                var url = config.apiurl + 'geolocationByName?term=' + searchTerm;
                return getDataPromise('getLocationByTerm', url);
            },
            //searchMunicipality = function(searchTerm) {
            //    logger.log('searchMunicipality', null, title, true);
            //    var url = config.apiurl + 'searchMunicipality/' + searchTerm;
            //    return getDataPromise(url);
            //},
            hentOmraadeForMatrikkelenhet = function(kommuneNr, gaardsnr, bruksNr, festNr, seksjonsNr) {
                logger.log('hentOmraadeForMatrikkelenhet', null, title, true);
                var url = config.apiurl + 'HentOmraadeForMatrikkelenhet/' + kommuneNr + '/' + gaardsnr + '/' + bruksNr + '/' + festNr + '/' + seksjonsNr;
                return getDataPromise('hentOmraadeForMatrikkelenhet', url);
            },
            finnMatrikkelenheter = function (kommuneNr, gaardsnr, bruksNr) {
                logger.log('finnMatrikkelenheter', null, title, true);
                var url = config.apiurl + 'FinnMatrikkelenheter/' + kommuneNr + '/' + gaardsnr + '/' + bruksNr;
                return getDataPromise('finnMatrikkelenheter', url);
            },
            getGbnrByTerm = function(searchTerm) {
                logger.log('getGbnrByTerm', null, title, true);
                var url = config.apiurl + 'geolocationByGBNr/' + searchTerm;
                return getDataPromise('getGbnrByTerm', url);
            },
            getNatureAreaStatisticsBySearchFilter = function (filter) {
                logger.log('getNatureAreaStatisticsBySearchFilter', null, title, true);
                var url = config.dataAdmApiUrl + 'GetNatureAreaStatisticsBySearchFilter';
                return postDataPromise('getNatureAreaStatisticsBySearchFilter', url, filter);
            },
            getNatureAreaInstitutionSummary = function (filter) {
                logger.log('getNatureAreaInstitutionSummary', null, title, true);
                var url = config.dataAdmApiUrl + 'GetNatureAreaInstitutionSummary';
                return postDataPromise('getNatureAreaInstitutionSummary', url, filter);
            },
            getNatureAreaSummary = function (filter) {
                logger.log('getNatureAreaSummary', null, title, true);
                var url = config.dataAdmApiUrl + 'GetNatureAreaSummary';
                return postDataPromise('getNatureAreaSummary', url, filter);
            },
            getAreaSummary = function (filter) {
                logger.log('getAreaSummary', null, title, true);
                var url = config.dataAdmApiUrl + 'GetAreaSummary';
                return postDataPromise('getAreaSummary', url, filter);
            },
            getGridSummary = function () {
                logger.log('getGridSummary', null, title, true);
                var url = config.dataAdmApiUrl + 'getGridSummary';
                return getDataPromise('getGridSummary', url);
            },

            getAreas = function(type, nr) {
                logger.log('getArea', null, title, true);
                var url = config.dataAdmApiUrl + 'GetAreas/?areatype=' + type + '&number=' + nr;
                return getDataPromise('getAreas' + nr, url);
            },
            searchAreas = function (name, areatype) {
                logger.log('searchAreas', null, title, true);
                var url = config.dataAdmApiUrl + 'SearchAreas/?name=' + name + '&areatype=' + areatype;
                return getDataPromise('searchAreas', url);
            },
            exportNatureAreasByLocalIds = function (ids) {
                logger.log('exportNatureAreasByLocalIds', null, title, true);
                var url = config.dataAdmApiUrl + 'ExportNatureAreasByLocalIds';
                var iddata = { 'LocalIds': ids };
                return postDataPromise('exportNatureAreasByLocalIds', url, iddata);
            },
            exportNatureAreasBySearchFilter = function (filter) {
                logger.log('exportNatureAreasBySearchFilter', null, title, true);
                var url = config.dataAdmApiUrl + 'ExportNatureAreasBySearchFilter';
                return postDataPromise('exportNatureAreasBySearchFilter', url, filter);
            },
            exportNatureAreasAsShapeBySearchFilter = function (filter) {
                logger.log('exportNatureAreasAsShapeBySearchFilter', null, title, true);
                var url = config.dataAdmApiUrl + 'ExportNatureAreasAsShapeBySearchFilter';
                return postBinaryRequest(url, filter, 'application/zip', 'data.zip');
            },
            exportNatureAreasAsGmlBySearchFilter = function (filter) {
                logger.log('exportNatureAreasAsGmlBySearchFilter', null, title, true);
                var url = config.dataAdmApiUrl + 'ExportNatureAreasAsGmlBySearchFilter';
                return postBinaryRequest(url, filter, 'application/xml', 'data.xml');
            },
            exportNatureAreasAsXlsxBySearchFilter = function (filter) {
                logger.log('exportNatureAreasAsXlsxBySearchFilter', null, title, true);
                var url = config.dataAdmApiUrl + 'ExportNatureAreasAsXlsxBySearchFilter';
                return postBinaryRequest(url, filter, 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', 'data.xlsx');
            },
            getNatureAreasBySearchFilter = function(filter) {
                logger.log('getNatureAreasBySearchFilter', null, title, true);
                var url = config.dataAdmApiUrl + 'GetNatureAreasBySearchFilter?';
                return postDataPromise('getNatureAreasBySearchFilter', url, filter);
            },
            getNatureAreaInfosBySearchFilter = function (filter) {
                logger.log('getNatureAreaInfosBySearchFilter', null, title, true);
                var url = config.dataAdmApiUrl + 'getNatureAreaInfosBySearchFilter?';
                return postDataPromise('getNatureAreaInfosBySearchFilter', url, filter);
            },

            getNatureAreaByLocalId = function (id) {
                logger.log('getNatureAreaByLocalId', null, title, true);
                var url = config.dataAdmApiUrl + 'GetNatureAreaByLocalId/' + id;
                return getDataPromise('getNatureAreaByLocalId' + id, url);
            },

            getMetadataByNatureAreaLocalId = function (id) {
                logger.log('getMetadataByNatureAreaLocalId', null, title, true);
                var url = config.dataAdmApiUrl + 'GetMetadataByNatureAreaLocalId/' + id;
                return getDataPromise('getMetadataByNatureAreaLocalId' + id, url);
            },
            getExpiredMetadatasByNatureAreaLocalId = function (id) {
                logger.log('getExpiredMetadatasByNatureAreaLocalId', null, title, true);
                var url = config.dataAdmApiUrl + 'GetExpiredMetadatasByNatureAreaLocalId/' + id;
                return getDataPromise('getExpiredMetadatasByNatureAreaLocalId' + id, url);
            },

            getGrid = function (filter) {
                logger.log('GetGrid', null, title, true);
                var url = config.dataAdmApiUrl + 'GetGrid?';
                return postDataPromise('getGrid', url, filter);
            },

            /// ---------------------
            services = {
                promiseKeeper: promiseKeeper,
                getLocationByTerm: getLocationByTerm,
                getGbnrByTerm: getGbnrByTerm,
                getNatureAreasBySearchFilter: getNatureAreasBySearchFilter,
                getNatureAreaInfosBySearchFilter: getNatureAreaInfosBySearchFilter,
                getNatureAreaByLocalId: getNatureAreaByLocalId,
                getMetadataByNatureAreaLocalId: getMetadataByNatureAreaLocalId,
                getExpiredMetadatasByNatureAreaLocalId: getExpiredMetadatasByNatureAreaLocalId,
                getNatureAreaInstitutionSummary: getNatureAreaInstitutionSummary,
                getNatureAreaStatisticsBySearchFilter: getNatureAreaStatisticsBySearchFilter,
                getNatureAreaSummary: getNatureAreaSummary,
                getAreaSummary: getAreaSummary,
                getGridSummary: getGridSummary,
                getAreas: getAreas,
                searchAreas: searchAreas,
                hentOmraadeForMatrikkelenhet: hentOmraadeForMatrikkelenhet,
                finnMatrikkelenheter: finnMatrikkelenheter,
                //searchMunicipality: searchMunicipality,
                getGrid: getGrid,

                /* Export */
                downloadFile: downloadFile,
                exportNatureAreasByLocalIds: exportNatureAreasByLocalIds,
                exportNatureAreasBySearchFilter: exportNatureAreasBySearchFilter,
                exportNatureAreasAsShapeBySearchFilter: exportNatureAreasAsShapeBySearchFilter,
                exportNatureAreasAsGmlBySearchFilter: exportNatureAreasAsGmlBySearchFilter,
                exportNatureAreasAsXlsxBySearchFilter: exportNatureAreasAsXlsxBySearchFilter,

                /* Import */
                uploadGrid: uploadGrid,
                uploadDataDelivery: uploadDataDelivery,
                uploadGridDelivery: uploadGridDelivery,
                getListOfDataDeliveries: getListOfDataDeliveries,
                getListOfImportedDataDeliveries: getListOfImportedDataDeliveries,
                getAllGridDeliveries: getAllGridDeliveries,
                getFile: getFile,
                publiserDataleveranse: publiserDataleveranse,
                deleteGridDelivery: deleteGridDelivery,
                authenticate: authenticate
            };

        return services;
    });

define(['services/logger', "knockout", 'durandal/app', 'services/dataServices', "services/application", "viewmodels/mapOl3", 'viewmodels/nav', 'services/codeLists', "services/knockoutExtensions", "bootstrap-treeview"],
    function (logger, ko, app, dataServices, application, map, nav, codeLists, jqAutoComplete, treeview) {
        var title = "treeview",
        $summarytree,
        $gridtree,
        $sourcetree,
        vm = {
            activate: function () {
                logger.log(title + ' View Activated', null, title, true);
                //dataServices.finnMatrikkelenheter(1601, 10, 1);    // Kickstart matrikkel soap service.. todo: remove?
            },
            admAreas: ko.observableArray(),
            conservationAreas: ko.observableArray(),
            isMapView: ko.computed(function () {
                return (nav.activeView() === 'map');
            }),

            isNotHideMenuView: ko.computed(function () {
                return (nav.activeView() !== 'import') && (nav.activeView() !== 'factsheet');
            }),

            hasNodesSelected: ko.computed(function () {
                return (application.filter.NatureLevelCodes().length + application.filter.NatureAreaTypeCodes().length + application.filter.DescriptionVariableCodes().length + application.filter.Municipalities().length +
                    +application.filter.Counties().length) + application.filter.ConservationAreas().length > 0;
            }),
            hasGeometryFilter: ko.computed(function () {
                return application.filter.Geometry() && application.filter.Geometry().length > 0;
            }),
            hasGrid: ko.computed(function () {
                return application.grid.Grid() !== undefined && typeof (application.grid.Grid()) === "object";
            }),
            hasSourceFilter: ko.computed(function () {
                return application.filter.Institutions() && application.filter.Institutions().length > 0;
            }),
            geoInfoVisible: ko.observable(false),
            toggleGeoInfo: function () {
                vm.geoInfoVisible(!vm.geoInfoVisible());
            },
            naturInfoVisible: ko.observable(false),
            toggleNaturInfo: function () {
                vm.naturInfoVisible(!vm.naturInfoVisible());
            },
            gridInfoVisible: ko.observable(false),
            toggleGridInfo: function () {
                vm.gridInfoVisible(!vm.gridInfoVisible());
            },
            sourceInfoVisible: ko.observable(false),
            toggleSourceInfo: function () {
                vm.sourceInfoVisible(!vm.sourceInfoVisible());
            },
            matrikkelEnheter: ko.observableArray(),
            selectedAdmArea: ko.observable(),
            admSpinner: ko.observable(false),
            conservationSpinner: ko.observable(false),
            selectedConservationArea: ko.observable(),
            matrikkelSpinner: ko.observable(false),
            selectedMatrikkelEnhet: ko.observable(),
            admArea: function (type, nr, text) {
                return {
                    type: ko.observable(type),
                    nr: ko.observable(nr),
                    displayName: ko.observable(text)
                };
            },
            matrikkelEnhet: function (nr, text) {
                return {
                    nr: ko.observable(nr),
                    displayName: ko.observable(text)
                };
            },
            getAdmAreas: function (searchTerm, oarray) {
                searchTerm = searchTerm.replace(/\s/g, '').toLowerCase();
                if (!/[a-zæøå]{2,}/.test(searchTerm)) {
                    return;
                }

                vm.admSpinner(true);
                dataServices.searchAreas(searchTerm, 0).then(function (data) {
                //dataServices.searchMunicipality(searchTerm).then(function (data) {
                    var result = [];
                    _.forEach(data, function (place) {
                        if (place.Type !== 3) {
                            var desc = place.Name;
                            if (place.Type === 1) {
                                // Kommune, find fylke
                                var countyNr = Math.floor(place.Number / 100);
                                desc = place.Name + ", " + codeLists.counties[countyNr];
                            }
                            result.push(vm.admArea(place.Type, place.Number, desc));
                            //var desc = place.Name + ", " + place.CountyName;
                            //result.push(vm.kommune(place.MunicipalityNr, desc));
                        }
                    });
                    oarray(result);
                    vm.admSpinner(false);
                });
            },
            getConservationAreas: function (searchTerm, oarray) {
                searchTerm = searchTerm.replace(/\s/g, '').toLowerCase();
                if (!/[a-zæøå]{2,}/.test(searchTerm)) {
                    return;
                    }

                vm.conservationSpinner(true);
                dataServices.searchAreas(searchTerm, 3).then(function (data) {
                    var result =[];
                    _.forEach(data, function (place) {
                        var desc = place.Name;
                        result.push(vm.admArea(place.Type, place.Number, desc));
                        });
                        oarray(result);
                        vm.conservationSpinner(false);
                        });
            },
            getMatrikkelEnheter: function (searchTerm, oarray) {
                var params = searchTerm.match(/(\d+)/g);
                if (params.length >= 2 && /\d{4}/.test(params[0])) {
                    (params.length === 2) && params.push(0);
                    vm.matrikkelSpinner(true);
                    dataServices.finnMatrikkelenheter.apply(this, params).then(function (data) {
                        var result = [];
                        _.forEach(data, function (enhet) {
                            var nr = enhet.kommunenummer + "-" + enhet.gaardsnummer + "/" + enhet.bruksnummer;
                            var desc = enhet.kommunenummer + "-" + enhet.gaardsnummer + "/" + enhet.bruksnummer;
                            result.push(vm.matrikkelEnhet(nr, desc));
                        });
                        oarray(result);
                        vm.matrikkelSpinner(false);
                    });
                }
            },
            clearSearchFields: function () {
                $('#adm-search').val('');
                $('#conservation-search').val('');
                $('#mat-search').val('');
            },

            compositionComplete: function () {
                vm.buildTree();
                vm.buildGridTree();
                $('#input-search').on('keyup', vm.search);

                $('#input-search').on('click', function (e) {
                    $summarytree.treeview('clearSearch');
                    $('#input-search').val('');
                });
                $('#input-searchGrid').on('keyup', vm.searchGrid);

                $('#input-searchGrid').on('click', function (e) {
                    $gridtree.treeview('clearSearch');
                    $('#input-searchGrid').val('');
                });
                $('#adm-search').on('click', function (e) {
                    vm.clearSearchFields();
                });
                $('#conservation-search').on('click', function (e) {
                    vm.clearSearchFields();
                });
                $('#mat-search').on('click', function (e) {
                    vm.clearSearchFields();
                });
                $('#drawpolygon').on('click', function (e) {
                    vm.clearSearchFields();
                });
                app.on('currentSelection:remove').then(function () {
                    vm.clearSearchFields();
                    vm.buildTree();
                });
                app.on('currentSelection:add').then(function () {
                    vm.buildTree();
                });
                app.on('mapview:activate').then(function () {
                    vm.buildTree();
                });

                $('#btn-checked').on('click', vm.showCheckedNodes);
                $('#btn-collapse').on('click', vm.collapseNodes);
                $('#btn-reset').on('click', vm.clearCheckedNodes);
            }
        };
        vm.buildSourceTree = function (filter) {
            var tree = [];
            dataServices.getNatureAreaInstitutionSummary(filter).then(function (data) {
                var type = "Institutions";
                _.each(data, function (value, key, list) {
                    var inst = {
                        text: value.name,
                        tags: [value.natureAreaCount],
                        type: type,
                        code: value.name,
                        state: {
                            checked: application.filter[type].indexOf(value.Name) >= 0
                        }
                    };
                    tree.push(inst);
                });

                $sourcetree = $('#sourcetree').treeview({
                    data: tree,
                    showTags: true,
                    enableTitles: true,
                    showCheckbox: true,
                    showIcon: false,
                    onNodeChecked: function (event, node) {
                        application.updateFilter(true, node.type, node.code);
                    },
                    onNodeUnchecked: function (event, node) {
                        application.updateFilter(false, node.type, node.code);
                    }
                });

            });

        };
        vm.buildGridTree = function() {
            var tree = [];
            dataServices.getGridSummary()
                .then(function(data) {
                    function buildGridTreeNodes(node, list, key, type, tag) {
                        node.nodes = [];
                        _.each(list[key].GridLayers,
                            function(value, key, list) {
                                var subnode = {
                                    text: value.Name,
                                    tags: [tag],
                                    type: type,
                                    code: value.Id,
                                    min: value.MinValue,
                                    max: value.MaxValue,
                                    node: value
                                };
                                node.nodes.push(subnode);
                            });
                    }


                    _.each(data,
                        function(value, key, list) {
                            var tag = value.gridDescription;
                            if (value.gridDescription.slice(0, 3) === "SSB") {
                                tag = value.gridDescription.substring(4);
                            }
                            var grid = {
                                text: value.gridDescription,
                                tags: [],
                                type: value.gridType,
                                code: "0"

                            };
                            if (!$.isEmptyObject(list[key].GridLayers)) {
                                buildGridTreeNodes(grid, list, key, value.gridType, tag);
                            }
                            tree.push(grid);
                        });

                    $gridtree = $('#gridtree')
                        .treeview({
                            data: tree,
                            multiSelect: false,
                            showTags: true,
                            enableTitles: true,
                            showIcon: false,
                            onNodeSelected: function(event, node) {
                                map.applyGrid(node);
                            },
                            onNodeUnselected: function(event, node) {
                                map.applyGrid("");
                            }
                        });

                });
        };
        vm.clearGrid = function() {
            var nodeId = $('#gridtree').treeview('getSelected', "");
            $('#gridtree').treeview('unselectNode', [nodeId, { silent: false }]);
            map.applyGrid("");
        };
        vm.clearSourceFilter = function() {
            $sourcetree.treeview('uncheckAll', { silent: false });
            application.filter.Institutions([]);
        };

        vm.buildTree = function(filter) {
            filter = filter || application.filter;
            vm.buildSourceTree(filter); // Update this in parallell

            var tree = [];
            dataServices.getAreaSummary(filter)
                .then(function(areaResult) {
                    dataServices.getNatureAreaSummary(filter)
                        .then(function(result) {

                            function makeRootNode(text, type, url, count) {
                                return {
                                    text: text,
                                    selectable: false,
                                    href: url || "javascript:void(0);",
                                    state: {
                                        expanded: false
                                    },
                                    nodes: [],
                                    tags: [count || 0],
                                    base: type
                                };
                            }

                            function makeNode(text, type, key, url, count) {
                                var subnode = {
                                    text: text,
                                    selectable: false,
                                    href: url,
                                    tags: [count],
                                    type: type,
                                    code: key,
                                    state: {
                                        checked: application.filter[type].indexOf(key) >= 0
                                    }
                                };
                                return subnode;
                            }

                            function wikiUrl(topic) {
                                return 'javascript:window.open("https:\/\/no.wikipedia.org\/wiki\/' +
                                    topic +
                                    '", "_blank");void(0);';
                            }

                            function openUrl(url) {
                                return url
                                    ? 'javascript:window.open("' + url + '", "_blank");void(0);'
                                    : "javascript:void(0);";
                            }

                            function conservationAreaFactSheetUrl(id) {
                                var padId = application.pad(id, 8);
                                return 'javascript:window.open("http:\/\/faktaark.naturbase.no\/Vern\/?id=VV' +
                                    padId +
                                    '", "_blank");void(0);';
                            }

                            function buildAreaTreeRecursive(node, list, key) {
                                node.nodes = [];
                                _.each(list[key].Areas,
                                    function(value, key, list) {
                                        var type = "Municipalities";
                                        var url = wikiUrl(value.Name);
                                        var subnode = makeNode(value.Name, type, key, url, value.NatureAreaCount);

                                        if (!$.isEmptyObject(list[key].Areas)) {
                                            buildAreaTreeRecursive(subnode, list, key);
                                        }
                                        node.nodes.push(subnode);
                                    });
                            }

                            function buildConservationAreaTreeRecursive(node, list, key) {
                                node.nodes = [];
                                _.each(list[key].Areas,
                                    function(value, key, list) {
                                        var type = "ConservationAreas";
                                        var url = conservationAreaFactSheetUrl(key);
                                        var subnode = makeNode(value.Name, type, key, url, value.NatureAreaCount);

                                        if (!$.isEmptyObject(list[key].Areas)) {
                                            buildConservationAreaTreeRecursive(subnode, list, key);
                                        }
                                        node.nodes.push(subnode);
                                    });
                            }

                            var fylker = makeRootNode("Administrative områder", "adm");
                            _.each(areaResult.Areas,
                                function(value, key, list) {
                                    var type = "Counties";
                                    var url = wikiUrl(value.Name);

                                    var county = makeNode(value.Name, type, key, url, value.NatureAreaCount);

                                    if (!$.isEmptyObject(list[key].Areas)) {
                                        buildAreaTreeRecursive(county, list, key);
                                    }
                                    fylker.nodes.push(county);
                                });
                            fylker.tags[0] = areaResult.AreaCount;
                            tree.push(fylker);

                            var conservationAreas = makeRootNode("Verneområder", "conservation");
                            _.each(areaResult.ConservationAreas,
                                function(value, key, list) {
                                    var type = "conservationCategory";
                                    var url = wikiUrl(value.Name);
                                    var node = makeRootNode(value.Name, type, url, value.NatureAreaCount);

                                    if (!$.isEmptyObject(list[key].Areas)) {
                                        buildConservationAreaTreeRecursive(node, list, key);
                                    }

                                    conservationAreas.nodes.push(node);
                                });
                            conservationAreas.tags[0] = areaResult.ConservationAreaCount;
                            tree.push(conservationAreas);

                            function buildTreeRecursive(node, list, key, type) {
                                node.nodes = [];
                                _.each(list[key].Codes,
                                    function(value, key, list) {
                                        var url = openUrl(value.Url);
                                        var subnode = makeNode(value.Name, type, key, url, value.Count);

                                        if (!$.isEmptyObject(list[key].Codes)) {
                                            buildTreeRecursive(subnode, list, key, type);
                                        }
                                        node.nodes.push(subnode);
                                    });
                            }

                            var nin = makeRootNode("Naturområdetyper", "nature");
                            _.each(result.NatureAreaTypes.Codes,
                                function(value, key, list) {
                                    var type = "NatureLevelCodes";
                                    var url = openUrl(value.Url);
                                    var root = makeNode(value.Name, type, key, url, value.Count);

                                    if (!$.isEmptyObject(list[key].Codes)) {
                                        buildTreeRecursive(root, list, key, "NatureAreaTypeCodes");
                                    }
                                    nin.nodes.push(root);
                                    nin.tags[0] = nin.tags[0] + value.Count;
                                });
                            tree.push(nin);

                            var descriptionVariables = makeRootNode("Miljøvariabler", "variables");
                            _.each(result.DescriptionVariables.Codes,
                                function(value, key, list) {
                                    var type = "DescriptionVariableCodes";
                                    var url = openUrl(value.Url);
                                    var root = makeNode(value.Name, type, key, url, value.Count);

                                    if (!$.isEmptyObject(list[key].Codes)) {
                                        buildTreeRecursive(root, list, key, type);
                                    }
                                    descriptionVariables.nodes.push(root);
                                    descriptionVariables.tags[0] = descriptionVariables.tags[0] + value.Count;
                                });
                            if (descriptionVariables.tags[0] > 0) {
                                tree.push(descriptionVariables);
                            }


                            function findRootNode(node) {
                                var root = node;
                                while (root.parentId !== undefined) {
                                    root = $('#summarytree').treeview(true).getNode(root.parentId);
                                }
                                return root;
                            }

                            function recursiveAddToFilter(node) {
                                for (var i in node.nodes) {
                                    if (node.nodes.hasOwnProperty(i)) {
                                        var child = node.nodes[i];
                                        recursiveAddToFilter(child);
                                    }
                                }
                                application.updateFilterNoDupe(true, node.type, node.code);
                            }

                            function recursiveCountNodes(node) {
                                var count = 0;
                                for (var i in node.nodes) {
                                    if (node.nodes.hasOwnProperty(i)) {
                                        var child = node.nodes[i];
                                        count = count + recursiveCountNodes(child);
                                    }
                                }
                                count++; // include self
                                return count;
                            }

                            function recursiveCountSelectedNodes(node) {
                                var count = 0;
                                for (var i in node.nodes) {
                                    if (node.nodes.hasOwnProperty(i)) {
                                        var child = node.nodes[i];
                                        if (child.state.checked) {
                                            count = count + recursiveCountSelectedNodes(child);
                                        }
                                    }
                                }
                                if (node.state.checked) {
                                    count++; // include self if checked
                                }
                                return count;
                            }

                            function recursiveRecreateFilter(node) {
                                var rootChecked = node.state.checked;

                                if (node.nodes) {

                                    var childNodesCount = recursiveCountNodes(node) - 1; // exclude self
                                    var childNodesChecked = recursiveCountSelectedNodes(node) - 1; // exclude self

                                    if ((rootChecked && (childNodesChecked === 0)) ||
                                        childNodesChecked === childNodesCount) {
                                        // Add all Sub-nodes to filter
                                        // enable to not send municipalities when all/none in a county selected
                                        //if ((node.type !== "Counties") && (node.base !== "adm") /*&& (node.base !== "conservation")*/) {
                                        recursiveAddToFilter(node);
                                        //}
                                    } else if (childNodesChecked > 0) {
                                        for (var i in node.nodes) {
                                            if (node.nodes.hasOwnProperty(i)) {
                                                var child = node.nodes[i];
                                                if (child.state.checked) {
                                                    application.updateFilterNoDupe(true, child.type, child.code);
                                                    recursiveRecreateFilter(child);
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            function clearFilterByType(type) {
                                switch (type) {
                                case "adm":
                                    application.removeFilter("Counties");
                                    application.removeFilter("Municipalities");
                                    break;
                                case "conservation":
                                    application.removeFilter("ConservationAreas");
                                    break;
                                case "nature":
                                    application.removeFilter("NatureLevelCodes");
                                    application.removeFilter("NatureAreaTypeCodes");
                                    break;
                                case "variables":
                                    application.removeFilter("DescriptionVariableCodes");
                                    break;
                                }
                            }

                            function uncheckAllRecursive(node, updateTree) {
                                for (var i in node.nodes) {
                                    if (node.nodes.hasOwnProperty(i)) {
                                        var child = node.nodes[i];
                                        uncheckAllRecursive(child, updateTree);
                                        if (child.state.checked) {
                                            if (updateTree) {
                                                $('#summarytree')
                                                    .treeview(true)
                                                    .uncheckNode(child.nodeId, { silent: true });
                                            }
                                            child.state.checked = false;
                                        }
                                    }
                                }
                            }

                            function checkParentsRecursive(node) {
                                var root = node;
                                while (root.parentId !== undefined) {
                                    if (!root.state.checked) {
                                        $('#summarytree').treeview(true).checkNode(root.nodeId);
                                    }
                                    root = $('#summarytree').treeview(true).getNode(root.parentId);
                                }
                                if (!root.state.checked) {
                                    $('#summarytree').treeview(true).checkNode(root.nodeId);
                                }
                                return root;
                            }

                            function simplifySelectionRecursive(tree) {
                                // unchecking implicitly selected nodes when loading filter from bookmark
                                _.forEach(tree,
                                    function(branch) {
                                        var selected = recursiveCountSelectedNodes(branch);
                                        if (branch.state && (selected > 0)) {
                                            branch.state.checked = true;
                                        }
                                        var total = recursiveCountNodes(branch);

                                        if (total === selected) {
                                            uncheckAllRecursive(branch, false);
                                        } else if (selected > 0) {
                                            simplifySelectionRecursive(branch.nodes);
                                        }
                                    });
                            }

                            simplifySelectionRecursive(tree);

                            // trenger denne utvidelsen for trunkering av lange tekster:
                            // https://github.com/jonmiles/bootstrap-treeview/pull/108
                            $summarytree = $('#summarytree')
                                .treeview({
                                    data: tree,
                                    showTags: true,
                                    enableLinks: true,
                                    enableTitles: true,
                                    showCheckbox: true,
                                    showIcon: false,
                                    onNodeChecked: function(event, node) {
                                        var root = checkParentsRecursive(node);
                                        clearFilterByType(root.base);
                                        recursiveRecreateFilter(root);
                                    },
                                    onNodeUnchecked: function(event, node) {
                                        var root = findRootNode(node);
                                        uncheckAllRecursive(node, true);
                                        clearFilterByType(root.base);
                                        recursiveRecreateFilter(root);
                                    }
                        });
                        });
                });
        };
        vm.search = function(e) {
            var pattern = $('#input-search').val();
            var options = {
                ignoreCase: true, //$('#chk-ignore-case').is(':checked'),
                exactMatch: false, //$('#chk-exact-match').is(':checked'),
                revealResults: true //$('#chk-reveal-results').is(':checked')
            };
            $summarytree.treeview('search', [pattern, options]);

            if (pattern === '') {
                $('#summarytree li.hide').removeClass('hide');
            } else {
                $('#summarytree li').not('.search-result').addClass('hide');
            }
        };

        vm.searchGrid = function(e) {
            var pattern = $('#input-searchGrid').val();
            var options = {
                ignoreCase: true,
                exactMatch: false,
                revealResults: true
            };
            $gridtree.treeview('search', [pattern, options]);

            if (pattern === '') {
                $('#gridtree li.hide').removeClass('hide');
            } else {
                $('#gridtree li').not('.search-result').addClass('hide');
            }
        };

        application.rebuildTree.subscribe(function (value) {
            if (value) {
                vm.buildTree();
                application.rebuildTree(false);
            }
        });

        vm.collapseNodes = function () {
            $summarytree.treeview('collapseAll', { silent: true });
        };

        vm.clearCheckedNodes = function () {
            $summarytree.treeview('uncheckAll', { silent: false });
            application.filter.NatureLevelCodes([]);
            application.filter.NatureAreaTypeCodes([]);
            application.filter.DescriptionVariableCodes([]);
            application.filter.Municipalities([]);
            application.filter.Counties([]);
            vm.collapseNodes();
            vm.hiddenFilterOptions(false);
        };

        vm.showCheckedNodes = function () {
            $summarytree.treeview('expandAll', { silent: true });
            $('#summarytree li.hide').removeClass('hide');
            $('#summarytree li').not('.node-checked').addClass('hide');

            vm.hiddenFilterOptions(vm.hasNodesSelected() && ($('#summarytree li').not('.hide').length === 0));
        };
        vm.selectedConservationArea.subscribe(function (value) {
            if (value) {
                return vm.getAreaPolygon(value.type(), value.nr());    // ConservationArea == 3
            }
            return null;
        });

        vm.selectedAdmArea.subscribe(function (value) {
            if (value) {
                return vm.getAreaPolygon(value.type(), value.nr()); // Municipality == 1
            }
            return null;
        });

        vm.getAreaPolygon = function(areaType, nr) {
            return dataServices.getAreas(areaType, nr).then(function (geojsonObject) {
                if (map.selectionLayer) {
                    map.selectionLayer.getSource().addFeatures(
                        new ol.format.GeoJSON({ defaultDataProjection: "EPSG: 32633" }).
                        readFeatures(geojsonObject, { dataProjection: "EPSG: 32633" })
                    );
                } else {
                    // Hack! If map has not been initialized (example: refresh on list view), update filter instead of polygon
                    application.updateFilter(true, "Municipalities", nr);
                }
            });
        };

        vm.setCurrentView = function () {
            //vm.clearPolygon();
            var wktObject = map.getBoundingBox();
            if ((wktObject.length > 0) && (wktObject !== "POLYGON EMPTY")) {
                map.selectionLayer.getSource().addFeatures(
                    new ol.format.WKT({ defaultDataProjection: "EPSG: 32633" }).
                    readFeatures(wktObject, { dataProjection: "EPSG: 32633" })
                );
            } else {
                console.debug("Ingen boundingbox funnet");
            }
        };
        vm.clearPolygon = function () {
            if (map.selectionLayer) {
                map.selectionLayer.getSource().clear();
            }
            application.filter.Geometry("");
            app.trigger('currentSelection:remove');

            vm.hiddenFilterOptions(false);
        };
        vm.zoomToPolygon = function () {
            map.fitSelectionPolygon();
        };

        vm.selectedMatrikkelEnhet.subscribe(function (value) {
            if (value === null) return undefined;
            var params = value.nr().match(/(\d+)/g);
            if (params.length >= 3) {
                return dataServices.hentOmraadeForMatrikkelenhet.apply(this, params).then(function (wktObject) {
                    if (wktObject !== "POLYGON EMPTY") {
                        map.selectionLayer.getSource().addFeatures(
                            new ol.format.WKT({ defaultDataProjection: "EPSG: 32633" }).
                            readFeatures(wktObject, { dataProjection: "EPSG: 32633" })
                        );
                    } else {
                        console.debug("Ingen geometri funnet for " + params);
                    }
                });
            }
            return undefined;
        });

        vm.polygon = ko.observable();
        vm.addPolygon = function (polygon) {
            vm.polygon(polygon);
        };
        vm.removePolygon = function () {
            vm.polygon(undefined);
        };
        vm.drawPolygonActive = ko.observable(false);

        vm.togglePolygon = function () {
            if (vm.drawPolygonActive()) {
                map.deactivateDrawPolygon();
            } else {
                map.activateDrawPolygon();
            }
            vm.drawPolygonActive(!vm.drawPolygonActive());
        };
        vm.buttonLabel = ko.computed(function () {
            return vm.drawPolygonActive() ? "Avbryt" : "Tegn i kart";
        });

        vm.hiddenFilterOptions = ko.observable(false);

        app.on('drawPolygon:deactivate').then(function () {
            vm.drawPolygonActive(false);
        });

        return vm;
    });

define(['durandal/app', 'services/createPagedList', 'lodash', 'knockout', 'services/config', 'services/dataServices', 'services/application', 'viewmodels/listToolbar', 'viewmodels/nav', 'services/codeLists'],
    function (app, pagedList, _, ko, config, dataServices, application, toolbar, nav, codelists) {

        ko.components.register('chart-widget', {
            viewModel: function (params) {

                var w = 80;
                var h = 80;
                var r = h / 2;

                var color = d3.scale.category20();

                var natureTypes = params.natureTypes;
                var natureTypeShares = params.natureTypeShares;
                var element = params.element;

                var chart = d3.select(element);

                var vis = chart.append("svg").data([natureTypeShares]).attr("width", w).attr("height", h).append("svg:g").attr("transform", "translate(" + r + "," + r + ")");
                var pie = d3.layout.pie().value(function(d) {
                     return d.value;
                });

                // declare an arc generator function
                var arc = d3.svg.arc().outerRadius(r);

                // select paths, use arc generator to draw
                var arcs = vis.selectAll("g.slice").data(pie).enter().append("svg:g").attr("class", "slice");

                arcs.append("svg:path")
                    .attr("fill", function(d, i) {
                        return color(i);
                    })
                    .attr("class", "arc")
                    .attr("d", function(d) {
                        return arc(d);
                    })
                    .append("title").text(function (d, i) {
                        return natureTypes[i].code + " (" + d.value * 100 + "%)";
                    });
            },
            template:
                    '<span class="natureAreaChart"></span>'
        });

        ko.components.register('legend-widget', {
            viewModel: function (params) {
                var color = d3.scale.category20();
                var natureTypes = params.natureTypes;
                var level = params.level;
                var element = params.element;
                var legends = d3.select(element);

                for (var i = 0; i < natureTypes.length; ++i) {
                    var code, description, codeUrl;
                    if (level === 'main') {
                        code = natureTypes[i].mainTypeCode;
                        description = natureTypes[i].mainTypeDescription;
                        codeUrl = natureTypes[i].mainTypeCodeUrl;
                    } else { // if (level === 'sub') {
                        code = natureTypes[i].code;
                        description = natureTypes[i].description;
                        codeUrl = natureTypes[i].codeUrl;
                    }

                    legends.append("svg").attr("width", 25).attr("height", 15).append("rect").attr("x", 0).attr("y", 0).attr("width", 15).attr("height", 15).style("fill", color(i)).append("title").text(code);
                    legends.append("a").text(description).attr("href", codeUrl).attr("target", "_blank");

                    if (i !== natureTypes.length - 1) {
                        legends.append("br");
                    }
                }
            },
            template:
                   '<span class="natureAreaLegend"></span>'
        });

        ko.components.register('variable-widget', {
            viewModel: function (params) {
                var color = d3.scale.category20();
                var natureTypes = params.natureTypes;
                var element = params.element;
                var legends = d3.select(element);

                for (var i = 0; i < natureTypes.length; ++i) {
                    var natureTypeCode = natureTypes[i].code;
                    if (natureTypes[i].additionalVariables.length > 0) {
                        legends.append("svg").attr("width", 25).attr("height", 15).append("rect").attr("x", 0).attr("y", 0).attr("width", 15).attr("height", 15).style("fill", color(i)).append("title").text("Variabler tilknyttet " + natureTypeCode);
                        var codeUrl, codeAndValue, j;
                        for (j = 0; j < natureTypes[i].additionalVariables.length; ++j) {
                            codeAndValue = natureTypes[i].additionalVariables[j].codeAndValue;
                            codeUrl = natureTypes[i].additionalVariables[j].url || "javascript:void(0);"; // "http://www.artsdatabanken.no/" + code + "_" + value;
                            legends.append("a").text(codeAndValue).attr("href", codeUrl).attr("target", "_blank");
                        }
                        legends.append("br");

                        for (j = 0; j < natureTypes[i].customVariables.length; ++j) {
                            codeAndValue = natureTypes[i].customVariables[j].codeAndValue;
                            legends.append("span").text(codeAndValue);
                        }
                        legends.append("br");
                    }
                }
            },
            template:
                   '<span class="natureAreaLegend"></span>'
        });

        var storePopup = function() {
            // Store popover-content in #myPopoverContentContainer so bindings are not lost!
            var popElement = $('#listPopup');
            if (popElement
                .length >
                0) { // Store popover-content in #myPopoverContentContainer so bindings are not lost!
                // Store popover-content in #myPopoverContentContainer so bindings are not lost!
                $("#myPopoverContentContainer").append($("#natureAreaInfo"));
                $("#myPopoverContentContainer").append(popElement);
                $(popElement).popover("destroy");
            }
        };

        var getListData = function (page) {
            var maxListItems = ($('body').width() > 481) ? application.config.maxListItems : application.config.maxMobileListItems;
            var listFilter = application.listFilter();
            listFilter.IndexFrom = maxListItems * page + 1;
            listFilter.IndexTo = maxListItems + maxListItems * page;
            var promise = dataServices.getNatureAreaInfosBySearchFilter(listFilter /*, page*/),
                    result = promise.then(function (featureData) {
                        var resultList = [];
                        var totalPages = 1;
                        if (featureData !== undefined && featureData !== null) {
                            toolbar.totalListItems(featureData.NatureAreaCount || 0);

                            totalPages = Math.ceil(toolbar.totalListItems() / maxListItems);
                            for (var i = 0; i < Math.min(maxListItems, featureData.natureAreas.length); i++) {
                                var feature = featureData.natureAreas[i];
                                feature.natureTypes = [];
                                feature.natureTypeShares = [];
                                feature.natureAreaDescriptionVariables = [];

                                //avoid binding-crash if values are missing:
                                feature.surveyedYear = feature.surveyedYear || 'Ukjent';
                                feature.contractor = feature.contractor || 'Ukjent';
                                feature.surveyer = feature.surveyer || 'Ukjent';
                                feature.program = feature.program || 'Ukjent';

                                for (var j = 0; j < feature.parameters.length; ++j) {
                                    if (feature.parameters[j].share !== undefined) {
                                        var additionalVariables = [];
                                        var customVariables = [];
                                        var k;
                                        for (k = 0; k < feature.parameters[j].additionalVariables.length; ++k) {
                                            additionalVariables.push({
                                                "code": feature.parameters[j].additionalVariables[k].Code,
                                                "value": feature.parameters[j].additionalVariables[k].Value,
                                                "codeAndValue":
                                                    feature.parameters[j].additionalVariables[k].codeDescription +
                                                        " (" +
                                                        feature.parameters[j].additionalVariables[k].code +
                                                        "), verdi: " +
                                                        feature.parameters[j].additionalVariables[k].value
                                            });
                                        }
                                        for (k = 0; k < feature.parameters[j].customVariables.length; ++k) {
                                            var description = feature.parameters[j].customVariables[k].description;
                                            var code = feature.parameters[j].customVariables[k].specification;
                                            var value = feature.parameters[j].customVariables[k].value;
                                            var codeAndValue =
                                                description
                                                    ? (description + " (" + code + "), verdi: " + value)
                                                    : (code + ": " + value);
                                            customVariables.push({
                                                "codeAndValue": codeAndValue
                                            });
                                        }
                                        feature.natureTypes.push({
                                            "code": feature.parameters[j].code,
                                            "description": feature.parameters[j].codeDescription,
                                            "codeUrl": feature.parameters[j].codeUrl,
                                            "mainTypeCode": feature.parameters[j].mainTypeCode,
                                            "mainTypeDescription": feature.parameters[j].mainTypeDescription,
                                            "mainTypeCodeUrl": feature.parameters[j].mainTypeCodeUrl,
                                            "additionalVariables": additionalVariables,
                                            "customVariables": customVariables
                                        });
                                        feature.natureTypeShares.push({
                                            "value": feature.parameters[j].share
                                        });
                                    } else {
                                        feature.natureAreaDescriptionVariables.push({
                                            "codeAndValue":
                                                feature.parameters[j].codeDescription +
                                                    " (" +
                                                    feature.parameters[j].code +
                                                    "), verdi: " +
                                                    feature.parameters[j].value,
                                            "url": feature.parameters[j].codeUrl,
                                            "description": feature.parameters[j].description
                                        });
                                    }
                                }

                                resultList.push(feature);
                            }
                        }
                        return {
                            itemList: resultList,
                            totalPages: totalPages
                        };
                    });
                return result;
            },
            vm = pagedList(getListData);
        vm.title = "FeatureList";
        vm.totalListItems = toolbar.totalListItems;
        vm.maxListItems = ko.computed(function () {
    // Check for mobile and limit nr of rows to 3 if on mobile
            if ($('body').width() > 481) {
                return Math.min(application.config.maxListItems, toolbar.totalListItems());
            }
            return Math.min(application.config.maxMobileListItems, toolbar.totalListItems());
}, this);

        application.filterChanged.subscribe(function (value) {
            if (nav.activeView() === 'list') {
                vm.updateSelection();
            }
        });
        vm.showDetails = function(index, feature, event, position, container) {
            var popElement = $('#listPopup');
            //if (!popElement.length>0) {    // make it if it don't already exist
            //    popElement = $('<div id="listPopup" data-toggle="popover"></div>');
            //}
            if (feature && feature.LocalId) {
                $(event.currentTarget).before(popElement);
                dataServices.getNatureAreaByLocalId(feature.LocalId)
                    .then(function(data) {
                        dataServices.getMetadataByNatureAreaLocalId(feature.LocalId)
                            .then(function(metadata) {

                                if ((screen.width > 481) && (screen.height > 481)) {
                                    $(popElement)
                                        .popover({
                                            container: container || false,
                                            placement: 'auto ' + position,
                                            content: function() {
                                                // Retrieve content from the hidden #myPopoverContentContainer
                                                return $("#natureAreaInfo");
                                            },
                                            html: true,
                                            template: '<div class="popover" style="max-width: 600px;">' +
                                                '<button type="button" title="Lukk" class="close" onclick="$(&quot;#myPopoverContentContainer&quot;).append($(&quot;#natureAreaInfo&quot;));$(&quot;#listPopup&quot;).popover(&quot;destroy&quot;);"><span aria-hidden="true">×</span><span class="sr-only">Close</span></button>' +
                                                '<div class="arrow"></div>' +
                                                '<div class="popover-content" style="width: 600px; height: 320px;">' +
                                                '</div>' +
                                                '</div>'
                                        });

                                    $(popElement).popover('show');
                                    var popoverElement = $("#" + $(popElement).attr("aria-describedby"));
                                    if (popoverElement) {
                                        index === 0
                                            ? popoverElement.addClass("firstPopover")
                                            : popoverElement.removeClass("firstPopover");
                                    }

                                    //$(popElement).on('hide.bs.popover', function() {
                                    //    // Store popover-content in #myPopoverContentContainer so bindings are not lost!
                                    //    $("#myPopoverContentContainer").append($("#natureAreaInfo"));
                                    //    $("#myPopoverContentContainer").append($("#listPopup"));
                                    //});

                                    $(popElement)
                                        .on('hidden.bs.popover',
                                            function() {
                                                // Store popover-content in #myPopoverContentContainer so bindings are not lost!
                                                $("#myPopoverContentContainer").append($("#natureAreaInfo"));
                                                $("#myPopoverContentContainer").append($("#listPopup"));
                                            });

                                    application.currentFeature(data, metadata);

                                    app.trigger("currentFeatureChanged:trigger");
                                } else {
                                    // Mobile
                                    application.currentFeature(data, metadata);

                                    app.trigger("currentFeatureChanged:trigger");
                                    nav.navigateTo("details/" +
                                        application.viewportState.center() +
                                        "/" +
                                        application.viewportState.zoom() +
                                        "/background/" +
                                        application.viewportState.background());
                                }
                            });
                    });
            } else {
                storePopup();
                vm.overlay.setPosition(undefined);
            }
            app.on('mapview:activate')
                .then(function() {
                    storePopup();
                });
            app.on('factsheet:activate')
                .then(function() {
                    storePopup();
                });
            app.on('listview:pagechange')
                .then(function() {
                    storePopup();
                });
            app.on('app:toggleFullscreen')
                .then(function() {
                    if (nav.activeView() === 'list') {
                        storePopup();
                    }
                });
        };
        return vm;
    });

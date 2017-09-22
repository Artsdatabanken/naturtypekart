define(['services/logger', "knockout", 'durandal/app', 'services/config', 'services/application', 'services/dataServices', 'viewmodels/nav', 'd3'],
    function(logger, ko, app, config, application, dataServices, nav, d3) {
        var title = "statistics",
            vm = {
                activate: function (center, zoom, background, id, filter) {
                    //logger.log(title + ' View Activated', null, title, true)

                    application.viewportState.center(center);
                    application.viewportState.zoom(zoom);
                    application.viewportState.background(background);
                    application.parseUrlFilter(filter);

                    vm.mainWidth($('#main').width() - 20);
                    vm.mainHeight($('#main').height() - 20);

                    vm.getSummary();
                },

                totalAreas: ko.observable(0),
                loadingAreas: ko.observable(false),
                mainWidth: ko.observable(660),
                mainHeight: ko.observable(900),

                getSummary: function (filter) {
                    filter = filter || application.filter;
                    vm.loadingAreas(true);
                    //vm.totalAreas(0); // Enable to hide while generating new
                    dataServices.getNatureAreaStatisticsBySearchFilter(filter).then(function(result) {
                        $("#sourceChart").empty();
                        $("#yearChart").empty();
                        $("#mainTypeChart").empty();
                        $("#typeChart").empty();
                        vm.totalAreas(0);

                        for (var i = 0; i < result.SurveyYears.length; i++) {
                            vm.totalAreas(vm.totalAreas() + result.SurveyYears[i].NatureAreaCount);
                        }

                        vm.makeSourceChart(result.Institutions, "#sourceChart");
                        vm.makeYearChart(result.SurveyYears, "#yearChart");

                        var mainTypeData = [];
                        var subTypeData = [];
                        _.forEach(result.NatureAreaTypes.Codes, function(level1) {
                            _.forEach(level1.Codes, function(level2) {
                                mainTypeData.push({
                                    level: level1.Name,
                                    type: level2.Name,
                                    count: level2.Count
                                });
                                _.forEach(level2.Codes, function(level3) {
                                    subTypeData.push({
                                        base: level1.Name,
                                        level: level2.Name,
                                        type: level3.Name,
                                        count: level3.Count
                                    });
                                });

                            });
                        });

                        vm.makeTypeChart(mainTypeData, "#mainTypeChart");
                        vm.makeTypeChart(subTypeData, "#typeChart");
                        vm.loadingAreas(false);

                    });
                },

                makeYearChart: function (dataset, element) {
                    dataset.sort(function (a, b) {
                        return a.Year - b.Year;
                    });

                    var colorScale = d3.scale.linear()
                        .range(['lightblue', 'darkblue']);

                    var margin = { top: 20, right: 20, bottom: 30, left: 40 },
                        width = vm.mainWidth() - margin.left - margin.right,
                        height = Math.min(vm.mainHeight() - 150, 650) - margin.top - margin.bottom;
                    height = height < 0 ? 150 : height;

                    var x = d3.scale.ordinal()
                        .rangeRoundBands([0, width], 0.1);

                    var y = d3.scale.linear()
                        .range([height, 0]);

                    var xAxis = d3.svg.axis()
                        .scale(x)
                        .orient("bottom");

                    var yAxis = d3.svg.axis()
                        .scale(y)
                        .orient("left");

                    var svg = d3.select(element).append("svg")
                        .attr("width", width + margin.left + margin.right)
                        .attr("height", height + margin.top + margin.bottom)
                        .append("g")
                        .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

                    x.domain(dataset.map(function(d) {
                         return d.Year;
                    }));
                    y.domain([0, d3.max(dataset, function(d) {
                         return d.NatureAreaCount;
                    })]);
                    colorScale.domain([0, d3.max(dataset, function(d) {
                         return d.NatureAreaCount;
                    })]);

                    svg.append("g")
                        .attr("class", "x axis")
                        .attr("transform", "translate(0," + height + ")")
                        .call(xAxis);

                    svg.append("g")
                        .attr("class", "y axis")
                        .call(yAxis)
                        .append("text")
                        .attr("transform", "rotate(-90)")
                        .attr("y", 6)
                        .attr("dy", ".71em")
                        .style("text-anchor", "end")
                        .text("Antall")
                        .append("title").text("Antall naturområder");

                    var myBar = svg.selectAll(".bar")
                        .data(dataset)
                        .enter().append("rect")
                        .attr("class", "bar")
                        .attr("x", function(d) {
                             return x(d.Year);
                        })
                        .attr("width", x.rangeBand())
                        .attr("y", function(d) {
                             return y(0);
                        })
                        .attr("height", function(d) {
                             return height - y(0);
                        })
                        .attr("fill", function(d) {
                            return colorScale(d.NatureAreaCount);
                        });
                    myBar
                        .transition()
                        .attr("height", function(d) {
                             return height - y(d.NatureAreaCount);
                        })
                        .attr("y", function(d) {
                             return y(d.NatureAreaCount);
                        })
                        .duration(500);

                    myBar.append("title").text(function (d, i) {
                        return "Kartlagt i " + d.Year + ": " + Math.round(d.NatureAreaCount * 100 / vm.totalAreas()) + "% av utvalg";
                    });
                },

                makeSourceChart: function(dataset, element) {
                    var legendCircleSize = 15;
                    var legendSpacing = 15;
                    var r = vm.mainWidth() < 481 ? 50 : 150;

                    var color = d3.scale.category20();

                    var svg = d3.select(element)
                        .append("svg")
                        .attr("width", vm.mainWidth())
                        .attr("height", 2*r+20)
                        .append("g")
                        .attr("transform", "translate(" + r + "," + r + ")");

                    // declare an arc generator function
                    var arc = d3.svg.arc()
                        .outerRadius(r);

                    var pie = d3.layout.pie()
                        .value(function(d) {
                             return d.NatureAreaCount;
                        })
                        .sort(null);

                    // select paths, use arc generator to draw
                    var path = svg.selectAll("path")
                        .data(pie(dataset))
                        .enter()
                        .append("path")
                        .attr("d", arc)
                        .attr("class", "arc")
                        .attr("fill", function(d, i) {
                            return color(d.data.Name);
                        }).append("title").text(function(d, i) {
                            return d.data.Name + " " + Math.round(d.data.NatureAreaCount * 100 / vm.totalAreas()) + "% av utvalg";
                        });

                    var legend = svg.selectAll('.legend')
                        .data(color.domain())
                        .enter()
                        .append('g')
                        .attr('class', 'legend')
                        .attr('transform', function(d, i) {
                            var height = 20 + legendSpacing;
                            var offset = height * color.domain().length / 2;
                            var horz = r+50;
                            var vert = i * height - offset;
                            return 'translate(' + horz + ',' + vert + ')';
                        });

                    legend.append('circle')
                        .attr('cx', 0)
                        .attr('cy', 0)
                        .attr('r', legendCircleSize)
                        .style('fill', color)
                        .style('stroke', color);

                    legend.append('text')
                        .attr('x', legendCircleSize + legendSpacing)
                        .attr('y', legendCircleSize - legendSpacing)
                        .text(function(d) {
                             return d;
                        });

                },

                wrap: function (text, width) {
                    text.each(function () {
                        var text = d3.select(this),
                            words = text.text().split(/\s+/).reverse(),
                            word,
                            line = [],
                            lineNumber = 0,
                            lineHeight = 1.1, // ems
                            y = text.attr("y"),
                            dy = parseFloat(text.attr("dy")),
                            tspan = text.text(null).append("tspan").attr("x", -10).attr("y", y).attr("dy", dy + "em");
                        while (word = words.pop()) {
                            line.push(word);
                            tspan.text(line.join(" "));
                            if (tspan.node().getComputedTextLength() > width) {
                                line.pop();
                                tspan.text(line.join(" "));
                                line = [word];
                                tspan = text.append("tspan").attr("x", -10).attr("y", y).attr("dy", ++lineNumber * lineHeight + dy + "em").text(word);
                            }
                        }
                    });
                },

                makeTypeChart: function (dataset, element) {
                    dataset.sort(function (a, b) {
                        if (a.level === b.level) {
                            return a.count > b.count;
                        }
                        return a.level > b.level;
                    });

                    var color = d3.scale.category20();

                    var legendWidth = 300;
                    var chartHeight = 800;
                    var margin = { top: 20, right: 20, bottom: 30, left: 40 },
                        width = vm.mainWidth() - legendWidth - margin.left - margin.right,
                        height = chartHeight - margin.top - margin.bottom;


                    var y0 = d3.scale.ordinal()
                        .rangeRoundBands([0, height], 0.1);

                    var y1 = d3.scale.ordinal()
                        .rangeRoundBands([0, height], 0.1);

                    var x = d3.scale.linear()
                        .range([0, width]);

                    var yAxis0 = d3.svg.axis()
                        .scale(y0)
                        .orient("left");
                    var yAxis1 = d3.svg.axis()
                        .scale(y1)
                        .orient("left");

                    var xAxis = d3.svg.axis()
                        .scale(x)
                        .orient("top");

                    var svg = d3.select(element).append("svg")
                        .attr("width", width + margin.left + margin.right + legendWidth)
                        .attr("height", height + margin.top + margin.bottom)
                        .append("g")
                        .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

                    y0.domain(dataset.map(function(d) {
                         return d.type;
                    }));
                    y1.domain(dataset.map(function(d) {
                         return d.level;
                    }));

                    x.domain([0, d3.max(dataset, function(d) {
                         return d.count;
                    })]);

                    svg.append("g")
                        .attr("class", "y axis")
                        .attr("transform", "translate(" + legendWidth + ",0)")
                        .call(yAxis0)
                        .selectAll(".tick text")
                        .call(vm.wrap, y0.rangeBand());
                        //.selectAll("text")
                        //.append("title").text("Gjeldende nivå");
                    // todo: se http://bl.ocks.org/mbostock/7555321 for text-wrap

                    svg.append("g")
                        .attr("class", "y axis")
                        .attr("transform", "translate(20,0)")
                        .call(yAxis1)
                        //.append("text")
                        //.attr("transform", "rotate(-90)")
                        //.attr("y", 6)
                        //.attr("dy", ".71em")
                        //.style("text-anchor", "end")
                        //.text("Nivå")
                        .selectAll("text")
                        .style("text-anchor", "end")
                        .attr("dy", "-1.0em")
                        .attr("dx", "4.0em")
                        .attr("transform", "rotate(-90)")
                        .append("title").text("Overordnet nivå");

                    svg.append("g")
                        .attr("class", "x axis")
                        .attr("transform", "translate(" + legendWidth + ",0)")
                        .call(xAxis)
                        .append("text")
                        .attr("transform", "translate(-50,-10)")
                        .attr("x", 6)
                        .attr("dx", ".71em")
                        .style("text-anchor", "end")
                        .text("Antall")
                        .append("title").text("Antall naturområder");

                    var myBar = svg.selectAll(".bar")
                        .data(dataset)
                        .enter().append("rect")
                        .attr("transform", "translate(" + legendWidth + ",0)")
                        .attr("class", "bar")
                        .attr("y", function(d) {
                             return y0(d.type);
                        })
                        .attr("height", y0.rangeBand())
                        .attr("x", 0)
                        .attr("width", function(d) {
                             return x(0);
                        })
                        .attr("fill", function(d) {
                             return color(d.level);
                        });

                    myBar
                        .transition()
                        .attr("width", function(d) {
                             return x(d.count);
                        })
                        .duration(500);

                    myBar.append("title").text(function (d, i) {
                        return (d.base ? d.base + " - " : "") + d.level + " - " + d.type + " - " + Math.round(d.count * 100 / vm.totalAreas()) + "% av utvalg";
                    });
                },

                compositionComplete: function() {
                    application.filterChanged.subscribe(function (value) {
                        if (nav.activeView() === 'statistics') {
                            vm.getSummary();
                        }
                    });
                    app.on('main:resized').then(function(elem) {
                        vm.mainWidth($('#main').width() - 20);
                        vm.mainHeight($('#main').height() - 20);
                        vm.getSummary();
                    });
                }                
            };
        return vm;
    });

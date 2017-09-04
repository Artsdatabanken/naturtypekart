define(['services/adbFuncs', 'd3'],
    function (adbfuncs, d3) {


        var color = d3.scale.category20();

        return function (chartElement, legendElement, natureTypes, natureTypeShares, width) {
            var legends = d3.select(legendElement);
            var chart = d3.select(chartElement);
            var w = width || Math.max(natureTypes.length * 20, 80);
            var h = w;

            var r = h / 2;

            for (var i = 0; i < natureTypes.length; ++i) {
                legends.append("svg").attr("width", 25).attr("height", 15).append("rect").attr("x", 0).attr("y", 0).attr("width", 15).attr("height", 15).style("fill", color(i)).append("title").text(natureTypes[i].code);
                legends.append("span").text(adbfuncs.firstToUpperCase(natureTypes[i].description));
                if (i !== natureTypes.length - 1) {
                    legends.append("br");
                }
            }

            var vis = chart.append("svg").data([natureTypeShares]).attr("width", w).attr("height", h).append("svg:g").attr("transform", "translate(" + r + "," + r + ")");
            var pie = d3.layout.pie().value(function(d) {
                 return d.value;
            });

            // declare an arc generator function
            var arc = d3.svg.arc().outerRadius(r);

            // select paths, use arc generator to draw
            var arcs = vis.selectAll("g.slice").data(pie).enter().append("svg:g").attr("class", "slice");

            arcs.append("svg:path")
                .attr("fill", function (d, i) {
                    return color(i);
                })
                .attr("class", "arc")
                .attr("d", function (d) {
                    return arc(d);

                }).append("title").text(function (d, i) {
                    return natureTypes[i].code;
                });

        };
    });

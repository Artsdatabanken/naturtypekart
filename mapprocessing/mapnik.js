var mapnik = require("mapnik");
var fs = require("fs");

//mapnik.register_default_fonts();
mapnik.register_default_input_plugins();

var map = new mapnik.Map(2048, 2048);
map.load("./stylesheet.xml", function(err, map) {
    map.zoomAll();
    var im = new mapnik.Image(2048, 2048);
    map.render(im, function(err, im) {
        im.encode("png", function(err, buffer) {
            fs.writeFile("map.png", buffer);
         });
    });
});
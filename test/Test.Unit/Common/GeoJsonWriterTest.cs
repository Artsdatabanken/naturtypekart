using System.Collections.Generic;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Nin.Common;
using Nin.GeoJson;
using NUnit.Framework;

namespace Test.Unit.Common
{
    public class GeoJsonWriterTest
    {
        [Test]
        public void PointToGeoJsonTest()
        {
            var point = new Point(new GeographicPosition(150.555, -10.666));
            var featureCollection = new FeatureCollection(new List<Feature> {new Feature(point)});
            var featureCollectionJson = GeoJsonWriter.ToGeoJson(featureCollection);
            Assert.True(featureCollectionJson.Contains("\"type\":\"Point\""));
        }

        [Test]
        public void MultiPointToGeoJsonTest()
        {
            var multipoint = new MultiPoint(new List<Point>
            {
                new Point(new GeographicPosition(10, 40)),
                new Point(new GeographicPosition(40, 30)),
                new Point(new GeographicPosition(20, 20)),
                new Point(new GeographicPosition(30, 10))
            });
            var featureCollection = new FeatureCollection(new List<Feature> { new Feature(multipoint) });
            var featureCollectionJson = GeoJsonWriter.ToGeoJson(featureCollection);
            Assert.True(featureCollectionJson.Contains("\"type\":\"MultiPoint\""));
        }

        [Test]
        public void LineStringToGeoJsonTest()
        {
            var linestring = new LineString(new List<GeographicPosition>
            {
                new GeographicPosition(30, 10),
                new GeographicPosition(31, 11),
                new GeographicPosition(32, 12)
            });
            var featureCollection = new FeatureCollection(new List<Feature> { new Feature(linestring) });
            var featureCollectionJson = GeoJsonWriter.ToGeoJson(featureCollection);
            Assert.True(featureCollectionJson.Contains("\"type\":\"LineString\""));
        }

        [Test]
        public void MultiLineStringToGeoJsonTest()
        {
            var multilinestring = new MultiLineString(
                new List<LineString>
                {
                    new LineString(
                        new List<GeographicPosition>
                        {
                            new GeographicPosition(10, 10),
                            new GeographicPosition(20, 20),
                            new GeographicPosition(10, 40)
                        }
                    ),
                    new LineString(
                        new List<GeographicPosition>
                        {
                            new GeographicPosition(40, 40),
                            new GeographicPosition(30, 30),
                            new GeographicPosition(40, 40),
                            new GeographicPosition(30, 10)
                        }
                    )
                }
            );
            var featureCollection = new FeatureCollection(new List<Feature> { new Feature(multilinestring) });
            var featureCollectionJson = GeoJsonWriter.ToGeoJson(featureCollection);
            Assert.True(featureCollectionJson.Contains("\"type\":\"MultiLineString\""));
        }

        [Test]
        public void PolygonToGeoJsonTest()
        {
            var polygon = new Polygon(
            new List<LineString>
            {
                    new LineString(
                        new List<GeographicPosition>
                        {
                            new GeographicPosition(35, 10),
                            new GeographicPosition(45, 45),
                            new GeographicPosition(15, 40),
                            new GeographicPosition(10, 20),
                            new GeographicPosition(35, 10)
                        }
                    ),
                    new LineString(
                        new List<GeographicPosition>
                        {
                            new GeographicPosition(20, 30),
                            new GeographicPosition(35, 35),
                            new GeographicPosition(30, 20),
                            new GeographicPosition(20, 30)
                        }
                    )
                }
            );
            var featureCollection = new FeatureCollection(new List<Feature> { new Feature(polygon) });
            var featureCollectionJson = GeoJsonWriter.ToGeoJson(featureCollection);
            Assert.True(featureCollectionJson.Contains("\"type\":\"Polygon\""));
        }

        [Test]
        public void MultiPolygonToGeoJsonTest()
        {
            var multipolygon = new MultiPolygon(
                new List<Polygon>
                {
                    new Polygon(
                        new List<LineString>
                        {
                            new LineString(
                                new List<GeographicPosition>
                                {
                                    new GeographicPosition(40, 40),
                                    new GeographicPosition(20, 45),
                                    new GeographicPosition(45, 30),
                                    new GeographicPosition(40, 40)
                                }
                            ),
                            new LineString(
                                new List<GeographicPosition>
                                {
                                    new GeographicPosition(20, 35),
                                    new GeographicPosition(10, 30),
                                    new GeographicPosition(10, 10),
                                    new GeographicPosition(30, 5),
                                    new GeographicPosition(45, 20),
                                    new GeographicPosition(20, 35)
                                }
                            )
                        }
                    ),
                    new Polygon(
                        new List<LineString>
                        {
                            new LineString(
                                new List<GeographicPosition>
                                {
                                    new GeographicPosition(30, 20),
                                    new GeographicPosition(20, 15),
                                    new GeographicPosition(20, 25),
                                    new GeographicPosition(30, 20)
                                }
                            )
                        }
                    )
                }
            );
            var featureCollection = new FeatureCollection(new List<Feature> { new Feature(multipolygon) });
            var featureCollectionJson = GeoJsonWriter.ToGeoJson(featureCollection);
            Assert.True(featureCollectionJson.Contains("\"type\":\"MultiPolygon\""));
        }

        [Test]
        public void GeometryCollectionToGeoJsonTest()
        {
            var geometrycollection = new GeometryCollection(
                new List<IGeometryObject>
                {
                    new Point(
                        new GeographicPosition(4, 6)
                    ),
                    new LineString(
                        new List<GeographicPosition>
                        {
                            new GeographicPosition(4,6),
                            new GeographicPosition(7, 10)
                        }
                    )
                }
            );

            var featureCollection = new FeatureCollection(new List<Feature> { new Feature(geometrycollection) });
            var featureCollectionJson = GeoJsonWriter.ToGeoJson(featureCollection);
            Assert.True(featureCollectionJson.Contains("\"type\":\"GeometryCollection\""));
        }

        [Test]
        public void EmptyFeatureCollectionToGeoJsonTest()
        {
            var featureCollection = new FeatureCollection();
            var featureCollectionJson = GeoJsonWriter.ToGeoJson(featureCollection);
            Assert.True(featureCollectionJson.Contains("\"type\":\"FeatureCollection\""));
        }

        [Test]
        public void EmptyPolygonToGeoJsonTest()
        {
            var polygon = new Polygon(new List<LineString>());
            var featureCollection = new FeatureCollection(new List<Feature> { new Feature(polygon) });
            var featureCollectionJson = GeoJsonWriter.ToGeoJson(featureCollection);
            Assert.True(featureCollectionJson.Contains("\"type\":\"Polygon\""));
        }
    }
}

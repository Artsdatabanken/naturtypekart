using System.Data.SqlTypes;
using GeoJSON.Net;
using Microsoft.SqlServer.Types;
using Nin.Common;
using Nin.GeoJson;
using NUnit.Framework;

namespace Nin.Test.Unit.Common
{
    public class GeometryConverterTest
    {
        [Test]
        public void ConvertPointTest()
        {
            var point = SqlGeometry.STGeomFromText(new SqlChars("POINT(150.555 -10.666)"), 25832);
            var geom = GeoJsonGeometry.FromSqlGeometry(point);
            Assert.True(geom.Type == GeoJSONObjectType.Point);
        }

        [Test]
        public void ConvertMultiPointTest()
        {
            var multipoint = SqlGeometry.STGeomFromText(new SqlChars("MULTIPOINT((10 40), (40 30), (20 20), (30 10))"), 25832);
            var geom = GeoJsonGeometry.FromSqlGeometry(multipoint);
            Assert.True(geom.Type == GeoJSONObjectType.MultiPoint);
        }

        [Test]
        public void ConvertLineStringTest()
        {
            var linestring = SqlGeometry.STGeomFromText(new SqlChars("LINESTRING(30 10, 31 11, 32 12)"), 25832);
            var geom = GeoJsonGeometry.FromSqlGeometry(linestring);
            Assert.True(geom.Type == GeoJSONObjectType.LineString);
        }

        [Test]
        public void ConvertMultiLineStringTest()
        {
            var multilinestring = SqlGeometry.STGeomFromText(new SqlChars("MULTILINESTRING((10 10, 20 20, 10 40), (40 40, 30 30, 40 20, 30 10))"), 25832);
            var geom = GeoJsonGeometry.FromSqlGeometry(multilinestring);
            Assert.True(geom.Type == GeoJSONObjectType.MultiLineString);
        }

        [Test]
        public void ConvertPolygonTest()
        {
            var polygon = SqlGeometry.STGeomFromText(new SqlChars("POLYGON((150 -10, 150 110, 250 110, 250 -10, 150 -10))"), 25832);
            var geom = GeoJsonGeometry.FromSqlGeometry(polygon);
            Assert.True(geom.Type == GeoJSONObjectType.Polygon);
        }

        [Test]
        public void ConvertMultiPolygonTest()
        {
            var multipolygon = SqlGeometry.STGeomFromText(new SqlChars("MULTIPOLYGON (((40 40, 20 45, 45 30, 40 40)),((20 35, 10 30, 10 10, 30 5, 45 20, 20 35),(30 20, 20 15, 20 25, 30 20)))"), 25832);
            var geom = GeoJsonGeometry.FromSqlGeometry(multipolygon);
            Assert.True(geom.Type == GeoJSONObjectType.MultiPolygon);
        }

        [Test]
        public void ConvertGeometryCollectionTest()
        {
            var geometrycollection = SqlGeometry.STGeomFromText(new SqlChars("GEOMETRYCOLLECTION(POINT(4 6), LINESTRING(4 6, 7 10))"), 25832);
            var geom = GeoJsonGeometry.FromSqlGeometry(geometrycollection);
            Assert.True(geom.Type == GeoJSONObjectType.GeometryCollection);
        }
    }
}

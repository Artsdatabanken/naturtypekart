using System.Data.SqlTypes;
using Common;
using Microsoft.SqlServer.Types;
using NUnit.Framework;

namespace Test.Unit.Common
{
    class ShapeConverterTest
    {
        [Test]
        public void ConvertPointTest()
        {
            var point = SqlGeometry.STGeomFromText(new SqlChars("POINT(150.555 -10.666)"), 25832);
            var geom = DotSpatialGeometry.GetGeometry(point);
            Assert.True(geom.GeometryType == "Point");
            Assert.False(geom.IsEmpty);
        }

        [Test]
        public void ConvertMultiPointTest()
        {
            var multipoint = SqlGeometry.STGeomFromText(new SqlChars("MULTIPOINT((10 40), (40 30), (20 20), (30 10))"),
                25832);
            var geom = DotSpatialGeometry.GetGeometry(multipoint);
            Assert.True(geom.GeometryType == "MultiPoint");
            Assert.False(geom.IsEmpty);
        }

        [Test]
        public void ConvertEmptyMultiPointTest()
        {
            var multipoint = SqlGeometry.STGeomFromText(new SqlChars("MULTIPOINT EMPTY"), 25832);
            var geom = DotSpatialGeometry.GetGeometry(multipoint);
            Assert.True(geom.GeometryType == "MultiPoint");
            Assert.True(geom.IsEmpty);
        }

        [Test]
        public void ConvertLineStringTest()
        {
            var linestring = SqlGeometry.STGeomFromText(new SqlChars("LINESTRING(30 10, 31 11, 32 12)"), 25832);
            var geom = DotSpatialGeometry.GetGeometry(linestring);
            Assert.True(geom.GeometryType == "LineString");
            Assert.False(geom.IsEmpty);
        }

        [Test]
        public void ConvertEmptyLineStringTest()
        {
            var linestring = SqlGeometry.STGeomFromText(new SqlChars("LINESTRING EMPTY"), 25832);
            var geom = DotSpatialGeometry.GetGeometry(linestring);
            Assert.True(geom.GeometryType == "LineString");
            Assert.True(geom.IsEmpty);
        }

        [Test]
        public void ConvertMultiLineStringTest()
        {
            var multilinestring =
                SqlGeometry.STGeomFromText(
                    new SqlChars("MULTILINESTRING((10 10, 20 20, 10 40), (40 40, 30 30, 40 20, 30 10))"), 25832);
            var geom = DotSpatialGeometry.GetGeometry(multilinestring);
            Assert.True(geom.GeometryType == "MultiLineString");
            Assert.False(geom.IsEmpty);
        }

        [Test]
        public void ConvertEmptyMultiLineStringTest()
        {
            var multilinestring = SqlGeometry.STGeomFromText(new SqlChars("MULTILINESTRING EMPTY"), 25832);
            var geom = DotSpatialGeometry.GetGeometry(multilinestring);
            Assert.True(geom.GeometryType == "MultiLineString");
            Assert.True(geom.IsEmpty);
        }

        [Test]
        public void ConvertPolygonTest()
        {
            var polygon =
                SqlGeometry.STGeomFromText(
                    new SqlChars("POLYGON((20 35, 10 30, 10 10, 30 5, 45 20, 20 35),(30 20, 20 15, 20 25, 30 20))"),
                    25832);
            var geom = DotSpatialGeometry.GetGeometry(polygon);
            Assert.True(geom.GeometryType == "Polygon");
            Assert.False(geom.IsEmpty);
        }

        [Test]
        public void ConvertEmptyPolygonTest()
        {
            var polygon = SqlGeometry.STGeomFromText(new SqlChars("POLYGON EMPTY"), 25832);
            var geom = DotSpatialGeometry.GetGeometry(polygon);
            Assert.True(geom.GeometryType == "Polygon");
            Assert.True(geom.IsEmpty);
        }

        [Test]
        public void ConvertMultiPolygonTest()
        {
            var multipolygon =
                SqlGeometry.STGeomFromText(
                    new SqlChars(
                        "MULTIPOLYGON (((40 40, 20 45, 45 30, 40 40)),((20 35, 10 30, 10 10, 30 5, 45 20, 20 35),(30 20, 20 15, 20 25, 30 20)))"),
                    25832);
            var geom = DotSpatialGeometry.GetGeometry(multipolygon);
            Assert.True(geom.GeometryType == "MultiPolygon");
            Assert.False(geom.IsEmpty);
        }

        [Test]
        public void ConvertEmptyMultiPolygonTest()
        {
            var multipolygon = SqlGeometry.STGeomFromText(new SqlChars("MULTIPOLYGON EMPTY"), 25832);
            var geom = DotSpatialGeometry.GetGeometry(multipolygon);
            Assert.True(geom.GeometryType == "MultiPolygon");
            Assert.True(geom.IsEmpty);
        }

        [Test]
        public void ConvertGeometryCollectionTest()
        {
            var geometrycollection =
                SqlGeometry.STGeomFromText(new SqlChars("GEOMETRYCOLLECTION(POINT(4 6), LINESTRING(4 6, 7 10))"), 25832);
            var geom = DotSpatialGeometry.GetGeometry(geometrycollection);
            Assert.True(geom.GeometryType == "GeometryCollection");
            Assert.False(geom.IsEmpty);
        }

        [Test]
        public void ConvertEmptyGeometryCollectionTest()
        {
            var geometrycollection = SqlGeometry.STGeomFromText(new SqlChars("GEOMETRYCOLLECTION EMPTY"), 25832);
            var geom = DotSpatialGeometry.GetGeometry(geometrycollection);
            Assert.True(geom.GeometryType == "GeometryCollection");
            Assert.True(geom.IsEmpty);
        }
    }
}
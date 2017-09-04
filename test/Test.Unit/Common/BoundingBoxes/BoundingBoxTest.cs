using System.Data.SqlTypes;
using Microsoft.SqlServer.Types;
using Nin;
using Nin.Common.Map.Geometric.BoundingBoxes;
using NUnit.Framework;

namespace Test.Unit.Common.BoundingBoxes
{
    public class BoundingBoxTest
    {
        private readonly PolygonBoundingBox norge;

        public BoundingBoxTest()
        {
            norge = new Norway1Kmbuffer8kSimplifi8k();
        }

        [Test]
        public void BoundingBox_Of_Norway()
        {
            SqlGeometry trondheim = GeometryFromWkb("POINT (10.42 63.45)", 4326);
            var bound = trondheim.STBoundary();
            Assert.True(norge.Intersects(trondheim));
        }

        [Test]
        public void BoundingBox_Trondheim_is_in_Norway()
        {
            SqlGeometry trondheim = GeometryFromWkb("POINT (10.42 63.45)", 4326);
            Assert.True(norge.Intersects(trondheim));
        }

        [Test]
        public void BoundingBox_Trondheim_Utm_is_in_Norway()
        {
            SqlGeometry trondheim = GeometryFromWkb("POINT (570915 7036162)", 32632);
            Assert.True(norge.Intersects(trondheim));
        }
        	

        [Test]
        public void BoundingBox_Storlien_is_not_in_Norway()
        {
            SqlGeometry storlien = GeometryFromWkb("POINT (13.08 63.4)", 4326);
            Assert.False(norge.Intersects(storlien));
        }

        [Test]
        public void BoundingBox_Galdhøpiggen_is_in_Norway()
        {
            SqlGeometry galdhøpiggen = GeometryFromWkb("POINT (145996 6851887)", 32633);
            SqlGeometry galdhøpiggen32 = GeometryFromWkb("POINT (463551 6833873)", 32632);
            Assert.True(norge.Intersects(galdhøpiggen));

            Assert.True(norge.Intersects(galdhøpiggen));
            // 61.6365 8.3124
        }

        private SqlGeometry GeometryFromWkb(string wellKnownText, int srid)
        {
            var x = new MapProjection(norge.Srid).ReprojectFromWkt(wellKnownText, srid);
            SqlGeometry geometry = SqlGeometry.STGeomFromText(new SqlChars(x), norge.Srid);
            return geometry;
        }
    }
}
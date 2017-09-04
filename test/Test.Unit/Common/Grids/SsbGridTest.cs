using System.Data;
using DotSpatial.Data;
using DotSpatial.Topology;
using Nin.Common.Map.Geometric.Grids;
using NUnit.Framework;

namespace Nin.Test.Unit.Common.Grids
{
    public class SsbGridTest
    {
        [Test]
        public void GetCellId5000()
        {
            var id = new SsbGrid(5000).GetCellId(145996, 6851887);
            Assert.AreEqual(21450006850000L, id);
        }

        [Test]
        public void GetCentreCoordinate()
        {
            var coord = new SsbGrid(5000).GetGridCellLowerLeftCoordinate(21450006850000L);
            Assert.AreEqual(147500, coord.X);
            Assert.AreEqual(6850000, coord.Y);
        }

        [Test]
        public void GetPolygon()
        {
            Coordinate[] polygon = new SsbGrid(5000).GetPolygon(21450006850000L);
            Assert.AreEqual("(147500, 6850000)", polygon[0].ToString());
            Assert.AreEqual("(152500, 6850000)", polygon[1].ToString());
            Assert.AreEqual("(152500, 6855000)", polygon[2].ToString());
            Assert.AreEqual("(147500, 6855000)", polygon[3].ToString());
            Assert.AreEqual("(147500, 6850000)", polygon[4].ToString());
        }

        [Test][Ignore("test used during development")]
        public void BuildShape()
        {
            FeatureSet shp = new FeatureSet(FeatureType.Polygon);
            var columns = shp.DataTable.Columns;
            columns.Add(new DataColumn("SSBID", typeof(long)));
            columns.Add(new DataColumn("RSIZE", typeof(long)));
            columns.Add(new DataColumn("ROW", typeof(long)));
            columns.Add(new DataColumn("COL", typeof(long)));
            columns.Add(new DataColumn("XCOOR", typeof(long)));
            columns.Add(new DataColumn("YCOOR", typeof(long)));

            var gridSizeMeters = 500000;
            var ssbGrid = new SsbGrid(gridSizeMeters);
            var xMin = - -1000000.00;
            var yMin = 5500000.00;
            var xMax = 1120000;
            var yMax = 7940000;
            double x = xMin, y = yMin;
            for (int row = 0; y < yMax; row++)
            {
                for (int col = 0; x < xMax; col++)
                {
                    var lowerLeftCoordinate = new Coordinate(x, y);
                    Coordinate[] coords = ssbGrid.GetPolygon(lowerLeftCoordinate);
                    var feature = new Feature(FeatureType.Polygon, coords);

                    shp.Features.Add(feature);
                    feature.DataRow.BeginEdit();
                    feature.DataRow["SSBID"] = ssbGrid.GetCellId(lowerLeftCoordinate);
                    feature.DataRow["RSIZE"] = gridSizeMeters;
                    feature.DataRow["ROW"] = row;
                    feature.DataRow["COL"] = col;
                    feature.DataRow["XCOOR"] = lowerLeftCoordinate.X;
                    feature.DataRow["YCOOR"] = lowerLeftCoordinate.Y;
                    feature.DataRow.EndEdit();

                    x += gridSizeMeters;
                }
                y += gridSizeMeters;
                x = xMin;
            }

            shp.SaveAs(@"c:\temp\test.shp", true);
        }
    }
}
using System.Data.SqlTypes;
using Microsoft.SqlServer.Types;

namespace Nin.Common.Map.Geometric.BoundingBoxes
{

    public class Norway1Kmbuffer8kSimplifi8k : PolygonBoundingBox
    {
        public Norway1Kmbuffer8kSimplifi8k() : base(wkb, srid)
        {
        }

        const int srid = 32633;
        const string wkb = "POLYGON ((-111000 6649174, -95000 6684683, -105000 6837000, -89472 6907702, -17702 6999472, 75316 7055000, 148297 7139472, 249000 7223829, 336825 7422000, 325391 7509472, 365000 7544683, 380527 7587702, 415000 7614683, 470527 7697702, 565000 7755000, 612297 7833472, 730170 7891000, 794527 7950608, 877000 7975000, 993000 7979000, 1131702 7923472, 1134608 7784527, 1105000 7775000, 1071702 7700527, 1030527 7708297, 1026170 7779000, 985000 7795316, 955000 7785000, 941000  7756170, 944608 7664527, 925000 7649316, 917702 7620527, 868000 7630825, 820527 7615391, 782297 7626527, 760839 7661986, 734683 7655000, 730608 7590527, 717702 7576527, 650683 7585000, 639472 7528297, 591000 7520170, 575000 7475316, 580608 7424527, 545000 7375316, 539472 7338297, 501000 7319000, 494608 7230527, 465000 7175316, 475000 7163000, 470608 7084527, 410857 7086097, 373174 7018000, 371000 6853829, 404608 6819472, 405000 6787000, 393472 6758297, 375000 6755000, 385000 6727000, 380608 6650527, 367702 6630527, 345000 6625000, 323472 6518297, 279160 6521986, 259472 6505391, 224683 6505000, 125472 6419391, 4527 6415391, -80608 6500527, -91000 6560170, -113608 6590527, -111000 6649174))";
    }

    public class PolygonBoundingBox 
    {
        private readonly SqlGeometry sqlGeometry;

        public int Srid { get; }

        protected PolygonBoundingBox(string wkb, int srid)
        {
            Srid = srid;
            sqlGeometry = SqlGeometry.STGeomFromText(new SqlChars(wkb), srid);
        }

        public bool Intersects(SqlGeometry geometry)
        {
            var stIntersects = sqlGeometry.STIntersects(geometry);
            return stIntersects.IsTrue;
        }

        public override string ToString()
        {
            return sqlGeometry.AsTextZM().ToSqlString().ToString();
        }
    }
}
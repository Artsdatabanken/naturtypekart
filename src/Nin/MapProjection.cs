using System;
using System.Data.SqlTypes;
using DotSpatial.Projections;
using Microsoft.SqlServer.Types;
using Nin.Common;
using Nin.Configuration;
using Nin.Dataleveranser.Rutenett;
using Nin.Types.MsSql;

namespace Nin
{
    public class MapProjection
    {
        private const string Geometrycollection = "GeometryCollection";
        private const string Linestring = "LineString";
        private const string Multilinestring = "MultiLineString";
        private const string Multipoint = "MultiPoint";
        private const string Multipolygon = "MultiPolygon";
        private const string Point = "Point";
        private const string Polygon = "Polygon";

        private readonly int targetSrid;

        public MapProjection(int targetSrid)
        {
            this.targetSrid = targetSrid;
        }

        public string ReprojectFromWkt(string wellKnownText, int currentEpsgCode)
        {
            return Reproject(wellKnownText, currentEpsgCode).ToString();
        }

        private SqlGeometry Reproject(string wellKnownText, int currentEpsgCode)
        {
            var sqlGeometry = SqlGeometry.STGeomFromText(new SqlChars(wellKnownText), currentEpsgCode);
            return Reproject(sqlGeometry.MakeValid());
        }

        public static SqlGeometry Reproject(SqlGeometry sqlGeometry, int targetSrs)
        {
            return new MapProjection(targetSrs).Reproject(sqlGeometry);
        }

        public SqlGeometry Reproject(SqlGeometry sqlGeometry)
        {
            var targetSrs = targetSrid;
            var newProjection = GetProjectionInfo(targetSrs);
            var currentProjection = GetProjectionInfo(sqlGeometry.STSrid.Value);
            SqlGeometryBuilder sqlGeometryBuilder = new SqlGeometryBuilder();
            sqlGeometryBuilder.SetSrid(targetSrs);

            AddGeometry(sqlGeometry, ref sqlGeometryBuilder, currentProjection, newProjection);

            return sqlGeometryBuilder.ConstructedGeometry.MakeValid();
        }

        private static ProjectionInfo GetProjectionInfo(int epsgCode)
        {
            ProjectionInfo newProjection;
            try
            {
                newProjection = ProjectionInfo.FromEpsgCode(epsgCode);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new CoordinateSystemConverterException("Converting coordinates failed. Unknown EPSG code: " + epsgCode);
            }
            return newProjection;
        }

        private void AddGeometry(SqlGeometry sqlGeometry, ref SqlGeometryBuilder sqlGeometryBuilder, ProjectionInfo currentProjection, ProjectionInfo newProjection)
        {
            switch (sqlGeometry.STGeometryType().Value)
            {
                case Geometrycollection:
                    AddGeometryCollection(sqlGeometry, ref sqlGeometryBuilder, currentProjection, newProjection);
                    break;
                case Linestring:
                    AddLineString(sqlGeometry, ref sqlGeometryBuilder, currentProjection, newProjection);
                    break;
                case Multilinestring:
                    AddMultiLineString(sqlGeometry, ref sqlGeometryBuilder, currentProjection, newProjection);
                    break;
                case Multipoint:
                    AddMultiPoint(sqlGeometry, ref sqlGeometryBuilder, currentProjection, newProjection);
                    break;
                case Multipolygon:
                    AddMultiPolygon(sqlGeometry, ref sqlGeometryBuilder, currentProjection, newProjection);
                    break;
                case Point:
                    AddPoint(sqlGeometry, ref sqlGeometryBuilder, currentProjection, newProjection);
                    break;
                case Polygon:
                    AddPolygon(sqlGeometry, ref sqlGeometryBuilder, currentProjection, newProjection);
                    break;
                default:
                    throw new CoordinateSystemConverterException("Converting coordinates failed. Unknown geometry type.");
            }
        }

        private static void AddPoint(SqlGeometry sqlGeometry, ref SqlGeometryBuilder sqlGeometryBuilder, ProjectionInfo currentProjection, ProjectionInfo newProjection)
        {
            sqlGeometryBuilder.BeginGeometry(OpenGisGeometryType.Point);
            ReprojectCoordinates(sqlGeometry, ref sqlGeometryBuilder, currentProjection, newProjection);
            sqlGeometryBuilder.EndGeometry();
        }

        private static void AddMultiPoint(SqlGeometry sqlGeometry, ref SqlGeometryBuilder sqlGeometryBuilder, ProjectionInfo currentProjection, ProjectionInfo newProjection)
        {
            sqlGeometryBuilder.BeginGeometry(OpenGisGeometryType.MultiPoint);
            for (int i = 1; i <= sqlGeometry.STNumGeometries(); ++i)
                AddPoint(sqlGeometry.STGeometryN(i), ref sqlGeometryBuilder, currentProjection, newProjection);
            sqlGeometryBuilder.EndGeometry();
        }

        private static void AddLineString(SqlGeometry sqlGeometry, ref SqlGeometryBuilder sqlGeometryBuilder, ProjectionInfo currentProjection, ProjectionInfo newProjection)
        {
            sqlGeometryBuilder.BeginGeometry(OpenGisGeometryType.LineString);
            ReprojectCoordinates(sqlGeometry, ref sqlGeometryBuilder, currentProjection, newProjection);
            sqlGeometryBuilder.EndGeometry();
        }

        private static void AddMultiLineString(SqlGeometry sqlGeometry, ref SqlGeometryBuilder sqlGeometryBuilder, ProjectionInfo currentProjection, ProjectionInfo newProjection)
        {
            sqlGeometryBuilder.BeginGeometry(OpenGisGeometryType.MultiLineString);
            for (int i = 1; i <= sqlGeometry.STNumGeometries(); ++i)
                AddLineString(sqlGeometry.STGeometryN(i), ref sqlGeometryBuilder, currentProjection, newProjection);
            sqlGeometryBuilder.EndGeometry();
        }

        private static void AddPolygon(SqlGeometry sqlGeometry, ref SqlGeometryBuilder sqlGeometryBuilder, ProjectionInfo currentProjection, ProjectionInfo newProjection)
        {
            sqlGeometryBuilder.BeginGeometry(OpenGisGeometryType.Polygon);
            var exteriorRing = sqlGeometry.STExteriorRing();
            if (!exteriorRing.IsNull)
                ReprojectCoordinates(exteriorRing, ref sqlGeometryBuilder, currentProjection, newProjection);

            for (int i = 1; i <= sqlGeometry.STNumInteriorRing(); ++i)
            {
                var interiorRing = sqlGeometry.STInteriorRingN(i);
                ReprojectCoordinates(interiorRing, ref sqlGeometryBuilder, currentProjection, newProjection);
            }
            sqlGeometryBuilder.EndGeometry();
        }

        private static void AddMultiPolygon(SqlGeometry sqlGeometry, ref SqlGeometryBuilder sqlGeometryBuilder, ProjectionInfo currentProjection, ProjectionInfo newProjection)
        {
            sqlGeometryBuilder.BeginGeometry(OpenGisGeometryType.MultiPolygon);
            for (int i = 1; i <= sqlGeometry.STNumGeometries(); ++i)
                AddPolygon(sqlGeometry.STGeometryN(i), ref sqlGeometryBuilder, currentProjection, newProjection);
            sqlGeometryBuilder.EndGeometry();
        }

        private void AddGeometryCollection(SqlGeometry sqlGeometry, ref SqlGeometryBuilder sqlGeometryBuilder, ProjectionInfo currentProjection, ProjectionInfo newProjection)
        {
            sqlGeometryBuilder.BeginGeometry(OpenGisGeometryType.GeometryCollection);
            for (int i = 1; i <= sqlGeometry.STNumGeometries(); ++i)
                AddGeometry(sqlGeometry.STGeometryN(i), ref sqlGeometryBuilder, currentProjection, newProjection);
            sqlGeometryBuilder.EndGeometry();
        }

        private static void ReprojectCoordinates(SqlGeometry sqlGeometry, ref SqlGeometryBuilder sqlGeometryBuilder, ProjectionInfo currentProjection, ProjectionInfo newProjection)
        {
            if (sqlGeometry.STIsEmpty())
                return;

            double[] xy = new double[sqlGeometry.STNumPoints().Value * 2];
            double[] z = new double[sqlGeometry.STNumPoints().Value];
            double?[] m = new double?[sqlGeometry.STNumPoints().Value];

            for (int i = 0; i < sqlGeometry.STNumPoints(); ++i)
            {
                var point = sqlGeometry.STPointN(i + 1);
                xy[i * 2] = point.STX.Value;
                xy[i * 2 + 1] = point.STY.Value;
                if (point.HasZ)
                    z[i] = point.Z.Value;
                if (point.HasM)
                    m[i] = point.M.Value;
            }

            DotSpatial.Projections.Reproject.ReprojectPoints(xy, z, currentProjection, newProjection, 0, sqlGeometry.STNumPoints().Value);
            sqlGeometryBuilder.BeginFigure(xy[0], xy[1],
                sqlGeometry.HasZ ? z[0] : new double?(),
                sqlGeometry.HasM ? m[0] : new double?());

            for (int i = 1; i < sqlGeometry.STNumPoints(); ++i)
            {
                double x = xy[i * 2];
                double y = xy[i * 2 + 1];
                double? pz = sqlGeometry.HasZ ? z[i] : new double?();
                double? pm = sqlGeometry.HasM ? m[i] : new double?();
                sqlGeometryBuilder.AddLine(x, y, pz, pm);
            }

            sqlGeometryBuilder.EndFigure();
        }

        public bool IsInsideBounds(SqlGeometry sqlGeometry)
        {
            var currentProjection = ProjectionInfo.FromEpsgCode(sqlGeometry.STSrid.Value);

            double[] xy = new double[sqlGeometry.STNumPoints().Value * 2];
            double[] z = new double[sqlGeometry.STNumPoints().Value];
            for (int i = 0; i < sqlGeometry.STNumPoints(); ++i)
            {
                var point = sqlGeometry.STPointN(i + 1);
                xy[i*2] = point.STX.Value;
                xy[i*2 + 1] = point.STY.Value;
            }
            DotSpatial.Projections.Reproject.ReprojectPoints(xy, z, currentProjection, GetTargetProjection(), 0, sqlGeometry.STNumPoints().Value);
            foreach (var d in xy)
            {
                if (double.IsInfinity(d)) return false;
                if (double.IsNaN(d)) return false;
            }
            return true;
        }

        private ProjectionInfo GetTargetProjection()
        {
            return ProjectionInfo.FromEpsgCode(targetSrid);
        }

        public static void ConvertGeometry(Dataleveranse dataleveranse)
        {
            MapProjection reproject = new MapProjection(Config.Settings.Map.SpatialReferenceSystemIdentifier);
            dataleveranse.Metadata.Area = reproject.Reproject(dataleveranse.Metadata.Area);
            foreach (var natureArea in dataleveranse.Metadata.NatureAreas)
                natureArea.Area = reproject.Reproject(natureArea.Area);
        }

        public static void ConvertGeometry(Types.RavenDb.Dataleveranse dataleveranse, int epsgCode)
        {
            var converter = new MapProjection(epsgCode);
            dataleveranse.Metadata.Area = converter.ReprojectFromWkt(dataleveranse.Metadata.Area, dataleveranse.Metadata.AreaEpsgCode);
            dataleveranse.Metadata.AreaEpsgCode = epsgCode;
            foreach (var natureArea in dataleveranse.Metadata.NatureAreas)
            {
                natureArea.Area = converter.ReprojectFromWkt(natureArea.Area, natureArea.AreaEpsgCode);
                natureArea.AreaEpsgCode = epsgCode;
            }
        }

        public static void ConvertGeometry(GridLayerCellCustom gridLayerCellCustom)
        {
            MapProjection reproject = new MapProjection(Config.Settings.Map.SpatialReferenceSystemIdentifier);
            gridLayerCellCustom.CustomCell = reproject.Reproject(gridLayerCellCustom.CustomCell);
        }
    }
}

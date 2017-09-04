using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DotSpatial.Topology;
using GeoJSON.Net.Geometry;
using Microsoft.SqlServer.Types;
using Nin.Common;
using GeometryCollection = GeoJSON.Net.Geometry.GeometryCollection;
using LineString = GeoJSON.Net.Geometry.LineString;
using MultiLineString = GeoJSON.Net.Geometry.MultiLineString;
using MultiPoint = GeoJSON.Net.Geometry.MultiPoint;
using MultiPolygon = GeoJSON.Net.Geometry.MultiPolygon;
using Point = GeoJSON.Net.Geometry.Point;
using Polygon = GeoJSON.Net.Geometry.Polygon;

namespace Nin.GeoJson
{
    public static class GeoJsonGeometry
    {
        private const string GEOMETRYCOLLECTION = "GeometryCollection";
        private const string LINESTRING = "LineString";
        private const string MULTILINESTRING = "MultiLineString";
        private const string MULTIPOINT = "MultiPoint";
        private const string MULTIPOLYGON = "MultiPolygon";
        private const string POINT = "Point";
        private const string POLYGON = "Polygon";

        public static IGeometryObject FromSqlGeometry(SqlGeometry geometry)
        {
            IGeometryObject geometryObject;

            switch (geometry.STGeometryType().Value)
            {
                case GEOMETRYCOLLECTION:
                    geometryObject = FromSqlGeometryCollection(geometry);
                    break;
                case LINESTRING:
                    geometryObject = FromSqlLineString(geometry);
                    break;
                case MULTILINESTRING:
                    geometryObject = FromSqlMultiLineString(geometry);
                    break;
                case MULTIPOINT:
                    geometryObject = FromSqlMultPoint(geometry);
                    break;
                case MULTIPOLYGON:
                    geometryObject = FromSqlMultiPolygon(geometry);
                    break;
                case POINT:
                    geometryObject = FromSqlPoint(geometry);
                    break;
                case POLYGON:
                    geometryObject = FromSqlPolygon(geometry);
                    break;
                default:
                    throw new GeometryConverterException("Converting geometry failed. Unknown geometry type.");
            }

            return geometryObject;
        }

        private static Point FromSqlPoint(SqlGeometry point)
        {
            return new Point(GetGeographicPosition(point));
        }

        private static MultiPoint FromSqlMultPoint(SqlGeometry multiPoint)
        {
            List<Point> points = new List<Point>();

            for (int i = 1; i <= multiPoint.STNumGeometries(); ++i)
                points.Add(FromSqlPoint(multiPoint.STGeometryN(i)));

            return new MultiPoint(points);
        }

        private static LineString FromSqlLineString(SqlGeometry lineString)
        {
            Collection<GeographicPosition> points = new Collection<GeographicPosition>();

            for (int i = 1; i <= lineString.STNumPoints(); ++i)
                points.Add(GetGeographicPosition(lineString.STPointN(i)));

            return new LineString(points);
        }

        private static MultiLineString FromSqlMultiLineString(SqlGeometry multiLineString)
        {
            List<LineString> lineStrings = new List<LineString>();

            for (int i = 1; i <= multiLineString.STNumGeometries(); ++i)
                lineStrings.Add(FromSqlLineString(multiLineString.STGeometryN(i)));

            return new MultiLineString(lineStrings);
        }

        private static Polygon FromSqlPolygon(SqlGeometry polygon)
        {
            var lineStrings = new List<LineString>();
            var exteriorRing = polygon.STExteriorRing();

            if (!exteriorRing.IsNull)
                lineStrings.Add(FromSqlLineString(exteriorRing));

            for (int i = 1; i <= polygon.STNumInteriorRing(); ++i)
                lineStrings.Add(FromSqlLineString(polygon.STInteriorRingN(i)));

            return new Polygon(lineStrings);
        }

        private static MultiPolygon FromSqlMultiPolygon(SqlGeometry multiPolygon)
        {
            List<Polygon> polygons = new List<Polygon>();

            for (int i = 1; i <= multiPolygon.STNumGeometries(); ++i)
                polygons.Add(FromSqlPolygon(multiPolygon.STGeometryN(i)));

            return new MultiPolygon(polygons);
        }

        private static GeometryCollection FromSqlGeometryCollection(SqlGeometry geometryCollection)
        {
            List<IGeometryObject> geometries = new List<IGeometryObject>();

            for (int i = 1; i <= geometryCollection.STNumGeometries(); ++i)
                geometries.Add(FromSqlGeometry(geometryCollection.STGeometryN(i)));

            return new GeometryCollection(geometries);
        }

        private static GeographicPosition GetGeographicPosition(SqlGeometry point)
        {
            if (point.HasZ)
                return new GeographicPosition(
                    point.STY.Value,
                    point.STX.Value,
                    point.Z.Value
                );
            
                return new GeographicPosition(
                point.STY.Value,
                point.STX.Value
            );
        }

        public static IGeometryObject FromDotSpatial(IGeometry polygon)
        {
            switch (polygon.GeometryType)
            {
                case "Polygon":
                    return FromDotSpatial((DotSpatial.Topology.Polygon)polygon);
                case "MultiPolygon":
                    return FromDotSpatial((DotSpatial.Topology.MultiPolygon)polygon);
                case "LineString":
                    return FromDotSpatial((DotSpatial.Topology.LineString)polygon);
                case "MultiLineString":
                    return FromDotSpatial((DotSpatial.Topology.MultiLineString)polygon);
                case "GeometryCollection":
                    return FromDotSpatial((DotSpatial.Topology.GeometryCollection)polygon);
                default:
                    throw new Exception("Ukjent geometri '" + polygon.GeometryType + "'.");
            }
        }

        public static IGeometryObject FromDotSpatial(DotSpatial.Topology.MultiLineString multiLineString)
        {
            List<LineString> ls = new List<LineString>();
            for (int i = 0; i < multiLineString.Count; i++)
            {
                var lineString = (DotSpatial.Topology.LineString) multiLineString.GetGeometryN(i);
                ls.Add((LineString) FromDotSpatial(lineString));
            }
            return new MultiLineString(ls);
        }

        public static IGeometryObject FromDotSpatial(DotSpatial.Topology.GeometryCollection f)
        {
            GeometryCollection r = new GeometryCollection();
            for (int i = 0; i < f.NumGeometries; i++)
            {
                IGeometry g = (IGeometry) f.GetBasicGeometryN(i);
                r.Geometries.Add(FromDotSpatial(g));
            }
            return r;
        }

        public static IGeometryObject FromDotSpatial(DotSpatial.Topology.LineString lineString)
        {
            var ls = ToCoords(lineString);
            return new LineString(ls.Coordinates);
        }

        private static IGeometryObject FromDotSpatial(DotSpatial.Topology.MultiPolygon polygon)
        {
            List<Polygon> pl = new List<Polygon>();
            for (int i = 0; i < polygon.Geometries.Length; i++)
            {
                IGeometry geometry = polygon.Geometries[i];
                var g = FromDotSpatial((DotSpatial.Topology.Polygon)geometry);
                pl.Add(g);
            }
            MultiPolygon r = new MultiPolygon(pl);
            return r;
        }

        private static Polygon FromDotSpatial(DotSpatial.Topology.Polygon polygon)
        {
            List<LineString> lineStrings = new List<LineString>();
            lineStrings.Add(ToCoords(polygon.Shell));

            for (int i = 0; i < polygon.NumHoles; i++)
            {
                var interiorRingN = polygon.GetInteriorRingN(i);
                lineStrings.Add(ToCoords((ILinearRing) interiorRingN));
            }
            return new Polygon(lineStrings);
        }

        private static LineString ToCoords(IBasicGeometry ls)
        {
            List<GeographicPosition> p = new List<GeographicPosition>();
            foreach (Coordinate point in ls.Coordinates)
                p.Add(new GeographicPosition(point.Y, point.X));
            var points = new LineString(p);
            return points;
        }
    }
}

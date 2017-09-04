using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using DotSpatial.Topology;
using GeoJSON.Net;
using GeoJSON.Net.Feature;
using Microsoft.SqlServer.Types;
using geoJson = GeoJSON.Net.Geometry;
using GeometryCollection = DotSpatial.Topology.GeometryCollection;
using LineString = DotSpatial.Topology.LineString;
using MultiLineString = DotSpatial.Topology.MultiLineString;
using MultiPoint = DotSpatial.Topology.MultiPoint;
using MultiPolygon = DotSpatial.Topology.MultiPolygon;
using Point = DotSpatial.Topology.Point;
using Polygon = DotSpatial.Topology.Polygon;

namespace Common
{
    public static class DotSpatialGeometry
    {
        public static Geometry From(Feature feature)
        {
            var geometry = feature.Geometry;
            return FromGeoJson(geometry);
        }

        public static Geometry FromGeoJson(geoJson.IGeometryObject geometry)
        {
            switch (geometry.Type)
            {
                case GeoJSONObjectType.LineString:
                    return From((geoJson.LineString) geometry);
                case GeoJSONObjectType.MultiLineString:
                    return From((geoJson.MultiLineString) geometry);
                case GeoJSONObjectType.Polygon:
                    return From((geoJson.Polygon) geometry);
                case GeoJSONObjectType.MultiPolygon:
                    return From((geoJson.MultiPolygon) geometry);
                default:
                    throw new Exception("Ukjent geometritype '" + geometry.Type + "'.");
            }
        }

        private static Geometry From(geoJson.MultiLineString geometry)
        {
            var lineStrings = geometry.Coordinates;
            var p = new LineString[lineStrings.Count];
            for (var i = 0; i < lineStrings.Count; i++)
                p[i] = From(lineStrings[i]);
            return new MultiLineString(p);
        }

        private static LineString From(geoJson.LineString lineString)
        {
            return new LineString(FromCoordinates(lineString));
        }

        private static Geometry From(geoJson.MultiPolygon geometry)
        {
            var polygons = geometry.Coordinates;
            var p = new Polygon[polygons.Count];
            for (var i = 0; i < polygons.Count; i++)
                p[i] = From(polygons[i]);
            return new MultiPolygon(p);
        }

        private static Polygon From(geoJson.Polygon feature)
        {
            var rings = new List<LinearRing>();
            foreach (var lineString in feature.Coordinates)
            {
                var coords = FromCoordinates(lineString);
                var lr = new LinearRing(coords);
                rings.Add(lr);
            }
            var inner = new ILinearRing[rings.Count - 1];
            for (var i = 0; i < inner.Length; i++)
                inner[i] = rings[i + 1];
            var r = new Polygon(rings[0], inner);
            return r;
        }

        private static List<Coordinate> FromCoordinates(geoJson.LineString lineString)
        {
            var coords = new List<Coordinate>();
            foreach (var position in lineString.Coordinates)
            {
                var c = (geoJson.GeographicPosition) position;
                coords.Add(new Coordinate(c.Longitude, c.Latitude));
            }
            return coords;
        }

        public static Geometry From(SqlGeometry geometry)
        {
            return GetGeometry(geometry);
        }

        public static Geometry GetGeometry(SqlGeometry sqlGeometry)
        {
            Geometry shapeGeometry;

            switch (sqlGeometry.STGeometryType().Value)
            {
                case GEOMETRYCOLLECTION:
                    shapeGeometry = GetGeometryCollection(sqlGeometry);
                    break;
                case LINESTRING:
                    shapeGeometry = GetLineString(sqlGeometry);
                    break;
                case MULTILINESTRING:
                    shapeGeometry = GetMultiLineString(sqlGeometry);
                    break;
                case MULTIPOINT:
                    shapeGeometry = GetMultiPoint(sqlGeometry);
                    break;
                case MULTIPOLYGON:
                    shapeGeometry = GetMultiPolygon(sqlGeometry);
                    break;
                case POINT:
                    shapeGeometry = GetPoint(sqlGeometry);
                    break;
                case POLYGON:
                    shapeGeometry = GetPolygon(sqlGeometry);
                    break;
                default:
                    throw new Exception("Creating shape geometry failed. Unknown geometry type.");
            }

            return shapeGeometry;
        }

        private static GeometryCollection GetGeometryCollection(SqlGeometry sqlGeometryCollection)
        {
            var r = new IGeometry[sqlGeometryCollection.STNumGeometries().Value];

            for (var i = 1; i <= sqlGeometryCollection.STNumGeometries().Value; ++i)
                r[i - 1] = GetGeometry(sqlGeometryCollection.STGeometryN(i));

            return new GeometryCollection(r);
        }

        private static MultiPolygon GetMultiPolygon(SqlGeometry sqlMultiPolygon)
        {
            var polygons = new Polygon[sqlMultiPolygon.STNumGeometries().Value];

            for (var i = 1; i <= sqlMultiPolygon.STNumGeometries().Value; ++i)
                polygons[i - 1] = GetPolygon(sqlMultiPolygon.STGeometryN(i));

            return new MultiPolygon(polygons);
        }

        private static MultiLineString GetMultiLineString(SqlGeometry sqlMultiLineString)
        {
            var lineStrings = new Collection<LineString>();

            for (var i = 1; i <= sqlMultiLineString.STNumGeometries().Value; ++i)
                lineStrings.Add(GetLineString(sqlMultiLineString.STGeometryN(i)));

            return new MultiLineString(lineStrings);
        }

        private static MultiPoint GetMultiPoint(SqlGeometry sqlMultiPoint)
        {
            var coordinates = new Collection<Coordinate>();

            for (var i = 1; i <= sqlMultiPoint.STNumPoints().Value; ++i)
                coordinates.Add(GetCoordinate(sqlMultiPoint.STPointN(i)));
            var multiPoint = new MultiPoint(coordinates);
            return multiPoint;
        }

        private static Polygon GetPolygon(SqlGeometry sqlPolygon)
        {
            var exteriorRing = new LinearRing(new LineString(new Collection<Coordinate>()));
            var interiorRings = new ILinearRing[sqlPolygon.STNumInteriorRing().Value];

            if (!sqlPolygon.STExteriorRing().IsNull)
                exteriorRing = new LinearRing(GetLineString(sqlPolygon.STExteriorRing()));

            for (var i = 1; i <= sqlPolygon.STNumInteriorRing(); ++i)
                interiorRings[i - 1] = new LinearRing(GetLineString(sqlPolygon.STInteriorRingN(i)));

            return new Polygon(exteriorRing, interiorRings);
        }

        private static LineString GetLineString(SqlGeometry sqlLineString)
        {
            var coordinates = new Collection<Coordinate>();

            for (var i = 1; i <= sqlLineString.STNumPoints().Value; ++i)
                coordinates.Add(GetCoordinate(sqlLineString.STPointN(i)));

            return new LineString(coordinates);
        }

        private static Point GetPoint(SqlGeometry sqlPoint)
        {
            var point = new Point(GetCoordinate(sqlPoint));
            return point;
        }

        private static Coordinate GetCoordinate(SqlGeometry sqlPoint)
        {
            var coordinate = Coordinate.Empty;
            if (sqlPoint.STIsEmpty()) return coordinate;

            coordinate.X = sqlPoint.STX.Value;
            coordinate.Y = sqlPoint.STY.Value;
            if (sqlPoint.HasZ) coordinate.Z = sqlPoint.Z.Value;
            if (sqlPoint.HasM) coordinate.M = sqlPoint.M.Value;
            return coordinate;
        }

        public static Geometry FromWkb(string wkb, int spatialReferenceSystemIdentifier)
        {
            var g = SqlGeometry.STGeomFromText(new SqlChars(wkb), spatialReferenceSystemIdentifier);
            return From(g);
        }

        private const string GEOMETRYCOLLECTION = "GeometryCollection";
        private const string LINESTRING = "LineString";
        private const string MULTILINESTRING = "MultiLineString";
        private const string MULTIPOINT = "MultiPoint";
        private const string MULTIPOLYGON = "MultiPolygon";
        private const string POINT = "Point";
        private const string POLYGON = "Polygon";
    }
}
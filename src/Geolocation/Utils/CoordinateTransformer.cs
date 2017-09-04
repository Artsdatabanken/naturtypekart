using System;
using System.Collections.Generic;
using Geolocation.Model;
using Geolocation.Model.Coordinates;
using Nin.Geolocation.Model.Enums;
using Nin.Geolocation.ServiceImplmentation;
using ProjNet.CoordinateSystems.Transformations;

namespace Geolocation.Utils
{
    public static class CoordinateTransformer
    {
        private static readonly CoordinateTransformationFactory CoordinateTransformationFactory;
        private static readonly GeographyService GeographyService;
        private static readonly Dictionary<Tuple<int, int>, ICoordinateTransformation>
            Transformations =
                new Dictionary<Tuple<int, int>, ICoordinateTransformation>();

        static CoordinateTransformer()
        {
            CoordinateTransformationFactory = new CoordinateTransformationFactory();
            GeographyService = new GeographyService();
        }

        public static Koordinat TransformCoordinate(
            Koordinat sourceKoordinat, int nyttKoordinatsystem)
        {
            if (sourceKoordinat.Koordinatsystem == nyttKoordinatsystem)
                return sourceKoordinat;

            var coordinateSystemCombination =
                new Tuple<int, int>(
                    sourceKoordinat.Koordinatsystem, nyttKoordinatsystem);
            var coordinateTransformation = GetCoordinateTransformation(coordinateSystemCombination);
            var point = coordinateTransformation.MathTransform.Transform(sourceKoordinat.Point);

            Koordinat result;
            switch (nyttKoordinatsystem)
            {
                case (int)Koordinatsystem.Wgs84LatLon:
                    result = new LatLonKoordinat { Point = point };
                    break;
                case (int)Koordinatsystem.GoogleMercator:
                    result = new GoogleMercatorKoordinat { Point = point };
                    break;
                default:
                    result = new UtmKoordinat(nyttKoordinatsystem) { Point = point };
                    break;
            }

            result.MetricCoordinatePrecision = sourceKoordinat.MetricCoordinatePrecision;
            return result;
        }

        private static ICoordinateTransformation GetCoordinateTransformation(Tuple<int, int> coordinateSystemCombination)
        {
            lock (Transformations)
            {
                if (!Transformations.ContainsKey(coordinateSystemCombination))
                {
                    var coordinateTransformation = CoordinateTransformationFactory.CreateFromCoordinateSystems(
                        GeographyService.GetCoordinateSystemById(coordinateSystemCombination.Item1),
                        GeographyService.GetCoordinateSystemById(coordinateSystemCombination.Item2));
                    Transformations.Add(coordinateSystemCombination, coordinateTransformation);
                }
                var transformation = Transformations[coordinateSystemCombination];
                return transformation;
            }
        }
    }
}
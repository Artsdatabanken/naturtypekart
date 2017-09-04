using System;
using Common.Diagnostic.Network.Web;
using NUnit.Framework;

namespace Test.Smoke
{
    public class DataAdministratorTest
    {
        private readonly string baseUrl;

        public DataAdministratorTest()
        {
            var host = "it-webadbtest01.it.ntnu.no";
            host = Environment.GetEnvironmentVariable("TESTHOST") ?? host;
            baseUrl = $"http://{host}/NinApi_vs2017/data";
        }

        [Test][Ignore("Mangler testdata")]
        public void GetNatureAreaByLocalId()
        {
            Http.Get($"{baseUrl}/GetNatureAreaByLocalId/eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
        }

        [Test]
        public void GetNatureAreaStatisticsBySearchFilter()
        {
            Http.Post($"{baseUrl}/GetNatureAreaStatisticsBySearchFilter",
                    @"{""NatureLevelCodes"":[],""CenterPoints"":true,""NatureAreaTypeCodes"":[],""DescriptionVariableCodes"":[],""Municipalities"":[],""Counties"":[],""ConservationAreas"":[],""Institutions"":[],""Geometry"":"""",""BoundingBox"":"""",""EpsgCode"":""3857"",""IndexFrom"":0,""IndexTo"":25,""ForceRefreshToggle"":false}");
        }

        [Test]
        public void GetNatureAreaInfosBySearchFilter()
        {
            //baseUrl = @"it-webadbtest01.it.ntnu.no";
            Http.Post($"{baseUrl}/GetNatureAreaInfosBySearchFilter",
                @"{""NatureLevelCodes"":[],""NatureAreaTypeCodes"":[],""DescriptionVariableCodes"":[],""Municipalities"":[],""Counties"":[],""ConservationAreas"":[],""Institutions"":[],""Geometry"":"""",""EpsgCode"":""3857"",""IndexFrom"":1,""IndexTo"":10}");
        }

        [Test]
        public void Default()
        {
            Http.Get($"{baseUrl}/");
        }

        [Test]
        public void GetAreas()
        {
            Http.Get($"{baseUrl}/GetAreas/?areatype=1&number=1264");
        }

        [Test]
        public void GetAreaSummary()
        {
            Http.Post($"{baseUrl}/GetAreaSummary",
                @"{""NatureLevelCodes"":[],""CenterPoints"":false,""NatureAreaTypeCodes"":[],""DescriptionVariableCodes"":[],""Municipalities"":[],""Counties"":[],""ConservationAreas"":[],""Institutions"":[],""Geometry"":"""",""BoundingBox"":"""",""EpsgCode"":""3857"",""IndexFrom"":0,""IndexTo"":25,""ForceRefreshToggle"":false}");
        }

        [Test]
        public void GetGrid_10km()
        {
            Http.Post($"{baseUrl}/GetGrid",
                @"{""GridType"":""SSB010KM"",""Municipalities"":[],""Counties"":[],""Geometry"":"""",""BoundingBox"":""POLYGON ((1145484.3461079423 9082859.897425586,1153433.7970496004 9082859.897425586,1153433.7970496004 9091841.248249093,1145484.3461079423 9091841.248249093,1145484.3461079423 9082859.897425586))"",""EpsgCode"":""3857"",""GridLayerTypeId"":""0""}");
        }

        [Test]
        public void GetGrid_500km()
        {
            Http.Post($"{baseUrl}/GetGrid",
                @"{""GridType"":""SSB500KM"",""Municipalities"":[],""Counties"":[],""Geometry"":"""",""BoundingBox"":"""",""EpsgCode"":""3857"",""GridLayerTypeId"":""0""}");
        }

        [Test]
        public void GetGridSummary()
        {
            Http.Get($"{baseUrl}/getGridSummary");
        }

        [Test]
        public void GetNatureAreaInstitutionSummary()
        {
            Http.Post($"{baseUrl}/GetNatureAreaInstitutionSummary",
                @"{""NatureLevelCodes"":[],""CenterPoints"":false,""NatureAreaTypeCodes"":[],""DescriptionVariableCodes"":[],""Municipalities"":[],""Counties"":[],""ConservationAreas"":[],""Institutions"":[],""Geometry"":"""",""BoundingBox"":"""",""EpsgCode"":""3857"",""IndexFrom"":0,""IndexTo"":25,""ForceRefreshToggle"":false}");
        }

        [Test]
        public void GetNatureAreasBySearchFilter_Close()
        {
            Http.Post($"{baseUrl}/GetNatureAreasBySearchFilter",
                @"{""NatureLevelCodes"":[],""CenterPoints"":false,""NatureAreaTypeCodes"":[],""DescriptionVariableCodes"":[],""Municipalities"":[],""Counties"":[],""ConservationAreas"":[],""Institutions"":[],""Geometry"":"""",""BoundingBox"":""POLYGON ((1146350.6203588946 9080740.320878485,1154300.0713005527 9080740.320878485,1154300.0713005527 9089721.671701992,1146350.6203588946 9089721.671701992,1146350.6203588946 9080740.320878485))"",""EpsgCode"":""3857"",""IndexFrom"":0,""IndexTo"":25,""ForceRefreshToggle"":false}");
        }

        [Test]
        public void GetNatureAreasBySearchFilter_Distant()
        {
            Http.Post($"{baseUrl}/GetNatureAreasBySearchFilter",
                @"{""NatureLevelCodes"":[],""CenterPoints"":true,""NatureAreaTypeCodes"":[],""DescriptionVariableCodes"":[],""Municipalities"":[],""Counties"":[],""ConservationAreas"":[],""Institutions"":[],""Geometry"":"""",""BoundingBox"":"""",""EpsgCode"":""3857"",""IndexFrom"":0,""IndexTo"":25,""ForceRefreshToggle"":false}");
        }

        [Test]
        public void GetNatureAreaSummary()
        {
            Http.Post($"{baseUrl}/GetNatureAreaSummary",
                @"{""NatureLevelCodes"":[],""CenterPoints"":true,""NatureAreaTypeCodes"":[],""DescriptionVariableCodes"":[],""Municipalities"":[],""Counties"":[],""ConservationAreas"":[],""Institutions"":[],""Geometry"":"""",""BoundingBox"":"""",""EpsgCode"":""3857"",""IndexFrom"":0,""IndexTo"":25,""ForceRefreshToggle"":false}");
        }

        [Test]
        public void SearchAreas()
        {
            Http.Get($"{baseUrl}/SearchAreas/?name=tr&areatype=0");
        }
    }
}
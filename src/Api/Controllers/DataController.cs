using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using Api.Responses;
using Common;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nin;
using Nin.Api.Requests;
using Nin.Api.Responses;
using Nin.Aspnet;
using Nin.Dataleveranser.Rutenett;
using Nin.GeoJson;
using Nin.IO;
using Nin.IO.Excel;
using Nin.IO.RavenDb;
using Nin.IO.SqlServer;
using Nin.IO.Xml;
using Nin.Naturtyper;
using Nin.Områder;
using Nin.Types.MsSql;
using Raven.Abstractions.Extensions;
using Types;
using Dataleveranse = Nin.Types.RavenDb.Dataleveranse;
using System.Linq;
using Nin.Configuration;

namespace Api.Controllers
{
    public class DataController
    {
        private readonly GmlWriter gmlWriter = new GmlWriter();
        private readonly JsonSerializerSettings jsonSerializerSettings;
        private readonly NinRavenDb ninRavenDb;
        private readonly XmlConverter xmlConverter = new XmlConverter();

        public DataController()
        {
            ninRavenDb = new NinRavenDb();

            jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        [HttpGet]
        public NinHtmlResult Index()
        {
            return new NinHtmlResult(@"<html>
            <ul>
            <li><a href=""../Naturkoder"">Naturkoder</a></li>
            <li><a href=""GetGridSummary"">GetGridSummary</a></li>
            </ul>
            </html>");
        }

        [HttpGet]
        public Collection<GridSummaryItem> GetGridSummary()
        {
            var gridSummary = SqlServer.GetGridSummary();
            var areaLayerSummary = SqlServer.GetAreaLayerSummary();

            var gridSummaryItems = new Collection<GridSummaryItem>();

            foreach (var gridItem in gridSummary)
            {
                var gridSummaryItem = new GridSummaryItem(gridItem.Item1);
                foreach (var gridLayerItem in gridItem.Item2)
                {
                    var ninCode = Naturkodetrær.Naturvariasjon.HentFraKode(gridLayerItem.Code.Value);

                    var gridLayerSummaryItem = new GridLayerSummaryItem
                    {
                        Id = gridLayerItem.Id,
                        DocGuid = gridLayerItem.DocGuid,
                        Name = gridLayerItem.Name,
                        Description = gridLayerItem.Description,
                        Established = gridLayerItem.Established,
                        MinValue = gridLayerItem.MinValue,
                        MaxValue = gridLayerItem.MaxValue,
                        Code = gridLayerItem.Code.Value
                    };

                    if (ninCode != null)
                    {
                        gridLayerSummaryItem.CodeDescription = ninCode.Name;
                        gridLayerSummaryItem.CodeUrl = ninCode.Url;
                    }

                    gridLayerSummaryItem.Owner = gridLayerItem.Owner;
                    gridLayerSummaryItem.Documents = gridLayerItem.Documents;

                    gridSummaryItem.GridLayers.Add(gridLayerSummaryItem);
                }
                gridSummaryItems.Add(gridSummaryItem);
            }

            foreach (var areaItem in areaLayerSummary)
            {
                var areaSummaryItem = new GridSummaryItem(areaItem.Item1);
                foreach (var areaLayerItem in areaItem.Item2)
                {
                    var ninCode = Naturkodetrær.Naturvariasjon.HentFraKode(areaLayerItem.Code.Value);

                    var areaLayerSummaryItem = new GridLayerSummaryItem
                    {
                        Id = areaLayerItem.Id,
                        DocGuid = areaLayerItem.DocGuid,
                        Name = areaLayerItem.Name,
                        Description = areaLayerItem.Description,
                        Established = areaLayerItem.Established,
                        MinValue = areaLayerItem.MinValue,
                        MaxValue = areaLayerItem.MaxValue,
                        Code = areaLayerItem.Code.Value
                    };

                    if (ninCode != null)
                    {
                        areaLayerSummaryItem.CodeDescription = ninCode.Name;
                        areaLayerSummaryItem.CodeUrl = ninCode.Url;
                    }

                    areaLayerSummaryItem.Owner = areaLayerItem.Owner;
                    areaLayerSummaryItem.Documents = areaLayerItem.Documents;

                    areaSummaryItem.GridLayers.Add(areaLayerSummaryItem);
                }
                gridSummaryItems.Add(areaSummaryItem);
            }

            return gridSummaryItems;
        }

        private static object cachedNatureAreaSummary = null;

        [HttpPost]
        public object GetNatureAreaSummary([FromBody] AreaFilterRequest areaFilterRequest)
        {
            if(string.IsNullOrWhiteSpace(areaFilterRequest.Geometry) && cachedNatureAreaSummary != null)
            {
                return cachedNatureAreaSummary;
            }

            var geometry = "";

            if (areaFilterRequest != null)
                geometry = areaFilterRequest.Geometry;

            var natureAreaTypes = SqlServer.GetNatureAreaSummary(geometry);
            var natureAreaTypeHash = new CodeIds();
            var decriptionVariableHash = new CodeIds();

            foreach (var natureAreaTypeItem in natureAreaTypes)
                if (natureAreaTypeItem.Item3)
                {
                    if (natureAreaTypeHash.ContainsKey(natureAreaTypeItem.Item1))
                        natureAreaTypeHash[natureAreaTypeItem.Item1].Add(natureAreaTypeItem.Item2);
                    else
                        natureAreaTypeHash[natureAreaTypeItem.Item1] = new HashSet<int> { natureAreaTypeItem.Item2 };
                }
                else
                {
                    if (decriptionVariableHash.ContainsKey(natureAreaTypeItem.Item1))
                        decriptionVariableHash[natureAreaTypeItem.Item1].Add(natureAreaTypeItem.Item2);
                    else
                        decriptionVariableHash[natureAreaTypeItem.Item1] = new HashSet<int> { natureAreaTypeItem.Item2 };
                }

            CodeSummaryItem natureAreaTypeSummary = GetNatureAreaTypeSummary(natureAreaTypeHash);
            CodeSummaryItem descriptionVariableSummary = GetDescriptionVariableSummary(decriptionVariableHash);

            var r = new NatureAreaSummary
            {
                NatureAreaTypes = natureAreaTypeSummary,
                DescriptionVariables = descriptionVariableSummary
            };

            var jo = JObject.FromObject(r);

            RemoveFields(jo, "HandledIds", true);
            if (jo.First != null)
            {
                RemoveFields(jo.First.First, "Name", false);
                RemoveFields(jo.First.First, "Url", false);
                RemoveFields(jo.First.First, "Count", false);
            }
            if (jo.Last != null)
            {
                RemoveFields(jo.Last.First, "Name", false);
                RemoveFields(jo.Last.First, "Url", false);
                RemoveFields(jo.Last.First, "Count", false);
            }

            if (string.IsNullOrWhiteSpace(areaFilterRequest.Geometry))
            {
                cachedNatureAreaSummary = jo;
            }

            return jo;
        }

        private static CodeSummaryItem GetDescriptionVariableSummary(CodeIds decriptionVariableHash)
        {
            return GetCodeSummaryHierarchy(decriptionVariableHash, Naturkodetrær.Naturvariasjon);
        }

        private static CodeSummaryItem GetNatureAreaTypeSummary(CodeIds natureAreaTypeHash)
        {
            return GetCodeSummaryHierarchy(natureAreaTypeHash, Naturkodetrær.Naturtyper);
        }

        [HttpPost]
        public object GetNatureAreaInstitutionSummary([FromBody] AreaFilterRequest areaFilterRequest)
        {
            var geometry = "";
            var epsgCode = 0;

            if (areaFilterRequest != null)
            {
                geometry = areaFilterRequest.Geometry;
                epsgCode = areaFilterRequest.EpsgCode == null ? default(int) : int.Parse(areaFilterRequest.EpsgCode);
            }

            var natureAreaInstitutionSummary = SqlServer.GetNatureAreaInstitutionSummary(geometry, epsgCode);

            var natureAreaInstitutionSummaryItems = new Collection<NatureAreaSummaryItem>();
            foreach (var natureAreaInstitution in natureAreaInstitutionSummary)
                natureAreaInstitutionSummaryItems.Add(
                    new NatureAreaSummaryItem
                    {
                        Name = natureAreaInstitution.Item1,
                        NatureAreaCount = natureAreaInstitution.Item2
                    }
                );

            return natureAreaInstitutionSummaryItems;
        }

        [HttpPost]
        public object GetAreaSummary([FromBody] AreaFilterRequest areaFilterRequest)
        {
            var geometry = "";
            var epsgCode = 0;

            if (areaFilterRequest != null)
            {
                geometry = areaFilterRequest.Geometry;
                epsgCode = areaFilterRequest.EpsgCode == null ? default(int) : int.Parse(areaFilterRequest.EpsgCode);
            }

            var areaSummary = SqlServer.GetAreaSummary(geometry, epsgCode);
            var r = new AreaSummary
            {
                AreaCount = SqlServer.GetAreaSummaryCount(AreaType.Fylke, geometry, epsgCode),
                ConservationAreaCount = SqlServer.GetAreaSummaryCount(AreaType.Verneområde, geometry, epsgCode)
            };


            foreach (var areaSummaryItem in areaSummary)
                if (areaSummaryItem.Item4 == AreaType.Fylke)
                    r.Areas.Add(areaSummaryItem.Item1,
                        new AreaSummaryItem(areaSummaryItem.Item2, areaSummaryItem.Item5));

            foreach (var areaSummaryItem in areaSummary)
            {
                if (areaSummaryItem.Item4 != AreaType.Kommune) continue;
                var countyNumber = areaSummaryItem.Item1 / 100;
                if (r.Areas.ContainsKey(countyNumber))
                    r.Areas[countyNumber]
                        .Areas.Add(areaSummaryItem.Item1,
                            new AreaSummaryItem(areaSummaryItem.Item2, areaSummaryItem.Item5));
            }

            foreach (var areaSummaryItem in areaSummary)
            {
                if (areaSummaryItem.Item4 != AreaType.Verneområde) continue;
                var categoryName = Områdetyper.KodeTilNavn(areaSummaryItem.Item3);
                var category = areaSummaryItem.Item3;
                if (areaSummaryItem.Item3 == "Andre")
                    category = "OTHERS";

                if (r.ConservationAreas.ContainsKey(category))
                {
                    r.ConservationAreas[category].NatureAreaCount += areaSummaryItem.Item5;
                    r.ConservationAreas[category]
                        .Areas.Add(areaSummaryItem.Item1,
                            new AreaSummaryItem(areaSummaryItem.Item2, areaSummaryItem.Item5));
                }
                else
                {
                    var conservationAreaSummaryItem = new AreaSummaryItem(categoryName, areaSummaryItem.Item5);
                    conservationAreaSummaryItem.Areas.Add(areaSummaryItem.Item1,
                        new AreaSummaryItem(areaSummaryItem.Item2, areaSummaryItem.Item5));
                    r.ConservationAreas.Add(category, conservationAreaSummaryItem);
                }
            }

            var areaHierarchicallySummaryJson = JObject.FromObject(r);
            return areaHierarchicallySummaryJson;
        }

        [HttpGet]
        public object GetAreas(int areatype, int number)
        {
            var areas = SqlServer.GetAreas((AreaType)areatype, 0, number);
            return GeoJsonConverter.AreasToGeoJson(areas, false);
        }

        [HttpGet]
        public IActionResult SearchAreas(string name, int areatype)
        {
            var areas = SqlServer.SearchAreas((AreaType)areatype, name);
            var searchResultAsJson = JsonConvert.SerializeObject(areas, jsonSerializerSettings);
            var contentResult = new NinJsonResult(searchResultAsJson);
            return contentResult;
        }

        [HttpPost]
        public object GetGrid([FromBody] GridFilterRequest gridFilterRequest)
        {
            var gridType = RutenettType.Undefined;
            var areaType = AreaType.Undefined;
            var areaRequest = false;
            if (gridFilterRequest == null)
                throw new Exception("Type rutenett er ikke spesifisert.");

            switch (gridFilterRequest.GridType)
            {
                case "Undefined":
                    gridType = RutenettType.Undefined;
                    break;
                case "SSB0250M":
                    gridType = RutenettType.SSB0250M;
                    break;
                case "SSB0500M":
                    gridType = RutenettType.SSB0500M;
                    break;
                case "SSB001KM":
                    gridType = RutenettType.SSB001KM;
                    break;
                case "SSB002KM":
                    gridType = RutenettType.SSB002KM;
                    break;
                case "SSB005KM":
                    gridType = RutenettType.SSB005KM;
                    break;
                case "SSB010KM":
                    gridType = RutenettType.SSB010KM;
                    break;
                case "SSB025KM":
                    gridType = RutenettType.SSB025KM;
                    break;
                case "SSB050KM":
                    gridType = RutenettType.SSB050KM;
                    break;
                case "SSB100KM":
                    gridType = RutenettType.SSB100KM;
                    break;
                case "SSB250KM":
                    gridType = RutenettType.SSB250KM;
                    break;
                case "SSB500KM":
                    gridType = RutenettType.SSB500KM;
                    break;
                case "Kommune":
                    areaType = AreaType.Kommune;
                    areaRequest = true;
                    break;
                case "Fylke":
                    areaType = AreaType.Fylke;
                    areaRequest = true;
                    break;
                default:
                    throw new Exception("Ukjent rutenett '" + gridFilterRequest.GridType + "'.");
            }

            if (areaRequest)
            {
                var areaGrid = SqlServer.GetAreas(areaType, gridFilterRequest.GridLayerTypeId);
                return GeoJsonConverter.AreasToGeoJson(areaGrid, gridFilterRequest.GridLayerTypeId != 0);
            }

            var grid = SqlServer.GetGrid(
                gridType,
                gridFilterRequest.Municipalities,
                gridFilterRequest.Counties,
                gridFilterRequest.Geometry,
                gridFilterRequest.BoundingBox,
                gridFilterRequest.EpsgCode,
                gridFilterRequest.GridLayerTypeId
            );

            return GeoJsonConverter.GridToGeoJson(grid, gridFilterRequest.GridLayerTypeId != 0);
        }

        [HttpPost]
        public string GetNatureAreasBySearchFilter([FromBody] SearchFilterRequest searchFilterRequest)
        {
            var natureAreas = SqlServer.GetNatureAreasBySearchFilter(searchFilterRequest);
            var r = GeoJsonConverter.NatureAreasToGeoJson(natureAreas, !searchFilterRequest.CenterPoints);
            return r;
        }

        [HttpPost]
        public string GetNatureAreasBySearchFilterV2([FromBody] SearchFilterRequest searchFilterRequest)
        {
            var search = new SearchV2(Config.Settings.ConnectionString);

            var list = search.GetNatureAreasBySearchFilter(searchFilterRequest);

            var geoJson = GeoJsonConverter.NatureAreasToGeoJson(list, !searchFilterRequest.CenterPoints);
            
            return geoJson;
        }

        [HttpPost]
        public NatureAreaList GetNatureAreaInfosBySearchFilter([FromBody] SearchFilterRequest request)
        {
            var natureLevels = request.AnalyzeSearchFilterRequest();

            int natureAreaCount;
            var natureAreas = SqlServer.GetNatureAreasBySearchFilter(
                natureLevels,
                request.NatureAreaTypeCodes,
                request.DescriptionVariableCodes,
                request.Municipalities,
                request.Counties,
                request.ConservationAreas,
                request.Institutions,
                request.Geometry,
                "",
                request.EpsgCode,
                true,
                2,
                request.IndexFrom,
                request.IndexTo,
                out natureAreaCount
            );

            var natureAreaList = new NatureAreaList { NatureAreaCount = natureAreaCount };

            foreach (var natureArea in natureAreas)
            {
                var natureAreaExport = (NatureAreaExport)natureArea;
                AddCodeHierarchyInfo(natureAreaExport.Parameters);

                var natureAreaListItem = new NatureAreaListItem { LocalId = natureAreaExport.UniqueId.LocalId };

                var natureLevelCode = Naturkodetrær.Naturtyper.HentFraKode(natureAreaExport.Nivå);
                if (natureLevelCode != null)
                {
                    natureAreaListItem.NatureLevelCode = natureLevelCode.Id;
                    natureAreaListItem.NatureLevelDescription = natureLevelCode.Name;
                    natureAreaListItem.NatureLevelUrl = natureLevelCode.Url;
                }

                natureAreaListItem.Parameters = natureAreaExport.Parameters;
                HandleCustomVariables(natureAreaListItem.Parameters, natureArea.UniqueId.LocalId);

                natureAreaListItem.SurveyScale = natureAreaExport.MetadataSurveyScale;
                if (natureAreaExport.Surveyed.HasValue)
                    natureAreaListItem.SurveyedYear = natureAreaExport.Surveyed.Value.Year;

                natureAreaListItem.Contractor = natureAreaExport.MetadataContractor;
                natureAreaListItem.Surveyer = natureAreaExport.Surveyer != null
                    ? natureAreaExport.Surveyer.Company
                    : "";
                natureAreaListItem.Program = natureAreaExport.MetadataProgram;
                natureAreaList.NatureAreas.Add(natureAreaListItem);
            }

            //var natureAreaListItemsJson = JsonConvert.SerializeObject(natureAreaList, jsonSerializerSettings);
            //return natureAreaListItemsJson;
            return natureAreaList;
        }

        [HttpPost]
        public object GetNatureAreaStatisticsBySearchFilter([FromBody] SearchFilterRequest request)
        {
            var natureLevels = request.AnalyzeSearchFilterRequest();

            int natureAreaCount;
            var natureAreas = SqlServer.GetNatureAreasBySearchFilter(
                natureLevels,
                request.NatureAreaTypeCodes,
                request.DescriptionVariableCodes,
                request.Municipalities,
                request.Counties,
                request.ConservationAreas,
                request.Institutions,
                request.Geometry,
                "",
                request.EpsgCode,
                true,
                1,
                0,
                int.MaxValue,
                out natureAreaCount
            );

            var institutions = new Dictionary<string, int>();
            var surveyedYears = new Dictionary<int, int>();

            foreach (var natureArea in natureAreas)
            {
                if (institutions.ContainsKey(natureArea.Institution))
                    institutions[natureArea.Institution]++;
                else
                    institutions[natureArea.Institution] = 1;
                if (!natureArea.Surveyed.HasValue) continue;
                if (surveyedYears.ContainsKey(natureArea.Surveyed.Value.Year))
                    surveyedYears[natureArea.Surveyed.Value.Year]++;
                else
                    surveyedYears[natureArea.Surveyed.Value.Year] = 1;
            }

            var natureAreaTypeHash = new CodeIds();

            foreach (var natureArea in natureAreas)
                foreach (var parameter in natureArea.Parameters)
                {
                    var natureAreaType = parameter as NatureAreaType;
                    if (natureAreaType != null)
                        natureAreaTypeHash.Add(natureAreaType, natureArea);
                }

            var natureAreaTypeSummary = GetCodeSummaryHierarchy(natureAreaTypeHash, Naturkodetrær.Naturtyper);

            var natureAreaStatistics = new NatureAreaStatistics { NatureAreaTypes = natureAreaTypeSummary };

            foreach (var institution in institutions)
                natureAreaStatistics.Institutions.Add(new NatureAreaSummaryItem
                {
                    Name = institution.Key,
                    NatureAreaCount = institution.Value
                });
            foreach (var surveyYear in surveyedYears)
                natureAreaStatistics.SurveyYears.Add(new NatureAreaSurveyYearSummaryItem
                {
                    Year = surveyYear.Key,
                    NatureAreaCount = surveyYear.Value
                });

            var natureAreaStatisticsJson = JObject.FromObject(natureAreaStatistics);

            RemoveFields(natureAreaStatisticsJson, "HandledIds", true);
            if (natureAreaStatisticsJson.First == null) return natureAreaStatisticsJson;

            RemoveFields(natureAreaStatisticsJson.First.First, "Name", false);
            RemoveFields(natureAreaStatisticsJson.First.First, "Url", false);
            RemoveFields(natureAreaStatisticsJson.First.First, "Count", false);

            return natureAreaStatisticsJson;
        }

        [HttpGet]
        public object GetNatureAreaByLocalId(string id)
        {
            var natureArea = SqlServer.GetNatureAreaByLocalId(new Guid(id));
            AddCodeHierarchyInfo(natureArea.Parameters);
            return natureArea;
        }

        [HttpGet]
        public object GetMetadataByNatureAreaLocalId(string id)
        {
            var metadatas = SqlServer.GetMetadatasByNatureAreaLocalIds(new Collection<string> { id }, false);

            if (metadatas.Count > 0)
            {
                return metadatas[0];
            }
            return "";
        }

        [HttpGet]
        public IActionResult GetMetadatasByNatureAreaLocalIds([FromBody] LocalIdRequest localIdRequest)
        {
            var metadatas = SqlServer.GetMetadatasByNatureAreaLocalIds(localIdRequest.LocalIds, true);

            var metadatasJson = JsonConvert.SerializeObject(metadatas, jsonSerializerSettings);
            var contentResult = new NinJsonResult(metadatasJson);
            return contentResult;
        }

        [HttpGet]
        public IActionResult GetExpiredMetadatasByNatureAreaLocalId(string localId)
        {
            var metadatas = SqlServer.GetMetadatasByNatureAreaLocalIds(new Collection<string> { localId }, false);

            var dataDeliveriesWithNatureArea = new List<Dataleveranse>();

            if (metadatas.Count == 1)
            {
                var dataDeliveries = ninRavenDb.HentDataleveranserGjeldendeOgUtgåtte(metadatas[0].UniqueId.LocalId);

                foreach (var dataDelivery in dataDeliveries)
                    foreach (var natureArea in dataDelivery.Metadata.NatureAreas)
                    {
                        if (!natureArea.UniqueId.LocalId.Equals(new Guid(localId))) continue;

                        dataDelivery.Metadata.NatureAreas.Clear();
                        var natureAreaExport = new Nin.Types.RavenDb.NatureAreaExport(natureArea);
                        AddCodeHierarchyInfo(natureAreaExport.Parameters);
                        dataDelivery.Metadata.NatureAreas.Add(natureAreaExport);
                        MapProjection.ConvertGeometry(dataDelivery, metadatas[0].GetAreaEpsgCode());
                        natureAreaExport.Areas =
                            SqlServer.GetAreaLinkInfos(natureAreaExport.Area, natureAreaExport.AreaEpsgCode);
                        dataDeliveriesWithNatureArea.Add(dataDelivery);
                        break;
                    }
            }

            var metadatasJson = JsonConvert.SerializeObject(dataDeliveriesWithNatureArea, jsonSerializerSettings);

            var contentResult = new NinJsonResult(metadatasJson);
            return contentResult;
        }

        [HttpGet]
        public FileStreamResult ExportNatureAreasByLocalIds([FromBody] LocalIdRequest localIdRequest)
        {
            var metadatas = SqlServer.GetMetadatasByNatureAreaLocalIds(localIdRequest.LocalIds, true);
            var xDocument = xmlConverter.ToXml(metadatas);

            Stream xmlStream = new MemoryStream();
            xDocument.Save(xmlStream);
            xmlStream.Position = 0;

            return new FileStreamResult(xmlStream, "application/xml");
        }

        [HttpPost]
        public FileStreamResult ExportNatureAreasBySearchFilter([FromBody] SearchFilterRequest searchFilterRequest)
        {
            var metadatas = FindMetadatasBySearchFilter(searchFilterRequest);

            var xDocument = xmlConverter.ToXml(metadatas);
            Stream xmlStream = new MemoryStream();
            xDocument.Save(xmlStream);
            xmlStream.Position = 0;

            return new FileStreamResult(xmlStream, "application/xml");
        }

        [HttpPost]
        public FileStreamResult ExportNatureAreasAsShapeBySearchFilter(
            [FromBody] SearchFilterRequest searchFilterRequest)
        {
            int epsgCode;
            var natureAreas = FindNatureAreasBySearchFilter(searchFilterRequest, out epsgCode);

            var zipMemoryStream = ShapeGenerator.GenerateShapeFile(natureAreas, epsgCode);

            return new FileStreamResult(zipMemoryStream, "application/zip");
        }

        [HttpPost]
        public string ExportNatureAreasAsGmlBySearchFilter([FromBody] SearchFilterRequest searchFilterRequest)
        {
            int epsgCode;
            var natureAreas = FindNatureAreasBySearchFilter(searchFilterRequest, out epsgCode);

            var xDocument = gmlWriter.ConvertToGml(natureAreas);
            var builder = new StringBuilder();
            using (TextWriter writer = new StringWriter(builder))
            {
                xDocument.Save(writer);
            }
            return builder.ToString();
            //Stream xmlStream = new MemoryStream();
            //xDocument.Save(xmlStream);
            //xmlStream.Position = 0;
            //return new DownloadFileResult("natur.xml", xmlStream, "application/xml");
        }

        [HttpPost]
        public FileStreamResult ExportNatureAreasAsXlsxBySearchFilter(
            [FromBody] SearchFilterRequest searchFilterRequest)
        {
            var natureAreas = FindNatureAreasBySearchFilter(searchFilterRequest);
            var excelGenerator = new ExcelGenerator(Naturkodetrær.Naturvariasjon);
            var xlsxMemoryStream = excelGenerator.GenerateXlsxStream(natureAreas);
            return new FileStreamResult(xlsxMemoryStream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        private static void HandleCustomVariables(IEnumerable<Parameter> parameters, Guid natureAreaLocalId)
        {
            foreach (var parameter in parameters)
                HandleCustomVariable(natureAreaLocalId, parameter);
        }

        private static void HandleCustomVariable(Guid natureAreaLocalId, Parameter parameter)
        {
            if (parameter.GetType() != typeof(NatureAreaType)) return;
            var natureAreaType = (NatureAreaType)parameter;
            if (natureAreaType.CustomVariables.Count <= 0) return;

            var metadatas = SqlServer.GetMetadatasByNatureAreaLocalIds(
                new Collection<string> { natureAreaLocalId.ToString() },
                false);
            if (metadatas.Count <= 0) return;
            for (var i = 0; i < natureAreaType.CustomVariables.Count; ++i)
                foreach (var variableDefinition in metadatas[0].VariabelDefinitions)
                {
                    if (variableDefinition.GetType() != typeof(CustomVariableDefinition)) continue;

                    var customVariableDefinition = (CustomVariableDefinition)variableDefinition;
                    if (
                        !customVariableDefinition.Specification.Equals(
                            natureAreaType.CustomVariables[i].Specification)) continue;

                    var customVariableExport =
                        new CustomVariableExport(natureAreaType.CustomVariables[i])
                        {
                            Description = customVariableDefinition.Description
                        };
                    natureAreaType.CustomVariables[i] = customVariableExport;
                    break;
                }
        }

        private static void RemoveFields(JToken token, string field, bool recursive)
        {
            var container = token as JContainer;
            if (container == null) return;

            var removeList = new List<JToken>();
            foreach (var el in container.Children())
            {
                var p = el as JProperty;
                if (p != null && field.Equals(p.Name))
                    removeList.Add(el);
                if (recursive)
                    RemoveFields(el, field, true);
            }

            foreach (var el in removeList)
                el.Remove();
        }

        private static void AddCodeHierarchyInfo(IEnumerable<Parameter> parameters)
        {
            foreach (var parameter in parameters)
                AddCodeHierarchyInfo(parameter);
        }

        private static void AddCodeHierarchyInfo(Parameter parameter)
        {
            CodeItem ninCode;
            if (parameter.GetType() == typeof(NatureAreaType))
            {
                var natureAreaType = (NatureAreaType)parameter;
                ninCode = Naturkodetrær.Naturtyper.HentFraKode(natureAreaType.Code);
                foreach (var additionalParameter in natureAreaType.AdditionalVariables)
                    AddCodeHierarchyInfo(additionalParameter);
            }
            else if (parameter.GetType() == typeof(Nin.Types.RavenDb.NatureAreaType))
            {
                var natureAreaType = (Nin.Types.RavenDb.NatureAreaType)parameter;
                ninCode = Naturkodetrær.Naturtyper.HentFraKode(natureAreaType.Code);
                foreach (var additionalParameter in natureAreaType.AdditionalVariables)
                    AddCodeHierarchyInfo(additionalParameter);
            }
            else
            {
                ninCode = Naturkodetrær.Naturvariasjon.HentFraKode(parameter.Code);
            }

            if (ninCode == null) return;

            parameter.CodeDescription = ninCode.Name;
            parameter.CodeUrl = ninCode.Url;
            if (ninCode.ParentCodeItems.Count >= 2)
            {
                parameter.MainTypeCode = ninCode.ParentCodeItems[1].Id;
                parameter.MainTypeDescription = ninCode.ParentCodeItems[1].Name;
                parameter.MainTypeCodeUrl = ninCode.ParentCodeItems[1].Url;
            }
            else if (ninCode.ParentCodeItems.Count == 1)
            {
                parameter.MainTypeCode = ninCode.Id;
                parameter.MainTypeDescription = ninCode.Name;
                parameter.MainTypeCodeUrl = ninCode.Url;
            }
        }

        [HttpGet]
        private static Collection<NatureArea> FindNatureAreasBySearchFilter(SearchFilterRequest searchFilterRequest,
            out int epsgCode)
        {
            var metadatas = FindMetadatasBySearchFilter(searchFilterRequest);

            var natureAreas = new Collection<NatureArea>();
            epsgCode = -1;
            foreach (var metadata in metadatas)
            {
                if (epsgCode == -1) epsgCode = metadata.GetAreaEpsgCode();
                natureAreas.AddRange(metadata.NatureAreas);
            }

            return natureAreas;
        }

        [HttpGet]
        private static Collection<NatureAreaExport> FindNatureAreasBySearchFilter(
            SearchFilterRequest searchFilterRequest)
        {
            var metadatas = FindMetadatasBySearchFilter(searchFilterRequest);

            var natureAreaExports = new Collection<NatureAreaExport>();
            foreach (var metadata in metadatas)
                foreach (var natureArea in metadata.NatureAreas)
                {
                    var natureAreaExport =
                        new NatureAreaExport(natureArea)
                        {
                            MetadataContractor = metadata.Contractor.Company,
                            MetadataProgram = metadata.Program,
                            MetadataSurveyScale = metadata.SurveyScale
                        };
                    natureAreaExports.Add(natureAreaExport);
                }

            return natureAreaExports;
        }

        [HttpGet]
        private static IEnumerable<Metadata> FindMetadatasBySearchFilter(SearchFilterRequest searchFilterRequest)
        {
            var natureLevels = searchFilterRequest.AnalyzeSearchFilterRequest();

            return SqlServer.GetMetadatasBySearchFilter(
                natureLevels,
                searchFilterRequest.NatureAreaTypeCodes,
                searchFilterRequest.DescriptionVariableCodes,
                searchFilterRequest.Municipalities,
                searchFilterRequest.Counties,
                searchFilterRequest.ConservationAreas,
                searchFilterRequest.Institutions,
                searchFilterRequest.Geometry,
                "",
                searchFilterRequest.EpsgCode
            );
        }

        private static CodeSummaryItem GetCodeSummaryHierarchy(CodeIds idsForCodes, Naturetypekodetre kodetre)
        {
            var root = new CodeSummaryItem();

            var index = new Dictionary<string, CodeSummaryItem>();

            foreach (var idsForCode in idsForCodes)
            {
                var codeItemFromTree = kodetre.HentFraKode(idsForCode.Key);

                if (codeItemFromTree.Name == "?")
                {
                    continue;
                }

                HandleParents(root.Codes, index, codeItemFromTree);

                CodeSummaryItem item = null;

                if (!index.ContainsKey(codeItemFromTree.Id))
                {
                    item = new CodeSummaryItem(codeItemFromTree.Name, codeItemFromTree.Url, idsForCode.Value.Count);

                    if (codeItemFromTree.ParentCodeItems.Count > 0)
                    {
                        var parent = index[codeItemFromTree.ParentCodeItems.Last().Id];

                        parent.Codes.Add(codeItemFromTree.Id, item);
                    }
                    else
                    {
                        root.Codes.Add(codeItemFromTree.Id, item);
                    }

                    index.Add(codeItemFromTree.Id, item);
                }
                else
                {
                    item = index[codeItemFromTree.Id];
                    item.OwnCount += idsForCode.Value.Count;
                }
            }

            return root;
        }

        private static void HandleParents(Dictionary<string, CodeSummaryItem> rootCodes, Dictionary<string, CodeSummaryItem> index, CodeItem codeItemFromTree)
        {
            CodeSummaryItem item = null;

            for (int i = 0; i < codeItemFromTree.ParentCodeItems.Count; i++)
            {
                var parent = codeItemFromTree.ParentCodeItems[i];
                item = new CodeSummaryItem(parent.Name, parent.Url, 0);

                if (i == 0 && !rootCodes.ContainsKey(parent.Id))
                {
                    rootCodes.Add(parent.Id, item);
                    index.Add(parent.Id, item);
                }
                else if (!index.ContainsKey(parent.Id))
                {
                    var parentOfParent = index[codeItemFromTree.ParentCodeItems[i - 1].Id];

                    parentOfParent.Codes.Add(parent.Id, item);

                    index.Add(parent.Id, item);
                }
            }
        }
    }

internal class CodeIds : Dictionary<string, HashSet<int>>
    {
        public void Add(NatureAreaType natureAreaType, NatureArea natureArea)
        {
            if (!ContainsKey(natureAreaType.Code))
                this[natureAreaType.Code] = new HashSet<int> {natureArea.Id};
            else
                this[natureAreaType.Code].Add(natureArea.Id);
        }
    }
}
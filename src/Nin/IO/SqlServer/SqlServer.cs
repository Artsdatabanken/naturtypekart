using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Common.Rutenett;
using Microsoft.SqlServer.Types;
using Nin.Configuration;
using Nin.Dataleveranser;
using Nin.Dataleveranser.Rutenett;
using Nin.Diagnostic;
using Nin.Områder;
using Nin.Tasks;
using Nin.Types.GridTypes;
using Nin.Types.MsSql;
using Raven.Abstractions.Extensions;
using Types;
using Code = Nin.Types.MsSql.Code;
using Contact = Nin.Types.MsSql.Contact;
using CustomVariable = Nin.Types.MsSql.CustomVariable;
using CustomVariableDefinition = Nin.Types.MsSql.CustomVariableDefinition;
using DescriptionVariable = Nin.Types.MsSql.DescriptionVariable;
using Document = Nin.Types.MsSql.Document;
using Identification = Nin.Types.MsSql.Identification;
using Metadata = Nin.Types.MsSql.Metadata;
using NatureArea = Nin.Types.MsSql.NatureArea;
using NatureAreaType = Nin.Types.MsSql.NatureAreaType;
using Quality = Nin.Types.MsSql.Quality;
using NinStandardVariabel = Nin.Types.MsSql.NinStandardVariabel;

namespace Nin.IO.SqlServer
{
    /// <summary>
    /// Hele denne klassen er teknisk gjeld og bør skrives om og testdekkes.
    /// </summary>
    public static class SqlServer
    {
        public static void LagreDataleveranse(Dataleveranse dataleveranse)
        {
            if (dataleveranse.Operator != null)
                dataleveranse.Operator.Id = StoreContact(dataleveranse.Operator);

            const string sql = "INSERT INTO Dataleveranse (doc_id,name,leveranseDato,operatør_id,begrunnelseForEndring,beskrivelse,parent_id,opprettet) OUTPUT (Inserted.id) VALUES (@doc_id,@name,@deliveryDate,@operator_id,@reasonForChange,@description,@parent_id,@created)";
            using (var cmd = SqlStatement(sql))
            {
                Log.d("LDL", "Id: " + dataleveranse.Id);
                cmd.AddParameter("@doc_id", dataleveranse.Id);
                cmd.AddParameter("@name", dataleveranse.Name);
                cmd.AddParameter("@deliveryDate", dataleveranse.DeliveryDate);
                cmd.AddParameter("@operator_id", dataleveranse.Operator?.Id ?? null);
                cmd.AddParameter("@reasonForChange", dataleveranse.ReasonForChange);
                cmd.AddParameter("@description", dataleveranse.Description);
                cmd.AddParameter("@parent_id", dataleveranse.ParentId);
                cmd.AddParameter("@created", dataleveranse.Created);

                dataleveranse.DataId = (int)cmd.ExecuteScalar();
            }
            dataleveranse.Metadata.Id = LagreDataleveranse(dataleveranse.DataId, dataleveranse.Metadata);
        }

        public static void StoreAreas(IEnumerable<Area> areas)
        {
            foreach (var area in areas)
                StoreArea(area);
        }

        public static void StoreArea(Area area)
        {
            const string sql = "INSERT INTO Område (" +
                               "geometriType_id, " +
                               "nummer," +
                               "navn," +
                               "kategori, " +
                               "geometri" +
                               ") VALUES (" +
                               "@areaType_id," +
                               "@number," +
                               "@name," +
                               "@category, " +
                               "@area" +
                               ")";

            using (var cmd = SqlStatement(sql))
            {
                cmd.AddParameter("@areaType_id", (int)area.Type);
                cmd.AddParameter("@number", area.Number);
                cmd.AddParameter("@name", area.Name);
                cmd.AddParameter("@category", area.Category);
                cmd.AddParameter("@area", area.Geometry);

                cmd.ExecuteNonQuery();
            }

            var queue = new TaskQueue();
            foreach (var layer in Config.Settings.Map.Layers)
                queue.Enqueue(new TileAreaTask(area.Type, area.Number, layer.Name));
        }

        public static void BulkStoreAreas(IEnumerable<Area> areas)
        {
            var areaTableParameter = ToTableParameter(areas);
            using (var cmd = StoredProc("storeOmrådes"))
            {
                cmd.AddParameter(areaTableParameter);
                cmd.CommandTimeout = 600;
                cmd.ExecuteNonQuery();
            }

            var queue = new TaskQueue();
            foreach (var area in areas)
                foreach (var layer in Config.Settings.Map.Layers)
                    queue.Enqueue(new TileAreaTask(area.Type, area.Number, layer.Name));
        }

        public static int BulkStoreAreaLayer(AreaLayer areaLayer)
        {
            var areas = GetAreas(areaLayer.Type);
            if (areas.Count <= 0)
                throw new Exception("Fant ingen områder av type '" + areaLayer.Type +
                                    ". Har du husket å lese inn områdene?");

            if (areaLayer.Owner != null)
                areaLayer.Owner.Id = StoreContact(areaLayer.Owner);

            var areaLayerTypeId = StoreAreaLayerType(areaLayer);
            foreach (var document in areaLayer.Documents)
                document.Id = StoreDocument(0, 0, areaLayerTypeId, 0, document);

            BulkStoreAreaLayerItems(areaLayerTypeId, areaLayer, areas);
            return areas.Count;
        }

        private static int StoreAreaLayerType(AreaLayer areaLayer)
        {
            const string sql = @"INSERT INTO AreaLayerType 
(doc_guid, navn, koderegister, kodeversjon, kode, minimumsverdi, maksimumsverdi, beskrivelse, etablert, eier_id) 
OUTPUT (Inserted.id) 
VALUES (@doc_guid,@name, @codeRegister, @codeVersion, @code, @minValue,@maxValue, @description, @established,@owner_id);";

            using (var cmd = SqlStatement(sql))
            {
                cmd.AddParameter("@doc_guid", areaLayer.DocGuid);
                cmd.AddParameter("@name", areaLayer.Name);
                cmd.AddParameter("@codeRegister", areaLayer.Code.Registry);
                cmd.AddParameter("@codeVersion", areaLayer.Code.Version);
                cmd.AddParameter("@code", areaLayer.Code.Value);
                cmd.AddParameter("@minValue", areaLayer.MinValue);
                cmd.AddParameter("@maxValue", areaLayer.MaxValue);
                cmd.AddParameter("@description", areaLayer.Description);
                cmd.AddParameter("@established", areaLayer.Established);
                cmd.AddParameter("@owner_id", areaLayer.Owner.Id);

                int areaLayerTypeId = (int)cmd.ExecuteScalar();
                return areaLayerTypeId;
            }
        }

        public static void BulkStoreGrid(Grid grid)
        {
            var gridCellTable = new DataTable();
            gridCellTable.Columns.Add("rutenettype_id", typeof(int));
            gridCellTable.Columns.Add("geometrieId", typeof(string));
            gridCellTable.Columns.Add("geometri", typeof(SqlBytes));
            gridCellTable.Columns.Add("geometriEpsg", typeof(int));

            using (var cmd = StoredProc("storeRutenett"))
            {

                foreach (var cell in grid.Cells)
                {
                    var row = new object[4];
                    row[0] = (int)grid.Type;
                    row[1] = cell.CellId;
                    row[2] = cell.Geometry.STAsBinary();
                    row[3] = cell.Geometry.STSrid.Value;
                    gridCellTable.Rows.Add(row);
                }

                var gridTableParameter = new SqlParameter("@rutenettCells", SqlDbType.Structured)
                {
                    TypeName = "Rutenett",
                    Value = gridCellTable
                };

                cmd.AddParameter(gridTableParameter);
                cmd.CommandTimeout = 180; // TODO: Dette går jo ikke an.... 
                cmd.ExecuteNonQuery();
            }
        }

        public static int BulkStoreGridLayer(GridLayer gridLayer)
        {
            Grid grid = null;
            if (gridLayer.Type != RutenettType.Undefined)
                grid = GetGrid(gridLayer.Type, new Collection<int>(), new Collection<int>(), "", "", 0, 0);
            if (gridLayer.Owner != null)
                gridLayer.Owner.Id = StoreContact(gridLayer.Owner);

            const string sql = "INSERT INTO OmrådekartType(" +
                               "doc_guid, " +
                               "navn," +
                               "koderegister, " +
                               "kodeversjon, " +
                               "kode, " +
                               "minimumsverdi," +
                               "maksimumsverdi," +
                               "beskrivelse, " +
                               "etablert, " +
                               "eier_id" +
                               ") OUTPUT (" +
                               "Inserted.id" +
                               ") VALUES (" +
                               "@doc_guid, " +
                               "@name, " +
                               "@codeRegister, " +
                               "@codeVersion, " +
                               "@code, " +
                               "@minValue," +
                               "@maxValue," +
                               "@description, " +
                               "@established, " +
                               "@owner_id" +
                               ")";

            int gridLayerTypeId;
            using (var cmd = SqlStatement(sql))
            {

                cmd.AddParameter("@doc_guid", gridLayer.DocGuid);
                cmd.AddParameter("@name", gridLayer.Name);
                cmd.AddParameter("@codeRegister", gridLayer.Code.Registry);
                cmd.AddParameter("@codeVersion", gridLayer.Code.Version);
                cmd.AddParameter("@code", gridLayer.Code.Value);
                cmd.AddParameter("@minValue", gridLayer.MinValue);
                cmd.AddParameter("@maxValue", gridLayer.MaxValue);
                cmd.AddParameter("@description", gridLayer.Description);
                cmd.AddParameter("@established", gridLayer.Established);
                cmd.AddParameter("@owner_id", gridLayer.Owner.Id);

                gridLayerTypeId = (int)cmd.ExecuteScalar();
            }

            foreach (var document in gridLayer.Documents)
                document.Id = StoreDocument(0, 0, 0, gridLayerTypeId, document);

            if (gridLayer.Type != RutenettType.Undefined)
                BulkStoreGridLayerCells(gridLayerTypeId, gridLayer, grid);
            else
                BulkStoreCustomGridLayerCells(gridLayerTypeId, gridLayer);
            return gridLayer.Cells.Count;
        }

        public static IEnumerable<Tuple<string, int, bool>> GetNatureAreaSummary(string geometry)
        {
            var natureAreaSummary = new Collection<Tuple<string, int, bool>>();

            {
                var sql =
                    "SELECT dv.kode, na.id, 0 FROM Naturområde na, Beskrivelsesvariabel dv WHERE na.id = dv.naturområde_id";

                if (!string.IsNullOrEmpty(geometry))
                {
                    sql +=
                        " AND na.geometri.STIntersects(@area) = 1";
                }

                sql +=
                    " UNION SELECT nat.kode, na.id, 1 FROM Naturområde na, NaturområdeType nat WHERE na.id = nat.naturområde_id";

                if (!string.IsNullOrEmpty(geometry))
                {
                    sql +=
                        " AND na.geometri.STIntersects(@area) = 1";
                }

                //sql +=
                //    " UNION SELECT dv.kode, na.id, 0 FROM Naturområde na, Beskrivelsesvariabel dv, NaturområdeType nat WHERE na.id = nat.naturområde_id AND nat.id = dv.naturområdetype_id";

                //if (!string.IsNullOrEmpty(geometry))
                //{
                //    sql +=
                //        " AND " +
                //        "na.geometri.STIntersects(@area) = 1";
                //}

                using (var cmd = SqlStatement(sql))
                {
                    if (!string.IsNullOrEmpty(geometry))
                    {
                        var area = SqlGeometry.STGeomFromText(new SqlChars(geometry), Config.Settings.Map.SpatialReferenceSystemIdentifier).MakeValid();
                        cmd.AddParameter("@area", area);
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var code = reader.GetString(0);
                            var id = reader.GetInt32(1);
                            var natureAreaType = reader.GetInt32(2);
                            natureAreaSummary.Add(new Tuple<string, int, bool>(code, id, natureAreaType == 1));
                        }
                    }
                }
            }
            return natureAreaSummary;
        }

        public static IEnumerable<Tuple<string, int>> GetNatureAreaInstitutionSummary(string geometry, int epsgCode)
        {
            var natureAreaInstitutionSummary = new Collection<Tuple<string, int>>();
            var sql =
                "SELECT " +
                "na.institusjon, " +
                "count(1) as institutionCount " +
                "FROM " +
                "Naturområde na";

            if (!string.IsNullOrEmpty(geometry))
            {
                sql +=
                    " WHERE " +
                    "na.geometri.STIntersects(@area) = 1";
            }

            sql +=
                " GROUP BY " +
                "institusjon";

            using (var cmd = SqlStatement(sql))
            {
                if (!string.IsNullOrEmpty(geometry))
                {
                    var area = SqlGeometry.STGeomFromText(new SqlChars(geometry), epsgCode).MakeValid();
                    cmd.AddParameter("@area", area);
                }

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var institution = reader.GetString(0);
                        var count = reader.GetInt32(1);
                        natureAreaInstitutionSummary.Add(new Tuple<string, int>(institution, count));
                    }
                }
            }

            return natureAreaInstitutionSummary;
        }

        // TODO: Primitive obsession?
        public static Collection<Tuple<int, string, string, AreaType, int>> GetAreaSummary(string geometry, int epsgCode)
        {
            var areaSummary = new Collection<Tuple<int, string, string, AreaType, int>>();
            var sql =
                "SELECT " +
                "a.nummer, " +
                "a.navn, " +
                "a.kategori, " +
                "a.geometriType_id, " +
                "count(al.naturområde_id) " +
                "FROM " +
                "OmrådeLink al, " +
                "Område a";

            if (!string.IsNullOrEmpty(geometry))
            {
                sql +=
                    ", Naturområde na";
            }

            sql +=
                " WHERE " +
                "al.geometri_id = a.id ";

            if (!string.IsNullOrEmpty(geometry))
            {
                sql +=
                    "AND " +
                    "al.naturområde_id = na.id " +
                    "AND " +
                    "na.geometri.STIntersects(@area) = 1 ";
            }

            sql +=
                "GROUP BY " +
                "a.nummer, " +
                "a.navn, " +
                "a.kategori, " +
                "a.geometriType_id";

            using (var cmd = SqlStatement(sql))
            {

                if (!string.IsNullOrEmpty(geometry))
                {
                    var area = SqlGeometry.STGeomFromText(new SqlChars(geometry), epsgCode).MakeValid();
                    cmd.AddParameter("@area", area);
                }

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var number = reader.GetInt32(0);
                        var name = reader.GetString(1);
                        string category = null;
                        if (!reader.IsDBNull(2))
                            category = reader.GetString(2);
                        var areaTypeId = reader.GetInt32(3);
                        var count = reader.GetInt32(4);
                        areaSummary.Add(new Tuple<int, string, string, AreaType, int>(number, name, category,
                            (AreaType)areaTypeId, count));
                    }
                }
            }
            return areaSummary;
        }

        public static int GetAreaSummaryCount(AreaType areaType, string geometry, int epsgCode)
        {
            var sql =
                "SELECT " +
                "count(1) " +
                "FROM " +
                "Naturområde na " +
                "WHERE EXISTS(" +
                "SELECT " +
                "1 " +
                "FROM " +
                "OmrådeLink al, " +
                "Område a " +
                "WHERE " +
                "al.naturområde_id = na.id " +
                "AND " +
                "al.geometri_id = a.id " +
                "AND " +
                "a.geometriType_id = @areaType_id" +
                ")";

            if (!string.IsNullOrEmpty(geometry))
            {
                sql +=
                    " AND " +
                    "na.geometri.STIntersects(@area) = 1 ";
            }

            using (var cmd = SqlStatement(sql))
            {

                cmd.AddParameter("@areaType_id", (int)areaType);

                if (!string.IsNullOrEmpty(geometry))
                {
                    var area = SqlGeometry.STGeomFromText(new SqlChars(geometry), epsgCode).MakeValid();
                    cmd.AddParameter("@area", area);
                }

                var areaSummaryCount = (int)cmd.ExecuteScalar();
                return areaSummaryCount;
            }
        }

        public static IEnumerable<Tuple<AreaType, Collection<AreaLayer>>> GetAreaLayerSummary()
        {
            var areaLayerSummary = new Collection<Tuple<AreaType, Collection<AreaLayer>>>();

            const string sql = "SELECT " +
                               "at.id " +
                               "FROM " +
                               "OmrådeType at " +
                               "WHERE EXISTS(" +
                               "SELECT " +
                               "1 " +
                               "FROM " +
                               "Område a, " +
                               "Områdekart al " +
                               "WHERE " +
                               "at.id = a.geometriType_id " +
                               "AND " +
                               "a.id = al.geometri_id " +
                               ")";

            using (var cmd = SqlStatement(sql))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    areaLayerSummary.Add(new Tuple<AreaType, Collection<AreaLayer>>((AreaType)id,
                        new Collection<AreaLayer>()));
                }
            }

            foreach (var areaLayer in areaLayerSummary)
                areaLayer.Item2.AddRange(GetAreaLayerSummary(areaLayer.Item1));

            return areaLayerSummary;
        }

        public static IEnumerable<Tuple<RutenettType, Collection<GridLayer>>> GetGridSummary()
        {
            var gridSummary = new Collection<Tuple<RutenettType, Collection<GridLayer>>>();
            const string sql = "SELECT " +
                               "gt.id " +
                               "FROM " +
                               "Rutenettype gt " +
                               "WHERE EXISTS(" +
                               "SELECT " +
                               "1 " +
                               "FROM " +
                               "Rutenett g " +
                               "WHERE " +
                               "gt.id = g.rutenettype_id" +
                               ")";

            using (var cmd = SqlStatement(sql))
            {

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32(0);
                        gridSummary.Add(new Tuple<RutenettType, Collection<GridLayer>>((RutenettType)id,
                            new Collection<GridLayer>()));
                    }
                }
            }
            foreach (var grid in gridSummary)
                grid.Item2.AddRange(GetGridLayerSummary(grid.Item1));

            return gridSummary;
        }

        public static Collection<NatureArea> GetNatureAreasBySearchFilter(SearchFilterRequest searchFilterRequest)
        {
            int natureAreaCount;
            var natureAreas = GetNatureAreasBySearchFilter(
                searchFilterRequest.AnalyzeSearchFilterRequest(),
                searchFilterRequest.NatureAreaTypeCodes,
                searchFilterRequest.DescriptionVariableCodes,
                searchFilterRequest.Municipalities,
                searchFilterRequest.Counties,
                searchFilterRequest.ConservationAreas,
                searchFilterRequest.Institutions,
                searchFilterRequest.Geometry,
                searchFilterRequest.BoundingBox,
                searchFilterRequest.EpsgCode,
                searchFilterRequest.CenterPoints,
                0,
                0,
                0,
                out natureAreaCount
                );
            return natureAreas;
        }

        public static Collection<NatureArea> GetNatureAreasBySearchFilter(
            Collection<NatureLevel> natureLevels,
            Collection<string> natureAreaTypeCodes,
            Collection<string> descriptionVariableCodes,
            Collection<int> municipalities,
            Collection<int> counties,
            Collection<int> conservationAreas,
            Collection<string> institutions,
            string geometry,
            string boundingBox,
            int espgCode,
            bool centerPoints,
            int infoLevel,
            int indexFrom,
            int indexTo,
            out int natureAreaCount)
        {
            var natureAreas = new Collection<NatureArea>();

            Collection<Tuple<string, SqlDbType, object>> parameters;

            string fromClause;
            string whereClause;

            var queryOk = GenerateSearchFilterQuery(
                natureLevels,
                natureAreaTypeCodes,
                descriptionVariableCodes,
                municipalities,
                counties,
                conservationAreas,
                institutions,
                geometry,
                boundingBox,
                espgCode,
                out fromClause,
                out whereClause,
                out parameters);

            if (!queryOk)
            {
                natureAreaCount = 0;
                return natureAreas;
            }

            if (infoLevel == 0)
            {
                natureAreas = GetNatureAreaGeometries(fromClause, whereClause, parameters, centerPoints);
                natureAreaCount = natureAreas.Count;
            }
            else
                natureAreas = GetNatureAreaInfos(fromClause, whereClause, parameters, indexFrom, indexTo, infoLevel,
                    out natureAreaCount);

            return natureAreas;
        }

        public static Collection<Metadata> GetMetadatasByNatureAreaLocalIds(Collection<string> localIds, bool addNatureAreas)
        {
            var metadatas = new Collection<Metadata>();
            if (localIds.Count == 0)
                return metadatas;
            const string sql = "SELECT md.id, md.localId, md.navnerom, md.versjonId, md.program, md.prosjekt, md.prosjektbeskrivelse, md.formål, md.oppdragsgiver_id, md.eier_id, md.kartlagtFraDato, md.kartlagtTilDato, md.kartleggingsmålestokk, md.oppløsning, md.geometri, md.målemetode, md.nøyaktighet, md.visibility, md.målemetodeHøyde, md.nøyaktighetHøyde, md.maksimaltAvvik FROM KartlagtOmråde md WHERE md.id IN (SELECT DISTINCT na.kartlagtOmråde_id FROM Naturområde na WHERE na.localId IN ({0}) )";

            var localIdParameters = new Collection<string>();

            for (var i = 0; i < localIds.Count; ++i)
                localIdParameters.Add("@localId" + i);

            using (var cmd = SqlStatement(string.Format(sql, string.Join(", ", localIdParameters))))
            {
                for (var i = 0; i < localIds.Count; ++i)
                    cmd.AddParameter("@localId" + i, localIds[i]);

                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                    {
                        var metadata = new Metadata
                        {
                            Id = reader.GetInt32(0),
                            UniqueId = new Identification
                            {
                                LocalId = reader.GetGuid(1),
                                NameSpace = reader.GetString(2)
                            }
                        };
                        if (!reader.IsDBNull(3)) metadata.UniqueId.VersionId = reader.GetString(3);
                        metadata.Program = reader.GetString(4);
                        metadata.ProjectName = reader.GetString(5);
                        if (!reader.IsDBNull(6)) metadata.ProjectDescription = reader.GetString(6);
                        if (!reader.IsDBNull(7)) metadata.Purpose = reader.GetString(7);
                        metadata.Contractor = new Contact { Id = reader.GetInt32(8) };
                        metadata.Owner = new Contact { Id = reader.GetInt32(9) };
                        metadata.SurveyedFrom = reader.GetDateTime(10);
                        metadata.SurveyedTo = reader.GetDateTime(11);
                        metadata.SurveyScale = reader.GetString(12);
                        if (!reader.IsDBNull(13)) metadata.Resolution = reader.GetString(13);
                        metadata.Area = SqlGeometry.Deserialize(reader.GetSqlBytes(14));
                        metadata.Quality = new Quality { MeasuringMethod = reader.GetString(15) };
                        if (!reader.IsDBNull(16)) metadata.Quality.Accuracy = reader.GetInt32(16);
                        if (!reader.IsDBNull(17)) metadata.Quality.Visibility = reader.GetString(17);
                        if (!reader.IsDBNull(18))
                            metadata.Quality.MeasuringMethodHeight = reader.GetString(18);
                        if (!reader.IsDBNull(19)) metadata.Quality.AccuracyHeight = reader.GetInt32(19);
                        if (!reader.IsDBNull(20)) metadata.Quality.MaxDeviation = reader.GetInt32(20);
                        metadatas.Add(metadata);
                    }
            }

            foreach (var metadata in metadatas)
            {
                metadata.Contractor = GetContact(metadata.Contractor.Id);
                metadata.Owner = GetContact(metadata.Owner.Id);
                metadata.Documents = GetDocuments(metadata.Id);
                if (addNatureAreas)
                    metadata.NatureAreas = GetNatureAreasByMetadataId(metadata.Id, localIds);
                metadata.VariabelDefinitions = GetVariableDefinitions(metadata.Id);
            }

            return metadatas;
        }

        public static Collection<Metadata> GetMetadatasBySearchFilter(
            Collection<NatureLevel> natureLevels,
            Collection<string> natureAreaTypeCodes,
            Collection<string> descriptionVariableCodes,
            Collection<int> municipalities,
            Collection<int> counties,
            Collection<int> conservationAreas,
            Collection<string> institutions,
            string geometry,
            string boundingBox,
            int espgCode)
        {
            var metadatas = new Collection<Metadata>();

            Collection<Tuple<string, SqlDbType, object>> parameters;

            string fromClause;
            string whereClause;

            var queryOk = GenerateSearchFilterQuery(
                natureLevels,
                natureAreaTypeCodes,
                descriptionVariableCodes,
                municipalities,
                counties,
                conservationAreas,
                institutions,
                geometry,
                boundingBox,
                espgCode,
                out fromClause,
                out whereClause,
                out parameters);

            if (!queryOk)
                return metadatas;

            var sql = "SELECT DISTINCT na.localId FROM " + fromClause;

            Log.w("GMBSF", sql);
            if (!string.IsNullOrEmpty(whereClause))
                sql += " WHERE " + whereClause;

            using (var cmd = SqlStatement(sql))
            {
                foreach (var parameter in parameters)
                    cmd.AddParameter(parameter.Item1, parameter.Item2, parameter.Item3);

                var localIds = new Collection<string>();
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                    {
                        localIds.Add(reader.GetGuid(0).ToString());
                        Log.w("GMBSF", reader.GetGuid(0).ToString());
                    }
                return GetMetadatasByNatureAreaLocalIds(localIds, true);
            }
        }

        public static NatureArea GetNatureAreaByLocalId(Guid localId)
        {
            const string whereClause = "localId = @localId";

            var parameters = new Collection<Tuple<string, SqlDbType, object>>
            {
                new Tuple<string, SqlDbType, object>("@localId", SqlDbType.UniqueIdentifier, localId)
            };

            var natureAreas = GetNatureAreas(whereClause, parameters);
            if (natureAreas.Count <= 0) throw new Exception("Finner ikke naturområde med id '" + localId + "'.");
            return natureAreas[0];
        }

        public static Collection<Area> GetAreas(AreaType areaType, int areaLayerTypeId = 0, int number = 0)
        {
            var areas = new Collection<Area>();
            var sql = "SELECT a.id, a.geometriType_id, a.nummer, a.navn, a.kategori, a.geometri";

            if (areaLayerTypeId != 0)
                sql += ", al.trinn";

            sql += " FROM Område a";

            if (areaLayerTypeId != 0)
                sql += ", Områdekart al";

            sql += " WHERE a.geometriType_id = @areaType_id";

            if (number != 0)
                sql += " AND nummer = @number";

            if (areaLayerTypeId != 0)
                sql += " AND a.id = al.geometri_id AND al.områdeKartType_id = @areaLayerType_id";

            using (var cmd = SqlStatement(sql))
            {
                cmd.AddParameter("@areaType_id", (int)areaType);
                if (number != 0)
                    cmd.AddParameter("@number", number);
                if (areaLayerTypeId != 0)
                    cmd.AddParameter("@areaLayerType_id", areaLayerTypeId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var area = new Area
                        {
                            Id = reader.GetInt32(0),
                            Type = (AreaType)reader.GetInt32(1),
                            Number = reader.GetInt32(2),
                            Name = reader.GetString(3)
                        };
                        if (!reader.IsDBNull(4))
                            area.Category = reader.GetString(4);
                        area.Geometry = SqlGeometry.Deserialize(reader.GetSqlBytes(5));
                        if (areaLayerTypeId != 0)
                            area.Value = reader.GetString(6);
                        areas.Add(area);
                    }
                }
            }

            return areas;
        }

        public static Collection<Area> SearchAreas(AreaType areaType, string searchName)
        {
            var areas = new Collection<Area>();
            var sql = "SELECT geometriType_id, nummer, navn FROM Område WHERE ";

            if (areaType != AreaType.Undefined)
            {
                sql += "geometriType_id = @areaType_id AND ";
            }

            sql += "navn LIKE @searchName ORDER BY navn";

            using (var cmd = SqlStatement(sql))
            {
                if (areaType != AreaType.Undefined)
                    cmd.AddParameter("@areaType_id", (int)areaType);

                cmd.AddParameter("@searchName", "%" + searchName + "%");

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var area = new Area
                        {
                            Type = (AreaType)reader.GetInt32(0),
                            Number = reader.GetInt32(1),
                            Name = reader.GetString(2)
                        };
                        areas.Add(area);
                    }
                }
            }
            return areas;
        }

        public static Collection<Area> GetAreaLinkInfos(string wkt, int epsgCode)
        {
            var areaLinkInfos = new Collection<Area>();
            const string sql = "SELECT id, geometriType_id, nummer, navn FROM Område WHERE geometri.STIntersects(@area) = 1";

            using (var cmd = SqlStatement(sql))
            {
                var area = SqlGeometry.STGeomFromText(new SqlChars(wkt), epsgCode).MakeValid();
                cmd.AddParameter("@area", area);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var areaLinkInfo = new Area
                        {
                            Id = reader.GetInt32(0),
                            Type = (AreaType)reader.GetInt32(1),
                            Number = reader.GetInt32(2),
                            Name = reader.GetString(3)
                        };
                        areaLinkInfos.Add(areaLinkInfo);
                    }
                }
            }
            return areaLinkInfos;
        }

        public static Grid GetGrid(
            RutenettType rutenettType,
            Collection<int> municipalities,
            Collection<int> counties,
            string geometry,
            string boundingBox,
            int espgCode,
            int gridLayerTypeId)
        {
            var grid = new Grid(rutenettType);
            var preSql = "";
            var sql = "SELECT g.id, g.geometrieId, g.geometri";

            if (gridLayerTypeId != 0)
                sql += ", gl.trinn";

            sql += " FROM Rutenett g";

            if (gridLayerTypeId != 0)
                sql += ", Rutenettkart gl";

            sql += " WHERE g.rutenettype_id = @gridType_id";

            if (gridLayerTypeId != 0)
                sql += " AND gl.rutenett_id = g.id AND gl.rutenettkartType_id = @gridLayerType_id";

            var parameters = new Collection<Tuple<string, SqlDbType, object>>
            {
                new Tuple<string, SqlDbType, object>("@gridType_id", SqlDbType.Int, (int) rutenettType)
            };


            if (gridLayerTypeId != 0)
                parameters.Add(new Tuple<string, SqlDbType, object>("@gridLayerType_id", SqlDbType.Int,
                    gridLayerTypeId));

            if (municipalities != null && municipalities.Count > 0)
            {
                sql += " AND ";
                if (municipalities.Count == 1)
                {
                    preSql += "DECLARE @_kommune geometry;\n";
                    preSql +=
                        "SELECT @_kommune = geometri FROM Område WHERE geometriType_id = 1 AND nummer = @kommunenummer;\n";
                    sql += "g.geometri.STIntersects(@_kommune) = 1";
                    parameters.Add(new Tuple<string, SqlDbType, object>("@kommunenummer", SqlDbType.Int,
                        municipalities[0]));
                }
                else
                {
                    sql += '(';
                    for (var i = 0; i < municipalities.Count; ++i)
                    {
                        //sql += "cell.STIntersects((SELECT area FROM Area WHERE areaType_id = 1 AND number = @municipality" + i + ")) = 1";
                        preSql += "DECLARE @_kommune" + i + " geometry;\n";
                        preSql += "SELECT @_kommune" + i +
                                  " = geometri FROM Område WHERE geometriType_id = 1 AND nummer = @kommunenummer" + i + ";\n";
                        sql += "cell.STIntersects(@_kommune" + i + ") = 1";
                        if (i != municipalities.Count - 1) sql += " OR ";
                        parameters.Add(new Tuple<string, SqlDbType, object>("@kommunenummer" + i, SqlDbType.Int,
                            municipalities[i]));
                    }
                    sql += ')';
                }
            }

            if (counties != null && counties.Count > 0)
            {
                sql += " AND ";
                if (counties.Count == 1)
                {
                    //sql += "cell.STIntersects((SELECT area FROM Area WHERE areaType_id = 2 AND number = @fylke)) = 1";
                    preSql += "DECLARE @_fylke geometry;\n";
                    preSql += "SELECT @_fylke = geometri FROM Område WHERE geometriType_id = 2 AND nummer = @fylke;\n";
                    sql += "g.geometri.STIntersects(@_fylke) = 1";
                    parameters.Add(new Tuple<string, SqlDbType, object>("@fylke", SqlDbType.Int, counties[0]));
                }
                else
                {
                    sql += '(';
                    for (var i = 0; i < counties.Count; ++i)
                    {
                        //sql += "cell.STIntersects((SELECT area FROM Area WHERE areaType_id = 2 AND number = @fylke" + i + ")) = 1";
                        preSql += "DECLARE @_fylke" + i + " geometry;\n";
                        preSql += "SELECT @_fylke" + i +
                                  " = geometri FROM Område WHERE geometriType_id = 2 AND nummer = @fylke" + i + ";\n";
                        sql += "g.geometri.STIntersects(@_fylke" + i + ") = 1";
                        if (i != counties.Count - 1) sql += " OR ";
                        parameters.Add(new Tuple<string, SqlDbType, object>("@fylke" + i, SqlDbType.Int,
                            counties[i]));
                    }
                    sql += ')';
                }
            }

            if (!string.IsNullOrEmpty(geometry) && !string.IsNullOrEmpty(boundingBox))
            {
                sql += " AND ";
                var area = SqlGeometry.STGeomFromText(new SqlChars(geometry), espgCode).MakeValid();
                var bbox = SqlGeometry.STGeomFromText(new SqlChars(boundingBox), espgCode).MakeValid();
                var areaIntersection = area.STIntersection(bbox);

                if (areaIntersection.STIsEmpty())
                    return new Grid(rutenettType);
                sql += "g.geometri.STIntersects(@area) = 1";
                parameters.Add(new Tuple<string, SqlDbType, object>("@area", SqlDbType.VarBinary,
                    areaIntersection.Serialize()));
            }
            else if (!string.IsNullOrEmpty(boundingBox))
            {
                sql += " AND ";
                var bbox = SqlGeometry.STGeomFromText(new SqlChars(boundingBox), espgCode).MakeValid();
                sql += "g.geometri.STIntersects(@area) = 1";
                parameters.Add(new Tuple<string, SqlDbType, object>("@area", SqlDbType.VarBinary, bbox.Serialize()));
            }
            else if (!string.IsNullOrEmpty(geometry))
            {
                sql += " AND ";
                var area = SqlGeometry.STGeomFromText(new SqlChars(geometry), espgCode).MakeValid();
                sql += "g.geometri.STIntersects(@area) = 1";
                parameters.Add(new Tuple<string, SqlDbType, object>("@area", SqlDbType.VarBinary, area.Serialize()));
            }

            sql = preSql + sql;

            using (var cmd = SqlStatement(sql))
            {

                foreach (var parameter in parameters)
                    cmd.AddParameter(parameter.Item1, parameter.Item2, parameter.Item3);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var gridCell = new GridCell
                        {
                            Id = reader.GetInt32(0),
                            CellId = reader.GetString(1),
                            Geometry = SqlGeometry.Deserialize(reader.GetSqlBytes(2))
                        };
                        if (gridLayerTypeId != 0) gridCell.Value = reader.GetString(3);
                        grid.Cells.Add(gridCell);
                    }
                }
            }

            return grid;
        }

        public static void UpdateAreas(Collection<Area> areas)
        {
            UpdateAreas2(areas);
        }

        private static void UpdateAreas2(Collection<Area> areas)
        {
            if (areas.Count == 0)
                throw new Exception("Ingen områder.");
            var areaType = areas[0].Type;
            if (areas.Any(area => area.Type != areaType))
                throw new Exception("Kan ikke ha forskjellige områdetyper i samme sett.");
            var areaLayerItems = new Collection<Tuple<int, int, string>>();

            var sql =
                "SELECT " +
                "al.områdeKartType_id, " +
                "a.nummer, " +
                "al.trinn " +
                "FROM " +
                "Områdekart al, " +
                "Område a " +
                "WHERE " +
                "a.geometriType_id = @areaType_id " +
                "AND " +
                "a.id = al.geometri_id";

            using (var cmd = SqlStatement(sql))
            {

                cmd.AddParameter("@areaType_id", (int)areaType);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var areaLayerTypeId = reader.GetInt32(0);
                        var number = reader.GetInt32(1);
                        var value = reader.GetString(2);
                        areaLayerItems.Add(new Tuple<int, int, string>(areaLayerTypeId, number, value));
                    }
                }
            }

            // Delete all area layer link rows.
            if (areaLayerItems.Count > 0)
            {
                var distinctAreaLayerTypeIds = new HashSet<int>();

                foreach (var areaLayerItem in areaLayerItems)
                    distinctAreaLayerTypeIds.Add(areaLayerItem.Item1);

                sql = "DELETE FROM Områdekart WHERE områdeKartType_id IN (";
                for (var i = 0; i < distinctAreaLayerTypeIds.Count; ++i)
                {
                    sql += "@areaLayerType_id" + i;
                    if (i != distinctAreaLayerTypeIds.Count - 1) sql += ", ";
                }
                sql += ")";

                using (var cmd = SqlStatement(sql))
                {

                    var areaLayerTypeIdList = distinctAreaLayerTypeIds.ToList();
                    for (var i = 0; i < areaLayerTypeIdList.Count; ++i)
                        cmd.AddParameter("@areaLayerType_id" + i,
                            areaLayerTypeIdList[i]);

                    cmd.ExecuteNonQuery();
                }
            }

            DeleteAreas(areaType);
            BulkStoreAreas(areas);
            BulkRestoreAreaLayerItems(areaType, areaLayerItems);

            // Relink all nature area links.
            using (var cmd = StoredProc("relinkOmrådes"))
            {
                cmd.AddParameter("@geometritypeId", (int)areaType);
                cmd.CommandTimeout = 600;
                cmd.ExecuteNonQuery();
            }
        }

        public static void DeleteDataDelivery(Guid metadataLocalId)
        {
            DeleteNatureAreas(metadataLocalId);

            const string sql = "DELETE FROM " +
                               "Dataleveranse " +
                               "WHERE " +
                               "id " +
                               "IN (" +
                               "SELECT " +
                               "dataleveranse_id " +
                               "FROM " +
                               "KartlagtOmråde " +
                               "WHERE " +
                               "localId = @metadataLocalId" +
                               ")";

            using (var cmd = SqlStatement(sql))
            {
                cmd.AddParameter("@metadataLocalId", metadataLocalId);
                cmd.ExecuteNonQuery();
            }
        }

        private static int StoreDocument(int metadataId, int natureAreaId, int areaLayerTypeId, int gridLayerTypeId,
            Document document)
        {
            if (document.Author != null)
                document.Author.Id = StoreContact(document.Author);

            const string sql = "INSERT INTO Dokument(" +
                               "doc_guid," +
                               "kartlagtOmråde_id," +
                               "naturområde_id," +
                               "områdeKartType_id, " +
                               "rutenettkartType_id, " +
                               "tittel," +
                               "beskrivelse," +
                               "kartlegger_id," +
                               "filsti" +
                               ") OUTPUT (" +
                               "Inserted.id" +
                               ") VALUES (" +
                               "@doc_guid," +
                               "@metadata_id," +
                               "@natureArea_id," +
                               "@areaLayerType_id, " +
                               "@gridLayerType_id, " +
                               "@title," +
                               "@description," +
                               "@author_id," +
                               "@filepath" +
                               ")";

            using (var cmd = SqlStatement(sql))
            {
                cmd.AddParameter("@doc_guid", document.Guid);
                cmd.AddParameterNull("@metadata_id", metadataId);
                cmd.AddParameterNull("@natureArea_id", natureAreaId);
                cmd.AddParameterNull("@areaLayerType_id", areaLayerTypeId);
                cmd.AddParameterNull("@gridLayerType_id", gridLayerTypeId);
                cmd.AddParameter("@title", document.Title);
                cmd.AddParameter("@description", document.Description);
                cmd.AddParameter("@author_id", document.Author?.Id);
                cmd.AddParameter("@filepath", document.FileName);

                var id = (int)cmd.ExecuteScalar();
                return id;
            }
        }

        private static int StoreContact(Contact contact)
        {
            var cmd = StoredProc("createKontakt");
            cmd.AddParameter("@firmanavn", contact.Company);
            cmd.AddParameter("@kontaktperson", contact.ContactPerson);
            cmd.AddParameter("@epost", contact.Email);
            cmd.AddParameter("@telefon", contact.Phone);
            cmd.AddParameter("@hjemmeside", contact.Homesite);

            var idParameter = cmd.AddReturnParameter("@id", SqlDbType.Int);
            cmd.ExecuteNonQuery();

            var id = (int)idParameter.Value;
            return id;
        }

        private static int LagreDataleveranse(int dataDeliveryId, Metadata metadata)
        {
            if (metadata.Contractor != null)
                metadata.Contractor.Id = StoreContact(metadata.Contractor);
            if (metadata.Owner != null)
                metadata.Owner.Id = StoreContact(metadata.Owner);

            const string sql = "INSERT INTO KartlagtOmråde(" +
                               "dataleveranse_id," +
                               "localId," +
                               "navnerom," +
                               "versjonId," +
                               "program," +
                               "prosjekt," +
                               "prosjektbeskrivelse, " +
                               "formål," +
                               "oppdragsgiver_id," +
                               "eier_id," +
                               "kartlagtFraDato," +
                               "kartlagtTilDato," +
                               "kartleggingsmålestokk," +
                               "oppløsning," +
                               "geometri," +
                               "målemetode," +
                               "nøyaktighet," +
                               "visibility," +
                               "målemetodeHøyde," +
                               "nøyaktighetHøyde," +
                               "maksimaltAvvik" +
                               ") OUTPUT (" +
                               "Inserted.id" +
                               ") VALUES (" +
                               "@dataDelivery_id," +
                               "@localId," +
                               "@nameSpace, " +
                               "@versionId, " +
                               "@program," +
                               "@project," +
                               "@projectDescription, " +
                               "@purpose," +
                               "@contractor_id," +
                               "@owner_id," +
                               "@surveyedFrom," +
                               "@surveyedTo," +
                               "@surveyScale," +
                               "@resolution," +
                               "@area," +
                               "@measuringMethod," +
                               "@accuracy," +
                               "@visibility," +
                               "@measuringMethodHeight," +
                               "@accuracyHeight," +
                               "@maxDeviation" +
                               ")";

            int id;
            using (var cmd = SqlStatement(sql))
            {
                cmd.AddParameter("@dataDelivery_id", dataDeliveryId);
                cmd.AddParameter("@localId", metadata.UniqueId.LocalId);
                cmd.AddParameter("@nameSpace", metadata.UniqueId.NameSpace);
                cmd.AddParameter("@versionId", metadata.UniqueId.VersionId);
                cmd.AddParameter("@program", metadata.Program);
                cmd.AddParameter("@project", metadata.ProjectName);
                cmd.AddParameter("@projectDescription",
                    metadata.ProjectDescription);
                cmd.AddParameter("@purpose", metadata.Purpose);
                cmd.AddParameter("@contractor_id", metadata.Contractor?.Id);
                cmd.AddParameter("@owner_id", metadata.Owner?.Id);
                cmd.AddParameter("@surveyedFrom", metadata.SurveyedFrom);
                cmd.AddParameter("@surveyedTo", metadata.SurveyedTo);
                cmd.AddParameter("@surveyScale", metadata.SurveyScale);
                cmd.AddParameter("@resolution", metadata.Resolution);
                cmd.AddParameter("@area", metadata.Area);
                var quality = metadata.Quality;
                if (quality != null)
                {
                    cmd.AddParameter("@measuringMethod", quality.MeasuringMethod);
                    cmd.AddParameter("@accuracy", quality.Accuracy);
                    cmd.AddParameter("@visibility", quality.Visibility);
                    cmd.AddParameter("@measuringMethodHeight", quality.MeasuringMethodHeight);
                    cmd.AddParameter("@accuracyHeight", quality.AccuracyHeight);
                    cmd.AddParameter("@maxDeviation", quality.MaxDeviation);
                }
                id = (int)cmd.ExecuteScalar();
            }
            foreach (var document in metadata.Documents)
                document.Id = StoreDocument(id, 0, 0, 0, document);

            foreach (var natureArea in metadata.NatureAreas)
                natureArea.Id = StoreNatureArea(id, natureArea);

            foreach (NinVariabelDefinisjon variableDefinition in metadata.VariabelDefinitions)
            {
                if (variableDefinition.GetType() == typeof(NinStandardVariabel))
                    ((NinStandardVariabel)variableDefinition).Id = StoreStandardVariable(id,
                        (NinStandardVariabel)variableDefinition);
                else
                    ((CustomVariableDefinition)variableDefinition).Id = StoreCustomVariableDefinition(id,
                        (CustomVariableDefinition)variableDefinition);
            }
            return id;
        }

        private static int StoreNatureArea(int metadataId, NatureArea natureArea)
        {
            if (natureArea.Surveyer != null)
                natureArea.Surveyer.Id = StoreContact(natureArea.Surveyer);

            const string sql = "INSERT INTO Naturområde(" +
                               "kartlagtOmråde_id," +
                               "localId," +
                               "navnerom, " +
                               "versjonId, " +
                               "versjon," +
                               "naturnivå_id," +
                               "geometri," +
                               "geometriSenterpunkt," +
                               "kartlegger_id," +
                               "kartlagt," +
                               "beskrivelse," +
                               "institusjon" +
                               ") OUTPUT (" +
                               "Inserted.id" +
                               ") VALUES (" +
                               "@metadata_id," +
                               "@localId," +
                               "@nameSpace, " +
                               "@versionId, " +
                               "@version," +
                               "@natureLevel_id," +
                               "@area," +
                               "@areaCenterPoint," +
                               "@surveyer_id," +
                               "@surveyed," +
                               "@description," +
                               "@institution" +
                               ")";

            using (var cmd = SqlStatement(sql))
            {
                cmd.AddParameter("@metadata_id", metadataId);
                cmd.AddParameter("@localId", natureArea.UniqueId.LocalId);
                cmd.AddParameter("@nameSpace", natureArea.UniqueId.NameSpace);
                cmd.AddParameter("@versionId", natureArea.UniqueId.VersionId);
                cmd.AddParameter("@version", natureArea.Version);
                cmd.AddParameter("@natureLevel_id", (int)natureArea.Nivå);
                cmd.AddParameter("@area", natureArea.Area);
                cmd.AddParameter("@areaCenterPoint", natureArea.Area.STCentroid());
                cmd.AddParameter("@surveyer_id", natureArea.Surveyer?.Id ?? null);
                cmd.AddParameter("@surveyed", natureArea.Surveyed);
                cmd.AddParameter("@description", natureArea.Description);
                cmd.AddParameter("@institution", natureArea.Institution);
                cmd.CommandTimeout = 600; //WTF?
                natureArea.Id = (int)cmd.ExecuteScalar();
            }

            if (Config.Settings.Database.ImmediatelyLinkAreas)
                StoreAreaLink(natureArea);

            foreach (var document in natureArea.Documents)
                document.Id = StoreDocument(0, natureArea.Id, 0, 0, document);

            foreach (var parameter in natureArea.Parameters)
            {
                if (parameter.GetType() == typeof(DescriptionVariable))
                {
                    ((DescriptionVariable)parameter).Id = StoreDescriptionVariable(natureArea.Id, 0,
                        (DescriptionVariable)parameter);
                }
                else
                {
                    ((NatureAreaType)parameter).Id = StoreNatureAreaType(natureArea.Id, (NatureAreaType)parameter);
                }
            }

            return natureArea.Id;
        }

        private static int StoreDescriptionVariable(int natureAreaId, int natureAreaTypeId,
            DescriptionVariable descriptionVariable)
        {
            if (descriptionVariable.Surveyer != null)
                descriptionVariable.Surveyer.Id = StoreContact(descriptionVariable.Surveyer);

            const string sql = "INSERT INTO Beskrivelsesvariabel(" +
                               "naturområde_id," +
                               "naturområdetype_id," +
                               "kode," +
                               "kartlegger_id, " +
                               "kartlagt, " +
                               "trinn," +
                               "beskrivelse" +
                               ") OUTPUT (" +
                               "Inserted.id" +
                               ") VALUES (" +
                               "@natureArea_id," +
                               "@natureAreaType_id," +
                               "@code," +
                               "@surveyer_id, " +
                               "@surveyed, " +
                               "@value," +
                               "@description" +
                               ")";

            using (var cmd = SqlStatement(sql))
            {
                cmd.AddParameter("@natureArea_id", natureAreaId);
                cmd.AddParameter("@natureAreaType_id", natureAreaTypeId);
                cmd.AddParameter("@code", descriptionVariable.Code);
                cmd.AddParameter("@surveyer_id", descriptionVariable.Surveyer?.Id ?? null);
                cmd.AddParameter("@surveyed", descriptionVariable.Surveyed);
                cmd.AddParameter("@value", descriptionVariable.Value);
                cmd.AddParameter("@description", descriptionVariable.Description);

                var id = (int)cmd.ExecuteScalar();
                return id;
            }
        }

        private static int StoreNatureAreaType(int natureAreaId, NatureAreaType natureAreaType)
        {
            if (natureAreaType.Surveyer != null)
                natureAreaType.Surveyer.Id = StoreContact(natureAreaType.Surveyer);

            const string sql = "INSERT INTO NaturområdeType(" +
                               "naturområde_id," +
                               "kode," +
                               "kartlegger_id, " +
                               "kartlagt, " +
                               "andel" +
                               ") OUTPUT (" +
                               "Inserted.id" +
                               ") VALUES (" +
                               "@natureArea_id," +
                               "@code," +
                               "@surveyer_id, " +
                               "@surveyed, " +
                               "@share" +
                               ")";
            int id;
            using (var cmd = SqlStatement(sql))
            {

                cmd.AddParameter("@natureArea_id", natureAreaId);
                cmd.AddParameter("@code", natureAreaType.Code);
                cmd.AddParameter("@surveyer_id", natureAreaType.Surveyer?.Id ?? null);
                cmd.AddParameter("@surveyed", natureAreaType.Surveyed);
                cmd.AddParameter("@share", natureAreaType.Share);

                id = (int)cmd.ExecuteScalar();
            }

            foreach (var additionalVariabel in natureAreaType.AdditionalVariables)
                additionalVariabel.Id = StoreDescriptionVariable(natureAreaId, id, additionalVariabel);

            foreach (var customVariable in natureAreaType.CustomVariables)
                customVariable.Id = StoreCustomVariable(id, customVariable);

            return id;
        }

        private static int StoreCustomVariable(int natureAreaTypeId, CustomVariable customVariable)
        {
            const string sql = "INSERT INTO EgendefinertVariabel(" +
                               "naturområdetype_id," +
                               "spesifikasjon," +
                               "trinn" +
                               ") OUTPUT (" +
                               "Inserted.id" +
                               ") VALUES (" +
                               "@natureAreaType_id," +
                               "@specification," +
                               "@value" +
                               ")";

            using (var cmd = SqlStatement(sql))
            {
                cmd.AddParameter("@natureAreaType_id", natureAreaTypeId);
                cmd.AddParameter("@specification", customVariable.Specification);
                cmd.AddParameter("@value", customVariable.Value);

                return (int)cmd.ExecuteScalar();
            }
        }

        private static int StoreStandardVariable(int metadataId, NinStandardVariabel standardVariable)
        {
            const string sql = "INSERT INTO Standardvariabel(" +
                               "kartlagtOmråde_id," +
                               "koderegister," +
                               "kodeversjon," +
                               "kode" +
                               ") OUTPUT (" +
                               "Inserted.id" +
                               ") VALUES (" +
                               "@metadata_id," +
                               "@codeRegister," +
                               "@codeVersion," +
                               "@code" +
                               ")";

            using (var cmd = SqlStatement(sql))
            {

                cmd.AddParameter("@metadata_id", metadataId);
                cmd.AddParameter("@codeRegister",
                    standardVariable.VariableDefinition.Registry);
                cmd.AddParameter("@codeVersion",
                    standardVariable.VariableDefinition.Version);
                cmd.AddParameter("@code", standardVariable.VariableDefinition.Value);

                return (int)cmd.ExecuteScalar();
            }
        }

        private static int StoreCustomVariableDefinition(int metadataId,
            CustomVariableDefinition customVariableDefinition)
        {
            const string sql = "INSERT INTO EgendefinertVariabelDefinisjon(" +
                               "kartlagtOmråde_id," +
                               "spesifikasjon," +
                               "beskrivelse" +
                               ") OUTPUT (" +
                               "Inserted.id" +
                               ") VALUES (" +
                               "@metadata_id," +
                               "@specification," +
                               "@description" +
                               ")";

            using (var cmd = SqlStatement(sql))
            {
                cmd.AddParameter("@metadata_id", metadataId);
                cmd.AddParameter("@specification",
                    customVariableDefinition.Specification);
                cmd.AddParameter("@description", customVariableDefinition.Description);

                return (int)cmd.ExecuteScalar();
            }
        }

        private static SqlParameter ToTableParameter(IEnumerable<Area> areas)
        {
            var areaTable = new DataTable();
            areaTable.Columns.Add("geometriType_id", typeof(int));
            areaTable.Columns.Add("nummer", typeof(int));
            areaTable.Columns.Add("navn", typeof(string));
            areaTable.Columns.Add("kategori", typeof(string));
            areaTable.Columns.Add("geometri", typeof(SqlBytes));
            areaTable.Columns.Add("geometriEpgs", typeof(int));
            foreach (var area in areas)
            {
                if (area.Type <= AreaType.Undefined)
                    throw new Exception("Area type is not defined.");

                var row = new object[6];
                row[0] = (int)area.Type;
                row[1] = area.Number;
                row[2] = area.Name;
                row[3] = area.Category;
                row[4] = area.Geometry.STAsBinary();
                row[5] = area.Geometry.STSrid.Value;
                areaTable.Rows.Add(row);
            }

            var areaTableParameter = new SqlParameter("@geometris", SqlDbType.Structured)
            {
                TypeName = "Område",
                Value = areaTable
            };
            return areaTableParameter;
        }

        private static void BulkStoreAreaLayerItems(int areaLayerTypeId, AreaLayer areaLayer, Collection<Area> areas)
        {
            var table = new DataTable();
            table.Columns.Add("områdeKartType_id", typeof(int));
            table.Columns.Add("geometri_id", typeof(int));
            table.Columns.Add("trinn", typeof(string));

            using (var cmd = StoredProc("storeOmrådekart"))
            {
                foreach (var areaLayerItem in areaLayer.Items)
                {
                    foreach (var area in areas)
                    {
                        if (areaLayerItem.Number != area.Number) continue;
                        var row = new object[3];
                        row[0] = areaLayerTypeId;
                        row[1] = area.Id;
                        row[2] = areaLayerItem.Value;
                        table.Rows.Add(row);
                        break;
                    }
                }

                var areaLayerTableParameter =
                    new SqlParameter("@områdeKartItems", SqlDbType.Structured)
                    {
                        TypeName = "Områdekart",
                        Value = table
                    };

                cmd.AddParameter(areaLayerTableParameter);
                cmd.ExecuteNonQuery();
            }
        }

        private static void BulkRestoreAreaLayerItems(AreaType areaType,
            IEnumerable<Tuple<int, int, string>> areaLayerItems)
        {
            var areaLayerItemsTable = new DataTable();
            areaLayerItemsTable.Columns.Add("områdeKartType_id", typeof(int));
            areaLayerItemsTable.Columns.Add("geometriType_id", typeof(int));
            areaLayerItemsTable.Columns.Add("nummer", typeof(int));
            areaLayerItemsTable.Columns.Add("trinn", typeof(string));

            using (var cmd = StoredProc("restoreOmrådekart"))
            {
                foreach (var areaLayerItem in areaLayerItems)
                {
                    var row = new object[4];
                    row[0] = areaLayerItem.Item1;
                    row[1] = (int)areaType;
                    row[2] = areaLayerItem.Item2;
                    row[3] = areaLayerItem.Item3;
                    areaLayerItemsTable.Rows.Add(row);
                }

                var tableParameter =
                    new SqlParameter("@områdeKartItems", SqlDbType.Structured)
                    {
                        TypeName = "OmrådekartRestore",
                        Value = areaLayerItemsTable
                    };

                cmd.AddParameter(tableParameter);

                cmd.ExecuteNonQuery();
            }
        }

        private static void BulkStoreGridLayerCells(int gridLayerTypeId, GridLayer gridLayer, Grid grid)
        {
            var table = new DataTable();
            table.Columns.Add("rutenettkartType_id", typeof(int));
            table.Columns.Add("rutenett_id", typeof(int));
            table.Columns.Add("trinn", typeof(string));

            using (var cmd = StoredProc("storeRutenettkart"))
            {
                foreach (var gridLayerCell in gridLayer.Cells)
                {
                    foreach (var gridCell in grid.Cells)
                    {
                        if (!gridLayerCell.CellId.Equals(gridCell.CellId)) continue;
                        var row = new object[3];
                        row[0] = gridLayerTypeId;
                        row[1] = gridCell.Id;
                        row[2] = gridLayerCell.Value;
                        table.Rows.Add(row);
                        break;
                    }
                }

                var gridLayerTableParameter =
                    new SqlParameter("@rutenettkartCells", SqlDbType.Structured)
                    {
                        TypeName = "Rutenettkart",
                        Value = table
                    };

                cmd.AddParameter(gridLayerTableParameter);
                cmd.ExecuteNonQuery();
            }
        }

        private static void BulkStoreCustomGridLayerCells(int gridLayerTypeId, GridLayer gridLayer)
        {
            var cellTable = new DataTable();
            cellTable.Columns.Add("geometrieId", typeof(int));
            cellTable.Columns.Add("geometri", typeof(SqlBytes));
            cellTable.Columns.Add("geometriEpsg", typeof(int));
            cellTable.Columns.Add("trinn", typeof(string));

            using (var cmd = StoredProc("storeCustomRutenettkart"))
            {
                foreach (var gridLayerCell in gridLayer.Cells)
                {
                    var gridLayerCellCustom = (GridLayerCellCustom)gridLayerCell;
                    var row = new object[4];
                    row[0] = gridLayerCellCustom.CellId;
                    row[1] = gridLayerCellCustom.CustomCell.STAsBinary();
                    row[2] = gridLayerCellCustom.CustomCell.STSrid.Value;
                    row[3] = gridLayerCellCustom.Value;
                    cellTable.Rows.Add(row);
                    break;
                }

                cmd.AddParameter("@rutenettkartType_id", gridLayerTypeId);

                var gridLayerTableParameter =
                    new SqlParameter("@customRutenettkartCells", SqlDbType.Structured)
                    {
                        TypeName = "CustomRutenettkart",
                        Value = cellTable
                    };

                cmd.AddParameter(gridLayerTableParameter);
                cmd.ExecuteNonQuery();
            }
        }

        private static void StoreAreaLink(NatureArea natureArea)
        {
            using (var cmd = StoredProc("linkOmrådes"))
            {
                cmd.AddParameter("@naturområdeId", natureArea.Id);
                cmd.ExecuteNonQuery();
            }
        }

        private static IEnumerable<AreaLayer> GetAreaLayerSummary(AreaType areaType)
        {
            var areaLayerSummary = new Collection<AreaLayer>();

            const string sql = "SELECT " +
                               "alt.id, " +
                               "alt.doc_guid, " +
                               "alt.navn, " +
                               "alt.koderegister, " +
                               "alt.kodeversjon, " +
                               "alt.kode, " +
                               "alt.minimumsverdi, " +
                               "alt.maksimumsverdi, " +
                               "alt.beskrivelse, " +
                               "alt.etablert, " +
                               "alt.eier_id " +
                               "FROM " +
                               "OmrådekartType alt " +
                               "WHERE EXISTS (" +
                               "SELECT " +
                               "1 " +
                               "FROM " +
                               "Områdekart al, " +
                               "Område a " +
                               "WHERE " +
                               "alt.id = al.områdeKartType_id " +
                               "AND " +
                               "al.geometri_id = a.id " +
                               "AND " +
                               "a.geometriType_id = @areaType_id" +
                               ")";

            using (var cmd = SqlStatement(sql))
            {
                cmd.AddParameter("@areaType_id", (int)areaType);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var areaLayer = new AreaLayer
                        {
                            Id = reader.GetInt32(0),
                            DocGuid = reader.GetGuid(1),
                            Name = reader.GetString(2),
                            Code = new Code
                            {
                                Registry = reader.GetString(3),
                                Version = reader.GetString(4),
                                Value = reader.GetString(5)
                            },
                            MinValue = reader.GetString(6),
                            MaxValue = reader.GetString(7)
                        };
                        if (!reader.IsDBNull(8)) areaLayer.Description = reader.GetString(8);
                        areaLayer.Established = reader.GetDateTime(9);
                        areaLayer.Owner = new Contact { Id = reader.GetInt32(10) };
                        areaLayerSummary.Add(areaLayer);
                    }
                }
            }
            foreach (var areaLayer in areaLayerSummary)
            {
                areaLayer.Owner = GetContact(areaLayer.Owner.Id);
                areaLayer.Documents = GetDocuments(0, 0, areaLayer.Id);
            }

            return areaLayerSummary;
        }

        private static IEnumerable<GridLayer> GetGridLayerSummary(RutenettType rutenettType)
        {
            var gridLayerSummary = new Collection<GridLayer>();

            const string sql = "SELECT " +
                               "glt.id, " +
                               "glt.doc_guid, " +
                               "glt.navn, " +
                               "glt.koderegister, " +
                               "glt.kodeversjon, " +
                               "glt.kode, " +
                               "glt.minimumsverdi, " +
                               "glt.maksimumsverdi, " +
                               "glt.beskrivelse, " +
                               "glt.etablert, " +
                               "glt.eier_id " +
                               "FROM " +
                               "RutenettkartType glt " +
                               "WHERE EXISTS (" +
                               "SELECT " +
                               "1 " +
                               "FROM " +
                               "Rutenettkart gl, " +
                               "Rutenett g " +
                               "WHERE " +
                               "glt.id = gl.rutenettkartType_id " +
                               "AND " +
                               "gl.rutenett_id = g.id " +
                               "AND " +
                               "g.rutenettype_id = @gridType_id" +
                               ")";

            using (var cmd = SqlStatement(sql))
            {
                cmd.AddParameter("@gridType_id", (int)rutenettType);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var gridLayer = new GridLayer
                        {
                            Id = reader.GetInt32(0),
                            DocGuid = reader.GetGuid(1),
                            Name = reader.GetString(2),
                            Code = new Code
                            {
                                Registry = reader.GetString(3),
                                Version = reader.GetString(4),
                                Value = reader.GetString(5)
                            },
                            MinValue = reader.GetString(6),
                            MaxValue = reader.GetString(7)
                        };
                        if (!reader.IsDBNull(8)) gridLayer.Description = reader.GetString(8);
                        gridLayer.Established = reader.GetDateTime(9);
                        gridLayer.Owner = new Contact { Id = reader.GetInt32(10) };
                        gridLayerSummary.Add(gridLayer);
                    }
                }
            }

            foreach (var gridLayer in gridLayerSummary)
            {
                gridLayer.Owner = GetContact(gridLayer.Owner.Id);
                gridLayer.Documents = GetDocuments(0, 0, 0, gridLayer.Id);
            }

            return gridLayerSummary;
        }

        private static bool GenerateSearchFilterQuery(
            IReadOnlyList<NatureLevel> natureLevels,
            IReadOnlyList<string> natureAreaTypeCodes,
            IReadOnlyList<string> descriptionVariableCodes,
            IReadOnlyList<int> municipalities,
            IReadOnlyList<int> counties,
            IReadOnlyList<int> conservationAreas,
            IReadOnlyList<string> institutions,
            string geometry,
            string boundingBox,
            int espgCode,
            out string fromClause,
            out string whereClause,
            out Collection<Tuple<string, SqlDbType, object>> parameters
            )
        {
            parameters = new Collection<Tuple<string, SqlDbType, object>>();
            fromClause = "Naturområde na";
            whereClause = "";

            if (natureLevels != null && natureLevels.Count > 0)
            {
                if (whereClause != "") whereClause += " AND ";
                if (natureLevels.Count == 1)
                {
                    whereClause += "na.naturnivå_id = @natureLevel_id";
                    parameters.Add(new Tuple<string, SqlDbType, object>("@natureLevel_id", SqlDbType.Int,
                         (int)natureLevels[0]));
                }
                else
                {
                    whereClause += "na.naturnivå_id IN (";
                    for (var i = 0; i < natureLevels.Count; ++i)
                    {
                        whereClause += "@natureLevel_id" + i;
                        if (i != natureLevels.Count - 1) whereClause += ", ";
                        parameters.Add(new Tuple<string, SqlDbType, object>("@natureLevel_id" + i, SqlDbType.Int,
                            (int)natureLevels[i]));
                    }
                    whereClause += ")";
                }
            }

            if (institutions != null && institutions.Count > 0)
            {
                if (whereClause != "") whereClause += " AND ";
                if (institutions.Count == 1)
                {
                    whereClause += "na.institusjon = @institution";
                    parameters.Add(new Tuple<string, SqlDbType, object>("@institution", SqlDbType.VarChar,
                        institutions[0]));
                }
                else
                {
                    whereClause += "na.institusjon IN (";
                    for (var i = 0; i < institutions.Count; ++i)
                    {
                        whereClause += "@institution" + i;
                        if (i != institutions.Count - 1) whereClause += ", ";
                        parameters.Add(new Tuple<string, SqlDbType, object>("@institution" + i, SqlDbType.VarChar,
                            institutions[i]));
                    }
                    whereClause += ")";
                }
            }

            if (natureAreaTypeCodes != null && natureAreaTypeCodes.Count > 0)
            {
                fromClause += ", NaturområdeType nat";
                if (whereClause != "") whereClause += " AND ";
                whereClause += "na.id = nat.naturområde_id";
                whereClause += " AND ";
                if (natureAreaTypeCodes.Count == 1)
                {
                    var code = natureAreaTypeCodes[0].Replace(" ", "_");
                    whereClause += "nat.kode = @natcode ";
                    parameters.Add(new Tuple<string, SqlDbType, object>("@natcode", SqlDbType.VarChar,
                        code));
                }
                else
                {
                    whereClause += "nat.kode IN (";
                    for (var i = 0; i < natureAreaTypeCodes.Count; ++i)
                    {
                        var code = natureAreaTypeCodes[i].Replace(" ", "_");

                        whereClause += "@natcode" + i;
                        if (i != natureAreaTypeCodes.Count - 1) whereClause += ", ";
                        parameters.Add(new Tuple<string, SqlDbType, object>("@natcode" + i, SqlDbType.VarChar,
                            code));
                    }
                    whereClause += ")";
                }
            }

            if (descriptionVariableCodes != null && descriptionVariableCodes.Count > 0)
            {
                fromClause += ", Beskrivelsesvariabel dv";
                if (whereClause != "") whereClause += " AND ";
                whereClause += "na.id = dv.naturområde_id";
                whereClause += " AND ";
                if (descriptionVariableCodes.Count == 1)
                {
                    whereClause += "dv.kode = @dvcode ";
                    parameters.Add(new Tuple<string, SqlDbType, object>("@dvcode", SqlDbType.VarChar,
                        descriptionVariableCodes[0]));
                }
                else
                {
                    whereClause += "dv.kode IN (";
                    for (var i = 0; i < descriptionVariableCodes.Count; ++i)
                    {
                        whereClause += "@dvcode" + i;
                        if (i != descriptionVariableCodes.Count - 1) whereClause += ", ";
                        parameters.Add(new Tuple<string, SqlDbType, object>("@dvcode" + i, SqlDbType.VarChar,
                            descriptionVariableCodes[i]));
                    }
                    whereClause += ")";
                }
            }

            if ((municipalities != null && municipalities.Count > 0) || (counties != null && counties.Count > 0) ||
                (conservationAreas != null && conservationAreas.Count > 0))
            {
                if (whereClause != "") whereClause += " AND ";
                whereClause += "na.id in (";

                var subQuery = "";

                if (municipalities != null && municipalities.Count > 0)
                {
                    subQuery +=
                        "SELECT al.naturområde_id FROM OmrådeLink al, Område a WHERE al.geometri_id = a.id AND a.geometriType_id = 1 AND a.nummer";
                    if (municipalities.Count == 1)
                    {
                        subQuery += " = @municipality";
                        parameters.Add(new Tuple<string, SqlDbType, object>("@municipality", SqlDbType.Int,
                            municipalities[0]));
                    }
                    else
                    {
                        subQuery += " in (";
                        for (var i = 0; i < municipalities.Count; ++i)
                        {
                            subQuery += "@municipality" + i;
                            if (i != municipalities.Count - 1) subQuery += ", ";
                            parameters.Add(new Tuple<string, SqlDbType, object>("@municipality" + i, SqlDbType.Int,
                                municipalities[i]));
                        }
                        subQuery += ")";
                    }
                }

                if (counties != null && counties.Count > 0)
                {
                    if (!string.IsNullOrEmpty(subQuery)) subQuery += " INTERSECT ";

                    subQuery +=
                        "SELECT al.naturområde_id FROM OmrådeLink al, Område a WHERE al.geometri_id = a.id AND a.geometriType_id = 2 AND a.nummer";
                    if (counties.Count == 1)
                    {
                        subQuery += " = @fylke";
                        parameters.Add(new Tuple<string, SqlDbType, object>("@fylke", SqlDbType.Int, counties[0]));
                    }
                    else
                    {
                        subQuery += " in (";
                        for (var i = 0; i < counties.Count; ++i)
                        {
                            subQuery += "@fylke" + i;
                            if (i != counties.Count - 1) subQuery += ", ";
                            parameters.Add(new Tuple<string, SqlDbType, object>("@fylke" + i, SqlDbType.Int,
                                counties[i]));
                        }
                        subQuery += ")";
                    }
                }

                if (conservationAreas != null && conservationAreas.Count > 0)
                {
                    if (!string.IsNullOrEmpty(subQuery)) subQuery += " INTERSECT ";

                    subQuery +=
                        "SELECT al.naturområde_id FROM OmrådeLink al, Område a WHERE al.geometri_id = a.id AND a.geometriType_id = 3 AND a.nummer";
                    if (conservationAreas.Count == 1)
                    {
                        subQuery += " = @consArea";
                        parameters.Add(new Tuple<string, SqlDbType, object>("@consArea", SqlDbType.Int,
                            conservationAreas[0]));
                    }
                    else
                    {
                        subQuery += " in (";
                        for (var i = 0; i < conservationAreas.Count; ++i)
                        {
                            subQuery += "@consArea" + i;
                            if (i != conservationAreas.Count - 1) subQuery += ", ";
                            parameters.Add(new Tuple<string, SqlDbType, object>("@consArea" + i, SqlDbType.Int,
                                conservationAreas[i]));
                        }
                        subQuery += ")";
                    }
                }

                whereClause += subQuery;
                whereClause += ")";
            }

            if (!string.IsNullOrEmpty(geometry) && !string.IsNullOrEmpty(boundingBox))
            {
                if (whereClause != "") whereClause += " AND ";
                var area = SqlGeometry.STGeomFromText(new SqlChars(geometry), espgCode).MakeValid();
                var bbox = SqlGeometry.STGeomFromText(new SqlChars(boundingBox), espgCode).MakeValid();
                var areaIntersection = area.STIntersection(bbox);

                if (areaIntersection.STIsEmpty())
                {
                    return false;
                }
                whereClause += "na.geometri.STIntersects(@area) = 1";
                parameters.Add(new Tuple<string, SqlDbType, object>("@area", SqlDbType.VarBinary,
                    areaIntersection.Serialize()));
            }
            else if (!string.IsNullOrEmpty(boundingBox))
            {
                if (whereClause != "") whereClause += " AND ";
                var bbox = SqlGeometry.STGeomFromText(new SqlChars(boundingBox), espgCode).MakeValid();
                whereClause += "na.geometri.STIntersects(@area) = 1";
                parameters.Add(new Tuple<string, SqlDbType, object>("@area", SqlDbType.VarBinary, bbox.Serialize()));
            }
            else if (!string.IsNullOrEmpty(geometry))
            {
                if (whereClause != "") whereClause += " AND ";
                var area = SqlGeometry.STGeomFromText(new SqlChars(geometry), espgCode).MakeValid();
                whereClause += "na.geometri.STIntersects(@area) = 1";
                parameters.Add(new Tuple<string, SqlDbType, object>("@area", SqlDbType.VarBinary, area.Serialize()));
            }
            return true;
        }

        private static Collection<NatureArea> GetNatureAreaGeometries(string fromClause, string whereClause,
            IEnumerable<Tuple<string, SqlDbType, object>> parameters, bool centerPoints)
        {
            var natureAreas = new Collection<NatureArea>();
            var sql =
                "SELECT " +
                "na.id, " +
                "na.localId, ";

            int geometryReduction = 0;
            if (centerPoints)
                sql += "na.geometriSenterpunkt";
            else if (geometryReduction != 0)
                sql += "na.geometri.Reduce(" + geometryReduction + ")";
            else
                sql += "na.geometri";

            sql += " FROM " + fromClause;

            if (!string.IsNullOrEmpty(whereClause))
                sql += " WHERE " + whereClause;

            using (var cmd = SqlStatement(sql))
            {
                foreach (var parameter in parameters)
                    cmd.AddParameter(parameter.Item1, parameter.Item2, parameter.Item3);

                using (var reader = cmd.ExecuteReader())
                {
                    var processedNatureAreaIds = new HashSet<int>();

                    while (reader.Read())
                    {
                        var natureArea = new NatureArea
                        {
                            Id = reader.GetInt32(0),
                            UniqueId = new Identification { LocalId = reader.GetGuid(1) }
                        };
                        var sqlBytes = reader.GetSqlBytes(2);
                        natureArea.Area = SqlGeometry.Deserialize(sqlBytes);

                        if (processedNatureAreaIds.Contains(natureArea.Id)) continue;
                        natureAreas.Add(natureArea);
                        processedNatureAreaIds.Add(natureArea.Id);
                    }
                }
            }

            if (centerPoints) return natureAreas;

            foreach (var natureArea in natureAreas)
                natureArea.Parameters = GetParameters(natureArea.Id, false);

            return natureAreas;
        }

        private static Collection<NatureArea> GetNatureAreaInfos(string fromClause, string whereClause,
            IEnumerable<Tuple<string, SqlDbType, object>> parameters, int indexFrom, int indexTo, int infoLevel,
            out int natureAreaCount)
        {
            var processedNatureAreaIdCount = 0;
            var processedNatureAreaIds = new HashSet<int>();
            var natureAreas = new Collection<NatureArea>();
            var sql =
                "SELECT " +
                "na.id, " +
                "na.localId, " +
                "na.naturnivå_id, " +
                "na.kartlagt, " +
                "na.institusjon";

            if (infoLevel == 2)
                sql += ", na.kartlegger_id";

            sql += " FROM " + fromClause;

            if (!string.IsNullOrEmpty(whereClause))
                sql += " WHERE " + whereClause;

            sql += " ORDER BY na.id";

            using (var cmd = SqlStatement(sql))
            {
                foreach (var parameter in parameters)
                    cmd.AddParameter(parameter.Item1, parameter.Item2, parameter.Item3);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var natureArea = new NatureAreaExport
                        {
                            Id = reader.GetInt32(0),
                            UniqueId = new Identification { LocalId = reader.GetGuid(1) },
                            Nivå = (NatureLevel)reader.GetInt32(2)
                        };
                        if (!reader.IsDBNull(3)) natureArea.Surveyed = reader.GetDateTime(3);
                        natureArea.Institution = reader.GetString(4);

                        if (infoLevel == 2)
                        {
                            if (!reader.IsDBNull(5))
                            {
                                natureArea.Surveyer = new Contact { Id = reader.GetInt32(5) };
                            }
                        }

                        if (processedNatureAreaIds.Contains(natureArea.Id)) continue;
                        processedNatureAreaIdCount++;
                        if (processedNatureAreaIdCount >= indexFrom && processedNatureAreaIdCount <= indexTo)
                            natureAreas.Add(natureArea);
                        processedNatureAreaIds.Add(natureArea.Id);
                    }
                }
            }

            switch (infoLevel)
            {
                case 1:
                    if (ReturnStatisticsCache(whereClause, natureAreas.Count))
                        natureAreas = _natureAreaCache;
                    else
                    {
                        var natureAreaIds = new Collection<int>();
                        foreach (var natureArea in natureAreas)
                            natureAreaIds.Add(natureArea.Id);
                        var natureAreaTypes = GetNatureAreaTypes(natureAreaIds);
                        var i = 0;
                        foreach (var natureArea in natureAreas)
                        {
                            while (i != natureAreaTypes.Count && natureAreaTypes[i].NatureAreaId == natureArea.Id)
                            {
                                natureArea.Parameters.Add(natureAreaTypes[i]);
                                ++i;
                            }
                        }

                        SaveStatisticsCache(whereClause, natureAreas);
                    }
                    break;
                case 2:
                    foreach (var natureArea in natureAreas)
                    {
                        natureArea.Parameters = GetParameters(natureArea.Id, true);
                        if (natureArea.Surveyer != null)
                            natureArea.Surveyer = GetContact(natureArea.Surveyer.Id);
                        var metadata =
                            GetMetadatasByNatureAreaLocalIds(
                                new Collection<string> { natureArea.UniqueId.LocalId.ToString() }, false);
                        if (metadata.Count != 1) continue;
                        ((NatureAreaExport)natureArea).MetadataSurveyScale = metadata[0].SurveyScale;
                        ((NatureAreaExport)natureArea).MetadataProgram = metadata[0].Program;
                        ((NatureAreaExport)natureArea).MetadataContractor = metadata[0].Contractor.Company;
                    }
                    break;
            }

            natureAreaCount = processedNatureAreaIdCount;
            return natureAreas;
        }

        private static bool ReturnStatisticsCache(string whereClause, int natureAreaCount)
        {
            return whereClause.Equals("") && _natureAreaCache != null && natureAreaCount == _natureAreaCache.Count;
        }

        private static void SaveStatisticsCache(string whereClause, Collection<NatureArea> natureAreas)
        {
            if (whereClause.Equals("") && _natureAreaCache == null)
                _natureAreaCache = natureAreas;
        }

        private static Collection<NatureArea> GetNatureAreasByMetadataId(int metadataId, Collection<string> localIds)
        {
            var whereClause = "kartlagtOmråde_id = @metadata_id";

            var parameters = new Collection<Tuple<string, SqlDbType, object>>
            {
                new Tuple<string, SqlDbType, object>("@metadata_id", SqlDbType.Int, metadataId)
            };

            if (localIds.Count == 1)
            {
                whereClause +=
                    " AND " +
                    "localId = @localId";

                parameters.Add(new Tuple<string, SqlDbType, object>("@localId", SqlDbType.UniqueIdentifier,
                    new Guid(localIds[0])));
            }
            else if (localIds.Count > 1)
            {
                whereClause +=
                    " AND " +
                    "localId IN (";

                for (var i = 0; i < localIds.Count; ++i)
                {
                    whereClause += "@localId" + i;
                    if (i != localIds.Count - 1)
                        whereClause += ", ";
                    parameters.Add(new Tuple<string, SqlDbType, object>("@localId" + i, SqlDbType.UniqueIdentifier,
                        new Guid(localIds[i])));
                }

                whereClause += ")";
            }

            return GetNatureAreas(whereClause, parameters);
        }

        private static Collection<NatureArea> GetNatureAreas(string whereClause,
            IEnumerable<Tuple<string, SqlDbType, object>> parameters)
        {
            var natureAreas = new Collection<NatureArea>();
            var sql =
                "SELECT " +
                "na.id, " +
                //"na.metadata_id, " +
                "na.localId, " +
                "na.navnerom, " +
                "na.versjonId, " +
                "na.versjon, " +
                "na.naturnivå_id, " +
                "na.geometri, " +
                "na.kartlegger_id, " +
                "na.kartlagt, " +
                "na.beskrivelse, " +
                "na.institusjon " +
                "FROM " +
                "Naturområde na " +
                "WHERE " +
                whereClause;

            using (var cmd = SqlStatement(sql))
            {

                foreach (var parameter in parameters)
                    cmd.AddParameter(parameter.Item1, parameter.Item2, parameter.Item3);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var natureArea = new NatureArea
                        {
                            Id = reader.GetInt32(0),
                            UniqueId = new Identification
                            {
                                LocalId = reader.GetGuid(1),
                                NameSpace = reader.GetString(2)
                            }
                        };
                        if (!reader.IsDBNull(3)) natureArea.UniqueId.VersionId = reader.GetString(3);
                        natureArea.Version = reader.GetString(4);
                        natureArea.Nivå = (NatureLevel)reader.GetInt32(5);
                        natureArea.Area = SqlGeometry.Deserialize(reader.GetSqlBytes(6));
                        if (!reader.IsDBNull(7))
                        {
                            natureArea.Surveyer = new Contact { Id = reader.GetInt32(7) };
                        }
                        if (!reader.IsDBNull(8)) natureArea.Surveyed = reader.GetDateTime(8);
                        if (!reader.IsDBNull(9)) natureArea.Description = reader.GetString(9);
                        if (!reader.IsDBNull(10)) natureArea.Institution = reader.GetString(10);
                        natureAreas.Add(natureArea);
                    }
                }
            }

            foreach (var natureArea in natureAreas)
            {
                if (natureArea.Surveyer != null)
                    natureArea.Surveyer = GetContact(natureArea.Surveyer.Id);

                natureArea.Parameters = GetParameters(natureArea.Id, true);
                natureArea.Documents = GetDocuments(0, natureArea.Id);
            }

            return natureAreas;
        }

        private static Contact GetContact(int id)
        {
            var contact = new Contact();

            const string sql = "SELECT " +
                               "firmanavn, " +
                               "kontaktperson, " +
                               "epost, " +
                               "telefon, " +
                               "hjemmeside " +
                               "FROM " +
                               "Kontakt " +
                               "WHERE " +
                               "id = @id";

            using (var cmd = SqlStatement(sql))
            {
                cmd.AddParameter("@id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        contact.Id = id;
                        contact.Company = reader.GetString(0);
                        if (!reader.IsDBNull(1)) contact.ContactPerson = reader.GetString(1);
                        if (!reader.IsDBNull(2)) contact.Email = reader.GetString(2);
                        if (!reader.IsDBNull(3)) contact.Phone = reader.GetString(3);
                        if (!reader.IsDBNull(4)) contact.Homesite = reader.GetString(4);
                    }
                }
                return contact;
            }
        }

        private static Collection<Document> GetDocuments(int metadataId = 0,
            int natureAreaId = 0, int areaLayerTypeId = 0, int gridLayerTypeId = 0)
        {
            var documents = new Collection<Document>();

            var sql =
                "SELECT " +
                "id, " +
                "doc_guid, " +
                "tittel, " +
                "beskrivelse, " +
                "kartlegger_id, " +
                "filsti " +
                "FROM " +
                "Dokument " +
                "WHERE ";

            if (metadataId != 0) sql += "kartlagtOmråde_id = @metadata_id";
            else if (natureAreaId != 0) sql += "naturområde_id = @natureArea_id";
            else if (areaLayerTypeId != 0) sql += "områdeKartType_id = @areaLayerType_id";
            else if (gridLayerTypeId != 0) sql += "rutenettkartType_id = @gridLayerType_id";

            using (var cmd = SqlStatement(sql))
            {
                if (metadataId != 0) cmd.AddParameter("@metadata_id", metadataId);
                else if (natureAreaId != 0) cmd.AddParameter("@natureArea_id", natureAreaId);
                else if (areaLayerTypeId != 0)
                    cmd.AddParameter("@areaLayerType_id", areaLayerTypeId);
                else if (gridLayerTypeId != 0)
                    cmd.AddParameter("@gridLayerType_id", gridLayerTypeId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var document = new Document
                        {
                            Id = reader.GetInt32(0),
                            Guid = reader.GetGuid(1),
                            Title = reader.GetString(2),
                            Description = reader.GetString(3)
                        };
                        var authorId = reader.GetInt32(4);
                        if (authorId != 0)
                            document.Author = new Contact { Id = authorId };
                        document.FileName = reader.GetString(5);
                        documents.Add(document);
                    }
                }
            }

            foreach (var document in documents)
                if (document.Author != null)
                    document.Author = GetContact(document.Author.Id);

            return documents;
        }

        private static List<Parameter> GetParameters(int natureAreaId, bool addDescriptionVariables)
        {
            var parameters = new List<Parameter>();

            if (addDescriptionVariables)
                parameters.AddRange(GetDescriptionVariables(natureAreaId));
            parameters.AddRange(GetNatureAreaTypes(natureAreaId));

            return parameters;
        }

        private static Collection<DescriptionVariable> GetDescriptionVariables(int natureAreaId = 0, int natureAreaTypeId = 0)
        {
            var descriptionVariables = new Collection<DescriptionVariable>();

            var sql =
                "SELECT " +
                "id, " +
                "kode, " +
                "kartlegger_id, " +
                "kartlagt, " +
                "trinn, " +
                "beskrivelse " +
                "FROM " +
                "Beskrivelsesvariabel " +
                "WHERE ";

            if (natureAreaId != 0)
                sql +=
                    "naturområde_id = @natureArea_id" +
                    " AND " +
                    "naturområdetype_id IS NULL";
            else if (natureAreaTypeId != 0) sql += "naturområdetype_id = @natureAreaType_id";

            using (var cmd = SqlStatement(sql))
            {
                if (natureAreaId != 0) cmd.AddParameter("@natureArea_id", natureAreaId);
                else if (natureAreaTypeId != 0)
                    cmd.AddParameter("@natureAreaType_id", natureAreaTypeId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var descriptionVariable = new DescriptionVariable
                        {
                            Id = reader.GetInt32(0),
                            Code = reader.GetString(1)
                        };
                        if (!reader.IsDBNull(2))
                            descriptionVariable.Surveyer = new Contact { Id = reader.GetInt32(2) };
                        if (!reader.IsDBNull(3)) descriptionVariable.Surveyed = reader.GetDateTime(3);
                        if (!reader.IsDBNull(4)) descriptionVariable.Value = reader.GetString(4);
                        if (!reader.IsDBNull(5)) descriptionVariable.Description = reader.GetString(5);
                        descriptionVariables.Add(descriptionVariable);
                    }
                }
            }

            foreach (var descriptionVariable in descriptionVariables)
                if (descriptionVariable.Surveyer != null)
                    descriptionVariable.Surveyer = GetContact(descriptionVariable.Surveyer.Id);

            return descriptionVariables;
        }

        private static IEnumerable<NatureAreaType> GetNatureAreaTypes(int natureAreaId)
        {
            var natureAreaTypes = new Collection<NatureAreaType>();

            const string sql = "SELECT " +
                               "id, " +
                               "kode, " +
                               "kartlegger_id, " +
                               "kartlagt, " +
                               "andel " +
                               "FROM " +
                               "NaturområdeType " +
                               "WHERE " +
                               "naturområde_id = @natureArea_id";

            using (var cmd = SqlStatement(sql))
            {
                cmd.AddParameter("@natureArea_id", natureAreaId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var natureAreaType = new NatureAreaType
                        {
                            Id = reader.GetInt32(0),
                            Code = reader.GetString(1)
                        };
                        if (!reader.IsDBNull(2))
                            natureAreaType.Surveyer = new Contact { Id = reader.GetInt32(2) };
                        if (!reader.IsDBNull(3)) natureAreaType.Surveyed = reader.GetDateTime(3);
                        natureAreaType.Share = reader.GetDouble(4);
                        natureAreaTypes.Add(natureAreaType);
                    }
                }
            }

            foreach (var natureAreaType in natureAreaTypes)
            {
                natureAreaType.AdditionalVariables = GetDescriptionVariables(0, natureAreaType.Id);
                natureAreaType.CustomVariables = GetCustomVariables(natureAreaType.Id);

                if (natureAreaType.Surveyer == null) continue;
                natureAreaType.Surveyer = GetContact(natureAreaType.Surveyer.Id);
            }

            return natureAreaTypes;
        }

        private static Collection<NatureAreaType> GetNatureAreaTypes(IEnumerable<int> natureAreaIds)
        {
            var natureAreaTypes = new Collection<NatureAreaType>();

            var idTable = new DataTable();
            idTable.Columns.Add("id", typeof(int));

            using (var cmd = StoredProc("getNaturområdeTypeCodes"))
            {
                foreach (var natureAreaId in natureAreaIds)
                {
                    var row = new object[1];
                    row[0] = natureAreaId;
                    idTable.Rows.Add(row);
                }

                var idTableParameter = new SqlParameter("@ids", SqlDbType.Structured)
                {
                    TypeName = "Ids",
                    Value = idTable
                };

                cmd.AddParameter(idTableParameter);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var natureAreaType = new NatureAreaType
                        {
                            NatureAreaId = reader.GetInt32(0),
                            Code = reader.GetString(1)
                        };
                        natureAreaTypes.Add(natureAreaType);
                    }
                }

                cmd.ExecuteNonQuery();

                return natureAreaTypes;
            }
        }

        private static Collection<CustomVariable> GetCustomVariables(int natureAreaTypeId)
        {
            var customVariables = new Collection<CustomVariable>();

            const string sql = "SELECT " +
                               "id, " +
                               "spesifikasjon, " +
                               "trinn " +
                               "FROM " +
                               "EgendefinertVariabel " +
                               "WHERE " +
                               "naturområdetype_id = @natureAreaType_id";

            using (var cmd = SqlStatement(sql))
            {
                cmd.AddParameter("@natureAreaType_id", natureAreaTypeId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var customVariable = new CustomVariable
                        {
                            Id = reader.GetInt32(0),
                            Specification = reader.GetString(1),
                            Value = reader.GetString(2)
                        };
                        customVariables.Add(customVariable);
                    }
                }
                return customVariables;
            }
        }

        private static List<NinVariabelDefinisjon> GetVariableDefinitions(int metadataId)
        {
            var variableDefinitions = new List<NinVariabelDefinisjon>();

            variableDefinitions.AddRange(GetCustomVariableDefinitions(metadataId));
            variableDefinitions.AddRange(GetStandardVariables(metadataId));

            return variableDefinitions;
        }

        private static IEnumerable<CustomVariableDefinition> GetCustomVariableDefinitions(int metadataId)
        {
            var customVariableDefinitions = new Collection<CustomVariableDefinition>();

            const string sql = "SELECT " +
                               "id, " +
                               "spesifikasjon, " +
                               "beskrivelse " +
                               "FROM " +
                               "EgendefinertVariabelDefinisjon " +
                               "WHERE " +
                               "kartlagtOmråde_id = @metadata_id";

            using (var cmd = SqlStatement(sql))
            {
                cmd.AddParameter("@metadata_id", metadataId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var customVariableDefinition = new CustomVariableDefinition
                        {
                            Id = reader.GetInt32(0),
                            Specification = reader.GetString(1),
                            Description = reader.GetString(2)
                        };
                        customVariableDefinitions.Add(customVariableDefinition);
                    }
                }

                return customVariableDefinitions;
            }
        }

        private static IEnumerable<NinStandardVariabel> GetStandardVariables(int metadataId)
        {
            var standardVariables = new Collection<NinStandardVariabel>();

            const string sql = "SELECT " +
                               "id, " +
                               "koderegister, " +
                               "kodeversjon, " +
                               "kode " +
                               "FROM " +
                               "Standardvariabel " +
                               "WHERE " +
                               "kartlagtOmråde_id = @metadata_id";

            using (var cmd = SqlStatement(sql))
            {
                cmd.AddParameter("@metadata_id", metadataId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var standardVariable = new NinStandardVariabel
                        {
                            Id = reader.GetInt32(0),
                            VariableDefinition = new Code
                            {
                                Registry = reader.GetString(1),
                                Version = reader.GetString(2),
                                Value = reader.GetString(3)
                            }
                        };
                        standardVariables.Add(standardVariable);
                    }
                }
                return standardVariables;
            }
        }

        public static void DeleteDataDelivery(int docId)
        {
            DeleteNatureAreas(docId);

            const string sql = "DELETE FROM " +
                               "Dataleveranse " +
                               "WHERE " +
                               "doc_id = @doc_id";

            using (var cmd = SqlStatement(sql))
            {
                cmd.AddParameter("@doc_id", docId);
                cmd.ExecuteNonQuery();
            }
        }

        private static void DeleteNatureAreas(int docId)
        {
            DeleteNatureAreaTypes(docId);

            const string sql = "DELETE FROM " +
                               "Naturområde " +
                               "WHERE " +
                               "kartlagtOmråde_id IN (" +
                               "SELECT " +
                               "m.id " +
                               "FROM " +
                               "KartlagtOmråde m, " +
                               "Dataleveranse d " +
                               "WHERE " +
                               "m.dataleveranse_id = d.id " +
                               "AND " +
                               "d.doc_id = @doc_id" +
                               ")";

            using (var cmd = SqlStatement(sql))
            {
                cmd.AddParameter("@doc_id", docId);
                cmd.ExecuteNonQuery();
            }
        }

        private static void DeleteNatureAreas(Guid metadataLocalId)
        {
            DeleteNatureAreaTypes(metadataLocalId);

            const string sql = "DELETE FROM " +
                               "Naturområde " +
                               "WHERE " +
                               "kartlagtOmråde_id = (" +
                               "SELECT " +
                               "id " +
                               "FROM " +
                               "KartlagtOmråde " +
                               "WHERE " +
                               "localId = @metadataLocalId" +
                               ")";

            using (var cmd = SqlStatement(sql))
            {
                cmd.AddParameter("@metadataLocalId", metadataLocalId);
                cmd.ExecuteNonQuery();
            }
        }

        private static void DeleteNatureAreaTypes(int docId)
        {
            const string sql = "DELETE FROM " +
                               "NaturområdeType " +
                               "WHERE " +
                               "naturområde_id IN (" +
                               "SELECT " +
                               "id " +
                               "FROM " +
                               "Naturområde " +
                               "WHERE " +
                               "kartlagtOmråde_id = (" +
                               "SELECT " +
                               "m.id " +
                               "FROM " +
                               "KartlagtOmråde m, " +
                               "Dataleveranse d " +
                               "WHERE " +
                               "m.dataleveranse_id = d.id " +
                               "AND " +
                               "d.doc_id = @doc_id" +
                               ")" +
                               ")";

            using (var cmd = SqlStatement(sql))
            {
                cmd.AddParameter("@doc_id", docId);
                cmd.ExecuteNonQuery();
            }
        }

        private static void DeleteNatureAreaTypes(Guid metadataLocalId)
        {
            const string sql = "DELETE FROM " +
                               "NaturområdeType " +
                               "WHERE " +
                               "naturområde_id IN (" +
                               "SELECT " +
                               "id " +
                               "FROM " +
                               "Naturområde " +
                               "WHERE " +
                               "kartlagtOmråde_id = (" +
                               "SELECT " +
                               "id " +
                               "FROM " +
                               "KartlagtOmråde " +
                               "WHERE " +
                               "localId = @metadataLocalId" +
                               ")" +
                               ")";

            using (var cmd = SqlStatement(sql))
            {
                cmd.AddParameter("@metadataLocalId", metadataLocalId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void DeleteAreas(AreaType areaType)
        {
            const string sql =
                "DELETE FROM " +
                "Område " +
                "WHERE " +
                "geometriType_id = @areaType_id";

            using (var cmd = SqlStatement(sql))
            {
                cmd.AddParameter("@areaType_id", (int)areaType);
                cmd.ExecuteNonQuery();
            }
        }

        private static Collection<NatureArea> _natureAreaCache;

        public static int QueryRecordCount(string tableName)
        {
            var sql = SqlStatement($"SELECT COUNT(*) FROM {tableName}");
            return (int)sql.ExecuteScalar();
        }

        public static SqlStatement Query(string tableName, int maxRows)
        {
            var sqlStatement = SqlStatement($"SELECT TOP {maxRows} * FROM {tableName} ORDER BY 1 DESC");
            return sqlStatement;
        }

  //      private static PerformanceCounter pc;
        static SqlServer()
        {
//            pc = new PerformanceCounter(".NET Data Provider for SqlServer", "Numb erOfActiveConnectionPools", geti);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int GetCurrentProcessId();

        private static string GetInstanceName()
        {
            //This works for Winforms apps.  
            string instanceName =
                System.Reflection.Assembly.GetEntryAssembly().GetName().Name;

            // Must replace special characters like (, ), #, /, \\  
            string instanceName2 =
                AppDomain.CurrentDomain.FriendlyName.ToString().Replace('(', '[')
                    .Replace(')', ']').Replace('#', '_').Replace('/', '_').Replace('\\', '_');

            // For ASP.NET applications your instanceName will be your CurrentDomain's   
            // FriendlyName. Replace the line above that sets the instanceName with this:  
            // instanceName = AppDomain.CurrentDomain.FriendlyName.ToString().Replace('(','[')  
            // .Replace(')',']').Replace('#','_').Replace('/','_').Replace('\\','_');  

            string pid = GetCurrentProcessId().ToString();
            instanceName = instanceName + "[" + pid + "]";
            Console.WriteLine("Instance Name: {0}", instanceName);
            Console.WriteLine("---------------------------");
            return instanceName;
        }

        private static SqlStatement SqlStatement(string sql)
        {
    //        Console.WriteLine(pc.NextValue());
            return new SqlStatement(sql, Config.Settings.ConnectionString);
        }

        private static StoredProc StoredProc(string storedProc)
        {
            return new StoredProc(storedProc, Config.Settings.ConnectionString);
        }

        public static AreaLayerValues GetAreaLayerValues(AreaType areaType, int number)
        {
            var values = new AreaLayerValues();

            const string sql = @"select al.id, alt.navn, kode, trinn, minimumsverdi, maksimumsverdi from Områdekart al 
INNER JOIN OmrådekartType alt on al.områdeKartType_id = alt.id
INNER JOIN Område a ON a.id = al.geometri_id
WHERE a.nummer = @number AND a.geometriType_id = @areaType";

            using (var cmd = SqlStatement(sql))
            {
                cmd.AddParameter("@areaType", areaType);
                cmd.AddParameter("@number", number);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var value = new AreaLayerValue
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Code = reader.GetString(2),
                            Value = reader.GetString(3),
                            MinValue = reader.GetString(4),
                            MaxValue = reader.GetString(5)
                        };

                        values.Add(value);
                    }
                }
                return values;
            }
        }

        public static int BulkStoreAreaLayer(Layer layer)
        {
            if (layer is GridLayer)
                return BulkStoreGridLayer((GridLayer)layer);
            if (layer is AreaLayer)
                return BulkStoreAreaLayer((AreaLayer)layer);
            throw new Exception($"Ukjent type lag '{layer.GetType().Name}'.");
        }
    }

    public class AreaLayerValues : List<AreaLayerValue>
    {
    }

    public class AreaLayerValue
    {
        public int Id;
        public string Name;
        public string Value;
        public string Code;
        public string MinValue;
        public string MaxValue;
    }
}

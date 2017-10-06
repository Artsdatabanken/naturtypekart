using Microsoft.SqlServer.Types;
using Nin.Types.MsSql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using Types;

namespace Nin.IO.SqlServer
{
    public class Search
    {
        private readonly string _connectionString;

        public Search(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<INatureAreaGeoJson> GetNatureAreasBySearchFilter(SearchFilterRequest searchFilterRequest)
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

        public IEnumerable<INatureAreaGeoJson> GetNatureAreasBySearchFilter(
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
            IEnumerable<INatureAreaGeoJson> natureAreas = new List<INatureAreaGeoJson>();

            Collection<Tuple<string, SqlDbType, object>> parameters;

            var fromClause = new StringBuilder();
            var whereClause = new StringBuilder();

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
                fromClause,
                whereClause,
                out parameters);

            if (!queryOk)
            {
                natureAreaCount = 0;
                return natureAreas;
            }

            if (infoLevel == 0)
            {
                natureAreas = GetNatureAreaGeometries(fromClause.ToString(), whereClause.ToString(), parameters, centerPoints, !string.IsNullOrWhiteSpace(boundingBox));
                natureAreaCount = natureAreas.Count();
            }
            else
            {
                natureAreas = SqlServer.GetNatureAreaInfos(fromClause.ToString(), whereClause.ToString(), parameters, indexFrom, indexTo, infoLevel,
                    out natureAreaCount).Select(n => (INatureAreaGeoJson)n);
            }

            return natureAreas;
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
            StringBuilder fromClause,
            StringBuilder whereClause,
            out Collection<Tuple<string, SqlDbType, object>> parameters
            )
        {
            parameters = new Collection<Tuple<string, SqlDbType, object>>();
            fromClause.Append("Naturområde na");

            FilterOnAttribute(whereClause, parameters, "na.naturnivå_id", "@natureLevel_id", natureLevels.Select(nl => (int)nl).ToList(), SqlDbType.Int);

            FilterOnAttribute(whereClause, parameters, "na.institusjon", "@institution", institutions.ToList(), SqlDbType.VarChar);

            FilterOnNatureAreaTypeCodes(natureAreaTypeCodes, fromClause, whereClause, parameters);

            FilterOnDescriptionVariables(descriptionVariableCodes, fromClause, whereClause, parameters);

            FilterOnGeographicalAreas(municipalities, counties, conservationAreas, whereClause, parameters);

            return FilterOnGeometry(geometry, boundingBox, espgCode, whereClause, parameters);
        }

        private static bool FilterOnGeometry(string geometry, string boundingBox, int espgCode, StringBuilder whereClause, Collection<Tuple<string, SqlDbType, object>> parameters)
        {
            if (!string.IsNullOrEmpty(geometry) && !string.IsNullOrEmpty(boundingBox))
            {
                var area = SqlGeometry.STGeomFromText(new SqlChars(geometry), espgCode).MakeValid();
                var bbox = SqlGeometry.STGeomFromText(new SqlChars(boundingBox), espgCode).MakeValid();
                var areaIntersection = area.STIntersection(bbox);

                if (areaIntersection.STIsEmpty())
                {
                    return false;
                }

                FilterOnGeometry(whereClause, parameters, areaIntersection);
            }
            else if (!string.IsNullOrEmpty(boundingBox))
            {
                var bbox = SqlGeometry.STGeomFromText(new SqlChars(boundingBox), espgCode).MakeValid();

                FilterOnGeometry(whereClause, parameters, bbox);
            }
            else if (!string.IsNullOrEmpty(geometry))
            {
                var area = SqlGeometry.STGeomFromText(new SqlChars(geometry), espgCode).MakeValid();

                FilterOnGeometry(whereClause, parameters, area);
            }

            return true;
        }

        private static void FilterOnGeographicalAreas(IReadOnlyList<int> municipalities, IReadOnlyList<int> counties, IReadOnlyList<int> conservationAreas,
                                                        StringBuilder whereClause, Collection<Tuple<string, SqlDbType, object>> parameters)
        {
            if ((municipalities != null && municipalities.Count > 0) || (counties != null && counties.Count > 0) ||
                            (conservationAreas != null && conservationAreas.Count > 0))
            {
                ChainWhereClause(whereClause);

                whereClause.Append("na.id in (");

                var subQuery = new StringBuilder();

                FilterOnMunicipalities(municipalities, parameters, subQuery);

                FilterOnCounties(counties, parameters, subQuery);

                FilterOnConservationAreas(conservationAreas, parameters, subQuery);

                whereClause.Append(subQuery.ToString());
                whereClause.Append(")");
            }
        }

        private static void FilterOnConservationAreas(IReadOnlyList<int> conservationAreas, Collection<Tuple<string, SqlDbType, object>> parameters, StringBuilder subQuery)
        {
            if (conservationAreas != null && conservationAreas.Count > 0)
            {
                if (subQuery.Length != 0)
                {
                    subQuery.Append(" INTERSECT ");
                }

                FilterOnAttribute(subQuery, parameters,
                    "SELECT al.naturområde_id FROM OmrådeLink al, Område a WHERE al.geometri_id = a.id AND a.geometriType_id = 3 AND a.nummer", "@consArea",
                    conservationAreas.ToList(), SqlDbType.Int, false);
            }
        }

        private static void FilterOnCounties(IReadOnlyList<int> counties, Collection<Tuple<string, SqlDbType, object>> parameters, StringBuilder subQuery)
        {
            if (counties != null && counties.Count > 0)
            {
                if (subQuery.Length != 0)
                {
                    subQuery.Append(" INTERSECT ");
                }

                FilterOnAttribute(subQuery, parameters,
                    "SELECT al.naturområde_id FROM OmrådeLink al, Område a WHERE al.geometri_id = a.id AND a.geometriType_id = 2 AND a.nummer", "@fylke",
                    counties.ToList(), SqlDbType.Int, false);
            }
        }

        private static void FilterOnMunicipalities(IReadOnlyList<int> municipalities, Collection<Tuple<string, SqlDbType, object>> parameters, StringBuilder subQuery)
        {
            if (municipalities != null && municipalities.Count > 0)
            {
                FilterOnAttribute(subQuery, parameters,
                    "SELECT al.naturområde_id FROM OmrådeLink al, Område a WHERE al.geometri_id = a.id AND a.geometriType_id = 1 AND a.nummer", "@municipality",
                    municipalities.ToList(), SqlDbType.Int, false);
            }
        }

        private static void FilterOnDescriptionVariables(IReadOnlyList<string> descriptionVariableCodes, StringBuilder fromClause, StringBuilder whereClause, Collection<Tuple<string, SqlDbType, object>> parameters)
        {
            if (descriptionVariableCodes != null && descriptionVariableCodes.Count > 0)
            {
                fromClause.Append(", Beskrivelsesvariabel dv");

                ChainWhereClause(whereClause);

                whereClause.Append("na.id = dv.naturområde_id AND ");

                FilterOnAttribute(whereClause, parameters, "dv.kode", "@dvcode", descriptionVariableCodes.ToList(), SqlDbType.VarChar, false);
            }
        }

        private static void FilterOnNatureAreaTypeCodes(IReadOnlyList<string> natureAreaTypeCodes, StringBuilder fromClause, StringBuilder whereClause, Collection<Tuple<string, SqlDbType, object>> parameters)
        {
            if (natureAreaTypeCodes != null && natureAreaTypeCodes.Count > 0)
            {
                fromClause.Append(", NaturområdeType nat");

                ChainWhereClause(whereClause);

                whereClause.Append("na.id = nat.naturområde_id AND ");

                FilterOnAttribute(whereClause, parameters, "nat.kode", "@natcode", natureAreaTypeCodes.Select(natc => natc.Replace(" ", "_")).ToList(), SqlDbType.VarChar, false);
            }
        }

        private static void FilterOnGeometry(StringBuilder whereClause, Collection<Tuple<string, SqlDbType, object>> parameters, SqlGeometry paramValue)
        {
            ChainWhereClause(whereClause);

            whereClause.Append("na.geometri.STIntersects(@area) = 1");

            parameters.Add(new Tuple<string, SqlDbType, object>("@area", SqlDbType.VarBinary,
                paramValue.Serialize()));
        }

        private static void FilterOnAttribute<T>(StringBuilder clause, Collection<Tuple<string, SqlDbType, object>> parameters,
                                                    string leftSide, string paramName, IList<T> paramValues, SqlDbType paramType, bool addAnd = true)
        {
            if (paramValues != null && paramValues.Count > 0)
            {
                if (addAnd && clause.Length > 0 && !clause.ToString().EndsWith(" AND "))
                {
                    clause.Append(" AND ");
                }

                if (paramValues.Count == 1)
                {
                    clause.Append($"{ leftSide } = { paramName }");
                    parameters.Add(new Tuple<string, SqlDbType, object>(paramName, paramType,
                         paramValues[0]));
                }
                else
                {
                    clause.Append($"{ leftSide } IN (");

                    for (var i = 0; i < paramValues.Count; ++i)
                    {
                        clause.Append($"{ paramName }{i}");
                        if (i != paramValues.Count - 1)
                        {
                            clause.Append(", ");
                        }

                        parameters.Add(new Tuple<string, SqlDbType, object>($"{ paramName }{i}", paramType, paramValues[i]));
                    }

                    clause.Append(")");
                }
            }
        }

        private IEnumerable<INatureAreaGeoJson> GetNatureAreaGeometries(string fromClause, string whereClause,
            IEnumerable<Tuple<string, SqlDbType, object>> parameters, bool centerPoints, bool isBounded)
        {
            var natureAreas = new List<INatureAreaGeoJson>();

            var sql = new StringBuilder();

            string groupByClause = null;

            bool groupByKommuner = centerPoints && !isBounded;

            if (groupByKommuner)
            {
                sql.Append("SELECT o.id, COUNT(*)");
                fromClause += ", OmrådeLink ol, Område o";
                whereClause += (!string.IsNullOrWhiteSpace(whereClause) ? " AND " : string.Empty) + "na.id = ol.naturområde_id AND o.id = ol.geometri_id AND o.geometriType_id = 1";
                groupByClause = "o.id";
            }
            else
            {
                sql.Append("SELECT na.id, na.localId, ");

                if (centerPoints)
                {
                    sql.Append("na.geometriSenterpunkt");
                }
                else
                {
                    sql.Append("na.geometri");
                }
            }

            sql.Append(" FROM " + fromClause);

            if (!string.IsNullOrWhiteSpace(whereClause))
            {
                sql.Append(" WHERE " + whereClause);
            }

            if (!string.IsNullOrWhiteSpace(groupByClause))
            {
                sql.Append(" GROUP BY " + groupByClause);
            }

            var idsAndCounts = new List<(int id, int count)>();

            using (var cmd = SqlStatement(sql.ToString()))
            {
                foreach (var parameter in parameters)
                {
                    cmd.AddParameter(parameter.Item1, parameter.Item2, parameter.Item3);
                }

                using (var reader = cmd.ExecuteReader())
                {
                    if (groupByKommuner)
                    {
                        ReadIdsAndCounts(idsAndCounts, reader);
                    }
                    else
                    {
                        ReadNatureAreaGeoJson(natureAreas, reader, true);
                    }
                }
            }

            if (groupByKommuner)
            {
                PopulateGeometryForKommuner(natureAreas, idsAndCounts);
            }

            if (centerPoints)
            {
                return natureAreas;
            }

            foreach (var natureArea in natureAreas)
            {
                natureArea.Parameters = SqlServer.GetParameters(natureArea.Id, false);
            }

            return natureAreas;
        }

        private static void ReadIdsAndCounts(List<(int id, int count)> idsAndCounts, System.Data.SqlClient.SqlDataReader reader)
        {
            while (reader.Read())
            {
                int id = reader.GetInt32(0);
                int count = reader.GetInt32(1);

                idsAndCounts.Add((id: id, count: count));
            }
        }

        private void PopulateGeometryForKommuner(List<INatureAreaGeoJson> natureAreas, List<(int id, int count)> idsAndCounts)
        {
            if (idsAndCounts.Count == 0)
            {
                return;
            }

            var paramStr = string.Join(",", idsAndCounts.Select(iac => "@id" + iac.id));

            using (var cmd = SqlStatement($"SELECT o.id, o.geometri.STCentroid() FROM Område o WHERE id IN ({ paramStr })"))
            {
                foreach (var iac in idsAndCounts)
                {
                    cmd.AddParameter("@id" + iac.id, SqlDbType.Int, iac.id);
                }

                using (var reader = cmd.ExecuteReader())
                {
                    ReadNatureAreaGeoJson(natureAreas, reader, false, idsAndCounts);
                }
            }
        }

        private static void ReadNatureAreaGeoJson(List<INatureAreaGeoJson> natureAreas, System.Data.SqlClient.SqlDataReader reader, bool readUniqueId, List<(int id, int count)> idsAndCounts = null)
        {
            var processedNatureAreaIds = new HashSet<int>();

            while (reader.Read())
            {
                int index = 0;
                var natureArea = new NatureAreaGeoJson
                {
                    Id = reader.GetInt32(index++),
                    UniqueId = new Identification { LocalId = readUniqueId ? reader.GetGuid(index++) : Guid.Empty }
                };

                if (processedNatureAreaIds.Contains(natureArea.Id))
                {
                    continue;
                }

                var sqlBytes = reader.GetSqlBytes(index++);
                natureArea.Area = SqlGeometry.Deserialize(sqlBytes);

                if (idsAndCounts != null && idsAndCounts.Count > 0)
                {
                    natureArea.Count = idsAndCounts.First(iac => iac.id == natureArea.Id).count;
                }

                natureAreas.Add(natureArea);
                processedNatureAreaIds.Add(natureArea.Id);
            }
        }

        private SqlStatement SqlStatement(string sql)
        {
            return new SqlStatement(sql, _connectionString);
        }

        private static void ChainWhereClause(StringBuilder whereClause)
        {
            if (whereClause.Length != 0)
            {
                whereClause.Append(" AND ");
            }
        }
    }
}
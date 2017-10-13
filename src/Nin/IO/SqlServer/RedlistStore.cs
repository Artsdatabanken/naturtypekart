using Dapper;
using Microsoft.SqlServer.Types;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;

namespace Nin.IO.SqlServer
{
    public class RedlistStore
    {
        private readonly string _connectionString;

        public RedlistStore(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<NatureAreaCountByRedlistCategory> GetNatureAreaCountsByCategory(string geometry = null, int? epsg = null)
        {
            var builder = new SqlBuilder();

            var template = builder.AddTemplate(
 @"SELECT rlk.kategori_id AS Id, rlkt.verdi AS Name, COUNT(*) AS Count
FROM Rødlistekategori rlk
/**join**/
/**where**/
GROUP BY rlk.kategori_id, rlkt.verdi");

            builder.Join("KategoriSet rlkt ON rlkt.Id = rlk.kategori_id");

            FilterOnGeometry(geometry, epsg, builder);

            return Query<NatureAreaCountByRedlistCategory>(template);
        }

        public IEnumerable<NatureAreaCountByRedlistTheme> GetNatureAreaCountsByTheme(string geometry = null, int? epsg = null)
        {
            var builder = new SqlBuilder();

            var template = builder.AddTemplate(
 @"SELECT rlk.rødlistevurderingsenhet_id AS AssessmentUnitId, rlvt.verdi AS AssessmentUnitName, rlvt.Tema_Id AS ThemeId, rltt.verdi AS ThemeName, COUNT(*) AS AssessmentUnitCount
FROM Rødlistekategori rlk
/**join**/
/**where**/
GROUP BY rlk.rødlistevurderingsenhet_id, rlvt.verdi, rlvt.Tema_Id, rltt.verdi");

            builder.Join("RødlisteVurderingsenhetSet rlvt ON rlvt.Id = rlk.rødlistevurderingsenhet_id");
            builder.Join("TemaSet rltt ON rltt.Id = rlvt.Tema_Id");

            FilterOnGeometry(geometry, epsg, builder);

            var flatCounts = Query<AssessmentUnitCountDto>(template);

            return BuildTwoLevelTree(flatCounts);
        }

        private static IEnumerable<NatureAreaCountByRedlistTheme> BuildTwoLevelTree(IEnumerable<AssessmentUnitCountDto> flatCounts)
        {
            var dict = new Dictionary<int, NatureAreaCountByRedlistTheme>();

            foreach (var flatCount in flatCounts)
            {
                HandleFlatCount(dict, flatCount);
            }

            OrderAssessmentUnits(dict);

            return dict.Values.OrderBy(v => v.Name).ToList();
        }

        private static void OrderAssessmentUnits(Dictionary<int, NatureAreaCountByRedlistTheme> dict)
        {
            foreach (var value in dict.Values)
            {
                value.CountsByAssessmentUnit = value.CountsByAssessmentUnit.OrderBy(c => c.Name).ToList();
            }
        }

        private static void HandleFlatCount(Dictionary<int, NatureAreaCountByRedlistTheme> dict, AssessmentUnitCountDto flatCount)
        {
            NatureAreaCountByRedlistTheme theme = null;

            if (!dict.ContainsKey(flatCount.ThemeId))
            {
                theme = new NatureAreaCountByRedlistTheme(flatCount.ThemeId, flatCount.ThemeName);
                dict.Add(theme.Id, theme);
            }
            else
            {
                theme = dict[flatCount.ThemeId];
            }

            theme.CountsByAssessmentUnit.Add(new NatureAreaCountByRedlistAssessmentUnit
            {
                Id = flatCount.AssessmentUnitId,
                Name = flatCount.AssessmentUnitName,
                Count = flatCount.AssessmentUnitCount
            });
        }

        private static void FilterOnGeometry(string geometry, int? epsg, SqlBuilder builder)
        {
            if (!string.IsNullOrWhiteSpace(geometry))
            {
                builder.Join("Naturområde no ON no.id = rlk.naturområde_id");
                builder.Where("no.geometri.STIntersects(@Area) = 1", new { Area = SqlGeometry.STGeomFromText(new SqlChars(geometry), epsg.Value).MakeValid() });
            }
        }

        private IEnumerable<T> Query<T>(SqlBuilder.Template template)
        {
            string sql = template.RawSql;
            var parameters = template.Parameters;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                return conn.Query<T>(sql, parameters).ToList();
            }
        }

        private class AssessmentUnitCountDto
        {
            public int AssessmentUnitId { get; set; }
            public string AssessmentUnitName { get; set; }
            public int ThemeId { get; set; }
            public string ThemeName { get; set; }
            public int AssessmentUnitCount { get; set; }
        }
    }

    public class NatureAreaCountByRedlistTheme
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<NatureAreaCountByRedlistAssessmentUnit> CountsByAssessmentUnit { get; set; }

        public int Count
        {
            get
            {
                return CountsByAssessmentUnit.Sum(cbau => cbau.Count);
            }
        }

        public NatureAreaCountByRedlistTheme()
        {
            CountsByAssessmentUnit = new List<NatureAreaCountByRedlistAssessmentUnit>();
        }

        public NatureAreaCountByRedlistTheme(int id, string name) : this()
        {
            Id = id;
            Name = name;
        }
    }

    public class NatureAreaCountByRedlistAssessmentUnit
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
    }

    public class NatureAreaCountByRedlistCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
    }
}
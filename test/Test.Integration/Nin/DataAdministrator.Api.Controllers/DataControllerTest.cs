using Nin.IO;
using Nin.IO.SqlServer;
using NUnit.Framework;
using System.Linq;
using Test.Integration.Nin.Common;

namespace Test.Integration.Nin.DataAdministrator.Api.Controllers
{
    [Ignore("Test file deleted from repo because of size.")]
    public class DataControllerTest
    {
        public DataControllerTest()
        {
            var dataDelivery = TestDataDelivery.Create(@"Data\Area\ar5\mgk_ar5_nin.shp", 25833, 0, 10);
            SqlServer.LagreDataleveranse(dataDelivery);
        }

        [Test]
        public void GetNatureAreasBySearchFilter()
        {
            var request = new SearchFilterRequest { CenterPoints = true };
            var r = SqlServer.GetNatureAreasBySearchFilter(request);
            Assert.True(r.First().Area.InstanceOf("POINT").IsTrue);
        }

        [Test]
        public void GetNatureAreasSummary()
        {
            SqlServer.GetNatureAreaSummary("");
        }
    }
}
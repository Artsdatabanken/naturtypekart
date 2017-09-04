using System.Collections.ObjectModel;
using Nin.IO.Excel;
using Nin.Naturtyper;
using Nin.Types.MsSql;
using NUnit.Framework;

namespace Test.Integration.Nin.Common
{
    public class ExcelGeneratorTest
    {
        [Test]
        public void GenerateXlsxStreamTest()
        {
            var excelGenerator = new ExcelGenerator(new Naturetypekodetre());
            var xlsStream = excelGenerator.GenerateXlsxStream(new Collection<NatureAreaExport>());
            Assert.NotNull(xlsStream);
        }
    }
}

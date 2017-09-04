using System;
using System.Collections.ObjectModel;
using Nin;
using Nin.Types.MsSql;
using NUnit.Framework;

namespace Test.Unit.Common
{
    class GmlConverterTest
    {
        private readonly GmlWriter gmlWriter;

        public GmlConverterTest()
        {
            gmlWriter = new GmlWriter();
        }

        [Test]
        public void ConvertToGmlTest()
        {
            var natureArea = new NatureArea
            {
                UniqueId = new Identification {LocalId = Guid.NewGuid(), NameSpace = "NiN", VersionId = "2.0"}
            };

            var natureAreas = new Collection<NatureArea> {natureArea};

            gmlWriter.ConvertToGml(natureAreas);
        }
    }
}

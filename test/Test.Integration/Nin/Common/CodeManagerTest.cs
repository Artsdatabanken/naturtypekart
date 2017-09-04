using Nin.Naturtyper;
using NUnit.Framework;

namespace Test.Integration.Nin.Common
{
    class CodeManagerTest
    {
        [Test]
        public void GetCodeCountTest()
        {
            int codeCount = Naturkodetrær.Naturtyper.Count;
            Assert.IsTrue(codeCount > 1265);
        }

        [Test]
        public void GetNatureLevelCodeTest()
        {
            var codeItem = Naturkodetrær.Naturtyper.HentFraKode("NA");
            var codeItemPath = codeItem.ToString();
            Assert.IsNotEmpty(codeItemPath);
            Assert.NotNull(codeItem);
        }

        [Test]
        public void GetMainTypeGroupCodeTest()
        {
            var codeItem = Naturkodetrær.Naturtyper.HentFraKode("LI FF");
            var codeItemPath = codeItem.ToString();
            Assert.IsNotEmpty(codeItemPath);
            Assert.NotNull(codeItem);
        }

        [Test]
        public void GetMainTypeCodeTest()
        {
            var codeItem = Naturkodetrær.Naturtyper.HentFraKode("LD1");
            var codeItemPath = codeItem.ToString();
            Assert.IsNotEmpty(codeItemPath);
            Assert.NotNull(codeItem);
        }

        [Test]
        public void GetGroundTypeCodeTest()
        {
            var codeItem = Naturkodetrær.Naturtyper.HentFraKode("LA4-3");
            var codeItemPath = codeItem.ToString();
            Assert.IsNotEmpty(codeItemPath);
            Assert.NotNull(codeItem);
        }

        [Test]
        public void GetDescriptionVariableCountTest()
        {
            int descriptionVariableCount = Naturkodetrær.Naturvariasjon.Count;
            Assert.True(descriptionVariableCount > 0);
        }

        [Test]
        public void GetDescriptionVariableTest()
        {
            var descriptionVariableItem = Naturkodetrær.Naturvariasjon.HentFraKode("2BE");
            var descriptionVariableItemPath = descriptionVariableItem.ToString();
            Assert.IsNotEmpty(descriptionVariableItemPath);
            Assert.NotNull(descriptionVariableItem);
        }

        [Test]
        public void GetDescriptionVariableStepTest()
        {
            var descriptionVariableItem = Naturkodetrær.Naturvariasjon.HentFraKode("2BE-2_43");
            var descriptionVariableItemPath = descriptionVariableItem.ToString();
            Assert.IsNotEmpty(descriptionVariableItemPath);
            Assert.NotNull(descriptionVariableItem);
        }
    }
}
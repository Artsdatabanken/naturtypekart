using System;
using Nin.Dataleveranser.Rutenett;
using Nin.IO.RavenDb;
using Nin.Områder;
using Nin.Types.RavenDb;
using NUnit.Framework;
using Types;
using Code = Nin.Types.RavenDb.Code;
using Contact = Nin.Types.RavenDb.Contact;
using Identification = Nin.Types.RavenDb.Identification;
using Metadata = Nin.Types.RavenDb.Metadata;
using NatureArea = Nin.Types.RavenDb.NatureArea;
using Quality = Nin.Types.RavenDb.Quality;

namespace Test.Integration.Nin.DataAccess.RavenDb
{
    internal class RavenDbInterfaceTest
    {
        private readonly NinRavenDb dbInterface;

        public RavenDbInterfaceTest()
        {
            dbInterface = new NinRavenDb();
        }

        [Test]
        public void GetCurrentAndExpiredDataDeliveries()
        {
            var result = dbInterface.HentDataleveranserGjeldendeOgUtgåtte(new Guid("aaaaaaab-bbbb-cccc-dddd-000000000001"));
            Assert.NotNull(result);
        }

        [Test][Ignore("Test used during development.")]
        public void StoreDataDeliveryTest()
        {
            var identification = new Identification
            {
                LocalId = Guid.NewGuid(),
                NameSpace = "DataAccessTest",
                VersionId = "1.0"
            };

            var natureArea = new NatureArea
            {
                UniqueId = identification,
                Version = "2.0",
                Nivå = NatureLevel.Natursystem,
                //Area = SqlGeometry.STPolyFromText(new SqlChars("POLYGON ((5 5, 10 5, 10 10, 5 5))"), 0),
                //Area = SqlGeometry.STPolyFromText(new SqlChars("POLYGON ((-11 55, -10 35, -5.5 36, -1 36, 1 38, 5 38, 11 38, 14 36, 26 33, 29 36, 26 39, 29 46, 39 47, 40 49, 27 56, 27 60, 25 60, 20 58, 21 56, 19 55, 11 55, 10 57, 7 57, 8 54, 3 53, -2 60, -8 58, -11 55))"), 0),
                Area = "POLYGON ((-11 55, -10 35, -5.5 36, -1 36, 1 38, 5 38, 11 38, 14 36, 26 33, 29 36, 26 39, 29 46, 39 47, 40 49, 27 56, 27 60, 25 60, 20 58, 21 56, 19 55, 11 55, 10 57, 7 57, 8 54, 3 53, -2 60, -8 58, -11 55))",
                AreaEpsgCode = 25832,
                Surveyer = new Contact
                {
                    Company = "Norconsult informasjonssystemer AS",
                    ContactPerson = "Magne Tøndel",
                    Email = "magne.tondel@norconsult.com",
                    Phone = "+4748164614",
                    Homesite = "www.nois.no"
                },
                Surveyed = new DateTime(2015, 9, 18, 19, 00, 00),
                Description = "Description"
            };

            natureArea.Documents.Add(new Document
            {
                Title = "NatureAreaDocumentOne",
                Description = "Description",
                Author = new Contact
                {
                    Company = "Norconsult informasjonssystemer AS",
                    ContactPerson = "Magne Tøndel",
                    Email = "magne.tondel@norconsult.com",
                    Phone = "+4748164614",
                    Homesite = "www.nois.no"
                },
                FileName = "C:\\Document\\TestNatureAreaOne"
            });
            natureArea.Documents.Add(new Document
            {
                Title = "NatureAreaDocumentTwo",
                Description = "Description",
                Author = new Contact
                {
                    Company = "Norconsult informasjonssystemer AS",
                    ContactPerson = "Magne Tøndel",
                    Email = "magne.tondel@norconsult.com",
                    Phone = "+4748164614",
                    Homesite = "www.nois.no"
                },
                FileName = "C:\\Document\\TestNatureAreaTwo"
            });

            var metadata = new Metadata
            {
                UniqueId = identification,
                Program = "Program",
                ProjectName = "ProjectName",
                ProjectDescription = "ProjectDescription",

                Quality = new Quality
                {
                    MeasuringMethod = "Terrengmålt: Uspesifisert måleinstrument",
                    Accuracy = 1,
                    Visibility = "Fullt ut synlig/gjenfinnbar i terrenget",
                    MeasuringMethodHeight = "Terrengmålt: Totalstasjon",
                    AccuracyHeight = 2,
                    MaxDeviation = 3,
                },
            };

            metadata.NatureAreas.Add(natureArea);

            metadata.Documents.Add(
                new Document
                {
                    Title = "NatureAreaDocument",
                    Description = "Description",
                    FileName = "C:\\Document\\TestMetadata"
                }
            );

            var dataDelivery = new Dataleveranse
            {
                Name = "DataDeliveryTest",
                DeliveryDate = DateTime.Now,
                Metadata = metadata,
                Created = new DateTime(2015, 9, 17, 12, 30, 30),
                Publisering = Status.Importert
            };

            var id = dbInterface.LagreDataleveranse(dataDelivery);
            Assert.IsNotEmpty(id);
        }

        [Test]
        public void GetImportedDataDeliveriesTest()
        {
            if(DateTime.Today < new DateTime(2017,4,1)) return;
            var importedDataDeliveries = dbInterface.HentDataleveranserImportert();
            Assert.NotNull(importedDataDeliveries);
        }

        [Test][Ignore("Test used during development.")]
        public void StoreGridDeliveryTest()
        {
            var gridDelivery = new GridDelivery
            {
                Name = "GridDeliveryTest",
                Code = new Code { Value = "KA", Registry = "NiN", Version = "1.0" },
                AreaType = AreaType.Kommune,
                Description = "GridTestDescription",
                Established = DateTime.Now,
                Owner = new Contact
                {
                    Company = "Norconsult informasjonssystemer AS",
                    ContactPerson = "Magne Tøndel",
                    Email = "magne.tondel@norconsult.com",
                    Phone = "+4748164614",
                    Homesite = "www.nois.no"
                }
            };

            var id = dbInterface.LagreRutenettleveranse(gridDelivery);
            Assert.IsNotEmpty(id);
        }

        [Test]
        public void GetAllGridDeliveriesTest()
        {
            var result = dbInterface.HentRutenettleveranser();
            Assert.NotNull(result);
        }
    }
}

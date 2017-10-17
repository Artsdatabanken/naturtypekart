using System;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using System.IO;
using System.Xml;
using Microsoft.SqlServer.Types;
using Nin.IO.SqlServer;
using Nin.Types.MsSql;
using NUnit.Framework;
using Types;
using Code = Nin.Types.MsSql.Code;
using Contact = Nin.Types.MsSql.Contact;
using CustomVariable = Nin.Types.MsSql.CustomVariable;
using CustomVariableDefinition = Nin.Types.MsSql.CustomVariableDefinition;
using DescriptionVariable = Nin.Types.MsSql.DescriptionVariable;
using Identification = Nin.Types.MsSql.Identification;
using Metadata = Nin.Types.MsSql.Metadata;
using NatureArea = Nin.Types.MsSql.NatureArea;
using NatureAreaType = Nin.Types.MsSql.NatureAreaType;
using Quality = Nin.Types.MsSql.Quality;
using NinStandardVariabel = Nin.Types.MsSql.NinStandardVariabel;
using System.Collections.Generic;

namespace Test.Integration.Nin.DataAccess.MSSql
{
    public class SqlServerTest
    {
        [Test][Ignore("Database dependent unit test, used only during development.")]
        public void StoreDataDeliveryTest()
        {
            var natureArea = new NatureArea
            {
                Version = "Version",
                Nivå = NatureLevel.Natursystem,
                Surveyed = new DateTime(2015, 9, 18, 19, 00, 00),
                Description = "Description",
                UniqueId = new Identification
                {
                    LocalId = Guid.NewGuid(),
                    NameSpace = "DataAccessTest",
                    VersionId = "1.0"
                },
                Area = SqlGeometry.STPolyFromText(new SqlChars("POLYGON ((-11 55, -10 35, -5.5 36, -1 36, 1 38, 5 38, 11 38, 14 36, 26 33, 29 36, 26 39, 29 46, 39 47, 40 49, 27 56, 27 60, 25 60, 20 58, 21 56, 19 55, 11 55, 10 57, 7 57, 8 54, 3 53, -2 60, -8 58, -11 55))"), 25832),
                Surveyer = new Contact
                {
                    Company = "Norconsult informasjonssystemer AS",
                    ContactPerson = "Magne Tøndel",
                    Email = "magne.tondel@norconsult.com",
                    Phone = "+4748164614",
                    Homesite = "www.nois.no"
                },
            };

            natureArea.Documents.Add(new Document
            {
                Title = "NatureAreaDocumentOne",
                Description = "Description",
                FileName = "C:\\Document\\TestNatureAreaOne",
                Author = new Contact
                {
                    Company = "Norconsult informasjonssystemer AS",
                    ContactPerson = "Magne Tøndel",
                    Email = "magne.tondel@norconsult.com",
                    Phone = "+4748164614",
                    Homesite = "www.nois.no"
                }
            });
            natureArea.Documents.Add(new Document
            {
                Title = "NatureAreaDocumentTwo",
                Description = "Description",
                FileName = "C:\\Document\\TestNatureAreaTwo",
                Author = new Contact
                {
                    Company = "Norconsult informasjonssystemer AS",
                    ContactPerson = "Magne Tøndel",
                    Email = "magne.tondel@norconsult.com",
                    Phone = "+4748164614",
                    Homesite = "www.nois.no"
                }
            });

            natureArea.Parameters.Add(new DescriptionVariable
            {
                Code = "DescriptionVariableCode1",
                Value = "DescriptionVariableValue1",
                Description = "DescriptionVariableDescription1"
            });
            var descriptionVariable = new DescriptionVariable
            {
                Code = "DescriptionVariableCode2",
                Value = "DescriptionVariableValue2",
                Description = "DescriptionVariableDescription2"
            };
            var natureAreaType = new NatureAreaType
            {
                Code = "NatureAreaTypeCode2",
                Share = 1.0
            };
            natureAreaType.AdditionalVariables.Add(descriptionVariable);
            natureAreaType.CustomVariables.Add(new CustomVariable
            {
                Specification = "Specification",
                Value = "Value"
            });

            natureArea.Parameters.Add(natureAreaType);

            var metadata = new Metadata
            {
                Program = "Program",
                ProjectName = "ProjectName",
                ProjectDescription = "ProjectDescription",
                Purpose = "Purpose",
                SurveyedFrom = new DateTime(2015, 1, 1, 1, 0, 0),
                SurveyedTo = new DateTime(2015, 2, 2, 2, 0, 0),
                SurveyScale = "SurveyScale",
                Resolution = "Resolution",
                UniqueId = new Identification
                {
                    LocalId = Guid.NewGuid(),
                    NameSpace = "DataAccessTest",
                    VersionId = "1.0"
                },
                Contractor = new Contact
                {
                    Company = "Norconsult informasjonssystemer AS",
                    ContactPerson = "Magne Tøndel",
                    Email = "magne.tondel@norconsult.com",
                    Phone = "+4748164614",
                    Homesite = "www.nois.no"
                },
                Owner = new Contact
                {
                    Company = "Norconsult informasjonssystemer AS",
                    ContactPerson = "Magne Tøndel",
                    Email = "magne.tondel@norconsult.com",
                    Phone = "+4748164614",
                    Homesite = "www.nois.no"
                },
                Area = SqlGeometry.STPolyFromText(new SqlChars("POLYGON ((-11 55, -10 35, -5.5 36, -1 36, 1 38, 5 38, 11 38, 14 36, 26 33, 29 36, 26 39, 29 46, 39 47, 40 49, 27 56, 27 60, 25 60, 20 58, 21 56, 19 55, 11 55, 10 57, 7 57, 8 54, 3 53, -2 60, -8 58, -11 55))"), 25832),
                Quality = new Quality
                {
                    MeasuringMethod = "Terrengmålt: Uspesifisert måleinstrument",
                    Accuracy = 1,
                    Visibility = "Fullt ut synlig/gjenfinnbar i terrenget",
                    MeasuringMethodHeight = "Terrengmålt: Totalstasjon",
                    AccuracyHeight = 2,
                    MaxDeviation = 3,
                }
            };

            metadata.NatureAreas.Add(natureArea);

            metadata.Documents.Add(new Document
            {
                Title = "NatureAreaDocument",
                Description = "Description",
                FileName = "C:\\Document\\TestMetadata",
                Author = new Contact
                {
                    Company = "Norconsult informasjonssystemer AS",
                    ContactPerson = "Magne Tøndel",
                    Email = "magne.tondel@norconsult.com",
                    Phone = "+4748164614",
                    Homesite = "www.nois.no"
                }
            }
            );

            metadata.VariabelDefinitions.Add(new CustomVariableDefinition
            {
                Specification = "Specification",
                Description = "Description"
            });
            metadata.VariabelDefinitions.Add(new NinStandardVariabel
            {
                VariableDefinition = new Code
                {
                    Registry = "Registry",
                    Version = "Version",
                    Value = "DescriptionVariableCode1"
                }
            });
            metadata.VariabelDefinitions.Add(new NinStandardVariabel
            {
                VariableDefinition = new Code
                {
                    Registry = "Registry",
                    Version = "Version",
                    Value = "DescriptionVariableCode2"
                }
            });
            metadata.VariabelDefinitions.Add(new NinStandardVariabel
            {
                VariableDefinition = new Code
                {
                    Registry = "Registry",
                    Version = "Version",
                    Value = "NatureAreaTypeCode2"
                }
            });

            var dataDelivery = new Dataleveranse
            {
                Id = "DataDeliveries/100",
                Name = "Name",
                DeliveryDate = new DateTime(2015, 1, 1, 13, 00, 00),
                ReasonForChange = "Reason for change",
                Description = "Description",
                ParentId = null,
                Created = DateTime.Now,
                Expired = null,
                Publisering = Status.Gjeldende,
                Operator = new Contact
                {
                    Company = "Norconsult informasjonssystemer AS",
                    ContactPerson = "Magne Tøndel",
                    Email = "magne.tondel@norconsult.com",
                    Phone = "+4748164614",
                    Homesite = "www.nois.no"
                },
                Metadata = metadata
            };

            SqlServer.LagreDataleveranse(dataDelivery);
        }

        [Test][Ignore("Database dependent unit test, used only during development.")]
        public void GetMetadatasByNatureAreaLocalIdsTest()
        {
            var localIds = new List<string>
            {
                "aaaaaaaa-bbbb-cccc-dddd-100000000001",
                "aaaaaaaa-bbbb-cccc-dddd-100000000002"
            };

            SqlServer.GetMetadatasByNatureAreaLocalIds(localIds, true);
        }

        [Test][Ignore("Database dependent unit test, used only during development.")]
        public void DeleteDataDeliveryTest()
        {
            SqlServer.DeleteDataDelivery(100);
        }

        [Test][Ignore("Database dependent unit test, used only during development.")]
        public void GetNatureAreaSummaryTest()
        {
            SqlServer.GetNatureAreaSummary("");
        }

        [Test][Ignore("Database dependent unit test, used only during development.")]
        public void GetNatureAreasBySearchFilterTest()
        {
            int natureAreaCount;
            SqlServer.GetNatureAreasBySearchFilter(
                new Collection<NatureLevel>(),
                new Collection<string>(),
                new Collection<string>(),
                new Collection<int>(),
                new Collection<int>(),
                new Collection<int>(),
                new Collection<string>(),
                "",
                "",
                0,
                false,
                0,
                0,
                int.MaxValue,
                out natureAreaCount
            );
        }

        [Test][Ignore("Database dependent unit test, used only during development.")]
        public void CreateGmlTestData()
        {
            using (StreamWriter writer = new StreamWriter(@"C:\Artsdatabanken\gml_landskap\polygons.gml"))
            using (StreamReader reader = new StreamReader(@"C:\Artsdatabanken\gml_landskap\gml_landskap.gml"))
            {
                string polygon;
                while ((polygon = reader.ReadLine()) != null)
                {
                    if (!polygon.Contains("MultiSurface")) continue;
                    int firstId = polygon.IndexOf(" gml:id", 100);
                    int lastId = polygon.IndexOf("\"", firstId + 10);

                    string start = polygon.Substring(0, firstId);
                    string end = polygon.Substring(lastId + 1);
                    polygon = start + end;

                    int firstPolygon = polygon.IndexOf("<gml:Polygon");
                    int lastPolygon = polygon.IndexOf("</gml:Polygon>");

                    polygon = polygon.Substring(firstPolygon, lastPolygon - firstPolygon + 14);
                    polygon = polygon.Replace("<gml:Polygon>", "<gml:Polygon xmlns:gml='http://www.opengis.net/gml'>");

                    writer.WriteLine(polygon);
                }
            }
        }

        [Test][Ignore("Database dependent unit test, used only during development.")]
        public void CreatePerformanceTestData()
        {
            var dataDelivery = new Dataleveranse
            {
                Id = "DataDeliveries/0",
                Name = "DataleveranseTestYtelse",
                DeliveryDate = DateTime.Now,
                Operator = new Contact
                {
                    Company = "Norconsult informasjonssystemer AS",
                    ContactPerson = "Magne Tøndel",
                    Email = "magne.tondel@norconsult.com",
                    Phone = "+4748164614",
                    Homesite = "www.nois.no"
                },
                Created = DateTime.Now
            };

            var metadata = new Metadata
            {
                UniqueId = new Identification
                {
                    LocalId = Guid.NewGuid(),
                    NameSpace = "DataAccessTest",
                    VersionId = "1.0"
                },
                Program = "Test",
                ProjectName = "Naturtyper i Norge",
                ProjectDescription = "Kartlegging av naturtyper i Norge",
                Purpose = "Kartlegging og dokumentasjon",
                Contractor = new Contact
                {
                    Company = "Norconsult informasjonssystemer AS",
                    ContactPerson = "Magne Tøndel",
                    Email = "magne.tondel@norconsult.com",
                    Phone = "+4748164614",
                    Homesite = "www.nois.no"
                },
                Owner = new Contact
                {
                    Company = "Norconsult informasjonssystemer AS",
                    ContactPerson = "Magne Tøndel",
                    Email = "magne.tondel@norconsult.com",
                    Phone = "+4748164614",
                    Homesite = "www.nois.no"
                },
                SurveyedFrom = DateTime.Now,
                SurveyedTo = DateTime.Now,
                SurveyScale = "1:50000"
            };
            const int sourceEpsgCode = 32633;
            metadata.Area = SqlGeometry.STPolyFromText(new SqlChars("POLYGON EMPTY"), sourceEpsgCode);
            metadata.Quality = new Quality
            {
                MeasuringMethod = "Terrengmålt: Uspesifisert måleinstrument",
                Accuracy = 1,
                Visibility = "Fullt ut synlig/gjenfinnbar i terrenget",
                MeasuringMethodHeight = "Terrengmålt: Totalstasjon",
                AccuracyHeight = 2,
                MaxDeviation = 3,
            };

            for (int i = 1; i < 33; ++i)
            {
                metadata.VariabelDefinitions.Add(new NinStandardVariabel
                {
                    VariableDefinition = new Code
                    {
                        Registry = "NiN",
                        Version = "2.0",
                        Value = "NA V1-" + i
                    }
                });
            }

            dataDelivery.Metadata = metadata;

            using (var reader = new StreamReader(@"C:\Artsdatabanken\gml_landskap\polygons.gml"))
            {
                Random random = new Random();

                string polygon;
                while ((polygon = reader.ReadLine()) != null)
                {
                    var xmlTextReader = new XmlTextReader(polygon, XmlNodeType.Document, null);
                    SqlXml sqlXml = new SqlXml(xmlTextReader);
                    SqlGeometry sqlGeometry = SqlGeometry.GeomFromGml(sqlXml, sourceEpsgCode);

                    var natureArea = new NatureArea
                    {
                        UniqueId = new Identification
                        {
                            LocalId = Guid.NewGuid(),
                            NameSpace = "DataAccessTest",
                            VersionId = "1.0"
                        },
                        Version = "Test versjon",
                        Nivå = NatureLevel.Natursystem,
                        Area = sqlGeometry,
                        Institution = "Tøndel Consulting"
                    };

                    int randomCode = random.Next(1, 33);
                    natureArea.Parameters.Add(new NatureAreaType
                    {
                        Code = "NA V1-" + randomCode,
                        Share = 1
                    });

                    natureArea.Surveyed = new DateTime(random.Next(2000, 2016), 1, 1);

                    metadata.NatureAreas.Add(natureArea);
                }
            }

            SqlServer.LagreDataleveranse(dataDelivery);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Types;
using Nin.IO.Xml;
using NUnit.Framework;
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

namespace Test.Unit.Common
{
    class XmlConverterTest
    {
        [Test]
        public void ConvertToXmlTest()
        {
            var metadatas = new Collection<Metadata>();

            var identification = new Identification
            {
                LocalId = Guid.NewGuid(),
                NameSpace = "DataAccessTest",
                VersionId = "1.0"
            };

            var descriptionVariable1 = new DescriptionVariable
            {
                Code = "descCode1",
                Description = "descDescription1",
                Surveyed = new DateTime(2015, 9, 18, 19, 00, 00),
                Surveyer = new Contact
                {
                    Company = "Norconsult informasjonssystemer AS",
                    ContactPerson = "Magne Tøndel",
                    Email = "magne.tondel@norconsult.com",
                    Phone = "+4748164614",
                    Homesite = "www.nois.no"
                },
                Value = "descValue1"
            };

            var customVariableDefinition = new CustomVariableDefinition
            {
                Description = "customDescription",
                Specification = "customSpecification"
            };

            var standardVariable = new NinStandardVariabel
            {
                VariableDefinition = new Code
                {
                    Registry = "stdRegistry",
                    Version = "stdVersion",
                    Value = "stdCode"
                }
            };

            var descriptionVariable2 = new DescriptionVariable
            {
                Code = "descCode2",
                Description = "descDescription2",
                Surveyed = new DateTime(2015, 9, 18, 19, 00, 00),
                Surveyer = new Contact
                {
                    Company = "Norconsult informasjonssystemer AS",
                    ContactPerson = "Magne Tøndel",
                    Email = "magne.tondel@norconsult.com",
                    Phone = "+4748164614",
                    Homesite = "www.nois.no"
                },
                Value = "descValue2"
            };

            var customVariable = new CustomVariable
            {
                Specification = "customSpecification",
                Value = "customValue"
            };

            var natureAreaType = new NatureAreaType
            {
                Code = "naCode2",
                AdditionalVariables = new Collection<DescriptionVariable>(),
                CustomVariables = new Collection<CustomVariable>(),
                Share = 0.5,
            };

            natureAreaType.AdditionalVariables.Add(descriptionVariable2);
            natureAreaType.CustomVariables.Add(customVariable);

            var natureArea = new NatureArea
            {
                UniqueId = identification,
                Version = "2.0",
                Nivå = NatureLevel.Natursystem,
                Area = SqlGeometry.STGeomFromText(new SqlChars("POLYGON ((-11 55, -10 35, -5.5 36, -1 36, 1 38, 5 38, 11 38, 14 36, 26 33, 29 36, 26 39, 29 46, 39 47, 40 49, 27 56, 27 60, 25 60, 20 58, 21 56, 19 55, 11 55, 10 57, 7 57, 8 54, 3 53, -2 60, -8 58, -11 55))"), 25832),
                Surveyer = new Contact
                {
                    Company = "Norconsult informasjonssystemer AS",
                    ContactPerson = "Magne Tøndel",
                    Email = "magne.tondel@norconsult.com",
                    Phone = "+4748164614",
                    Homesite = "www.nois.no"
                },
                Surveyed = new DateTime(2015, 9, 18, 19, 00, 00),
                Description = "Description",
                Parameters = new List<Parameter>()
            };

            natureArea.Parameters.Add(descriptionVariable1);
            natureArea.Parameters.Add(natureAreaType);

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

            var metadataIdentification = new Identification
            {
                LocalId = Guid.NewGuid(),
                NameSpace = "DataAccessTest",
                VersionId = "1.0"
            };

            var metadata = new Metadata
            {
                UniqueId = metadataIdentification,
                Program = "Program",
                ProjectName = "ProjectName",
                ProjectDescription = "ProjectDescription",
                Purpose = "Purpose",
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
                SurveyScale = "SurveyScale",
                Resolution = "Resolution",
                Area = SqlGeometry.STGeomFromText(new SqlChars("POLYGON ((-11 55, -10 35, -5.5 36, -1 36, 1 38, 5 38, 11 38, 14 36, 26 33, 29 36, 26 39, 29 46, 39 47, 40 49, 27 56, 27 60, 25 60, 20 58, 21 56, 19 55, 11 55, 10 57, 7 57, 8 54, 3 53, -2 60, -8 58, -11 55))"), 25832),
                Quality = new Quality
                {
                    MeasuringMethod = "10",
                    Accuracy = 1,
                    Visibility = "0",
                    MeasuringMethodHeight = "10",
                    AccuracyHeight = 6,
                    MaxDeviation = 7,
                },
            };

            metadata.VariabelDefinitions.Add(customVariableDefinition);
            metadata.VariabelDefinitions.Add(standardVariable);

            metadata.NatureAreas.Add(natureArea);

            metadata.Documents.Add(
                new Document
                {
                    Title = "NatureAreaDocument",
                    Description = "Description",
                    FileName = "NatureAreaDocument.jpg"
                }
            );

            metadatas.Add(metadata);

            new XmlConverter().ToXml(metadatas);
        }
    }
}

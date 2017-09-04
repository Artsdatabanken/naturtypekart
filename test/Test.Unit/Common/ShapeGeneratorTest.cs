using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using Common;
using Microsoft.SqlServer.Types;
using NUnit.Framework;
using Types;
using Contact = Nin.Types.MsSql.Contact;
using CustomVariable = Nin.Types.MsSql.CustomVariable;
using DescriptionVariable = Nin.Types.MsSql.DescriptionVariable;
using Document = Nin.Types.MsSql.Document;
using Identification = Nin.Types.MsSql.Identification;
using NatureArea = Nin.Types.MsSql.NatureArea;
using NatureAreaType = Nin.Types.MsSql.NatureAreaType;

namespace Test.Unit.Common
{
    class ShapeGeneratorTest
    {
        [Test]
        public void GenerateShapeFileTest()
        {
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
                AdditionalVariables = new Collection<DescriptionVariable> {descriptionVariable1, descriptionVariable2},
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

            var memoryStream = ShapeGenerator.GenerateShapeFile(new Collection<NatureArea> {natureArea}, 25832);
            Assert.True(memoryStream.CanRead);
        }
    }
}

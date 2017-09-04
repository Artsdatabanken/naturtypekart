using System;
using Common.Diagnostic.Network.Web;
using Newtonsoft.Json;
using Nin.Test.Smoke;
using NUnit.Framework;

namespace Test.Smoke
{
    public class DataDeliveryControllerTest
    {
        private readonly string baseUrl;

        public DataDeliveryControllerTest()
        {
            var host = "it-webadbtest01.it.ntnu.no";
            host = Environment.GetEnvironmentVariable("TESTHOST") ?? host;
            baseUrl = $"http://{host}/NinDocument/DataDelivery";
        }

        [Test]
        public void Authenticate()
        {
            //baseUrl = "http://localhost:5000/NbicDocumentStoreApi/api/DataDelivery";

            var post = new FormPost($"{baseUrl}/Authenticate");
            post.Add("username", "");
            post.Add("password", "");

            post.Execute();
        }

        [Test]
        public void DownloadDocument()
        {
            Http.Get($"{baseUrl}/DownloadDocument/1");
        }

        [Test]
        public void GetAllGridDeliveries()
        {
            Http.Get($"{baseUrl}/GetAllGridDeliveries");
        }

        [Test]
        public void GetListOfDataDeliveries()
        {
            Http.Get($"{baseUrl}/GetListOfDataDeliveries?username=maton");
        }

        [Test]
        public void GetListOfImportedDataDeliveries()
        {
            string json = Http.Get($"{baseUrl}/GetListOfImportedDataDeliveries");
            //dynamic deliveries = JsonConvert.DeserializeObject(json);
        }

        [Test]
        public void Index()
        {
            Http.Get($"{baseUrl}");
        }

        [Test]
        public void UploadDataDelivery()
        {
            var post = new FormPost($"{baseUrl}/UploadDataDelivery");
            post.Add("username", "");
            post.Add("password", "");
            post.AddFile("metadata", TestSetup.GetDataPath(@"NatureArea\NiNCoreDataleveranseTest1_ver2.xml"));
            post.AddFile("files0", TestSetup.GetDataPath(@"NatureArea\skog.png"));
            post.AddFile("files1", TestSetup.GetDataPath(@"NatureArea\dokument.pdf"));
            post.AddFile("files2", TestSetup.GetDataPath(@"NatureArea\smurfine.jpg"));
            post.AddFile("files3", TestSetup.GetDataPath(@"NatureArea\dokument.docx"));

            post.Execute();
        }

        [Test]
        public void VerifyDataDelivery()
        {
            string json = Http.Get($"{baseUrl}/GetListOfImportedDataDeliveries");
            dynamic deliveries = JsonConvert.DeserializeObject(json);
            string id = deliveries[0].Id;

            var post = new FormPost($"{baseUrl}/VerifyDataDelivery");
            post.Add("username", "");
            post.Add("password", "");
            var id2 = id.Replace("datadeliveries/", "");
            post.Add("id", id2);

            post.Execute();
        }

        [Test][Ignore("Gjør om til unit test")]
        public void UploadDataDelivery_Negativ_GjørOmTilUnitTest()
        {
            try
            {
                //baseUrl = "http://localhost:5000/NbicDocumentStoreApi/api/DataDelivery";
                var post = new FormPost($"{baseUrl}/UploadDataDelivery");
                post.Add("username", "");
                post.Add("password", "");
                //post.AddFile("metadata", new FileInfo(@"..\Nim.Test.Integration\Data\NiNCoreImportMinExample.xml"));
                post.AddFile("metadata", TestSetup.GetDataPath(@"NiNCoreImportMinExample.xml"));
                post.Execute();
            }
            catch (Exception caught)
            {
                if (caught.Message.Contains("ligger utenfor Norge")) return;
                throw;
            }
            throw new Exception("Skal avvises fordi området ligger utenfor Norge");
        }

        [Test]
        public void UploadGrid()
        {
            //baseUrl = "http://localhost:5000/NbicDocumentStoreApi/api/DataDelivery";
            //Http.PostFile($"{baseUrl}/UploadGrid", new FileInfo("TestFiles\\RuteNettKartLandbruk.xml"), "form-data");

            var post = new FormPost($"{baseUrl}/UploadGrid");
            post.Add("username", "");
            post.Add("password", "");
            post.AddFile("grid", TestSetup.GetDataPath(@"AreaLayer\AdministrativtOmraadeKartTest.xml"));
            post.Execute();
        }

        [Test]
        public void UploadGridDelivery()
        {
            var post = new FormPost($"{baseUrl}/UploadGridDelivery");
            post.Add("username", "");
            post.Add("password", "");
            post.Add("navn", "navn");
            post.Add("beskrivelse", "beskrivelse");
            post.Add("kode", "kode");
            post.Add("koderegister", "koderegister");
            post.Add("kodeversjon", "kodeversjon");
            post.Add("firmanavn", "firma");
            post.Add("kontaktperson", "kontaktperson");
            post.Add("ownerEmail", "email@domain.xx");
            post.Add("telefon", "123456");
            post.Add("hjemmeside", "http://artsdatabanken.no");
            post.Add("etablertDato", "2016-01-31");
            post.Add("dokumentBeskrivelse", "dokumentbeskrivelse");
            post.Add("kartType", "RuteNett");
            post.Add("ssbType", "2");
            post.Add("aoType", "undefined");
            post.AddFile("files0", "RuteNettKartTest.xml", "text/xml", @"<?xml version=""1.0"" encoding=""UTF-8""?>");
            post.Execute();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Common.Session;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nin.Aspnet;
using Nin.Dataleveranser;
using Nin.Dataleveranser.Import;
using Nin.Dataleveranser.Rutenett;
using Nin.Diagnostic;
using Nin.IO.RavenDb;
using Nin.IO.RavenDb.Transformers;
using Nin.IO.SqlServer;
using Nin.Områder;
using Nin.Rutenett;
using Nin.Session;
using Nin.Types.RavenDb;
using File = Nin.Types.RavenDb.File;

namespace Api.Document.Controllers
{
    public class DataDeliveryController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var contentResult = new NinHtmlResult("Document.Api is running!");
            Log.v("AUTH", contentResult.Content);
            return contentResult;
        }

        [HttpPost]
        public object Authenticate()
        {
            var username = RequestForm("username");
            var password = RequestForm("password");
            Log.v("AUTH", "Authenticate '" + username + "'.");

            return userDb.Authenticate(username, password);
        }

        [HttpPost]
        public async Task<string> UploadDataDelivery()
        {
            Authorize("Dataleverandør");

            var username = RequestForm("username");

            var metadata1 = Request.Form.Files.GetFile("metadata");
            IDataFile metadata = new HttpFormFile(metadata1);
            var files = HttpFormFile.GetFormFiles(Request.Form.Files);

            await arkiv.LastOppDataleveranse(dataleveranseXmlGreier, metadata, files, username);

            return "OK";
        }

        [HttpPost]
        public async Task<string> UploadGrid()
        {
            Authorize("Dataleverandør");

            IFormFile grid2 = Request.Form.Files.GetFile("grid");
            HttpFormFile grid = new HttpFormFile(grid2);
            DataFiles files = HttpFormFile.GetFormFiles(Request.Form.Files);

            var gridXml = grid.ReadXml();
            dataleveranseXmlGreier.ValidateGrid(gridXml);
            try
            {
                var gridFileDocId = Guid.NewGuid();
                var gridFile = new File
                {
                    Id = gridFileDocId.ToString(),
                    FileName = grid.Filename,
                    ContentType = grid.ContentType,
                    Content = grid.OpenReadStream()
                };
                await arkiv.LagreFil(gridFile);

                Collection<Nin.Types.MsSql.Document> documents;
                if (gridXml.Root.Name.LocalName.Equals("AdministrativtOmraadeKart"))
                {
                    AreaLayer areaLayer = AreaLayerImpl.FromXml(gridXml);
                    files.CheckDocuments(areaLayer.Documents);
                    areaLayer.DocGuid = gridFileDocId;
                    documents = areaLayer.Documents;
                    SqlServer.BulkStoreAreaLayer(areaLayer);
                }
                else
                {
                    var gridLayer = GridLayerImpl.FromXml(gridXml);

                    files.CheckDocuments(gridLayer.Documents);
                    gridLayer.DocGuid = gridFileDocId;
                    documents = gridLayer.Documents;
                    SqlServer.BulkStoreGridLayer(gridLayer);
                }

                foreach (var file in files.Values)
                {
                    var fileName = file.Filename;
                    var document = FindDocument(documents, fileName);

                    var docFile = new File
                    {
                        Id = document.Guid.ToString(),
                        FileName = fileName,
                        ContentType = file.ContentType,
                        Content = file.OpenReadStream()
                    };
                    await arkiv.LagreFil(docFile);
                }
            }
            catch
            {
                arkiv.DiscardChanges();
                throw;
            }

            return "OK";
        }

        [HttpPost]
        public async Task<string> UploadGridDelivery()
        {
            Authorize("Dataleverandør");

            var mapType = RequestForm("kartType");
            var files = HttpFormFile.GetFormFiles(Request.Form.Files);

            var delivery = new GridDelivery
            {
                Name = RequestForm("navn"),
                Description = RequestForm("beskrivelse"),
                Code = new Code
                {
                    Value = RequestForm("kode"),
                    Registry = RequestForm("koderegister"),
                    Version = RequestForm("kodeversjon")
                },
                Owner = new Contact
                {
                    Company = RequestForm("firmanavn"),
                    ContactPerson = RequestForm("kontaktperson"),
                    Email = RequestForm("ownerEmail"),
                    Homesite = RequestForm("hjemmeside"),
                    Phone = RequestForm("telefon")
                },
                Established = DateTime.Parse(RequestForm("etablertDato"))
            };

            delivery.MapGridTypeNumber(RequestForm("ssbType"));
            delivery.MapAreaTypeNumber(RequestForm("aoType"));
            delivery.DocumentDescription = RequestForm("dokumentBeskrivelse");
            delivery.Username = RequestForm("username");

            try
            {
                foreach (var file in files.Values)
                {
                    var document = new Nin.Types.RavenDb.Document {FileName = file.Filename};
                    delivery.Documents.Add(document);

                    var docFile = new File
                    {
                        Id = document.Guid.ToString(),
                        FileName = document.FileName,
                        ContentType = file.ContentType,
                        Content = file.OpenReadStream()
                    };
                    await arkiv.LagreFil(docFile);
                }

                arkiv.LagreRutenettleveranse(delivery);
                arkiv.SaveChanges();
            }
            catch
            {
                arkiv.DiscardChanges();
                throw;
            }

            return "OK";
        }

        [HttpGet]
        public void DeleteGridDelivery(int id)
        {
            var gridDelivery = arkiv.HentRutenettleveranse("GridDeliveries/" + id);

            if (gridDelivery == null) return;
            arkiv.Slett("GridDeliveries/" + id);
            arkiv.SaveChanges();

            foreach (var document in gridDelivery.Documents)
                arkiv.SlettFil(document.Guid.ToString());
        }

        [HttpGet]
        public object GetAllGridDeliveries()
        {
            return arkiv.HentRutenettleveranser();
        }

        [HttpGet]
        public object DownloadDataDelivery(int id)
        {
            return arkiv.HentDataleveranse("DataDeliveries/" + id);
        }

        [HttpGet]
        public object GetListOfImportedDataDeliveries()
        {
            return arkiv.HentDataleveranselisteImportert();
        }

        [HttpGet]
        public object GetImportedDataDeliveries()
        {
            return arkiv.HentDataleveranserImportert();
        }

        [HttpGet]
        public List<DataleveranseListItem> GetListOfDataDeliveries(string username)
        {
            return arkiv.HentDataleveranserForBruker(username);
        }

        [HttpGet]
        public object GetDataDeliveries(string username)
        {
            return arkiv.HentDataleveranser(username);
        }

        [HttpPost]
        public object PubliserDataleveranse()
        {
            Authorize("Administrator");
            //TODO: Åpne porter for DTC mot SQL?
            //using (var transaction = new TransactionScope())
            {
                var id = RequestForm("id");
                DataleveransePubliserer.PubliserLeveranse(id, arkiv, userDb);
//                transaction.Complete();
            }
            return "{}";
        }

        [HttpGet]
        public async Task<FileResult> DownloadDocument(string id)
        {
            var docfile = await arkiv.HentFil(id);

            FileResult fileResult;

            if (docfile.ContentType.Equals("text/xml"))
            {
                var reader = new StreamReader(docfile.Content, Encoding.GetEncoding("iso-8859-1"));
                var xml = reader.ReadToEnd();

                Stream xmlStream = new MemoryStream();
                var writer = new StreamWriter(xmlStream);
                writer.Write(xml);
                writer.Flush();
                xmlStream.Position = 0;

                fileResult = new FileStreamResult(xmlStream, docfile.ContentType);
            }
            else
            {
                fileResult = new FileStreamResult(docfile.Content, docfile.ContentType);
            }
            return fileResult;
        }

        private static Nin.Types.MsSql.Document FindDocument(IEnumerable<Nin.Types.MsSql.Document> documents,
            string fileName)
        {
            Nin.Types.MsSql.Document doc = null;

            foreach (var document in documents)
            {
                if (!document.FileName.Equals(fileName)) continue;
                if (doc != null)
                    throw new DataDeliveryParseException("Found duplicates references of the file: " + fileName);
                doc = document;
            }

            if (doc == null)
                throw new DataDeliveryParseException("Could not find the document reference to the file: " +
                                                     fileName);
            return doc;
        }

        private string RequestForm(string key, string defaultValue = null)
        {
            if (Request.Form.ContainsKey(key)) return Request.Form[key];

            if (defaultValue == null)
                throw new Exception("Mangler skjemafelt '" + key + "'.");

            return defaultValue;
        }

        [HttpGet]
        private void Authorize(string accessLevel)
        {
            var username = RequestForm("username");
            var password = RequestForm("password");

            userDb.AssertHasRole(accessLevel, username, password);
        }

        public DataDeliveryController()
        {
            dataleveranseXmlGreier = new DataleveranseXmlGreier();
            arkiv = new NinRavenDb();
            userDb = new ArtsdatabankenUserDatabase();
        }

        private readonly DataleveranseXmlGreier dataleveranseXmlGreier;
        private readonly NinRavenDb arkiv;
        readonly IUserDatabase userDb;
    }
}
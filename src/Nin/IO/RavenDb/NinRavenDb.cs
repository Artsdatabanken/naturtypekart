using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nin.Configuration;
using Nin.Dataleveranser;
using Nin.Dataleveranser.Import;
using Nin.Dataleveranser.Rutenett;
using Nin.Diagnostic;
using Nin.IO.RavenDb.Transformers;
using Nin.Types.RavenDb;
using Raven.Abstractions.Extensions;
using Raven.Json.Linq;
using Types;
using File = Nin.Types.RavenDb.File;

namespace Nin.IO.RavenDb
{
    public class NinRavenDb
    {
        private const string dataleveranses = "Dataleveranses";
        private readonly IRavenDbStore ravenDbStore;
        private readonly IRavenFilesStore ravenFilesStore;

        private bool transformerInitialized;

        public NinRavenDb()
        {
            var settings = Config.Settings.ExternalDependency.DocumentArchive;
            Log.v("RAVE", "Raven DbUrl: " + settings.DbUrl);
            Log.v("RAVE", "Raven FsUrl: " + settings.FsUrl);
            ravenDbStore = new RavenDbStore(settings.DbUrl, settings.DbName);
            ravenFilesStore = new RavenFilesStore(settings.FsUrl, settings.FsName);
        }

        public string LagreDataleveranse(Dataleveranse dataleveranse)
        {
            ravenDbStore.Session.Store(dataleveranse);
            var dataDeliveryId = dataleveranse.Id; // Id is set by database
            //_ravenDbStore.Session.SaveChanges();
            return dataDeliveryId;
        }

        public Dataleveranse HentDataleveranse(string id)
        {
            var path = dataleveranses + "/" + id;
            var dataDelivery = ravenDbStore.Session.Load<object>(path);
            if (dataDelivery == null)
                throw new IOException($"Finner ikke dokument \'{path}\'.");
            return (Dataleveranse)dataDelivery;
        }

        public List<DataleveranseListItem> HentDataleveranserForBruker(string username)
        {
            InitTransformer();
            var DataleveranseListItems = ravenDbStore.Session.Query<Dataleveranse>()
                .TransformWith<DataleveranseListItemTransformer, DataleveranseListItem>()
                .Where(dataDelivery =>
                    dataDelivery.Username.Equals(username)
                )
                .ToList();

            return DataleveranseListItems;
        }

        private void InitTransformer()
        {
            // WTF?
            if (transformerInitialized) return;
            new DataleveranseListItemTransformer().Execute(ravenDbStore.Store);
            transformerInitialized = true;
        }

        public List<Dataleveranse> HentDataleveranser(string username)
        {
            return ravenDbStore.Session.Query<Dataleveranse>()
                .Where(
                    dataDelivery =>
                        dataDelivery.Username.Equals(username)
                )
                .ToList();
        }

        public List<DataleveranseListItem> HentDataleveranselisteImportert()
        {
            var DataleveranseListItems = ravenDbStore.Session.Query<Dataleveranse>()
                .TransformWith<DataleveranseListItemTransformer, DataleveranseListItem>()
                .Where(dataDelivery =>
                    dataDelivery.Status == Status.Importert
                )
                .ToList();

            return DataleveranseListItems;
        }

        public List<Dataleveranse> HentDataleveranserImportert()
        {
            return ravenDbStore.Session.Query<Dataleveranse>()
                .Where(
                    dataDelivery =>
                        dataDelivery.Publisering == Status.Importert
                )
                .ToList();
        }

        public List<Dataleveranse> HentDataleveranser(Guid localId)
        {
            var dataDeliveries = ravenDbStore.Session.Query<Dataleveranse>();
            return dataDeliveries.Where(
                    dataDelivery =>
                        dataDelivery.Metadata.UniqueId.LocalId == localId
                )
                .ToList();
        }

        public Dataleveranse HentDataleveranseImportert(Guid localId)
        {
            var result = ravenDbStore.Session.Query<Dataleveranse>()
                .Where(
                    dataDelivery =>
                        dataDelivery.Metadata.UniqueId.LocalId == localId &&
                        dataDelivery.Publisering == Status.Importert
                )
                .ToList();

            if (result.Count > 1)
                throw new DataDeliveryStoreException(
                    "Found more than one imported data deliveries with Metadata.LocalId like" + localId);
            return result.Count == 0 ? null : result[0];
        }

        public Dataleveranse FinnDataleveranse(Guid localId)
        {
            var result = ravenDbStore.Session.Query<Dataleveranse>()
                .Where(
                    dataDelivery =>
                        dataDelivery.Metadata.UniqueId.LocalId == localId &&
                        dataDelivery.Publisering == Status.Gjeldende
                )
                .ToList();

            if (result.Count > 1)
                throw new DataDeliveryStoreException(
                    "Found more than one current data deliveries with Metadata.LocalId like" + localId);
            if (result.Count == 0)
                return null;

            return result[0];
        }

        public List<Dataleveranse> HentDataleveranserGjeldendeOgUtgåtte(Guid localId)
        {
            return ravenDbStore.Session.Query<Dataleveranse>()
                .Customize(dataDelivery => dataDelivery.NoTracking())
                .Where(
                    dataDelivery =>
                        dataDelivery.Metadata.UniqueId.LocalId == localId &&
                        (dataDelivery.Publisering == Status.Gjeldende || dataDelivery.Publisering == Status.Utgått)
                )
                .OrderByDescending(dataDelivery => dataDelivery.Expired)
                .ToList();
        }

        public string LagreRutenettleveranse(GridDelivery gridDelivery)
        {
            ravenDbStore.Session.Store(gridDelivery);
            var gridDeliveryId = gridDelivery.Id; // Id is set by database
            //_ravenDbStore.Session.SaveChanges();
            return gridDeliveryId;
        }

        public GridDelivery HentRutenettleveranse(string id)
        {
            return ravenDbStore.Session.Load<GridDelivery>(id);
        }

        public void Slett(string id)
        {
            ravenDbStore.Session.Delete(id);
        }

        public List<GridDelivery> HentRutenettleveranser()
        {
            return ravenDbStore.Session.Query<GridDelivery>().ToList();
        }

        public async Task<string> LagreFil(File file)
        {
            var metadata = new RavenJObject
            {
                {"filename", file.FileName},
                {"contentType", file.ContentType}
            };

            ravenFilesStore.Session.RegisterUpload("/files/" + file.Id, file.Content, metadata);

            await ravenFilesStore.Session.SaveChangesAsync();

            return file.Id;
        }

        public async Task<File> HentFil(string id)
        {
            var metadata = new Reference<RavenJObject>();

            var file = new File
            {
                Content = await ravenFilesStore.Session.DownloadAsync("/files/" + id, metadata),
                FileName = metadata.Value.Value<string>("filename"),
                ContentType = metadata.Value.Value<string>("contentType")
            };


            return file;
        }

        public async void SlettFil(string id)
        {
            ravenFilesStore.Session.RegisterFileDeletion("/files/" + id);
            await ravenFilesStore.Session.SaveChangesAsync();
        }

        public void SaveChanges()
        {
            ravenDbStore.Session.SaveChanges();
        }

        public void DiscardChanges()
        {
            ravenDbStore.Dispose();
        }

        private void OvertaForForrigeVersjon(Dataleveranse dataleveranse)
        {
            var importedDataDelivery =
                HentDataleveranseImportert(dataleveranse.Metadata.UniqueId.LocalId);
            if (importedDataDelivery == null) return;

            importedDataDelivery.Expired = DateTime.Now;
            importedDataDelivery.Publisering = Status.Utgått;
            LagreDataleveranse(importedDataDelivery);
            dataleveranse.ParentId = importedDataDelivery.Id;
        }

        private void ImporterDataleveranse(Dataleveranse dataleveranse, string username)
        {
            dataleveranse.Created = DateTime.Now;
            dataleveranse.Publisering = Status.Importert;
            dataleveranse.Username = username;
            LagreDataleveranse(dataleveranse);
        }

        public async Task LastOppDataleveranse(DataleveranseXmlGreier dataDeliveryCore, IDataFile metadata, DataFiles files,
            string username)
        {
            if (metadata == null)
                throw new Exception("Kan ikke lagre en tom leveranse.");

            var dataDeliveryXml = metadata.ReadXml();
            dataDeliveryCore.ValidateDataDelivery(dataDeliveryXml);
            var dataleveranse = DataleveranseXmlGreier.ParseDataDelivery(dataDeliveryXml);
            files.CheckDocuments(dataleveranse);
            DataleveranseXmlGreier.ValidateDataDeliveryContent(dataleveranse);

            OvertaForForrigeVersjon(dataleveranse);
            ImporterDataleveranse(dataleveranse, username);

            var metadataFile = new File
            {
                Id = dataleveranse.Id.Replace("DataDeliveries/", ""),
                FileName = metadata.Filename,
                ContentType = metadata.ContentType,
                Content = metadata.OpenReadStream()
            };
            await LagreFil(metadataFile);

            foreach (var file in files.Values)
            {
                var fileName = file.Filename;
                var document = dataleveranse.Metadata.FindDocument(fileName);
                if (document == null)
                    throw new DataDeliveryParseException("Finner ingen referanse i datafilen til vedlegg '" + fileName +
                                                         "'.");

                var docFile = new File
                {
                    Id = document.Guid.ToString(),
                    FileName = fileName,
                    ContentType = file.ContentType,
                    Content = file.OpenReadStream()
                };
                await LagreFil(docFile);
            }

            SaveChanges();
        }
    }
}
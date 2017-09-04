using System.Collections.Generic;
using System.Collections.ObjectModel;
using Nin.Types.RavenDb;

namespace Nin.Dataleveranser.Import
{
    public class DataFiles : Dictionary<string, IDataFile>
    {
        private void CheckDocuments(IEnumerable<Document> documents)
        {
            foreach (var document in documents)
            {
                bool documentFound = false;
                foreach (var file in Values)
                {
                    if (file.Filename != document.FileName) continue;
                    documentFound = true;
                    break;
                }
                if (!documentFound)
                    throw new DataDeliveryParseException("Dataleveransen mangler filen '" + document.FileName + "'.");
            }
        }

        public void CheckDocuments(Collection<Nin.Types.MsSql.Document> collection)
        {
            if (Count > collection.Count)
                throw new DataDeliveryParseException("The grid contains file(s) without document reference(s)");
            var docs = new Collection<Document>();
            foreach (var document in (IEnumerable<Nin.Types.MsSql.Document>) collection)
                docs.Add(new Document(document));
            CheckDocuments(docs);
        }

        public void CheckDocuments(Dataleveranse dataleveranse)
        {
            int documentCount = 0;

            CheckDocuments(dataleveranse.Metadata.Documents);
            documentCount += dataleveranse.Metadata.Documents.Count;

            foreach (var natureArea in dataleveranse.Metadata.NatureAreas)
            {
                CheckDocuments(natureArea.Documents);
                documentCount += natureArea.Documents.Count;
            }

            if (Count > documentCount)
                throw new DataDeliveryParseException(
                    "The data delivery contains file(s) without document reference(s)");
        }
    }
}
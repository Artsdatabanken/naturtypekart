using System;
using System.IO;
using System.Xml.Linq;
using Nin.Dataleveranser;
using Nin.Types.MsSql;
using SqlServer = Nin.IO.SqlServer.SqlServer;

namespace Nin.Command
{
    public class ImportDataDeliveryCommand : DatabaseCommand
    {
        public override void Execute()
        {
            Types.RavenDb.Dataleveranse dataleveranse;
            using (var streamReader = File.OpenText(dataFile))
            {
                var fileContent = streamReader.ReadToEnd();

                XDocument dataDeliveryXml = XDocument.Parse(fileContent);
                dataleveranse = DataleveranseXmlGreier.ParseDataDelivery(dataDeliveryXml);
            }
            //DataleveranseXmlGreier.ValidateDataDeliveryContent(dataDelivery);

            var dataDeliveryMsSql = new Dataleveranse(dataleveranse);
            MapProjection.ConvertGeometry(dataDeliveryMsSql);
            foreach (var natureArea in dataDeliveryMsSql.Metadata.NatureAreas)
                natureArea.Institution = "Institution";

            dataDeliveryMsSql.Id = "1";
            dataDeliveryMsSql.Created = DateTime.Now;
            SqlServer.DeleteDataDelivery(dataDeliveryMsSql.Metadata.UniqueId.LocalId);
            SqlServer.LagreDataleveranse(dataDeliveryMsSql);
        }

        public ImportDataDeliveryCommand(string dataFile)
        {
            this.dataFile = dataFile;
        }

        protected override string GetDescription()
        {
            return "Importing data delivery from file '" + dataFile + "'.";
        }

        private readonly string dataFile;
    }
}
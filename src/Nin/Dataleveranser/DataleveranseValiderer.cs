using System;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Nin.Dataleveranser
{
    public class DataleveranseValiderer
    {
        private readonly XmlSchemaSet dataDeliverySchemaSet;

        public DataleveranseValiderer(XmlSchemaSet dataDeliverySchemaSet)
        {
            this.dataDeliverySchemaSet = dataDeliverySchemaSet;
        }

        public void ValidateDataDelivery(XDocument dataDeliveryXml)
        {
            bool error = false;
            string message = "";

            dataDeliveryXml.Validate(dataDeliverySchemaSet, (o, e) =>
            {
                if (!string.IsNullOrEmpty(message)) message += "\n";
                message += e.Message;
                error = true;
            });

            if (error)
                throw new Exception(message);
        }
    }
}
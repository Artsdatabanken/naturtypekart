using System;

namespace Nin.Dataleveranser
{
    public class DataDeliveryParseException : Exception
    {
        public DataDeliveryParseException(string message) : base(message)
        {
        }
    }
}
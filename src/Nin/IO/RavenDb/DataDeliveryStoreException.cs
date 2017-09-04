using System;

namespace Nin.IO.RavenDb
{
    public class DataDeliveryStoreException : Exception
    {
        public DataDeliveryStoreException(string message) : base(message)
        {
        }
    }
}
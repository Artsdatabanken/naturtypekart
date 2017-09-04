using System;
using Common.Session;
using Nin.Diagnostic;
using Nin.IO.RavenDb;
using Nin.IO.SqlServer;
using Nin.Types.MsSql;
using Types;

namespace Nin.Dataleveranser
{
    public class DataleveransePubliserer
    {
        public static void PubliserLeveranse(string id, NinRavenDb arkiv, IUserDatabase userDb)
        {
            Log.i("DDL", "Publiser dataleveranse #" + id);

            var dataDelivery = arkiv.HentDataleveranse(id);
            switch (dataDelivery.Publisering)
            {
                case Status.Gjeldende:
                    throw new Exception("Leveransen er allerede gjeldende.");
                case Status.Utgått:
                    throw new Exception("Leveransen er utgått og kan ikke publiseres.");
            }

            var dataDeliveryMsSql = new Dataleveranse(dataDelivery);
            MapProjection.ConvertGeometry(dataDeliveryMsSql);

            var userInstitution = userDb.GetUserInstitution(dataDelivery.Username);
            foreach (var natureArea in dataDeliveryMsSql.Metadata.NatureAreas)
                natureArea.Institution = userInstitution;

            SqlServer.DeleteDataDelivery(dataDeliveryMsSql.Metadata.UniqueId.LocalId);
            SqlServer.LagreDataleveranse(dataDeliveryMsSql);

            MarkerSistGjeldendeLeveranseSomUtgått(arkiv, dataDelivery);

            dataDelivery.Publisering = Status.Gjeldende;
            arkiv.LagreDataleveranse(dataDelivery);
            arkiv.SaveChanges();
        }

        private static void MarkerSistGjeldendeLeveranseSomUtgått(NinRavenDb arkiv, Types.RavenDb.Dataleveranse dataDelivery)
        {
            Types.RavenDb.Dataleveranse currentDataDelivery = arkiv.FinnDataleveranse(dataDelivery.Metadata.UniqueId.LocalId);
            if (currentDataDelivery != null)
            {
                currentDataDelivery.Publisering = Status.Utgått;
                currentDataDelivery.Expired = DateTime.Now;
                arkiv.LagreDataleveranse(currentDataDelivery);
                dataDelivery.ParentId = currentDataDelivery.Id;
            }
        }
    }
}
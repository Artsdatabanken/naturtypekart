using System.Linq;
using Nin.Types.RavenDb;
using Raven.Client.Indexes;

namespace Nin.IO.RavenDb.Transformers
{
    public class DataleveranseListItemTransformer : AbstractTransformerCreationTask<Dataleveranse>
    {
        public DataleveranseListItemTransformer()
        {
            TransformResults = results =>
                from
                dataDelivery
                in
                results
                select
                new DataleveranseListItem
                {
                    Id = dataDelivery.Id,
                    Name = dataDelivery.Name,
                    Description = dataDelivery.Description,
                    Status = dataDelivery.Publisering,
                    Username = dataDelivery.Username,
                    Created = dataDelivery.Created,
                    Expired = dataDelivery.Expired,
                    DeliveryDate = dataDelivery.DeliveryDate,
                    OperatorCompany = dataDelivery.Operator.Company,
                    OperatorContactPerson = dataDelivery.Operator.ContactPerson,
                    OperatorEmail = dataDelivery.Operator.Email,
                    OperatorHomesite = dataDelivery.Operator.Homesite,
                    MetadataProjectDescription = dataDelivery.Metadata.ProjectDescription,
                    MetadataProjectName = dataDelivery.Metadata.ProjectName
                };
        }
    }
}
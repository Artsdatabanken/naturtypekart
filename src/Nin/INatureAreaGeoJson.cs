using Microsoft.SqlServer.Types;
using Nin.Types.MsSql;
using System.Collections.Generic;
using Types;

namespace Nin
{
    public interface INatureAreaGeoJson
    {
        int Id { get; set; }

        List<Parameter> Parameters { get; set; }

        Identification UniqueId { get; set; }

        SqlGeometry Area { get; set; }

        int Count { get; set; }
    }
}
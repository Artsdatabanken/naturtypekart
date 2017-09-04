using Nin.Aspnet;
using Nin.Naturtyper;

namespace Api.Controllers
{
    public class NaturkoderController
    {
        public NinHtmlResult Index()
        {
            return new NinHtmlResult(@"<ul>
    <li><a href=""Typer"">Naturtyper</a></li>
    <li><a href=""Typer?kode=NA_T34-C-2"">Naturtype kode NA_T34-C-2</a></li>
    <li><a href=""Variasjon"">Naturvariasjon</a></li>
    <li><a href=""Variasjon?kode=1AE-MB-0-XY"">Naturvariasjon kode 1AE-MB-0-XY</a></li>
</ul>");
        }

        public object Typer(string kode = null)
        {
            if(string.IsNullOrEmpty(kode))
                return Naturkodetrær.Naturtyper.Koder;
            return Naturkodetrær.Naturtyper.HentFraKode(kode);
        }

        public object Variasjon(string kode=null)
        {
            if(string.IsNullOrEmpty(kode))
                return Naturkodetrær.Naturvariasjon.Koder;
            return Naturkodetrær.Naturvariasjon.HentFraKode(kode);
        }
    }
}
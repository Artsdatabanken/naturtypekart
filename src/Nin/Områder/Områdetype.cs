namespace Nin.Områder
{
    public class Områdetype
    {
        public Områdetype(string kode, string navn)
        {
            Kode = kode;
            Navn = navn;
        }

        public string Kode { get; }
        public string Navn { get; }
    }
}
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;
using Nin.Configuration;

namespace Nin.Dataleveranser
{
    public class NinXElement : XElement
    {
        public string mapsTo;
        private static XNamespace ninNs => Config.Settings.Namespace.Nin;

        public NinXElement(XName name) : base(ninNs + name.ToString())
        {
        }

        public NinXElement(XName name, object content) : base(ninNs + name.ToString(), content)
        {
        }

        public NinXElement(XName name, string mapsTo, object content) : base(ninNs + name.ToString(), content)
        {
            this.mapsTo = mapsTo;
        }

        public NinXElement(XName name, string mapsTo, params object[] content) : base(ninNs + name.ToString(), content)
        {
            this.mapsTo = mapsTo;
        }

        public void Print(string prefix)
        {
            Indent(Name.LocalName, mapsTo, prefix);
            if (NextNode != null)
                Print(prefix, NextNode);
            string pwc = Name.LocalName;
            if (!string.IsNullOrEmpty(prefix))
                pwc = prefix + "." + Name.LocalName;
            //Indent(Name.LocalName, mapsTo, prefix);
            foreach (var child in Descendants())
                Print(pwc, child);
            mapsTo = null;
        }

        private void Print(string prefix, XNode child)
        {
            var ne = child as NinXElement;
            if (ne != null)
            {
                ne.Print(prefix);
            }
            else
            {
                if (child.NextNode != null)
                    Print(prefix, child.NextNode);
            }
        }

        private static void Indent(string s, string mapsTo, string prefix)
        {
            if (string.IsNullOrEmpty(mapsTo))
                return;
            string source = s;
            if (!string.IsNullOrEmpty(prefix))
                source = prefix + "." + source;
            Debug.WriteLine(source + ": " + mapsTo);
            Debug.WriteLine("-" + mapsTo + ": " + source);
            if (!atob.ContainsKey(source))
                atob[source] = mapsTo;
            if (!btoa.ContainsKey(mapsTo))
                btoa[mapsTo] = source;
        }

        public static Dictionary<string, string> atob = new Dictionary<string, string>();
        public static Dictionary<string, string> btoa = new Dictionary<string, string>();
    }
}
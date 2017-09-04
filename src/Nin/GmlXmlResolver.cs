using System;
using System.Collections.Generic;
using System.Xml;
using Nin.Diagnostic;

namespace Nin
{
    public class GmlXmlResolver : XmlUrlResolver
    {
        private Uri CacheUri { get; }

        public readonly Dictionary<string, string> UriMap = new Dictionary<string, string>();

        public GmlXmlResolver(Uri cacheUri)
        {
            CacheUri = cacheUri;
        }

        public override Uri ResolveUri(Uri baseUri, string relativeUri)
        {
            if (UriMap.ContainsKey(relativeUri))
            {
                var mapUri = new Uri(CacheUri, UriMap[relativeUri]);
                Log.c("XSD", $"Overriding resolution of {relativeUri} to {mapUri}");
                return mapUri;
            }

            var uri = base.ResolveUri(baseUri, relativeUri);
            Log.c("XSD", $"Resolving {relativeUri} to {uri}");
            return uri;
        }
    }
}
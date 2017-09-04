using System;
using Raven.Client;

namespace Nin.IO.RavenDb
{
    internal interface IRavenDbStore : IDisposable
    {
        IDocumentSession Session { get; }
        IDocumentStore Store { get; }
    }
}
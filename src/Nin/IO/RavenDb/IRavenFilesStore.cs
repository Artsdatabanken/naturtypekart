using System;
using Raven.Client.FileSystem;

namespace Nin.IO.RavenDb
{
    internal interface IRavenFilesStore  : IDisposable
    {
        IAsyncFilesSession Session { get; }
    }
}
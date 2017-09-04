using System.Threading.Tasks;
using Raven.Client.FileSystem;

namespace Nin.IO.RavenDb
{
    internal class RavenFilesStore : IRavenFilesStore
    {
        private readonly string url;
        private readonly string filesystem;
        private readonly bool autoSaveChanges;
        private readonly int maxRequestsPerSession;
        private IAsyncFilesSession session;
        private IFilesStore store;

        public RavenFilesStore(string url, string filesystem)
        {
            maxRequestsPerSession = 29;
            this.url = url;
            this.filesystem = filesystem;
            autoSaveChanges = false;
        }

        public IAsyncFilesSession Session
        {
            get
            {
                if (session == null)
                    GenerateNewSession();
                else if (session.Advanced.NumberOfRequests > maxRequestsPerSession - 3)
                    SaveChangesAndGetNewSession();

                return session;
            }
        }

        private IFilesStore Store
        {
            get
            {
                if (store != null) return store;
                store = new FilesStore {Url = url, DefaultFileSystem = filesystem};
                store.Initialize();

                return store;
            }
        }

        public void Dispose()
        {
            if (session != null)
            {
                session.Dispose();
                session = null;
            }

            if (store != null)
            {
                store.Dispose();
                store = null;
            }
        }

        private void SaveChangesAndGetNewSession(bool forceSave = false)
        {
            if (forceSave || autoSaveChanges)
            {
                session.Advanced.MaxNumberOfRequestsPerSession = session.Advanced.MaxNumberOfRequestsPerSession + 100;
                Task t = session.SaveChangesAsync();
                t.ContinueWith(task => GenerateNewSession());
            }
            else
                GenerateNewSession();
        }

        private void GenerateNewSession()
        {
            session?.Dispose();

            session = Store.OpenAsyncSession();
            session.Advanced.MaxNumberOfRequestsPerSession = maxRequestsPerSession;
        }
    }
}
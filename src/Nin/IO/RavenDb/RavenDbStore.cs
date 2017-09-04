using Raven.Client;

namespace Nin.IO.RavenDb
{
    internal class RavenDbStore : IRavenDbStore
    {
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

        public IDocumentStore Store
        {
            get
            {
                if (store != null) return store;

                //this.store = new DocumentStore { ConnectionStringName = this.connectionStringName };
                store = new Raven.Client.Document.DocumentStore {Url = url, DefaultDatabase = database};
                //((DocumentStoreBase)_store).RegisterListener(new RemoteIdIncrementListener(_store));
                store.Initialize();

                return store;
            }
        }

        public IDocumentSession Session
        {
            get
            {
                if (session == null)
                {
                    GenerateNewSession();
                }
                else if (session.Advanced.NumberOfRequests > maxRequestsPerSession - 3)
                {
                    SaveChangesAndGetNewSession();
                }

                return session;
            }
        }

        private void SaveChangesAndGetNewSession(bool forceSave = false)
        {
            if (session.Advanced.HasChanges && (forceSave || autoSaveChanges))
            {
                session.Advanced.MaxNumberOfRequestsPerSession = session.Advanced.MaxNumberOfRequestsPerSession +
                                                                 100;
                session.SaveChanges();
            }

            GenerateNewSession();
        }

        private void GenerateNewSession()
        {
            session?.Dispose();

            session = Store.OpenSession();
            session.Advanced.MaxNumberOfRequestsPerSession = maxRequestsPerSession;
        }

        public RavenDbStore(string url, string database)
        {
            maxRequestsPerSession = 29;
            this.url = url;
            this.database = database;
            autoSaveChanges = false;
        }

        private readonly bool autoSaveChanges;
        private readonly string database;
        private readonly int maxRequestsPerSession;
        private readonly string url;
        private IDocumentSession session;
        private IDocumentStore store;
    }
}
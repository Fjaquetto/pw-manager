using Google.Cloud.Firestore;
using PWManager.Infra.Context.Firestore.DataContracts;

namespace PWManager.Infra.Context.Firestore
{
    public class FirestoreContext : IFirestoreContext
    {
        public FirestoreDb Database { get; }

        public FirestoreContext(string projectId)
        {
            Database = FirestoreDb.Create(projectId);
        }
    }
}
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Grpc.Auth;
using Grpc.Net.Client;
using PWManager.Infra.Context.Firestore.DataContracts;

namespace PWManager.Infra.Context.Firestore
{
    public class FirestoreContext : IFirestoreContext
    {
        public FirestoreDb Database { get; }

        public FirestoreContext(string projectId, string firebaseKeyPath)
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", firebaseKeyPath);
            Database = FirestoreDb.Create(projectId);
        }
    }
}
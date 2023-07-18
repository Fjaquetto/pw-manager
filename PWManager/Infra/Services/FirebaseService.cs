using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using PWManager.Domain.DataContracts;

namespace PWManager.Infra.Services
{
    public class FirebaseService : IFirebaseService
    {
        private readonly string _pathToKey;

        public FirebaseService(string pathToKey)
        {
            _pathToKey = pathToKey;
        }

        public FirebaseApp InitializeFirebase()
        {
            GoogleCredential credential = GoogleCredential.FromFile(_pathToKey);
            FirebaseApp defaultApp = FirebaseApp.Create(new AppOptions() { Credential = credential });

            return defaultApp;
        }
    }
}

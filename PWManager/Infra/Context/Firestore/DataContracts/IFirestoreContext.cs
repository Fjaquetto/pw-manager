using Google.Cloud.Firestore;

namespace PWManager.Infra.Context.Firestore.DataContracts
{
    public interface IFirestoreContext
    {
        FirestoreDb Database { get; }
    }
}

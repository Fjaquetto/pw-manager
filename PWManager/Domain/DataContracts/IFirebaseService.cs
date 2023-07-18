using FirebaseAdmin;

namespace PWManager.Domain.DataContracts
{
    public interface IFirebaseService
    {
        FirebaseApp InitializeFirebase();
    }
}

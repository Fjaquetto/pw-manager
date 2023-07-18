using Google.Cloud.Firestore;
using PWManager.Domain.DataContracts;
using PWManager.Domain.Model;
using PWManager.Infra.Context.Firestore.DataContracts;

namespace PWManager.Infra.Repository
{
    public class FirestoreRepository : IFirestoreRepository
    {
        private readonly IFirestoreContext _context;
        private readonly CollectionReference _userCollection;

        public FirestoreRepository(IFirestoreContext context)
        {
            _context = context;
            _userCollection = _context.Database.Collection("users");
        }

        public async Task<User> GetUser(string userId)
        {
            DocumentSnapshot snapshot = await _userCollection.Document(userId).GetSnapshotAsync();

            if (snapshot.Exists)
            {
                Dictionary<string, object> userData = snapshot.ToDictionary();
                User user = new User(
                    userData["Site"].ToString(),
                    userData["Login"].ToString(),
                    userData["Password"].ToString(),
                    Guid.Parse(userData["Id"].ToString()),
                    bool.Parse(userData["IsActive"].ToString())
                );

                user.CreationDate = DateTime.Parse(userData["CreationDate"].ToString());

                return user;
            }
            else
            {
                return null;
            }
        }

        public async Task<User> AddUser(User user)
        {
            user.EncryptData();

            var docRef = await _userCollection.AddAsync(user);
            user.Id = new Guid(docRef.Id);

            return user;
        }

        public async Task UpdateUser(User user)
        {
            var docRef = _userCollection.Document(user.Id.ToString());
            user.EncryptData();

            await docRef.SetAsync(user, SetOptions.Overwrite);
        }
    }
}

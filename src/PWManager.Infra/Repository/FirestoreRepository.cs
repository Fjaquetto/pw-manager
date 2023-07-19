using Google.Cloud.Firestore;
using PWManager.Domain.DataContracts;
using PWManager.Infra.Context.Firestore.DataContracts;

namespace PWManager.Infra.Repository
{
    public class FirestoreRepository<T> : IFirestoreRepository<T>
    {
        private readonly IFirestoreContext _context;
        private readonly CollectionReference _collection;

        public FirestoreRepository(IFirestoreContext context)
        {
            _context = context;
            _collection = _context.Database.Collection(typeof(T).Name.ToLower());
        }

        public async Task<List<T>> GetAllAsync(Func<Dictionary<string, object>, T> converter)
        {
            QuerySnapshot snapshot = _collection.GetSnapshotAsync().Result;
            var entities = new List<T>();

            foreach (var document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    T entity = converter(document.ToDictionary());
                    entities.Add(entity);
                }
            }

            return await Task.FromResult(entities);
        }

        public async Task<T> GetAsync(string id, Func<Dictionary<string, object>, T> converter)
        {
            var snap = _collection.WhereEqualTo("Id", id).GetSnapshotAsync().Result;

            var document = snap.Documents?.FirstOrDefault();
            if (document != null)
            {
                var obj = converter(document.ToDictionary());
                return await Task.FromResult(obj);
            }

            return default;
        }

        public async Task<T> AddAsync(T entity, Func<T, Dictionary<string, object>> converter)
        {
            _collection.AddAsync(converter(entity)).Wait();

            return await Task.FromResult(entity);
        }

        public async Task DeleteAsync(string id)
        {
            var snap = _collection.WhereEqualTo("Id", id).GetSnapshotAsync().Result;

            var document = snap.Documents?.FirstOrDefault();
            if (document != null)
            {
                document.Reference.DeleteAsync().Wait();
            }

            await Task.CompletedTask;
        }

        public async Task DeleteAllAsync()
        {
            QuerySnapshot snapshot = _collection.GetSnapshotAsync().Result;

            foreach (var document in snapshot.Documents)
            {
                document.Reference.DeleteAsync().Wait();
            }

            await Task.CompletedTask;
        }
    }
}

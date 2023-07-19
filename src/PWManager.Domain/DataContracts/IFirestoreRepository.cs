using PWManager.Domain.Model;

namespace PWManager.Domain.DataContracts
{
    public interface IFirestoreRepository<T>
    {
        Task<List<T>> GetAllAsync(Func<Dictionary<string, object>, T> converter);
        Task<T> GetAsync(string id, Func<Dictionary<string, object>, T> converter);
        Task<T> AddAsync(T entity, Func<T, Dictionary<string, object>> converter);
        Task DeleteAsync(string id);
        Task DeleteAllAsync();
    }
}

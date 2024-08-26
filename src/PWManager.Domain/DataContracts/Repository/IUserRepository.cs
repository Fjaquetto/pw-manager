using PWManager.Domain.Model;

namespace PWManager.Domain.DataContracts.Repository
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetUserByIdAsync(Guid id);
    }
}

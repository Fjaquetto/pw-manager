using PWManager.Domain.Model;

namespace PWManager.Application.DataContracts
{
    public interface IUserApplication
    {
        Task<User> GetUserByIdAsync(Guid id);
        Task AddUserAsync(User user);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task DeleteUserAsync(User user);
    }
}

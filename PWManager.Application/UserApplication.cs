using PWManager.Application.DataContracts;
using PWManager.Domain.DataContracts.Repository;
using PWManager.Domain.Model;

namespace PWManager.Application
{
    public class UserApplication : IUserApplication
    {
        private readonly IUserRepository _userRepository;

        public UserApplication(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task AddUserAsync(User user)
        {
            await _userRepository.AddAsync(user);
        }

        public async Task<User> GetUserByIdAsync(Guid id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }

        public async Task DeleteUserAsync(User user)
        {
            await _userRepository.DeleteAsync(user);
        }
    }
}

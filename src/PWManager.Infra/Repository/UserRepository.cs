using Microsoft.EntityFrameworkCore;
using PWManager.Domain.DataContracts.Repository;
using PWManager.Domain.Model;
using PWManager.Infra.Context.SQLite;

namespace PWManager.Infra.Repository
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(PWDbContext context) : base(context)
        {
        }

        public async Task<User> GetUserByIdAsync(Guid id)
        {
            return await GetAsync(x => x.Id.Equals(id));
        }

        public Task<bool> AnyAsync() => context.User.AnyAsync();
    }
}

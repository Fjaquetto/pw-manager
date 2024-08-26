using PWManager.Domain.DataContracts.InfraService;
using PWManager.Domain.Model;

namespace PWManager.Infra.Services
{
    public class UserEncryptorService : IUserEncryptorService
    {
        public User EncryptUser(User user)
        {
            user.Site = EncryptorService.Encrypt(user.Site);
            user.Login = EncryptorService.Encrypt(user.Login);
            user.Password = EncryptorService.Encrypt(user.Password);
            return user;
        }

        public User DecryptUser(User user)
        {
            user.Site = EncryptorService.Decrypt(user.Site);
            user.Login = EncryptorService.Decrypt(user.Login);
            user.Password = EncryptorService.Decrypt(user.Password);
            return user;
        }
    }
}

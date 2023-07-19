using PWManager.Domain.DataContracts;
using PWManager.Domain.Model;

namespace PWManager.Infra.Services
{
    public class UserEncryptorService : IUserEncryptorService
    {
        public User EncryptUser(User user)
        {
            EncryptorService.Encrypt(user.Site);
            EncryptorService.Encrypt(user.Login);
            EncryptorService.Encrypt(user.Password);
            return user;
        }

        public User DecryptUser(User user)
        {
            EncryptorService.Decrypt(user.Site);
            EncryptorService.Decrypt(user.Login);
            EncryptorService.Decrypt(user.Password);
            return user;
        }
    }
}

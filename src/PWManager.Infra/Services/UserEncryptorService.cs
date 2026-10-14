using PWManager.Domain.DataContracts.InfraService;
using PWManager.Domain.Model;

namespace PWManager.Infra.Services
{
    public class UserEncryptorService(IVaultSession session) : IUserEncryptorService
    {
        public User EncryptUser(User user)
        {
            var key = session.GetKey();

            user.Site = VaultCrypto.Encrypt(key, user.Site);
            user.Login = VaultCrypto.Encrypt(key, user.Login);
            user.Password = VaultCrypto.Encrypt(key, user.Password);
            return user;
        }

        public User DecryptUser(User user)
        {
            var key = session.GetKey();

            user.Site = VaultCrypto.Decrypt(key, user.Site);
            user.Login = VaultCrypto.Decrypt(key, user.Login);
            user.Password = VaultCrypto.Decrypt(key, user.Password);
            return user;
        }
    }
}

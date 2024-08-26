using PWManager.Domain.Model;

namespace PWManager.Domain.DataContracts.InfraService
{
    public interface IUserEncryptorService
    {
        User EncryptUser(User user);
        User DecryptUser(User user);
    }
}

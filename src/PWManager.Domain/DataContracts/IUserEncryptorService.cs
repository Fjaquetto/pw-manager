using PWManager.Domain.Model;

namespace PWManager.Domain.DataContracts
{
    public interface IUserEncryptorService
    {
        User EncryptUser(User user);
        User DecryptUser(User user);
    }
}

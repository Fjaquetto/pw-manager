using PWManager.Infra.Extensions;

namespace PWManager.Domain.Model
{
    public class User
    {
        protected User() { }
        public User(string site, string login, string password, Guid? id = null, bool isActive = true)
        {
            Id = id ?? Guid.NewGuid();
            Site = site;
            Login = login;
            Password = password;
            CreationDate = DateTime.Now;
            IsActive = isActive;
        }

        public Guid Id { get; set; }
        public string Site { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public DateTime CreationDate { get; set; }
        public bool IsActive { get; set; }

        public User EncryptData()
        {
            Site = EncryptorData.Encrypt(Site);
            Login = EncryptorData.Encrypt(Login);
            Password = EncryptorData.Encrypt(Password);

            return this;
        }

        public User DecryptData()
        {
            Site = EncryptorData.Decrypt(Site);
            Login = EncryptorData.Decrypt(Login);
            Password = EncryptorData.Decrypt(Password);

            return this;
        }
    }
}

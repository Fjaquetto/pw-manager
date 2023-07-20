namespace PWManager.Domain.Model
{
    public class User
    {
        public User() { }
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
    }
}

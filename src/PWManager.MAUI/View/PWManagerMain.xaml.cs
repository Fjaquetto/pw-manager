using PWManager.Domain.DataContracts;
using PWManager.Domain.Model;
using PWManager.MAUI.ViewModel;

namespace PWManager.MAUI.View;

public partial class PWManagerMain : ContentPage
{
    public PWManagerMain(IRepository<User> repository)
    {
        InitializeComponent();
        BindingContext = new UserViewModel(repository);
    }

    void OnInsertClicked(object sender, EventArgs e)
    {
        var site = SiteEntry.Text;
        var login = LoginEntry.Text;
        var password = PasswordEntry.Text;

        var newUser = new User(site, login, password);

    }
}
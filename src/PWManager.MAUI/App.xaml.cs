using PWManager.MAUI.View;

namespace PWManager.MAUI
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}
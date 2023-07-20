using Microsoft.Extensions.DependencyInjection;
using PWManager.Domain.DataContracts;
using PWManager.Domain.Model;
using PWManager.Infra.Services;

namespace PWManager.Application.Forms
{
    public partial class PWManagerKey : Form
    {
        private IServiceProvider _serviceProvider;

        public PWManagerKey(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            EncryptorService.EncryptorPassword = txtEncryptor.Text;

            PWManager manager = new PWManager(_serviceProvider);
            manager.Show();

            manager.Closed += (_, args) => this.Close();

            this.Hide();
        }
    }
}

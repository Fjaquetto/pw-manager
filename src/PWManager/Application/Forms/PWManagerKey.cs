using PWManager.Infra.Services;

namespace PWManager.Application.Forms
{
    public partial class PWManagerKey : Form
    {
        private readonly Lazy<PWManager> _pwManager;

        public PWManagerKey(Lazy<PWManager> pwManager)
        {
            _pwManager = pwManager;

            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            EncryptorService.EncryptorPassword = txtEncryptor.Text;

            _pwManager.Value.Show();

            _pwManager.Value.Closed += (_, args) => this.Close();

            this.Hide();
        }
    }
}

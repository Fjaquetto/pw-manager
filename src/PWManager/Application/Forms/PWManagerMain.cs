using PWManager.Application.DataContracts;
using PWManager.Domain.DataContracts.InfraService;
using PWManager.Domain.Model;
using PWManager.Infra.Helpers;
using System.Security.Cryptography;

namespace PWManager
{
    public partial class PWManager : Form
    {
        private readonly IUserApplication _userApplication;
        private readonly IUserEncryptorService _userEncryptorService;

        private List<User> _decryptedUsers;
        private const int PASSWORD_LENGTH = 12;

        public PWManager(IUserEncryptorService userEncryptorService, IUserApplication userApplication)
        {
            _userEncryptorService = userEncryptorService;
            _userApplication = userApplication;

            InitializeComponent();
            PopulateUserGrid();
            HideColumnsInDataGrid();
        }

        private void PopulateUserGrid()
        {
            try
            {
                var users = _userApplication.GetAllUsersAsync().Result;
                _decryptedUsers = users.Select(x => _userEncryptorService.DecryptUser(x)).ToList();
            }
            catch (CryptographicException)
            {
                MessageBox.Show("Incorrect password.", "Alert");

                System.Windows.Forms.Application.Exit();
            }

            dgUser.DataSource = _decryptedUsers;
        }

        private void HideColumnsInDataGrid()
        {
            if (dgUser.Columns.Contains("Id"))
                dgUser.Columns["Id"].Visible = false;
        }

        private void FilterUserData()
        {
            dgUser.DataSource = _decryptedUsers
                .Where(x =>
                    x.Site.ToUpper().Contains(txtSiteSearch.Text.ToUpper()) &&
                    x.Login.ToUpper().Contains(txtLoginSearch.Text.ToUpper()))
                .ToList();
        }

        private void CleanFields()
        {
            txtSite.Clear();
            txtLogin.Clear();
            txtPassword.Clear();
        }

        #region Events
        private void btnInserir_Click(object sender, EventArgs e)
        {
            var user = new User(txtSite.Text, txtLogin.Text, txtPassword.Text);
            _userEncryptorService.EncryptUser(user);

            _userApplication.AddUserAsync(user).Wait();

            PopulateUserGrid();
            CleanFields();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgUser.SelectedRows)
            {
                var id = row.Cells["Id"].Value.ToString();
                var user = _userApplication.GetUserByIdAsync(Guid.Parse(id)).Result;
                _userApplication.DeleteUserAsync(user).Wait();
            }

            PopulateUserGrid();
        }
        private void txtSiteSearch_TextChanged(object sender, EventArgs e)
        {
            FilterUserData();
        }

        private void txtLoginSearch_TextChanged(object sender, EventArgs e)
        {
            FilterUserData();
        }

        private void btnGeneratePassword_Click(object sender, EventArgs e)
        {
            txtGeneratePassword.Text = GeneratePasswordExtension.Generate(PASSWORD_LENGTH);
        }
        #endregion
    }
}
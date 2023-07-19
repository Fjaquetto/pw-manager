using Microsoft.Extensions.DependencyInjection;
using PWManager.Domain.DataContracts;
using PWManager.Domain.Model;
using PWManager.Infra.Helpers;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace PWManager
{
    public partial class PWManager : Form
    {
        private readonly IRepository<User> _repository;
        private readonly IUserEncryptorService _userEncryptorService;
        private readonly IFirestoreRepository<User> _firestoreRepository;

        private List<User> _decryptedUsers;

        public PWManager(IServiceProvider serviceProvider)
        {
            _repository = serviceProvider.GetRequiredService<IRepository<User>>();
            _userEncryptorService = serviceProvider.GetRequiredService<IUserEncryptorService>();
            _firestoreRepository = serviceProvider.GetRequiredService<IFirestoreRepository<User>>();

            InitializeComponent();
            PopulateUserGrid();
            HideColumnsInDataGrid();
        }

        private void PopulateUserGrid()
        {
            try
            {
                var users = _firestoreRepository.GetAllAsync(ObjectExtensions.DictionaryToEntity<User>).Result;
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

        #region Events
        private void btnInserir_Click(object sender, EventArgs e)
        {
            var user = new User(txtSite.Text, txtLogin.Text, txtPassword.Text);
            _userEncryptorService.EncryptUser(user);

            _repository.AddAsync(user).Wait();
            _firestoreRepository.AddAsync(user, ObjectExtensions.EntityToDictionary);

            PopulateUserGrid();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgUser.SelectedRows)
            {
                var id = row.Cells["Id"].Value;
                var user = _repository.GetAsync(x => x.Id.Equals(id)).Result;
                _repository.DeleteAsync(user).Wait();
                _firestoreRepository.DeleteAsync(user.Id.ToString());
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
        #endregion
    }
}
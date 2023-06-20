using Microsoft.Extensions.DependencyInjection;
using PWManager.Domain.DataContracts;
using PWManager.Domain.Model;
using System.Windows.Forms;

namespace PWManager
{
    public partial class PWManager : Form
    {
        private readonly IRepository<User> _repository;
        private List<User> _decryptedUsers;

        public PWManager(IServiceProvider serviceProvider)
        {
            _repository = serviceProvider.GetRequiredService<IRepository<User>>();
            InitializeComponent();
            PopulateUserGrid();
            HideColumnsInDataGrid();
        }

        private void PopulateUserGrid()
        {
            var users = _repository.GetAllAsync().Result;
            _decryptedUsers = users.Select(x => x.DecryptData()).ToList();
            dgUser.DataSource = _decryptedUsers;
        }

        private void HideColumnsInDataGrid()
        {
            if (dgUser.Columns.Contains("Id"))
            {
                dgUser.Columns["Id"].Visible = false;
            }
        }

        #region Events
        private void btnInserir_Click(object sender, EventArgs e)
        {
            var user = new User(txtSite.Text, txtLogin.Text, txtPassword.Text).EncryptData();
            _repository.AddAsync(user).Wait();
            PopulateUserGrid();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgUser.SelectedRows)
            {
                var id = row.Cells["Id"].Value;
                var user = _repository.GetAsync(x => x.Id.Equals(id)).Result;
                _repository.DeleteAsync(user).Wait();
            }
            PopulateUserGrid();
        }
        #endregion


        private void txtSiteSearch_TextChanged(object sender, EventArgs e)
        {
            dgUser.DataSource = _decryptedUsers.Where(x => x.Site.Contains(txtSiteSearch.Text)).ToList();
        }
    }
}
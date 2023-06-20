using Microsoft.Extensions.DependencyInjection;
using PWManager.Domain.DataContracts;
using PWManager.Domain.Model;
using System.Windows.Forms;

namespace PWManager
{
    public partial class PWManager : Form
    {
        private readonly IRepository<User> _repository;
        public PWManager(IServiceProvider serviceProvider)
        {
            _repository = serviceProvider.GetRequiredService<IRepository<User>>();
            InitializeComponent();
            PopulateUserGrid();
            DisableResizing();
            HideColumnsInDataGrid();
        }

        private void PopulateUserGrid()
        {
            var users = _repository.GetAllAsync().Result;
            var decryptedUsers = users.Select(x => x.EncryptData());
            dgUser.DataSource = decryptedUsers;
        }

        private void HideColumnsInDataGrid()
        {
            if (dgUser.Columns.Contains("Id"))
            {
                dgUser.Columns["Id"].Visible = false;
            }
        }

        private void DisableResizing()
        {
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
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
    }
}
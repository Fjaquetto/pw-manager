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
        private Guid? _selectedUserId = null;

        public PWManager(IUserEncryptorService userEncryptorService, IUserApplication userApplication)
        {
            _userEncryptorService = userEncryptorService;
            _userApplication = userApplication;

            InitializeComponent();
            PopulateUserGrid();
            HideColumnsInDataGrid();
            SetupDataGridViewEvents();
            SetupControlEvents();
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
            
            if (dgUser.Columns.Contains("LastUpdated"))
            {
                dgUser.Columns["LastUpdated"].HeaderText = "Last Updated";
                dgUser.Columns["LastUpdated"].DefaultCellStyle.Format = "g";
            }
            
            if (dgUser.Columns.Contains("CreationDate"))
            {
                dgUser.Columns["CreationDate"].HeaderText = "Creation Date";
                dgUser.Columns["CreationDate"].DefaultCellStyle.Format = "g";
            }
        }

        private void SetupDataGridViewEvents()
        {
            dgUser.SelectionChanged += DgUser_SelectionChanged;
            dgUser.CellDoubleClick += DgUser_CellDoubleClick;
            dgUser.KeyDown += DgUser_KeyDown;
            dgUser.CellClick += DgUser_CellClick;
        }

        private void SetupControlEvents()
        {
            btnClear.Click += btnClear_Click;
            btnInserir.Click += btnInserir_Click;
            btnGeneratePassword.Click += btnGeneratePassword_Click;

            txtLoginSearch.TextChanged += txtLoginSearch_TextChanged;
            txtSiteSearch.TextChanged += txtSiteSearch_TextChanged;

            editToolStripMenuItem.Click += editToolStripMenuItem_Click;
            deleteToolStripMenuItem.Click += deleteToolStripMenuItem_Click;
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
            _selectedUserId = null;
            btnInserir.Text = "Insert";
            
            dgUser.ClearSelection();
        }

        #region Events
        private void btnInserir_Click(object sender, EventArgs e)
        {
            if (_selectedUserId.HasValue)
            {
                var user = _userApplication.GetUserByIdAsync(_selectedUserId.Value).Result;
                var decryptedUser = _userEncryptorService.DecryptUser(user);
                
                decryptedUser.Site = txtSite.Text;
                decryptedUser.Login = txtLogin.Text;
                decryptedUser.Password = txtPassword.Text;
                decryptedUser.LastUpdated = DateTime.Now;
                
                var encryptedUser = _userEncryptorService.EncryptUser(decryptedUser);
                _userApplication.UpdateUserAsync(encryptedUser).Wait();
                
                MessageBox.Show("Entry updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                var user = new User(txtSite.Text, txtLogin.Text, txtPassword.Text);
                _userEncryptorService.EncryptUser(user);
                _userApplication.AddUserAsync(user).Wait();
                
                MessageBox.Show("Entry inserted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            PopulateUserGrid();
            CleanFields();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dgUser.SelectedRows.Count > 0)
            {
                var result = MessageBox.Show(
                    "Are you sure you want to delete the selected entry?", 
                    "Confirm Delete", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    foreach (DataGridViewRow row in dgUser.SelectedRows)
                    {
                        var id = row.Cells["Id"].Value.ToString();
                        var user = _userApplication.GetUserByIdAsync(Guid.Parse(id)).Result;
                        _userApplication.DeleteUserAsync(user).Wait();
                    }

                    PopulateUserGrid();
                    CleanFields();
                    MessageBox.Show("Entry deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Please select an entry to delete.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
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

        private void btnClear_Click(object sender, EventArgs e)
        {
            CleanFields();
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dgUser.SelectedRows.Count > 0)
            {
                var row = dgUser.SelectedRows[0];
                _selectedUserId = Guid.Parse(row.Cells["Id"].Value.ToString());
                
                txtSite.Text = row.Cells["Site"].Value.ToString();
                txtLogin.Text = row.Cells["Login"].Value.ToString();
                txtPassword.Text = row.Cells["Password"].Value.ToString();
                
                btnInserir.Text = "Update";
                
                txtSite.Focus();
            }
        }

                private void DgUser_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgUser.SelectedRows.Count > 0)
            {
                var row = dgUser.SelectedRows[0];
                _selectedUserId = Guid.Parse(row.Cells["Id"].Value.ToString());
                
                txtSite.Text = row.Cells["Site"].Value.ToString();
                txtLogin.Text = row.Cells["Login"].Value.ToString();
                txtPassword.Text = row.Cells["Password"].Value.ToString();
                
                btnInserir.Text = "Update";
            }
            else
            {
                CleanFields();
                _selectedUserId = null;
                btnInserir.Text = "Insert";
            }
        }

        private void DgUser_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var row = dgUser.Rows[e.RowIndex];
                _selectedUserId = Guid.Parse(row.Cells["Id"].Value.ToString());
                
                txtSite.Text = row.Cells["Site"].Value.ToString();
                txtLogin.Text = row.Cells["Login"].Value.ToString();
                txtPassword.Text = row.Cells["Password"].Value.ToString();
                
                btnInserir.Text = "Update";
                
                txtSite.Focus();
            }
        }

        private void DgUser_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && dgUser.SelectedRows.Count > 0)
            {
                deleteToolStripMenuItem_Click(sender, e);
            }
        }

        private void DgUser_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                var columnName = dgUser.Columns[e.ColumnIndex].Name;
                var cellValue = dgUser.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();

                if (columnName == "Password")
                {
                    Clipboard.SetText(cellValue);
                    MessageBox.Show("Password copied to clipboard!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (columnName == "Login")
                {
                    Clipboard.SetText(cellValue);
                    MessageBox.Show("Login copied to clipboard!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        #endregion
    }
}
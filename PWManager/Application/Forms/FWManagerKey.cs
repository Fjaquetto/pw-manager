using PWManager.Infra.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.DataFormats;

namespace PWManager.Application.Forms
{
    public partial class FWManagerKey : Form
    {
        private IServiceProvider _serviceProvider;
        public FWManagerKey(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            EncryptorData.EncryptorPassword = txtEncryptor.Text;

            PWManager manager = new PWManager(_serviceProvider);
            manager.Show();

            manager.Closed += (_, args) => this.Close();

            this.Hide();
        }
    }
}

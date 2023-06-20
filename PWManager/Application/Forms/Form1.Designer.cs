using System.Windows.Forms;

namespace PWManager
{
    partial class PWManager
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            dgUser = new DataGridView();
            contextDelete = new ContextMenuStrip(components);
            deleteToolStripMenuItem = new ToolStripMenuItem();
            txtSite = new TextBox();
            txtLogin = new TextBox();
            txtPassword = new TextBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            groupBox1 = new GroupBox();
            btnInserir = new Button();
            ((System.ComponentModel.ISupportInitialize)dgUser).BeginInit();
            contextDelete.SuspendLayout();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // dgUser
            // 
            dgUser.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgUser.ContextMenuStrip = contextDelete;
            dgUser.Location = new Point(10, 90);
            dgUser.Name = "dgUser";
            dgUser.RowTemplate.Height = 25;
            dgUser.Size = new Size(343, 235);
            dgUser.TabIndex = 0;
            // 
            // contextDelete
            // 
            contextDelete.Items.AddRange(new ToolStripItem[] { deleteToolStripMenuItem });
            contextDelete.Name = "contextDelete";
            contextDelete.Size = new Size(181, 48);
            contextDelete.Text = "Delete";
            // 
            // deleteToolStripMenuItem
            // 
            deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            deleteToolStripMenuItem.Size = new Size(180, 22);
            deleteToolStripMenuItem.Text = "Delete";
            deleteToolStripMenuItem.Click += deleteToolStripMenuItem_Click;
            // 
            // txtSite
            // 
            txtSite.Location = new Point(6, 18);
            txtSite.Name = "txtSite";
            txtSite.Size = new Size(100, 23);
            txtSite.TabIndex = 1;
            // 
            // txtLogin
            // 
            txtLogin.Location = new Point(123, 18);
            txtLogin.Name = "txtLogin";
            txtLogin.Size = new Size(100, 23);
            txtLogin.TabIndex = 2;
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(256, 21);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(94, 23);
            txtPassword.TabIndex = 3;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 3);
            label1.Name = "label1";
            label1.Size = new Size(26, 15);
            label1.TabIndex = 4;
            label1.Text = "Site";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(133, 3);
            label2.Name = "label2";
            label2.Size = new Size(37, 15);
            label2.TabIndex = 5;
            label2.Text = "Login";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(256, 3);
            label3.Name = "label3";
            label3.Size = new Size(57, 15);
            label3.TabIndex = 6;
            label3.Text = "Password";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(btnInserir);
            groupBox1.Controls.Add(txtSite);
            groupBox1.Controls.Add(txtLogin);
            groupBox1.Location = new Point(10, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(346, 84);
            groupBox1.TabIndex = 7;
            groupBox1.TabStop = false;
            // 
            // btnInserir
            // 
            btnInserir.Location = new Point(6, 47);
            btnInserir.Name = "btnInserir";
            btnInserir.Size = new Size(334, 23);
            btnInserir.TabIndex = 0;
            btnInserir.Text = "Inserir";
            btnInserir.UseVisualStyleBackColor = true;
            btnInserir.Click += btnInserir_Click;
            // 
            // PWManager
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(365, 337);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(txtPassword);
            Controls.Add(dgUser);
            Controls.Add(groupBox1);
            Name = "PWManager";
            Text = "PWManager";
            ((System.ComponentModel.ISupportInitialize)dgUser).EndInit();
            contextDelete.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dgUser;
        private TextBox txtSite;
        private TextBox txtLogin;
        private TextBox txtPassword;
        private Label label1;
        private Label label2;
        private Label label3;
        private GroupBox groupBox1;
        private Button btnInserir;
        private ContextMenuStrip contextDelete;
        private ToolStripMenuItem deleteToolStripMenuItem;
    }
}
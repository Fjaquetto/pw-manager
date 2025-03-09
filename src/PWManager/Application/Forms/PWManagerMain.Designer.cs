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
            editToolStripMenuItem = new ToolStripMenuItem();
            deleteToolStripMenuItem = new ToolStripMenuItem();
            txtSite = new TextBox();
            txtLogin = new TextBox();
            txtPassword = new TextBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            groupBox1 = new GroupBox();
            btnClear = new Button();
            btnInserir = new Button();
            groupBox2 = new GroupBox();
            label5 = new Label();
            label4 = new Label();
            txtLoginSearch = new TextBox();
            txtSiteSearch = new TextBox();
            txtGeneratePassword = new TextBox();
            btnGeneratePassword = new Button();
            ((System.ComponentModel.ISupportInitialize)dgUser).BeginInit();
            contextDelete.SuspendLayout();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // dgUser
            // 
            dgUser.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgUser.ContextMenuStrip = contextDelete;
            dgUser.Location = new Point(12, 160);
            dgUser.Name = "dgUser";
            dgUser.Size = new Size(343, 235);
            dgUser.TabIndex = 0;
            // 
            // contextDelete
            // 
            contextDelete.Items.AddRange(new ToolStripItem[] { editToolStripMenuItem, deleteToolStripMenuItem });
            contextDelete.Name = "contextDelete";
            contextDelete.Size = new Size(108, 48);
            contextDelete.Text = "Options";
            // 
            // editToolStripMenuItem
            // 
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.Size = new Size(107, 22);
            editToolStripMenuItem.Text = "Edit";
            editToolStripMenuItem.Click += editToolStripMenuItem_Click;
            // 
            // deleteToolStripMenuItem
            // 
            deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            deleteToolStripMenuItem.Size = new Size(107, 22);
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
            groupBox1.Controls.Add(btnClear);
            groupBox1.Controls.Add(btnInserir);
            groupBox1.Controls.Add(txtSite);
            groupBox1.Controls.Add(txtLogin);
            groupBox1.Location = new Point(10, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(346, 84);
            groupBox1.TabIndex = 7;
            groupBox1.TabStop = false;
            // 
            // btnClear
            // 
            btnClear.Location = new Point(66, 50);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(94, 23);
            btnClear.TabIndex = 7;
            btnClear.Text = "Clear";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            // 
            // btnInserir
            // 
            btnInserir.Location = new Point(191, 50);
            btnInserir.Name = "btnInserir";
            btnInserir.Size = new Size(94, 23);
            btnInserir.TabIndex = 6;
            btnInserir.Text = "Insert";
            btnInserir.UseVisualStyleBackColor = true;
            btnInserir.Click += btnInserir_Click;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(label5);
            groupBox2.Controls.Add(label4);
            groupBox2.Controls.Add(txtLoginSearch);
            groupBox2.Controls.Add(txtSiteSearch);
            groupBox2.Location = new Point(10, 93);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(346, 61);
            groupBox2.TabIndex = 8;
            groupBox2.TabStop = false;
            groupBox2.Text = "Search";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(6, 19);
            label5.Name = "label5";
            label5.Size = new Size(26, 15);
            label5.TabIndex = 3;
            label5.Text = "Site";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(191, 19);
            label4.Name = "label4";
            label4.Size = new Size(37, 15);
            label4.TabIndex = 2;
            label4.Text = "Login";
            // 
            // txtLoginSearch
            // 
            txtLoginSearch.Location = new Point(191, 32);
            txtLoginSearch.Name = "txtLoginSearch";
            txtLoginSearch.Size = new Size(149, 23);
            txtLoginSearch.TabIndex = 1;
            txtLoginSearch.TextChanged += txtLoginSearch_TextChanged;
            // 
            // txtSiteSearch
            // 
            txtSiteSearch.Location = new Point(6, 32);
            txtSiteSearch.Name = "txtSiteSearch";
            txtSiteSearch.Size = new Size(154, 23);
            txtSiteSearch.TabIndex = 0;
            txtSiteSearch.TextChanged += txtSiteSearch_TextChanged;
            // 
            // txtGeneratePassword
            // 
            txtGeneratePassword.Location = new Point(12, 401);
            txtGeneratePassword.Name = "txtGeneratePassword";
            txtGeneratePassword.Size = new Size(343, 23);
            txtGeneratePassword.TabIndex = 9;
            txtGeneratePassword.TextAlign = HorizontalAlignment.Center;
            // 
            // btnGeneratePassword
            // 
            btnGeneratePassword.Location = new Point(12, 430);
            btnGeneratePassword.Name = "btnGeneratePassword";
            btnGeneratePassword.Size = new Size(343, 23);
            btnGeneratePassword.TabIndex = 10;
            btnGeneratePassword.Text = "Generate Password";
            btnGeneratePassword.UseVisualStyleBackColor = true;
            btnGeneratePassword.Click += btnGeneratePassword_Click;
            // 
            // PWManager
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(365, 469);
            Controls.Add(btnGeneratePassword);
            Controls.Add(txtGeneratePassword);
            Controls.Add(groupBox2);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(txtPassword);
            Controls.Add(dgUser);
            Controls.Add(groupBox1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "PWManager";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "PWManager";
            ((System.ComponentModel.ISupportInitialize)dgUser).EndInit();
            contextDelete.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
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
        private Button btnClear;
        private Button btnInserir;
        private ContextMenuStrip contextDelete;
        private ToolStripMenuItem editToolStripMenuItem;
        private ToolStripMenuItem deleteToolStripMenuItem;
        private GroupBox groupBox2;
        private Label label5;
        private Label label4;
        private TextBox txtLoginSearch;
        private TextBox txtSiteSearch;
        private TextBox txtGeneratePassword;
        private Button btnGeneratePassword;
    }
}
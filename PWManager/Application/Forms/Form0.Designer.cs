﻿namespace PWManager.Application.Forms
{
    partial class Form0
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtEncryptor = new TextBox();
            btnOk = new Button();
            label1 = new Label();
            SuspendLayout();
            // 
            // txtEncryptor
            // 
            txtEncryptor.Location = new Point(12, 27);
            txtEncryptor.Name = "txtEncryptor";
            txtEncryptor.Size = new Size(100, 23);
            txtEncryptor.TabIndex = 0;
            // 
            // btnOk
            // 
            btnOk.Location = new Point(12, 56);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(100, 23);
            btnOk.TabIndex = 1;
            btnOk.Text = "OK";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Click += btnOk_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(57, 15);
            label1.TabIndex = 2;
            label1.Text = "Password";
            // 
            // Form0
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(124, 97);
            Controls.Add(label1);
            Controls.Add(btnOk);
            Controls.Add(txtEncryptor);
            Name = "Form0";
            Text = "Form0";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtEncryptor;
        private Button btnOk;
        private Label label1;
    }
}
namespace EmployeeManagementSystem.Forms
{
    partial class RegisterForm
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
            this.signup_showPass = new System.Windows.Forms.CheckBox();
            this.sigunp_btn = new System.Windows.Forms.Button();
            this.signup_password = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.signup_username = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.exit = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label6 = new System.Windows.Forms.Label();
            this.signup_loginBtn = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // signup_showPass
            // 
            this.signup_showPass.AutoSize = true;
            this.signup_showPass.Cursor = System.Windows.Forms.Cursors.Hand;
            this.signup_showPass.Font = new System.Drawing.Font("Tahoma", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.signup_showPass.Location = new System.Drawing.Point(567, 276);
            this.signup_showPass.Name = "signup_showPass";
            this.signup_showPass.Size = new System.Drawing.Size(120, 20);
            this.signup_showPass.TabIndex = 20;
            this.signup_showPass.Text = "Show Password";
            this.signup_showPass.UseVisualStyleBackColor = true;
            this.signup_showPass.CheckedChanged += new System.EventHandler(this.signup_showPass_CheckedChanged);
            // 
            // sigunp_btn
            // 
            this.sigunp_btn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(11)))), ((int)(((byte)(97)))));
            this.sigunp_btn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.sigunp_btn.FlatAppearance.BorderSize = 0;
            this.sigunp_btn.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(8)))), ((int)(((byte)(138)))));
            this.sigunp_btn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(8)))), ((int)(((byte)(138)))));
            this.sigunp_btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.sigunp_btn.Font = new System.Drawing.Font("Arial Rounded MT Bold", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sigunp_btn.ForeColor = System.Drawing.Color.White;
            this.sigunp_btn.Location = new System.Drawing.Point(365, 317);
            this.sigunp_btn.Name = "sigunp_btn";
            this.sigunp_btn.Size = new System.Drawing.Size(116, 48);
            this.sigunp_btn.TabIndex = 19;
            this.sigunp_btn.Text = "SIGN UP";
            this.sigunp_btn.UseVisualStyleBackColor = false;
            this.sigunp_btn.Click += new System.EventHandler(this.sigunp_btn_Click);
            // 
            // signup_password
            // 
            this.signup_password.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.signup_password.Location = new System.Drawing.Point(365, 235);
            this.signup_password.Multiline = true;
            this.signup_password.Name = "signup_password";
            this.signup_password.PasswordChar = '*';
            this.signup_password.Size = new System.Drawing.Size(322, 35);
            this.signup_password.TabIndex = 18;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Tahoma", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(361, 210);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(87, 21);
            this.label4.TabIndex = 17;
            this.label4.Text = "Password:";
            // 
            // signup_username
            // 
            this.signup_username.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.signup_username.Location = new System.Drawing.Point(365, 153);
            this.signup_username.Multiline = true;
            this.signup_username.Name = "signup_username";
            this.signup_username.Size = new System.Drawing.Size(322, 35);
            this.signup_username.TabIndex = 16;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Tahoma", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(361, 128);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(91, 21);
            this.label2.TabIndex = 15;
            this.label2.Text = "Username:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial Rounded MT Bold", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(360, 57);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(208, 27);
            this.label3.TabIndex = 14;
            this.label3.Text = "Register Account";
            // 
            // exit
            // 
            this.exit.AutoSize = true;
            this.exit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.exit.Font = new System.Drawing.Font("Tahoma", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.exit.Location = new System.Drawing.Point(691, 9);
            this.exit.Name = "exit";
            this.exit.Size = new System.Drawing.Size(22, 22);
            this.exit.TabIndex = 13;
            this.exit.Text = "X";
            this.exit.Click += new System.EventHandler(this.exit_Click);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(8)))), ((int)(((byte)(138)))));
            this.panel2.Controls.Add(this.pictureBox1);
            this.panel2.Controls.Add(this.label6);
            this.panel2.Controls.Add(this.signup_loginBtn);
            this.panel2.Controls.Add(this.label5);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(344, 452);
            this.panel2.TabIndex = 12;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::EmployeeManagementSystem.Properties.Resources.EMPL_M_S;
            this.pictureBox1.Location = new System.Drawing.Point(95, 57);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(131, 127);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Tahoma", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.White;
            this.label6.Location = new System.Drawing.Point(24, 209);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(291, 22);
            this.label6.TabIndex = 2;
            this.label6.Text = "Employee Management System";
            // 
            // signup_loginBtn
            // 
            this.signup_loginBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(11)))), ((int)(((byte)(97)))));
            this.signup_loginBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.signup_loginBtn.FlatAppearance.BorderSize = 0;
            this.signup_loginBtn.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Purple;
            this.signup_loginBtn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Purple;
            this.signup_loginBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.signup_loginBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.signup_loginBtn.ForeColor = System.Drawing.Color.White;
            this.signup_loginBtn.Location = new System.Drawing.Point(28, 377);
            this.signup_loginBtn.Name = "signup_loginBtn";
            this.signup_loginBtn.Size = new System.Drawing.Size(288, 50);
            this.signup_loginBtn.TabIndex = 1;
            this.signup_loginBtn.Text = "SIGN IN";
            this.signup_loginBtn.UseVisualStyleBackColor = false;
            this.signup_loginBtn.Click += new System.EventHandler(this.signup_loginBtn_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Tahoma", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(78, 343);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(163, 22);
            this.label5.TabIndex = 0;
            this.label5.Text = "Login your Account";
            // 
            // RegisterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.ClientSize = new System.Drawing.Size(725, 452);
            this.Controls.Add(this.signup_showPass);
            this.Controls.Add(this.sigunp_btn);
            this.Controls.Add(this.signup_password);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.signup_username);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.exit);
            this.Controls.Add(this.panel2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "RegisterForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "RegisterForm";
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox signup_showPass;
        private System.Windows.Forms.Button sigunp_btn;
        private System.Windows.Forms.TextBox signup_password;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox signup_username;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label exit;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button signup_loginBtn;
        private System.Windows.Forms.Label label5;
    }
}
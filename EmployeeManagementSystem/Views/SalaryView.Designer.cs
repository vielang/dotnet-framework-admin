namespace EmployeeManagementSystem.Views
{
    partial class SalaryView
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.salary_employeeID = new System.Windows.Forms.TextBox();
            this.salary_name = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.salary_salary = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.salary_position = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.salary_updateBtn = new System.Windows.Forms.Button();
            this.salary_clearBtn = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.salary_clearBtn);
            this.panel1.Controls.Add(this.salary_updateBtn);
            this.panel1.Controls.Add(this.salary_position);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.salary_salary);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.salary_name);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.salary_employeeID);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(20, 22);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(297, 610);
            this.panel1.TabIndex = 1;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.dataGridView1);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Location = new System.Drawing.Point(335, 22);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(570, 605);
            this.panel2.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Tahoma", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(10, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(119, 28);
            this.label2.TabIndex = 1;
            this.label2.Text = "Employees";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(18, 54);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "Employee ID:";
            // 
            // salary_employeeID
            // 
            this.salary_employeeID.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.salary_employeeID.Location = new System.Drawing.Point(21, 75);
            this.salary_employeeID.Multiline = true;
            this.salary_employeeID.Name = "salary_employeeID";
            this.salary_employeeID.Size = new System.Drawing.Size(255, 36);
            this.salary_employeeID.TabIndex = 1;
            // 
            // salary_name
            // 
            this.salary_name.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.salary_name.Location = new System.Drawing.Point(21, 153);
            this.salary_name.Multiline = true;
            this.salary_name.Name = "salary_name";
            this.salary_name.Size = new System.Drawing.Size(255, 36);
            this.salary_name.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(18, 132);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 18);
            this.label3.TabIndex = 2;
            this.label3.Text = "Full Name:";
            // 
            // salary_salary
            // 
            this.salary_salary.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.salary_salary.Location = new System.Drawing.Point(21, 309);
            this.salary_salary.Multiline = true;
            this.salary_salary.Name = "salary_salary";
            this.salary_salary.Size = new System.Drawing.Size(255, 36);
            this.salary_salary.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(18, 288);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 18);
            this.label4.TabIndex = 4;
            this.label4.Text = "Salary:";
            // 
            // salary_position
            // 
            this.salary_position.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.salary_position.Location = new System.Drawing.Point(21, 234);
            this.salary_position.Multiline = true;
            this.salary_position.Name = "salary_position";
            this.salary_position.Size = new System.Drawing.Size(255, 36);
            this.salary_position.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(18, 213);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(61, 18);
            this.label5.TabIndex = 6;
            this.label5.Text = "Position:";
            // 
            // salary_updateBtn
            // 
            this.salary_updateBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(11)))), ((int)(((byte)(97)))));
            this.salary_updateBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.salary_updateBtn.FlatAppearance.BorderSize = 0;
            this.salary_updateBtn.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(11)))), ((int)(((byte)(97)))));
            this.salary_updateBtn.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(8)))), ((int)(((byte)(138)))));
            this.salary_updateBtn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(8)))), ((int)(((byte)(138)))));
            this.salary_updateBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.salary_updateBtn.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.salary_updateBtn.ForeColor = System.Drawing.Color.White;
            this.salary_updateBtn.Location = new System.Drawing.Point(21, 351);
            this.salary_updateBtn.Name = "salary_updateBtn";
            this.salary_updateBtn.Size = new System.Drawing.Size(122, 45);
            this.salary_updateBtn.TabIndex = 14;
            this.salary_updateBtn.Text = "Update";
            this.salary_updateBtn.UseVisualStyleBackColor = false;
            this.salary_updateBtn.Click += new System.EventHandler(this.salary_updateBtn_Click);
            // 
            // salary_clearBtn
            // 
            this.salary_clearBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(11)))), ((int)(((byte)(97)))));
            this.salary_clearBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.salary_clearBtn.FlatAppearance.BorderSize = 0;
            this.salary_clearBtn.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(11)))), ((int)(((byte)(97)))));
            this.salary_clearBtn.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(8)))), ((int)(((byte)(138)))));
            this.salary_clearBtn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(8)))), ((int)(((byte)(138)))));
            this.salary_clearBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.salary_clearBtn.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.salary_clearBtn.ForeColor = System.Drawing.Color.White;
            this.salary_clearBtn.Location = new System.Drawing.Point(154, 351);
            this.salary_clearBtn.Name = "salary_clearBtn";
            this.salary_clearBtn.Size = new System.Drawing.Size(122, 45);
            this.salary_clearBtn.TabIndex = 15;
            this.salary_clearBtn.Text = "Clear";
            this.salary_clearBtn.UseVisualStyleBackColor = false;
            this.salary_clearBtn.Click += new System.EventHandler(this.salary_clearBtn_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(11)))), ((int)(((byte)(97)))));
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.EnableHeadersVisualStyles = false;
            this.dataGridView1.Location = new System.Drawing.Point(15, 75);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(11)))), ((int)(((byte)(97)))));
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.RowHeadersDefaultCellStyle = dataGridViewCellStyle5;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowHeadersWidth = 51;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.RowsDefaultCellStyle = dataGridViewCellStyle6;
            this.dataGridView1.RowTemplate.Height = 24;
            this.dataGridView1.Size = new System.Drawing.Size(537, 513);
            this.dataGridView1.TabIndex = 2;
            this.dataGridView1.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellClick);
            // 
            // Salary
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "Salary";
            this.Size = new System.Drawing.Size(923, 648);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox salary_position;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox salary_salary;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox salary_name;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox salary_employeeID;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button salary_clearBtn;
        private System.Windows.Forms.Button salary_updateBtn;
        private System.Windows.Forms.DataGridView dataGridView1;
    }
}

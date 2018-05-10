namespace vComputeServer
{
    partial class Server_form
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
            this.btnServerStart = new System.Windows.Forms.Button();
            this.logtxt = new System.Windows.Forms.RichTextBox();
            this.AssemblyList = new System.Windows.Forms.ListBox();
            this.btnShowAssemblies = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.listView1 = new System.Windows.Forms.ListView();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnServerStart
            // 
            this.btnServerStart.Location = new System.Drawing.Point(26, 500);
            this.btnServerStart.Name = "btnServerStart";
            this.btnServerStart.Size = new System.Drawing.Size(639, 42);
            this.btnServerStart.TabIndex = 0;
            this.btnServerStart.Text = "Start Server";
            this.btnServerStart.UseVisualStyleBackColor = true;
            this.btnServerStart.Click += new System.EventHandler(this.btnServerStart_Click);
            // 
            // logtxt
            // 
            this.logtxt.BackColor = System.Drawing.Color.Black;
            this.logtxt.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logtxt.ForeColor = System.Drawing.Color.Lime;
            this.logtxt.Location = new System.Drawing.Point(26, 28);
            this.logtxt.Name = "logtxt";
            this.logtxt.ReadOnly = true;
            this.logtxt.Size = new System.Drawing.Size(639, 464);
            this.logtxt.TabIndex = 1;
            this.logtxt.Text = "";
            this.logtxt.TextChanged += new System.EventHandler(this.logtxt_TextChanged);
            // 
            // AssemblyList
            // 
            this.AssemblyList.FormattingEnabled = true;
            this.AssemblyList.IntegralHeight = false;
            this.AssemblyList.Location = new System.Drawing.Point(682, 28);
            this.AssemblyList.Name = "AssemblyList";
            this.AssemblyList.Size = new System.Drawing.Size(224, 464);
            this.AssemblyList.TabIndex = 2;
            // 
            // btnShowAssemblies
            // 
            this.btnShowAssemblies.Location = new System.Drawing.Point(682, 500);
            this.btnShowAssemblies.Name = "btnShowAssemblies";
            this.btnShowAssemblies.Size = new System.Drawing.Size(224, 40);
            this.btnShowAssemblies.TabIndex = 3;
            this.btnShowAssemblies.Text = "Show Assemblies";
            this.btnShowAssemblies.UseVisualStyleBackColor = true;
            this.btnShowAssemblies.Click += new System.EventHandler(this.btnShowAssemblies_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Server Console";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label2.Location = new System.Drawing.Point(679, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Assembly List";
            // 
            // listView1
            // 
            this.listView1.CausesValidation = false;
            this.listView1.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listView1.FullRowSelect = true;
            this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listView1.Location = new System.Drawing.Point(921, 28);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Scrollable = false;
            this.listView1.ShowGroups = false;
            this.listView1.Size = new System.Drawing.Size(167, 464);
            this.listView1.TabIndex = 6;
            this.listView1.TabStop = false;
            this.listView1.TileSize = new System.Drawing.Size(167, 20);
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.List;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label3.Location = new System.Drawing.Point(918, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(86, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Clients and Load";
            // 
            // Server_form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1102, 552);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnShowAssemblies);
            this.Controls.Add(this.AssemblyList);
            this.Controls.Add(this.logtxt);
            this.Controls.Add(this.btnServerStart);
            this.Name = "Server_form";
            this.Text = "Server_form";
            this.Load += new System.EventHandler(this.Server_form_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnServerStart;
        private System.Windows.Forms.RichTextBox logtxt;
        private System.Windows.Forms.ListBox AssemblyList;
        private System.Windows.Forms.Button btnShowAssemblies;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Label label3;
    }
}
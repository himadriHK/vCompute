namespace vComputeClient
{
	partial class Form1
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
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnStartClient = new System.Windows.Forms.Button();
            this.lblclienttext = new System.Windows.Forms.Label();
            this.lblclientID = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.TxtHostName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.AssemblyList = new System.Windows.Forms.ListBox();
            this.btnshowassemblies = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(481, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(70, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Assembly List";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 84);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(106, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Write your code here";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 552);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(227, 23);
            this.button1.TabIndex = 8;
            this.button1.Text = "Upload Assembly";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 121);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(422, 379);
            this.textBox1.TabIndex = 7;
            // 
            // btnStartClient
            // 
            this.btnStartClient.Location = new System.Drawing.Point(705, 523);
            this.btnStartClient.Name = "btnStartClient";
            this.btnStartClient.Size = new System.Drawing.Size(117, 23);
            this.btnStartClient.TabIndex = 14;
            this.btnStartClient.Text = "Start Client";
            this.btnStartClient.UseVisualStyleBackColor = true;
            this.btnStartClient.Click += new System.EventHandler(this.btnStartClient_Click);
            // 
            // lblclienttext
            // 
            this.lblclienttext.AutoSize = true;
            this.lblclienttext.Location = new System.Drawing.Point(39, 20);
            this.lblclienttext.Name = "lblclienttext";
            this.lblclienttext.Size = new System.Drawing.Size(76, 13);
            this.lblclienttext.TabIndex = 15;
            this.lblclienttext.Text = "I am Client ID: ";
            // 
            // lblclientID
            // 
            this.lblclientID.AutoSize = true;
            this.lblclientID.Location = new System.Drawing.Point(130, 20);
            this.lblclientID.Name = "lblclientID";
            this.lblclientID.Size = new System.Drawing.Size(0, 13);
            this.lblclientID.TabIndex = 16;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(12, 526);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(314, 20);
            this.textBox2.TabIndex = 17;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 510);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(82, 13);
            this.label5.TabIndex = 18;
            this.label5.Text = "Assembly Name";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(12, 592);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(227, 23);
            this.button2.TabIndex = 19;
            this.button2.Text = "Request Task";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // TxtHostName
            // 
            this.TxtHostName.Location = new System.Drawing.Point(503, 523);
            this.TxtHostName.Name = "TxtHostName";
            this.TxtHostName.Size = new System.Drawing.Size(172, 20);
            this.TxtHostName.TabIndex = 20;
            this.TxtHostName.TextChanged += new System.EventHandler(this.TxtHostName_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(419, 526);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(60, 13);
            this.label6.TabIndex = 21;
            this.label6.Text = "Host Name";
            // 
            // AssemblyList
            // 
            this.AssemblyList.FormattingEnabled = true;
            this.AssemblyList.Location = new System.Drawing.Point(484, 122);
            this.AssemblyList.Name = "AssemblyList";
            this.AssemblyList.Size = new System.Drawing.Size(241, 381);
            this.AssemblyList.TabIndex = 22;
            this.AssemblyList.SelectedIndexChanged += new System.EventHandler(this.AssemblyList_SelectedIndexChanged);
            // 
            // btnshowassemblies
            // 
            this.btnshowassemblies.Location = new System.Drawing.Point(625, 80);
            this.btnshowassemblies.Name = "btnshowassemblies";
            this.btnshowassemblies.Size = new System.Drawing.Size(164, 23);
            this.btnshowassemblies.TabIndex = 23;
            this.btnshowassemblies.Text = "show available assemblies";
            this.btnshowassemblies.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnshowassemblies.UseVisualStyleBackColor = true;
            this.btnshowassemblies.Click += new System.EventHandler(this.btnshowassemblies_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(843, 645);
            this.Controls.Add(this.btnshowassemblies);
            this.Controls.Add(this.AssemblyList);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.TxtHostName);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.lblclientID);
            this.Controls.Add(this.lblclienttext);
            this.Controls.Add(this.btnStartClient);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Form1";
            this.Text = "vCompute Client";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnStartClient;
        private System.Windows.Forms.Label lblclienttext;
        private System.Windows.Forms.Label lblclientID;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox TxtHostName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ListBox AssemblyList;
        private System.Windows.Forms.Button btnshowassemblies;
    }
}


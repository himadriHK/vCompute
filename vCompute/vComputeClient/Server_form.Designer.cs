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
            this.SuspendLayout();
            // 
            // btnServerStart
            // 
            this.btnServerStart.Location = new System.Drawing.Point(26, 536);
            this.btnServerStart.Name = "btnServerStart";
            this.btnServerStart.Size = new System.Drawing.Size(101, 39);
            this.btnServerStart.TabIndex = 0;
            this.btnServerStart.Text = "Start Server";
            this.btnServerStart.UseVisualStyleBackColor = true;
            this.btnServerStart.Click += new System.EventHandler(this.btnServerStart_Click);
            // 
            // logtxt
            // 
            this.logtxt.Location = new System.Drawing.Point(26, 28);
            this.logtxt.Name = "logtxt";
            this.logtxt.Size = new System.Drawing.Size(639, 464);
            this.logtxt.TabIndex = 1;
            this.logtxt.Text = "";
            // 
            // Server_form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(749, 603);
            this.Controls.Add(this.logtxt);
            this.Controls.Add(this.btnServerStart);
            this.Name = "Server_form";
            this.Text = "Server_form";
            this.Load += new System.EventHandler(this.Server_form_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnServerStart;
        private System.Windows.Forms.RichTextBox logtxt;
    }
}
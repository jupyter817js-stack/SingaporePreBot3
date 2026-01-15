
namespace SingaporePreBot3
{
    partial class frmTest
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
            this.btnURL = new System.Windows.Forms.Button();
            this.txtData = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnScript = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnURL
            // 
            this.btnURL.Location = new System.Drawing.Point(30, 84);
            this.btnURL.Name = "btnURL";
            this.btnURL.Size = new System.Drawing.Size(110, 25);
            this.btnURL.TabIndex = 2;
            this.btnURL.Text = "Go To URL";
            this.btnURL.UseVisualStyleBackColor = true;
            this.btnURL.Click += new System.EventHandler(this.btnURL_Click);
            // 
            // txtData
            // 
            this.txtData.Location = new System.Drawing.Point(81, 25);
            this.txtData.Name = "txtData";
            this.txtData.Size = new System.Drawing.Size(441, 20);
            this.txtData.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 28);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Data";
            // 
            // btnScript
            // 
            this.btnScript.Location = new System.Drawing.Point(179, 84);
            this.btnScript.Name = "btnScript";
            this.btnScript.Size = new System.Drawing.Size(110, 25);
            this.btnScript.TabIndex = 5;
            this.btnScript.Text = "Execute Script";
            this.btnScript.UseVisualStyleBackColor = true;
            this.btnScript.Click += new System.EventHandler(this.btnScript_Click);
            // 
            // frmTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(556, 149);
            this.Controls.Add(this.btnScript);
            this.Controls.Add(this.txtData);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnURL);
            this.Name = "frmTest";
            this.Text = "Test";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnURL;
        private System.Windows.Forms.TextBox txtData;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnScript;
    }
}
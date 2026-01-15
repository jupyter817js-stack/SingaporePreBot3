
namespace SingaporePreBot3
{
    partial class frmRelation
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
            this.btnDisMatch = new System.Windows.Forms.Button();
            this.btnMatch = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.dgvRelation = new System.Windows.Forms.DataGridView();
            this.dgvTwo = new System.Windows.Forms.DataGridView();
            this.dgvOne = new System.Windows.Forms.DataGridView();
            this.lblRelation = new System.Windows.Forms.Label();
            this.lblSite2 = new System.Windows.Forms.Label();
            this.lblSite1 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbPair = new System.Windows.Forms.ComboBox();
            this.Column9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column11 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column12 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column13 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column14 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column16 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column17 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column18 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column19 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column21 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column22 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column23 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column24 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column26 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column27 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column28 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRelation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTwo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOne)).BeginInit();
            this.SuspendLayout();
            // 
            // btnDisMatch
            // 
            this.btnDisMatch.Location = new System.Drawing.Point(424, 38);
            this.btnDisMatch.Name = "btnDisMatch";
            this.btnDisMatch.Size = new System.Drawing.Size(86, 23);
            this.btnDisMatch.TabIndex = 21;
            this.btnDisMatch.Text = "UnMatch";
            this.btnDisMatch.UseVisualStyleBackColor = true;
            this.btnDisMatch.Click += new System.EventHandler(this.btnDisMatch_Click);
            // 
            // btnMatch
            // 
            this.btnMatch.Location = new System.Drawing.Point(330, 38);
            this.btnMatch.Name = "btnMatch";
            this.btnMatch.Size = new System.Drawing.Size(86, 23);
            this.btnMatch.TabIndex = 20;
            this.btnMatch.Text = "Match";
            this.btnMatch.UseVisualStyleBackColor = true;
            this.btnMatch.Click += new System.EventHandler(this.btnMatch_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(236, 38);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(86, 23);
            this.btnRefresh.TabIndex = 19;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // dgvRelation
            // 
            this.dgvRelation.AllowUserToAddRows = false;
            this.dgvRelation.AllowUserToDeleteRows = false;
            this.dgvRelation.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvRelation.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRelation.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column19,
            this.Column21,
            this.Column22,
            this.Column23,
            this.Column24,
            this.Column26,
            this.Column27,
            this.Column28});
            this.dgvRelation.Location = new System.Drawing.Point(528, 64);
            this.dgvRelation.Name = "dgvRelation";
            this.dgvRelation.RowHeadersVisible = false;
            this.dgvRelation.Size = new System.Drawing.Size(934, 515);
            this.dgvRelation.TabIndex = 18;
            // 
            // dgvTwo
            // 
            this.dgvTwo.AllowUserToAddRows = false;
            this.dgvTwo.AllowUserToDeleteRows = false;
            this.dgvTwo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.dgvTwo.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTwo.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column14,
            this.Column16,
            this.Column17,
            this.Column18});
            this.dgvTwo.Location = new System.Drawing.Point(25, 266);
            this.dgvTwo.Name = "dgvTwo";
            this.dgvTwo.ReadOnly = true;
            this.dgvTwo.RowHeadersVisible = false;
            this.dgvTwo.Size = new System.Drawing.Size(485, 313);
            this.dgvTwo.TabIndex = 17;
            // 
            // dgvOne
            // 
            this.dgvOne.AllowUserToAddRows = false;
            this.dgvOne.AllowUserToDeleteRows = false;
            this.dgvOne.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvOne.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column9,
            this.Column11,
            this.Column12,
            this.Column13});
            this.dgvOne.Location = new System.Drawing.Point(25, 64);
            this.dgvOne.Name = "dgvOne";
            this.dgvOne.ReadOnly = true;
            this.dgvOne.RowHeadersVisible = false;
            this.dgvOne.Size = new System.Drawing.Size(485, 178);
            this.dgvOne.TabIndex = 16;
            // 
            // lblRelation
            // 
            this.lblRelation.AutoSize = true;
            this.lblRelation.Location = new System.Drawing.Point(525, 48);
            this.lblRelation.Name = "lblRelation";
            this.lblRelation.Size = new System.Drawing.Size(65, 13);
            this.lblRelation.TabIndex = 15;
            this.lblRelation.Text = "Relation List";
            // 
            // lblSite2
            // 
            this.lblSite2.AutoSize = true;
            this.lblSite2.Location = new System.Drawing.Point(22, 250);
            this.lblSite2.Name = "lblSite2";
            this.lblSite2.Size = new System.Drawing.Size(49, 13);
            this.lblSite2.TabIndex = 14;
            this.lblSite2.Text = "Site Two";
            // 
            // lblSite1
            // 
            this.lblSite1.AutoSize = true;
            this.lblSite1.Location = new System.Drawing.Point(22, 48);
            this.lblSite1.Name = "lblSite1";
            this.lblSite1.Size = new System.Drawing.Size(48, 13);
            this.lblSite1.TabIndex = 13;
            this.lblSite1.Text = "Site One";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "Site Pair";
            // 
            // cmbPair
            // 
            this.cmbPair.FormattingEnabled = true;
            this.cmbPair.Location = new System.Drawing.Point(95, 20);
            this.cmbPair.Name = "cmbPair";
            this.cmbPair.Size = new System.Drawing.Size(121, 21);
            this.cmbPair.TabIndex = 11;
            this.cmbPair.SelectedIndexChanged += new System.EventHandler(this.cmbPair_SelectedIndexChanged);
            // 
            // Column9
            // 
            this.Column9.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Column9.HeaderText = "League";
            this.Column9.Name = "Column9";
            this.Column9.ReadOnly = true;
            // 
            // Column11
            // 
            this.Column11.FillWeight = 45F;
            this.Column11.HeaderText = "Score";
            this.Column11.Name = "Column11";
            this.Column11.ReadOnly = true;
            this.Column11.Width = 45;
            // 
            // Column12
            // 
            this.Column12.FillWeight = 150F;
            this.Column12.HeaderText = "Home";
            this.Column12.Name = "Column12";
            this.Column12.ReadOnly = true;
            this.Column12.Width = 150;
            // 
            // Column13
            // 
            this.Column13.FillWeight = 150F;
            this.Column13.HeaderText = "Away";
            this.Column13.Name = "Column13";
            this.Column13.ReadOnly = true;
            this.Column13.Width = 150;
            // 
            // Column14
            // 
            this.Column14.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Column14.HeaderText = "League";
            this.Column14.Name = "Column14";
            this.Column14.ReadOnly = true;
            // 
            // Column16
            // 
            this.Column16.FillWeight = 45F;
            this.Column16.HeaderText = "Score";
            this.Column16.Name = "Column16";
            this.Column16.ReadOnly = true;
            this.Column16.Width = 45;
            // 
            // Column17
            // 
            this.Column17.FillWeight = 150F;
            this.Column17.HeaderText = "Home";
            this.Column17.Name = "Column17";
            this.Column17.ReadOnly = true;
            this.Column17.Width = 150;
            // 
            // Column18
            // 
            this.Column18.FillWeight = 150F;
            this.Column18.HeaderText = "Away";
            this.Column18.Name = "Column18";
            this.Column18.ReadOnly = true;
            this.Column18.Width = 150;
            // 
            // Column19
            // 
            this.Column19.FillWeight = 120F;
            this.Column19.HeaderText = "League1";
            this.Column19.Name = "Column19";
            this.Column19.Width = 120;
            // 
            // Column21
            // 
            this.Column21.FillWeight = 45F;
            this.Column21.HeaderText = "Score1";
            this.Column21.Name = "Column21";
            this.Column21.Width = 45;
            // 
            // Column22
            // 
            this.Column22.FillWeight = 150F;
            this.Column22.HeaderText = "Home1";
            this.Column22.Name = "Column22";
            this.Column22.Width = 150;
            // 
            // Column23
            // 
            this.Column23.FillWeight = 150F;
            this.Column23.HeaderText = "Away1";
            this.Column23.Name = "Column23";
            this.Column23.Width = 150;
            // 
            // Column24
            // 
            this.Column24.FillWeight = 120F;
            this.Column24.HeaderText = "League2";
            this.Column24.Name = "Column24";
            this.Column24.Width = 120;
            // 
            // Column26
            // 
            this.Column26.FillWeight = 45F;
            this.Column26.HeaderText = "Score2";
            this.Column26.Name = "Column26";
            this.Column26.Width = 45;
            // 
            // Column27
            // 
            this.Column27.FillWeight = 150F;
            this.Column27.HeaderText = "Home2";
            this.Column27.Name = "Column27";
            this.Column27.Width = 150;
            // 
            // Column28
            // 
            this.Column28.FillWeight = 150F;
            this.Column28.HeaderText = "Away2";
            this.Column28.Name = "Column28";
            this.Column28.Width = 150;
            // 
            // frmRelation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1484, 633);
            this.Controls.Add(this.btnDisMatch);
            this.Controls.Add(this.btnMatch);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.dgvRelation);
            this.Controls.Add(this.dgvTwo);
            this.Controls.Add(this.dgvOne);
            this.Controls.Add(this.lblRelation);
            this.Controls.Add(this.lblSite2);
            this.Controls.Add(this.lblSite1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbPair);
            this.Name = "frmRelation";
            this.Text = "Relation";
            this.Load += new System.EventHandler(this.frmRelation_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvRelation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTwo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOne)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnDisMatch;
        private System.Windows.Forms.Button btnMatch;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.DataGridView dgvRelation;
        private System.Windows.Forms.DataGridView dgvTwo;
        private System.Windows.Forms.DataGridView dgvOne;
        private System.Windows.Forms.Label lblRelation;
        private System.Windows.Forms.Label lblSite2;
        private System.Windows.Forms.Label lblSite1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbPair;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column19;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column21;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column22;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column23;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column24;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column26;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column27;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column28;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column14;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column16;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column17;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column18;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column9;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column11;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column12;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column13;
    }
}
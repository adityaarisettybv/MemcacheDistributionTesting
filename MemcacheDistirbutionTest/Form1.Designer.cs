namespace MemcacheDistirbutionTest
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
            this.txtResults = new System.Windows.Forms.TextBox();
            this.btnDefaultNodeLocatorDefaultHasing = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtResults
            // 
            this.txtResults.Location = new System.Drawing.Point(420, 25);
            this.txtResults.Multiline = true;
            this.txtResults.Name = "txtResults";
            this.txtResults.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtResults.Size = new System.Drawing.Size(691, 481);
            this.txtResults.TabIndex = 1;
            // 
            // btnDefaultNodeLocatorDefaultHasing
            // 
            this.btnDefaultNodeLocatorDefaultHasing.Location = new System.Drawing.Point(6, 32);
            this.btnDefaultNodeLocatorDefaultHasing.Name = "btnDefaultNodeLocatorDefaultHasing";
            this.btnDefaultNodeLocatorDefaultHasing.Size = new System.Drawing.Size(180, 23);
            this.btnDefaultNodeLocatorDefaultHasing.TabIndex = 0;
            this.btnDefaultNodeLocatorDefaultHasing.Text = "Default Node Locator Hasing Test";
            this.btnDefaultNodeLocatorDefaultHasing.UseVisualStyleBackColor = true;
            this.btnDefaultNodeLocatorDefaultHasing.Click += new System.EventHandler(this.btnDefaultNodeLocatorDefaultHasing_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnDefaultNodeLocatorDefaultHasing);
            this.groupBox1.Location = new System.Drawing.Point(12, 25);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(375, 258);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Default Node Locator";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1123, 530);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.txtResults);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtResults;
        private System.Windows.Forms.Button btnDefaultNodeLocatorDefaultHasing;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}


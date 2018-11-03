
namespace GABAK
{
    partial class AboutForm
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
            this.labelALO = new System.Windows.Forms.Label();
            this.panelTop = new System.Windows.Forms.Panel();
            this.labelVersion = new System.Windows.Forms.Label();
            this.labelCopyright = new System.Windows.Forms.Label();
            this.labelRights = new System.Windows.Forms.Label();
            this.labelContactInfo = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelALO
            // 
            this.labelALO.AutoSize = true;
            this.labelALO.Location = new System.Drawing.Point(12, 97);
            this.labelALO.Name = "labelALO";
            this.labelALO.Size = new System.Drawing.Size(180, 13);
            this.labelALO.TabIndex = 0;
            this.labelALO.Text = "GABAK Warehouse Layout Optimizer";
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.SystemColors.Window;
            this.panelTop.Location = new System.Drawing.Point(0, 2);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(286, 92);
            this.panelTop.TabIndex = 1;
            // 
            // labelVersion
            // 
            this.labelVersion.AutoSize = true;
            this.labelVersion.Location = new System.Drawing.Point(12, 110);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(60, 13);
            this.labelVersion.TabIndex = 2;
            this.labelVersion.Text = "Version 1.0";
            // 
            // labelCopyright
            // 
            this.labelCopyright.AutoSize = true;
            this.labelCopyright.Location = new System.Drawing.Point(12, 123);
            this.labelCopyright.Name = "labelCopyright";
            this.labelCopyright.Size = new System.Drawing.Size(202, 13);
            this.labelCopyright.TabIndex = 3;
            this.labelCopyright.Text = "Developed By Sabahattin Gökhan Özden";
            // 
            // labelRights
            // 
            this.labelRights.AutoSize = true;
            this.labelRights.Location = new System.Drawing.Point(12, 136);
            this.labelRights.Name = "labelRights";
            this.labelRights.Size = new System.Drawing.Size(63, 13);
            this.labelRights.TabIndex = 4;
            this.labelRights.Text = "MIT License";
            // 
            // labelContactInfo
            // 
            this.labelContactInfo.AutoSize = true;
            this.labelContactInfo.Location = new System.Drawing.Point(12, 149);
            this.labelContactInfo.Name = "labelContactInfo";
            this.labelContactInfo.Size = new System.Drawing.Size(135, 13);
            this.labelContactInfo.TabIndex = 5;
            this.labelContactInfo.Text = "gokhan.ozden@yahoo.com";
            // 
            // AboutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 241);
            this.Controls.Add(this.labelContactInfo);
            this.Controls.Add(this.labelRights);
            this.Controls.Add(this.labelCopyright);
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.labelALO);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.ShowIcon = false;
            this.Text = "About GABAK";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelALO;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Label labelCopyright;
        private System.Windows.Forms.Label labelRights;
        private System.Windows.Forms.Label labelContactInfo;
    }
}
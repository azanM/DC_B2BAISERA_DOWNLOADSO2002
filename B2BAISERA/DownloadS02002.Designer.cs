﻿namespace B2BAISERA
{
    partial class DownloadS02002
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
            this.LblMessage = new System.Windows.Forms.Label();
            this.LblTicketNo = new System.Windows.Forms.Label();
            this.LblAcknowledge = new System.Windows.Forms.Label();
            this.LblResult = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // LblMessage
            // 
            this.LblMessage.AutoSize = true;
            this.LblMessage.Location = new System.Drawing.Point(28, 135);
            this.LblMessage.Name = "LblMessage";
            this.LblMessage.Size = new System.Drawing.Size(64, 13);
            this.LblMessage.TabIndex = 18;
            this.LblMessage.Text = "LblMessage";
            // 
            // LblTicketNo
            // 
            this.LblTicketNo.AutoSize = true;
            this.LblTicketNo.Location = new System.Drawing.Point(28, 102);
            this.LblTicketNo.Name = "LblTicketNo";
            this.LblTicketNo.Size = new System.Drawing.Size(65, 13);
            this.LblTicketNo.TabIndex = 17;
            this.LblTicketNo.Text = "LblTicketNo";
            // 
            // LblAcknowledge
            // 
            this.LblAcknowledge.AutoSize = true;
            this.LblAcknowledge.Location = new System.Drawing.Point(28, 72);
            this.LblAcknowledge.Name = "LblAcknowledge";
            this.LblAcknowledge.Size = new System.Drawing.Size(86, 13);
            this.LblAcknowledge.TabIndex = 16;
            this.LblAcknowledge.Text = "LblAcknowledge";
            // 
            // LblResult
            // 
            this.LblResult.AutoSize = true;
            this.LblResult.Location = new System.Drawing.Point(28, 42);
            this.LblResult.Name = "LblResult";
            this.LblResult.Size = new System.Drawing.Size(51, 13);
            this.LblResult.TabIndex = 15;
            this.LblResult.Text = "LblResult";
            // 
            // DownloadS02002
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.LblMessage);
            this.Controls.Add(this.LblTicketNo);
            this.Controls.Add(this.LblAcknowledge);
            this.Controls.Add(this.LblResult);
            this.Name = "DownloadS02002";
            this.Text = "DownloadS02002";
            this.Load += new System.EventHandler(this.DownloadS02002_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label LblMessage;
        private System.Windows.Forms.Label LblTicketNo;
        private System.Windows.Forms.Label LblAcknowledge;
        private System.Windows.Forms.Label LblResult;
    }
}
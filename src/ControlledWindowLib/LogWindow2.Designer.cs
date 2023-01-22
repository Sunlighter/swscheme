namespace ControlledWindowLib
{
    partial class LogWindow2
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
            this.listView1 = new System.Windows.Forms.ListView();
            this.chNumber = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chMessage = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panel1 = new System.Windows.Forms.Panel();
            this.cbScrollToEnd = new System.Windows.Forms.CheckBox();
            this.buttonPause = new System.Windows.Forms.Button();
            this.buttonCopy = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chNumber,
            this.chMessage});
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.FullRowSelect = true;
            this.listView1.Location = new System.Drawing.Point(0, 0);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(381, 312);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.VirtualMode = true;
            this.listView1.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.listView1_RetrieveVirtualItem);
            this.listView1.ClientSizeChanged += new System.EventHandler(this.listView1_ClientSizeChanged);
            // 
            // chNumber
            // 
            this.chNumber.Text = "Number";
            this.chNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // chMessage
            // 
            this.chMessage.Text = "Message";
            this.chMessage.Width = 300;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.cbScrollToEnd);
            this.panel1.Controls.Add(this.buttonPause);
            this.panel1.Controls.Add(this.buttonCopy);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 312);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(381, 34);
            this.panel1.TabIndex = 1;
            // 
            // cbScrollToEnd
            // 
            this.cbScrollToEnd.AutoSize = true;
            this.cbScrollToEnd.Checked = true;
            this.cbScrollToEnd.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbScrollToEnd.Location = new System.Drawing.Point(16, 8);
            this.cbScrollToEnd.Name = "cbScrollToEnd";
            this.cbScrollToEnd.Size = new System.Drawing.Size(86, 17);
            this.cbScrollToEnd.TabIndex = 2;
            this.cbScrollToEnd.Text = "Scroll to End";
            this.cbScrollToEnd.UseVisualStyleBackColor = true;
            // 
            // buttonPause
            // 
            this.buttonPause.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.buttonPause.Location = new System.Drawing.Point(112, 4);
            this.buttonPause.Name = "buttonPause";
            this.buttonPause.Size = new System.Drawing.Size(76, 24);
            this.buttonPause.TabIndex = 1;
            this.buttonPause.Text = "&Pause";
            this.buttonPause.UseVisualStyleBackColor = true;
            this.buttonPause.Click += new System.EventHandler(this.buttonPause_Click);
            // 
            // buttonCopy
            // 
            this.buttonCopy.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.buttonCopy.Location = new System.Drawing.Point(192, 4);
            this.buttonCopy.Name = "buttonCopy";
            this.buttonCopy.Size = new System.Drawing.Size(76, 24);
            this.buttonCopy.TabIndex = 0;
            this.buttonCopy.Text = "&Copy";
            this.buttonCopy.UseVisualStyleBackColor = true;
            this.buttonCopy.Click += new System.EventHandler(this.buttonCopy_Click);
            // 
            // LogWindow2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(381, 346);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.panel1);
            this.Name = "LogWindow2";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "LogWindow2";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LogWindow2_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.LogWindow2_FormClosed);
            this.Load += new System.EventHandler(this.LogWindow2_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button buttonCopy;
        private System.Windows.Forms.ColumnHeader chNumber;
        private System.Windows.Forms.ColumnHeader chMessage;
        private System.Windows.Forms.Button buttonPause;
        private System.Windows.Forms.CheckBox cbScrollToEnd;
    }
}
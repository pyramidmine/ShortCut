namespace TabControlTest
{
	partial class MainForm
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
			this.tabControlQuery = new System.Windows.Forms.TabControl();
			this.tabPage0 = new System.Windows.Forms.TabPage();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.buttonFetchSize = new System.Windows.Forms.Button();
			this.listBoxLog = new System.Windows.Forms.ListBox();
			this.textBoxFetchSize0 = new System.Windows.Forms.TextBox();
			this.textBoxFetchSize1 = new System.Windows.Forms.TextBox();
			this.tabControlQuery.SuspendLayout();
			this.tabPage0.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControlQuery
			// 
			this.tabControlQuery.Controls.Add(this.tabPage0);
			this.tabControlQuery.Controls.Add(this.tabPage1);
			this.tabControlQuery.Location = new System.Drawing.Point(12, 12);
			this.tabControlQuery.Name = "tabControlQuery";
			this.tabControlQuery.SelectedIndex = 0;
			this.tabControlQuery.Size = new System.Drawing.Size(541, 224);
			this.tabControlQuery.TabIndex = 0;
			// 
			// tabPage0
			// 
			this.tabPage0.Controls.Add(this.textBoxFetchSize0);
			this.tabPage0.Location = new System.Drawing.Point(4, 22);
			this.tabPage0.Name = "tabPage0";
			this.tabPage0.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage0.Size = new System.Drawing.Size(533, 198);
			this.tabPage0.TabIndex = 0;
			this.tabPage0.Text = "Query #1";
			this.tabPage0.UseVisualStyleBackColor = true;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.textBoxFetchSize1);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(533, 198);
			this.tabPage1.TabIndex = 1;
			this.tabPage1.Text = "Query #2";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// buttonFetchSize
			// 
			this.buttonFetchSize.Location = new System.Drawing.Point(12, 243);
			this.buttonFetchSize.Name = "buttonFetchSize";
			this.buttonFetchSize.Size = new System.Drawing.Size(75, 23);
			this.buttonFetchSize.TabIndex = 1;
			this.buttonFetchSize.Text = "Fetch Size";
			this.buttonFetchSize.UseVisualStyleBackColor = true;
			this.buttonFetchSize.Click += new System.EventHandler(this.ButtonFetchSize_Click);
			// 
			// listBoxLog
			// 
			this.listBoxLog.FormattingEnabled = true;
			this.listBoxLog.ItemHeight = 12;
			this.listBoxLog.Location = new System.Drawing.Point(13, 273);
			this.listBoxLog.Name = "listBoxLog";
			this.listBoxLog.Size = new System.Drawing.Size(540, 172);
			this.listBoxLog.TabIndex = 2;
			// 
			// textBoxFetchSize0
			// 
			this.textBoxFetchSize0.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::TabControlTest.Properties.Settings.Default, "FetchSize0", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
			this.textBoxFetchSize0.Location = new System.Drawing.Point(7, 7);
			this.textBoxFetchSize0.Name = "textBoxFetchSize0";
			this.textBoxFetchSize0.Size = new System.Drawing.Size(100, 21);
			this.textBoxFetchSize0.TabIndex = 0;
			this.textBoxFetchSize0.Text = global::TabControlTest.Properties.Settings.Default.FetchSize0;
			// 
			// textBoxFetchSize1
			// 
			this.textBoxFetchSize1.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::TabControlTest.Properties.Settings.Default, "FetchSize1", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
			this.textBoxFetchSize1.Location = new System.Drawing.Point(7, 7);
			this.textBoxFetchSize1.Name = "textBoxFetchSize1";
			this.textBoxFetchSize1.Size = new System.Drawing.Size(100, 21);
			this.textBoxFetchSize1.TabIndex = 0;
			this.textBoxFetchSize1.Text = global::TabControlTest.Properties.Settings.Default.FetchSize1;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(565, 456);
			this.Controls.Add(this.listBoxLog);
			this.Controls.Add(this.buttonFetchSize);
			this.Controls.Add(this.tabControlQuery);
			this.Name = "MainForm";
			this.Text = "MainForm";
			this.tabControlQuery.ResumeLayout(false);
			this.tabPage0.ResumeLayout(false);
			this.tabPage0.PerformLayout();
			this.tabPage1.ResumeLayout(false);
			this.tabPage1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl tabControlQuery;
		private System.Windows.Forms.TabPage tabPage0;
		private System.Windows.Forms.TextBox textBoxFetchSize0;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TextBox textBoxFetchSize1;
		private System.Windows.Forms.Button buttonFetchSize;
		private System.Windows.Forms.ListBox listBoxLog;
	}
}


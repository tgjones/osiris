namespace Osiris.Builder
{
	partial class OsirisBuilderForm
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
			this.pnlXna = new System.Windows.Forms.Panel();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.prgObjectProperties = new System.Windows.Forms.PropertyGrid();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.trvObjects = new System.Windows.Forms.TreeView();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.SuspendLayout();
			// 
			// pnlXna
			// 
			this.pnlXna.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlXna.Location = new System.Drawing.Point(0, 0);
			this.pnlXna.Name = "pnlXna";
			this.pnlXna.Size = new System.Drawing.Size(518, 564);
			this.pnlXna.TabIndex = 0;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.pnlXna);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
			this.splitContainer1.Size = new System.Drawing.Size(784, 564);
			this.splitContainer1.SplitterDistance = 518;
			this.splitContainer1.TabIndex = 1;
			// 
			// prgObjectProperties
			// 
			this.prgObjectProperties.Dock = System.Windows.Forms.DockStyle.Fill;
			this.prgObjectProperties.Location = new System.Drawing.Point(0, 0);
			this.prgObjectProperties.Name = "prgObjectProperties";
			this.prgObjectProperties.Size = new System.Drawing.Size(262, 315);
			this.prgObjectProperties.TabIndex = 0;
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = new System.Drawing.Point(0, 0);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.tabControl1);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.prgObjectProperties);
			this.splitContainer2.Size = new System.Drawing.Size(262, 564);
			this.splitContainer2.SplitterDistance = 245;
			this.splitContainer2.TabIndex = 3;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.trvObjects);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(254, 219);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Components";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// trvObjects
			// 
			this.trvObjects.Dock = System.Windows.Forms.DockStyle.Fill;
			this.trvObjects.Location = new System.Drawing.Point(3, 3);
			this.trvObjects.Name = "trvObjects";
			this.trvObjects.Size = new System.Drawing.Size(248, 213);
			this.trvObjects.TabIndex = 2;
			this.trvObjects.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.trvObjects_AfterSelect);
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Location = new System.Drawing.Point(0, 0);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(262, 245);
			this.tabControl1.TabIndex = 3;
			// 
			// OsirisBuilderForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(784, 564);
			this.Controls.Add(this.splitContainer1);
			this.Name = "OsirisBuilderForm";
			this.Text = "Form1";
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			this.splitContainer2.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabControl1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel pnlXna;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.PropertyGrid prgObjectProperties;
		private System.Windows.Forms.SplitContainer splitContainer2;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TreeView trvObjects;
	}
}


namespace LinqVecDemo
{
	partial class MainWin
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
			dockPanel = new WeifenLuo.WinFormsUI.Docking.DockPanel();
			vS2015BlueTheme = new WeifenLuo.WinFormsUI.Docking.VS2015BlueTheme();
			menuStrip = new MenuStrip();
			fileToolStripMenuItem = new ToolStripMenuItem();
			menuFileNew = new ToolStripMenuItem();
			menuFileOpen = new ToolStripMenuItem();
			toolStripSeparator1 = new ToolStripSeparator();
			menuFileSave = new ToolStripMenuItem();
			menuFileSaveAs = new ToolStripMenuItem();
			toolStripSeparator2 = new ToolStripSeparator();
			menuFileExit = new ToolStripMenuItem();
			statusStrip = new StatusStrip();
			viewToolStripMenuItem = new ToolStripMenuItem();
			menuViewLayout = new ToolStripMenuItem();
			menuViewTools = new ToolStripMenuItem();
			menuStrip.SuspendLayout();
			SuspendLayout();
			// 
			// dockPanel
			// 
			dockPanel.Dock = DockStyle.Fill;
			dockPanel.DockBackColor = Color.FromArgb(41, 57, 85);
			dockPanel.Location = new Point(0, 24);
			dockPanel.Name = "dockPanel";
			dockPanel.Padding = new Padding(6);
			dockPanel.ShowAutoHideContentOnHover = false;
			dockPanel.Size = new Size(800, 404);
			dockPanel.TabIndex = 0;
			dockPanel.Theme = vS2015BlueTheme;
			// 
			// menuStrip
			// 
			menuStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, viewToolStripMenuItem });
			menuStrip.Location = new Point(0, 0);
			menuStrip.Name = "menuStrip";
			menuStrip.Size = new Size(800, 24);
			menuStrip.TabIndex = 1;
			menuStrip.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { menuFileNew, menuFileOpen, toolStripSeparator1, menuFileSave, menuFileSaveAs, toolStripSeparator2, menuFileExit });
			fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			fileToolStripMenuItem.Size = new Size(37, 20);
			fileToolStripMenuItem.Text = "&File";
			// 
			// menuFileNew
			// 
			menuFileNew.Name = "menuFileNew";
			menuFileNew.ShortcutKeys = Keys.Control | Keys.N;
			menuFileNew.Size = new Size(180, 22);
			menuFileNew.Text = "&New";
			// 
			// menuFileOpen
			// 
			menuFileOpen.Name = "menuFileOpen";
			menuFileOpen.ShortcutKeys = Keys.Control | Keys.O;
			menuFileOpen.Size = new Size(180, 22);
			menuFileOpen.Text = "&Open...";
			// 
			// toolStripSeparator1
			// 
			toolStripSeparator1.Name = "toolStripSeparator1";
			toolStripSeparator1.Size = new Size(177, 6);
			// 
			// menuFileSave
			// 
			menuFileSave.Name = "menuFileSave";
			menuFileSave.ShortcutKeys = Keys.Control | Keys.S;
			menuFileSave.Size = new Size(180, 22);
			menuFileSave.Text = "&Save";
			// 
			// menuFileSaveAs
			// 
			menuFileSaveAs.Name = "menuFileSaveAs";
			menuFileSaveAs.Size = new Size(180, 22);
			menuFileSaveAs.Text = "Save &As...";
			// 
			// toolStripSeparator2
			// 
			toolStripSeparator2.Name = "toolStripSeparator2";
			toolStripSeparator2.Size = new Size(177, 6);
			// 
			// menuFileExit
			// 
			menuFileExit.Name = "menuFileExit";
			menuFileExit.Size = new Size(180, 22);
			menuFileExit.Text = "E&xit";
			// 
			// statusStrip
			// 
			statusStrip.Location = new Point(0, 428);
			statusStrip.Name = "statusStrip";
			statusStrip.Size = new Size(800, 22);
			statusStrip.TabIndex = 2;
			statusStrip.Text = "statusStrip1";
			// 
			// viewToolStripMenuItem
			// 
			viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { menuViewLayout, menuViewTools });
			viewToolStripMenuItem.Name = "viewToolStripMenuItem";
			viewToolStripMenuItem.Size = new Size(44, 20);
			viewToolStripMenuItem.Text = "&View";
			// 
			// menuViewLayout
			// 
			menuViewLayout.Name = "menuViewLayout";
			menuViewLayout.ShortcutKeys = Keys.Control | Keys.L;
			menuViewLayout.Size = new Size(180, 22);
			menuViewLayout.Text = "&Layout";
			// 
			// menuViewTools
			// 
			menuViewTools.Name = "menuViewTools";
			menuViewTools.ShortcutKeys = Keys.Control | Keys.T;
			menuViewTools.Size = new Size(180, 22);
			menuViewTools.Text = "&Tools";
			// 
			// MainWin
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 450);
			Controls.Add(dockPanel);
			Controls.Add(statusStrip);
			Controls.Add(menuStrip);
			MainMenuStrip = menuStrip;
			Name = "MainWin";
			Text = "LinqVec";
			menuStrip.ResumeLayout(false);
			menuStrip.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private WeifenLuo.WinFormsUI.Docking.VS2015BlueTheme vS2015BlueTheme;
		private MenuStrip menuStrip;
		private ToolStripMenuItem fileToolStripMenuItem;
		private ToolStripSeparator toolStripSeparator1;
		private ToolStripSeparator toolStripSeparator2;
		public WeifenLuo.WinFormsUI.Docking.DockPanel dockPanel;
		public ToolStripMenuItem menuFileNew;
		public ToolStripMenuItem menuFileOpen;
		public ToolStripMenuItem menuFileSave;
		public ToolStripMenuItem menuFileSaveAs;
		public ToolStripMenuItem menuFileExit;
		public StatusStrip statusStrip;
		private ToolStripMenuItem viewToolStripMenuItem;
		public ToolStripMenuItem menuViewLayout;
		public ToolStripMenuItem menuViewTools;
		private ToolStripMenuItem toolboxToolStripMenuItem;
	}
}
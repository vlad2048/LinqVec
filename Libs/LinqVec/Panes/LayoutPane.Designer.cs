namespace LinqVec.Panes
{
	partial class LayoutPane
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
			components = new System.ComponentModel.Container();
			layoutTree = new BrightIdeasSoftware.TreeListView();
			((System.ComponentModel.ISupportInitialize)layoutTree).BeginInit();
			SuspendLayout();
			// 
			// layoutTree
			// 
			layoutTree.CellEditUseWholeCell = false;
			layoutTree.Dock = DockStyle.Fill;
			layoutTree.FullRowSelect = true;
			layoutTree.Location = new Point(0, 0);
			layoutTree.MultiSelect = false;
			layoutTree.Name = "layoutTree";
			layoutTree.ShowGroups = false;
			layoutTree.Size = new Size(800, 450);
			layoutTree.TabIndex = 1;
			layoutTree.UseOverlays = false;
			layoutTree.UseWaitCursorWhenExpanding = false;
			layoutTree.View = View.Details;
			layoutTree.VirtualMode = true;
			// 
			// LayoutPane
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 450);
			Controls.Add(layoutTree);
			Name = "LayoutPane";
			Text = "Layout";
			((System.ComponentModel.ISupportInitialize)layoutTree).EndInit();
			ResumeLayout(false);
		}

		#endregion

		public BrightIdeasSoftware.TreeListView layoutTree;
	}
}
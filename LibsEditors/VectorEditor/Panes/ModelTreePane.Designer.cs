namespace VectorEditor.Panes
{
	partial class ModelTreePane
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
			modelTree = new BrightIdeasSoftware.TreeListView();
			((System.ComponentModel.ISupportInitialize)modelTree).BeginInit();
			SuspendLayout();
			// 
			// modelTree
			// 
			modelTree.CellEditUseWholeCell = false;
			modelTree.Dock = DockStyle.Fill;
			modelTree.Location = new Point(0, 0);
			modelTree.Name = "modelTree";
			modelTree.ShowGroups = false;
			modelTree.Size = new Size(800, 450);
			modelTree.TabIndex = 0;
			modelTree.View = View.Details;
			modelTree.VirtualMode = true;
			// 
			// ModelTreePane
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 450);
			Controls.Add(modelTree);
			Name = "ModelTreePane";
			Text = "ModelTreePane";
			((System.ComponentModel.ISupportInitialize)modelTree).EndInit();
			ResumeLayout(false);
		}

		#endregion

		public BrightIdeasSoftware.TreeListView modelTree;
	}
}
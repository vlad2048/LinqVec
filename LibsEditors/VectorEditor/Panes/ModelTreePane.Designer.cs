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
			trackedList = new BrightIdeasSoftware.ObjectListView();
			groupBox1 = new GroupBox();
			((System.ComponentModel.ISupportInitialize)modelTree).BeginInit();
			((System.ComponentModel.ISupportInitialize)trackedList).BeginInit();
			groupBox1.SuspendLayout();
			SuspendLayout();
			// 
			// modelTree
			// 
			modelTree.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			modelTree.CellEditUseWholeCell = false;
			modelTree.FullRowSelect = true;
			modelTree.Location = new Point(0, 0);
			modelTree.MultiSelect = false;
			modelTree.Name = "modelTree";
			modelTree.ShowGroups = false;
			modelTree.Size = new Size(799, 216);
			modelTree.TabIndex = 0;
			modelTree.UseOverlays = false;
			modelTree.UseWaitCursorWhenExpanding = false;
			modelTree.View = View.Details;
			modelTree.VirtualMode = true;
			// 
			// trackedList
			// 
			trackedList.CellEditUseWholeCell = false;
			trackedList.Dock = DockStyle.Fill;
			trackedList.FullRowSelect = true;
			trackedList.Location = new Point(3, 19);
			trackedList.MultiSelect = false;
			trackedList.Name = "trackedList";
			trackedList.ShowGroups = false;
			trackedList.Size = new Size(793, 207);
			trackedList.TabIndex = 1;
			trackedList.UseOverlays = false;
			trackedList.View = View.Details;
			// 
			// groupBox1
			// 
			groupBox1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			groupBox1.Controls.Add(trackedList);
			groupBox1.Location = new Point(0, 222);
			groupBox1.Name = "groupBox1";
			groupBox1.Size = new Size(799, 229);
			groupBox1.TabIndex = 2;
			groupBox1.TabStop = false;
			groupBox1.Text = "Tracked";
			// 
			// ModelTreePane
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 450);
			Controls.Add(groupBox1);
			Controls.Add(modelTree);
			Name = "ModelTreePane";
			Text = "ModelTreePane";
			((System.ComponentModel.ISupportInitialize)modelTree).EndInit();
			((System.ComponentModel.ISupportInitialize)trackedList).EndInit();
			groupBox1.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		public BrightIdeasSoftware.TreeListView modelTree;
		public BrightIdeasSoftware.ObjectListView trackedList;
		private GroupBox groupBox1;
	}
}
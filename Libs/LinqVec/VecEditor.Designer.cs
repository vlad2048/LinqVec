using LinqVec.Structs;
using ReactiveVars;

namespace LinqVec
{
	partial class VecEditor<TDoc>
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent(IRwVar<Transform> transform)
		{
			statusStrip = new StatusStrip();
			drawPanel = new Controls.DrawPanel(transform);
			SuspendLayout();
			// 
			// statusStrip
			// 
			statusStrip.Location = new Point(0, 331);
			statusStrip.Name = "statusStrip";
			statusStrip.Size = new Size(492, 22);
			statusStrip.TabIndex = 0;
			statusStrip.Text = "statusStrip1";
			// 
			// drawPanel
			// 
			drawPanel.Dock = DockStyle.Fill;
			drawPanel.Location = new Point(0, 0);
			drawPanel.Name = "drawPanel";
			drawPanel.Size = new Size(492, 331);
			drawPanel.TabIndex = 1;
			// 
			// VecEditor
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			Controls.Add(drawPanel);
			Controls.Add(statusStrip);
			Name = "VecEditor";
			Size = new Size(492, 353);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		public StatusStrip statusStrip;
		public Controls.DrawPanel drawPanel;
	}
}

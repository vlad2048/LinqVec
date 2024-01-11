namespace Storybook
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
			statusStrip = new StatusStrip();
			drawPanel = new Controls.DrawPanel();
			btnSave = new Button();
			SuspendLayout();
			// 
			// statusStrip
			// 
			statusStrip.Location = new Point(0, 428);
			statusStrip.Name = "statusStrip";
			statusStrip.Size = new Size(800, 22);
			statusStrip.TabIndex = 0;
			statusStrip.Text = "statusStrip1";
			// 
			// drawPanel
			// 
			drawPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			drawPanel.Location = new Point(0, 35);
			drawPanel.Name = "drawPanel";
			drawPanel.Size = new Size(800, 390);
			drawPanel.TabIndex = 1;
			// 
			// btnSave
			// 
			btnSave.Location = new Point(5, 5);
			btnSave.Name = "btnSave";
			btnSave.Size = new Size(75, 23);
			btnSave.TabIndex = 2;
			btnSave.Text = "Save";
			btnSave.UseVisualStyleBackColor = true;
			// 
			// MainWin
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 450);
			Controls.Add(btnSave);
			Controls.Add(drawPanel);
			Controls.Add(statusStrip);
			Name = "MainWin";
			Text = "Storybook";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private StatusStrip statusStrip;
		private Controls.DrawPanel drawPanel;
		private Button btnSave;
	}
}
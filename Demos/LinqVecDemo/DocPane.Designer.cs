﻿namespace LinqVecDemo
{
    partial class DocPane
	{
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			vecEditor = new LinqVec.VecEditor();
			SuspendLayout();
			// 
			// vecEditor
			// 
			vecEditor.Dock = DockStyle.Fill;
			vecEditor.Location = new Point(0, 0);
			vecEditor.Name = "vecEditor";
			vecEditor.Size = new Size(800, 450);
			vecEditor.TabIndex = 0;
			// 
			// DocPane
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 450);
			Controls.Add(vecEditor);
			Name = "DocPane";
			Text = "Untitled";
			ResumeLayout(false);
		}

		#endregion

		public LinqVec.VecEditor vecEditor;
	}
}

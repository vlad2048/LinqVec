using CoolColorPicker.Controls;

namespace CoolColorPicker
{
	partial class ColorPickerDialog
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
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.valTrackBar = new System.Windows.Forms.TrackBar();
			this.label3 = new System.Windows.Forms.Label();
			this.valUpDown = new System.Windows.Forms.NumericUpDown();
			this.satTrackBar = new System.Windows.Forms.TrackBar();
			this.label2 = new System.Windows.Forms.Label();
			this.satUpDown = new System.Windows.Forms.NumericUpDown();
			this.hueTrackBar = new System.Windows.Forms.TrackBar();
			this.label1 = new System.Windows.Forms.Label();
			this.hueUpDown = new System.Windows.Forms.NumericUpDown();
			this.okButton = new System.Windows.Forms.Button();
			this.alphaTrackBar = new System.Windows.Forms.TrackBar();
			this.label4 = new System.Windows.Forms.Label();
			this.alphaUpDown = new System.Windows.Forms.NumericUpDown();
			this.cancelButton = new System.Windows.Forms.Button();
			this.previewPanel = new CoolColorPicker.Controls.PreviewPanel();
			this.rgbPickPanel = new RgbPickPanel();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.valTrackBar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.valUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.satTrackBar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.satUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.hueTrackBar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.hueUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.alphaTrackBar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.alphaUpDown)).BeginInit();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.valTrackBar);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.valUpDown);
			this.groupBox1.Controls.Add(this.satTrackBar);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.satUpDown);
			this.groupBox1.Controls.Add(this.hueTrackBar);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.hueUpDown);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.groupBox1.Location = new System.Drawing.Point(443, 125);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(321, 117);
			this.groupBox1.TabIndex = 2;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "HSV";
			// 
			// valTrackBar
			// 
			this.valTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.valTrackBar.AutoSize = false;
			this.valTrackBar.Location = new System.Drawing.Point(31, 83);
			this.valTrackBar.Maximum = 100;
			this.valTrackBar.Name = "valTrackBar";
			this.valTrackBar.Size = new System.Drawing.Size(232, 26);
			this.valTrackBar.TabIndex = 6;
			this.valTrackBar.TickFrequency = 5;
			this.valTrackBar.Value = 100;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(6, 88);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(17, 15);
			this.label3.TabIndex = 7;
			this.label3.Text = "V:";
			// 
			// valUpDown
			// 
			this.valUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.valUpDown.Location = new System.Drawing.Point(269, 86);
			this.valUpDown.Name = "valUpDown";
			this.valUpDown.Size = new System.Drawing.Size(46, 23);
			this.valUpDown.TabIndex = 7;
			this.valUpDown.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
			// 
			// satTrackBar
			// 
			this.satTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.satTrackBar.AutoSize = false;
			this.satTrackBar.Location = new System.Drawing.Point(31, 51);
			this.satTrackBar.Maximum = 100;
			this.satTrackBar.Name = "satTrackBar";
			this.satTrackBar.Size = new System.Drawing.Size(232, 26);
			this.satTrackBar.TabIndex = 4;
			this.satTrackBar.TickFrequency = 5;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 56);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(16, 15);
			this.label2.TabIndex = 4;
			this.label2.Text = "S:";
			// 
			// satUpDown
			// 
			this.satUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.satUpDown.Location = new System.Drawing.Point(269, 54);
			this.satUpDown.Name = "satUpDown";
			this.satUpDown.Size = new System.Drawing.Size(46, 23);
			this.satUpDown.TabIndex = 5;
			// 
			// hueTrackBar
			// 
			this.hueTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.hueTrackBar.AutoSize = false;
			this.hueTrackBar.Location = new System.Drawing.Point(31, 19);
			this.hueTrackBar.Maximum = 360;
			this.hueTrackBar.Name = "hueTrackBar";
			this.hueTrackBar.Size = new System.Drawing.Size(232, 26);
			this.hueTrackBar.TabIndex = 2;
			this.hueTrackBar.TickFrequency = 10;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(19, 15);
			this.label1.TabIndex = 1;
			this.label1.Text = "H:";
			// 
			// hueUpDown
			// 
			this.hueUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.hueUpDown.Location = new System.Drawing.Point(269, 22);
			this.hueUpDown.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
			this.hueUpDown.Name = "hueUpDown";
			this.hueUpDown.Size = new System.Drawing.Size(46, 23);
			this.hueUpDown.TabIndex = 3;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.okButton.Location = new System.Drawing.Point(449, 289);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(75, 23);
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// alphaTrackBar
			// 
			this.alphaTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.alphaTrackBar.AutoSize = false;
			this.alphaTrackBar.Location = new System.Drawing.Point(474, 248);
			this.alphaTrackBar.Maximum = 255;
			this.alphaTrackBar.Name = "alphaTrackBar";
			this.alphaTrackBar.Size = new System.Drawing.Size(230, 26);
			this.alphaTrackBar.TabIndex = 8;
			this.alphaTrackBar.TickFrequency = 8;
			this.alphaTrackBar.Value = 255;
			// 
			// label4
			// 
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(447, 253);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(18, 15);
			this.label4.TabIndex = 10;
			this.label4.Text = "A:";
			// 
			// alphaUpDown
			// 
			this.alphaUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.alphaUpDown.Location = new System.Drawing.Point(710, 251);
			this.alphaUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.alphaUpDown.Name = "alphaUpDown";
			this.alphaUpDown.Size = new System.Drawing.Size(46, 23);
			this.alphaUpDown.TabIndex = 9;
			this.alphaUpDown.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.cancelButton.Location = new System.Drawing.Point(530, 289);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 23);
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// previewPanel
			// 
			this.previewPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.previewPanel.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.previewPanel.Location = new System.Drawing.Point(449, 12);
			this.previewPanel.Name = "previewPanel";
			this.previewPanel.Size = new System.Drawing.Size(313, 107);
			this.previewPanel.TabIndex = 13;
			// 
			// rgbPickPanel
			// 
			this.rgbPickPanel.Location = new System.Drawing.Point(12, 12);
			this.rgbPickPanel.MaximumSize = new System.Drawing.Size(425, 300);
			this.rgbPickPanel.MinimumSize = new System.Drawing.Size(425, 300);
			this.rgbPickPanel.Name = "rgbPickPanel";
			this.rgbPickPanel.Size = new System.Drawing.Size(425, 300);
			this.rgbPickPanel.TabIndex = 14;
			// 
			// ColorPickerDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(774, 324);
			this.Controls.Add(this.rgbPickPanel);
			this.Controls.Add(this.previewPanel);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.alphaTrackBar);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.alphaUpDown);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.groupBox1);
			this.Name = "ColorPickerDialog";
			this.Text = "Color";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.valTrackBar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.valUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.satTrackBar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.satUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.hueTrackBar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.hueUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.alphaTrackBar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.alphaUpDown)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private GroupBox groupBox1;
		private TrackBar hueTrackBar;
		private Label label1;
		private NumericUpDown hueUpDown;
		private Button okButton;
		private TrackBar valTrackBar;
		private Label label3;
		private NumericUpDown valUpDown;
		private TrackBar satTrackBar;
		private Label label2;
		private NumericUpDown satUpDown;
		private TrackBar alphaTrackBar;
		private Label label4;
		private NumericUpDown alphaUpDown;
		private Button cancelButton;
		private PreviewPanel previewPanel;
		private RgbPickPanel rgbPickPanel;
	}
}
using GBX.NET.Engines.Game;
using System.Runtime.InteropServices.JavaScript;
using System.Windows.Forms;

namespace ReplayVisualizer
{
    partial class Form1 : Form
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
            pictureBox1 = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(938, 539);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(938, 539);
            Controls.Add(pictureBox1);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox pictureBox1 = new();

        /// <summary>
        /// Draws the position of a Ghost's sample data projected on the specified plane.
        /// </summary>
        /// <param name="samples">Sample data of a CGameCtnGhost.</param>
        /// <param name="viewPlane">The plane on which the positions will e projected.</param>
        public void DrawSamples(System.Collections.ObjectModel.ObservableCollection<CGameGhost.Data.Sample> samples, ViewPlane viewPlane, Color penColor, Color bgColor, float multiplier, float penWidth, bool saveBitmap, string bitmapPath)
        {
            // Find the minima and maxima of the sample positions
            float xMax = float.MinValue, xMin = float.MaxValue, yMax = float.MinValue, yMin = float.MaxValue;
            for (int i = 0; i < samples.Count; i++)
            {
                switch(viewPlane)
                {
                    case ViewPlane.ZX:
                        {
                            xMin = Math.Min(xMin, samples[i].Position.Z);
                            xMax = Math.Max(xMax, samples[i].Position.Z);
                            yMin = Math.Min(yMin, samples[i].Position.X);
                            yMax = Math.Max(yMax, samples[i].Position.X);
                            break;
                        }
                    case ViewPlane.XY:
                        {
                            xMin = Math.Min(xMin, samples[i].Position.X);
                            xMax = Math.Max(xMax, samples[i].Position.X);
                            yMin = Math.Min(yMin, samples[i].Position.Y);
                            yMax = Math.Max(yMax, samples[i].Position.Y);
                            break;
                        }
                    case ViewPlane.ZY:
                        {
                            xMin = Math.Min(xMin, samples[i].Position.Z);
                            xMax = Math.Max(xMax, samples[i].Position.Z);
                            yMin = Math.Min(yMin, samples[i].Position.Y);
                            yMax = Math.Max(yMax, samples[i].Position.Y);
                            break;
                        }
                }
            }


            // Define some parameters to properly center the samples on the window
            float xWhitespace = 250.0f;
            float yWhitespace = 250.0f;
            float xSize = xMax - xMin + xWhitespace;
            float ySize = yMax - yMin + yWhitespace;


            // Set correct offsets, depending on which axis needs to be inverted
            float offset0 = 0.0f, offset1 = 0.0f;
            switch(viewPlane)
            {
                case ViewPlane.ZX:
                    {
                        offset0 = xMax + xWhitespace / 2;
                        offset1 = - yMin + yWhitespace / 2;
                        break;
                    }
                case ViewPlane.XY:
                case ViewPlane.ZY:
                    {
                        offset0 = - xMin + xWhitespace / 2;
                        offset1 = yMax + yWhitespace / 2;
                        break;
                    }
            }


            // Initialize the PictureBox to draw in
            pictureBox1.Refresh();
            pictureBox1.BackColor = bgColor;
            pictureBox1.Image = new Bitmap((int)(multiplier * xSize), (int)(multiplier * ySize));
            Bitmap map = (Bitmap)pictureBox1.Image;
            Graphics g = Graphics.FromImage(map);
            g.Clear(bgColor);


            // Initialize Pen for drawing
            Pen pen = new(penColor, multiplier * penWidth);
            pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
            pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;


            // Loop over all samples, drawing a line from the sample to the next with respect to the projection.
            Point p0 = new();
            Point p1 = new();
            for (int i = 0; i < samples.Count - 1; i++)
            {
                switch (viewPlane)
                {
                    case ViewPlane.ZX:
                        {
                            p0 = new((int)((-samples[i + 0].Position.Z + offset0) * multiplier), (int)((samples[i + 0].Position.X + offset1) * multiplier));
                            p1 = new((int)((-samples[i + 1].Position.Z + offset0) * multiplier), (int)((samples[i + 1].Position.X + offset1) * multiplier));
                            break;
                        }
                    case ViewPlane.XY:
                        {
                            p0 = new((int)((samples[i + 0].Position.X + offset0) * multiplier), (int)((-samples[i + 0].Position.Y + offset1) * multiplier));
                            p1 = new((int)((samples[i + 1].Position.X + offset0) * multiplier), (int)((-samples[i + 1].Position.Y + offset1) * multiplier));
                            break;
                        }
                    case ViewPlane.ZY:
                        {
                            p0 = new((int)((samples[i + 0].Position.Z + offset0) * multiplier), (int)((-samples[i + 0].Position.Y + offset1) * multiplier));
                            p1 = new((int)((samples[i + 1].Position.Z + offset0) * multiplier), (int)((-samples[i + 1].Position.Y + offset1) * multiplier));
                            break;
                        }
                }

                g.DrawLine(pen, p0, p1);
            }

            pen.Dispose();
            g.Dispose();

            if (saveBitmap)
            {
                map.Save(bitmapPath);
            }
        }
    }
}

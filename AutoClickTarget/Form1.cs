using PInvoke;
using System.Collections.Generic;

namespace AutoClickTarget
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		protected override void WndProc(ref Message m)
		{
			System.Diagnostics.Debug.WriteLine($"{DateTime.Now:ss.ff} {(User32.WindowMessage)m.Msg}, {m.HWnd}, {m.WParam}, {m.LParam}");

			base.WndProc(ref m);	
		}

		int FormCount = 0;
		int FormX = 0;
		int FormY = 0;

		private void Form1_MouseClick(object sender, MouseEventArgs e)
		{
			FormCount++;
			FormX = e.X;
			FormY = e.Y;
			Invalidate();
		}

		private void Form1_Paint(object sender, PaintEventArgs e)
		{
			TextRenderer.DrawText(e.Graphics, $"{FormCount}", Font, new Point(10, 10), ForeColor);
			TextRenderer.DrawText(e.Graphics, $"({FormX},{FormY})", Font, new Point(10, 25), ForeColor);
		}
	}
}
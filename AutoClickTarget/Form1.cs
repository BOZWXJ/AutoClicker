namespace AutoClickTarget
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		int FormCount = 0;
		int FormX = 0;
		int FormY = 0;

		int Count = 0;
		int X = 0;
		int Y = 0;

		private void Form1_MouseClick(object sender, MouseEventArgs e)
		{
			FormCount++;
			FormX = e.X;
			FormY = e.Y;
			panel1.Invalidate();
		}

		private void panel1_MouseClick(object sender, MouseEventArgs e)
		{
			Count++;
			X = e.X;
			Y = e.Y;
			panel1.Invalidate();
		}

		private void panel1_Paint(object sender, PaintEventArgs e)
		{
			TextRenderer.DrawText(e.Graphics, $"{FormCount}", Font, new Point(10, 10), panel1.ForeColor);
			TextRenderer.DrawText(e.Graphics, $"({FormX},{FormY})", Font, new Point(10, 25), panel1.ForeColor);
			TextRenderer.DrawText(e.Graphics, $"{Count}", Font, new Point(10, 40), panel1.ForeColor);
			TextRenderer.DrawText(e.Graphics, $"({X},{Y})", Font, new Point(10, 55), panel1.ForeColor);
		}

	}
}
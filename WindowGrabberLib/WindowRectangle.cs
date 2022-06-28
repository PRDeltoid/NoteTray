using System.Drawing;

namespace WindowGrabberLib;

public class WindowRectangle
{
	public Rectangle Bounds { get; }
	public uint ZLevel { get; }
	public IntPtr Window { get; }
	public string Title { get; }

	public WindowRectangle(IntPtr window, string title, Rectangle bounds, uint zLevel)
	{
		Window = window;
		Title = title;
		Bounds = bounds;
		ZLevel = zLevel;
	}
}
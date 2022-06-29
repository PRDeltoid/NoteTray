using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace WindowGrabberLib;

public static class WindowsStateSnapshotter
{
	#region WinAPI Imports
	[DllImport("user32.dll")]
	private static extern bool GetWindowRect(IntPtr hWnd, ref RECT rect);
	[DllImport("User32.dll")]
	static extern int GetWindowLong(IntPtr hwnd, GWL style);
	[DllImport("User32.dll")]
	static extern IntPtr GetTopWindow(IntPtr hwnd);
	[DllImport("User32.dll")]
	static extern IntPtr GetDesktopWindow();
	[DllImport("User32.dll")]
	static extern IntPtr GetWindow(IntPtr hwnd, uint command);
	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
	[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
	static extern int GetWindowTextLength(IntPtr hWnd);
	[DllImport("user32.dll", SetLastError=true)]
	static extern bool GetLayeredWindowAttributes(IntPtr hwnd, out uint crKey, out byte bAlpha, out uint dwFlags);
	
	public enum GWL
	{
		GWL_WNDPROC =    (-4),
		GWL_HINSTANCE =  (-6),
		GWL_HWNDPARENT = (-8),
		GWL_STYLE =      (-16),
		GWL_EXSTYLE =    (-20),
		GWL_USERDATA =   (-21),
		GWL_ID =     (-12)
	}	
	
	const UInt32 WS_OVERLAPPED = 0;
	const UInt32 WS_POPUP = 0x80000000;
	const UInt32 WS_CHILD = 0x40000000;
	const UInt32 WS_MINIMIZE = 0x20000000;
	const UInt32 WS_VISIBLE = 0x10000000;
	const UInt32 WS_DISABLED = 0x8000000;
	const UInt32 WS_CLIPSIBLINGS = 0x4000000;
	const UInt32 WS_CLIPCHILDREN = 0x2000000;
	const UInt32 WS_MAXIMIZE = 0x1000000;
	const UInt32 WS_CAPTION = 0xC00000;      // WS_BORDER or WS_DLGFRAME  
	const UInt32 WS_BORDER = 0x800000;
	const UInt32 WS_DLGFRAME = 0x400000;
	const UInt32 WS_VSCROLL = 0x200000;
	const UInt32 WS_HSCROLL = 0x100000;
	const UInt32 WS_SYSMENU = 0x80000;
	const UInt32 WS_THICKFRAME = 0x40000;
	const UInt32 WS_GROUP = 0x20000;
	const UInt32 WS_TABSTOP = 0x10000;
	const UInt32 WS_MINIMIZEBOX = 0x20000;
	const UInt32 WS_MAXIMIZEBOX = 0x10000;
	const UInt32 WS_TILED = WS_OVERLAPPED;
	const UInt32 WS_ICONIC = WS_MINIMIZE;
	const UInt32 WS_SIZEBOX = WS_THICKFRAME;
	const UInt32 WS_EX_TRANSPARENT = 0x0020;	
	
	[StructLayout(LayoutKind.Sequential)]
	private struct RECT
	{
		public readonly int Left;        // x position of upper-left corner
		public readonly int Top;         // y position of upper-left corner
		public readonly int Right;       // x position of lower-right corner
		public readonly int Bottom;      // y position of lower-right corner
	}
	#endregion
	
	public static List<WindowRectangle> GetStateSnapshot(IntPtr ignoreWindow)
	{
		List<WindowRectangle> procWindows = new List<WindowRectangle>();
		uint zLevel = 0;
		// Start from the bottom (the desktop) and work up through the windows by z-level
		IntPtr window = GetTopWindow(GetDesktopWindow());
		do
		{
			string windowTitle = GetWindowTitle(window);
			// Skip loop early if the window matches the ignoreWindow param, is not visible or is not a real window
			if(IsWindowVisible(window) == false || window == ignoreWindow || windowTitle is "" or "Program Manager" or "Microsoft Text Input Application")  continue;
			
			procWindows.Add(new WindowRectangle(window, windowTitle, GetWindowRect(window), zLevel));
			zLevel++;
		} while ((window = GetWindow(window, 2 /*GW_HWNDNEXT*/)) != IntPtr.Zero);
		return procWindows;
	}

	private static bool IsWindowVisible(IntPtr window)
	{
		int result = GetWindowLong(window, GWL.GWL_STYLE);
		return (result & WS_VISIBLE) != 0;
	}

	private static string GetWindowTitle(IntPtr window)
	{
		int length = GetWindowTextLength(window) + 1;
		StringBuilder title = new StringBuilder(length);
		GetWindowText(window, title, length);
		return title.ToString();
	}

	private static Rectangle GetWindowRect(IntPtr window)
	{
		RECT procRecRaw = new RECT();
		GetWindowRect(window, ref procRecRaw);
			
		return new Rectangle
		{
			X = procRecRaw.Left + 8,
			Y = procRecRaw.Top,
			Width = procRecRaw.Right - procRecRaw.Left - 16,
			Height = procRecRaw.Bottom - procRecRaw.Top - 8
		};	
	}
}
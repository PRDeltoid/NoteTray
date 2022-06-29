using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using Serilog;
using Point = System.Drawing.Point;

namespace WindowGrabberLib;

public class WindowGrabber
{
	#region WinAPI Imports
	[StructLayout(LayoutKind.Sequential)]
	private struct POINT
	{
		public int X;
		public int Y;
	}
	
	[StructLayout(LayoutKind.Sequential)]
	private struct MSLLHOOKSTRUCT
	{
		public POINT pt;
		public uint mouseData;
		public uint flags;
		public uint time;
		public IntPtr dwExtraInfo;
	}
	
	private enum MouseMessages
	{
		WM_LBUTTONDOWN = 0x0201,
		WM_LBUTTONUP = 0x0202,
		WM_MOUSEMOVE = 0x0200,
		WM_MOUSEWHEEL = 0x020A,
		WM_RBUTTONDOWN = 0x0204,
		WM_RBUTTONUP = 0x0205,
		WM_LBUTTONDBLCLK = 0x0203,
		WM_MBUTTONDOWN = 0x0207,
		WM_MBUTTONUP = 0x0208
	}
	
	private delegate IntPtr MouseHookHandler(int nCode, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr SetWindowsHookEx(int idHook, MouseHookHandler lpfn, IntPtr hMod, uint dwThreadId);
	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool UnhookWindowsHookEx(IntPtr hhk);
	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr GetModuleHandle(string lpModuleName);
	[DllImport("User32.dll")]
	static extern IntPtr GetDC(IntPtr hwnd);
	[DllImport("User32.dll")]
	static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);
	[DllImport("user32.dll")]
	private static extern bool GetCursorPos(out POINT lpPoint);
	[DllImport("user32.dll")]
	static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);	
	#endregion

	// Tracks the hook so we can unhook it later
	private IntPtr _hookId = IntPtr.Zero;
	private static MouseHookHandler _handler;
	
	// Tracks the window grabber periodic mouse check for cancellation when user clicks
	private CancellationTokenSource _cancelToken = new CancellationTokenSource();
	private PeriodicTimer _timer;
	private readonly Font _font;

	public WindowGrabber()
	{
		_font = new Font("Times New Roman", 12.0f);
		/*
		 * We have to use a static "_handler" value to pass a delegate into User32 pinvoke calls because:
		 * "you should not pass a lambda or delegate directly into a Win32 Api call (or any C api call for that matter).
		 * The fix is either to make a static variable containing a reference to the function"
		 * - https://stackoverflow.com/a/69105090
		 * */
		_handler = HookFunc;
	}
	
	~WindowGrabber()
	{
		// Unhook incase the caller forgot to
		MouseUnhook();
	}
	
	public async Task<IntPtr> GrabWindowAsync()
	{
		Log.Debug($"GrabWindow running");
		List<WindowRectangle> state = WindowsStateSnapshotter.GetStateSnapshot();
		_timer = new PeriodicTimer(TimeSpan.FromMilliseconds(200));

		IntPtr desktop = GetDC(IntPtr.Zero);
		using Graphics g = Graphics.FromHdc(desktop);
		
		MouseHook();

		while (await _timer.WaitForNextTickAsync())
		{
			Point p = GetCursorPosition();
			// Find the first colliding window with our cursor's position
			foreach (WindowRectangle window in state)
			{
				if (window.Contains(p))
				{
					DrawOutline(in g, window);
					// Once we find the topmost collision, don't test any further.
					// This is because only one outline should be present at a time
					break;
				}
			}
		}
		MouseUnhook();
		
		Point finishPos = GetCursorPosition();
		Log.Debug($"Window selection stopped. Final position: ({finishPos.X}, {finishPos.Y})");
		int _ = ReleaseDC(IntPtr.Zero, desktop);

		// Find what window was clicked (if any)
		foreach (WindowRectangle window in state)
		{
			if (window.Contains(finishPos))
			{
				Log.Debug($"Clicked window: {window.Title} ({window.Window})");
				return window.Window;
			}
		}

		// If no window was clicked, return a zero ptr
		return IntPtr.Zero;
	}

	private void DrawOutline(in Graphics graphics, WindowRectangle window)
	{
		graphics.DrawRectangle(Pens.Cyan, window.Bounds.X, window.Bounds.Y, window.Bounds.Width, window.Bounds.Height);
		// Render the window title in the top left
		SizeF sizeOfText = graphics.MeasureString(window.Title, new Font("Arial", 12f));
		graphics.FillRectangle(Brushes.LightCyan, window.Bounds.X + 8, window.Bounds.Y + 8, sizeOfText.Width, sizeOfText.Height);
		graphics.DrawString(window.Title, _font, Brushes.Cyan, window.Bounds.X + 8, window.Bounds.Y + 8);
	}
	
	private static Point GetCursorPosition()
	{
		GetCursorPos(out POINT p);
		return new Point(p.X, p.Y);
	}

	private void MouseHook()
	{
		Log.Debug("Mouse Hook");
		using ProcessModule module = Process.GetCurrentProcess().MainModule;
		IntPtr handle = GetModuleHandle(module.ModuleName);
		uint threadID = GetWindowThreadProcessId(handle, out uint _);
		Log.Debug($"Set Hook with handle: {handle} and thread ID: {threadID}");
		_hookId =  SetWindowsHookEx(14 /* WH_MOUSE_LL */, _handler, handle, threadID);
	}

	private void MouseUnhook()
	{
		// Mouse is not hooked. Exit early
		if (_hookId == IntPtr.Zero) return;
		
		Log.Debug("Mouse Unhook");

		UnhookWindowsHookEx(_hookId);	
	}
	
	private IntPtr HookFunc(int nCode, IntPtr wParam, IntPtr lParam)
	{
		if (nCode >= 0)
		{
			switch ((MouseMessages)wParam)
			{
				case MouseMessages.WM_LBUTTONDOWN:
					Log.Debug("Mouse Click");
					_timer?.Dispose();
					break;
			}
		}
		return CallNextHookEx(_hookId, nCode, wParam, lParam);
	}
}
using System;
using System.Runtime.InteropServices;
using System.Windows;

using System.Windows.Threading;
using Serilog;

namespace NoteTray;

public class WindowSnapper
{
#pragma warning disable CS0660, CS0661
    private struct Rect
#pragma warning restore CS0660, CS0661
    {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Right { get; set; }
        public int Bottom { get; set; }

        public int Height
        {
            get { return Bottom - Top; }
        }

        public static bool operator !=(Rect r1, Rect r2)
        {
            return !(r1 == r2);
        }

        public static bool operator ==(Rect r1, Rect r2)
        {
            return r1.Left == r2.Left && r1.Right == r2.Right && r1.Top == r2.Top && r1.Bottom == r2.Bottom;
        }
    }

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);
    
    [DllImport("user32.dll")]
    private static extern bool IsWindow(IntPtr hwnd);

    private readonly DispatcherTimer _timer;
    private IntPtr _attachedWindowHandle;
    private Rect _lastBounds;
    private readonly Window _window;
    private Rect _lastBounds1;

    public WindowSnapper(Window window)
    {
        _window = window;
        _window.Topmost = true;
        

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(10),
            IsEnabled = false
        };
        _timer.Tick += (x, y) => SnapToWindow();
    }

    public void Attach(IntPtr windowHandle)
    {
        _attachedWindowHandle = windowHandle;
        _timer.Start();
    }

    public void Detach()
    {
        Log.Debug("WindowSnapper Detaching");
        _timer.Stop();
    }

    private void SnapToWindow()
    {
        // Disable the window snapper if the window we are attached to stops existing
        if (DoesWindowExist(_attachedWindowHandle) == false)
        {
            Detach();
            return;
        }

        Rect bounds = GetWindowBounds(_attachedWindowHandle);

        // If the attached window has moved, move the Note Tray so it stays attached to the left side
        if (bounds != _lastBounds)
        {
            _window.Top = bounds.Top;
            _window.Left = bounds.Left - _window.Width;
            _window.Height = bounds.Height;
            _lastBounds = bounds;
            // Update Note Tray location for use in future calls to SnapToWindow() in the code below
            _lastBounds1 = GetWindowBounds(_window);;
            return;
        }
        
        // If the Note Tray was moved seperately from its attached window, we assume the user no longer wants the window snapping behavior so we detach
        if (_lastBounds1 != GetWindowBounds(_window))
        {
            Detach();
        }
    }

    private static bool DoesWindowExist(IntPtr attachedWindowHandle)
    {
        return IsWindow(attachedWindowHandle);
    }

    private static Rect GetWindowBounds(IntPtr handle)
    {
        Rect bounds = new Rect();
        GetWindowRect(handle, ref bounds);
        return bounds;
    }
    
    private static Rect GetWindowBounds(Window window)
    {
        return new Rect() {Top = (int)window.Top, Left = (int)window.Left , Bottom = (int)(window.Top + window.Height) , Right = (int)(window.Left + window.Width)};
    }
}
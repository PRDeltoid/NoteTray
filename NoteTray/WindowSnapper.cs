using System;
using System.Runtime.InteropServices;
using System.Windows;

using System.Windows.Threading;

namespace NoteTray;

public class WindowSnapper
{
    private struct Rect
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
    private IntPtr _windowHandle;
    private Rect _lastBounds;
    private readonly Window _window;

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
        _windowHandle = windowHandle;
        _timer.Start();
    }

    public void Detach()
    {
        _timer.Stop();
    }

    private void SnapToWindow()
    {
        // Disable the window snapper if the window we are attached to stops existing
        if (IsWindow(_windowHandle) == false)
        {
            Detach();
            return;
        }
        
        Rect bounds = GetWindowBounds(_windowHandle);

        if (bounds != _lastBounds)
        {
            _window.Top = bounds.Top;
            _window.Left = bounds.Left - _window.Width;
            _window.Height = bounds.Height;
            _lastBounds = bounds;
        }
    }

    private static Rect GetWindowBounds(IntPtr handle)
    {
        Rect bounds = new Rect();
        GetWindowRect(handle, ref bounds);
        return bounds;
    }
}
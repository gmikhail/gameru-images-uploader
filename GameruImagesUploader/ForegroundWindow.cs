using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameruImagesUploader
{
    class ForegroundWindow
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(HandleRef hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
            public int Width
            {
                get { return Right - Left; }
            }

            public int Height
            {
                get { return Bottom - Top; }
            }
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        
        [DllImport("user32.dll")]
        static extern int GetWindowRect(IntPtr hWnd, out RECT rect);

        public static RECT GetForegroundWindowRect()
        {
            IntPtr hWnd = GetForegroundWindow();
            RECT rect;
            GetWindowRect(hWnd, out rect);
            return rect;
        }
    }
}

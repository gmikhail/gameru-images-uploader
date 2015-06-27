using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GameruImagesUploader
{
    class KeyboardHook
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
 
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, KeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
 
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
 
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
 
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
 
        public delegate IntPtr KeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        public event EventHandler<KeyEvent> OnKeyEventHandler;
 
        private KeyboardProc proc;
        private IntPtr hookID = IntPtr.Zero;

        public KeyboardHook()
        {
            proc = HookCallback;
        }
 
        public void HookKeyboard()
        {
            hookID = SetHook(proc);
        }
 
        public void UnHookKeyboard()
        {
            UnhookWindowsHookEx(hookID);
        }
 
        private IntPtr SetHook(KeyboardProc proc)
        {
            try
            {
                using (Process curProcess = Process.GetCurrentProcess())
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
                }
            }
            catch(Exception ex)
            {
                Log.Add(ex);
            }
            return new IntPtr();
        }

        private static readonly List<int> KeysDown = new List<int>();

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (nCode >= 0)
                {
                    int vkCode = Marshal.ReadInt32(lParam);

                    if (OnKeyEventHandler != null)
                    {
                        var keyEventArgs = new KeyEvent();
                        if (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN)
                        {
                            keyEventArgs.KeyDownArgs(KeyInterop.KeyFromVirtualKey(vkCode));
                        }
                        else if (wParam == (IntPtr)WM_KEYUP || wParam == (IntPtr)WM_SYSKEYDOWN)
                        {
                            keyEventArgs.KeyUpArgs(KeyInterop.KeyFromVirtualKey(vkCode));
                        }
                        OnKeyEventHandler(this, keyEventArgs);
                    }
                }
                return CallNextHookEx(hookID, nCode, wParam, lParam);
            }
            catch(Exception ex)
            {
                Log.Add(ex);
            }
            return new IntPtr();
        }
    }

    public class KeyEvent : EventArgs
    {
        public Key KeyDown { get; private set; }
        public Key KeyUp { get; private set; }
 
        public void KeyDownArgs(Key key)
        {
            KeyDown = key;
        }
        public void KeyUpArgs(Key key)
        {
            KeyUp = key;
        }
    }
}
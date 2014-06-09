using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace WCompose
{
    public abstract class KeyboardHook : IDisposable
    {
        protected enum EventType : ulong
        {
            KeyDown = 0x0100, KeyUp = 0x0101,
            SysKeyDown = 0x0104, SysKeyUp = 0x0105,
        }
        
        private const int HC_ACTION = 0;
        private const int WH_KEYBOARD_LL = 13;

        private readonly SafeHookHandle _hook;
        private readonly LowLevelKeyboardProc _proc;

        protected KeyboardHook()
        {
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                _proc = Proc;

                var hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(curModule.ModuleName), 0);
                if (hookHandle.IsInvalid) throw new Win32Exception();
                
                _hook = hookHandle;
            }
        }

        private IntPtr Proc(int nCode, UIntPtr wParam, ref KBDLLHOOKSTRUCT lParam)
        {
            if (nCode == HC_ACTION)
            {
                if (TryHandle((EventType) wParam.ToUInt64(), lParam))
                {
                    return (IntPtr)1;
                }
            }

            return CallNextHookEx(_hook, nCode, wParam, ref lParam);
        }

        [Flags]
        protected enum KeyFlags : uint
        {
            Injected = 0x10,
        }

        protected struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public KeyFlags flags;
            public uint time;
            public UIntPtr extraInfo;
        }


        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr LowLevelKeyboardProc(
            int nCode,
            UIntPtr wParam,
            ref KBDLLHOOKSTRUCT lParam
            );

        private class SafeHookHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            public SafeHookHandle() : base(true)
            {
            }

            protected override bool ReleaseHandle()
            {
                return UnhookWindowsHookEx(DangerousGetHandle());
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern SafeHookHandle SetWindowsHookEx(
            int idHook,
            LowLevelKeyboardProc lpfn,
            IntPtr hMod,
            uint dwThreadId
            );

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr handle);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(
            SafeHookHandle hhk,
            int nCode,
            UIntPtr wParam,
            ref KBDLLHOOKSTRUCT lParam
            );


        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        protected abstract bool TryHandle(EventType eventType, KBDLLHOOKSTRUCT keyCodes);

        public void Dispose()
        {
            Dispose(true);
            GC.KeepAlive(_proc);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                using (_hook)
                {
                }
            }
        }
    }
}
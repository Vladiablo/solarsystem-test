using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SolarSystem.Platform.Windows
{
    internal static class User32
    {
        internal enum CmdShow : int
        {
            Hide = 0,
            ShowNormal = 1,
            ShowMinimized = 2,
            ShowMaximized = 3,
            ShowNoActivate = 4,
            Show = 5,
            Minimize = 6,
            ShowMinNoActive = 7,
            ShowNA = 8,
            Restore = 9,
            ShowDefault = 10,
            ForceMinimize = 11
        }

        [DllImport("user32.dll", EntryPoint = "ShowWindow", CallingConvention = CallingConvention.StdCall)]
        internal static extern bool ShowWindow(IntPtr hWnd, CmdShow nCmdShow);
    }
}

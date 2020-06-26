using System;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading.Tasks;
using hidePSBCproxyTips.Properties;
using System.Runtime.InteropServices;

namespace hidePSBCproxyTips
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            bool hasMutex;
            System.Threading.Mutex instance = new System.Threading.Mutex(true, "IMutexName", out hasMutex);
            if (!hasMutex)
            {
                Environment.Exit(0);
                return;
            }
            Form form1 = new Form();
            form1.WindowState = FormWindowState.Minimized;
            form1.ShowInTaskbar = false;
            form1.Text = "PSBC弹窗自动关闭程序";
            form1.Icon = Icon.FromHandle(AppIcon.favicon.GetHicon());
            form1.Load += new EventHandler((object o, EventArgs e) =>
            {
                Task.Factory.StartNew(() =>
                {
                    for (; ; )
                    {
                        Task.Run(() =>
                        {
                            IntPtr hWnd = FindWindow(null, "提示");
                            if (hWnd != IntPtr.Zero)
                            {
                                int Pid;
                                GetWindowThreadProcessId(hWnd, out Pid);
                                string name = Process.GetProcessById(Pid).ProcessName;
                                if (name.Equals("PSBCInput"))
                                {
                                    IntPtr childHwnd = FindWindowEx(hWnd, IntPtr.Zero, null, "确定");
                                    if (childHwnd != IntPtr.Zero)
                                    {
                                        SendMessage(childHwnd, 0xF5, 0, 0);
                                    };
                                }
                            }
                        });
                        Thread.Sleep(800);
                    }
                });
            });
            Application.Run(form1);
            instance.ReleaseMutex();
        }

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);

        [DllImport("kernel32.dll")]
        public static extern int OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        private extern static IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", EntryPoint = "SendMessageA")]
        private static extern int SendMessage(IntPtr hwnd, uint wMsg, int wParam, int lParam);
    }
}

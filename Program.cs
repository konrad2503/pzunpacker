using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PZDepack
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 frm1 = new Form1();
            SetConsoleIcon(frm1.Icon);
            Application.Run(frm1);
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetConsoleIcon(IntPtr hIcon);
        public static void SetConsoleIcon(System.Drawing.Icon icon)
        {
            SetConsoleIcon(icon.Handle);
        }
    }
}

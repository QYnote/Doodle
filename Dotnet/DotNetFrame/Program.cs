using System;
using System.Windows.Forms;

namespace DotNetFrame
{
    internal static class Program
    {
        static readonly string ProcessorType = Environment.Is64BitProcess ? "x64" : "x86";
        static readonly string RunDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            InsertSQLiteDLL();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Application.Run(new View.FrmSolution());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}\r\nTrace:{ex.StackTrace}");
            }
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetDllDirectory(string lpPathName);

        static void InsertSQLiteDLL()
        {
            string path = System.IO.Path.Combine(RunDirectory, "SQLite", ProcessorType);

            if (System.IO.Directory.Exists(path))
            {
                SetDllDirectory(path);
            }
        }
    }
}

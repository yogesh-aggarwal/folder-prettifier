using System;
using System.Windows.Forms;

namespace FolderPrettifier
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string folderPath = args.Length == 0 ? "" : args[0];

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main(folderPath));
        }
    }
}

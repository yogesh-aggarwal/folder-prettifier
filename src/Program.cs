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
            if (CommandLine.TryDispatchUpdater(args))
            {
                return;
            }

            string folderPath = CommandLine.GetCurrentFolder(args);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main(folderPath));
        }
    }
}

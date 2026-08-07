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
            if (args.Length >= 3 && args[0] == "--apply-update")
            {
                int oldPid;
                if (int.TryParse(args[2], out oldPid))
                {
                    UpdateService.RunUpdater(args[1], oldPid);
                }
                return;
            }

            string folderPath = args.Length == 0 ? "" : args[0];

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main(folderPath));
        }
    }
}

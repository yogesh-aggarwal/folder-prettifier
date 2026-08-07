using System;

namespace FolderPrettifier
{
    internal static class CommandLine
    {
        internal static bool TryDispatchUpdater(string[] args)
        {
            string updaterTarget;
            int updaterPid;
            if (!UpdateService.TryParseUpdaterArgs(args, out updaterTarget, out updaterPid))
            {
                return false;
            }

            UpdateService.RunUpdater(updaterTarget, updaterPid);
            return true;
        }

        internal static string GetCurrentFolder(string[] args)
        {
            return args == null || args.Length == 0 ? "" : args[0];
        }
    }
}

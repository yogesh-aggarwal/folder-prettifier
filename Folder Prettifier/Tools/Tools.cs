using System;
using Windows.System;

namespace Folder_Prettifier.Tools
{
    class Tools
    {
        async static public void OpenURL(string uri)
        {
            await Launcher.LaunchUriAsync(new Uri(uri));
        }
    }
}

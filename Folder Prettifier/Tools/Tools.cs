using System;
using Windows.System;

namespace Folder_Prettifier.Tools
{
    public class Tools
    {
        async static public void OpenURL(string uri)
        {
            await Launcher.LaunchUriAsync(new Uri(uri));
        }
    }

    public class Storage
    {
        public static Windows.Storage.ApplicationDataContainer localSettings =
                Windows.Storage.ApplicationData.Current.LocalSettings;
        public static Windows.Storage.StorageFolder localFolder =
            Windows.Storage.ApplicationData.Current.LocalFolder;

        public static dynamic Get(string key, dynamic defaultValue = null)
        {
            try
            {
                dynamic value = Storage.localSettings.Values[key];
                if (value == null) throw new NullReferenceException();
                return Storage.localSettings.Values[key];
            }
            catch (NullReferenceException)
            {
                Set(key, true);
                return defaultValue;
            }
        }

        public static void Set(string key, dynamic value = null)
        {
            try
            {
                Storage.localSettings.Values[key] = value;
            }
            catch { }
        }
    }
}

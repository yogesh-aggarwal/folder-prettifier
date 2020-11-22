using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http;
using Windows.System;

namespace Folder_Prettifier.Tools
{
    public class Tools
    {
        async static public void OpenURL(string uri)
        {
            await Launcher.LaunchUriAsync(new Uri(uri));
        }

        public static bool CheckInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead(Resources.InternetCheckUrl))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }

    public class Resources
    {
        public static dynamic BasicCatalog = "{\"mp4\":{\"folderName\":\"Videos\"},\"mkv\":{\"folderName\":\"Videos\"},\"m4a\":{\"folderName\":\"Videos\"},\"m4v\":{\"folderName\":\"Videos\"},\"f4v\":{\"folderName\":\"Videos\"},\"f4a\":{\"folderName\":\"Videos\"},\"m4b\":{\"folderName\":\"Videos\"},\"m4r\":{\"folderName\":\"Videos\"},\"f4b\":{\"folderName\":\"Videos\"},\"mov\":{\"folderName\":\"Videos\"},\"3gp\":{\"folderName\":\"Videos\"},\"3gp2\":{\"folderName\":\"Videos\"},\"3g2\":{\"folderName\":\"Videos\"},\"3gpp\":{\"folderName\":\"Videos\"},\"3gpp2\":{\"folderName\":\"Videos\"},\"ogv\":{\"folderName\":\"Videos\"},\"ogx\":{\"folderName\":\"Videos\"},\"wmv\":{\"folderName\":\"Videos\"},\"asf\":{\"folderName\":\"Videos\"},\"webm\":{\"folderName\":\"Videos\"},\"flv\":{\"folderName\":\"Videos\"},\"avi\":{\"folderName\":\"Videos\"},\"OP1a\":{\"folderName\":\"Videos\"},\"OP-Atom\":{\"folderName\":\"Videos\"},\"ts\":{\"folderName\":\"Videos\"},\"lxf\":{\"folderName\":\"Videos\"},\"gxf\":{\"folderName\":\"Videos\"},\"vob\":{\"folderName\":\"Videos\"},\"mp3\":{\"folderName\":\"Music\"},\"aa\":{\"folderName\":\"Music\"},\"aac\":{\"folderName\":\"Music\"},\"aax\":{\"folderName\":\"Music\"},\"act\":{\"folderName\":\"Music\"},\"aiff\":{\"folderName\":\"Music\"},\"amr\":{\"folderName\":\"Music\"},\"ape\":{\"folderName\":\"Music\"},\"au\":{\"folderName\":\"Music\"},\"awb\":{\"folderName\":\"Music\"},\"dct\":{\"folderName\":\"Music\"},\"dss\":{\"folderName\":\"Music\"},\"dvf\":{\"folderName\":\"Music\"},\"flac\":{\"folderName\":\"Music\"},\"gsm\":{\"folderName\":\"Music\"},\"iklax\":{\"folderName\":\"Music\"},\"ivs\":{\"folderName\":\"Music\"},\"m4p\":{\"folderName\":\"Music\"},\"mmf\":{\"folderName\":\"Music\"},\"mpc\":{\"folderName\":\"Music\"},\"msv\":{\"folderName\":\"Music\"},\"nmf\":{\"folderName\":\"Music\"},\"nsf\":{\"folderName\":\"Music\"},\"ogg\":{\"folderName\":\"Music\"},\"oga\":{\"folderName\":\"Music\"},\"mogg\":{\"folderName\":\"Music\"},\"opus\":{\"folderName\":\"Music\"},\"ra\":{\"folderName\":\"Music\"},\"rm\":{\"folderName\":\"Music\"},\"raw\":{\"folderName\":\"Music\"},\"sln\":{\"folderName\":\"Music\"},\"tta\":{\"folderName\":\"Music\"},\"vox\":{\"folderName\":\"Music\"},\"wav\":{\"folderName\":\"Music\"},\"wma\":{\"folderName\":\"Music\"},\"wv\":{\"folderName\":\"Music\"},\"8svx\":{\"folderName\":\"Music\"},\"jfif\":{\"folderName\":\"Images\"},\"bmp\":{\"folderName\":\"Images\"},\"gif\":{\"folderName\":\"Images\"},\"jpeg\":{\"folderName\":\"Images\"},\"jpg\":{\"folderName\":\"Images\"},\"png\":{\"folderName\":\"Images\"},\"bpg\":{\"folderName\":\"Images\"},\"deep\":{\"folderName\":\"Images\"},\"drw\":{\"folderName\":\"Images\"},\"ecw\":{\"folderName\":\"Images\"},\"fits\":{\"folderName\":\"Images\"},\"flif\":{\"folderName\":\"Images\"},\"ico\":{\"folderName\":\"Images\"},\"ilbm\":{\"folderName\":\"Images\"},\"img\":{\"folderName\":\"Images\"},\"nrrd\":{\"folderName\":\"Images\"},\"pam\":{\"folderName\":\"Images\"},\"pcx\":{\"folderName\":\"Images\"},\"pgf\":{\"folderName\":\"Images\"},\"sgi\":{\"folderName\":\"Images\"},\"sid\":{\"folderName\":\"Images\"},\"tga\":{\"folderName\":\"Images\"},\"cd5\":{\"folderName\":\"Images\"},\"cpt\":{\"folderName\":\"Images\"},\"psd\":{\"folderName\":\"Images\"},\"xcf\":{\"folderName\":\"Images\"},\"pdn\":{\"folderName\":\"Images\"},\"cgm\":{\"folderName\":\"Images\"},\"afdesign\":{\"folderName\":\"Images\"},\"al\":{\"folderName\":\"Images\"},\"cdr\":{\"folderName\":\"Images\"},\"gem\":{\"folderName\":\"Images\"},\"hpgl\":{\"folderName\":\"Images\"},\"hvif\":{\"folderName\":\"Images\"},\"athml\":{\"folderName\":\"Images\"},\"naplps\":{\"folderName\":\"Images\"},\"odg\":{\"folderName\":\"Images\"},\"qcc\":{\"folderName\":\"Images\"},\"regis\":{\"folderName\":\"Images\"},\"vml\":{\"folderName\":\"Images\"},\"xar\":{\"folderName\":\"Images\"},\"xps\":{\"folderName\":\"Images\"},\"amf\":{\"folderName\":\"Images\"},\"blend\":{\"folderName\":\"Images\"},\"dgn\":{\"folderName\":\"Images\"},\"dwg\":{\"folderName\":\"Images\"},\"dxf\":{\"folderName\":\"Images\"},\"flt\":{\"folderName\":\"Images\"},\"fvrml\":{\"folderName\":\"Images\"},\"hsf\":{\"folderName\":\"Images\"},\"iges\":{\"folderName\":\"Images\"},\"imml\":{\"folderName\":\"Images\"},\"ipa\":{\"folderName\":\"Images\"},\"webp\":{\"folderName\":\"Images\"},\"jt\":{\"folderName\":\"Images\"},\"ma\":{\"folderName\":\"Images\"},\"mb\":{\"folderName\":\"Images\"},\"ob\":{\"folderName\":\"Images\"},\"ply\":{\"folderName\":\"Images\"},\"prc\":{\"folderName\":\"Images\"},\"step\":{\"folderName\":\"Images\"},\"skp\":{\"folderName\":\"Images\"},\"stl\":{\"folderName\":\"Images\"},\"u3d\":{\"folderName\":\"Images\"},\"vrml\":{\"folderName\":\"Images\"},\"xaml\":{\"folderName\":\"Images\"},\"xgl\":{\"folderName\":\"Images\"},\"xvl\":{\"folderName\":\"Images\"},\"xvrml\":{\"folderName\":\"Images\"},\"x3d\":{\"folderName\":\"Images\"},\"3d\":{\"folderName\":\"Images\"},\"3df\":{\"folderName\":\"Images\"},\"3ds\":{\"folderName\":\"Images\"},\"3dxml\":{\"folderName\":\"Images\"},\"jps\":{\"folderName\":\"Images\"},\"svg\":{\"folderName\":\"Images\"},\"txt\":{\"folderName\":\"Documents\\Text\"},\"rtf\":{\"folderName\":\"Documents\\Text\"},\"info\":{\"folderName\":\"Documents\\Text\"},\"prn\":{\"folderName\":\"Documents\\Text\"},\"md\":{\"folderName\":\"Documents\\Text\"},\"zip\":{\"folderName\":\"Compressions\"},\"iso\":{\"folderName\":\"Compressions\"},\"rar\":{\"folderName\":\"Compressions\"},\"7z\":{\"folderName\":\"Compressions\"},\"gz\":{\"folderName\":\"Compressions\"},\"dmg\":{\"folderName\":\"Compressions\"},\"exe\":{\"folderName\":\"Apps\\Windows\"},\"msi\":{\"folderName\":\"Apps\\Windows\"},\"apk\":{\"folderName\":\"Apps\\Android\"},\"py\":{\"folderName\":\"Documents\\Scripts\"},\"pyc\":{\"folderName\":\"Documents\\Scripts\"},\"js\":{\"folderName\":\"Documents\\Scripts\"},\"json\":{\"folderName\":\"Documents\\Scripts\"},\"html\":{\"folderName\":\"Documents\\Scripts\"},\"xml\":{\"folderName\":\"Documents\\Scripts\"},\"css\":{\"folderName\":\"Documents\\Scripts\"},\"c\":{\"folderName\":\"Documents\\Scripts\"},\"cs\":{\"folderName\":\"Documents\\Scripts\"},\"r\":{\"folderName\":\"Documents\\Scripts\"},\"java\":{\"folderName\":\"Documents\\Scripts\"},\"cmd\":{\"folderName\":\"Documents\\Scripts\"},\"bat\":{\"folderName\":\"Documents\\Scripts\"},\"db\":{\"folderName\":\"Documents\\Scripts\"},\"sql\":{\"folderName\":\"Documents\\Scripts\"},\"dll\":{\"folderName\":\"Documents\\Scripts\"},\"pdf\":{\"folderName\":\"Documents\\Office\\PDF\"},\"docx\":{\"folderName\":\"Documents\\Office\\Word\"},\"docm\":{\"folderName\":\"Documents\\Office\\Word\"},\"doc\":{\"folderName\":\"Documents\\Office\\Word\"},\"dotm\":{\"folderName\":\"Documents\\Office\\Word\"},\"dotx\":{\"folderName\":\"Documents\\Office\\Word\"},\"dot\":{\"folderName\":\"Documents\\Office\\Word\"},\"csv\":{\"folderName\":\"Documents\\Office\\Excel\"},\"xlsm\":{\"folderName\":\"Documents\\Office\\Excel\"},\"xlm\":{\"folderName\":\"Documents\\Office\\Excel\"},\"xls\":{\"folderName\":\"Documents\\Office\\Excel\"},\"pptx\":{\"folderName\":\"Documents\\Office\\Presentation\"},\"pptm\":{\"folderName\":\"Documents\\Office\\Presentation\"},\"ppt\":{\"folderName\":\"Documents\\Office\\Presentation\"},\"pot\":{\"folderName\":\"Documents\\Office\\Presentation\"},\"potx\":{\"folderName\":\"Documents\\Office\\Presentation\"},\"potm\":{\"folderName\":\"Documents\\Office\\Presentation\"},\"pps\":{\"folderName\":\"Documents\\Office\\Presentation\"},\"ppsm\":{\"folderName\":\"Documents\\Office\\Presentation\"},\"ppa\":{\"folderName\":\"Documents\\Office\\Presentation\"},\"ppam\":{\"folderName\":\"Documents\\Office\\Presentation\"},\"eml\":{\"folderName\":\"Documents\\Office\\Presentation\"},\"vsix\":{\"folderName\":\"Others\\Program Files\"},\"ttf\":{\"folderName\":\"Fonts\\ttf\"},\"otf\":{\"folderName\":\"Fonts\\otf\"},\"woff2\":{\"folderName\":\"Fonts\\woff2\"},\"torrent\":{\"folderName\":\"Others\\Torrents\"},\"ini\":{\"folderName\":\"Others\\Configurations\"},\"net\":{\"folderName\":\"Others\\Internet Files\"},\"ai\":{\"folderName\":\"Others\\Unknown\"},\"eps\":{\"folderName\":\"Others\\Unknown\"},\"nfo\":{\"folderName\":\"Information\"}}";
        public static string InternetCheckUrl = "http://github.com";
        public static string CatalogUrl = "https://raw.githubusercontent.com/yogesh-aggarwal/folder-prettifier/master/catalog.json";
        public static string CatalogFileName = "folder_prettifier_catalog.json";
        public static string NoticesUrl = "https://raw.githubusercontent.com/yogesh-aggarwal/folder-prettifier/master/notice.json";
        public static string NoticesFileName = "folder_prettifier_notice.json";
    }

    public class Data
    {
        public static dynamic catalog;

        public static async void FetchCatalog()
        {
            string result = "";
            if (Tools.CheckInternetConnection())
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    using (HttpResponseMessage response = await client.GetAsync(Resources.CatalogUrl))
                    using (HttpContent content = response.Content)
                    {
                        result = await content.ReadAsStringAsync();
                        using (StreamWriter sw = File.CreateText($@"{Path.GetTempPath()}{Resources.CatalogFileName}"))
                        {
                            sw.WriteLine(result);
                        }
                    }
                }
                catch { }
            }
            else
            {
                try
                {
                    using (StreamReader sr = File.OpenText($@"{Path.GetTempPath()}{Resources.CatalogFileName}"))
                    {
                        string s = "";
                        while ((s = sr.ReadLine()) != null)
                        {
                            result += s;
                        }
                    }
                }
                catch { }
            }

            if (result == "")
            {
                result = Resources.BasicCatalog;
            }

            try
            {
                catalog = JsonConvert.DeserializeObject(result);
            }
            catch
            {
                //status.Text = "No catalog! Can't proceed";
                return;
            }
        }
    }

    /*
    public class NotificationAction
    {
        public string name;
        public string action;
        public ToastActivationType type;
    }

    public class Notification
    { 
        public static void Simple(string title, string content, int seconds = 4)
        {
            ToastNotifier ToastNotifier = ToastNotificationManager.CreateToastNotifier();
            Windows.Data.Xml.Dom.XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            Windows.Data.Xml.Dom.XmlNodeList toastNodeList = toastXml.GetElementsByTagName("text");
            toastNodeList.Item(0).AppendChild(toastXml.CreateTextNode(title));
            toastNodeList.Item(1).AppendChild(toastXml.CreateTextNode(content));
            Windows.Data.Xml.Dom.IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            Windows.Data.Xml.Dom.XmlElement audio = toastXml.CreateElement("audio");
            audio.SetAttribute("src", "ms-winsoundevent:Notification.SMS");

            ToastNotification toast = new ToastNotification(toastXml);
            toast.ExpirationTime = DateTime.Now.AddSeconds(seconds);
            //ToastNotifier.Show(toast);
        }

        public static void WithIcon(string title, string content, string iconUri, int seconds = 4)
        {
            var toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText() { Text = title },
                            new AdaptiveText() { Text = content },
                        },
                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = iconUri,
                            HintCrop = ToastGenericAppLogoCrop.Circle
                        }
                    }
                },
                Launch = "action=viewFriendRequest&userId=49183"
            };

            // Create the toast notification
            var toastNotif = new ToastNotification(toastContent.GetXml());

            // And send the notification
            ToastNotificationManager.CreateToastNotifier().Show(toastNotif);
        }

        public static void WithIconAndAction(string title, string content, string iconUri,
                                             NotificationAction primaryAction, int seconds = 4)
        {
            var toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText() { Text = title },
                            new AdaptiveText() { Text = content },
                        },
                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = iconUri,
                            HintCrop = ToastGenericAppLogoCrop.Circle
                        }
                    }
                },
                Actions = new ToastActionsCustom()
                {
                    Buttons =
                    {
                        new ToastButton(primaryAction.name, primaryAction.action)
                        {
                            ActivationType = primaryAction.type
                        },
                    },
                },
            };

            // Create the toast notification
            var toastNotif = new ToastNotification(toastContent.GetXml());

            // And send the notification
            ToastNotificationManager.CreateToastNotifier().Show(toastNotif);
        }
        
        public static void WithImageAndAction(string title, string content, string imageUri,
                                             NotificationAction primaryAction, int seconds = 4)
        {
            var toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText() { Text = title },
                            new AdaptiveText() { Text = content },
                            new AdaptiveImage() { Source = "https://unsplash.it/360/202?image=883" },
                        }
                    }
                },
                Actions = new ToastActionsCustom()
                {
                    Buttons =
                    {
                        new ToastButton(primaryAction.name, primaryAction.action.ToString())
                        {
                            ActivationType = ToastActivationType.Foreground
                        },
                    },
                },
            };

            // Create the toast notification
            var toastNotif = new ToastNotification(toastContent.GetXml());

            // And send the notification
            ToastNotificationManager.CreateToastNotifier().Show(toastNotif);
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
                //dynamic value = Storage.localSettings.Values[key];
                //if (value == null) throw new NullReferenceException();
                return Storage.localSettings.Values[key];
            }
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
            {
                Set(key, defaultValue);
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
    */
}

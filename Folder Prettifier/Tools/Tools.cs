using System;
using Windows.System;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;

namespace Folder_Prettifier.Tools
{
    public class Tools
    {
        async static public void OpenURL(string uri)
        {
            await Launcher.LaunchUriAsync(new Uri(uri));
        }
    }

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
}

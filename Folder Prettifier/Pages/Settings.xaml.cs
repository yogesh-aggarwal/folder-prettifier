using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Folder_Prettifier.Dialogs;
using Folder_Prettifier.Tools;

namespace Folder_Prettifier.Pages
{
    public sealed partial class Settings : Page
    {
        private bool isThemeChangedFirstTime = false;
        public string theme = Tools.Storage.Get("Theme", "System Default").ToString();
        public bool isAddToPath = Tools.Storage.Get("IsAddToPath", false);
        public bool isInContextMenu = Tools.Storage.Get("IsInContextMenu", false);
        public bool isAutoStart = Tools.Storage.Get("IsAutoStart", true);
        public bool isAutoManage = Tools.Storage.Get("IsAutoManage", false);

        async public void ChangeTheme(object sender, SelectionChangedEventArgs e)
        {
            Tools.Storage.Set("Theme", ((ComboBox)sender).SelectedValue.ToString());

            if (!isThemeChangedFirstTime)
            {
                isThemeChangedFirstTime = true;
            }
            else
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Restart Required",
                    Content = "New theme has been applied. Please restart the application to continue...",
                    CloseButtonText = "Okay"
                };
                await dialog.ShowAsync();
            }
        }

        private void ToggleAddToPath(object sender, RoutedEventArgs e)
        {
            Tools.Storage.Set("IsAddToPath", ((ToggleSwitch)sender).IsOn);
        }
        
        private void ToggleInContextMenu(object sender, RoutedEventArgs e)
        {
            Tools.Storage.Set("IsInContextMenu", ((ToggleSwitch)sender).IsOn);
        }

        private void ToggleAutoStart(object sender, RoutedEventArgs e)
        {
            Tools.Storage.Set("IsAutoStart", ((ToggleSwitch)sender).IsOn);
        }
        
        private void ToggleAutoManage(object sender, RoutedEventArgs e)
        {
            Tools.Storage.Set("IsAutoManage", ((ToggleSwitch)sender).IsOn);
        }

        public Settings()
        {
            this.InitializeComponent();
        }

        async private void ResetSettings(object sender, RoutedEventArgs e)
        {
            try
            {
                ContentDialog locationPromptDialog = new ContentDialog
                {
                    Title = "Reset Settings?",
                    Content = "Are you willing to remove all your preferred settings?",
                    CloseButtonText = "No",
                    PrimaryButtonText = "Yes, Proceed"
                };

                ContentDialogResult result = await locationPromptDialog.ShowAsync();
                if (result.Equals(ContentDialogResult.Primary))
                {
                    ContentDialog doneDialog = new ContentDialog
                    {
                        Title = "Settings Reset Done",
                        Content = "Settings are reseted successfully, now you can proceed!",
                        CloseButtonText = "Okay",
                    };
                    await doneDialog.ShowAsync();
                }
            }
            catch { }
        }
        
        async private void OpenAbout(object sender, RoutedEventArgs e)
        {
            AboutDialog aboutDialog = new AboutDialog();
            await aboutDialog.ShowAsync();
        }

        async private void OpenLicense(object sender, RoutedEventArgs e)
        {
            LicenseDialog licenseDialog = new LicenseDialog();
            await licenseDialog.ShowAsync();
        }

        async private void OpenPrivacyPolicy(object sender, RoutedEventArgs e)
        {
            PrivacyPolicyDialog privacyPolicyDialog= new PrivacyPolicyDialog();
            await privacyPolicyDialog.ShowAsync();
        }

        private void OpenWebsite(object sender, RoutedEventArgs e)
        {
            Tools.Tools.OpenURL("https://folderprettifier.web.app");
        }
    }
}

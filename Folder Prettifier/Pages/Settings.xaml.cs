using Folder_Prettifier.Dialogs;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Folder_Prettifier.Pages
{
    public sealed partial class Settings : Page
    {
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

        async private void OpenLicense(object sender, RoutedEventArgs e)
        {
            LicenseDialog licenseDialog = new LicenseDialog();
            await licenseDialog.ShowAsync();
        }
        
        private void OpenWebsite(object sender, RoutedEventArgs e)
        {
            Tools.Tools.OpenURL("https://folderprettifier.web.app");
        }
    }
}

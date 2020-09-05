using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Folder_Prettifier.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
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
    }
}

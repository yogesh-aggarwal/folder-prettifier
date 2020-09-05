using System;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Controls;
using Folder_Prettifier.Dialogs;

namespace Folder_Prettifier
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            // Navigate to Manage Folders [1]
            navigationView.SelectedItem = navigationView.MenuItems[1];
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            Windows.UI.Xaml.Controls.NavigationViewItem selectedItemTag = (Windows.UI.Xaml.Controls.NavigationViewItem)args.SelectedItem;
            if (args.IsSettingsSelected)
            {
                contentFrame.Navigate(Type.GetType("Folder_Prettifier.Pages.Settings"));
            }
            else
            {
                contentFrame.Navigate(Type.GetType($"Folder_Prettifier.Pages.{selectedItemTag.Tag}"));
            }
        }

        async private void LoginUser(object sender, TappedRoutedEventArgs e)
        {
            LoginDialog login = new LoginDialog();
            await login.ShowAsync();
        }
    }
}

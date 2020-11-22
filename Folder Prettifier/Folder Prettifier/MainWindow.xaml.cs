using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Controls;
using Folder_Prettifier.Dialogs;

namespace Folder_Prettifier
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            contentFrame.Navigate(Type.GetType("Folder_Prettifier.Pages.ManageFoldersPage"));
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            try
            {
                NavigationViewItem selectedItemTag = (NavigationViewItem)args.SelectedItem;
                if (args.IsSettingsSelected)
                {
                    contentFrame.Navigate(Type.GetType("Folder_Prettifier.Pages.Settings"));
                }
                else
                {
                    contentFrame.Navigate(Type.GetType($"Folder_Prettifier.Pages.{selectedItemTag.Tag}"));
                }
            }
            catch { }
        }

        async private void LoginUser(object sender, TappedRoutedEventArgs e)
        {
            LoginDialog login = new LoginDialog();
            await login.ShowAsync();
        }
    }
}

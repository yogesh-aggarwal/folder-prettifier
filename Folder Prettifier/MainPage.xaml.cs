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
using Folder_Prettifier.Tools;


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

        private void Login(object sender, TappedRoutedEventArgs e)
        {
            Tools.Tools.Login();
        }
    }
}

using Folder_Prettifier.Dialogs.ManageFolders;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Folder_Prettifier.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ManageFoldersPage : Page
    {
        public ManageFoldersPage()
        {
            this.InitializeComponent();
        }

        async private void OpenManageTasksDialog(object sender, RoutedEventArgs e)
        {
            ReplaceTasksDialog replaceTasksDialog = new ReplaceTasksDialog();
            await replaceTasksDialog.ShowAsync();
        }

        async private void OpenCustomizeCatalogDialog(object sender, RoutedEventArgs e)
        {
            CustomizeCatalogDialog customizeCatalogDialog = new CustomizeCatalogDialog();
            await customizeCatalogDialog.ShowAsync();
        }
    }
}

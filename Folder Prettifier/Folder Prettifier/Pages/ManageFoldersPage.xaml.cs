using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Folder_Prettifier.Tools;
using Folder_Prettifier.Dialogs.ManageFolders;

namespace Folder_Prettifier.Pages
{
    public sealed partial class ManageFoldersPage : Page
    {
        public ManageFoldersPage()
        {
            this.InitializeComponent();
            InitailizeState();
        }

        private void InitailizeState()
        {
            // NAME MANIPULATION
            isNameManipulation.IsOn = ManageFolderManageData.isNameManipulation;
            AutoNameManipulation autoNameManipulation = ManageFolderManageData.nameManipulation;
            if (autoNameManipulation == AutoNameManipulation.Capitalize)
            {
                capitalize.IsChecked = true;
            }
            else if (autoNameManipulation == AutoNameManipulation.AntiCapitalize)
            {
                antiCapitalize.IsChecked = true;
            }
            else if (autoNameManipulation == AutoNameManipulation.AllCapital)
            {
                allCapital.IsChecked = true;
            }
            else if (autoNameManipulation == AutoNameManipulation.AllSmall)
            {
                allSmall.IsChecked = true;
            }
            else if (autoNameManipulation == AutoNameManipulation.None)
            {
                none.IsChecked = true;
            }

            // CATEGORIZE FILES
            isCategorize.IsOn = ManageFolderManageData.isCategorize;
            AutoCategorizeFiles autoCategorizeFiles = ManageFolderManageData.categorizeFiles;
            if (autoCategorizeFiles == AutoCategorizeFiles.Extensions)
            {
                extensions.IsChecked = true;
            }
            else if (autoCategorizeFiles == AutoCategorizeFiles.Date)
            {
                date.IsChecked = true;
            }
        }

        async private void OpenManageTasksDialog(object sender, RoutedEventArgs e)
        {
            ReplaceTasksDialog replaceTasksDialog = new ReplaceTasksDialog();
            await replaceTasksDialog.ShowAsync();
        }

        async private void OpenManageFolders(object sender, RoutedEventArgs e)
        {
            ManageFoldersDialog replaceTasksDialog = new ManageFoldersDialog();
            await replaceTasksDialog.ShowAsync();
        }

        async private void OpenCustomizeCatalogDialog(object sender, RoutedEventArgs e)
        {
            CustomizeCatalogDialog customizeCatalogDialog = new CustomizeCatalogDialog();
            await customizeCatalogDialog.ShowAsync();
        }

        private void StartManagement(object sender, RoutedEventArgs e)
        {
            ManageFolderManageData.nameAppend.AtStart = nameStartsWith.Text;
            ManageFolderManageData.nameAppend.AtEnd = nameEndsWith.Text;

            // Temporary: Get from user folder entries
            List<string> folders = new List<string>();
            folders.Add(@"D:\Downloads\Test\Prettify");

            //try
            //{
            Tools.Manage.Start(folders);
            //}
            //catch { }
        }

        ////////////////////////////////////////////////////////////
        //////////////////////// UI MANAGEMENT /////////////////////
        ////////////////////////////////////////////////////////////

        // Manage Folder UI Part
        // Name Manipulation
        public void ToggleNameManipulation(object sender, RoutedEventArgs e)
        {
            ManageFolderManageData.isNameManipulation = isNameManipulation.IsOn;
        }

        public void SelectCapitalize(object sender, RoutedEventArgs e)
        {
            ManageFolderManageData.nameManipulation = AutoNameManipulation.Capitalize;
        }

        public void SelectAntiCapitalize(object sender, RoutedEventArgs e)
        {
            ManageFolderManageData.nameManipulation = AutoNameManipulation.AntiCapitalize;
        }

        public void SelectAllCapital(object sender, RoutedEventArgs e)
        {
            ManageFolderManageData.nameManipulation = AutoNameManipulation.AllCapital;
        }

        public void SelectAllSmall(object sender, RoutedEventArgs e)
        {
            ManageFolderManageData.nameManipulation = AutoNameManipulation.AllSmall;
        }

        public void SelectNone(object sender, RoutedEventArgs e)
        {
            ManageFolderManageData.nameManipulation = AutoNameManipulation.None;
        }

        // Categorize
        public void ToggleCategorize(object sender, RoutedEventArgs e)
        {
            ManageFolderManageData.isCategorize = isCategorize.IsOn;
        }

        public void SelectExtensions(object sender, RoutedEventArgs e)
        {
            ManageFolderManageData.categorizeFiles = AutoCategorizeFiles.Extensions;
        }

        public void SelectDate(object sender, RoutedEventArgs e)
        {
            ManageFolderManageData.categorizeFiles = AutoCategorizeFiles.Date;
        }
    }
}

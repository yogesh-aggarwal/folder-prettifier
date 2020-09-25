using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Folder_Prettifier.Dialogs.ManageFolders;
using Folder_Prettifier.Tools;

namespace Folder_Prettifier.Pages
{
    class ManageFolderManageData
    {
        public static bool isNameManipulation = true;
        public static AutoNameManipulation nameManipulation = AutoNameManipulation.Capitalize;

        public static NameAppend nameAppend = new NameAppend();

        public static bool isCategorize = true;
        public static AutoCategorizeFiles categorizeFiles = AutoCategorizeFiles.Extensions;
    }

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

        async private void OpenCustomizeCatalogDialog(object sender, RoutedEventArgs e)
        {
            CustomizeCatalogDialog customizeCatalogDialog = new CustomizeCatalogDialog();
            await customizeCatalogDialog.ShowAsync();
        }

        async private void StartManagement(object sender, RoutedEventArgs e)
        {
            ManageFolderManageData.nameAppend.AtStart = nameStartsWith.Text;
            ManageFolderManageData.nameAppend.AtEnd = nameEndsWith.Text;

            //try
            //{
                await Tools.Manage.Start(ManageFolderManageData.isNameManipulation,
                                   ManageFolderManageData.nameManipulation,
                                   ManageFolderManageData.nameAppend,
                                   ManageFolderManageData.isCategorize,
                                   ManageFolderManageData.categorizeFiles);
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

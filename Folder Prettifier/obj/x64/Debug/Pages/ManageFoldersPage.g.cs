﻿#pragma checksum "A:\UWP\Folder Prettifier\Folder Prettifier\Pages\ManageFoldersPage.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "E174D7C5F19F6D58E36777F5CAF0126457E2A84EA0936C0C2E15D6F1AE2D1363"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Folder_Prettifier.Pages
{
    partial class ManageFoldersPage : 
        global::Windows.UI.Xaml.Controls.Page, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.19041.1")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 2: // Pages\ManageFoldersPage.xaml line 84
                {
                    global::Windows.UI.Xaml.Controls.Button element2 = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)element2).Click += this.OpenCustomizeCatalogDialog;
                }
                break;
            case 3: // Pages\ManageFoldersPage.xaml line 71
                {
                    this.extensions = (global::Windows.UI.Xaml.Controls.RadioButton)(target);
                    ((global::Windows.UI.Xaml.Controls.RadioButton)this.extensions).Click += this.SelectExtensions;
                }
                break;
            case 4: // Pages\ManageFoldersPage.xaml line 72
                {
                    this.date = (global::Windows.UI.Xaml.Controls.RadioButton)(target);
                    ((global::Windows.UI.Xaml.Controls.RadioButton)this.date).Click += this.SelectDate;
                }
                break;
            case 5: // Pages\ManageFoldersPage.xaml line 67
                {
                    this.isCategorize = (global::Windows.UI.Xaml.Controls.ToggleSwitch)(target);
                    ((global::Windows.UI.Xaml.Controls.ToggleSwitch)this.isCategorize).Toggled += this.ToggleCategorize;
                }
                break;
            case 6: // Pages\ManageFoldersPage.xaml line 68
                {
                    global::Windows.UI.Xaml.Controls.TextBlock element6 = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                    ((global::Windows.UI.Xaml.Controls.TextBlock)element6).Tapped += this.ToggleCategorize;
                }
                break;
            case 7: // Pages\ManageFoldersPage.xaml line 59
                {
                    global::Windows.UI.Xaml.Controls.Button element7 = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)element7).Click += this.OpenManageTasksDialog;
                }
                break;
            case 8: // Pages\ManageFoldersPage.xaml line 52
                {
                    this.nameEndsWith = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                }
                break;
            case 9: // Pages\ManageFoldersPage.xaml line 48
                {
                    this.nameStartsWith = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                }
                break;
            case 10: // Pages\ManageFoldersPage.xaml line 37
                {
                    this.capitalize = (global::Windows.UI.Xaml.Controls.RadioButton)(target);
                    ((global::Windows.UI.Xaml.Controls.RadioButton)this.capitalize).Click += this.SelectCapitalize;
                }
                break;
            case 11: // Pages\ManageFoldersPage.xaml line 38
                {
                    this.antiCapitalize = (global::Windows.UI.Xaml.Controls.RadioButton)(target);
                    ((global::Windows.UI.Xaml.Controls.RadioButton)this.antiCapitalize).Click += this.SelectAntiCapitalize;
                }
                break;
            case 12: // Pages\ManageFoldersPage.xaml line 39
                {
                    this.allCapital = (global::Windows.UI.Xaml.Controls.RadioButton)(target);
                    ((global::Windows.UI.Xaml.Controls.RadioButton)this.allCapital).Click += this.SelectAllCapital;
                }
                break;
            case 13: // Pages\ManageFoldersPage.xaml line 40
                {
                    this.allSmall = (global::Windows.UI.Xaml.Controls.RadioButton)(target);
                    ((global::Windows.UI.Xaml.Controls.RadioButton)this.allSmall).Click += this.SelectAllSmall;
                }
                break;
            case 14: // Pages\ManageFoldersPage.xaml line 41
                {
                    this.none = (global::Windows.UI.Xaml.Controls.RadioButton)(target);
                    ((global::Windows.UI.Xaml.Controls.RadioButton)this.none).Click += this.SelectNone;
                }
                break;
            case 15: // Pages\ManageFoldersPage.xaml line 30
                {
                    this.isNameManipulation = (global::Windows.UI.Xaml.Controls.ToggleSwitch)(target);
                    ((global::Windows.UI.Xaml.Controls.ToggleSwitch)this.isNameManipulation).Toggled += this.ToggleNameManipulation;
                }
                break;
            case 16: // Pages\ManageFoldersPage.xaml line 31
                {
                    global::Windows.UI.Xaml.Controls.TextBlock element16 = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                    ((global::Windows.UI.Xaml.Controls.TextBlock)element16).Tapped += this.ToggleNameManipulation;
                }
                break;
            case 17: // Pages\ManageFoldersPage.xaml line 23
                {
                    global::Windows.UI.Xaml.Controls.Button element17 = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)element17).Click += this.StartManagement;
                }
                break;
            default:
                break;
            }
            this._contentLoaded = true;
        }

        /// <summary>
        /// GetBindingConnector(int connectionId, object target)
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.19041.1")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Windows.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Windows.UI.Xaml.Markup.IComponentConnector returnValue = null;
            return returnValue;
        }
    }
}


using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Folder_Prettifier.Dialogs
{
    public sealed partial class UserBenefits : ContentDialog
    {
        Action callback;

        public void DefaultCallback()
        {

        }

        public UserBenefits(Action callback = null)
        {
            this.InitializeComponent();
            if (callback == null) callback = DefaultCallback;
            this.callback = callback;
        }

        private void CloseDialog(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Hide();
            callback();
        }
    }
}

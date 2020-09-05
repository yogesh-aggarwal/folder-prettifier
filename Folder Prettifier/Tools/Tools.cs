using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Folder_Prettifier.Tools
{
    class Tools
    {
        async static public void Login()
        {
            Console.WriteLine("Login the user!");
            ContentDialog dialog = new ContentDialog
            {
                Title= "Successfully logged in",
                Content="You have logged in succefully & can take benifits of the restricted services",
                CloseButtonText="Got it"
            };
            await dialog.ShowAsync();
        }
    }
}

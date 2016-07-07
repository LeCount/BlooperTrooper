using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using SharedResources;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        ///<summary>
        /// Login to server
        ///</summary>
        public static bool LoginToServer(string username, string password)
        {
            MessageBox.Show(username + password);
            
            return true;
        }


    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace ideaSpaceApplication
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
    #if DEBUG
        public App()
        {
            StartupUri = new System.Uri("projectWindow.xaml", UriKind.Relative);
            Run();
        }
    #endif
    }
}
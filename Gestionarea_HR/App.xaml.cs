using System;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Microsoft.UI;

namespace Gestionarea_HR
{
    public partial class App : Application
    {
        private Window? loginWindow;

        public static Window? CurrentWindow { get; private set; }

        public App()
        {
            this.InitializeComponent();
            

        }

      

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            loginWindow = new Window();
            loginWindow.Title = "Gestionarea HR";

            var frame = new Frame();
            loginWindow.Content = frame;
            frame.Navigate(typeof(LoginPage));

            CurrentWindow = loginWindow;

            loginWindow.Activate();
        }

       

        

    }
}
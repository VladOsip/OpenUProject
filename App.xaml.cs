using System;
using System.Windows;
using MySchoolYear.View;
using MySchoolYear.ViewModel;
using MySchoolYear.ViewModel.Utilities;

namespace MySchoolYear
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Startup the program with its login window
        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            // Create the Login View and attach its ViewModel
            LoginWindow loginScreen = new LoginWindow();
            loginScreen.DataContext = new LoginViewModel(Application.Current.Resources["MessageBoxService"] as IMessageBoxService);
            loginScreen.Show();
        }
    }
}

using System;
using System.Windows;
using MySchoolYear.View.Utilities;

namespace MySchoolYear.View
{
    /// <summary>
    /// Interaction logic for ApplicationMainWindow.xaml
    /// </summary>
    public partial class ApplicationMainWindow : Window, IClosableScreen
    {
        public ApplicationMainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Close this window
        /// </summary>
        public void CloseScreen()
        {
            base.Close();
        }
    }
}

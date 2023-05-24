using System.Security;
using System.Windows;
using MySchoolYear.View.Utilities;

namespace MySchoolYear.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window, IHavePassword, IClosableScreen
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The secure password for this login page
        /// </summary>
        public SecureString SecurePassword => PasswordText.SecurePassword;

        /// <summary>
        /// Confirmation secure password for this login page - doesn't actually exists so resending password
        /// </summary>
        public SecureString ConfirmationSecurePassword => PasswordText.SecurePassword;

        /// <summary>
        /// Close this window
        /// </summary>
        public void CloseScreen()
        {
            base.Close();
        }
    }
}

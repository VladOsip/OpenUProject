using System.Security;
using System.Windows;
using MySchoolYear.View.Utilities;

namespace MySchoolYear.ViewModel
{
    /// <summary>
    /// Interaction logic for NewPasswordWindow.xaml
    /// </summary>
    public partial class NewPasswordWindow : Window, IHavePassword, IDialogWindow
    {
        public NewPasswordWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The secure password for this dialog window
        /// </summary>
        public SecureString SecurePassword => PasswordText.SecurePassword;

        /// <summary>
        /// The confirmation secure password for this dialog window
        /// </summary>
        public SecureString ConfirmationSecurePassword => ConfirmationPasswordText.SecurePassword;

        /// <summary>
        /// Closes this dialog window with the requested result
        /// </summary>
        /// <param name="result">DialogResult value</param>
        public void CloseDialogWindow(bool result)
        {
            this.DialogResult = result;
        }
    }
}

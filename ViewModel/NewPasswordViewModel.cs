using System;
using System.Security;
using System.Windows.Input;
using MySchoolYear.ViewModel.Utilities;
using MySchoolYear.View.Utilities;
using MySchoolYear.Model;

namespace MySchoolYear.ViewModel
{
    public class NewPasswordViewModel : BaseViewModel
    {
        #region Fields
        private ICommand _setPasswordCommand;
        private bool _isConfirmationPasswordInvalid;
        private bool _isPasswordInvalid;

        private SchoolEntities _context;
        private User _user;
        #endregion

        #region Properties
        public bool IsPasswordInvalid
        {
            get
            {
                return _isPasswordInvalid;
            }
            set
            {
                _isPasswordInvalid = value;
                OnPropertyChanged("IsPasswordInvalid");
            }
        }
        
        public bool IsConfirmationPasswordInvalid
        {
            get
            {
                return _isConfirmationPasswordInvalid;
            }
            set
            {
                _isConfirmationPasswordInvalid = value;
                OnPropertyChanged("IsConfirmationPasswordInvalid");
            }
        }

        public ICommand SetPasswordCommand
        {
            get
            {
                if (_setPasswordCommand == null)
                {
                    _setPasswordCommand = new RelayCommand(
                        p => SetPassword(p),
                        p => p is IHavePassword);
                }

                return _setPasswordCommand;
            }
        }
        #endregion

        #region Constructor
        public NewPasswordViewModel(SchoolEntities context, User connectedUser)
        {
            _context = context;
            _user = connectedUser;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Attempt to set the user password's
        /// </summary>
        /// <param name="parameter">The source window of the View that implement IHavePassword and IDialogWindow</param>
        private void SetPassword(object parameter)
        {
            SecureString password = (parameter as IHavePassword).SecurePassword;
            SecureString confirmationPassword = (parameter as IHavePassword).ConfirmationSecurePassword;

            // Reset error flags
            IsPasswordInvalid = false;
            IsConfirmationPasswordInvalid = false;

            // Check if the password is valid
            if (password.Length < Globals.MINIMUM_PASSWORD_LENGTH || password.Length > Globals.MAXIMUM_PASSWORD_LENGTH)
            {
                IsPasswordInvalid = true;
            }
            // Check if the confirmation password is the same as the actual password
            else if (password.Unsecure() != confirmationPassword.Unsecure())
            {
                IsConfirmationPasswordInvalid = true;
            }
            else
            {
                // Password is valid. Save it and return.
                // Note that the password is saved in plain-text for convenience and simplification. In an actual product it would stay secured.
                _user.password = password.Unsecure();
                _user.hasToChangePassword = false;
                _context.SaveChanges();
                (parameter as IDialogWindow).CloseDialogWindow(true);
            }
        }
        #endregion
    }
}

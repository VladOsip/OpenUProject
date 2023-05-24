using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security;
using System.Windows;
using System.Windows.Input;
using MySchoolYear.Model;
using MySchoolYear.View;
using MySchoolYear.View.Utilities;
using MySchoolYear.ViewModel.Utilities;

namespace MySchoolYear.ViewModel
{
    /// <summary>
    /// ViewModel for the login process logic.
    /// </summary>
    public class LoginViewModel : BaseViewModel
    {
        #region Fields
        private SchoolEntities _mySchoolModel;
        private ICommand _loginCommand;
        #endregion

        #region Properties
        public string Username { get; set; }
        public ICommand LoginCommand 
        {
            get
            {
                if (_loginCommand == null)
                {
                    _loginCommand = new RelayCommand(
                        p => AttemptLoginCommand(p),
                        p => p is IHavePassword && p is IClosableScreen);
                }

                return _loginCommand;
            }
        }
        #endregion

        #region Constructors
        public LoginViewModel(IMessageBoxService messageBoxService)
            : base(messageBoxService)
        {
            _mySchoolModel = new SchoolEntities();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Try to login with the input username and password.
        /// </summary>
        /// <param name="parameter">IHavePassword object that contains the SecureString input password, and that is also IClosableScreen</param>
        private void AttemptLoginCommand(object parameter)
        {
            SecureString password = (parameter as IHavePassword).SecurePassword;
            var validInput = CheckLoginInputValidity(Username, password);

            IClosableScreen thisScreen = (parameter as IClosableScreen);
            
            if (validInput.Valid)
            {
                // Unsecure the password to compare it to the DB - in a proper implementation, the hashed passwords would be compared, but for simplicity
                // we check it unsecured (and the DB holds passwords in plain text for the same reason).
                var unsecuredPassword = password.Unsecure();

                // Search for the user in the DB.
                User connectedAccount = _mySchoolModel.Users.SingleOrDefault(user => user.username == Username && user.password == unsecuredPassword);

                // If the user is found, connect as it and open the application.
                if (connectedAccount != null && !connectedAccount.isDisabled)
                {
                    // Ask the user to change his password before continuing
                    if (connectedAccount.hasToChangePassword)
                    {
                        NewPasswordWindow newPasswordDialog = new NewPasswordWindow();
                        NewPasswordViewModel newPasswordDialogVM = new NewPasswordViewModel(_mySchoolModel, connectedAccount);
                        newPasswordDialog.DataContext = newPasswordDialogVM;
                        newPasswordDialog.ShowDialog();
                    }

                    // Launch the application main window with the connected user account
                    SwitchToTheMainApplication(thisScreen, connectedAccount.Person.Single());
                }
                // Report incorrect user credentials error.
                else
                {
                    _messageBoxService.ShowMessage("שם המשתמש ו/או הסיסמא שגויים!", "Login Failed!", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
                }
            }
            // Report invalid credentials error.
            else
            {
                _messageBoxService.ShowMessage(validInput.ErrorReport, "Login Failed!", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
        }

        private void SwitchToTheMainApplication(IClosableScreen thisLoginScreen, Person connectedUser)
        {
            ApplicationMainWindow appMainWindow = new ApplicationMainWindow();
            ApplicationViewModel context = new ApplicationViewModel(connectedUser, _messageBoxService);
            appMainWindow.DataContext = context;
            appMainWindow.Show();

            // Close this Login Window 
            thisLoginScreen.CloseScreen();
        }

        /// <summary>
        /// Assistant method to check the validity of the username and password.
        /// </summary>
        /// <param name="username">The Username to check</param>
        /// <param name="password">Secured version of the Password to check</param>
        /// <returns>The validity tests result</returns>
        private ValidityResult CheckLoginInputValidity(string username, SecureString password)
        {
            ValidityResult result = new ValidityResult();
            result.Valid = true;

            // Did the user write a username
            if (username == null || username.Length == 0)
            {
                result.ErrorReport = "אנא הכנס שם משתמש";
                result.Valid = false;
            }
            // Is the username valid
            else if (username.Length < Globals.MINIMUM_USERNAME_LENGTH || username.Length > Globals.MAXIMUM_USERNAME_LENGTH)
            {
                result.ErrorReport = string.Format("שם משתמש לא תקין. אורך שם המשתמש חייב להיות בין {0} לבין {1} תווים",
                                                    Globals.MINIMUM_USERNAME_LENGTH, Globals.MAXIMUM_USERNAME_LENGTH);
                result.Valid = false;
            }
            // Did the user write a password
            else if (password == null || password.Length == 0)
            {
                result.ErrorReport = "אנא הכנס סיסמא";
                result.Valid = false;
            }
            // Is the password valid
            else if (password.Length < Globals.MINIMUM_PASSWORD_LENGTH || password.Length > Globals.MAXIMUM_PASSWORD_LENGTH)
            {
                result.ErrorReport = string.Format("סיסמא לא תקינה. אורך הסיסמא חייב להיות בין {0} לבין {1} תווים",
                                                    Globals.MINIMUM_PASSWORD_LENGTH, Globals.MAXIMUM_PASSWORD_LENGTH);
                result.Valid = false;
            }

            return result;
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MySchoolYear.Model;
using MySchoolYear.ViewModel.Utilities;

namespace MySchoolYear.ViewModel
{
    /// <summary>
    /// Creating and sending a message
    /// </summary>
    public class CreateMessageViewModel : BaseViewModel, IScreenViewModel
    {
        #region Fields
        private ICommand _refreshDataCommand;
        private ICommand _sendMessageCommand;

        private SchoolEntities _schoolData;

        private ObservableDictionary<int, string> _recipients;
        private int _selectedRecipient;

        private bool _sendingToStudent;
        private bool _sendingToTeacher;
        private bool _sendingToClass;
        private bool _sendingToParent;
        private bool _sendingToManagement;
        private bool _sendingToEveryone;

        private bool _canSendToClass;
        private bool _canSendToEveryone;

        private string _messageTitle;
        private string _messageText;

        private int NOT_ASSIGNED = -1;
        private int EVERYONE_OPTION = 0;
        #endregion

        #region Properties / Commands
        // Base Properties
        public Person ConnectedPerson { get; private set; }
        public bool HasRequiredPermissions { get; }
        public string ScreenName { get { return "שליחת הודעה"; } }

        // Business Logic Properties / Commands
        public ObservableDictionary<int, string> Recipients
        {
            get
            {
                return _recipients;
            }
            set
            {
                if (_recipients != value)
                {
                    _recipients = value;
                    OnPropertyChanged("Recipients");
                }
            }
        }
        public int SelectedRecipient 
        { 
            get
            {
                return _selectedRecipient;
            }
            set
            {
                if (_selectedRecipient != value)
                {
                    _selectedRecipient = value;
                    OnPropertyChanged("SelectedRecipient");
                }
            }
        }

        public bool SendingToStudent
        { 
            get 
            {
                return _sendingToStudent; 
            }
            set 
            {
                if (_sendingToStudent != value)
                {
                    _sendingToStudent = value;
                    OnPropertyChanged("SendingToStudent");

                    // Update recipients list if it is changing to this category
                    if (value == true)
                    {
                        UpdateRecipientsList(MessageRecipientsTypes.Students);
                    }
                }
            }
        }
        public bool SendingToTeacher
        {
            get
            {
                return _sendingToTeacher;
            }
            set
            {
                if (_sendingToTeacher != value)
                {
                    _sendingToTeacher = value;
                    OnPropertyChanged("SendingToTeacher");

                    // Update recipients list if it is changing to this category
                    if (value == true)
                    {
                        UpdateRecipientsList(MessageRecipientsTypes.Teachers);
                    }
                }
            }
        }
        public bool SendingToClass
        {
            get
            {
                return _sendingToClass;
            }
            set
            {
                if (_sendingToClass != value)
                {
                    _sendingToClass = value;
                    OnPropertyChanged("SendingToClass");

                    // Update recipients list if it is changing to this category
                    if (value == true)
                    {
                        UpdateRecipientsList(MessageRecipientsTypes.Class);
                    }
                }
            }
        }
        public bool SendingToParent
        {
            get
            {
                return _sendingToParent;
            }
            set
            {
                if (_sendingToParent != value)
                {
                    _sendingToParent = value;

                    // Update recipients list if it is changing to this category
                    if (value == true)
                    {
                        UpdateRecipientsList(MessageRecipientsTypes.Parent);
                    }

                    OnPropertyChanged("SendingToParent");
                }
            }
        }
        public bool SendingToManagement
        {
            get
            {
                return _sendingToManagement;
            }
            set
            {
                if (_sendingToManagement != value)
                {
                    _sendingToManagement = value;
                    OnPropertyChanged("SendingToManagement");

                    // Update recipients list if it is changing to this category
                    if (value == true)
                    {
                        UpdateRecipientsList(MessageRecipientsTypes.Management);
                    }
                }
            }
        }
        public bool SendingToEveryone
        {
            get
            {
                return _sendingToEveryone;
            }
            set
            {
                if (_sendingToEveryone != value)
                {
                    _sendingToEveryone = value;
                    OnPropertyChanged("SendingToEveryone");

                    // Update recipients list if it is changing to this category
                    if (value == true)
                    {
                        UpdateRecipientsList(MessageRecipientsTypes.Everyone);
                    }
                }
            }
        }

        public bool CanSendToClass
        {
            get
            {
                return _canSendToClass;
            }
            set
            {
                if (_canSendToClass != value)
                {
                    _canSendToClass = value;
                    OnPropertyChanged("CanSendToClass");
                }
            }
        }
        public bool CanSendToEveryone
        {
            get
            {
                return _canSendToEveryone;
            }
            set
            {
                if (_canSendToEveryone != value)
                {
                    _canSendToEveryone = value;
                    OnPropertyChanged("CanSendToEveryone");
                }
            }
        }

        public string MessageTitle
        {
            get
            {
                return _messageTitle;
            }
            set
            {
                if (_messageTitle != value)
                {
                    _messageTitle = value;
                    OnPropertyChanged("MessageTitle");
                }
            }
        }
        public string MessageText
        { 
            get
            {
                return _messageText;
            }
            set
            {
                if (_messageText != value)
                {
                    _messageText = value;
                    OnPropertyChanged("MessageText");
                }
            }
        }


        /// <summary>
        /// Create a new message with the current data and send it
        /// </summary>
        public ICommand SendMessageCommand
        {
            get
            {
                if (_sendMessageCommand == null)
                {
                    _sendMessageCommand = new RelayCommand(p => CreateNewMessage());
                }
                return _sendMessageCommand;
            }
        }
        #endregion

        #region Constructors
        public CreateMessageViewModel(Person connectedPerson, ICommand refreshDataCommand, IMessageBoxService messageBoxService)
            : base (messageBoxService)
        {
            _refreshDataCommand = refreshDataCommand;
            _schoolData = new SchoolEntities();

            // Set permissions
            HasRequiredPermissions = true;

            if (connectedPerson.isTeacher || connectedPerson.isPrincipal || connectedPerson.isSecretary)
            {
                CanSendToClass = true;
            }

            if (connectedPerson.isPrincipal || connectedPerson.isSecretary)
            {
                CanSendToEveryone = true;
            }
        }
        #endregion

        #region Methods
        public void Initialize(Person connectedPerson)
        {
            ConnectedPerson = connectedPerson;
            ResetAll();
        }

        /// <summary>
        /// Reset all the fields and selections in the form
        /// </summary>
        public void ResetAll()
        {
            Recipients = new ObservableDictionary<int, string>();

            // Select student category as the default category option
            SendingToStudent = true;
            SendingToClass = false;
            SendingToEveryone = false;
            SendingToTeacher = false;
            SendingToParent = false;
            SendingToManagement = false;

            MessageTitle = string.Empty;
            MessageText = string.Empty;
        }

        /// <summary>
        /// Update the recipients list following a selection of recipients category
        /// </summary>
        /// <param name="recipientsType">The category of recipients to use</param>
        private void UpdateRecipientsList(MessageRecipientsTypes recipientsType)
        {
            // Create the list of recipients for the current category selection
            CreateRecipientsList(recipientsType);

            // Select first available recipient in the list (if it has any)
            SelectedRecipient = (Recipients.Count() > 0) ? Recipients.First().Key : NOT_ASSIGNED;

            // For some reason, after re-initializing the recipients list, the selection is not updated properly unless called again
            OnPropertyChanged("SelectedRecipient");
        }

        /// <summary>
        /// Uses the current category selection and creates the list of recipients accordingly
        /// </summary>
        /// <param name="recipientsType">The category of recipients to use</param>
        private void CreateRecipientsList(MessageRecipientsTypes recipientsType)
        {
            // Reset the recipients list
            Recipients.Clear();

            // Create the list of students
            switch (recipientsType)
            {
                case MessageRecipientsTypes.Students:
                    // Adding a 'send to every student' option if the user has relevent permissions
                    if (CanSendToEveryone)
                    {
                        Recipients.Add(EVERYONE_OPTION, "כל התלמידים");
                    }

                    // Add every active student in the school
                    _schoolData.Persons.Where(person => !person.User.isDisabled && person.isStudent).ToList()
                        .ForEach(person => Recipients.Add(person.personID, person.firstName + " " + person.lastName));
                    break;
                case MessageRecipientsTypes.Parent:
                    // Add every parent in the school
                    _schoolData.Persons.Where(person => !person.User.isDisabled && person.isParent).ToList()
                        .ForEach(person => Recipients.Add(person.personID, person.firstName + " " + person.lastName));
                    break;
                case MessageRecipientsTypes.Teachers:
                    // Add a 'send to management' option if the user has relevent permissions
                    if (CanSendToEveryone)
                    {
                        Recipients.Add(EVERYONE_OPTION, "כל המורים");
                    }

                    // Add the teachers
                    _schoolData.Persons.Where(person => !person.User.isDisabled && person.isTeacher).ToList()
                        .ForEach(person => Recipients.Add(person.personID, person.firstName + " " + person.lastName));
                    break;
                case MessageRecipientsTypes.Management:
                    // Add a 'send to management' option if the user has relevent permissions
                    if (CanSendToEveryone)
                    {
                        Recipients.Add(EVERYONE_OPTION, "כל ההנהלה");
                    }

                    // Add the secretaries and principal
                    _schoolData.Persons.Where(person => !person.User.isDisabled && (person.isPrincipal || person.isSecretary)).ToList()
                        .ForEach(person => Recipients.Add(person.personID, person.firstName + " " + person.lastName));
                    break;
                case MessageRecipientsTypes.Class:
                    // Add every class in the school
                    _schoolData.Classes.ToList().ForEach(schoolClass => Recipients.Add(schoolClass.classID, schoolClass.className));
                    break;
                case MessageRecipientsTypes.Everyone:
                    Recipients.Add(EVERYONE_OPTION, "כל בית הספר");
                    break;
                default:
                    throw new ArgumentException("Invalid recipient type!");
            }
        }

        /// <summary>
        /// Create a new message with the current data and send it
        /// </summary>
        private void CreateNewMessage()
        {
            ValidityResult inputValid = IsInputValid();
            if (inputValid.Valid)
            {
                // Create a message depending on selected recipient category
                // Check first the options to send a message to everyone from a specific group
                if (SendingToStudent && SelectedRecipient == EVERYONE_OPTION)
                {
                    MessagesHandler.CreateMessage(MessageTitle, MessageText, MessageRecipientsTypes.Students, ConnectedPerson.personID);
                }
                else if (SendingToTeacher && SelectedRecipient == EVERYONE_OPTION)
                {
                    MessagesHandler.CreateMessage(MessageTitle, MessageText, MessageRecipientsTypes.Teachers, ConnectedPerson.personID);
                }
                else if (SendingToManagement && SelectedRecipient == EVERYONE_OPTION)
                {
                    MessagesHandler.CreateMessage(MessageTitle, MessageText, MessageRecipientsTypes.Management, ConnectedPerson.personID);
                }
                else if (SendingToEveryone)
                {
                    MessagesHandler.CreateMessage(MessageTitle, MessageText, MessageRecipientsTypes.Everyone, ConnectedPerson.personID);
                }
                // Handle messages for a specific class
                else if (SendingToClass)
                {
                    MessagesHandler.CreateMessage(MessageTitle, MessageText, MessageRecipientsTypes.Class, ConnectedPerson.personID, SelectedRecipient);
                }
                // All other messages are aimed for a specific person
                else
                {
                    MessagesHandler.CreateMessage(MessageTitle, MessageText, MessageRecipientsTypes.Person, ConnectedPerson.personID, SelectedRecipient);
                }

                // Report success
                _messageBoxService.ShowMessage("הודעה נוצרה בהצלחה ונשלחה ל" + Recipients[SelectedRecipient],
                                                "נשלחה הודעה", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);

                // Update all data
                _refreshDataCommand.Execute(null);
            }
            else
            {
                _messageBoxService.ShowMessage(inputValid.ErrorReport, "נכשל ביצירת הודעה", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
        }

        /// <summary>
        /// Checks if the input message is valid
        /// </summary>
        /// <returns>The validity of the input</returns>
        private ValidityResult IsInputValid()
        {
            ValidityResult result = new ValidityResult() { Valid = true };

            // Check if a recipient was selected
            if (SelectedRecipient == NOT_ASSIGNED)
            {
                result.Valid = false;
                result.ErrorReport = "לא נבחר למי לשלוח את ההודעה";
            }
            else if (MessageTitle == string.Empty)
            {
                result.Valid = false;
                result.ErrorReport = "לא הוזנה כותרת להודעה";
            }
            else if (MessageText == string.Empty)
            {
                result.Valid = false;
                result.ErrorReport = "לא הוזן תוכן להודעה";
            }

            return result;
        }
        #endregion
    }
}

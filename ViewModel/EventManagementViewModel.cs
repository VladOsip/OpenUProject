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
    /// Creating and sending an event
    /// </summary>
    public class EventManagementViewModel : BaseViewModel, IScreenViewModel
    {
        #region Sub-Structs
        private enum EventRecipientsTypes
        {
            Students,
            Classes,
            Everyone,
        }

        private enum ActionOnEvent
        { 
            Created,
            Deleted,
            Updated
        }

        public class EventData
        {
            public int ID { get; set; }
            public string EventName { get; set; }
            public string EventText { get; set; }
            public string EventLocation { get; set; }
            public string SubmitterName { get; set; }
            public DateTime EventDatetime { get; set; }
        }
        #endregion

        #region Fields
        private ICommand _refreshDataCommand;
        private ICommand _sendEventCommand;
        private ICommand _deleteEventCommand;
        private ICommand _updateEventCommand;

        private SchoolEntities _schoolData;

        private bool _searchingStudentEvents;
        private bool _searchingClassEvents;
        private bool _searchingSchoolEvents;
        private int _selectedSearchChoiceID;

        private ObservableDictionary<int, string> _availableSearchChoices;
        public ObservableCollection<EventData> _eventsTableData;
        public EventData _selectedEvent;

        private ObservableDictionary<int, string> _recipients;
        private int _selectedRecipient;

        private bool _sendingToStudent;
        private bool _sendingToClass;
        private bool _sendingToEveryone;

        private bool _canSendToEveryone;

        private ObservableCollection<string> _possibleEvents;
        private ObservableCollection<string> _possibleLocations;
        private DateTime _eventDatetime;
        private string _eventLocation;
        private string _eventName;
        private string _eventText;

        private int NOT_ASSIGNED = -1;
        private int EVERYONE_OPTION = 0;
        #endregion

        #region Properties / Commands
        // Base Properties
        public Person ConnectedPerson { get; private set; }
        public bool HasRequiredPermissions { get; }
        public string ScreenName { get { return "ניהול אירועים"; } }

        // Business Logic Properties / Commands
        public bool SearchingStudentEvents
        {
            get
            {
                return _searchingStudentEvents;
            }
            set
            {
                if (_searchingStudentEvents != value)
                {
                    _searchingStudentEvents = value;
                    OnPropertyChanged("SearchingStudentEvents");

                    if (value == true)
                    {
                        // Manually turn off the other search options before using this category, because WPF switches the new choice on before changing the other off,
                        // causing two options to be on at the same time, and in this case this causes an issue with the selected event
                        SearchingClassEvents = false;
                        SearchingSchoolEvents = false;
                        UseSelectedSearchCategory(EventRecipientsTypes.Students);
                    }
                }
            }
        }
        public bool SearchingClassEvents
        {
            get
            {
                return _searchingClassEvents;
            }
            set
            {
                if (_searchingClassEvents != value)
                {
                    _searchingClassEvents = value;
                    OnPropertyChanged("SearchingClassEvents");

                    if (value == true)
                    {
                        // Manually turn off the other search options before using this category, because WPF switches the new choice on before changing the other off,
                        // causing two options to be on at the same time, and in this case this causes an issue with the selected event
                        SearchingStudentEvents = false;
                        SearchingSchoolEvents = false;
                        UseSelectedSearchCategory(EventRecipientsTypes.Classes);
                    }
                }
            }
        }
        public bool SearchingSchoolEvents
        {
            get
            {
                return _searchingSchoolEvents;
            }
            set
            {
                if (_searchingSchoolEvents != value)
                {
                    _searchingSchoolEvents = value;
                    OnPropertyChanged("SearchingSchoolEvents");

                    if (value == true)
                    {
                        // Manually turn off the other search options before using this category, because WPF switches the new choice on before changing the other off,
                        // causing two options to be on at the same time, and in this case this causes an issue with the selected event
                        SearchingStudentEvents = false;
                        SearchingClassEvents = false;
                        UseSelectedSearchCategory(EventRecipientsTypes.Everyone);
                    }
                }
            }
        }

        public ObservableDictionary<int, string> AvailableSearchChoices
        {
            get
            {
                return _availableSearchChoices;
            }
            set
            {
                if (_availableSearchChoices != value)
                {
                    _availableSearchChoices = value;
                    OnPropertyChanged("AvailableSearchChoices");
                }
            }
        }
        public int SelectedSearchChoice
        {
            get
            {
                return _selectedSearchChoiceID;
            }
            set
            {
                // The ID value might not change when we switched categories (e.g choosing first item in different categories)
                // so update everytime the 'set' is called
                _selectedSearchChoiceID = value;
                UseSelectedSearchChoice();
                OnPropertyChanged("SelectedSearchChoice");
            }
        }

        public ObservableCollection<EventData> EventsTableData
        {
            get
            {
                return _eventsTableData;
            }
            set
            {
                if (_eventsTableData != value)
                {
                    _eventsTableData = value;
                    OnPropertyChanged("EventsTableData");
                }
            }
        }
        public EventData SelectedEvent
        {
            get
            {
                return _selectedEvent;
            }
            set
            {
                if (_selectedEvent != value)
                {
                    _selectedEvent = value;
                    UseSelectedEvent(_selectedEvent);
                    OnPropertyChanged("SelectedEvent");
                }
            }
        }

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
                        UpdateRecipientsList(EventRecipientsTypes.Students);
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
                        UpdateRecipientsList(EventRecipientsTypes.Classes);
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
                        UpdateRecipientsList(EventRecipientsTypes.Everyone);
                    }
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

        public DateTime EventDatetime
        {
            get
            {
                return _eventDatetime;
            }
            set
            {
                if (_eventDatetime != value)
                {
                    _eventDatetime = value;
                    OnPropertyChanged("EventDatetime");
                }
            }
        }

        public ObservableCollection<string> PossibleLocations
        {
            get
            {
                return _possibleLocations;
            }
            set
            {
                if (_possibleLocations != value)
                {
                    _possibleLocations = value;
                    OnPropertyChanged("PossibleLocations");
                }
            }
        }
        public string EventLocation
        {
            get
            {
                return _eventLocation;
            }
            set
            {
                if (_eventLocation != value)
                {
                    _eventLocation = value;
                    OnPropertyChanged("EventLocation");
                }
            }
        }

        public ObservableCollection<string> PossibleEvents 
        { 
            get
            {
                return _possibleEvents;
            }
            set
            {
                if (_possibleEvents != value)
                {
                    _possibleEvents = value;
                    OnPropertyChanged("PossibleEvents");
                }
            }
        }
        public string EventName
        {
            get
            {
                return _eventName;
            }
            set
            {
                if (_eventName != value)
                {
                    _eventName = value;
                    OnPropertyChanged("EventName");
                }
            }
        }

        public string EventText
        { 
            get
            {
                return _eventText;
            }
            set
            {
                if (_eventText != value)
                {
                    _eventText = value;
                    OnPropertyChanged("EventText");
                }
            }
        }

        /// <summary>
        /// Delete the currently selected event
        /// </summary>
        public ICommand DeleteEventCommand
        {
            get
            {
                if (_deleteEventCommand == null)
                {
                    _deleteEventCommand = new RelayCommand(p => DeleteSelectedEvent());
                }
                return _deleteEventCommand;
            }
        }

        /// <summary>
        /// Update the currently selected event
        /// </summary>
        public ICommand UpdateEventCommand
        {
            get
            {
                if (_updateEventCommand == null)
                {
                    _updateEventCommand = new RelayCommand(p => UpdateSelectedEvent());
                }
                return _updateEventCommand;
            }
        }

        /// <summary>
        /// Create a new event with the current data and send it
        /// </summary>
        public ICommand SendEventCommand
        {
            get
            {
                if (_sendEventCommand == null)
                {
                    _sendEventCommand = new RelayCommand(p => CreateNewEvent());
                }
                return _sendEventCommand;
            }
        }
        #endregion

        #region Constructors
        public EventManagementViewModel(Person connectedPerson, ICommand refreshDataCommand, IMessageBoxService messageBoxService)
        {
            _refreshDataCommand = refreshDataCommand;
            _messageBoxService = messageBoxService;

            // Set permissions
            if (connectedPerson.isTeacher || connectedPerson.isPrincipal || connectedPerson.isSecretary)
            {
                HasRequiredPermissions = true;

                if (connectedPerson.isPrincipal || connectedPerson.isSecretary)
                {
                    CanSendToEveryone = true;
                }
            }
            else
            {
                HasRequiredPermissions = false;
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
        private void ResetAll()
        {
            _schoolData = new SchoolEntities();
            AvailableSearchChoices = new ObservableDictionary<int, string>();
            EventsTableData = new ObservableCollection<EventData>();
            Recipients = new ObservableDictionary<int, string>();

            PossibleLocations = new ObservableCollection<string>(_schoolData.Rooms.Select(room => "חדר " + room.roomName).ToList());
            PossibleEvents =
                new ObservableCollection<string>(new List<string>() { "מבחן", "טיול", "חופשה","פגישה", "שינוי בשיעור", "טקס", "אירוע" });

            // Select student category as the default category option
            SearchingStudentEvents = true;
            SearchingClassEvents = false;
            SearchingSchoolEvents = false;
            SendingToStudent = true;
            SendingToClass = false;
            SendingToEveryone = false;

            SelectedSearchChoice = NOT_ASSIGNED;
            SelectedEvent = null;

            // Get the date of Tommarow as the default
            EventDatetime = DateTime.Today.AddDays(1);

            EventLocation = string.Empty;
            EventName = string.Empty;
            EventText = string.Empty;
        }

        /// <summary>
        /// Updates the AvailableSearchChoices list with the options per the search category
        /// </summary>
        private void UseSelectedSearchCategory(EventRecipientsTypes searchCategory)
        {
            AvailableSearchChoices = CreateRecipientsList(searchCategory);

            // Use the first choice in the AvailableSearchChoices dictioanry as the default choice
            if (AvailableSearchChoices.Count > 0)
            {
                SelectedSearchChoice = AvailableSearchChoices.First().Key;
            }
        }

        /// <summary>
        /// Updates the EventsTableData with the events of the SelectedSearchChoice 
        /// </summary>
        private void UseSelectedSearchChoice()
        {
            if (SelectedSearchChoice != NOT_ASSIGNED)
            {
                // Clean the events table before adding items to it
                EventsTableData.Clear();

                // Check which category the search choice is from, and fill the EventsTableData with the relevent events
                if (SearchingStudentEvents)
                {
                    // Find the student that was chosen, and get his/hers events (both his personnal and class's events)
                    Student student = _schoolData.Students.Find(SelectedSearchChoice);
                    _schoolData.Events.Where(schoolEvent => schoolEvent.recipientID == student.studentID || schoolEvent.recipientClassID == student.classID).ToList().
                        ForEach(schoolEvent => EventsTableData.Add(ModelEventToEventData(schoolEvent)));
                }
                else if (SearchingClassEvents)
                {
                    // Find all the events for the class that was chosen
                    _schoolData.Events.Where(schoolEvent => schoolEvent.recipientClassID == SelectedSearchChoice).ToList().
                        ForEach(schoolEvent => EventsTableData.Add(ModelEventToEventData(schoolEvent)));
                }
                else if (SearchingSchoolEvents)
                {
                    // Find all the events that are for the entire school
                    _schoolData.Events.Where(schoolEvent => schoolEvent.recipientClassID == null && schoolEvent.recipientID == null).ToList().
                        ForEach(schoolEvent => EventsTableData.Add(ModelEventToEventData(schoolEvent)));
                }
                else
                {
                    // Unknown search category - don't show any event
                }
            }
        }


        /// <summary>
        /// Choose a specific event and view its information.
        /// </summary>
        /// <param name="selectedEvent">The event's data</param>
        private void UseSelectedEvent(EventData selectedEvent)
        {
            // Update the properties per the selected lesson
            if (selectedEvent != null)
            {
                SendingToStudent = SearchingStudentEvents;
                SendingToClass = SearchingClassEvents;
                SendingToEveryone = SearchingSchoolEvents;
                EventDatetime = selectedEvent.EventDatetime;
                EventLocation = selectedEvent.EventLocation;
                EventName = selectedEvent.EventName;
                EventText = selectedEvent.EventText;
            }
            else
            {
                // No lesson was selected -> Reset the properties
                SendingToStudent = true;
                SendingToClass = false;
                SendingToEveryone = false;
                EventDatetime = DateTime.Today.AddDays(1);
                EventLocation = string.Empty;
                EventName = string.Empty;
                EventText = string.Empty;
            }
        }

        /// <summary>
        /// Converts the Model's event class into the local EventData class
        /// </summary>
        /// <param name="lesson">The Model's lesson</param>
        /// <returns>Corresponding LessonData version of the lesson</returns>
        private EventData ModelEventToEventData(Event schoolEvent)
        {
            EventData eventData = new EventData()
            {
                ID = schoolEvent.eventID,
                EventDatetime = schoolEvent.eventDate,
                EventLocation = schoolEvent.location,
                EventName = schoolEvent.name,
                EventText = schoolEvent.description,
                SubmitterName = schoolEvent.Submitter.firstName + " " + schoolEvent.Submitter.lastName
            };

            return eventData;
        }

        /// <summary>
        /// Update the recipients list following a selection of recipients category
        /// </summary>
        /// <param name="recipientsType">The category of recipients to use</param>
        private void UpdateRecipientsList(EventRecipientsTypes recipientsType)
        {
            // Create the list of recipients for the current category selection
            Recipients = CreateRecipientsList(recipientsType);

            // Select first available recipient in the list (if it has any)
            SelectedRecipient = (Recipients.Count() > 0) ? Recipients.First().Key : NOT_ASSIGNED;

            // For some reason, after re-initializing the recipients list, the selection is not updated properly unless called again
            OnPropertyChanged("SelectedRecipient");
        }

        /// <summary>
        /// Uses the current category selection and creates a dictionary of recipients accordingly
        /// </summary>
        /// <param name="recipientsType">The category of recipients to use</param>
        /// <returns>An ObservableDictionary with all the relevent recipients (organized as <RecipientID, RecipientName>)</returns>
        private ObservableDictionary<int, string> CreateRecipientsList(EventRecipientsTypes recipientsType)
        {
            // Create a recipients dictioanry
            ObservableDictionary<int, string> recipients = new ObservableDictionary<int, string>();

            // Create the list of students
            switch (recipientsType)
            {
                case EventRecipientsTypes.Students:
                    // Add every active student in the school
                    _schoolData.Persons.Where(person => !person.User.isDisabled && person.isStudent).ToList()
                        .ForEach(person => recipients.Add(person.personID, person.firstName + " " + person.lastName));
                    break;
                case EventRecipientsTypes.Classes:
                    // Add every class in the school
                    _schoolData.Classes.ToList().ForEach(schoolClass => recipients.Add(schoolClass.classID, schoolClass.className));
                    break;
                case EventRecipientsTypes.Everyone:
                    recipients.Add(EVERYONE_OPTION, "כל בית הספר");
                    break;
                default:
                    throw new ArgumentException("Invalid recipient type!");
            }

            return recipients;
        }

        /// <summary>
        /// Delete the currently selected event
        /// </summary>
        private void DeleteSelectedEvent()
        {
            // Check that a room was selected
            if (SelectedEvent != null)
            {
                // Get the lesson that is going to be deleted
                Event selectedEvent = _schoolData.Events.Find(SelectedEvent.ID);

                // As this is a serious action, request a confirmation from the user
                bool confirmation = _messageBoxService.ShowMessage("האם אתה בטוח שברצונך למחוק את האירוע?",
                                                                    "מחיקת אירוע!", MessageType.ACCEPT_CANCEL_MESSAGE, MessagePurpose.INFORMATION);
                if (confirmation == true)
                {
                    // Remove the lesson
                    _schoolData.Events.Remove(selectedEvent);

                    // Save and report changes
                    _schoolData.SaveChanges();
                    SendMessageAboutEvent(selectedEvent, ActionOnEvent.Deleted);
                    _messageBoxService.ShowMessage("האירוע נמחק בהצלחה!",
                            "מחיקת אירוע!", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);
                    _refreshDataCommand.Execute(null);
                }
            }
            else
            {
                _messageBoxService.ShowMessage("אנא בחר אירוע קודם כל.",
                                               "נכשל במחיקת אירוע", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
        }

        /// <summary>
        /// Update the currently selected event
        /// </summary>
        private void UpdateSelectedEvent()
        {
            // Check that an event was selected
            if (SelectedEvent != null)
            {
                // Check the input validity was selected
                ValidityResult inputValid = IsInputValid();
                if (inputValid.Valid)
                {
                    // Get the event that is going to be edited
                    Event selectedEvent = _schoolData.Events.Find(SelectedEvent.ID);

                    // Update the event's data
                    selectedEvent.submitterID = ConnectedPerson.personID;
                    selectedEvent.eventDate = EventDatetime;
                    selectedEvent.location = EventLocation;
                    selectedEvent.name = EventName;
                    selectedEvent.description = EventText;
                    SetEventRecipients(selectedEvent);

                    // Update the model
                    _schoolData.SaveChanges();

                    // Report action success
                    SendMessageAboutEvent(selectedEvent, ActionOnEvent.Updated);
                    _messageBoxService.ShowMessage("האירוע עודכן בהצלחה!", "עודכן אירוע", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);

                    // Update data in all screens
                    _refreshDataCommand.Execute(null);
                }
                else
                {
                    _messageBoxService.ShowMessage(inputValid.ErrorReport, "נכשל בעדכון אירוע", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
                }
            }
            else
            {
                _messageBoxService.ShowMessage("אנא בחר אירוע קודם כל.",
                                               "נכשל בעדכון אירוע", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
        }

        /// <summary>
        /// Create a new event with the current data and send it
        /// </summary>
        private void CreateNewEvent()
        {
            ValidityResult inputValid = IsInputValid();
            if (inputValid.Valid)
            {
                // Create a new event
                Event newEvent = new Event()
                {
                    eventDate = EventDatetime,
                    location = EventLocation,
                    name = EventName,
                    description = EventText,
                    submitterID = ConnectedPerson.personID
                };
                SetEventRecipients(newEvent);

                // Save the event
                _schoolData.Events.Add(newEvent);
                _schoolData.SaveChanges();

                // Report success
                SendMessageAboutEvent(newEvent, ActionOnEvent.Created);
                _messageBoxService.ShowMessage("אירוע נוצר בהצלחה ונשלח ל" + Recipients[SelectedRecipient],
                                                "נשלח אירוע", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);

                // Update all data
                _refreshDataCommand.Execute(null);
            }
            else
            {
                _messageBoxService.ShowMessage(inputValid.ErrorReport, "נכשל ביצירת אירוע", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
        }

        /// <summary>
        /// Set an event's recipients per the selected recipient category
        /// </summary>
        /// <param name="schoolEvent"></param>
        private void SetEventRecipients(Event schoolEvent)
        {
            // Check first the options to send an event to everyone
            if (SendingToEveryone)
            {
                // Event is for everyone if it doesn't have a recipientID or recipientClassID
                schoolEvent.recipientID = null;
                schoolEvent.recipientClassID = null;
            }
            // Handle messages for a specific class
            else if (SendingToClass)
            {
                schoolEvent.recipientID = null;
                schoolEvent.recipientClassID = SelectedRecipient;
            }
            // All other events are aimed for a specific person
            else
            {
                schoolEvent.recipientID = SelectedRecipient;
                schoolEvent.recipientClassID = null;
            }
        }

        /// <summary>
        /// Send a message to the relevent recipients about an update in an event
        /// </summary>
        /// <param name="schoolEvent">The event to report</param>
        private void SendMessageAboutEvent(Event schoolEvent, ActionOnEvent action)
        {
            string eventTitle;
            string eventMessage;

            // Create the event message & title depedning on the type of action that happens to the event
            switch (action)
            {
                case ActionOnEvent.Created:
                {
                    eventTitle = "הוזן אירוע חדש ביומן!";
                    eventMessage = ConnectedPerson.firstName + " " + ConnectedPerson.lastName 
                        + " יצר את האירוע '" + schoolEvent.name + "' בתאריך " + schoolEvent.eventDate;
                    break;
                }
                case ActionOnEvent.Deleted:
                {
                    eventTitle = "נמחק אירוע!";
                    eventMessage = ConnectedPerson.firstName + " " + ConnectedPerson.lastName
                        + " מחק את האירוע '" + schoolEvent.name + "' שהיה בתאריך " + schoolEvent.eventDate;
                    break;
                }
                case ActionOnEvent.Updated:
                {
                    eventTitle = "עודכנו פרטי אירוע!";
                    eventMessage = ConnectedPerson.firstName + " " + ConnectedPerson.lastName
                        + " עדכן את האירוע '" + schoolEvent.name + "' שבתאריך " + schoolEvent.eventDate;
                    break;
                }
                default:
                {
                    throw new ArgumentException("Invalid ActionOnEvent type");
                }
            }

            // Check to whom to send the message depending on the recipients of the event
            // Check if its a school event (has no specified recipients)
            if (schoolEvent.recipientID == null && schoolEvent.recipientClassID == null)
            {
                MessagesHandler.CreateMessage(eventTitle, eventMessage, MessageRecipientsTypes.Everyone);
            }
            // Check if its an event for a specific class
            else if (schoolEvent.recipientClassID != null)
            {
                MessagesHandler.CreateMessage(eventTitle, eventMessage, MessageRecipientsTypes.Class, null, schoolEvent.recipientClassID.Value);
            }
            // All other events are aimed for a specific person
            else
            {
                MessagesHandler.CreateMessage(eventTitle, eventMessage, MessageRecipientsTypes.Person, null, schoolEvent.recipientID.Value);
            }
        }
        
        /// <summary>
        /// Checks if the input for the event is valid
        /// </summary>
        /// <returns>The validity of the input</returns>
        private ValidityResult IsInputValid()
        {
            ValidityResult result = new ValidityResult() { Valid = true };

            // Check if a recipient was selected
            if (SelectedRecipient == NOT_ASSIGNED)
            {
                result.Valid = false;
                result.ErrorReport = "לא נבחר למי לשלוח את האירוע";
            }
            else if (EventDatetime == null)
            {
                result.Valid = false;
                result.ErrorReport = "לא הוזן תאריך לאירוע";
            }
            else if (EventName == string.Empty)
            {
                result.Valid = false;
                result.ErrorReport = "לא הוזנה כותרת לאירוע";
            }
            else if (EventText == string.Empty)
            {
                result.Valid = false;
                result.ErrorReport = "לא הוזן תוכן לאירוע";
            }

            return result;
        }
        #endregion
    }
}

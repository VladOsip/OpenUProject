using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MySchoolYear.Model;
using MySchoolYear.ViewModel.Utilities;

namespace MySchoolYear.ViewModel
{
    /// <summary>
    /// Shows the user a calender with his upcoming events
    /// </summary>
    public class CalenderViewModel : BaseViewModel, IScreenViewModel
    {
        #region Fields
        private SchoolEntities _schoolData;
        private Jarloo.Calendar.Calendar _calender;

        private ICommand _updateCalenderCommand;
        private ICommand _updateSelectedDayCommand;

        private List<string> _months;
        private string _selectedMonth;

        private List<Event> _selectedDayEvents;
        private Event _selectedEvent;
        #endregion

        #region Properties / Commands
        // Base Properties
        public Person ConnectedPerson { get; private set; }
        public bool HasRequiredPermissions { get; }
        public string ScreenName { get { return "לוח שנה"; } }

        // Business Logic Properties
        public List<string> Months
        {
            get
            {
                if (_months == null)
                {
                    _months = new List<string>();
                }

                return _months;
            }
        }

        public string SelectedMonth
        {
            get
            {
                return _selectedMonth;
            }
            set
            {
                if (_selectedMonth != value && Months.Contains(value))
                {
                    _selectedMonth = value;
                    OnPropertyChanged("SelectedMonth");
                }
            }
        }

        public List<Event> SelectedDayEvents
        { 
            get
            {
                return _selectedDayEvents;
            }
            set
            {
                if (_selectedDayEvents != value)
                {
                    _selectedDayEvents = value;
                    OnPropertyChanged("SelectedDayEvents");
                }
            }
        }
        public Event SelectedEvent
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
                    OnPropertyChanged("SelectedEvent");
                }
            }
        }

        /// <summary>
        /// Update the calender to show the current selected month's events
        /// </summary>
        public ICommand UpdateCalenderCommand
        {
            get
            {
                if (_updateCalenderCommand == null)
                {
                    _updateCalenderCommand = new RelayCommand(p => UpdateCalender((Jarloo.Calendar.Calendar)p),
                                                              p => p is Jarloo.Calendar.Calendar);
                }
                return _updateCalenderCommand;
            }
        }
        /// <summary>
        /// Display the events of the selected day
        /// </summary>
        public ICommand UpdateSelectedDayCommand
        {
            get
            {
                if (_updateSelectedDayCommand == null)
                {
                    _updateSelectedDayCommand = new RelayCommand(p => GetSelectedDayEvents());
                }
                return _updateSelectedDayCommand;
            }
        }
        #endregion

        #region Constructors
        public CalenderViewModel(Person connectedPerson)
        {
            HasRequiredPermissions = true;

            if (HasRequiredPermissions)
            {
                _schoolData = new SchoolEntities();
                _months = new List<string> 
                    { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
                _selectedDayEvents = new List<Event>();
            }
        }
        #endregion

        #region Methods
        public void Initialize(Person connectedPerson)
        {
            ConnectedPerson = connectedPerson;

            // Use current month as the default. (Note that DateTime.Now.Month indexes from 1 to 12 and not 0-11)
            SelectedMonth = _months[DateTime.Now.Month - 1];
        }

        /// <summary>
        /// Update the calender to show data for the selected month
        /// </summary>
        /// <param name="calender">The visual calender</param>
        private void UpdateCalender(Jarloo.Calendar.Calendar calender)
        {
            DateTime targetDate = new DateTime(DateTime.Now.Year, Months.IndexOf(SelectedMonth) + 1, 1);
            calender.BuildCalendar(targetDate);

            // Get a list of relevent events to the current user
            List<Event> userEvents = GetRelevantEvents(calender.Days[0].DayDate, calender.Days.Last().DayDate);

            // Check over the displayed days and add their events 
            foreach (Event currentEvent in userEvents)
            {
                // Calculate number of day for the currentEvent
                int dayOffset = (currentEvent.eventDate - calender.Days[0].DayDate).Days;

                // Note the event's title at this day. 
                // If there are multiple events in the same day, space them out properly
                if (calender.Days[dayOffset].Notes != string.Empty && calender.Days[dayOffset].Notes != null)
                {
                    calender.Days[dayOffset].Notes += ", ";
                }
                calender.Days[dayOffset].Notes += currentEvent.name;
            }

            // Save the calender to use for determining selected day
            _calender = calender;
        }

        /// <summary>
        /// Create a list with all the events that are relevent to the current connected user and are within specific dates
        /// </summary>
        /// <param name="startingDate">The earliest relevent date</param>
        /// <param name="endDate">The latest relevent date</param>
        /// <returns></returns>
        private List<Event> GetRelevantEvents(DateTime startingDate, DateTime endDate)
        {
            // Check that the starting date is before the end date (otherwise it is meaningless)
            if (startingDate > endDate)
            {
                throw new ArgumentException("Invalid dates received when searching for events");
            }

            // Create the minimum query to only get events that are within the specified dates. Only compare dates (without time of day)
            // Note that LINQ cannot directly use the Date property of a Datetime so we need to use DBFunctions
            var startDateWithoutTime = startingDate.Date;
            var endDateWithoutTime = endDate.Date;
            var eventsQuery = 
                _schoolData.Events.Where(schoolEvent => (DbFunctions.TruncateTime(schoolEvent.eventDate) >= startDateWithoutTime &&
                                                         DbFunctions.TruncateTime(schoolEvent.eventDate) <= endDateWithoutTime));
            var aslist = eventsQuery.ToList();

            // Create a basic list with events that are for the entire school (have no specific recipient ID or class ID),
            // aswell as the events that were created by the current user, or are meant directly to him/her
            // Using HashSet to make sure the events are unique and not added multiple times
            HashSet<Event> userEvents = eventsQuery.Where(schoolEvent => 
                                                                (schoolEvent.recipientClassID == null && schoolEvent.recipientID == null) ||
                                                                schoolEvent.submitterID == ConnectedPerson.personID ||
                                                                schoolEvent.recipientID == ConnectedPerson.personID)
                                                                .ToHashSet();

            // Check the user's permissions and create the list of events accordingly
            if (ConnectedPerson.isStudent)
            {
                // Get the events of the student's class
                userEvents.UnionWith(eventsQuery.AsEnumerable().Where(schoolEvent =>
                                                       schoolEvent.recipientClassID == ConnectedPerson.Student.classID)
                                                       .ToHashSet());
            }
            else if (ConnectedPerson.isParent)
            {
                // Get all events of the parent's children classes
                userEvents.UnionWith(eventsQuery.AsEnumerable().Where(schoolEvent => 
                                                        ConnectedPerson.ChildrenStudents.Any(childStudent => 
                                                                                            childStudent.classID == schoolEvent.recipientClassID))
                                                        .ToHashSet());
            }
            else if (ConnectedPerson.isTeacher && ConnectedPerson.Teacher.classID != null)
            {
                // Show an homeroom teacher any event of his own class
                userEvents.UnionWith(eventsQuery.AsEnumerable().Where(schoolEvent => 
                                                        schoolEvent.recipientClassID == ConnectedPerson.Teacher.classID)
                                                        .ToHashSet());
            }
            else if (ConnectedPerson.isSecretary || ConnectedPerson.isPrincipal)
            {
                // Show every school event
                userEvents.UnionWith(eventsQuery.ToHashSet());
            }

            return userEvents.ToList();
        }

        /// <summary>
        /// Gather the information of events for the current day
        /// </summary>
        private void GetSelectedDayEvents()
        {
            // Get the selected day data
            var selectedDay = _calender.SelectedDay;

            if (selectedDay != null)
            {
                // get the events that are in the selected day
                SelectedDayEvents = GetRelevantEvents(selectedDay.DayDate, selectedDay.DayDate);
                SelectedEvent = SelectedDayEvents.FirstOrDefault();
            }
            else
            {
                SelectedDayEvents = new List<Event>();
                SelectedEvent = null;
            }
        }
        #endregion
    }
}

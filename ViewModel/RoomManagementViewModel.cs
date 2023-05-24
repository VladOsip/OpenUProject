using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MySchoolYear.Model;
using MySchoolYear.ViewModel.Utilities;

namespace MySchoolYear.ViewModel
{
    /// <summary>
    /// Manages the school's Rooms - creating, editing and deleting rooms
    /// </summary>
    public class RoomManagementViewModel : BaseViewModel, IScreenViewModel
    {
        #region Sub-Structs
        public class RoomData
        {
            public int ID { get; set; }
            public string Name { get; set; }
            
            public string HomeroomClassName { get; set; }
            public Nullable<int> HomeroomClassID { get; set; }
            
            public List<LessonsInRoom> LessonsInThisRoom { get; set; }
        }

        public class LessonsInRoom
        {
            public int LessonID { get; set; }
            public int ClassID { get; set; }
            public string CourseName { get; set; }
            public string ClassName { get; set; }
            public string DayFirstLesson { get; set; }
            public string HourFirstLesson { get; set; }
            public string DaySecondLesson { get; set; }
            public string HourSecondLesson { get; set; }
            public string DayThirdLesson { get; set; }
            public string HourThirdLesson { get; set; }
            public string DayFourthLesson { get; set; }
            public string HourFourthLesson { get; set; }
        }
        #endregion

        #region Fields
        private ICommand _refreshDataCommand;
        private ICommand _deleteRoomCommand;
        private ICommand _updateRoomCommand;
        private ICommand _createNewRoomCommand;

        private SchoolEntities _schoolData;

        private RoomData _selectedRoom;
        private string _selectedRoomName;
        private int _selectedClass;

        public ObservableCollection<RoomData> _roomsTableData;
        private ObservableDictionary<int, string> _availableClasses;
        private ObservableCollection<LessonsInRoom> _lessonsInSelectedRoom;

        private Nullable<int> _previousRoomClass;

        private const int NO_ASSIGNED_CLASS = -1;
        #endregion

        #region Properties / Commands
        // Base Properties
        public Person ConnectedPerson { get; private set; }
        public bool HasRequiredPermissions { get; }
        public string ScreenName { get { return "ניהול חדרים"; } }

        // Business Logic Properties
        public ObservableCollection<RoomData> RoomsTableData 
        { 
            get
            {
                return _roomsTableData;
            }
            set
            {
                if (_roomsTableData != value)
                {
                    _roomsTableData = value;
                    OnPropertyChanged("RoomsTableData");
                }
            }
        }
        public RoomData SelectedRoom 
        { 
            get
            {
                return _selectedRoom;
            }
            set
            {
                if (_selectedRoom != value)
                {
                    _selectedRoom = value;
                    UseSelectedRoom(_selectedRoom);
                    OnPropertyChanged("SelectedRoom");
                }
            }
        }
        public string RoomName 
        { 
            get
            {
                return _selectedRoomName;
            }
            set
            {
                if (_selectedRoomName != value)
                {
                    _selectedRoomName = value;
                    OnPropertyChanged("RoomName");
                }
            }
        }

        public ObservableDictionary<int, string> AvailableClasses
        {
            get
            {
                return _availableClasses;
            }
            set
            {
                if (_availableClasses != value)
                {
                    _availableClasses = value;
                    OnPropertyChanged("AvailableClasses");
                }
            }
        }
        public int SelectedClass
        {
            get
            {
                return _selectedClass;
            }
            set
            {
                if (_selectedClass != value)
                {
                    _selectedClass = value;
                    OnPropertyChanged("SelectedClass");
                }
            }
        }

        public ObservableCollection<LessonsInRoom> LessonsInSelectedRoom
        {
            get
            {
                return _lessonsInSelectedRoom;
            }
            set
            {
                if (_lessonsInSelectedRoom != value)
                {
                    _lessonsInSelectedRoom = value;
                    OnPropertyChanged("LessonsInSelectedRoom");
                }
            }
        }

        /// <summary>
        /// Delete the currently selected room
        /// </summary>
        public ICommand DeleteRoomCommand
        {
            get
            {
                if (_deleteRoomCommand == null)
                {
                    _deleteRoomCommand = new RelayCommand(p => DeleteSelectedRoom());
                }
                return _deleteRoomCommand;
            }
        }

        /// <summary>
        /// Update the currently selected room
        /// </summary>
        public ICommand UpdateRoomCommand
        {
            get
            {
                if (_updateRoomCommand == null)
                {
                    _updateRoomCommand = new RelayCommand(p => UpdateSelectedRoom());
                }
                return _updateRoomCommand;
            }
        }

        /// <summary>
        /// Create a new room with the current data
        /// </summary>
        public ICommand CreateNewRoomCommand
        {
            get
            {
                if (_createNewRoomCommand == null)
                {
                    _createNewRoomCommand = new RelayCommand(p => CreateNewRoom());
                }
                return _createNewRoomCommand;
            }
        }

        #endregion

        #region Constructors
        public RoomManagementViewModel(Person connectedPerson, ICommand refreshDataCommand, IMessageBoxService messageBoxService)
            : base(messageBoxService)
        {
            HasRequiredPermissions = connectedPerson.isSecretary || connectedPerson.isPrincipal;
            _refreshDataCommand = refreshDataCommand;

            if (HasRequiredPermissions)
            {
                _schoolData = new SchoolEntities();
                AvailableClasses = new ObservableDictionary<int, string>();
            }

        }
        #endregion

        #region Methods
        public void Initialize(Person connectedPerson)
        {
            ConnectedPerson = connectedPerson;

            // Get the list of existing rooms
            RoomsTableData = new ObservableCollection<RoomData>(_schoolData.Rooms.AsEnumerable().Select(room => ModelRoomToRoomData(room)).ToList());

            // Create the basic list of available classes
            AvailableClasses.Clear();

            // Add a 'No class' option, as not all rooms are assigned to a specific class
            AvailableClasses.Add(NO_ASSIGNED_CLASS, "אין כיתה משויכת");

            // Create the list of classes that don't have an homeroom already
            _schoolData.Classes.Where(schoolClass => schoolClass.roomID == null).ToList()
                .ForEach(schoolClass => AvailableClasses.Add(schoolClass.classID, schoolClass.className));

            SelectedClass = NO_ASSIGNED_CLASS;

            // For some reason, after re-initializing this view, the SelectedClass is not updated properly in the view unless called again
            OnPropertyChanged("SelectedClass"); 
        }

        /// <summary>
        /// Converts the Model's Room class into the local RoomData class
        /// </summary>
        /// <param name="room">The Model's room</param>
        /// <returns>Corresponding RoomData version of the room</returns>
        private RoomData ModelRoomToRoomData(Room room)
        {
            RoomData roomData = new RoomData();
            roomData.ID = room.roomID;
            roomData.Name = room.roomName;

            // Check if the room has an homeroom class
            if (room.Classes != null && room.Classes.Count > 0)
            {
                roomData.HomeroomClassID = room.Classes.Single().classID;
                roomData.HomeroomClassName = room.Classes.Single().className;
            }

            // Check if the room has lessons associated with it
            if (room.Lessons != null && room.Lessons.Count > 0)
            {
                roomData.LessonsInThisRoom = room.Lessons.Select(lesson => new LessonsInRoom()
                    {
                        LessonID = lesson.lessonID,
                        ClassID = lesson.classID,
                        ClassName = lesson.Class.className,
                        CourseName = lesson.Course.courseName,
                        DayFirstLesson = Globals.ConvertDayNumberToName(lesson.firstLessonDay),
                        DaySecondLesson = Globals.ConvertDayNumberToName(lesson.secondLessonDay),
                        DayThirdLesson = Globals.ConvertDayNumberToName(lesson.thirdLessonDay),
                        DayFourthLesson = Globals.ConvertDayNumberToName(lesson.fourthLessonDay),
                        HourFirstLesson = Globals.ConvertHourNumberToName(lesson.firstLessonHour),
                        HourSecondLesson = Globals.ConvertHourNumberToName(lesson.secondLessonHour),
                        HourThirdLesson = Globals.ConvertHourNumberToName(lesson.thirdLessonHour),
                        HourFourthLesson = Globals.ConvertHourNumberToName(lesson.fourthLessonHour)
                    }).ToList();
            }

            return roomData;
        }

        /// <summary>
        /// Choose a specific room and view its information.
        /// </summary>
        /// <param name="selectedRoom">The room's data</param>
        private void UseSelectedRoom(RoomData selectedRoom)
        {
            // Cleanup previous selections
            SelectedClass = NO_ASSIGNED_CLASS;
            RoomName = string.Empty;
            LessonsInSelectedRoom = new ObservableCollection<LessonsInRoom>();

            // Remove the previous room choice's class from the available classes list (as it is already assigned to that room)
            if (_previousRoomClass != null)
            {
                AvailableClasses.Remove(_previousRoomClass.Value);
            }

            // Update the properties per the selected room
            if (selectedRoom != null)
            {
                RoomName = selectedRoom.Name;

                // If the room has an homeroom, add it first to the available classes list
                if (selectedRoom.HomeroomClassID != null)
                {
                    AvailableClasses.Add(selectedRoom.HomeroomClassID.Value, selectedRoom.HomeroomClassName);
                    SelectedClass = selectedRoom.HomeroomClassID.Value;
                }

                // Save this room class ID so it can be removed from the available classes when we select another room
                _previousRoomClass = selectedRoom.HomeroomClassID;

                // Create the list of lessons in the current room
                if (selectedRoom.LessonsInThisRoom != null)
                {
                    LessonsInSelectedRoom = new ObservableCollection<LessonsInRoom>(selectedRoom.LessonsInThisRoom);
                }
            }
        }

        /// <summary>
        /// Delete the currently selected room
        /// </summary>
        private void DeleteSelectedRoom()
        {
            // Check that a room was selected
            if (SelectedRoom == null)
            {
                _messageBoxService.ShowMessage("אנא בחר חדר קודם כל.",
                                               "נכשל במחיקת חדר", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
            else
            {
                // Get the room that is going to be deleted
                Room selectedRoom = _schoolData.Rooms.Find(SelectedRoom.ID);

                // As this is a serious action, request a confirmation from the user
                bool confirmation = _messageBoxService.ShowMessage("האם אתה בטוח שברצונך למחוק את חדר " + selectedRoom.roomName + "?",
                                                                    "מחיקת חדר!", MessageType.ACCEPT_CANCEL_MESSAGE, MessagePurpose.INFORMATION);
                if (confirmation == true)
                {
                    // Remove the room from any associated class
                    Class previousClass = _schoolData.Classes.Where(schoolClass => schoolClass.roomID == selectedRoom.roomID).FirstOrDefault();
                    if (previousClass != null)
                    {
                        previousClass.roomID = null;
                    }
                    
                    // Clear the previous room property (as it was removed anyway)
                    this._previousRoomClass = null;

                    // Remove the room from any associated lessons
                    _schoolData.Lessons.Where(lesson => lesson.roomID == selectedRoom.roomID).ToList().ForEach(lesson => lesson.roomID = null);

                    // Delete the room itself
                    _schoolData.Rooms.Remove(selectedRoom);

                    // Save and report changes
                    _schoolData.SaveChanges();
                    _messageBoxService.ShowMessage("החדר " + selectedRoom.roomName + " נמחק בהצלחה!",
                            "מחיקת חדר!", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);
                    _refreshDataCommand.Execute(null);
                }
            }
        }

        /// <summary>
        /// Update the currently selected room
        /// </summary>
        private void UpdateSelectedRoom()
        {
            // Check that a room was selected
            if (SelectedRoom == null)
            {
                _messageBoxService.ShowMessage("אנא בחר חדר קודם כל.",
                                               "נכשל בעדכון חדר", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
            else
            {
                // Get the room that is going to be edited and the related class (if any)
                Room selectedRoom = _schoolData.Rooms.Find(SelectedRoom.ID);
                Class previousClass = _schoolData.Classes.Where(schoolClass=>schoolClass.roomID == selectedRoom.roomID).FirstOrDefault();
                Class selectedClass = _schoolData.Classes.Find(SelectedClass);

                // Check that the room's new name is unique
                if (_schoolData.Rooms.Where(room => room.roomID != selectedRoom.roomID && room.roomName == RoomName).Any())
                {
                    _messageBoxService.ShowMessage("שם החדר תפוס כבר! אנא תן שם חדש", "נכשל בעדכון חדר", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
                }
                // Check that the room selected class doesn't have another room already
                else if (SelectedClass != NO_ASSIGNED_CLASS && selectedClass.roomID != null && selectedClass.roomID != selectedRoom.roomID)
                {
                    _messageBoxService.ShowMessage("לכיתה שנבחרה יש כבר חדר אם. בטל את הבחירה או עדכן את הכיתה קודם.",
                                                   "נכשל בעדכון חדר", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
                }
                else
                {
                    // Update the room's data
                    selectedRoom.roomName = RoomName;

                    // Remove the room from its previous class (assuming it has changed)
                    if (previousClass != null && previousClass.roomID != SelectedClass)
                    {
                        previousClass.roomID = null;

                        // Clear the previous room property (as it was removed anyway)
                        this._previousRoomClass = null;
                    }

                    // Add the room to the selected class
                    if (SelectedClass != NO_ASSIGNED_CLASS)
                    {
                        // Update class to use this room's ID
                        selectedClass.roomID = selectedRoom.roomID; 
                    }

                    // Update the model
                    _schoolData.SaveChanges();

                    // Report action success
                    _messageBoxService.ShowMessage("החדר " + RoomName + " עודכן בהצלחה!", "עודכן חדר", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);

                    // Update data in all screens
                    _refreshDataCommand.Execute(null);
                }
            }
        }
        
        /// <summary>
        /// Create a new room with current data
        /// </summary>
        private void CreateNewRoom()
        {
            // Check that the room's name is unique
            if (_schoolData.Rooms.Where(room => room.roomName == RoomName).Any())
            {
                _messageBoxService.ShowMessage("שם החדר תפוס כבר! אנא תן שם חדש", "נכשל ביצירת חדר", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
            // Check that the room selected class doesn't have another room already
            else if (SelectedClass != NO_ASSIGNED_CLASS && _schoolData.Classes.Find(SelectedClass).roomID != null)
            {
                _messageBoxService.ShowMessage("לכיתה שנבחרה יש כבר חדר אם. בטל את הבחירה או עדכן את הכיתה קודם.",
                                               "נכשל ביצירת חדר", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
            else
            {
                // Create a new room
                Room newRoom = new Room() { roomName = RoomName };
                _schoolData.Rooms.Add(newRoom);
                _schoolData.SaveChanges();

                // If a class was selected, update it too
                if (SelectedClass != NO_ASSIGNED_CLASS)
                {
                    _schoolData.Classes.Find(SelectedClass).roomID = newRoom.roomID;
                    _schoolData.SaveChanges();
                }

                // Report action success
                _messageBoxService.ShowMessage("החדר " + RoomName + " נוצר בהצלחה!", "נוצר חדר", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);

                // Update data in all screens
                _refreshDataCommand.Execute(null);
            }
        }

        #endregion
    }
}

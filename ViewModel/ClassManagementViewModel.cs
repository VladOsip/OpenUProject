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
    public class ClassManagementViewModel : BaseViewModel, IScreenViewModel
    {
        #region Sub-Structs
        public class ClassData
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public int StudentCount { get; set; }
            public string HomeroomTeacherName { get; set; }
            public Nullable<int> HomeroomTeacherID { get; set; }
            public string RoomName { get; set; }
            public Nullable<int> RoomID { get; set; }
            public List<LessonInClass> LessonsInThisClass { get; set; }
            public List<string> StudentsInThisClass { get; set; }
        }

        public class LessonInClass
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
        private ICommand _deleteClassCommand;
        private ICommand _updateClassCommand;
        private ICommand _createNewClassCommand;

        private SchoolEntities _schoolData;

        private ClassData _selectedClass;
        private string _selectedClassName;
        private int _selectedTeacher;
        private int _selectedRoom;

        public ObservableCollection<ClassData> _classesTableData;
        private ObservableDictionary<int, string> _availableTeachers;
        private ObservableDictionary<int, string> _availableRooms;
        private ObservableCollection<LessonInClass> _lessonsInSelectedClass;
        private ObservableCollection<string> _studentsInSelectedClass;

        private Nullable<int> _previousHomeroomTeacher;
        private Nullable<int> _previousRoom;

        private const int NOT_ASSIGNED = -1;
        #endregion

        #region Properties / Commands
        // Base Properties
        public Person ConnectedPerson { get; private set; }
        public bool HasRequiredPermissions { get; }
        public string ScreenName { get { return "ניהול כיתות"; } }

        // Business Logic Properties
        public ObservableCollection<ClassData> ClassesTableData 
        { 
            get
            {
                return _classesTableData;
            }
            set
            {
                if (_classesTableData != value)
                {
                    _classesTableData = value;
                    OnPropertyChanged("ClassesTableData");
                }
            }
        }
        public ClassData SelectedClass 
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
                    UseSelectedClass(_selectedClass);
                    OnPropertyChanged("SelectedClass");
                }
            }
        }
        public string ClassName 
        { 
            get
            {
                return _selectedClassName;
            }
            set
            {
                if (_selectedClassName != value)
                {
                    _selectedClassName = value;
                    OnPropertyChanged("ClassName");
                }
            }
        }

        public ObservableDictionary<int, string> AvailableTeachers
        {
            get
            {
                return _availableTeachers;
            }
            set
            {
                if (_availableTeachers != value)
                {
                    _availableTeachers = value;
                    OnPropertyChanged("AvailableTeachers");
                }
            }
        }
        public int SelectedTeacher
        {
            get
            {
                return _selectedTeacher;
            }
            set
            {
                if (_selectedTeacher != value)
                {
                    _selectedTeacher = value;
                    OnPropertyChanged("SelectedTeacher");
                }
            }
        }

        public ObservableDictionary<int, string> AvailableRooms
        {
            get
            {
                return _availableRooms;
            }
            set
            {
                if (_availableRooms != value)
                {
                    _availableRooms = value;
                    OnPropertyChanged("AvailableRooms");
                }
            }
        }
        public int SelectedRoom
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
                    OnPropertyChanged("SelectedRoom");
                }
            }
        }

        public ObservableCollection<LessonInClass> LessonsInSelectedClass
        {
            get
            {
                return _lessonsInSelectedClass;
            }
            set
            {
                if (_lessonsInSelectedClass != value)
                {
                    _lessonsInSelectedClass = value;
                    OnPropertyChanged("LessonsInSelectedClass");
                }
            }
        }

        public ObservableCollection<string> StudentsInSelectedClass
        {
            get
            {
                return _studentsInSelectedClass;
            }
            set
            {
                if (_studentsInSelectedClass != value)
                {
                    _studentsInSelectedClass = value;
                    OnPropertyChanged("StudentsInSelectedClass");
                }
            }
        }

        /// <summary>
        /// Delete the currently selected class
        /// </summary>
        public ICommand DeleteClassCommand
        {
            get
            {
                if (_deleteClassCommand == null)
                {
                    _deleteClassCommand = new RelayCommand(p => DeleteSelectedClass());
                }
                return _deleteClassCommand;
            }
        }

        /// <summary>
        /// Update the currently selected class
        /// </summary>
        public ICommand UpdateClassCommand
        {
            get
            {
                if (_updateClassCommand == null)
                {
                    _updateClassCommand = new RelayCommand(p => UpdateSelectedClass());
                }
                return _updateClassCommand;
            }
        }

        /// <summary>
        /// Create a new school class with the current data
        /// </summary>
        public ICommand CreateNewClassCommand
        {
            get
            {
                if (_createNewClassCommand == null)
                {
                    _createNewClassCommand = new RelayCommand(p => CreateNewClass());
                }
                return _createNewClassCommand;
            }
        }

        #endregion

        #region Constructors
        public ClassManagementViewModel(Person connectedPerson, ICommand refreshDataCommand, IMessageBoxService messageBoxService)
            : base(messageBoxService)
        {
            HasRequiredPermissions = connectedPerson.isSecretary || connectedPerson.isPrincipal;
            _refreshDataCommand = refreshDataCommand;

            if (HasRequiredPermissions)
            {
                _schoolData = new SchoolEntities();
                AvailableTeachers = new ObservableDictionary<int, string>();
                AvailableRooms = new ObservableDictionary<int, string>();
            }

        }
        #endregion

        #region Methods
        public void Initialize(Person connectedPerson)
        {
            ConnectedPerson = connectedPerson;

            // Get the list of existing classes
            ClassesTableData = new ObservableCollection<ClassData>(_schoolData.Classes.AsEnumerable().Select(currClass => ModelClassToClassData(currClass)).ToList());

            // Create the basic list of available classes
            AvailableTeachers.Clear();

            // Add a 'No teacher' option, as not all classes have a teacher assigned to them.
            AvailableTeachers.Add(NOT_ASSIGNED, "אין מורה משויך");

            // Create the list of teachers that are not homeroom teachers already.
            _schoolData.Teachers.Where(teacher => teacher.classID == null && !teacher.Person.User.isDisabled).ToList()
                .ForEach(teacher => AvailableTeachers.Add(teacher.teacherID, teacher.Person.firstName + " " + teacher.Person.lastName));

            // Create the basic list of available rooms
            AvailableRooms.Clear();

            // Add a 'no room' option, as not all classes have an an assigned room.
            AvailableRooms.Add(NOT_ASSIGNED, "אין חדר משויך");

            // Create the list of rooms that are not assigned to a specific class already
            _schoolData.Rooms.Where(room => room.Classes.Count() == 0).ToList()
                .ForEach(room => AvailableRooms.Add(room.roomID, room.roomName));

            // Reset selections
            SelectedTeacher = NOT_ASSIGNED;
            SelectedRoom = NOT_ASSIGNED;

            // For some reason, after re-initializing this view, the selections are not updated properly in the view unless called again
            OnPropertyChanged("SelectedTeacher");
            OnPropertyChanged("SelectedRoom");
        }

        /// <summary>
        /// Converts the Model's Class object into a local classData object
        /// </summary>
        /// <param name="room">The Model's Class</param>
        /// <returns>Corresponding ClassData version of the class</returns>
        private ClassData ModelClassToClassData(Class modelClass)
        {
            ClassData classData = new ClassData();
            
            // Get the basic class data
            classData.ID = modelClass.classID;
            classData.Name = modelClass.className;
            classData.StudentCount = (modelClass.Students != null) ? modelClass.Students.Count() : 0;

            // Check if the class has an homeroom teacher
            if (modelClass.Teachers != null && modelClass.Teachers.Count > 0)
            {
                var classTeacher = modelClass.Teachers.Single();
                classData.HomeroomTeacherID = classTeacher.teacherID;
                classData.HomeroomTeacherName = classTeacher.Person.firstName + " " + classTeacher.Person.lastName;
            }

            // Check if the class has a room associated with it
            if (modelClass.roomID != null)
            {
                classData.RoomID = modelClass.roomID;
                classData.RoomName = modelClass.Room.roomName;
            }

            // Check if the class has lessons associated with it
            if (modelClass.Lessons != null && modelClass.Lessons.Count > 0)
            {
                classData.LessonsInThisClass = modelClass.Lessons.Select(lesson => new LessonInClass()
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

            // Check if the class has students associated with it
            if (modelClass.Students != null && modelClass.Students.Count > 0)
            {
                classData.StudentsInThisClass = modelClass.Students.Where(student => !student.Person.User.isDisabled)
                    .Select(student => student.Person.firstName + " " + student.Person.lastName).ToList();
            }

            return classData;
        }

        /// <summary>
        /// Choose a specific class and view its information.
        /// </summary>
        /// <param name="selectedClass">The class's data</param>
        private void UseSelectedClass(ClassData selectedClass)
        {
            // Cleanup previous selections
            SelectedTeacher = NOT_ASSIGNED;
            SelectedRoom = NOT_ASSIGNED;
            ClassName = string.Empty;
            LessonsInSelectedClass = new ObservableCollection<LessonInClass>();
            StudentsInSelectedClass = new ObservableCollection<string>();

            // Remove the previous class's homeroom teacher from the available teachers list as he/she are already assigned to the previous class
            if (_previousHomeroomTeacher != null)
            {
                AvailableTeachers.Remove(_previousHomeroomTeacher.Value);
            }
            // Remove the previous class's room from the available rooms list as it is already assigned to the previous class
            if (_previousRoom != null)
            {
                AvailableRooms.Remove(_previousRoom.Value);
            }

            // Update the properties per the selected class
            if (selectedClass != null)
            {
                ClassName = selectedClass.Name;

                // If the class has an teacher, add it first to the available teachers list
                if (selectedClass.HomeroomTeacherID != null)
                {
                    AvailableTeachers.Add(selectedClass.HomeroomTeacherID.Value, selectedClass.HomeroomTeacherName);
                    SelectedTeacher = selectedClass.HomeroomTeacherID.Value;
                }
                // If the class has a room associated with it, add it first to the available rooms list
                if (selectedClass.RoomID != null)
                {
                    AvailableRooms.Add(selectedClass.RoomID.Value, selectedClass.RoomName);
                    SelectedRoom = selectedClass.RoomID.Value;
                }

                // Save the homeroom teacher and room IDs so they can be removed when we select another class
                _previousHomeroomTeacher = selectedClass.HomeroomTeacherID;
                _previousRoom = selectedClass.RoomID;

                // Create the list of lessons in the current class
                if (selectedClass.LessonsInThisClass != null)
                {
                    LessonsInSelectedClass = new ObservableCollection<LessonInClass>(selectedClass.LessonsInThisClass);
                }

                // Create the list of students in the current clas
                if (selectedClass.StudentsInThisClass != null)
                {
                    StudentsInSelectedClass = new ObservableCollection<string>(selectedClass.StudentsInThisClass);
                }
            }
        }

        /// <summary>
        /// Delete the currently selected class
        /// </summary>
        private void DeleteSelectedClass()
        {
            // Check that a class was selected
            if (SelectedClass == null)
            {
                _messageBoxService.ShowMessage("אנא בחר כיתה קודם כל.",
                                               "נכשל במחיקת כיתה", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
            else
            {
                // Get the class that is going to be edited
                Class selectedClass = _schoolData.Classes.Find(SelectedClass.ID);

                // As this is a serious action, request a confirmation from the user
                bool confirmation = _messageBoxService.ShowMessage("האם אתה בטוח שברצונך למחוק את הכיתה " + selectedClass.className + "?",
                                                                    "מחיקת כיתה!", MessageType.ACCEPT_CANCEL_MESSAGE, MessagePurpose.INFORMATION);
                if (confirmation == true)
                {
                    // Remove the class from any of its associations (events, students)
                    _schoolData.Students.Where(student => student.classID == selectedClass.classID)
                                            .ToList().ForEach(student => student.classID = null);
                    _schoolData.Events.Where(eventData => eventData.recipientClassID == selectedClass.classID)
                                            .ToList().ForEach(eventData => eventData.recipientClassID = null);

                    // Lessons are meant only for a specific class and are meaningless after a class was removed - delete all lessons for this class
                    var releventLessons = _schoolData.Lessons.Where(lesson => lesson.classID == selectedClass.classID);
                    foreach (var lesson in releventLessons)
                    {
                        _schoolData.Lessons.Remove(lesson);
                    }

                    // Clear the previous class properties (as this class is removed)
                    this._previousHomeroomTeacher = null;
                    this._previousRoom = null;

                    // Delete the class itself
                    _schoolData.Classes.Remove(selectedClass);

                    // Save and report changes
                    _schoolData.SaveChanges();
                    _messageBoxService.ShowMessage("הכיתה " + selectedClass.className + " נמחקה בהצלחה!",
                            "מחיקת כיתה!", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);
                    _refreshDataCommand.Execute(null);
                }
            }
        }

        /// <summary>
        /// Update the currently selected class
        /// </summary>
        private void UpdateSelectedClass()
        {
            // Check that a class was selected
            if (SelectedClass == null)
            {
                _messageBoxService.ShowMessage("אנא בחר כיתה קודם כל.",
                                               "נכשל בעדכון כיתה", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
            else
            {
                // Get the class that is going to be edited
                Class selectedClassModel = _schoolData.Classes.Find(SelectedClass.ID);

                // Get the homeroom teacher that was associated with the class, and the newly selected homeroom teacher
                Teacher previousTeacher = _schoolData.Teachers.Find(_previousHomeroomTeacher);
                Teacher selectedTeacher = _schoolData.Teachers.Find(SelectedTeacher);

                // Get the room that was associated with the class, and the newly selected room
                Room previousRoom = _schoolData.Rooms.Find(_previousRoom);
                Room selectedRoom = _schoolData.Rooms.Find(SelectedRoom);

                // Check that the class's new name is unique
                if (_schoolData.Classes.Where(classData => classData.classID != selectedClassModel.classID && classData.className == ClassName).Any())
                {
                    _messageBoxService.ShowMessage("שם הכיתה תפוס כבר! אנא תן שם חדש", "נכשל בעדכון כיתה", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
                }
                // Check that the selected homeroom teacher doesn't have another class already
                else if (SelectedTeacher != NOT_ASSIGNED && selectedTeacher.classID != null && selectedTeacher.classID != selectedClassModel.classID)
                {
                    _messageBoxService.ShowMessage("המורה שנבחר הוא מחנך של כיתה אחרת כבר. אנא עדכן את הבחירה.",
                                                   "נכשל בעדכון כיתה", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
                }
                // Check that the selected room isn't associated with another class already
                else if (SelectedRoom != NOT_ASSIGNED && selectedRoom.Classes != null && selectedRoom.Classes.Single().classID != selectedClassModel.classID)
                {
                    _messageBoxService.ShowMessage("החדר שנבחר משויך לכיתה אחרת. אנא עדכן את הבחירה.",
                                                   "נכשל בעדכון כיתה", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
                }
                else
                {
                    // Update the class's data
                    selectedClassModel.className = ClassName;
                    selectedClassModel.roomID = SelectedRoom;

                    // Remove the class from its previous teacher (assuming it has changed)
                    if (previousTeacher != null && previousTeacher.teacherID != SelectedTeacher)
                    {
                        previousTeacher.classID = null;
                        _schoolData.SaveChanges();

                        // Clear the previous room property (as it was removed anyway)
                        this._previousHomeroomTeacher = null;
                    }

                    // Room association is saved in the class - no need to update the old room. Only forget about it in the property
                    this._previousRoom = null;

                    // Add the class to the selected teacher
                    if (SelectedTeacher != NOT_ASSIGNED)
                    {
                        // Update teacher to use this class's ID
                        selectedTeacher.classID = selectedClassModel.classID; 
                    }

                    // Update the model
                    _schoolData.SaveChanges();

                    // Report action success
                    _messageBoxService.ShowMessage("הכיתה " + ClassName + " עודכנה בהצלחה!", "עודכנה כיתה", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);

                    // Update data in all screens
                    _refreshDataCommand.Execute(null);
                }
            }
        }
        
        /// <summary>
        /// Create a new class with current data
        /// </summary>
        private void CreateNewClass()
        {
            // Check that the class's name is unique
            if (_schoolData.Classes.Where(classData => classData.className == ClassName).Any())
            {
                _messageBoxService.ShowMessage("שם הכיתה תפוס כבר! אנא תן שם חדש", "נכשל ביצירת כיתה", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
            // Check that the class's selected Teacher don't have another class already
            else if (SelectedTeacher != NOT_ASSIGNED && _schoolData.Teachers.Find(SelectedTeacher).classID != null)
            {
                _messageBoxService.ShowMessage("המחנך שנבחר אחראי כבר על כיתה אחרת. בטל את הבחירה או עדכן את המורה קודם.",
                                               "נכשל ביצירת כיתה", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
            // Check that the class's selected room doesn't have another class already
            else if (SelectedRoom != NOT_ASSIGNED && _schoolData.Classes.Where(currClass => currClass.roomID == SelectedRoom).Any())
            {
                _messageBoxService.ShowMessage("המחנך שנבחר אחראי כבר על כיתה אחרת. בטל את הבחירה או עדכן את המורה קודם.",
                                               "נכשל ביצירת כיתה", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
            else
            {
                // Create a new class
                Class newClass = new Class() { className = ClassName };
                if (SelectedRoom != NOT_ASSIGNED)
                {
                    newClass.roomID = SelectedRoom;
                }
                else
                {
                    newClass.roomID = null;
                }

                _schoolData.Classes.Add(newClass);
                _schoolData.SaveChanges();

                // If a teacher was selected, update it too
                if (SelectedTeacher != NOT_ASSIGNED)
                {
                    _schoolData.Teachers.Find(SelectedTeacher).classID = newClass.classID;
                    _schoolData.SaveChanges();
                }

                // Report action success
                _messageBoxService.ShowMessage("הכיתה " + ClassName + " נוצר בהצלחה!", "נוצרה כיתה", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);

                // Update data in all screens
                _refreshDataCommand.Execute(null);
            }
        }

        #endregion
    }
}

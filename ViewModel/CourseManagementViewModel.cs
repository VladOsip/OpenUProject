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
    /// Manages the school's Courses - creating, editing and deleting courses
    /// </summary>
    public class CourseManagementViewModel : BaseViewModel, IScreenViewModel
    {
        #region Sub-Structs
        public class CourseData
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public bool IsHomeroomTeacherOnly { get; set; }
            public List<LessonsOfCourse> LessonsOfThisCourse { get; set; }
            public List<string> TeachersOfThisCourse { get; set; }
        }

        public class LessonsOfCourse
        {
            public int LessonID { get; set; }
            public int ClassID { get; set; }
            public string ClassName { get; set; }
            public string TeacherName { get; set; }
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
        private ICommand _deleteCourseCommand;
        private ICommand _updateCourseCommand;
        private ICommand _createNewCourseCommand;

        private SchoolEntities _schoolData;

        private CourseData _selectedCourse;
        private string _selectedCourseName;
        private bool _isSelectedCourseHomeroomOnly;

        public ObservableCollection<CourseData> _coursesTableData;
        private ObservableCollection<LessonsOfCourse> _lessonsOfSelectedCourse;
        private ObservableCollection<string> _teachersOfSelectedCourse;
        #endregion

        #region Properties / Commands
        // Base Properties
        public Person ConnectedPerson { get; private set; }
        public bool HasRequiredPermissions { get; }
        public string ScreenName { get { return "ניהול מקצועות"; } }

        // Business Logic Properties
        public ObservableCollection<CourseData> CoursesTableData 
        { 
            get
            {
                return _coursesTableData;
            }
            set
            {
                if (_coursesTableData != value)
                {
                    _coursesTableData = value;
                    OnPropertyChanged("CoursesTableData");
                }
            }
        }
        public CourseData SelectedCourse 
        { 
            get
            {
                return _selectedCourse;
            }
            set
            {
                if (_selectedCourse != value)
                {
                    _selectedCourse = value;
                    UseSelectedCourse(_selectedCourse);
                    OnPropertyChanged("SelectedCourse");
                }
            }
        }
        public string CourseName 
        { 
            get
            {
                return _selectedCourseName;
            }
            set
            {
                if (_selectedCourseName != value)
                {
                    _selectedCourseName = value;
                    OnPropertyChanged("CourseName");
                }
            }
        }
        public bool IsSelectedCourseHomeroomOnly
        {
            get
            {
                return _isSelectedCourseHomeroomOnly;
            }
            set
            {
                if (_isSelectedCourseHomeroomOnly != value)
                {
                    _isSelectedCourseHomeroomOnly = value;
                    OnPropertyChanged("IsSelectedCourseHomeroomOnly");
                }
            }
        }

        public ObservableCollection<LessonsOfCourse> LessonsOfSelectedCourse
        {
            get
            {
                return _lessonsOfSelectedCourse;
            }
            set
            {
                if (_lessonsOfSelectedCourse != value)
                {
                    _lessonsOfSelectedCourse = value;
                    OnPropertyChanged("LessonsOfSelectedCourse");
                }
            }
        }

        public ObservableCollection<string> TeachersOfSelectedCourse
        {
            get
            {
                return _teachersOfSelectedCourse;
            }
            set
            {
                if (_teachersOfSelectedCourse != value)
                {
                    _teachersOfSelectedCourse = value;
                    OnPropertyChanged("TeachersOfSelectedCourse");
                }
            }
        }

        /// <summary>
        /// Delete the currently selected course
        /// </summary>
        public ICommand DeleteCourseCommand
        {
            get
            {
                if (_deleteCourseCommand == null)
                {
                    _deleteCourseCommand = new RelayCommand(p => DeleteSelectedCourse());
                }
                return _deleteCourseCommand;
            }
        }

        /// <summary>
        /// Update the currently selected course
        /// </summary>
        public ICommand UpdateCourseCommand
        {
            get
            {
                if (_updateCourseCommand == null)
                {
                    _updateCourseCommand = new RelayCommand(p => UpdateSelectedCourse());
                }
                return _updateCourseCommand;
            }
        }

        /// <summary>
        /// Create a new course with the current data
        /// </summary>
        public ICommand CreateNewCourseCommand
        {
            get
            {
                if (_createNewCourseCommand == null)
                {
                    _createNewCourseCommand = new RelayCommand(p => CreateNewCourse());
                }
                return _createNewCourseCommand;
            }
        }

        #endregion

        #region Constructors
        public CourseManagementViewModel(Person connectedPerson, ICommand refreshDataCommand, IMessageBoxService messageBoxService)
            : base (messageBoxService)
        {
            HasRequiredPermissions = connectedPerson.isSecretary || connectedPerson.isPrincipal;
            _refreshDataCommand = refreshDataCommand;

            if (HasRequiredPermissions)
            {
                _schoolData = new SchoolEntities();
            }
        }
        #endregion

        #region Methods
        public void Initialize(Person connectedPerson)
        {
            ConnectedPerson = connectedPerson;

            // Get the list of existing courses
            CoursesTableData = new ObservableCollection<CourseData>(_schoolData.Courses.AsEnumerable().Select(course => ModelCourseToCourseData(course)).ToList());
        }

        /// <summary>
        /// Converts the Model's Course object into a local CourseData object
        /// </summary>
        /// <param name="room">The Model's Course</param>
        /// <returns>Corresponding CourseData version of the course</returns>
        private CourseData ModelCourseToCourseData(Course modelCourse)
        {
            CourseData courseData = new CourseData();
            
            // Get the basic class data
            courseData.ID = modelCourse.courseID;
            courseData.Name = modelCourse.courseName;
            courseData.IsHomeroomTeacherOnly = modelCourse.isHomeroomTeacherOnly;

            // Check if the class has lessons associated with it
            if (modelCourse.Lessons != null && modelCourse.Lessons.Count > 0)
            {
                courseData.LessonsOfThisCourse = modelCourse.Lessons.Select(lesson => new LessonsOfCourse()
                    {
                        LessonID = lesson.lessonID,
                        ClassID = lesson.classID,
                        ClassName = lesson.Class.className,
                        TeacherName = lesson.Teacher.Person.firstName + " " + lesson.Teacher.Person.lastName,
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

            courseData.TeachersOfThisCourse = new List<string>();

            // Check for teachers that are teaching this course (and that aren't disabled)
            // The course can be a teacher's primary, secondary, tertiary or quaternary course, so we have to check every option
            foreach (Teacher teacher in _schoolData.Teachers.Where(teacher => !teacher.Person.User.isDisabled &&
                    (teacher.firstCourseID == courseData.ID || teacher.secondCourseID == courseData.ID ||
                    teacher.thirdCourseID == courseData.ID || teacher.fourthCourseID == courseData.ID)))
            {
                courseData.TeachersOfThisCourse.Add(teacher.Person.firstName + " " + teacher.Person.lastName);
            }

            return courseData;
        }

        /// <summary>
        /// Choose a specific course and view its information.
        /// </summary>
        /// <param name="selectedCourse">The course's data</param>
        private void UseSelectedCourse(CourseData selectedCourse)
        {
            // Cleanup previous selection
            CourseName = string.Empty;
            IsSelectedCourseHomeroomOnly = false;
            LessonsOfSelectedCourse = new ObservableCollection<LessonsOfCourse>();
            TeachersOfSelectedCourse = new ObservableCollection<string>();

            // Update the properties per the selected course
            if (selectedCourse != null)
            {
                CourseName = selectedCourse.Name;
                IsSelectedCourseHomeroomOnly = selectedCourse.IsHomeroomTeacherOnly;

                // Create the list of lessons of the selected course
                if (selectedCourse.LessonsOfThisCourse != null)
                {
                    LessonsOfSelectedCourse = new ObservableCollection<LessonsOfCourse>(selectedCourse.LessonsOfThisCourse);
                }

                // Create the list of teachers of the selected course
                if (selectedCourse.TeachersOfThisCourse != null)
                {
                    TeachersOfSelectedCourse = new ObservableCollection<string>(selectedCourse.TeachersOfThisCourse);
                }
            }
        }

        /// <summary>
        /// Delete the currently selected course
        /// </summary>
        private void DeleteSelectedCourse()
        {
            // Check that a course was selected
            if (SelectedCourse == null)
            {
                _messageBoxService.ShowMessage("אנא בחר מקצוע קודם כל.",
                                               "נכשל במחיקת מקצוע", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
            else
            {
                // Get the course that is going to be edited
                Course selectedCourse = _schoolData.Courses.Find(SelectedCourse.ID);

                // As this is a serious action, request a confirmation from the user
                bool confirmation = _messageBoxService.ShowMessage("האם אתה בטוח שברצונך למחוק את המקצוע " + selectedCourse.courseName + "?",
                                                                    "מחיקת מקצוע!", MessageType.ACCEPT_CANCEL_MESSAGE, MessagePurpose.INFORMATION);
                if (confirmation == true)
                {
                    RemoveCourseFromTeachers(selectedCourse);

                    // Lessons are meant only for a specific course and are meaningless after it was removed - delete all lessons from this course
                    var releventLessons = _schoolData.Lessons.Where(lesson => lesson.courseID == selectedCourse.courseID);
                    foreach (var lesson in releventLessons)
                    {
                        _schoolData.Lessons.Remove(lesson);
                    }

                    // Delete the course itself
                    _schoolData.Courses.Remove(selectedCourse);

                    // Save and report changes
                    _schoolData.SaveChanges();
                    _messageBoxService.ShowMessage("המקצוע " + selectedCourse.courseName + " נמחקה בהצלחה!",
                            "מחיקת מקצוע!", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);
                    _refreshDataCommand.Execute(null);
                }
            }
        }

        /// <summary>
        /// Assistant method to remove a course from its teachers while keeping their courses organized
        /// Note that teachers can have multiple courses (up to four courses)
        /// </summary>
        /// <param name="selectedCourse">The course to delete</param>
        private static void RemoveCourseFromTeachers(Course selectedCourse)
        {
            // Remove the course from all teachers in which it is their first course
            foreach (Teacher teacher in selectedCourse.Teachers)
            {
                if (teacher.fourthCourseID != null)
                {
                    // Switch with the fourth course
                    teacher.firstCourseID = teacher.fourthCourseID;
                    teacher.fourthCourseID = null;
                }
                else if (teacher.thirdCourseID != null)
                {
                    // Switch with the third course
                    teacher.firstCourseID = teacher.thirdCourseID;
                    teacher.thirdCourseID = null;
                }
                else if (teacher.secondCourseID != null)
                {
                    // Switch with the second course
                    teacher.firstCourseID = teacher.secondCourseID;
                    teacher.secondCourseID = null;
                }
                else
                {
                    // this was the only course of the teacher. Delete it (and leave the teacher with no courses)
                    teacher.firstCourseID = null;
                }
            }
            // Remove the course from all teachers in which it is their second course
            foreach (Teacher teacher in selectedCourse.Teachers)
            {
                if (teacher.fourthCourseID != null)
                {
                    // Switch with the fourth course
                    teacher.secondCourseID = teacher.fourthCourseID;
                    teacher.fourthCourseID = null;
                }
                else if (teacher.thirdCourseID != null)
                {
                    // Switch with the third course
                    teacher.secondCourseID = teacher.thirdCourseID;
                    teacher.thirdCourseID = null;
                }
                else
                {
                    // The teacher only had this and another course, so remove it directly
                    teacher.firstCourseID = null;
                }
            }
            // Remove the course from all teachers in which it is their third course
            foreach (Teacher teacher in selectedCourse.Teachers)
            {
                if (teacher.fourthCourseID != null)
                {
                    // Switch with the fourth course
                    teacher.thirdCourseID = teacher.fourthCourseID;
                    teacher.fourthCourseID = null;
                }
                else
                {
                    teacher.thirdCourseID = null;
                }
            }
            // Remove the course from all teachers in which it is their fourth
            foreach (Teacher teacher in selectedCourse.Teachers)
            {
                teacher.fourthCourseID = null;
            }
        }

        /// <summary>
        /// Update the currently selected course
        /// </summary>
        private void UpdateSelectedCourse()
        {
            // Check that a course was selected
            if (SelectedCourse == null)
            {
                _messageBoxService.ShowMessage("אנא בחר מקצוע קודם כל.",
                                               "נכשל בעדכון מקצוע", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
            else
            {
                // Get the course that is going to be edited
                Course selectedCourse = _schoolData.Courses.Find(SelectedCourse.ID);

                // Check that the course's new name is unique
                if (_schoolData.Courses.Where(course => course.courseID != selectedCourse.courseID && course.courseName == CourseName).Any())
                {
                    _messageBoxService.ShowMessage("שם המקצוע תפוס כבר! אנא תן שם חדש", "נכשל בעדכון מקצוע", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
                }
                else
                {
                    // Update the course's data
                    selectedCourse.courseName = CourseName;
                    selectedCourse.isHomeroomTeacherOnly = IsSelectedCourseHomeroomOnly;

                    // Update the model
                    _schoolData.SaveChanges();

                    // Report action success
                    _messageBoxService.ShowMessage("המקצוע " + CourseName + " עודכן בהצלחה!", "עודכן מקצוע!", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);

                    // Update data in all screens
                    _refreshDataCommand.Execute(null);
                }
            }
        }
        
        /// <summary>
        /// Create a new course with the current data
        /// </summary>
        private void CreateNewCourse()
        {
            // Check that the course's name is unique
            if (_schoolData.Courses.Where(courseData => courseData.courseName == CourseName).Any())
            {
                _messageBoxService.ShowMessage("שם המקצוע תפוס כבר! אנא תן שם חדש", "נכשל ביצירת מקצוע", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
            else
            {
                // Create a new course
                Course newCourse = new Course() { courseName = CourseName, isHomeroomTeacherOnly = IsSelectedCourseHomeroomOnly };
                
                _schoolData.Courses.Add(newCourse);
                _schoolData.SaveChanges();

                // Report action success
                _messageBoxService.ShowMessage("המקצוע " + CourseName + " נוצר בהצלחה!", "נוצר מקצוע!", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);

                // Update data in all screens
                _refreshDataCommand.Execute(null);
            }
        }

        #endregion
    }
}

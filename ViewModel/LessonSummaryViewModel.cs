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
    /// Allows a teacher to report missing students in his lesson and other notes about them, and automatically sending them a message about it
    /// </summary>
    public class LessonSummaryViewModel : BaseViewModel, IScreenViewModel
    {
        #region Sub-Structs
        public class StudentAtLesson
        {
            public int StudentID { get; set; }
            public string Name { get; set; }
            public bool WasMissing { get; set; }
            public string Notes { get; set; }
        }
        #endregion

        #region Fields
        private ICommand _refreshDataCommand;
        private ICommand _reportLessonCommand;

        private SchoolEntities _schoolData;

        private ObservableDictionary<int, string> _classes;
        private ObservableDictionary<int, string> _courses;
        private int _selectedClass;
        private int _selectedCourse;
        private DateTime _selectedDate;

        private ObservableCollection<StudentAtLesson> _studentsInLesson;

        private const int NOT_ASSIGNED = -1;
        #endregion

        #region Properties / Commands
        // Base Properties
        public Person ConnectedPerson { get; private set; }
        public bool HasRequiredPermissions { get; }
        public string ScreenName { get { return "הזנת דוח שיעור"; } }

        // Business Logic Properties / Commands
        public ObservableDictionary<int, string> Courses
        {
            get
            {
                return _courses;
            }
            set
            {
                if (_courses != value)
                {
                    _courses = value;
                    OnPropertyChanged("Courses");
                }
            }
        }
        public int SelectedCourse
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
                    OnPropertyChanged("SelectedCourse");

                    if (_selectedCourse != NOT_ASSIGNED)
                    {
                        FindClassForCourse(ConnectedPerson.Teacher.teacherID, _selectedCourse);
                    }
                }
            }
        }
        public ObservableDictionary<int, string> Classes
        {
            get
            {
                return _classes;
            }
            set
            {
                if (_classes != value)
                {
                    _classes = value;
                    OnPropertyChanged("Classes");
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

                    if (_selectedClass != NOT_ASSIGNED)
                    {
                        FindClassStudents(_selectedClass);
                    }
                }
            }
        }

        public DateTime SelectedDate
        { 
            get
            {
                return _selectedDate;
            }
            set
            {
                // Get the only the date value, without specific time
                if (_selectedDate != value.Date)
                {
                    _selectedDate = value.Date;
                    OnPropertyChanged("SelectedDate");
                }
            }
        }

        public ObservableCollection<StudentAtLesson> StudentsInLesson
        {
            get 
            {
                return _studentsInLesson;
            }
            set
            {
                if (_studentsInLesson != value)
                {
                    _studentsInLesson = value;
                    OnPropertyChanged("StudentsInLesson");
                }
            }
        }

        /// <summary>
        /// Send the lesson report
        /// </summary>
        public ICommand ReportLessonCommand
        {
            get
            {
                if (_reportLessonCommand == null)
                {
                    _reportLessonCommand = new RelayCommand(p => ReportLesson());
                }
                return _reportLessonCommand;
            }
        }
        #endregion

        #region Constructors
        public LessonSummaryViewModel(Person connectedPerson, ICommand refreshDataCommand, IMessageBoxService messageBoxService)
            : base(messageBoxService)
        {
            _refreshDataCommand = refreshDataCommand;

            // Only teachers can use this
            if (connectedPerson.isTeacher)
            {
                HasRequiredPermissions = true;
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

            // Initalize all lists
            Courses = new ObservableDictionary<int, string>();
            Classes = new ObservableDictionary<int, string>();
            StudentsInLesson = new ObservableCollection<StudentAtLesson>();
            _schoolData = new SchoolEntities();

            SelectedDate = DateTime.Now;
            SelectedCourse = NOT_ASSIGNED;
            SelectedClass = NOT_ASSIGNED;

            // Get the teacher's data
            Teacher _teacherInformation = ConnectedPerson.Teacher;
            if (_teacherInformation != null)
            {
                // Only gathering courses here -> classes depend on the selected course
                GatherTeacherCourses(_teacherInformation);
            }
            else
            {
                // Report there is some issue with the teacher in the Database (probably user has teacher permissions but isn't a teacher
                _messageBoxService.ShowMessage("תקלה בהרשאות המורה", "תקלה", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
        }

        /// <summary>
        /// Gather all the courses a specific teacher teaches
        /// </summary>
        /// <param name="teacher">The teacher to use</param>
        private void GatherTeacherCourses(Teacher teacher)
        {
            // Reset the Courses collection to fit the current teacher
            Courses = new ObservableDictionary<int, string>(TeacherCoursesHandler.GetTeacherCoursesNames(teacher, true));

            // Automatically select a course if possible
            if (Courses.Count() > 0)
            {
                SelectedCourse = Courses.First().Key;
            }
            else
            {
                SelectedCourse = NOT_ASSIGNED;
            }

            // For some reason the selections are not updated properly in the view unless called again
            OnPropertyChanged("SelectedCourse");
        }

        /// <summary>
        /// Gather all the classes that the selected teacher teaches from the specific course
        /// </summary>
        /// <param name="teacherID">The ID of the teacher</param>
        /// <param name="courseID">The ID of the course</param>
        private void FindClassForCourse(int teacherID, int courseID)
        {
            // Reset the classes collection
            Classes.Clear();

            // Gather all the classes that the teacher teaches of this course (has a lesson for them). Use HashSet to get each class only once
            var teacherClasses = _schoolData.Lessons.Where(lesson => lesson.courseID == courseID && lesson.teacherID == teacherID).
                                   Select(lesson => lesson.Class).ToHashSet();
            foreach (Class schoolClass in teacherClasses)
            {
                Classes.Add(schoolClass.classID, schoolClass.className);
            }

            // For courses that are Homeroom courses, add the teacher's homeroom class too
            if (_schoolData.Courses.Find(courseID).isHomeroomTeacherOnly)
            {
                // Gather the teacher info, and check if he is an homeroom teacher for a class that wasn't added before already
                Teacher teacherInfo = _schoolData.Teachers.Find(teacherID);
                if (teacherInfo.classID.HasValue && !Classes.ContainsKey(teacherInfo.classID.Value))
                {
                    Classes.Add(teacherInfo.Class.classID, teacherInfo.Class.className);
                }
            }

            // Automatically select a class if possible
            if (Classes.Count() > 0)
            {
                SelectedClass = Classes.First().Key;

                // For some reason the selections are not updated properly in the view unless called again
                OnPropertyChanged("SelectedClass");
            }
            else
            {
                SelectedClass = NOT_ASSIGNED;
            }
        }

        /// <summary>
        /// Gather all the students from a specific class
        /// </summary>
        /// <param name="classID">The ID of the class</param>
        private void FindClassStudents(int classID)
        {
            // Gather all students from a class
            StudentsInLesson = 
                new ObservableCollection<StudentAtLesson>(_schoolData.Classes.Find(classID).Students.Where(student => !student.Person.User.isDisabled)
                .Select(student => new StudentAtLesson()
                {
                    StudentID = student.studentID,
                    Name =student.Person.firstName + " " + student.Person.lastName,
                    WasMissing = false,
                    Notes = string.Empty
                }));
        }

        /// <summary>
        /// Report each student at the selected lesson that was missing and/or has notes added to him, if any
        /// </summary>
        private void ReportLesson()
        {
            // Check if the report is possible (class&course were selected)
            if (SelectedClass != NOT_ASSIGNED && SelectedCourse != NOT_ASSIGNED)
            {
                bool didSendAnyReport = false;

                // Create the basic template for the report - lesson date and the reporter information
                string reportTemplate =
                    "המורה " + ConnectedPerson.firstName + " " + ConnectedPerson.lastName +
                    " הזין לך הערות בתאריך " + SelectedDate.ToString("dd/MM/yy") +
                    " לשיעור " + Courses[SelectedCourse] + ":\n";

                // Go over every student, and look for any that was reported by the teacher
                foreach (StudentAtLesson student in StudentsInLesson)
                {
                    if (student.WasMissing || student.Notes != string.Empty)
                    {
                        // Get the student's full information
                        Student studentInfo = _schoolData.Students.Find(student.StudentID);

                        // Create a report
                        string report = reportTemplate;

                        if (student.WasMissing)
                        {
                            report += "החסרת מהשיעור.\n";

                            // Add an absences count for the student
                            studentInfo.absencesCounter = studentInfo.absencesCounter + 1;
                        }
                        if (student.Notes != string.Empty)
                        {
                            report += student.Notes + "\n";
                        }

                        // Send a message to the student about the report
                        MessagesHandler.CreateMessage("קיבלת הערה בשיעור", report, 
                                                        MessageRecipientsTypes.Person, ConnectedPerson.Teacher.teacherID, student.StudentID);

                        // If the student has any parents, send the report to them too
                        if (studentInfo.parentID.HasValue)
                        {
                            string parentReport = "ילדך " + student.Name + " קיבל את ההערה הבאה בשיעור:\n" + report;
                            MessagesHandler.CreateMessage("ילדך קיבל הערה בשיעור", parentReport,
                                                            MessageRecipientsTypes.Person, 
                                                            ConnectedPerson.Teacher.teacherID, studentInfo.parentID.Value);
                        }

                        didSendAnyReport = true;
                    }
                }

                // Check if any report was required, and update accordingly
                if (didSendAnyReport)
                {
                    _schoolData.SaveChanges();
                    _refreshDataCommand.Execute(null);
                }

                // Report action success to the teacher
                string resultMessage = "הוזן דוח שיעור למקצוע " + Courses[SelectedCourse] + " לכיתה " + Classes[SelectedClass];
                _messageBoxService.ShowMessage("הוזן דוח שיעור", resultMessage, MessageType.OK_MESSAGE);
            }
            else
            {
                // No class or course selected. Cannot send a report
                _messageBoxService.ShowMessage("נכשל בשליחת דוח", "מקצוע או כיתה לא נבחרו. אנא בחר קודם לפני שליחת הדוח", 
                                                MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
        }
        #endregion
    }
}

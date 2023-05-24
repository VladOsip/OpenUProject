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
    /// Allows a teacher to set the grades of his students in a specific class & course
    /// </summary>
    public class GradesReportViewModel : BaseViewModel, IScreenViewModel
    {
        #region Sub-Structs
        public class GradedStudent
        {
            public int StudentID { get; set; }
            public string Name { get; set; }
            public int Score { get; set; }
            public string Notes { get; set; }

            // These properties are used to check for changes
            public int OriginalScore { get; set; }
            public string OriginalNotes { get; set; }
        }
        #endregion

        #region Fields
        private ICommand _refreshDataCommand;
        private ICommand _updateGradesCommand;

        private SchoolEntities _schoolData;

        private ObservableDictionary<int, string> _classes;
        private ObservableDictionary<int, string> _courses;
        private int _selectedClass;
        private int _selectedCourse;

        private ObservableCollection<GradedStudent> _studentGrades;

        private const int NOT_ASSIGNED = -1;
        #endregion

        #region Properties / Commands
        // Base Properties
        public Person ConnectedPerson { get; private set; }
        public bool HasRequiredPermissions { get; }
        public string ScreenName { get { return "הזנת ציונים"; } }

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
                // We might re-use the class when changing courses, so allow reselecting it
                _selectedClass = value;
                OnPropertyChanged("SelectedClass");

                FindClassStudents(_selectedClass);
            }
        }

        public ObservableCollection<GradedStudent> StudentsGrades
        {
            get 
            {
                return _studentGrades;
            }
            set
            {
                if (_studentGrades != value)
                {
                    _studentGrades = value;
                    OnPropertyChanged("StudentsGrades");
                }
            }
        }

        /// <summary>
        /// Update the grades of the students in the currently selected class & course combination
        /// </summary>
        public ICommand UpdateGradesCommand
        {
            get
            {
                if (_updateGradesCommand == null)
                {
                    _updateGradesCommand = new RelayCommand(p => UpdateGrades());
                }
                return _updateGradesCommand;
            }
        }
        #endregion

        #region Constructors
        public GradesReportViewModel(Person connectedPerson, ICommand refreshDataCommand, IMessageBoxService messageBoxService)
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
            StudentsGrades = new ObservableCollection<GradedStudent>();
            _schoolData = new SchoolEntities();

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
            if (classID != NOT_ASSIGNED)
            {
                // Gather all students from a class
                StudentsGrades =
                    new ObservableCollection<GradedStudent>(_schoolData.Classes.Find(classID).Students.Where(student => !student.Person.User.isDisabled)
                        .Select(student => ModelStudentToGradedStudent(student, SelectedCourse)));
            }
            else
            {
                // No class selected - just clear the students list
                StudentsGrades = new ObservableCollection<GradedStudent>();
            }
        }

        /// <summary>
        /// Converts the Model's student class into the local GradedStudent class for the selected course
        /// </summary>
        /// <param name="student">The student to convert</param>
        /// <param name="courseID">The ID of the course to grade the student on</param>
        /// <returns>A GradedStudent version of this student</returns>
        private GradedStudent ModelStudentToGradedStudent(Student student, int courseID)
        {
            GradedStudent gradedStudent = new GradedStudent();

            gradedStudent.StudentID = student.studentID;
            gradedStudent.Name = student.Person.firstName + " " + student.Person.lastName;

            // Get the student's current grade for this course (if he was graded already)
            Grade currentGrade = _schoolData.Grades.Find(student.studentID, courseID);
            if (currentGrade != null)
            {
                gradedStudent.Score = currentGrade.score;
                gradedStudent.Notes = currentGrade.notes;
            }
            else
            {
                gradedStudent.Score = Globals.GRADE_NO_VALUE;
                gradedStudent.Notes = string.Empty;
            }

            gradedStudent.OriginalScore = gradedStudent.Score;
            gradedStudent.OriginalNotes = gradedStudent.Notes;

            return gradedStudent;
        }

        /// <summary>
        /// Update the score for each student of the selected class & course combination.
        /// </summary>
        private void UpdateGrades()
        {
            // Check if the report is possible (class&course were selected)
            if (SelectedClass != NOT_ASSIGNED && SelectedCourse != NOT_ASSIGNED)
            {
                bool didSendAnyReport = false;

                // Create the basic template for the report - lesson date and the reporter information
                string reportTemplate =
                    "המורה " + ConnectedPerson.firstName + " " + ConnectedPerson.lastName +
                    " הזין לך ציון במקצוע " + Courses[SelectedCourse] + ":\n";

                // Go over every student, and look for any that was reported by the teacher
                foreach (GradedStudent student in StudentsGrades)
                {
                    // Update the grade for this student if it has changed
                    if (student.Score != student.OriginalScore || student.Notes != student.OriginalNotes)
                    {
                        // Get the student's full information
                        Student studentInfo = _schoolData.Students.Find(student.StudentID);

                        // Report the grade
                        Grade grade = new Grade() 
                        { 
                            studentID = studentInfo.studentID, 
                            courseID = SelectedCourse, 
                            teacherID = ConnectedPerson.Teacher.teacherID,
                            score = Convert.ToByte(student.Score) 
                        };
                        string gradeReport = reportTemplate;
                        gradeReport += "ציון: " + student.Score + "\n";

                        if (student.Notes != string.Empty)
                        {
                            gradeReport += "הערות: " + student.Notes + "\n";
                            grade.notes = student.Notes;
                        }

                        // Save the grade, and send a message to the student about it
                        _schoolData.Grades.Add(grade);
                        MessagesHandler.CreateMessage("קיבלת ציון", gradeReport, MessageRecipientsTypes.Person, null, student.StudentID);

                        // If the student has any parents, send a message to them too
                        if (studentInfo.parentID.HasValue)
                        {
                            string parentReport = "ילדך " + student.Name + " קיבל ציון:\n" + gradeReport;
                            MessagesHandler.CreateMessage("ילדך קיבל ציון", parentReport, MessageRecipientsTypes.Person, null, studentInfo.parentID.Value);
                        }

                        didSendAnyReport = true;
                    }
                }

                // Check if any student was report was required, and update accordingly
                if (didSendAnyReport)
                {
                    _schoolData.SaveChanges();
                    _refreshDataCommand.Execute(null);
                }

                // Report action success to the teacher
                string resultMessage = "הוזנו ציונים למקצוע " + Courses[SelectedCourse] + " לכיתה " + Classes[SelectedClass];
                _messageBoxService.ShowMessage("הוזנו ציונים", resultMessage, MessageType.OK_MESSAGE);
            }
            else
            {
                // No class or course selected. Cannot send a report
                _messageBoxService.ShowMessage("נכשל בהזנת ציונים", "מקצוע או כיתה לא נבחרו. אנא בחר קודם לפני הזנת ציונים",
                                                MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
        }
        #endregion
    }
}

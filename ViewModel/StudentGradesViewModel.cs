using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using MySchoolYear.Model;
using MySchoolYear.ViewModel.Utilities;

namespace MySchoolYear.ViewModel
{
    /// <summary>
    /// Student's grades summary page.
    /// </summary>
    public class StudentGradesViewModel : BaseViewModel, IScreenViewModel
    {
        #region Sub-Structs
        public struct GradeData
        {
            public int CourseID { get; set; }
            public int TeacherID { get; set; }
            public string CourseName { get; set; }
            public int Score { get; set; }
            public string TeacherNotes { get; set; }
        }
        #endregion

        #region Fields
        private List<Student> _students;
        private List<GradeData> _grades;

        private Student _currentStudent;
        private GradeData _selectedGrade;

        private double _averageGrade;
        private int _absences;
        private string _homeroomTeacher;

        private string _appealText;

        private ICommand _changeStudentCommand;
        private ICommand _appealGradeCommand;
        #endregion

        #region Properties / Commands
        // Base Properties
        public Person ConnectedPerson { get; private set; }
        public bool HasRequiredPermissions { get; }
        public string ScreenName { get { return "סיכום לימודים"; } }
        #endregion

        // Business Logic Properties / Commands

        /// <summary>
        /// All students that the current user can view
        /// </summary>
        public List<Student> Students
        {
            get
            {
                return _students;
            }
            set
            {
                if (_students != value)
                {
                    _students = value;
                    OnPropertyChanged("Students");
                }
            }
        }
        /// <summary>
        /// The student whose grades are viewed currently
        /// </summary>
        public Student CurrentStudent
        {
            get
            {
                return _currentStudent;
            }
            set
            {
                if (_currentStudent != value)
                {
                    _currentStudent = value;
                    OnPropertyChanged("CurrentStudent");

                    // Show this student's grades
                    Grades = _currentStudent.Grades.Select(score =>
                        new GradeData() 
                        { 
                            CourseID = score.courseID, 
                            TeacherID = score.teacherID,
                            CourseName = score.Course.courseName, 
                            Score = score.score, 
                            TeacherNotes = score.notes
                        }).ToList();

                    Absences = _currentStudent.absencesCounter;

                    // Show this studen't homeroom teacher (if any)
                    if (_currentStudent.Class.Teachers.Count > 0)
                    {
                        HomeroomTeacher = "מחנך: " +
                                    _currentStudent.Class.Teachers.First().Person.firstName +
                                    " " + _currentStudent.Class.Teachers.First().Person.lastName;
                    }
                    else
                    {
                        HomeroomTeacher = string.Empty;
                    }
                }
            }
        }

        /// <summary>
        /// The grades of the student
        /// </summary>
        public List<GradeData> Grades
        {
            get
            {
                return _grades;
            }
            set
            {
                if (_grades != value)
                {
                    _grades = value;
                    OnPropertyChanged("Grades");

                    // Update the average grade
                    if (_grades != null && _grades.Count > 0)
                    {
                        AverageGrade =  Math.Round(_grades.Average(x => x.Score), 1);
                    }
                    else
                    {
                        AverageGrade = 0;
                    }
                }
            }
        }
        public GradeData SelectedGrade
        {
            get
            {
                return _selectedGrade;
            }
            set
            {
                // Might have the same course for different student, so update selected grade everytime
                _selectedGrade = value;
                OnPropertyChanged("SelectedGrade");
            }
        }

        /// <summary>
        /// The average of the student's grades
        /// </summary>
        public double AverageGrade
        {
            get
            {
                return _averageGrade;
            }
            set
            {
                if (_averageGrade != value)
                {
                    _averageGrade = value;
                    OnPropertyChanged("AverageGrade");
                }
            }
        }

        /// <summary>
        /// The number of absences for this student
        /// </summary>
        public int Absences
        {
            get
            {
                return _absences;
            }
            set
            {
                if (_absences != value)
                {
                    _absences = value;
                    OnPropertyChanged("Absences");
                }
            }
        }

        /// <summary>
        /// The name of the Homeroom Teacher for this student
        /// </summary>
        public string HomeroomTeacher
        {
            get
            {
                return _homeroomTeacher;
            }
            set
            {
                if (_homeroomTeacher != value)
                {
                    _homeroomTeacher = value;
                    OnPropertyChanged("HomeroomTeacher");
                }
            }
        }

        /// <summary>
        /// Can the connected person view grades of multiple students?
        /// </summary>
        public bool CanViewDifferentStudents { get; private set; }

        /// <summary>
        /// Can the connected person appeal the grades?
        /// </summary>
        public bool CanAppealGrades { get; set; }

        /// <summary>
        /// An appeal request's text
        /// </summary>
        public string AppealText
        {
            get
            {
                return _appealText;
            }
            set
            {
                if (_appealText != value)
                {
                    _appealText = value;
                    OnPropertyChanged("AppealText");
                }
            }
        }

        /// <summary>
        /// Changes which student's grades are viewed
        /// </summary>
        public ICommand ChangeStudentCommand
        {
            get
            {
                if (_changeStudentCommand == null)
                {
                    _changeStudentCommand = new RelayCommand(
                        p => ChangeStudent((Student)p),
                        p => p is Student);
                }

                return _changeStudentCommand;
            }
        }

        public ICommand AppealGradeCommand
        {
            get
            {
                if (_appealGradeCommand == null)
                {
                    _appealGradeCommand = new RelayCommand(p => AppealGrade());
                }

                return _appealGradeCommand;
            }
        }

        #region Constructors
        public StudentGradesViewModel(Person connectedPerson, IMessageBoxService messageBoxService)
            : base(messageBoxService)
        {
            // This page is relevent to every one in the school, except for teachers that aren't homeroom teachers
            if (!connectedPerson.isTeacher || (connectedPerson.isTeacher && connectedPerson.Teacher.classID != null))
            {
                HasRequiredPermissions = true;
            }

            Students = new List<Student>();
            Grades = new List<GradeData>();
        }
        #endregion

        #region Methods
        public void Initialize(Person connectedPerson)
        {
            ConnectedPerson = connectedPerson;

            if (HasRequiredPermissions)
            {
                // Reset the students list
                Students.Clear();

                // Initialize the lists according to the user type
                if (ConnectedPerson.isStudent)
                {
                    // A student can only see his own grades
                    Students.Add(ConnectedPerson.Student);
                    CanViewDifferentStudents = false;

                    // Only a student can appeal grades (and only his own)
                    CanAppealGrades = true;
                }
                else if (ConnectedPerson.isTeacher && ConnectedPerson.Teacher.classID != null)
                {
                    // An homeroom teacher can see the grades of all of his students
                    Students.AddRange(ConnectedPerson.Teacher.Class.Students.Where(student => student.Person.User.isDisabled == false));
                    CanViewDifferentStudents = true;
                    CanAppealGrades = false;
                }
                else if (ConnectedPerson.isParent)
                {
                    // A parent can see the grades of all of his children
                    Students.AddRange(ConnectedPerson.ChildrenStudents.Where(student => student.Person.User.isDisabled == false));
                    CanViewDifferentStudents = true;
                    CanAppealGrades = false;
                }
                else if (ConnectedPerson.isPrincipal || ConnectedPerson.isSecretary)
                {
                    // The school management can see the grades of all the students in the school
                    SchoolEntities schoolData = new SchoolEntities();
                    Students.AddRange(schoolData.Students.Where(student => student.Person.User.isDisabled == false));
                    CanViewDifferentStudents = true;
                    CanAppealGrades = false;
                }

                CurrentStudent = Students.First();
            }
        }

        /// <summary>
        /// Changes which student's grades are viewed
        /// </summary>
        /// <param name="newStudent">The new student whose grades should be shown</param>
        private void ChangeStudent(Student newStudent)
        {
            // Make sure newStudent is one of the students that can be shown (is part of Students list)
            if (Students.Contains(newStudent))
            {
                CurrentStudent = newStudent;
            }
        }

        /// <summary>
        /// Attempt to appeal the currently selected grade
        /// </summary>
        private void AppealGrade()
        {
            // Check that a grade was selected
            if (SelectedGrade.CourseName != string.Empty)
            {
                // Check that a appeal text was entered 
                if (AppealText.Count() > 0)
                {
                    // Send an appeal message to the relevent teacher
                    MessagesHandler.CreateMessage("בקשת ערעור", AppealText, MessageRecipientsTypes.Person, ConnectedPerson.personID, SelectedGrade.TeacherID);

                    // Report that the appeal has been sent to the user
                    _messageBoxService.ShowMessage("הוזן ערעור", "הוזנה בקשת ערעור במקצוע " + SelectedGrade.CourseName,
                                                    MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);

                    // Clear after appeal
                    AppealText = string.Empty;
                }
                else
                {
                    // Report invalid input
                    _messageBoxService.ShowMessage("נכשל בהזנת ערעור", "אנא הזן הודעה לערעור", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
                }
            }
            else
            {
                // Report invalid input
                _messageBoxService.ShowMessage("נכשל בהזנת ערעור", "אנא בחר מקצוע לפני הזנת בקשת ערעור", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
        }
        #endregion
    }
}

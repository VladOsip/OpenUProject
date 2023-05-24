using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MySchoolYear.Model;
using MySchoolYear.ViewModel.Utilities;

namespace MySchoolYear.ViewModel
{
    public class UserCreationViewModel : BaseViewModel, IScreenViewModel
    {
        #region Fields
        private ICommand _registerUserCommand;
        private ICommand _refreshDataCommand;

        private bool _isNewStudent;
        private bool _isNewTeacher;
        private bool _isNewParent;
        private bool _isNewSecretary;

        private const int FIELD_NOT_SET = -1;
        #endregion

        #region Properties / Commands
        // Base Properties
        public Person ConnectedPerson { get; private set; }
        public bool HasRequiredPermissions { get; }
        public string ScreenName { get { return "יצירת משתמשים"; } }

        // Business Logic Properties
        public bool CanCreateSecretaries { get; private set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateTime Birthdate { get; set; }

        public bool IsNewStudent 
        { 
            get
            {
                return _isNewStudent;
            }
            set
            {
                if (_isNewStudent != value)
                {
                    _isNewStudent = value;
                    OnPropertyChanged("IsNewStudent");
                }
            }
        }
        public bool IsNewTeacher
        {
            get
            {
                return _isNewTeacher;
            }
            set
            {
                if (_isNewTeacher != value)
                {
                    _isNewTeacher = value;
                    OnPropertyChanged("IsNewTeacher");
                }
            }
        }
        public bool IsNewParent
        {
            get
            {
                return _isNewParent;
            }
            set
            {
                if (_isNewParent != value)
                {
                    _isNewParent = value;
                    OnPropertyChanged("IsNewParent");
                }
            }
        }
        public bool IsNewSecretary
        {
            get
            {
                return _isNewSecretary;
            }
            set
            {
                if (_isNewSecretary != value)
                {
                    _isNewSecretary = value;
                    OnPropertyChanged("IsNewSecretary");
                }
            }
        }

        public Dictionary<int, string> AvailableClasses { get; set; }
        public Dictionary<int, string> AvailableHomeroomClasses { get; set; }
        public Nullable<int> SelectedClass { get; set; }
        public Dictionary<int, string> AvailableParents { get; set; }
        public Nullable<int> SelectedParent { get; set; }
        
        public Dictionary<int, string> AvailableStudents { get; set; }
        public Nullable<int> SelectedStudent { get; set; }

        public Dictionary<int, string> AvailableCourses { get; set; }
        public Dictionary<int, string> AvailableCoursesMustChoose { get; set; }
        public Nullable<int> SelectedCourse1 { get; set; }
        public Nullable<int> SelectedCourse2 { get; set; }
        public Nullable<int> SelectedCourse3 { get; set; }
        public Nullable<int> SelectedCourse4 { get; set; }
        public Nullable<int> SelectedHomeroomClass { get; set; }

        /// <summary>
        /// Create a user per the screen's properties
        /// </summary>
        public ICommand RegisterUserCommand
        {
            get
            {
                if (_registerUserCommand == null)
                {
                    _registerUserCommand = new RelayCommand(
                        p => RegisterUser());
                }

                return _registerUserCommand;
            }
        }
        #endregion

        #region Constructors
        public UserCreationViewModel(Person connectedPerson, ICommand refreshDataCommand, IMessageBoxService messageBoxService)
            : base(messageBoxService)
        {
            // Check if the user is part of the management team (and therefor is allowed to create users)
            if (connectedPerson.isSecretary || connectedPerson.isPrincipal)
            {
                HasRequiredPermissions = true;
                _refreshDataCommand = refreshDataCommand;
                
                // Only the principal can create management users
                if (connectedPerson.isPrincipal)
                {
                    CanCreateSecretaries = true;
                }
                else
                {
                    CanCreateSecretaries = false;
                }

                AvailableClasses = new Dictionary<int, string>();
                AvailableParents = new Dictionary<int, string>();
                AvailableStudents = new Dictionary<int, string>();
                AvailableCourses = new Dictionary<int, string>();
                AvailableCoursesMustChoose = new Dictionary<int, string>();
                AvailableHomeroomClasses = new Dictionary<int, string>();
            }
            else
            {
                HasRequiredPermissions = false;
                CanCreateSecretaries = false;
            }
        }
        #endregion

        #region Methods
        public void Initialize(Person connectedPerson)
        {
            ConnectedPerson = connectedPerson;

            ResetAll();

            if (HasRequiredPermissions)
            {
                // Get the school data and use to it to populate this View Model's data
                SchoolEntities schoolData = new SchoolEntities();

                // Create a list of all the classes in the school
                schoolData.Classes.ToList().ForEach(currClass => AvailableClasses.Add(currClass.classID, currClass.className));

                AvailableHomeroomClasses.Add(FIELD_NOT_SET, "לא מוגדר");
                schoolData.Classes.Where(currClass => currClass.Teachers.Count() == 0).ToList()
                    .ForEach(currClass => AvailableHomeroomClasses.Add(currClass.classID, currClass.className));

                // Create a list of all the parents in the school
                AvailableParents.Add(FIELD_NOT_SET, "לא מוגדר");
                schoolData.Persons.Where(p => p.isParent).ToList()
                    .ForEach(parent => AvailableParents.Add(parent.personID, parent.firstName + " " + parent.lastName));

                // Create a list of all the students in the school
                schoolData.Persons.Where(p => p.isStudent).ToList()
                    .ForEach(student => AvailableStudents.Add(student.personID, student.firstName + " " + student.lastName));

                // Create a list of all the courses in the school
                schoolData.Courses.Where(course => course.isHomeroomTeacherOnly == false).ToList()
                    .ForEach(course => AvailableCoursesMustChoose.Add(course.courseID, course.courseName));
                AvailableCourses.Add(FIELD_NOT_SET, "לא מוגדר");
                AvailableCoursesMustChoose.ToList().ForEach(course => AvailableCourses.Add(course.Key, course.Value));
            }
        }

        // Clears all the data in this View Model
        private void ResetAll()
        {
            // Reset all of the lists
            AvailableClasses.Clear();
            AvailableParents.Clear();
            AvailableStudents.Clear();
            AvailableCourses.Clear();
            AvailableCoursesMustChoose.Clear();
            AvailableHomeroomClasses.Clear();

            // Reset all properties
            Username = "";
            FirstName = "";
            LastName = "";
            Email = "";
            Phone = "";
            Birthdate = new DateTime();

            IsNewStudent = false;
            IsNewTeacher = false;
            IsNewParent = false;
            IsNewSecretary = false;

            SelectedHomeroomClass = null;
            SelectedParent = null;
            SelectedStudent = null;
            SelectedClass = null;
            SelectedCourse1 = null;
            SelectedCourse2 = null;
            SelectedCourse3 = null;
            SelectedCourse4 = null;
        }

        /// <summary>
        /// Register a new user per the input properties
        /// </summary>
        private void RegisterUser()
        {
            // Check if the user has permissions to register a new user
            if (HasRequiredPermissions)
            {
                var validInput = CheckInputValidity();

                // Check if the input is valid and if so register a new user per the user type
                if (validInput.Valid)
                {
                    SchoolEntities schoolData = new SchoolEntities();

                    // Create the User
                    User newUser = new User()
                    {
                        username = Username,
                        password = "123456",
                        hasToChangePassword = true,
                        isDisabled = false
                    };
                    schoolData.Users.Add(newUser);
                    schoolData.SaveChanges();

                    // Create a corresponding Person
                    Person newPerson = new Person()
                    {
                        userID = newUser.userID,

                        firstName = FirstName,
                        lastName = LastName,
                        phoneNumber = Phone,
                        email = Email,
                        birthdate = Birthdate,
                        
                        isStudent = IsNewStudent,
                        isParent = IsNewParent,
                        isTeacher = IsNewTeacher,
                        isSecretary = IsNewSecretary,
                        isPrincipal = false
                    };
                    schoolData.Persons.Add(newPerson);
                    schoolData.SaveChanges();

                    // Check if the new user is a student, and create Student info accordingly
                    if (IsNewStudent)
                    {
                        Student newStudent = new Student()
                        {
                            studentID = newPerson.personID,
                            classID = SelectedClass.Value,
                            parentID = (SelectedParent != null && SelectedParent != FIELD_NOT_SET) ? SelectedParent : null,
                            absencesCounter = 0
                        };
                        schoolData.Students.Add(newStudent);
                        schoolData.SaveChanges();
                    }

                    // Check if the new user is a parent, update its child accordingly
                    if (IsNewParent)
                    {
                        schoolData.Students.Find(SelectedStudent.Value).parentID = newPerson.personID;
                    }

                    // Check if the new user is a teacher, and create Teacher info accordingly
                    if (IsNewTeacher)
                    {
                        Teacher newTeacher = new Teacher()
                        {
                            teacherID = newPerson.personID,
                            classID = (SelectedHomeroomClass != null && SelectedHomeroomClass != FIELD_NOT_SET) ? SelectedHomeroomClass : null,
                            firstCourseID = SelectedCourse1.Value,
                            secondCourseID = (SelectedCourse2 != null && SelectedCourse2 != FIELD_NOT_SET) ? SelectedCourse2 : null,
                            thirdCourseID = (SelectedCourse3 != null && SelectedCourse3 != FIELD_NOT_SET) ? SelectedCourse3 : null,
                            fourthCourseID = (SelectedCourse4 != null && SelectedCourse4 != FIELD_NOT_SET) ? SelectedCourse4 : null
                        };
                        schoolData.Teachers.Add(newTeacher);
                        schoolData.SaveChanges();
                    }

                    _messageBoxService.ShowMessage("נרשם המשתמש " + newPerson.firstName + " " + newPerson.lastName + "!",
                        "הצלחה!", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);
                    _refreshDataCommand.Execute(null);
                }
                else
                {
                    // Report invalid input
                    _messageBoxService.ShowMessage(validInput.ErrorReport, "הרשמה נכשלה!", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
                }
            }
        }

        /// <summary>
        /// Helper method for the registration. Checks if the input is valid
        /// </summary>
        /// <returns></returns>
        private ValidityResult CheckInputValidity()
        {
            ValidityResult result = new ValidityResult();
            result.Valid = true;

            // Check that a username was written
            if (Username == null || Username.Length == 0)
            {
                result.ErrorReport = "אנא הכנס שם משתמש";
                result.Valid = false;
            }
            // Is the username valid
            else if (Username.Length < Globals.MINIMUM_USERNAME_LENGTH || Username.Length > Globals.MAXIMUM_USERNAME_LENGTH)
            {
                result.ErrorReport = string.Format("שם משתמש לא תקין. אורך שם המשתמש חייב להיות בין {0} לבין {1} תווים",
                                                    Globals.MINIMUM_USERNAME_LENGTH, Globals.MAXIMUM_USERNAME_LENGTH);
                result.Valid = false;
            }
            // Make sure that last name was written
            else if (FirstName == null || FirstName.Length == 0)
            {
                result.ErrorReport = "אנא הכנס שם פרטי";
                result.Valid = false;
            }
            // Make sure that last name was written
            else if (LastName == null || LastName.Length == 0)
            {
                result.ErrorReport = "אנא הכנס שם משפחה";
                result.Valid = false;
            }
            // Make sure at least one user type was chosen
            else if (!IsNewStudent && !IsNewParent && !IsNewTeacher && !IsNewSecretary)
            {
                result.ErrorReport = "אנא בחר סוג משתמש";
                result.Valid = false;
            }
            // Student must be in a class
            else if (IsNewStudent && (SelectedClass == null || SelectedClass == FIELD_NOT_SET))
            {
                result.ErrorReport = "אנא בחר כיתה לתלמיד";
                result.Valid = false;
            }
            // Parent must have a child
            else if (IsNewParent && (SelectedStudent == null || SelectedStudent == FIELD_NOT_SET))
            {
                result.ErrorReport = "אנא בחר את הילד של ההורה";
                result.Valid = false;
            }
            else if (IsNewTeacher && (SelectedCourse1 == null || SelectedCourse1 == FIELD_NOT_SET))
            {
                result.ErrorReport = "אנא בחר מקצוע אחד לפחות למורה";
                result.Valid = false;
            }

            return result;
        }
        #endregion
    }
}

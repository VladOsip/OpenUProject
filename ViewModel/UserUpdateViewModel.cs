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
    public class UserUpdateViewModel : BaseViewModel, IScreenViewModel
    {
        #region Fields
        private ICommand _deleteUserCommand;
        private ICommand _updateUserCommand;
        private ICommand _refreshDataCommand;
        private SchoolEntities _schoolData;
        private string _selectedUserType;
        private int _selectedUser;

        private string _userEmail;
        private string _userPhone;
        private Nullable<DateTime> _userBirthdate;

        private bool _isStudent;
        private bool _isTeacher;
        private bool _isParent;
        private bool _isSecretary;
        private bool _isPrincipal;

        private Nullable<int> _selectedClass;
        private Nullable<int> _selectedParent;
        private Nullable<int> _selectedStudent;
        private Nullable<int> _selectedCourse1;
        private Nullable<int> _selectedCourse2;
        private Nullable<int> _selectedCourse3;
        private Nullable<int> _selectedCourse4;
        private Nullable<int> _selectedHomeroomClass;

        private const int FIELD_NOT_SET = -1;
        #endregion

        #region Properties / Commands
        // Base Properties
        public Person ConnectedPerson { get; private set; }
        public bool HasRequiredPermissions { get; }
        public string ScreenName { get { return "עדכון משתמשים"; } }

        // Business Logic Properties
        public bool CanEditManagement { get; private set; }
        public string Phone 
        { 
            get
            {
                return _userPhone;
            }
            set
            {
                if (_userPhone != value)
                {
                    _userPhone = value;
                    OnPropertyChanged("Phone");
                }
            }
        }
        public string Email
        {
            get
            {
                return _userEmail;
            }
            set
            {
                if (_userEmail != value)
                {
                    _userEmail = value;
                    OnPropertyChanged("Email");
                }
            }
        }
        public Nullable<DateTime> Birthdate 
        {
            get
            {
                return _userBirthdate;
            }
            set
            {
                if (_userBirthdate != value)
                {
                    _userBirthdate = value;
                    OnPropertyChanged("Birthdate");
                }
            }
        }

        public bool IsStudent 
        { 
            get
            {
                return _isStudent;
            }
            set
            {
                if (_isStudent != value)
                {
                    _isStudent = value;
                    OnPropertyChanged("IsStudent");
                }
            }
        }
        public bool IsTeacher
        {
            get
            {
                return _isTeacher;
            }
            set
            {
                if (_isTeacher != value)
                {
                    _isTeacher = value;
                    OnPropertyChanged("IsTeacher");
                }
            }
        }
        public bool IsParent
        {
            get
            {
                return _isParent;
            }
            set
            {
                if (_isParent != value)
                {
                    _isParent = value;
                    OnPropertyChanged("IsParent");
                }
            }
        }
        public bool IsSecretary
        {
            get
            {
                return _isSecretary;
            }
            set
            {
                if (_isSecretary != value)
                {
                    _isSecretary = value;
                    OnPropertyChanged("IsSecretary");
                }
            }
        }
        public bool IsPrincipal
        {
            get
            {
                return _isPrincipal;
            }
            set
            {
                if (_isPrincipal != value)
                {
                    _isPrincipal = value;
                    OnPropertyChanged("IsPrincipal");
                }
            }
        }

        public List<string> AvailableUserTypes { get; set; }
        public string SelectedUserType
        {
            get
            {
                return _selectedUserType;
            }
            set
            {
                if (_selectedUserType != value)
                {
                    _selectedUserType = value;
                    ChangeAvailableUsers(_selectedUserType);
                    OnPropertyChanged("SelectedUserType");
                }
            }
        }

        public ObservableDictionary<int, string> AvailableUsers { get; set; }
        public int SelectedUser 
        { 
            get
            {
                return _selectedUser;
            }
            set 
            {
                if (_selectedUser != value)
                {
                    _selectedUser = value;
                    ChangeSelectedUser();
                    OnPropertyChanged("SelectedUser");
                }
            }
        }

        public Dictionary<int, string> AvailableClasses { get; set; }
        public Dictionary<int, string> AvailableHomeroomClasses { get; set; }
        public Nullable<int> SelectedClass
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
        public Dictionary<int, string> AvailableParents { get; set; }
        public Nullable<int> SelectedParent
        {
            get
            {
                return _selectedParent;
            }
            set
            {
                if (_selectedParent != value)
                {
                    _selectedParent = value;
                    OnPropertyChanged("SelectedParent");
                }
            }
        }

        public Dictionary<int, string> AvailableStudents { get; set; }
        public Nullable<int> SelectedStudent
        {
            get
            {
                return _selectedStudent;
            }
            set
            {
                if (_selectedStudent != value)
                {
                    _selectedStudent = value;
                    OnPropertyChanged("SelectedStudent");
                }
            }
        }

        public Dictionary<int, string> AvailableCourses { get; set; }
        public Dictionary<int, string> AvailableCoursesMustChoose { get; set; }
        public Nullable<int> SelectedCourse1
        {
            get
            {
                return _selectedCourse1;
            }
            set
            {
                if (_selectedCourse1 != value)
                {
                    _selectedCourse1 = value;
                    OnPropertyChanged("SelectedCourse1");
                }
            }
        }
        public Nullable<int> SelectedCourse2
        {
            get
            {
                return _selectedCourse2;
            }
            set
            {
                if (_selectedCourse2 != value)
                {
                    _selectedCourse2 = value;
                    OnPropertyChanged("SelectedCourse2");
                }
            }
        }
        public Nullable<int> SelectedCourse3
        {
            get
            {
                return _selectedCourse3;
            }
            set
            {
                if (_selectedCourse3 != value)
                {
                    _selectedCourse3 = value;
                    OnPropertyChanged("SelectedCourse3");
                }
            }
        }
        public Nullable<int> SelectedCourse4
        {
            get
            {
                return _selectedCourse4;
            }
            set
            {
                if (_selectedCourse4 != value)
                {
                    _selectedCourse4 = value;
                    OnPropertyChanged("SelectedCourse4");
                }
            }
        }
        public Nullable<int> SelectedHomeroomClass
        {
            get
            {
                return _selectedHomeroomClass;
            }
            set
            {
                if (_selectedHomeroomClass != value)
                {
                    _selectedHomeroomClass = value;
                    OnPropertyChanged("SelectedHomeroomClass");
                }
            }
        }

        /// <summary>
        /// Create a user per the screen's properties
        /// </summary>
        public ICommand UpdateUserCommand
        {
            get
            {
                if (_updateUserCommand == null)
                {
                    _updateUserCommand = new RelayCommand(
                        p => UpdateUser());
                }

                return _updateUserCommand;
            }
        }

        /// <summary>
        /// Create a user per the screen's properties
        /// </summary>
        public ICommand DeleteUserCommand
        {
            get
            {
                if (_deleteUserCommand == null)
                {
                    _deleteUserCommand = new RelayCommand(
                        p => DeleteUser());
                }

                return _deleteUserCommand;
            }
        }
        #endregion

        #region Constructors
        public UserUpdateViewModel(Person connectedPerson, ICommand refreshDataCommand, IMessageBoxService messageBoxService)
            : base(messageBoxService)
        {
            // Check if the user is part of the management team (and therefor is allowed to update users)
            if (connectedPerson.isSecretary || connectedPerson.isPrincipal)
            {
                HasRequiredPermissions = true;
                _refreshDataCommand = refreshDataCommand;
                
                // Only the principal can edit management users
                if (connectedPerson.isPrincipal)
                {
                    CanEditManagement = true;
                }
                else
                {
                    CanEditManagement = false;
                }

                AvailableUserTypes = new List<string>();
                AvailableUsers = new ObservableDictionary<int, string>();
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
                CanEditManagement = false;
            }
        }
        #endregion

        #region Methods
        public void Initialize(Person connectedPerson)
        {
            // Reset all information
            ConnectedPerson = connectedPerson;
            AvailableUserTypes.Clear();
            AvailableUsers.Clear();
            AvailableClasses.Clear();
            AvailableParents.Clear();
            AvailableStudents.Clear();
            AvailableCourses.Clear();
            AvailableCoursesMustChoose.Clear();
            AvailableHomeroomClasses.Clear();

            if (HasRequiredPermissions)
            {
                _schoolData = new SchoolEntities();

                // Create a list of all the editable user types
                if (!CanEditManagement)
                {
                    AvailableUserTypes.AddRange(new List<string>() { Globals.USER_TYPE_STUDENT, Globals.USER_TYPE_TEACHERS, Globals.USER_TYPE_PARENTS });
                }
                else
                {
                    AvailableUserTypes.AddRange(new List<string>() { Globals.USER_TYPE_STUDENT, Globals.USER_TYPE_TEACHERS, Globals.USER_TYPE_PARENTS,
                                                                     Globals.USER_TYPE_SECRETARIES, Globals.USER_TYPE_PRINCIPAL });
                }
                SelectedUserType = AvailableUserTypes[0];

                // Create a list of all the classes in the school
                _schoolData.Classes.ToList().ForEach(currClass => AvailableClasses.Add(currClass.classID, currClass.className));

                AvailableHomeroomClasses.Add(FIELD_NOT_SET, "לא מוגדר");
                _schoolData.Classes.Where(currClass => currClass.Teachers.Count() == 0).ToList()
                    .ForEach(currClass => AvailableHomeroomClasses.Add(currClass.classID, currClass.className));

                // Create a list of all the parents in the school
                AvailableParents.Add(FIELD_NOT_SET, "לא מוגדר");
                _schoolData.Persons.Where(p => p.isParent).ToList()
                    .ForEach(parent => AvailableParents.Add(parent.personID, parent.firstName + " " + parent.lastName));

                // Create a list of all the students in the school
                _schoolData.Persons.Where(p => p.isStudent).ToList()
                    .ForEach(student => AvailableStudents.Add(student.personID, student.firstName + " " + student.lastName));

                // Create a list of all the courses in the school
                _schoolData.Courses.Where(course => course.isHomeroomTeacherOnly == false).ToList()
                    .ForEach(course => AvailableCoursesMustChoose.Add(course.courseID, course.courseName));
                AvailableCourses.Add(FIELD_NOT_SET, "לא מוגדר");
                AvailableCoursesMustChoose.ToList().ForEach(course => AvailableCourses.Add(course.Key, course.Value));
            }
        }

        /// <summary>
        /// Allow choosing users that are from the specific selectedUserType
        /// </summary>
        /// <param name="selectedUserType">User type of available users. Expected values per const USER_TYPE_X fields</param>
        private void ChangeAvailableUsers(string selectedUserType)
        {
            // Clean the available users from the previous choice of user types
            AvailableUsers.Clear();

            // Create a query all the editable users in the school from the specified type
            IQueryable<Person> usersQuery;
            switch (selectedUserType)
            {
                case Globals.USER_TYPE_STUDENT:
                {
                    usersQuery = _schoolData.Persons.Where(person => person.isStudent);
                    break;
                }
                case Globals.USER_TYPE_PARENTS:
                {
                    usersQuery = _schoolData.Persons.Where(person => person.isParent);
                    break;
                }
                case Globals.USER_TYPE_TEACHERS:
                {
                    usersQuery = _schoolData.Persons.Where(person => person.isTeacher);
                    break;
                }
                case Globals.USER_TYPE_SECRETARIES:
                {
                    usersQuery = _schoolData.Persons.Where(person => person.isSecretary);
                    break;
                }
                case Globals.USER_TYPE_PRINCIPAL:
                {
                    usersQuery = _schoolData.Persons.Where(person => person.isPrincipal);
                    break;
                }
                default:
                {
                    usersQuery = Enumerable.Empty<Person>().AsQueryable();
                    break;
                }
            }

            // Create a list of all the editable users in the school
            usersQuery.Where(person => !person.User.isDisabled).ToList().
                ForEach(person => AvailableUsers.Add(person.personID, person.firstName + " " + person.lastName));
        }

        /// <summary>
        /// Use the new selected user data
        /// </summary>
        private void ChangeSelectedUser()
        {
            // Get the selected person's data
            Person selectedPerson = _schoolData.Persons.Find(SelectedUser);
            if (selectedPerson != null)
            {
                // Update base info
                Email = selectedPerson.email;
                Phone = selectedPerson.phoneNumber;
                Birthdate = selectedPerson.birthdate;

                IsStudent = selectedPerson.isStudent;
                IsParent = selectedPerson.isParent;
                IsTeacher = selectedPerson.isTeacher;
                IsPrincipal = selectedPerson.isPrincipal;
                IsSecretary = selectedPerson.isSecretary;

                // Update student info
                if (IsStudent)
                {
                    SelectedClass = selectedPerson.Student.classID;
                    SelectedParent = (selectedPerson.Student.parentID != null) ? selectedPerson.Student.parentID : FIELD_NOT_SET; // Optional field
                }
                else
                {
                    SelectedClass = null;
                    SelectedParent = null;
                }

                // Update parent info
                if (IsParent)
                {
                    SelectedStudent = selectedPerson.ChildrenStudents.First().studentID;
                }
                else
                {
                    SelectedParent = null;
                }

                // Update teacher info
                if (IsTeacher)
                {
                    SelectedHomeroomClass = (selectedPerson.Teacher.classID != null) ? selectedPerson.Teacher.classID : FIELD_NOT_SET; // Optional field
                    SelectedCourse1 = selectedPerson.Teacher.firstCourseID;
                    SelectedCourse2 = (selectedPerson.Teacher.secondCourseID != null) ? selectedPerson.Teacher.secondCourseID : FIELD_NOT_SET; // Optional field
                    SelectedCourse3 = (selectedPerson.Teacher.thirdCourseID != null) ? selectedPerson.Teacher.thirdCourseID : FIELD_NOT_SET; // Optional field
                    SelectedCourse4 = (selectedPerson.Teacher.fourthCourseID != null) ? selectedPerson.Teacher.fourthCourseID : FIELD_NOT_SET; // Optional field
                }
                else
                {
                    SelectedHomeroomClass = null;
                    SelectedCourse1 = null;
                    SelectedCourse2 = null;
                    SelectedCourse3 = null;
                    SelectedCourse4 = null;
                }
            }
        }

        /// <summary>
        /// Update the selected user per the input properties
        /// </summary>
        private void UpdateUser()
        {
            // Check if the user has permissions to register a new user
            if (HasRequiredPermissions)
            {
                var validInput = CheckInputValidity();

                // Check if the input is valid and if so register a new user per the user type
                if (validInput.Valid)
                {
                    // Update the Person data
                    Person selectedPerson = _schoolData.Persons.Find(SelectedUser);
                    selectedPerson.phoneNumber = Phone;
                    selectedPerson.email = Email;
                    selectedPerson.birthdate = Birthdate;
                    selectedPerson.isStudent = IsStudent;
                    selectedPerson.isParent = IsParent;
                    selectedPerson.isTeacher = IsTeacher;
                    selectedPerson.isSecretary = IsSecretary;
                    selectedPerson.isPrincipal = IsPrincipal;
                    _schoolData.SaveChanges();

                    // Check if the user is a student, and update Student info accordingly
                    if (IsStudent)
                    {
                        Student personAsStudent = selectedPerson.Student;

                        // If the user wasn't a student before, create a new student. Otherwise update his data
                        if (personAsStudent == null)
                        {
                            personAsStudent = new Student()
                            {
                                studentID = selectedPerson.personID,
                                classID = SelectedClass.Value,
                                parentID = (SelectedParent != null && SelectedParent != FIELD_NOT_SET) ? SelectedParent : null,
                                absencesCounter = 0
                            };
                            _schoolData.Students.Add(personAsStudent);
                        }
                        else
                        {
                            personAsStudent.classID = SelectedClass.Value;
                            personAsStudent.parentID = (SelectedParent != null && SelectedParent != FIELD_NOT_SET) ? SelectedParent : null;
                        }
                        _schoolData.SaveChanges();
                    }

                    // Check if the user is a parent and update its child accordingly
                    if (IsParent)
                    {
                        _schoolData.Students.Find(SelectedStudent.Value).parentID = selectedPerson.personID;
                    }

                    // Check if the user is a teacher, and update its Teacher info accordingly
                    if (IsTeacher)
                    {
                        Teacher personAsTeacher = selectedPerson.Teacher;

                        // If the user wasn't a teacher before, create a new teacher. Otherwise update his data
                        if (personAsTeacher == null)
                        {
                            personAsTeacher = new Teacher()
                            {
                                teacherID = selectedPerson.personID,
                                classID = (SelectedHomeroomClass != null && SelectedHomeroomClass != FIELD_NOT_SET) ? SelectedHomeroomClass : null,
                                firstCourseID = SelectedCourse1.Value,
                                secondCourseID = (SelectedCourse2 != null && SelectedCourse2 != FIELD_NOT_SET) ? SelectedCourse2 : null,
                                thirdCourseID = (SelectedCourse3 != null && SelectedCourse3 != FIELD_NOT_SET) ? SelectedCourse3 : null,
                                fourthCourseID = (SelectedCourse4 != null && SelectedCourse4 != FIELD_NOT_SET) ? SelectedCourse4 : null
                            };

                            _schoolData.Teachers.Add(personAsTeacher);
                        }
                        else
                        {
                            personAsTeacher.classID = (SelectedHomeroomClass != null && SelectedHomeroomClass != FIELD_NOT_SET) ? SelectedHomeroomClass : null;
                            personAsTeacher.firstCourseID = SelectedCourse1.Value;
                            personAsTeacher.secondCourseID = (SelectedCourse2 != null && SelectedCourse2 != FIELD_NOT_SET) ? SelectedCourse2 : null;
                            personAsTeacher.thirdCourseID = (SelectedCourse3 != null && SelectedCourse3 != FIELD_NOT_SET) ? SelectedCourse3 : null;
                            personAsTeacher.fourthCourseID = (SelectedCourse4 != null && SelectedCourse4 != FIELD_NOT_SET) ? SelectedCourse4 : null;
                        }
                        _schoolData.SaveChanges();
                    }

                    _messageBoxService.ShowMessage("עודכן המשתמש " + selectedPerson.firstName + " " + selectedPerson.lastName + "!",
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
        /// Delete the selected user
        /// </summary>
        private void DeleteUser()
        {
            // Get the selected person's data
            Person selectedPerson = _schoolData.Persons.Find(SelectedUser);

            // As this is a serious action, request a confirmation from the user
            bool confirmation = _messageBoxService.ShowMessage("האם אתה בטוח שברצונך למחוק את " + selectedPerson.firstName + " " + selectedPerson.lastName,
                                                                "מחיקת משתמש", MessageType.ACCEPT_CANCEL_MESSAGE, MessagePurpose.INFORMATION);
            if (confirmation == true)
            {
                selectedPerson.User.isDisabled = true;
                _messageBoxService.ShowMessage("נמחק המשתמש " + selectedPerson.firstName + " " + selectedPerson.lastName + "!",
                        "הצלחה!", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);
                _refreshDataCommand.Execute(null);
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

            // Make sure at least one user type was chosen
            if (!IsStudent && !IsParent && !IsTeacher && !IsSecretary && !IsPrincipal)
            {
                result.ErrorReport = "אנא בחר סוג משתמש";
                result.Valid = false;
            }
            // Student must be in a class
            else if (IsStudent && (SelectedClass == null || SelectedClass == FIELD_NOT_SET))
            {
                result.ErrorReport = "אנא בחר כיתה לתלמיד";
                result.Valid = false;
            }
            // Parent must have a child
            else if (IsParent && (SelectedStudent == null || SelectedStudent == FIELD_NOT_SET))
            {
                result.ErrorReport = "אנא בחר את הילד של ההורה";
                result.Valid = false;
            }
            else if (IsTeacher && (SelectedCourse1 == null || SelectedCourse1 == FIELD_NOT_SET))
            {
                result.ErrorReport = "אנא בחר מקצוע אחד לפחות למורה";
                result.Valid = false;
            }

            return result;
        }
        #endregion
    }
}

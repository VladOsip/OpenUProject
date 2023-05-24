using MySchoolYear.Model;
using System.Linq;
using System.Collections.Generic;
using MySchoolYear.ViewModel.Utilities;
using System;
using System.Collections.ObjectModel;

namespace MySchoolYear.ViewModel
{
    /// <summary>
    /// The school's contacts page - display contact information for the management and teachers in the school
    /// </summary>
    public class ContactsInfoViewModel : BaseViewModel, IScreenViewModel
    {
        #region Sub-Structs
        public struct SecretaryInfo
        {
            public string Name { get; set; }
            public string Phone { get; set; }
        }
        public struct TeacherInfo
        {
            public string Name { get; set; }
            public string CoursesNames { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
        }
        #endregion

        #region Properties
        // Base Properties
        public Person ConnectedPerson { get; private set; }
        public bool HasRequiredPermissions { get; private set; }
        public string ScreenName { get { return "צור קשר"; } }

        // Business Logic Properties
        public string PrincipalName { get; private set; }
        public string PrincipalEmail { get; private set; }
        public ObservableCollection<SecretaryInfo> Secretaries { get; set; }
        public ObservableCollection<TeacherInfo> Teachers { get; set; }
        #endregion

        #region Constructors
        public ContactsInfoViewModel(Person connectedPerson)
        {
            HasRequiredPermissions = true;
            Secretaries = new ObservableCollection<SecretaryInfo>();
            Teachers = new ObservableCollection<TeacherInfo>();
        }
        #endregion

        #region Methods
        public void Initialize(Person connectedPerson)
        {
            ConnectedPerson = connectedPerson;
            SchoolEntities schoolData = new SchoolEntities();

            // School basic information
            var schoolInfo = schoolData.SchoolInfo;

            // Get the principal information
            var principal = schoolData.Persons.FirstOrDefault(person => person.isPrincipal && !person.User.isDisabled);
            if (principal != null)
            {
                PrincipalName = principal.firstName + " " + principal.lastName;
                PrincipalEmail = principal.email;
            }

            // Get the secretaries information
            Secretaries.Clear();
            schoolData.Persons.Where(person => person.isSecretary && !person.User.isDisabled).ToList()
                .ForEach(person => Secretaries.Add(new SecretaryInfo() { Name = person.firstName + " " + person.lastName, Phone = person.phoneNumber }));

            // Get the teachers information
            Teachers.Clear();
            schoolData.Persons.Where(person => person.isTeacher && !person.User.isDisabled).ToList()
               .ForEach(person => Teachers.Add(new TeacherInfo()
               {
                   Name = person.firstName + " " + person.lastName,
                   CoursesNames = GetTeacherCourseNames(person.Teacher),
                   Email = person.email,
                   Phone = person.phoneNumber
               }));
        }

        /// <summary>
        /// Get the names of all the courses a teacher teaches
        /// </summary>
        /// <param name="teacher">The teacher</param>
        /// <returns>String with the names of all the teacher's courses</returns>
        private string GetTeacherCourseNames(Teacher teacher)
        {
            string courseNames = string.Empty;

            // Get the name of each course the teacher teaches
            foreach (string teacherCourseName in TeacherCoursesHandler.GetTeacherCoursesNames(teacher, false).Values)
            {
                courseNames += teacherCourseName + ", ";
            }

            // Remove the last ', '
            courseNames = courseNames.Substring(0, courseNames.Length - 2);

            return courseNames;
        }
        #endregion
    }
}
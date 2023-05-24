using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using MySchoolYear.Model;
using MySchoolYear.ViewModel.Utilities;

namespace MySchoolYear.ViewModel
{
    /// <summary>
    /// School settings page
    /// </summary>
    public class SchoolManagementViewModel : BaseViewModel, IScreenViewModel
    {
        #region Fields
        private string _schoolName;
        private string _schoolDescription;
        private string _schoolLogo;

        private ICommand _chooseImageCommand;
        private ICommand _prepareNewYearCommand;
        private ICommand _saveChangesCommand;
        private ICommand _refreshDataCommand;
        #endregion

        #region Properties / Commands
        // Base Properties
        public Person ConnectedPerson { get; private set; }
        public bool HasRequiredPermissions { get; }
        public string ScreenName { get { return "ניהול בית הספר"; } }

        // Business Logic Properties
        public string SchoolName
        {
            get
            {
                return _schoolName;
            }
            set
            {
                if (_schoolName != value)
                {
                    _schoolName = value;
                    OnPropertyChanged("SchoolName");
                }
            }
        }
        public string SchoolDescription
        {
            get
            {
                return _schoolDescription;
            }
            set
            {
                if (_schoolDescription != value)
                {
                    _schoolDescription = value;
                    OnPropertyChanged("SchoolDescription");
                }
            }
        }
        public string SchoolLogo
        {
            get
            {
                return _schoolLogo;
            }
            set
            {
                if (_schoolLogo != value)
                {
                    _schoolLogo = value;
                    OnPropertyChanged("SchoolLogo");
                    OnPropertyChanged("SchoolLogoFullPath");
                }
            }
        }
        public string SchoolLogoFullPath
        {
            get
            {
                return SchoolLogo.Contains(":\\") ? SchoolLogo : "/MySchoolYear;component/Images/" + SchoolLogo;   
            }
        }

        /// <summary>
        /// Let the user choose a file from his computer
        /// </summary>
        public ICommand ChooseImageCommand
        {
            get
            {
                if (_chooseImageCommand == null)
                {
                    _chooseImageCommand = new RelayCommand(
                        p => ChooseImage());
                }

                return _chooseImageCommand;
            }
        }

        /// <summary>
        /// Reset data in preparation for a new year (deleting studentds, scores, messages, events...)
        /// </summary>
        public ICommand PrepareNewYearCommand
        {
            get
            {
                if (_prepareNewYearCommand == null)
                {
                    _prepareNewYearCommand = new RelayCommand(
                        p => PrepareNewYear());
                }

                return _prepareNewYearCommand;
            }
        }

        /// <summary>
        /// Update the school information
        /// </summary>
        public ICommand SaveChangesCommand
        {
            get
            {
                if (_saveChangesCommand == null)
                {
                    _saveChangesCommand = new RelayCommand(
                        p => SaveChanges());
                }

                return _saveChangesCommand;
            }
        }
        #endregion

        #region Constructors
        public SchoolManagementViewModel(Person connectedPerson, ICommand refreshDataCommand, IMessageBoxService messageBoxService)
            : base (messageBoxService)
        {
            if (connectedPerson.isPrincipal)
            {
                HasRequiredPermissions = true;
                _refreshDataCommand = refreshDataCommand;
            }
        }
        #endregion

        #region Methods
        public void Initialize(Person connectedPerson)
        {
            ConnectedPerson = connectedPerson;

            if (HasRequiredPermissions)
            {
                var mySchoolInfo = new SchoolEntities().SchoolInfo;
                SchoolName = mySchoolInfo.Find("schoolName").value;
                SchoolDescription = mySchoolInfo.Find("schoolDescription").value;
                SchoolLogo = mySchoolInfo.Find("schoolImage").value;
            }
        }

        /// <summary>
        /// Let the user choose an image through Window's OpenFileDialog 
        /// </summary>
        private void ChooseImage()
        {
            if (HasRequiredPermissions)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == true)
                    SchoolLogo = openFileDialog.FileName;
            }
        }

        /// <summary>
        /// Reset data in preparation for a new year (deleting studentds, scores, messages, events...)
        /// </summary>
        private void PrepareNewYear()
        {
            if (HasRequiredPermissions)
            {
                // Ask for confirmation again as this is a dangerous action
                bool confirmationResult = _messageBoxService.ShowMessage("תהליך זה ימחק מידע על התלמידים הקיימים. האם להמשיך?",
                                                               "Are you sure?", MessageType.ACCEPT_CANCEL_MESSAGE, MessagePurpose.INFORMATION);
                if (confirmationResult)
                {
                    // Delete all yearly information (student users, events, scores, lessons...)
                    SchoolEntities mySchool = new SchoolEntities();
                    
                    // Delete students
                    foreach (Person person in mySchool.Persons.Where(p => p.isStudent))
                    {
                        person.User.isDisabled = true;
                        mySchool.Persons.Remove(person);
                    }

                    mySchool.Students.RemoveRange(mySchool.Students);
                    mySchool.Events.RemoveRange(mySchool.Events);
                    mySchool.Messages.RemoveRange(mySchool.Messages);
                    mySchool.Grades.RemoveRange(mySchool.Grades);
                    mySchool.Lessons.RemoveRange(mySchool.Lessons);

                    mySchool.SaveChanges();
                    _refreshDataCommand.Execute(null);
                    _messageBoxService.ShowMessage("מידע עודכן!", "Saved Changes", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);
                }
            }
        }

        /// <summary>
        /// Update the school information
        /// </summary>
        private void SaveChanges()
        {
            if (HasRequiredPermissions)
            {
                SchoolEntities mySchool = new SchoolEntities();
                mySchool.SchoolInfo.Find("schoolName").value = SchoolName;
                mySchool.SchoolInfo.Find("schoolDescription").value = SchoolDescription;
                mySchool.SchoolInfo.Find("schoolImage").value = SchoolLogo;

                mySchool.SaveChanges();
                _refreshDataCommand.Execute(null);
                _messageBoxService.ShowMessage("הגדרות עודכנו!", "Saved Changes", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);
            }
        }
        #endregion
    }
}

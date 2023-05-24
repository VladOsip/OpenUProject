using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySchoolYear.Model;

namespace MySchoolYear.ViewModel.Utilities
{
    /// <summary>
    /// Required interface for the actual screens of the application
    /// </summary>
    public interface IScreenViewModel
    {
        #region Properties
        Person ConnectedPerson { get; }
        bool HasRequiredPermissions { get; }
        string ScreenName { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Initialize the ViewModel properties
        /// </summary>
        /// <param name="connectedPerson">The current information about the connected user</param>
        void Initialize(Person connectedPerson);
        #endregion
    }
}

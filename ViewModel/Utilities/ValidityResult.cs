using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySchoolYear.ViewModel.Utilities
{
    /// <summary>
    /// Helper result struct for validity checks - contains whether the validity checks were successful, and if not contains an error message.
    /// </summary>
    public struct ValidityResult
    {
        public bool Valid;
        public string ErrorReport;
    }
}

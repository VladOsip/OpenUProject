using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySchoolYear.Model
{
    /// <summary>
    /// This contains global static variables and information
    /// </summary>
    public static class Globals
    {
        #region Constants
        public const int MINIMUM_USERNAME_LENGTH = 3;
        public const int MINIMUM_PASSWORD_LENGTH = 4;
        public const int MAXIMUM_USERNAME_LENGTH = 16;
        public const int MAXIMUM_PASSWORD_LENGTH = 16;

        public const string USER_TYPE_STUDENT = "תלמידים";
        public const string USER_TYPE_PARENTS = "הורים";
        public const string USER_TYPE_TEACHERS = "מורים";
        public const string USER_TYPE_SECRETARIES = "מזכירות";
        public const string USER_TYPE_PRINCIPAL = "מנהלים";

        public readonly static string[] HOUR_NAMES = { "ראשונה", "שנייה", "שלישית", "רביעית", "חמישית", "שישית", "שביעית", "שמינית", "תשיעית", "עשירית" };
        public readonly static string[] DAY_NAMES = { "ראשון", "שני", "שלישי", "רביעי", "חמישי", "שישי" };

        public const int GRADE_MAX_VALUE = 100;
        public const int GRADE_MIN_VALUE = 1;
        public const int GRADE_NO_VALUE = 0;
        #endregion

        #region Assistant Methods
        /// <summary>
        /// Converts school hours from a number into HOUR_NAMES format
        /// </summary>
        /// <param name="hourNumber">The school hour number</param>
        /// <returns>The corresponding hour name as a string</returns>
        public static string ConvertHourNumberToName(Nullable<int> hourNumber)
        {
            string hourName = "אין"; ;
            
            // Check if hourNumber is within the valid range for school hours
            if (hourNumber != null && hourNumber > 0 && hourNumber < HOUR_NAMES.Count())
            {
                hourName = HOUR_NAMES[hourNumber.Value];
            }

            return hourName;
        }

        /// <summary>
        /// Converts school days from a number into DAY_NAMES format
        /// </summary>
        /// <param name="dayNumber">The school day number</param>
        /// <returns>The corresponding day name as a string</returns>
        public static string ConvertDayNumberToName(Nullable<int> dayNumber)
        {
            string dayName = "אין"; ;

            // Check if dayNumber is within the valid range for school days
            if (dayNumber != null && dayNumber > 0 && dayNumber < DAY_NAMES.Count())
            {
                dayName = DAY_NAMES[dayNumber.Value];
            }

            return dayName;
        }
        #endregion
    }
}

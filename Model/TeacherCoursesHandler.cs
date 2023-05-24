using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySchoolYear.Model
{
    /// <summary>
    /// Helper methods to handle teacher courses' information
    /// </summary>
    public static class TeacherCoursesHandler
    {
        /// <summary>
        /// Create a dictionary of <ID, NAME> of all the courses a specific teacher can teach
        /// </summary>
        /// <param name="teacher">The teacher to use</param>
        /// <param name="searchForHomeroomCourses">Whether to gather homeroom teachers' courses too</param>
        /// <returns>A dictionary with the IDs and names of all the courses this teacher can teach</returns>
        public static Dictionary<int, string> GetTeacherCoursesNames(Teacher teacher, bool searchForHomeroomCourses)
        {
            Dictionary<int, string> teacherCourses = new Dictionary<int, string>();

            if (teacher != null)
            {
                // Gather the teacher's courses information
                if (teacher.firstCourseID != null)
                {
                    teacherCourses.Add(teacher.firstCourseID.Value, teacher.FirstCourse.courseName);
                }
                if (teacher.secondCourseID != null)
                {
                    teacherCourses.Add(teacher.secondCourseID.Value, teacher.SecondCourse.courseName);
                }
                if (teacher.thirdCourseID != null)
                {
                    teacherCourses.Add(teacher.thirdCourseID.Value, teacher.ThirdCourse.courseName);
                }
                if (teacher.fourthCourseID != null)
                {
                    teacherCourses.Add(teacher.fourthCourseID.Value, teacher.FourthCourse.courseName);
                }

                // Homeroom teachers can also teach their homeroom class any homeroom course
                if (searchForHomeroomCourses && teacher.classID != null)
                {
                    SchoolEntities schoolData = new SchoolEntities();
                    foreach (Course course in schoolData.Courses.Where(course => course.isHomeroomTeacherOnly))
                    {
                        // Make sure the course wasn't added already
                        if (!teacherCourses.ContainsKey(course.courseID))
                        {
                            teacherCourses.Add(course.courseID, course.courseName);
                        }
                    }
                }
            }

            return teacherCourses;
        }

        /// <summary>
        /// Create a list of of all the courses a specific teacher teaches
        /// </summary>
        /// <param name="teacher">The teacher to use</param>
        /// <param name="searchForHomeroomCourses">Whether to gather homeroom teachers' courses too</param>
        /// <returns>A list of courses this teacher can teach</returns>
        public static List<Course> GetTeacherCourses(Teacher teacher, bool searchForHomeroomCourses)
        {
            List<Course> teacherCourses = new List<Course>();

            if (teacher != null)
            {
                // Gather the teacher's courses information
                if (teacher.firstCourseID != null)
                {
                    teacherCourses.Add(teacher.FirstCourse);
                }
                if (teacher.secondCourseID != null)
                {
                    teacherCourses.Add(teacher.SecondCourse);
                }
                if (teacher.thirdCourseID != null)
                {
                    teacherCourses.Add(teacher.ThirdCourse);
                }
                if (teacher.fourthCourseID != null)
                {
                    teacherCourses.Add(teacher.FourthCourse);
                }

                // Homeroom teachers can also teach their homeroom class any homeroom course
                if (searchForHomeroomCourses && teacher.classID != null)
                {
                    SchoolEntities schoolData = new SchoolEntities();
                    foreach (Course course in schoolData.Courses.Where(course => course.isHomeroomTeacherOnly))
                    {
                        // Make sure the course wasn't added already
                        if (!teacherCourses.Contains(course))
                        {
                            teacherCourses.Add(course);
                        }
                    }
                }
            }

            return teacherCourses;
        }
    }
}

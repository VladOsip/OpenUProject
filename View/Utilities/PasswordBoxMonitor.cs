using System.Windows;
using System.Windows.Controls;

namespace MySchoolYear.View.Utilities
{
    /// <summary>
    /// A custom template for PasswordBox control that supports place-holder text.
    /// </summary>
    class PasswordBoxMonitor : DependencyObject
    {
        public static readonly DependencyProperty IsMonitoringProperty =
            DependencyProperty.RegisterAttached("IsMonitoring", typeof(bool), typeof(PasswordBoxMonitor), new UIPropertyMetadata(false, OnIsMonitoringChanged));

        public static readonly DependencyProperty PasswordLengthProperty =
            DependencyProperty.RegisterAttached("PasswordLength", typeof(int), typeof(PasswordBoxMonitor), new UIPropertyMetadata(0));

        /// <summary>
        /// Check if an object is monitoring its property.
        /// </summary>
        /// <param name="obj">The object whose property we want to check.</param>
        /// <returns> Whether the property is monitored or not</returns>
        public static bool GetIsMonitoring(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsMonitoringProperty);
        }

        /// <summary>
        /// Set the object to monitor its property
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetIsMonitoring(DependencyObject obj, bool value)
        {
            obj.SetValue(IsMonitoringProperty, value);
        }

        /// <summary>
        /// Get the current password's length
        /// </summary>
        /// <param name="obj">The password object</param>
        /// <returns>Password's length</returns>
        public static int GetPasswordLength(DependencyObject obj)
        {
            return (int)obj.GetValue(PasswordLengthProperty);
        }

        /// <summary>
        /// Update password length
        /// </summary>
        /// <param name="obj">Password object</param>
        /// <param name="value">New password's length</param>
        public static void SetPasswordLength(DependencyObject obj, int value)
        {
            obj.SetValue(PasswordLengthProperty, value);
        }

        /// <summary>
        /// Event for when the password is changed while its being monitored.
        /// </summary>
        /// <param name="d">The password object</param>
        /// <param name="e">Info on the monitored property</param>
        private static void OnIsMonitoringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pb = d as PasswordBox;
            if (pb == null)
            {
                return;
            }
            if ((bool)e.NewValue)
            {
                pb.PasswordChanged += PasswordChanged;
            }
            else
            {
                pb.PasswordChanged -= PasswordChanged;
            }
        }

        /// <summary>
        /// Event for when the password is changed
        /// </summary>
        /// <param name="sender">The password object</param>
        /// <param name="e">Info on how the password was changed</param>
        static void PasswordChanged(object sender, RoutedEventArgs e)
        {
            var pb = sender as PasswordBox;
            if (pb == null)
            {
                return;
            }
            SetPasswordLength(pb, pb.Password.Length);
        }
    }
}

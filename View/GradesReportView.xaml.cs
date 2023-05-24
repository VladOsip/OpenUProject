using MySchoolYear.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MySchoolYear.View
{
    /// <summary>
    /// Interaction logic for GradesReportView.xaml
    /// </summary>
    public partial class GradesReportView : UserControl
    {
        public GradesReportView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Make sure a field has only numbers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsValidGrade(object sender, TextCompositionEventArgs e)
        {
            try
            {
                // Check the previewed character that is added is a number
                Convert.ToInt32(e.Text);

                // Get the text of the textbox after adding the previewed character, using the current selection in the textbox to determine the location
                // of the new character
                TextBox textBox = sender as TextBox;
                string updatedText =
                    textBox.Text.Substring(0, textBox.SelectionStart) + e.Text + textBox.Text.Substring(textBox.SelectionStart + textBox.SelectionLength);

                // Check that the full grade text is a valid grade score
                int inputNumber = Convert.ToInt32(updatedText);

                // Check if the score is outside the valid grade score
                if ((inputNumber > Globals.GRADE_MAX_VALUE || inputNumber < Globals.GRADE_MIN_VALUE) && inputNumber != Globals.GRADE_NO_VALUE)
                {
                    e.Handled = true;
                }
            }
            catch
            {
                e.Handled = true;
            };
        }
    }
}

using System.Windows;
using MySchoolYear.ViewModel.Utilities;

namespace MySchoolYear.View.Utilities
{
    /// <summary>
    /// A WPF message box.
    /// </summary>
    public class WPFMessageBoxService : IMessageBoxService
    {
        public bool ShowMessage(string text, string caption, MessageType messageType, MessagePurpose purpose = MessagePurpose.INFORMATION)
        {
            // Choose an appropriate image to this message purpose.
            MessageBoxImage messagePurpose;
            switch (purpose)
            {
                case MessagePurpose.ERROR:
                    messagePurpose = MessageBoxImage.Error;
                    break;
                case MessagePurpose.DEBUG:
                    messagePurpose = MessageBoxImage.Asterisk;
                    break;
                case MessagePurpose.INFORMATION:
                default:
                    messagePurpose = MessageBoxImage.Information;
                    break;
            }

            // Choose and appropriate MessageBoxButton for this message type
            MessageBoxButton messageButtons;
            switch (messageType)
            {
                case MessageType.ACCEPT_CANCEL_MESSAGE:
                    messageButtons = MessageBoxButton.OKCancel;
                    break;
                case MessageType.YES_NO_MESSAGE:
                    messageButtons = MessageBoxButton.YesNo;
                    break;
                case MessageType.OK_MESSAGE:
                default:
                    messageButtons = MessageBoxButton.OK;
                    break;
            }

            // Display the message box and return the user's response.
            MessageBoxResult result = MessageBox.Show(text, caption, messageButtons, messagePurpose, MessageBoxResult.OK , MessageBoxOptions.RtlReading);
            if (result == MessageBoxResult.Yes || result == MessageBoxResult.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
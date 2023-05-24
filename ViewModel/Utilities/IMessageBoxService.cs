namespace MySchoolYear.ViewModel.Utilities
{
    /// <summary>
    /// Possible message box types.
    /// </summary>
    public enum MessageType
    {
        OK_MESSAGE,
        YES_NO_MESSAGE,
        ACCEPT_CANCEL_MESSAGE
    }
    
    /// <summary>
    /// Intended Purpose of the message box
    /// </summary>
    public enum MessagePurpose
    {
        INFORMATION,
        ERROR,
        DEBUG,
    }

    /// <summary>
    /// Support message boxes.
    /// </summary>
    public interface IMessageBoxService
    {
        /// <summary>
        /// Show a message box
        /// </summary>
        /// <param name="text">Text of the message box</param>
        /// <param name="caption">Title of the message box</param>
        /// <param name="messageType">Possible actions from the message box</param>
        /// <param name="purpose">Intended purpose for showing the message box</param>
        /// <returns>Action of the user</returns>
        bool ShowMessage(string text, string caption, MessageType messageType, MessagePurpose purpose = MessagePurpose.INFORMATION);
    }
}
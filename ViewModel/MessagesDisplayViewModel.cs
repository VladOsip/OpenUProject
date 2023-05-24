using System;
using System.Collections.Generic;
using System.Linq;
using MySchoolYear.Model;
using MySchoolYear.ViewModel.Utilities;

namespace MySchoolYear.ViewModel
{
    /// <summary>
    /// Viewing incoming messages
    /// </summary>
    public class MessagesDisplayViewModel : BaseViewModel, IScreenViewModel
    {
        #region Sub-Structs
        public class DisplayedMessage
        {
            public string SenderName { get; set; }
            public string RecipientName { get; set; }
            public DateTime MessageDateTime { get; set; }
            public string MessageDate 
            { 
                get
                {
                    return MessageDateTime.ToString("dd/MM/yyyy");
                }
            }
            public string Title { get; set; }
            public string Details { get; set; }
        }
        #endregion

        #region Properties / Commands
        // Base Properties
        public Person ConnectedPerson { get; private set; }
        public bool HasRequiredPermissions { get; }
        public string ScreenName { get { return "הצגת הודעות"; } }

        // Business Logic Properties
        public List<DisplayedMessage> Messages { get; set; }
        #endregion

        #region Constructors
        public MessagesDisplayViewModel(Person connectedPerson)
        {
            HasRequiredPermissions = true;
        }
        #endregion

        #region Methods
        // Gather the messages that are relevent to the connected user.
        public void Initialize(Person connectedPerson)
        {
            ConnectedPerson = connectedPerson;

            // Reset data first
            Messages = new List<DisplayedMessage>();
            var schoolMessages = new SchoolEntities().Messages;

            // Start by gathering the messages that are for everyone in the school or directly to the connected user.
            schoolMessages.Where(message => message.forEveryone).ToList()
                .ForEach(message => Messages.Add(ModelMessageToDisplayedMessage(message, MessageRecipientsTypes.Everyone)));
            schoolMessages.Where(message => message.recipientID == ConnectedPerson.personID).ToList()
                .ForEach(message => Messages.Add(ModelMessageToDisplayedMessage(message, MessageRecipientsTypes.Person)));

            // Gather messages depending on the user permissions
            if (ConnectedPerson.isStudent)
            {
                // User is a student => Gather messages for their class, and messages for all students
                schoolMessages.Where(message => message.recipientClassID == ConnectedPerson.Student.classID)
                    .ToList().ForEach(message => Messages.Add(ModelMessageToDisplayedMessage(message, MessageRecipientsTypes.Class)));
                schoolMessages.Where(message => message.forAllStudents).ToList()
                    .ForEach(message => Messages.Add(ModelMessageToDisplayedMessage(message, MessageRecipientsTypes.Students)));
            }
            if (ConnectedPerson.isParent)
            {
                // Gather messages for the classes of the user's children (but not direct messages to them - keeping those private)
                schoolMessages.AsEnumerable().Where(message => ConnectedPerson.ChildrenStudents.Any(childStudent => 
                                                                                      childStudent.classID == message.recipientClassID))
                    .ToList().ForEach(message => Messages.Add(ModelMessageToDisplayedMessage(message, MessageRecipientsTypes.Class)));
            }
            if (ConnectedPerson.isTeacher)
            {
                // Gather messages for all teachers
                schoolMessages.Where(message => message.forAllTeachers).ToList()
                    .ForEach(message => Messages.Add(ModelMessageToDisplayedMessage(message, MessageRecipientsTypes.Teachers)));

                if (ConnectedPerson.Teacher.classID != null)
                {
                    // User is an homeroom teacher - gather messages for his class
                    schoolMessages.Where(message => ConnectedPerson.Teacher.classID == message.recipientClassID).ToList()
                        .ForEach(message => Messages.Add(ModelMessageToDisplayedMessage(message, MessageRecipientsTypes.Class)));
                }
            }
            if (ConnectedPerson.isSecretary || ConnectedPerson.isPrincipal)
            {
                // Gather messages for management
                schoolMessages.Where(message => message.forAllManagement).ToList()
                    .ForEach(message => Messages.Add(ModelMessageToDisplayedMessage(message, MessageRecipientsTypes.Management)));
            }

            // Order messages by date (latest first)
            Messages = Messages.OrderByDescending(message => message.MessageDateTime).ToList();
        }

        /// <summary>
        /// Converts the Model's Message object in a local DisplayedMessage object
        /// </summary>
        /// <param name="message">The message to convert</param>
        /// <param name="messageType">The message to convert</param>
        /// <returns>DisplayedMessage version of the message</returns>
        private DisplayedMessage ModelMessageToDisplayedMessage(Message message, MessageRecipientsTypes messageType)
        {
            // Make sure input is correct
            if (message == null)
            {
                throw new ArgumentNullException("Arguement 'message' cannot be null!");
            }

            DisplayedMessage displayedMessage = new DisplayedMessage();

            // Get the name of the sender (unless it is an automatic message)
            if (message.senderID != null)
            {
                displayedMessage.SenderName = message.SenderPerson.firstName + " " + message.SenderPerson.lastName;
            }
            else
            {
                displayedMessage.SenderName = "הודעה אוטומטית";
            }

            displayedMessage.RecipientName = GetRecipientName(message, messageType);
            displayedMessage.Title = message.title;
            displayedMessage.MessageDateTime = message.date;
            displayedMessage.Details = message.data;

            return displayedMessage;
        }
        
        /// <summary>
        /// Assistant method that creates the recipient name for a message depending on the message type
        /// </summary>
        /// <param name="message">The source message</param>
        /// <param name="messageType">The type of recipient (class, everyone, specific person...)</param>
        /// <returns>The fitting recipient name</returns>
        private static string GetRecipientName(Message message, MessageRecipientsTypes messageType)
        {
            string recipientName;
            switch (messageType)
            {
                case MessageRecipientsTypes.Class:
                    recipientName = "כיתה " + message.Class.className;
                    break;
                case MessageRecipientsTypes.Everyone:
                    recipientName = "הודעה כללית";
                    break;
                case MessageRecipientsTypes.Management:
                    recipientName = "הנהלה";
                    break;
                case MessageRecipientsTypes.Students:
                    recipientName = "תלמידים";
                    break;
                case MessageRecipientsTypes.Teachers:
                    recipientName = "מורים";
                    break;
                case MessageRecipientsTypes.Person:
                default:
                    recipientName = message.ReceiverPerson.firstName + " " + message.ReceiverPerson.lastName;
                    break;
            }

            return recipientName;
        }
        #endregion
    }
}

namespace Soundbite.Messaging
{
    /// <summary>
    /// Enum of the types of notifications that are supported by <see cref="SbNotificationJob"/> and its related handlers
    /// </summary>
    public enum SbNotificationJobType
    {
        Unknown = 0,

        // INotificationService
        UserWelcome = 10,
        UserInvite = 20,
        PersonInvite = 30,
        MemberInvite = 40,

        // ISbNotificationService
        SessionReminder = 50,
        SessionPublish = 60,
        SessionHostPublish = 70
    }
}

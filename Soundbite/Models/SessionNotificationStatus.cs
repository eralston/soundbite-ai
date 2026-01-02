namespace Soundbite
{
    /// <summary>
    /// Strongly-typed representation of the JSON found in the <see cref="SessionNotificationEntity.StatusJson"/>
    /// field of the <see cref="SessionNotificationEntity"/> entity and related models.
    /// </summary>
    public class SessionNotificationStatus
    {
        public ExceptionInfo LastException { get; set; }
        public bool Throttled { get; set; }
        public bool UserNotFound { get; set; }
        public bool UsedAltUpn { get; set; }
        public string Info { get; set; }
    }
}
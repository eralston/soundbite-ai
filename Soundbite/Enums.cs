namespace Soundbite
{
    /// <summary>
    /// A sparse enumeration of the various types of <see cref="SessionNotificationEntity"/>
    /// </summary>
    public enum SessionNotificationType
    {
        Unknown = 0,
        Reminder = 10,
        Publish = 20,
        HostPublish = 30
    }

    /// <summary>
    /// An aspect of the event type indicating the channel for notifications
    /// </summary>
    public enum NotificationChannel
    {
        None = 0,
        Email = 10,
        SMS = 20,
        Teams = 30,
    }

    /// <summary>
    /// Sparse enum on the lifecycle of the given notification
    /// </summary>
    public enum NotificationStatus
    {
        Unknown = 0,
        Success = 10,
        Failed = 20,
        Pending = 30,
        Processing = 40,
        Retry = 50
    }

    /// <summary>
    /// The types of sessions available in the system
    /// </summary>
    public enum SessionType
    {
        /// <summary>
        /// A recorded clip that is only available to the owner
        /// </summary>
        Memo,           // 1

        /// <summary>
        /// A stream of clips shared between two (or more) users like a chat
        /// </summary>
        Message,        // 1-to-1

        /// <summary>
        /// A collection of Prompts, each with a clip, sent by one person to a group of people
        /// </summary>
        Announcement,   // 1-to-m

        /// <summary>
        /// A collection of Prompts that gather clips from one person to a group of people
        /// </summary>
        Survey,         // m-to-1

        /// <summary>
        /// A collection of Prompts that gather clips for each prompt from a group of people, then shares the responses of the other participants to each person
        /// </summary>
        Meeting         // m-to-m
    }

    /// <summary>
    /// The role of the clip with respect to the Session, telling the back-end how it should be used given the Session type's workflow
    /// </summary>
    public enum ClipType
    {
        /// <summary>
        /// A clip provided by a Host that initializes the state of a Prompt attached to the Session
        /// </summary>
        Prompt = 0,

        /// <summary>
        /// A clip provided by a participant that furthers the life-cycle of a Session
        /// </summary>
        Contribution = 1,

        /// <summary>
        /// A clip attached to the Prompt that does not further the lifecycle, but is available to other participants as content
        /// </summary>
        Comment = 2,

        /// <summary>
        /// A file that was submitted for server-side processing and not available as content.        
        /// </summary>
        RawClip = 3,

        /// <summary>
        /// A file containing the raw transcript data for a clip.
        /// </summary>
        RawTranscript = 4,
    }

    /// <summary>
    /// Enumeration of the various ways a 
    /// </summary>
    public enum ClipHostingType
    {
        /// <summary>
        /// Denotes an unknown hosting type or a clip file that requires no hosting.
        /// </summary>
        None = 0,

        /// <summary>
        /// Denotes that the clip file is hosted by azure storage.
        /// </summary>
        AzureStorage = 10,

        /// <summary>
        /// Denots that the clip is hosted as a streaming asset in azure media services.
        /// </summary>
        AzureStreaming = 20,

        /// <summary>
        /// Denotes that the clip is hosted as streaming asset in azure without media services.
        /// </summary>
        SbStreaming = 30
    }

    /// <summary>
    /// The role of a Participant with regard to a Session
    /// </summary>
    public enum ParticipantRole
    {
        /// <summary>
        /// The default zero value indicating the record was created, but the role was not defined
        /// </summary>
        Unknown = 0,        // Value set incorrectly by BL

        /// <summary>
        /// A read-only participants in the Session, only able to listen - may be able to Comment
        /// </summary>
        Audience = 25,      // Read-only

        /// <summary>
        /// A read-write participant in the Session, able to listen, Contribute, and Comment
        /// </summary>
        Participant = 50,   // Read-Write

        /// <summary>
        /// The owner role for the Session, able to create, edit, listen, Contibute, and Comment
        /// </summary>
        Host = 100,         // Admin
    }

    /// <summary>
    /// The reaction of a Participant to a session
    /// </summary>
    public enum ParticipantReactionType
    {
        /// <summary>
        /// No indicate reaction on the participant yet
        /// </summary>
        None = 0,

        /// <summary>
        /// Thumbs up
        /// </summary>
        Like = 10,

        /// <summary>
        /// Heart
        /// </summary>
        Love = 20,

        /// <summary>
        /// Haha
        /// </summary>
        Laugh = 30,

        /// <summary>
        /// Holy sh*t
        /// </summary>
        Wow = 40,

        /// <summary>
        /// Too bad
        /// </summary>
        Sad = 50
    }

    /// <summary>
    /// The workflow states of a Participant in regards to a Session
    /// </summary>
    public enum ParticipantState
    {
        /// <summary>
        /// The record was created, but workflow state not yet set
        /// </summary>
        Unknown = 0,                // Unset state

        /// <summary>
        /// The workflow is waiting for some timed event to comments
        /// </summary>
        Pending = 1,                // While the the meeting is still waiting on the future

        /// <summary>
        /// The workflow wants a participant to record a Clip
        /// </summary>
        ContributionRequested = 2,  // While the meeting has passed Reminder

        /// <summary>
        /// The participant has successfully recorded a Clip
        /// </summary>
        Contributed = 3,            // After contribution, before Deadline

        /// <summary>
        /// The workflow wants the participant to listen to the Session
        /// </summary>
        ConsumptionRequested = 4,   // After deadline, before consumption

        /// <summary>
        /// The participant was successfully Acknowledged listening to the Session
        /// </summary>
        Consumed = 5,               // Fully up-to-date on session

        /// <summary>
        /// The participant has "left" the session and will no longer receive workflow prompts, but they will still see it in the user-interface
        /// </summary>
        Unsubscribed = 6            // Human has "left" the session and will no longer receive status
    }

    /// <summary>
    /// Enumeration of the various transcript states.
    /// </summary>
    public enum TranscriptState
    {
        /// <summary>
        /// Denotes that a transcript was not requested and is not available.
        /// </summary>
        None = 0,

        /// <summary>
        /// Denotes that the transcript was requested but is not yet avialable.
        /// </summary>
        Requested = 1,

        /// <summary>
        /// Denotes that the transcript is available.
        /// </summary>
        Available = 2,

        /// <summary>
        /// Denotes that a transcript was requested but processing failed.
        /// </summary>
        Failed = 3
    }

    /// <summary>
    /// Enumeration of the various media processing states.
    /// </summary>
    public enum MediaProcessingState
    {
        /// <summary>
        /// Denotes that no media processing was requested.
        /// </summary>
        None = 0,

        /// <summary>
        /// Denotes that media processing was requested but is not yet avialable.
        /// </summary>
        Requested = 10,

        /// <summary>
        /// Denotes that media processing is complete.
        /// </summary>
        Complete = 20,

        /// <summary>
        /// Denotes that media processing failed.
        /// </summary>
        Failed = 30
    }

    /// <summary>
    /// Enumeration of the various clip processing operation
    /// </summary>
    public enum ClipOperationType
    {
        Unknown = 0,
        AzureMediaSvcEncoding = 100,
        AzureMediaSvcTranscribe = 200,
        SoundbiteMediaEncoding = 300
    }

    /// <summary>
    /// Enumeration of the various clip operation states
    /// </summary>
    public enum ClipOperationStateType
    {
        Unknown = 0,
        NotRequested = 100,
        Requested = 200,
        Error = 600,
        Complete = 1000
    }
}
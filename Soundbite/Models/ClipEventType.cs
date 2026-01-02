namespace Soundbite.Models
{
    /// <summary>
    /// The various types of <see cref="ClipEventEntity"/> event types
    /// </summary>
    /// <remarks>
    /// A "chunk" is a prospective concept that clips may not arrive in a single request, but most of the time that will be the entire file
    /// </remarks>
    public enum ClipEventType
    {
        // WARNING: After first release to Preview, always add new types with a unique number and do NOT change or reuse them
        // As if October 2021, is a total list of brainstormed ideas for events, not a complete TODO list of any kind

        Unknown = 0,

        // Server
        // Reported within the platform itself and 100% trustworthy

        /// <summary>
        /// The clip successfully arrived on the server
        /// </summary>
        ServerUpload = 100,

        /// <summary>
        /// Generating secure access to write a clip. This can then be cached, so it is NOT directly analogous to each use
        /// </summary>
        ServerUnlockForWrite = 200,

        /// <summary>
        /// Generating secure access to read a clip. This can then be cached, so it is NOT directly analogous to each use
        /// </summary>
        ServerUnlockForRead = 300,

        /// <summary>
        /// When the server responds with a secure link to the location of a clip with a time relevant to the duration of the chunk of the clip
        /// </summary>
        ServerShareLinkSecure = 400,

        /// <summary>
        /// When the server responds with a link to the *public* location of a clip with a time relevant to the duration of the chunk of the clip
        /// </summary>
        ServerShareLinkPublic = 500,

        // Events that are self-reported by client
        // They are informative, but not entirely trustworthy
        // This means they are helpful for expressing audience activity, but NOT billing

        /// <summary>
        /// Event for when a client records a clip, which may have happened more than once for a clip, expressing the duration of each attempt
        /// </summary>
        ClientRecord = 500,

        /// <summary>
        /// The client loaded at least one part of the clip into a player, the time being the duration of the chunk
        /// </summary>
        ClientLoad = 600,

        /// <summary>
        /// The client started playing the clip (from the start, from a pause, etc) starting from the given time
        /// </summary>
        ClientStart = 700,

        /// <summary>
        /// The client paused playback with the current timestamp as the time
        /// </summary>
        ClientPause = 800,

        /// <summary>
        /// The client scrubbed the clip with a final position given as time
        /// </summary>
        ClientScrub = 900,

        /// <summary>
        /// The client completes the clip to the end
        /// </summary>
        ClientFinish = 1000
    }
}

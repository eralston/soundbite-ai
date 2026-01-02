using System;

namespace Masticore
{
    /// <summary>
    /// Wrapper for DateTime that can be shifted during unit tests
    /// </summary>
    public static class Time
    {
        /// <summary>
        /// Used by unit tests to fast-forward/reverse time
        /// </summary>
        public static DateTime? FixedDateTime { get; set; } = null;

        /// <summary>
        /// Return FixedDateTime OR Time.UtcNow if not available 
        /// </summary>
        public static DateTime UtcNow
        {
            get
            {
                if (FixedDateTime.HasValue)
                {
                    return FixedDateTime.Value;
                }

                return DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Resets the FixedDateTime field, which resumes UtcNow returning the real time
        /// </summary>
        public static void Reset()
        {
            FixedDateTime = null;
        }
    }
}

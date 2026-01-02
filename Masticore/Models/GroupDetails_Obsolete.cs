using System;
using System.Collections.Generic;

namespace Masticore.Models
{
    /// <summary>
    /// DO NOT USE! Use <see cref="GroupDetails"/> plus querying collections separately
    /// </summary>
    [Obsolete("Dropped for perf reasons; use GroupDetails instead")]
    public class GroupDetails_Obsolete : GroupDetails
    {
        /// <summary>
        /// DO NOT USE! Dropped from <see cref="GroupDetails"/>
        /// </summary>
        [Obsolete("Dropped for perf reasons; use GroupDetails instead")]
        public IEnumerable<Member> Members { get; set; }
    }
}
using Masticore.Models;
using System.Collections.Generic;

namespace Masticore.Resources
{
    /// <summary>
    /// Contains the fields required to create a new group.
    /// </summary>
    public class NewGroup : Group
    {
        /// <summary>
        /// Gets or sets a list containing the members of the team.
        /// </summary>        
        public IEnumerable<NewMember> Members { get; set; }
    }
}
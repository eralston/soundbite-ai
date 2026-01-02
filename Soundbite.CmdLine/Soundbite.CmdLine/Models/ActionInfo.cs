using System;

namespace Soundbite.CmdLine
{
    /// <summary>
    /// Stores information about <see cref="ICmdLineAction"/> implementations that can be executed.
    /// </summary>
    internal class ActionInfo
    {
        /// <summary>
        /// Gets or sets the display name of the action.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a more detailed description of the action.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the .NET Type information about the action.
        /// </summary>
        public Type TypeRef { get; set; }
    }
}
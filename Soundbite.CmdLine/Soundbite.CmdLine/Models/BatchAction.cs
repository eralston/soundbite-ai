namespace Soundbite.CmdLine
{
    /// <summary>
    /// Defines a single action entry in a <see cref="BatchFile"/>.
    /// </summary>
    internal class BatchAction
    {
        /// <summary>
        /// Gets or sets the name of the action to execute.  This must match the name found in the
        /// action implementation.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets an array containing parameter names/values used to configure the action.
        /// </summary>
        public BatchParameter[] Params { get; set; }
    }
}
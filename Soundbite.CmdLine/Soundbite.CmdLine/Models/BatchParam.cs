namespace Soundbite.CmdLine
{
    /// <summary>
    /// Stores name/value information for action parameters in a batch file.
    /// </summary>
    internal class BatchParameter
    {
        /// <summary>
        /// Gets or sets the name of the parameter.  This value must match the 
        /// name of the parameter in an action.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value of the parameter.
        /// </summary>
        public string Value { get; set; }
    }
}
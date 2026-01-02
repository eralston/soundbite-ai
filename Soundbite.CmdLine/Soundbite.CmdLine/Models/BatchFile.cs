namespace Soundbite.CmdLine
{
    /// <summary>
    /// Allows for the defintion and configuration of one or more actions to execute.
    /// </summary>
    internal class BatchFile
    {
        /// <summary>
        /// Gets or sets a description name for the batch file.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the actions to execute.
        /// </summary>
        public BatchAction[] Actions { get; set; }
    }
}
namespace Soundbite.CmdLine
{
    /// <summary>
    /// Interface that defines a command line action.  A command line action is intended to be an
    /// operation that can execute unattended given a set of parameters.
    /// </summary>
    public interface ICmdLineAction
    {
        /// <summary>
        /// Gets the parameter definitions for the command line action.
        /// </summary>
        Parameter[] Parameters { get; }

        /// <summary>
        /// Runs the action
        /// </summary>
        void Run();
    }
}

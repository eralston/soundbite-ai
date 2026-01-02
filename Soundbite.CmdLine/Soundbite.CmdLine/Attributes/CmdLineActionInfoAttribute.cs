using System;

namespace Soundbite.CmdLine
{
    /// <summary>
    /// Attribute used to identify and describe command line actions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class CmdLineActionInfoAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public CmdLineActionInfoAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }

}
using System;

namespace Masticore
{

    /// <summary>
    /// Attribute that marks a class for inclusion in code generation. Can also be used to exclude a
    /// class from code generation by setting the <see cref="CodeGenMethodAttribute.Ignore"/> 
    /// property to <c>false</c>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class CodeGenModelAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a value indicating whether to exclude the class from code generation.
        /// </summary>
        public bool Ignore { get; set; } = false;

        /// <summary>
        /// Gets or sets the name of the model. If not set, then the name of the class/interface is used instead
        /// </summary>
        public string Name { get; set; }
    }
}
using System;

namespace Masticore
{
    /// <summary>
    /// Attribute that informs the code generation mechanism to effectively switch out references 
    /// for the class for the interface instead.  The idea here is that the class is a known 
    /// concrete implementation of the interface, and we want to refer to things in JavaScript with 
    /// interfaces anyway, so we don't want to double-over the number of objects in the client.  
    /// 
    /// The reason this exists is because JavaScript works well with interfaces but .NET
    /// controllers require extra configuration to convert an interface into an implementation
    /// when accepting an interface as a parameter.  Implementing concrete classes left us
    /// with a doubling of objects (the concrete ones and the interfaces) which really made
    /// the client-side code messy (although it worked for the most part).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CodeGenTransform : Attribute
    {
        #region Properties

        /// <summary>
        /// Gets or sets the target interface that the attributed type should be converted 
        /// to during code generation.
        /// </summary>        
        public Type TargetType { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenTransform"/> class.
        /// </summary>
        /// <param name="targetType">Target type into which the attributed type should be converted.</param>
        public CodeGenTransform(Type targetType)
        {
            TargetType = targetType;
        }

        #endregion
    }
}
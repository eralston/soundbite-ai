using System;

namespace Soundbite.CmdLine
{
    /// <summary>
    /// Represents a single parameter of an action. 
    /// </summary>
    public abstract class Parameter
    {
        #region Properties

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description of the property.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the .NET type of the property.
        /// </summary>
        public abstract Type ValueType { get; }

        #endregion


    }

    /// <summary>
    /// Typed parameter implementation.
    /// </summary>
    /// <typeparam name="T">.NET Type of the parameter value.</typeparam>
    public class Parameter<T> : Parameter
    {
        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="Parameter{T}"/> instance.
        /// </summary>
        /// <param name="name">Name of the parameter.</param>
        /// <param name="description">Detailed description of the parameter.</param>
        public Parameter(string name, string description = null)
        {
            Name = name;
            Description = description;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public override Type ValueType => typeof(T);

        /// <summary>
        /// Gets or sets the typed value of the parameter.
        /// </summary>
        public T Value { get; set; }

        #endregion
    }
}

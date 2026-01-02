using System;

namespace Masticore
{
    /// <summary>
    /// Attribute that marks a Property or Parameter
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class CodeGenFieldAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a value indicating whether the property allows <c>null</c> values.
        /// </summary>
        public bool IsNullable { get; set; }

        /// <summary>
        /// Gets or sets the output name for this attribute
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets if the given property is server-side only (will be ignored during generation)
        /// This is synonymous with the Newtonsoft.Json.JsonIgnore attribute, but with regard to model generation
        /// </summary>
        public bool Ignore { get; set; } = false;

        /// <summary>
        /// Gets or sets the overriding type name for this field. CodeGen will otherwise use reflection to determine its own type name
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenFieldAttribute"/> class.
        /// </summary>
        public CodeGenFieldAttribute()
        {
        }
    }
}
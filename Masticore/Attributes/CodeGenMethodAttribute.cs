using System;

namespace Masticore
{
    /// <summary>
    /// Enumeration of the different code generation method scenarios.  Some methods require 
    /// special processing that cannot be identified by the signature alone.  This enumeration 
    /// identifies those types of scenarios so they can be processed accordingly.
    /// </summary>
    public enum CodeGenScenarioType
    {
        /// <summary>
        /// Flag indicating that the method should be processed normally.
        /// </summary>
        Normal = 0,

        /// <summary>
        /// Flag indicating that the method expects a single file to be uploaded from form data.        
        /// </summary>
        FileUploadSingleFromFormData = 10,

        /// <summary>
        /// Flag indicating that the method expects a single file to be uploaded as a blob.
        /// </summary>
        FileUploadSingleFromBlob = 20,
    }

    /// <summary>
    /// Attribute that describes code generation properties for the class / method / property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CodeGenMethodAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the return type of a method when it does not match the return type in the back-end code.
        /// </summary>
        public string ClientScriptReturnType { get; set; }

        /// <summary>
        /// Gets or sets a flag identifying the code generation scenario for the method.  Some 
        /// methods require special processing that cannot be identified by the signature alone
        /// and this flag helps identify those scenarios.
        /// </summary>
        public CodeGenScenarioType Scenario { get; set; }

        /// <summary>
        /// Property that can be used to pass scenario-specific information into the scenario.  
        /// This value is scenario-specific so you will need to read about the scenario in the 
        /// <see cref="CodeGenScenarioType"/> documentation to see what scenario-specific info 
        /// can be provided through this property.
        /// </summary>
        public string ScenarioInfo { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to exclude the method from code generation.
        /// </summary>
        public bool Ignore { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the returned value can be null in C# or undefined in Typescript
        /// </summary>
        public bool CanReturnNull { get; set; }

        /// <summary>
        /// Creates a new <see cref="CodeGenMethodAttribute"/> instance.
        /// </summary>
        /// <param name="clientScriptReturnType">Specifies the return type of the method for client libraries.  Leave <c>null</c> or <see cref="string.Empty"/> for auto conversion.</param>
        /// <param name="scenario">Specifies the code generation scenario applied to this method. See <see cref="CodeGenScenarioType"/> for additional information.</param>
        /// <param name="scenarioInfo">Specifies scenario-specific information.</param>
        /// <param name="ignore">Flag indicating whether to exclude the method from code generation.</param>
        public CodeGenMethodAttribute(
            string clientScriptReturnType = null,
            CodeGenScenarioType scenario = CodeGenScenarioType.Normal,
            string scenarioInfo = null,
            bool ignore = false)
        {
            ClientScriptReturnType = clientScriptReturnType;
            Scenario = scenario;
            ScenarioInfo = scenarioInfo;
            Ignore = ignore;
        }
    }
}
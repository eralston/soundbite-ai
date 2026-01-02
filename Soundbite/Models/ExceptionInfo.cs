using Masticore;
using System;

namespace Soundbite
{
    /// <summary>
    /// Represents serializable exception information.
    /// </summary>
    public class ExceptionInfo
    {
        #region Static Methods

        /// <summary>
        /// Intended for capturing exception information in a string format. Attempts to serialize
        /// the exception as a JSON string but in the event serialization fails, this method creates
        /// a new <see cref="ExceptionInfo"/> from the <paramref name="ex"/> which should always
        /// serialize without issue.
        /// </summary>
        /// <param name="ex">Exception to serialize.</param>
        /// <returns>a string containing error information about the exception.</returns>
        public static string SerializeExOrExInfo(Exception ex)
        {
            try
            {
                // There may be some scenarios where this does not work very well.
                return ex.ToLowerCamelJson();
            }
            catch
            {
                // This should always work
                return new ExceptionInfo(ex).ToLowerCamelJson();
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="ExceptionInfo"/> instance.
        /// </summary>
        /// <param name="ex">Exception from which to build exception information.</param>
        public ExceptionInfo(Exception ex)
        {
            Message = ex.Message;
            Type = ex.GetType().FullName;
            StackTrace = ex.StackTrace;
            if (ex.InnerException != null)
            {
                Inner = new ExceptionInfo(ex.InnerException);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the exception message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the .NET type name of the exception.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the stack trace associated with the exception.
        /// </summary>
        public string StackTrace { get; set; }

        /// <summary>
        /// Gets or sets the inner exception.
        /// </summary>
        public ExceptionInfo Inner { get; set; }

        #endregion
    }
}
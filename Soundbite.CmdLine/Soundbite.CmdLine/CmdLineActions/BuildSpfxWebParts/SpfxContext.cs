using System;
using System.Diagnostics;

namespace Soundbite.CmdLine
{
    /// <summary>
    /// Stores environment specific settings for the SPFX build process.
    /// </summary>
    public class SpfxContext
    {
        #region Classes

        public class WrappedActionContext
        {
            public bool Failed { get; set; } = false;
            public string SuccessMsg = "OK";
            public ConsoleColor SuccessColor = ConsoleColor.Green;
            public string ErrorMsg = "Failed";

        }

        #endregion

        #region Fields
        private int _currentIndent = 0;
        private bool _isNewLine = true;
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the root directory of the solution.
        /// </summary>
        public string RootDirectory { get; set; }

        /// <summary>
        /// Gets or sets the current environment settings.
        /// </summary>
        public SpfxEnv Environment { get; set; }

        public SpfxDirectories Dirs { get; }

        public string StableVersion { get; set; }

        public string AutoVersion { get; set; }

        public bool IsCdnBuild { get; set; }

        public bool IsAutoVersionBuild { get; set; }

        public bool IsNewAutoVersion => StableVersion == AutoVersion;

        /// <summary>
        /// Gets the root URL of the AutoVersion CDN files
        /// </summary>
        public string CdnPathAutoVersion => $"{Environment.CdnBasePath}/autoVersion/v{AutoVersion}";

        /// <summary>
        /// Gets the root URL of the StableVersion CDN files
        /// </summary>
        public string CdnPathStableVersion => $"{Environment.CdnBasePath}/v{StableVersion}";

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="SpfxContext"/> instance.
        /// </summary>
        public SpfxContext()
        {
            Dirs = new SpfxDirectories(this);
        }

        #endregion

        #region Methods

        [DebuggerStepThrough]
        public void Indent()
        {
            _currentIndent += 1;
        }

        [DebuggerStepThrough]
        public void Indent(Action action)
        {
            Indent();
            action?.Invoke();
            UnIndent();
        }

        [DebuggerStepThrough]
        public void UnIndent()
        {
            if (_currentIndent > 0)
            {
                _currentIndent -= 1;
            }
        }

        [DebuggerStepThrough]
        public void Write(string msg, params object[] args)
        {
            if (_isNewLine && _currentIndent > 0)
            {
                Console.Write(new string(' ', 2 * _currentIndent));
            }
            Console.Write(msg, args);
            _isNewLine = false;
        }

        [DebuggerStepThrough]
        public void WriteLine(string msg = null, params object[] args)
        {
            if (msg != null)
            {
                Write(msg, args);
            }

            Console.WriteLine();
            _isNewLine = true;
        }

        [DebuggerStepThrough]
        public void WriteInColor(string msg, ConsoleColor color)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(msg);
            Console.ForegroundColor = originalColor;
        }

        [DebuggerStepThrough]
        public void WriteLineInColor(string msg, ConsoleColor color)
        {
            WriteInColor(msg, color);
            WriteLine();
        }

        [DebuggerStepThrough]
        public void WriteDots()
        {
            int dotCount = 70 - Console.CursorLeft;
            if (dotCount > 0)
            {
                Console.Write(new string('.', dotCount));
            }
        }

        [DebuggerStepThrough]
        public void WrapAction(string title, Action action, string msgOk = "OK", string msgFail = "Failed")
        {
            WrapAction(title, (ex) => { action?.Invoke(); }, msgOk, msgFail);
        }

        public void WrapAction(string title, Action<WrappedActionContext> action, string msgOk = "OK", string msgFail = "Failed")
        {
            WrappedActionContext context = new WrappedActionContext() { ErrorMsg = msgFail, SuccessMsg = msgOk };
            Write(title);
            Write(" ...");
            try
            {
                action?.Invoke(context);
                WriteDots();
                if (!context.Failed)
                {
                    WriteInColor(context.SuccessMsg, ConsoleColor.Green);
                }
                else
                {
                    WriteInColor(context.ErrorMsg, ConsoleColor.Red);
                }

                WriteLine();
            }
            catch
            {
                WriteDots();
                WriteInColor(context.ErrorMsg, ConsoleColor.Red);
                WriteLine();
                throw;
            }
        }

        #endregion
    }
}
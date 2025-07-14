using System;
using System.Runtime.CompilerServices;

#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)

namespace SAPUtils.Utils {
    /// <summary>
    /// This interface provides methods to log messages at various log levels such as Trace, Debug, Info, Warn, Error, and Critical.
    /// It also supports logging exception details when errors or critical failures occur.
    /// </summary>
    public interface ILogger {
        /// <summary>
        /// Logs a trace-level message with optional formatting arguments.
        /// </summary>
        /// <param name="message">The message template to log.</param>
        /// <param name="args">Optional arguments to format into the message.</param>
        void Trace(string message, params object[] args);

        /// <summary>
        /// Logs an object at trace level.
        /// </summary>
        /// <param name="obj">The object to log.</param>
        void TraceObject(object obj,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0
        );

        /// <summary>
        /// Logs a debug-level message with optional formatting arguments.
        /// </summary>
        /// <param name="message">The message template to log.</param>
        /// <param name="args">Optional arguments to format into the message.</param>
        void Debug(string message, params object[] args);

        /// <summary>
        /// Logs an object at debug level.
        /// </summary>
        /// <param name="obj">The object to log.</param>
        void DebugObject(object obj,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0
        );

        /// <summary>
        /// Logs an informational message with optional formatting arguments.
        /// </summary>
        /// <param name="message">The message template to log.</param>
        /// <param name="args">Optional arguments to format into the message.</param>
        void Info(string message, params object[] args);

        /// <summary>
        /// Logs an object at info level.
        /// </summary>
        /// <param name="obj">The object to log.</param>
        void InfoObject(object obj,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0
        );

        /// <summary>
        /// Logs a warning message with optional formatting arguments.
        /// </summary>
        /// <param name="message">The message template to log.</param>
        /// <param name="args">Optional arguments to format into the message.</param>
        void Warning(string message, params object[] args);

        /// <summary>
        /// Logs an object at warning level.
        /// </summary>
        /// <param name="obj">The object to log.</param>
        void WarningObject(object obj,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0
        );

        /// <summary>
        /// Logs an error message with optional formatting arguments.
        /// </summary>
        /// <param name="message">The error message template to log.</param>
        /// <param name="args">Optional arguments to format into the message.</param>
        void Error(string message, params object[] args);

        /// <summary>
        /// Logs an error message and an optional exception.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        /// <param name="ex">An optional exception related to the error.</param>
        void Error(string message, Exception ex = null,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0
        );

        /// <summary>
        /// Logs an exception at error level.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        void Error(Exception ex = null,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0
        );

        /// <summary>
        /// Logs a critical message with optional formatting arguments.
        /// </summary>
        /// <param name="message">The critical message template to log.</param>
        /// <param name="args">Optional arguments to format into the message.</param>
        void Critical(string message, params object[] args);

        /// <summary>
        /// Logs a critical message and an optional exception.
        /// </summary>
        /// <param name="message">The critical message to log.</param>
        /// <param name="ex">An optional exception related to the critical failure.</param>
        void Critical(string message, Exception ex = null,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0
        );

        /// <summary>
        /// Logs an exception at critical level.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        void Critical(Exception ex = null,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0
        );
    }
}
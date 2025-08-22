using System;
using System.Runtime.CompilerServices;

// ReSharper disable UnusedMember.Global

#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)

namespace SAPUtils.Utils {
    /// <summary>
    /// Provides methods for logging messages and exceptions at different severity levels.
    /// <br/>
    /// Log levels:
    /// <list type="bullet">
    ///   <item>
    ///     <term>Trace</term>
    ///     <description>
    ///     The most detailed level, used for low-level debugging and tracing program flow.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term>Debug</term>
    ///     <description>
    ///     For diagnostic information that is useful during development and debugging.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term>Info</term>
    ///     <description>
    ///     General application events that describe normal operations (e.g., service start/stop).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term>Warning</term>
    ///     <description>
    ///     Potential issues or unexpected events that are not necessarily errors, but may require attention.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term>Error</term>
    ///     <description>
    ///     Errors that prevent normal execution of a particular operation. Typically indicates an exception or failure.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term>Critical</term>
    ///     <description>
    ///     Severe errors that require immediate attention, often leading to application or system failure.
    ///     </description>
    ///   </item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// Each level supports logging messages, objects, or exceptions, with optional caller information for context.
    /// </remarks>
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
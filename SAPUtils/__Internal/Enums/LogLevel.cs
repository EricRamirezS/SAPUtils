using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SAPUtils.__Internal.Enums {
    /// <summary>
    /// Defines the severity levels used for logging operations.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LogLevel {
        /// <summary>
        /// Logs that contain the most detailed messages. These messages may contain sensitive application data.
        /// Typically used only for debugging during development.
        /// </summary>
        Trace = 0,

        /// <summary>
        /// Logs that are useful for debugging and have short-term value.
        /// </summary>
        Debug = 1,

        /// <summary>
        /// Logs that track the general flow of the application. These logs should have long-term value.
        /// </summary>
        Info = 2,

        /// <summary>
        /// Logs that highlight an abnormal or unexpected event in the application flow,
        /// but do not otherwise cause the application to stop.
        /// </summary>
        Warning = 3,

        /// <summary>
        /// Logs that highlight when the current flow of execution is stopped due to a failure.
        /// These should indicate a problem that needs to be investigated.
        /// </summary>
        Error = 4,

        /// <summary>
        /// Logs that describe an unrecoverable application or system crash, or a catastrophic failure
        /// that requires immediate attention.
        /// </summary>
        Critical = 5,

        /// <summary>
        /// Disables logging. No messages will be logged.
        /// </summary>
        None = 6,
    }
}
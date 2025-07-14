using System;
using System.Runtime.CompilerServices;
using SAPUtils.Internal.Enums;
using SAPUtils.Utils;

namespace SAPUtils.Internal.Utils {
    internal partial class Logger : ILogger {
        public void Trace(string message, params object[] args) {
            string msg = Format(message, args);
            (string callerName, string callerFile, int callerLine) = GetCallerInfo();
            Log(LogLevel.Trace, msg, null, callerName, callerFile, callerLine);
        }

        public void TraceObject(object obj,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0) =>
            Log(LogLevel.Trace, obj, callerName, callerFile, callerLine);

        public void Debug(string message, params object[] args) {
            string msg = Format(message, args);
            (string callerName, string callerFile, int callerLine) = GetCallerInfo();
            Log(LogLevel.Debug, msg, null, callerName, callerFile, callerLine);
        }

        public void DebugObject(object obj,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0) =>
            Log(LogLevel.Debug, obj, callerName, callerFile, callerLine);

        public void Info(string message, params object[] args) {
            string msg = Format(message, args);
            (string callerName, string callerFile, int callerLine) = GetCallerInfo();
            Log(LogLevel.Info, msg, null, callerName, callerFile, callerLine);
        }

        public void InfoObject(object obj,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0) =>
            Log(LogLevel.Info, obj, callerName, callerFile, callerLine);

        public void Warning(string message, params object[] args) {
            string msg = Format(message, args);
            (string callerName, string callerFile, int callerLine) = GetCallerInfo();
            Log(LogLevel.Warning, msg, null, callerName, callerFile, callerLine);
        }

        public void WarningObject(object obj,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0) =>
            Log(LogLevel.Warning, obj, callerName, callerFile, callerLine);

        public void Error(string message, params object[] args) {
            string msg = Format(message, args);
            (string callerName, string callerFile, int callerLine) = GetCallerInfo();
            Log(LogLevel.Error, msg, null, callerName, callerFile, callerLine);
        }

        public void Error(string message, Exception ex = null,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0) =>
            Log(LogLevel.Error, message, ex, callerName, callerFile, callerLine);

        public void Error(Exception ex = null,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0) {
            string msg = ex?.Message ?? "Unknown error";
            Log(LogLevel.Error, msg, ex, callerName, callerFile, callerLine);
        }

        public void Critical(string message, params object[] args) {
            string msg = Format(message, args);
            (string callerName, string callerFile, int callerLine) = GetCallerInfo();
            Log(LogLevel.Critical, msg, null, callerName, callerFile, callerLine);
        }

        public void Critical(string message, Exception ex = null,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0) =>
            Log(LogLevel.Critical, message, ex, callerName, callerFile, callerLine);

        public void Critical(Exception ex = null,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0) {
            string msg = ex?.Message ?? "Critical error";
            Log(LogLevel.Critical, msg, ex, callerName, callerFile, callerLine);
        }
    }
}
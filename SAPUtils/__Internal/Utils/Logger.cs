using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SAPUtils.__Internal.Enums;
using SAPUtils.Utils;
using Company = SAPbobsCOM.Company;


namespace SAPUtils.__Internal.Utils {
    [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
    internal partial class Logger {
        private const string LogSettingFileName = "LogSettings.json";
        private static ILogger _instace;

        private static readonly object LogLock = new object();
        private readonly Company _company;
        private readonly int _daysToKeep;
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly string _logDirectory;
        private readonly LogInfo _logInfo;
        private readonly bool _logToConsole;
        private readonly LogLevel _minimumLevel;
        private readonly StringifyStrategy _stringifyStrategy;


        private Logger(Company company) {
            _company = company;
            LoggerSettings config = LoadSettings();

            _logDirectory = config.LogDirectoryPath;
            _daysToKeep = config.RetentionDays;
            _minimumLevel = Enum.TryParse(config.LogLevel, true, out LogLevel level) ? level : LogLevel.Info;
            _stringifyStrategy = Enum.TryParse(config.StringifyStrategy, true, out StringifyStrategy stratety) ? stratety : StringifyStrategy.ToString;
            _logToConsole = config.LogToConsole;
            _logInfo = config.LogInfo;

            if (!Directory.Exists(_logDirectory)) {
                Directory.CreateDirectory(_logDirectory);
            }

            CleanOldLogs();

            _jsonSerializerSettings = new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Include,
                Formatting = Formatting.Indented,
            };
        }

        public static ILogger Instance => _instace ?? (_instace = new Logger(SapAddon._company));

        internal static LoggerSettings LoadSettings() {
            if (!File.Exists(LogSettingFileName)) {
                LoggerSettings defaultSettings = new LoggerSettings();
                JObject rootObject = new JObject {
                    ["Logger"] = JObject.FromObject(defaultSettings),
                };
                File.WriteAllText(LogSettingFileName, rootObject.ToString());
                return defaultSettings;
            }
            string json = File.ReadAllText(LogSettingFileName);
            JObject configRoot = JObject.Parse(json);
            return configRoot["Logger"]?.ToObject<LoggerSettings>();
        }

        private string GetLogFilePath() {
            string date = DateTime.Now.ToString("yyyyMMdd");
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _logDirectory,
                $"LOG_{_company.CompanyName}_{_company.UserName}_{date}.log");
        }

        private void Log(
            LogLevel level,
            object obj,
            string callerName = "",
            string callerFile = "",
            int callerLine = 0) {

            if (level < _minimumLevel)
                return;

            string typeName = obj?.GetType().FullName ?? "null";
            string serialized = "null";
            if (obj != null) {
                switch (_stringifyStrategy) {
                    case StringifyStrategy.ToString:
                        serialized = obj.ToString();
                        break;
                    case StringifyStrategy.Json:
                        serialized = JsonConvert.SerializeObject(obj, _jsonSerializerSettings);
                        break;
                    case StringifyStrategy.Xml:
                        try {

                            XmlSerializer serializer = new XmlSerializer(obj.GetType());
                            using (StringWriter sw = new StringWriter()) {
                                serializer.Serialize(sw, obj);
                                serialized = sw.ToString();
                            }

                        }
                        catch (Exception ex) {
                            serialized = $"[No serializable a XML] {ex.Message}. ToString(): {obj}";
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            string message = $"[Object: {typeName}]{Environment.NewLine}{serialized}";

            Log(level,
                message,
                null,
                callerName,
                callerFile,
                callerLine);
        }

        private void Log(
            LogLevel level,
            string message,
            Exception ex = null,
            string callerName = "",
            string callerFile = "",
            int callerLine = 0) {
            if (level < _minimumLevel)
                return;

            int threadId = Thread.CurrentThread.ManagedThreadId;
            string timestamp = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            string fileName = Path.GetFileName(callerFile);

            List<string> logEntryInfo = new List<string>();
            if (_logInfo.IncludeTimeStamp) {
                logEntryInfo.Add($"{timestamp}");
            }
            if (_logInfo.IncludeThreadId) {
                logEntryInfo.Add($"[Thread {threadId}]");
            }
            if (_logInfo.IncludeLogLevel) {
                logEntryInfo.Add($"[{level.ToString().ToUpperInvariant()}]");
            }
            if (_logInfo.IncludeCallerFile) {
                logEntryInfo.Add($"{fileName}:{callerLine}");
            }
            if (_logInfo.IncludeCallerMethod) {
                logEntryInfo.Add($"({callerName})");
            }
            string logEntry = message;
            if (logEntryInfo.Any()) {
                logEntry = string.Join(" ", logEntryInfo);
                logEntry = $"{logEntry} - {message}";
            }

            string logFilePath = GetLogFilePath();
            lock (LogLock) {
                try {
                    WriteLogToFile(logFilePath, logEntry, ex);
                    if (!_logToConsole) return;
                    ConsoleLogger.PrintColoredLogEntry(timestamp, threadId, level, message, callerName, fileName, callerLine);
                    if (ex != null) {
                        ConsoleLogger.PrintExceptionToConsole(ex);
                    }
                }
                catch (IOException ioEx) {
                    // Alternativa: intentar escribir en un archivo fallback o reintentar luego
                    Console.WriteLine($"[Logger] Error escribiendo log: {ioEx.Message}");
                }
            }
        }

        private static string ExceptionToString(Exception exception) {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"{exception.GetType().Name}: {exception.Message}");

            if (exception.TargetSite != null) {
                sb.AppendLine(
                    $"Occurred in: {exception.TargetSite.DeclaringType?.FullName}.{exception.TargetSite.Name}");
            }

            StackTrace stackTrace = new StackTrace(exception, true);
            StackFrame frame = stackTrace.GetFrame(0);
            if (frame?.GetFileName() != null) {
                sb.AppendLine($"File: {frame.GetFileName()}");
                sb.AppendLine($"Line: {frame.GetFileLineNumber()}");
            }

            sb.AppendLine("Stack Trace:");
            sb.AppendLine(exception.StackTrace);

            Exception inner = exception.InnerException;
            while (inner != null) {
                sb.AppendLine();
                sb.AppendLine($"Caused by: {inner.GetType().Name}: {inner.Message}");


                if (inner.TargetSite != null) {
                    sb.AppendLine($"Occurred in: {inner.TargetSite.DeclaringType?.FullName}.{inner.TargetSite.Name}");
                }

                StackTrace innerStackTrace = new StackTrace(inner, true);
                StackFrame innerFrame = innerStackTrace.GetFrame(0);
                if (innerFrame?.GetFileName() != null) {
                    sb.AppendLine($"File: {innerFrame.GetFileName()}");
                    sb.AppendLine($"Line: {innerFrame.GetFileLineNumber()}");
                }

                sb.AppendLine("Stack Trace:");
                sb.AppendLine(inner.StackTrace);
                inner = inner.InnerException;
            }

            return sb.ToString();
        }

        private void CleanOldLogs() {
            string[] files = Directory.GetFiles(_logDirectory, "log_*.log");

            foreach (string file in files) {
                DateTime lastWrite = File.GetLastWriteTime(file);
                if (!((DateTime.Now - lastWrite).TotalDays > _daysToKeep)) continue;
                try {
                    File.Delete(file);
                }
                catch (Exception ex) {
                    Console.WriteLine($"No se pudo eliminar el archivo {file}: {ex.Message}");
                }
            }
        }

        private static void WriteLogToFile(string logFilePath, string logEntry, Exception ex = null) {
            using (FileStream stream = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8)) {
                writer.WriteLine(logEntry);
                if (ex != null) {
                    writer.WriteLine(ExceptionToString(ex));
                }
            }
        }

        private static string Format(string message, object[] args) {
            if (args == null || args.Length == 0) return message;
            try {
                return string.Format(message, args);
            }
            catch (FormatException) {
                return message + " [ERROR FORMATEANDO ARGUMENTOS]";
            }
        }

        private static (string callerName, string callerFile, int callerLine) GetCallerInfo(int skipFrames = 2) {
            StackTrace stackTrace = new StackTrace(true);
            StackFrame frame = stackTrace.GetFrame(skipFrames);
            string callerName = frame.GetMethod().Name;
            string callerFile = frame.GetFileName();
            int callerLine = frame.GetFileLineNumber();
            return (callerName, callerFile, callerLine);
        }

        [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        internal class LoggerSettings {
            public string LogDirectoryPath { get; set; } = "logs";
            public int RetentionDays { get; set; } = 7;
            public string LogLevel { get; set; } = "Info";
            public string StringifyStrategy { get; set; } = "ToString";
            public bool LogToConsole { get; set; } = true;
            public LogInfo LogInfo { get; set; } = new LogInfo();
        }

        private enum StringifyStrategy {
            ToString = 0,
            Json = 1,
            Xml = 2,
        }

        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        internal class LogInfo {
            public bool IncludeTimeStamp { get; set; } = true;
            public bool IncludeThreadId { get; set; } = true;
            public bool IncludeLogLevel { get; set; } = true;
            public bool IncludeCallerFile { get; set; } = true;
            public bool IncludeCallerMethod { get; set; } = true;
        }
    }
}
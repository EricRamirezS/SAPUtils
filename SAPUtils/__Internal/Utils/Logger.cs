using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SAPUtils.__Internal.Enums;
using SAPUtils.I18N;
using SAPUtils.Utils;
using Company = SAPbobsCOM.Company;


namespace SAPUtils.__Internal.Utils {
    [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
    internal partial class Logger : IDisposable {
        private const string LogSettingFileName = "LogSettings.json";
        private static ILogger _instance;

        private static readonly BlockingCollection<Func<Task>> Queue = new BlockingCollection<Func<Task>>();

        private static readonly CancellationTokenSource Cts = new CancellationTokenSource();

        private readonly Company _company;
        private readonly int _daysToKeep;
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly string _logDirectory;
        private readonly LogInfo _logInfo;
        private readonly bool _logToConsole;
        private readonly Task _logWorker;
        private readonly LogLevel _minimumLevel;
        private readonly StringifyStrategy _stringifyStrategy;

        private Logger(Company company) {
            _company = company;
            LoggerSettings config = LoadSettings();

            _logWorker = Task.Run(ProcessLogQueueAsync, Cts.Token);


            _logDirectory = config.LogDirectoryPath;
            _daysToKeep = config.RetentionDays;
            _minimumLevel = Enum.TryParse(config.LogLevel, true, out LogLevel level) ? level : LogLevel.Info;
            _stringifyStrategy = Enum.TryParse(config.StringifyStrategy, true, out StringifyStrategy strategy)
                ? strategy
                : StringifyStrategy.ToString;
            _logToConsole = config.LogToConsole;
            _logInfo = config.LogInfo;

            if (!Directory.Exists(_logDirectory)) {
                Directory.CreateDirectory(_logDirectory);
            }

            CleanOldLogs();

            _jsonSerializerSettings = new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Include,
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                DefaultValueHandling = DefaultValueHandling.Include,
                TypeNameHandling = TypeNameHandling.Auto,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                Error = (sender, args) => {
                    Debug(Texts.Logger_Logger_JSON_error___0_, args.ErrorContext.Error.Message);
                    args.ErrorContext.Handled = true;
                },
            };
        }

        public static ILogger Instance => _instance ?? (_instance = new Logger(SapAddon.__Company));

        public void Dispose() {
            Queue?.CompleteAdding();
            Cts.Cancel();
            _logWorker?.Wait();
        }

        private static async Task ProcessLogQueueAsync() {
            foreach (Func<Task> action in Queue.GetConsumingEnumerable()) {
                try {
                    await action();
                }
                catch (Exception ex) {
                    Console.WriteLine(
                        Texts.Logger_ProcessLogQueue__Logger__Error_writing_log___0_,
                        ex.Message
                    );
                }

                await Task.Yield();
            }
        }

        [Localizable(false)]
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

        private void Enqueue(Func<Task> logAction) {
            if (_minimumLevel == LogLevel.None) return;
            if (!Queue.IsAddingCompleted) {
                Queue.Add(logAction);
            }
        }

        [Localizable(false)]
        private string GetLogFilePath() {
            string date = DateTime.Now.ToString("yyyyMMdd");

            string safeCompany = SanitizeFileName(_company?.CompanyName ?? Texts.Logger_GetLogFilePath_Company);
            string safeUser = SanitizeFileName(_company?.UserName ?? Texts.Logger_GetLogFilePath_User);

            string fileName = $"LOG_{safeCompany}_{safeUser}_{date}.log";
            fileName = SanitizeFileName(fileName);

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string logsDir = Path.Combine(baseDir, "Logs");

            string fullPath = Path.Combine(logsDir, fileName);
            if (fullPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0) {
                ConsoleLogger.Error(
                    new Exception(Texts.Logger_GetLogFilePath_The_path_contains_invalid_characters___fullPath_));
            }

            return fullPath;
        }

        private void Log(
            LogLevel level,
            object obj,
            string callerName = "",
            string callerFile = "",
            int callerLine = 0) {
            if (level < _minimumLevel)
                return;

            string typeName = obj?.GetType().FullName ?? Texts.Logger_Log__null;
            string serialized = Texts.Logger_Log__null;
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
                            serialized = string.Format(Texts.Logger_Log__Not_serializable_to_XML___0___ToString_____1_,
                                ex.Message, obj);
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            string message = string.Format(Texts.Logger_Log__Object___0___1__2_, typeName, Environment.NewLine,
                serialized);

            Log(level,
                message,
                null,
                callerName,
                callerFile,
                callerLine);
        }

        [Localizable(false)]
        private void Log(
            LogLevel level,
            string message,
            Exception ex = null,
            string callerName = "",
            string callerFile = "",
            int callerLine = 0) {
            if (level < _minimumLevel) return;
            Enqueue(async () => {
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

                try {
                    await WriteLogToFile(GetLogFilePath(), logEntry, ex);
                    if (!_logToConsole) return;
                    ConsoleLogger.PrintColoredLogEntry(timestamp, threadId, level, message, callerName, fileName,
                        callerLine);
                    if (ex != null) {
                        ConsoleLogger.PrintExceptionToConsole(ex);
                    }
                }
                catch (IOException ioEx) {
                    if (!_logToConsole) return;
                    ConsoleLogger.PrintExceptionToConsole(ioEx);
                }
            });
        }

        private static string ExceptionToString(Exception exception) {
            StringBuilder sb = new StringBuilder();

            // ReSharper disable once LocalizableElement
            sb.AppendLine($"{exception.GetType().Name}: {exception.Message}");

            if (exception.TargetSite != null) {
                sb.AppendLine(
                    string.Format(Texts.Logger_ExceptionToString_Occurred_in___0___1_,
                        exception.TargetSite.DeclaringType?.FullName, exception.TargetSite.Name));
            }

            StackTrace stackTrace = new StackTrace(exception, true);
            StackFrame frame = stackTrace.GetFrame(0);
            if (frame?.GetFileName() != null) {
                sb.AppendLine(string.Format(Texts.Logger_ExceptionToString_File___0_, frame.GetFileName()));
                sb.AppendLine(string.Format(Texts.Logger_ExceptionToString_Line___0_, frame.GetFileLineNumber()));
            }

            sb.AppendLine(Texts.Logger_ExceptionToString_Stack_Trace_);
            sb.AppendLine(exception.StackTrace);

            Exception inner = exception.InnerException;
            while (inner != null) {
                sb.AppendLine();
                sb.AppendLine(string.Format(Texts.Logger_ExceptionToString_Caused_by___0____1_, inner.GetType().Name,
                    inner.Message));


                if (inner.TargetSite != null) {
                    sb.AppendLine(string.Format(Texts.Logger_ExceptionToString_Occurred_in___0___1_,
                        inner.TargetSite.DeclaringType?.FullName, inner.TargetSite.Name));
                }

                StackTrace innerStackTrace = new StackTrace(inner, true);
                StackFrame innerFrame = innerStackTrace.GetFrame(0);
                if (innerFrame?.GetFileName() != null) {
                    sb.AppendLine(string.Format(Texts.Logger_ExceptionToString_File___0_, innerFrame.GetFileName()));
                    sb.AppendLine(string.Format(Texts.Logger_ExceptionToString_Line___0_,
                        innerFrame.GetFileLineNumber()));
                }

                sb.AppendLine(Texts.Logger_ExceptionToString_Stack_Trace_);
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
                    Console.WriteLine(Texts.Logger_CleanOldLogs_Could_not_delete_file__0____1_, file, ex.Message);
                    Error(string.Format(Texts.Logger_CleanOldLogs_Could_not_delete_file__0____1_, file, ex.Message));
                }
            }
        }

        private static async Task WriteLogToFile(string logFilePath, string logEntry, Exception ex = null) {
            using (FileStream stream =
                   new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite, 4096, useAsync: true))
            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8)) {
                await writer.WriteLineAsync(logEntry);
                if (ex != null) {
                    await writer.WriteLineAsync(ExceptionToString(ex));
                }
            }
        }

        private static string Format(string message, object[] args) {
            if (args == null || args.Length == 0) return message;
            try {
                return string.Format(message, args);
            }
            catch (FormatException ex) {
                return string.Format(Texts.Logger_Format__0___ERROR_FORMATTING_ARGUMENTS____1_, message, ex.Message);
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

        private static string SanitizeFileName(string input, char replacementChar = '_') {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            IEnumerable<char> invalidChars =
                Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()).Distinct();

            string sanitized = new string(input.Select(c => invalidChars.Contains(c) ? replacementChar : c).ToArray());
            return sanitized;
        }

        [Localizable(false)]
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
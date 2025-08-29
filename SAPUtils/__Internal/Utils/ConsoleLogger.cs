using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SAPUtils.__Internal.Enums;

// ReSharper disable UnusedMember.Global

namespace SAPUtils.__Internal.Utils {
    [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
    internal class ConsoleLogger : IDisposable {

        private static readonly LogLevel MinimumLevel;
        private static readonly Logger.LogInfo LogInfo;

        private static readonly BlockingCollection<Action> _queue = new BlockingCollection<Action>(new ConcurrentQueue<Action>());
        private static readonly Task _worker;

        static ConsoleLogger() {
            _worker = Task.Run(() => {
                foreach (Action action in _queue.GetConsumingEnumerable()) {
                    action();
                }
            });
            Logger.LoggerSettings config = Logger.LoadSettings();
            LogInfo = config.LogInfo;
            MinimumLevel = Enum.TryParse(config.LogLevel, true, out LogLevel level) ? level : LogLevel.Info;
        }

        public void Dispose() {
            _queue?.CompleteAdding();
            _worker?.Wait();
        }

        private static void Enqueue(Action action) {
            if (MinimumLevel == LogLevel.None) return;
            _queue.Add(action);
        }

        private static void PrintColoredLogEntry(
            LogLevel level,
            string message,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0) {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            string timestamp = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            PrintColoredLogEntry(timestamp, threadId, level, message, callerName, callerFile, callerLine);
        }

        internal static void PrintColoredLogEntry(
            string timestamp,
            int threadId,
            LogLevel level,
            string message,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0) {
            if (MinimumLevel > level) return;
            Enqueue(() => {

                ConsoleColor originalColor = Console.ForegroundColor;

                bool includeSeparator = false;
                if (LogInfo.IncludeTimeStamp) {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(timestamp + " ");
                    includeSeparator = true;
                }
                if (LogInfo.IncludeThreadId) {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write($"[Thread {threadId}] ");
                    includeSeparator = true;
                }

                if (LogInfo.IncludeLogLevel) {
                    ConsoleColor levelColor;
                    switch (level) {
                        case LogLevel.Trace:
                            levelColor = ConsoleColor.Gray;
                            break;
                        case LogLevel.Debug:
                            levelColor = ConsoleColor.Green;
                            break;
                        case LogLevel.Info:
                            levelColor = ConsoleColor.White;
                            break;
                        case LogLevel.Warning:
                            levelColor = ConsoleColor.Yellow;
                            break;
                        case LogLevel.Error:
                            levelColor = ConsoleColor.Red;
                            break;
                        case LogLevel.Critical:
                            levelColor = ConsoleColor.Magenta;
                            break;
                        case LogLevel.None:
                        default:
                            levelColor = ConsoleColor.White;
                            break;
                    }
                    Console.ForegroundColor = levelColor;
                    Console.Write($"[{level.ToString().ToUpperInvariant()}] ");
                    includeSeparator = true;
                }

                if (LogInfo.IncludeCallerFile) {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write($"{Path.GetFileName(callerFile)}:{callerLine} ");
                    includeSeparator = true;
                }

                if (LogInfo.IncludeCallerMethod) {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write($"({callerName})");
                }

                if (includeSeparator) {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write(" - ");
                }

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(message);

                Console.ForegroundColor = originalColor;
            });
        }

        internal static void PrintExceptionToConsole(Exception exception) {
            Enqueue(() => {

                ConsoleColor originalColor = Console.ForegroundColor;
                PrintExceptionBlock(exception);

                Exception inner = exception.InnerException;
                while (inner != null) {
                    PrintExceptionBlock(inner, isInner: true);
                    inner = inner.InnerException;
                }

                Console.ForegroundColor = originalColor;
                return;

                void PrintExceptionBlock(Exception ex, bool isInner = false) {
                    if (isInner) {
                        Console.ForegroundColor = ConsoleColor.DarkMagenta;
                        Console.WriteLine();
                        Console.Write("Caused by: ");
                    }
                    else {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }

                    Console.WriteLine($"{ex.GetType().Name}: {ex.Message}");

                    if (ex.TargetSite != null) {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Occurred in: {ex.TargetSite.DeclaringType?.FullName}.{ex.TargetSite.Name}");
                    }

                    StackTrace stackTrace = new StackTrace(ex, true);
                    StackFrame frame = stackTrace.GetFrame(0);

                    if (frame?.GetFileName() != null) {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"File: {frame.GetFileName()}");
                        Console.WriteLine($"Line: {frame.GetFileLineNumber()}");
                    }

                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("Stack Trace:");
                    Console.WriteLine(ex.StackTrace);
                }
            });
        }

        public static void Trace(
            string message,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0)
            => PrintColoredLogEntry(LogLevel.Trace, message, callerName, callerFile, callerLine);

        public static void Debug(
            string message,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0)
            => PrintColoredLogEntry(LogLevel.Debug, message, callerName, callerFile, callerLine);

        public static void Info(
            string message,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0)
            => PrintColoredLogEntry(LogLevel.Info, message, callerName, callerFile, callerLine);

        public static void Warning(
            string message,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0)
            => PrintColoredLogEntry(LogLevel.Warning, message, callerName, callerFile, callerLine);

        public static void Error(
            string message,
            Exception ex = null,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0) {
            if (MinimumLevel > LogLevel.Error) return;
            PrintColoredLogEntry(LogLevel.Error, message, callerName, callerFile, callerLine);
            if (ex != null) {
                PrintExceptionToConsole(ex);
            }
        }

        public static void Error(Exception ex = null) {
            if (MinimumLevel > LogLevel.Error) return;
            if (ex != null) {
                PrintExceptionToConsole(ex);
            }
        }

        public static void Critical(
            string message,
            Exception ex = null,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0) {
            if (MinimumLevel > LogLevel.Critical) return;
            PrintColoredLogEntry(LogLevel.Critical, message, callerName, callerFile, callerLine);
            if (ex != null) {
                PrintExceptionToConsole(ex);
            }
        }

        public static void Critical(Exception ex = null) {
            if (MinimumLevel > LogLevel.Critical) return;
            if (ex != null) {
                PrintExceptionToConsole(ex);
            }
        }
    }

}
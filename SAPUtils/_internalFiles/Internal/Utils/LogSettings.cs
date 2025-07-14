using System;
using System.IO;
using Newtonsoft.Json;
using SAPUtils.Internal.Enums;

namespace SAPUtils.Internal.Utils
{
    [Serializable]
    internal class LogSettings
    {
        public LogLevel LogLevel { get; set; } = LogLevel.Info;
        public bool LogToConsole { get;  set;} = true;
        public bool LogToFile { get;  set;} = true;
        public string MessageFormat { get;  set;} = "[{DateTime}][{Company}][{User}][{Level}]: {Message}";

        private static LogSettings _instance;

        public static LogSettings Instance
        {
            get
            {
                if (_instance != null) return _instance;

                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogSettings.json");
                string json;
                if (File.Exists(filePath))
                {
                    try
                    {
                        json = File.ReadAllText(filePath);
                        _instance = JsonConvert.DeserializeObject<LogSettings>(json);
                    }
                    catch (Exception)
                    {
                        _instance = new LogSettings();
                    }
                }
                else
                {
                    _instance = new LogSettings();
                }

                try
                {
                    json = JsonConvert.SerializeObject(_instance, Formatting.Indented);
                    File.WriteAllText(filePath, json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving LogSettings file: {ex.Message}");
                }

                return _instance;
            }
        }
    }
}
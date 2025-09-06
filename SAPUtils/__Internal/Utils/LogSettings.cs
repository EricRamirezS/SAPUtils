using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Newtonsoft.Json;
using SAPUtils.__Internal.Enums;
using SAPUtils.I18N;

namespace SAPUtils.__Internal.Utils {
    [Serializable]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    internal class LogSettings {

        private static LogSettings _instance;
        public LogLevel LogLevel { get; set; } = LogLevel.Info;
        public bool LogToConsole { get; set; } = true;
        public bool LogToFile { get; set; } = true;
        // ReSharper disable once LocalizableElement
        public string MessageFormat { get; set; } = "[{DateTime}][{Company}][{User}][{Level}]: {Message}";

        public static LogSettings Instance
        {
            get
            {
                if (_instance != null) return _instance;

                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogSettings.json");
                string json;
                if (File.Exists(filePath)) {
                    try {
                        json = File.ReadAllText(filePath);
                        _instance = JsonConvert.DeserializeObject<LogSettings>(json);
                    }
                    catch (Exception) {
                        _instance = new LogSettings();
                    }
                }
                else {
                    _instance = new LogSettings();
                }

                try {
                    json = JsonConvert.SerializeObject(_instance, Formatting.Indented);
                    File.WriteAllText(filePath, json);
                }
                catch (Exception ex) {
                    Console.WriteLine(Texts.LogSettings_Instance_Error_saving_LogSettings_file___0_, ex.Message);
                }

                return _instance;
            }
        }
    }
}
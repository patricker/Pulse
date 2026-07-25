using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Pulse.Base
{
    public class Log
    {
        public enum LoggerLevels
        {
            Errors = 1,
            Warnings = 10,
            Information = 20,
            Verbose = 30
        }

        public static Log Logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = new Log();
                }

                return _logger;
            }
        }

        private static Log _logger = null;

        public LoggerLevels LoggerLevel { get; set; }

        private string LogFilePath { 
            get {
                return Path.Combine(LogDirectory, string.Format("Pulse_{0}.txt", DateTime.Now.ToString("yyyyMMdd")));
            } 
        }

        private string LogDirectory { 
            get {
                // Use LocalAppData for multi-platform and to avoid Program Files write issues
                string dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Pulse");
                return Path.Combine(dataPath, "Logs");
            } 
        }

        private string CurrentFolder
        {
            get
            {
                // For backward compat, but prefer LocalAppData
                try
                {
                    string dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Pulse");
                    if (Directory.Exists(dataPath)) return dataPath;
                }
                catch { }
                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        private Log() {
            FolderSetup();
            LoggerLevel = LoggerLevels.Warnings;

            //write out Log Startup information

        }

        private void FolderSetup()
        {
            if (!Directory.Exists(LogDirectory))
                Directory.CreateDirectory(LogDirectory);
        }

        public void Write(string message, LoggerLevels level)
        {
            //skip messages that are outside of our logging scope
            if (level > LoggerLevel) return;

            //log the message
            try
            {
                File.AppendAllText(LogFilePath, 
                    string.Format("{0} -- {1} -- {2}{3}", 
                        DateTime.Now.ToString(),
                        level.ToString(),
                        message, Environment.NewLine));
            }
            catch { }
        }
    }
}

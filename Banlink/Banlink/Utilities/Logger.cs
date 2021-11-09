using System;
using System.Drawing;
using System.IO;
using Console = Colorful.Console;

namespace Banlink.Utilities
{
    public static class Logger
    {
        public enum LogLevel
        {
            Info,
            Warn,
            Error,
            Fatal
        }

        public static void Log(LogLevel level, string message)
        {
            switch (level)
            {
                case LogLevel.Info:
                    Console.WriteLine($"[INFO] {message}", Color.LimeGreen);
                    LogToFile($"[INFO] {message}");
                    break;
                case LogLevel.Warn:
                    Console.WriteLine($"[WARN] {message}", Color.FromArgb(243, 229, 0));
                    LogToFile($"[WARN] {message}");
                    break;
                case LogLevel.Error:
                    Console.WriteLine($"[ERROR] {message}", Color.FromArgb(255, 79, 0));
                    LogToFile($"[ERROR] {message}");
                    break;
                case LogLevel.Fatal:
                    Console.WriteLine($"[FATAL] {message}", Color.FromArgb(166, 0, 0));
                    LogToFile($"[FATAL] {message}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, "Invalid Log Level?");
            }
        }

        public static void LogToFile(string message)
        {
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "logs")))
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "logs"));

            var file = File.AppendText($"logs\\log-{Banlink.Time}.txt");
            file.WriteLine($"[{DateTime.Now}] {message}");
            file.Close();
        }
    }
}
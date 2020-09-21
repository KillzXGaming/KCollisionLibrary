using System;
using System.Collections.Generic;
using System.Text;

namespace KclLibrary
{
    /// <summary>
    /// Keeps track of debug information being printed.
    /// </summary>
    public class DebugLogger
    {
        private static string Value;

        /// <summary>
        /// Runs an event when the log information is updated.
        /// </summary>
        public static EventHandler OnDebuggerUpdated;

        /// <summary>
        /// Runs an event when the progress has been updated with a precent value passed.
        /// Used when a model is replaced to keep track of progress bar information.
        /// </summary>
        public static EventHandler OnProgressUpdated;

        public static bool IsCurrentError = false;

        public string GetLog() {
            return Value;
        }

        /// <summary>
        /// Writes a string of text to the logger.
        /// </summary>
        public static void WriteLine(string value)
        {
            Value += $"{value}\n";
            Console.WriteLine($"DebugLogger {value}");
            OnDebuggerUpdated?.Invoke(value, EventArgs.Empty);
        }

        public static void WriteError(string value)
        {
            Value += $"{value}\n";
            Console.WriteLine($"DebugLogger {value}");
            IsCurrentError = true;
            OnDebuggerUpdated?.Invoke(value, EventArgs.Empty);
            IsCurrentError = false;
        }


        /// <summary>
        /// Passes a precent int value over to the progress handler.
        /// </summary>
        public static void UpdateProgress(int value) {
            OnProgressUpdated?.Invoke(value, EventArgs.Empty);
        }
    }
}

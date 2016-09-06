using System;

namespace Warden.Utils
{
    public class ConsoleWardenLogger : IWardenLogger
    {
        private readonly WardenLoggerLevel _minLevel;
        private readonly bool _useColors;

        public ConsoleWardenLogger(WardenLoggerLevel minLevel = WardenLoggerLevel.All, bool useColors = true)
        {
            _minLevel = minLevel;
            _useColors = useColors;
        }

        public void Trace(string message)
        {
            if(DoesNotHaveMinimalLevel(WardenLoggerLevel.Trace))
                return;

            DisplayMessage(WardenLoggerLevel.Trace, message);
        }

        public void Info(string message)
        {
            if (DoesNotHaveMinimalLevel(WardenLoggerLevel.Info))
                return;

            DisplayMessage(WardenLoggerLevel.Info, message);
        }

        public void Error(string message, Exception exception = null)
        {
            if (DoesNotHaveMinimalLevel(WardenLoggerLevel.Error))
                return;

            if (exception != null)
                message += $"\nException: {exception}";

            DisplayMessage(WardenLoggerLevel.Error, message);
        }

        private void DisplayMessage(WardenLoggerLevel level, string message)
        {
            Action logAction = () => Console.WriteLine($"{level.ToString().ToUpperInvariant()}: {message}");
            if (!_useColors)
            {
                logAction();

                return;
            }

            var foregroundColor = ConsoleColor.White;
            switch (level)
            {
                case WardenLoggerLevel.Trace:
                    foregroundColor = ConsoleColor.Yellow;
                    break;
                case WardenLoggerLevel.Info:
                    foregroundColor = ConsoleColor.Green;
                    break;
                case WardenLoggerLevel.Error:
                    foregroundColor = ConsoleColor.Red;
                    break;
            }
            Console.ForegroundColor = foregroundColor;
            logAction();
            Console.ForegroundColor = ConsoleColor.White;
        }

        private bool DoesNotHaveMinimalLevel(WardenLoggerLevel level)
        {
            return _minLevel < level;
        }
    }
}
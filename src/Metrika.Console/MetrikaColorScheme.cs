using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrika.Console
{
    /// <summary>
    /// Performance measurement color configuration for console output
    /// </summary>
    public class MetrikaColorScheme
    {
        /// <summary>
        /// Color for fast operations (default: Green, &lt;500ms)
        /// </summary>
        public ConsoleColor FastColor { get; set; } = ConsoleColor.Green;

        /// <summary>
        /// Color for normal operations (default: Blue, 500-1000ms)
        /// </summary>
        public ConsoleColor NormalColor { get; set; } = ConsoleColor.Blue;

        /// <summary>
        /// Color for slow operations (default: Yellow, &gt;1000ms)
        /// </summary>
        public ConsoleColor SlowColor { get; set; } = ConsoleColor.Yellow;

        /// <summary>
        /// Color for threshold exceeded (default: Red)
        /// </summary>
        public ConsoleColor ThresholdExceededColor { get; set; } = ConsoleColor.Red;

        /// <summary>
        /// Default color scheme (Green/Blue/Yellow/Red)
        /// </summary>
        public static MetrikaColorScheme Default => new();

        /// <summary>
        /// Pastel color scheme (Cyan/Magenta/DarkYellow/DarkRed)
        /// </summary>
        public static MetrikaColorScheme Pastel => new()
        {
            FastColor = ConsoleColor.Cyan,
            NormalColor = ConsoleColor.Magenta,
            SlowColor = ConsoleColor.DarkYellow,
            ThresholdExceededColor = ConsoleColor.DarkRed
        };

        /// <summary>
        /// Monochrome color scheme (Gray/White/DarkGray/White)
        /// </summary>
        public static MetrikaColorScheme Monochrome => new()
        {
            FastColor = ConsoleColor.Gray,
            NormalColor = ConsoleColor.White,
            SlowColor = ConsoleColor.DarkGray,
            ThresholdExceededColor = ConsoleColor.White
        };

        /// <summary>
        /// Dark theme color scheme
        /// </summary>
        public static MetrikaColorScheme Dark => new()
        {
            FastColor = ConsoleColor.DarkGreen,
            NormalColor = ConsoleColor.DarkCyan,
            SlowColor = ConsoleColor.DarkYellow,
            ThresholdExceededColor = ConsoleColor.DarkRed
        };
    }
}

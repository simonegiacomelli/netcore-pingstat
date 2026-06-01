using System;
using System.Globalization;

namespace PingStat
{
    // Formats an elapsed up/down span the way the human-facing log shows it:
    // the most significant non-zero unit is unpadded, every lesser unit keeps
    // its two-digit zero padding. Leading zero units are dropped entirely.
    //   2s  ->        "2s"
    //   1m03s ->   "1m03s"
    //   7h08m21s -> "7h08m21s"
    // Padding the result to ColumnWidth right-aligns the column so larger
    // magnitudes visibly shift left, making severity readable at a glance.
    internal static class DurationFormatter
    {
        // Width of the widest common value, "13h38m52s". PadLeft is a minimum:
        // values with 3+ digit hours simply exceed it without truncation.
        internal const int ColumnWidth = 9;

        internal static string Format(TimeSpan span)
        {
            span = span.Duration();

            var hours = (int)span.TotalHours;   // total, not the 0-23 component
            var minutes = span.Minutes;
            var seconds = span.Seconds;

            if (hours > 0)
                return string.Format(CultureInfo.InvariantCulture, "{0}h{1:00}m{2:00}s", hours, minutes, seconds);

            if (minutes > 0)
                return string.Format(CultureInfo.InvariantCulture, "{0}m{1:00}s", minutes, seconds);

            return string.Format(CultureInfo.InvariantCulture, "{0}s", seconds);
        }

        // Right-aligned for the log column; see ColumnWidth.
        internal static string FormatPadded(TimeSpan span)
        {
            return Format(span).PadLeft(ColumnWidth);
        }
    }
}

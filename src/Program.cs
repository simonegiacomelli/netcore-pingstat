using System;
using System.IO;
using System.Threading;

namespace PingStat
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Using ini: " + PathManager.GetIniFilename());
            var ini = new IniFile(PathManager.GetIniFilename());
            var hosts = new Hosts(ini);
            EventWaitHandle wait = new AutoResetEvent(false);
            var go = true;
            Console.CancelKeyPress += (s, e) =>
            {
                go = false;
                wait.Set();
            };
            log("Program start");
            while (go)
            {
                Doit(hosts);
                var time = ini.ReadIntegerAndForce("main", "PollInterval", 1000);
                if (wait.WaitOne(time))
                    break;
            }
        }


        // The two arrows anchor opposite columns so up/down read apart at a glance:
        // UP is flush-left, DOWN sits to the right. Each event's text hugs its own
        // arrow — UP's trails to the right of it, DOWN's is right-aligned before it.
        private const string UpMark = "UP ↑";
        private const string DownMark = "DOWN ↓";
        private const string DownPhrasePrefix = "was up for ";
        private const string UpPhrasePrefix = "was down for ";
        private const int Gap = 5;
        // The DOWN phrase is fixed width (prefix + the padded duration), so its arrow
        // always lands in the same column; UP's text begins just past that arrow.
        private static readonly int DownArrowColumn = Gap + DownPhrasePrefix.Length + DurationFormatter.ColumnWidth + Gap;
        private static readonly int UpInfoColumn = DownArrowColumn + DownMark.Length + 1;

        private static void Doit(Hosts hosts)
        {
            hosts.RefreshPing();
            hosts.WriteVerboseLog();
            if (hosts.OnLineStatusChanged)
            {
                hosts.OnLineStatusClear();
                log(FormatStatusChange(hosts.OnLine, hosts.LastStateSpan));
            }
        }

        internal static string FormatStatusChange(bool online, TimeSpan lastStateSpan)
        {
            var hasSpan = 0 != lastStateSpan.Ticks;

            if (online)
            {
                if (!hasSpan)
                    return UpMark;
                return UpMark.PadRight(UpInfoColumn) + UpPhrasePrefix + DurationFormatter.FormatPadded(lastStateSpan);
            }

            var phrase = hasSpan ? DownPhrasePrefix + DurationFormatter.FormatPadded(lastStateSpan) : "";
            return phrase.PadLeft(DownArrowColumn - Gap) + new string(' ', Gap) + DownMark;
        }

        private static void log(string s)
        {
            var line = DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss fff ") + s + Environment.NewLine;
            Console.Write(line);
            File.AppendAllText("linestatus.log", line);
        }
    }
}
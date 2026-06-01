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


        // Indents DOWN events so up/down read apart at a glance even while scrolling.
        private static readonly string DownIndent = new string(' ', 30);

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
            if (online)
            {
                var str = "UP ↑";
                if (0 != lastStateSpan.Ticks)
                    str += " was down for " + DurationFormatter.FormatPadded(lastStateSpan);
                return str;
            }
            else
            {
                var str = DownIndent + "DOWN ↓";
                if (0 != lastStateSpan.Ticks)
                    str += " was up for " + DurationFormatter.FormatPadded(lastStateSpan);
                return str;
            }
        }

        private static void log(string s)
        {
            var line = DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss fff ") + s + Environment.NewLine;
            Console.Write(line);
            File.AppendAllText("linestatus.log", line);
        }
    }
}
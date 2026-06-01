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


        private static void Doit(Hosts hosts)
        {
            hosts.RefreshPing();
            hosts.WriteVerboseLog();
            if (hosts.OnLineStatusChanged)
            {
                hosts.OnLineStatusClear();
                var str = hosts.OnLine ? "Ping is Up" : "Ping is down";
                if (0 != hosts.LastStateSpan.Ticks)
                    str = str + string.Format(" (was {0} for {1})", hosts.OnLine ? "down" : "up", hosts.LastStateSpan);
                log(str);
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
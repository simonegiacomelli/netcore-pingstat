using System.IO;
using System.Text;

namespace PingStat
{
    // Resolves the ini path (always against the working directory) and the output
    // files that sit beside it. Output names are the fixed "verbose.tsv" /
    // "linestatus.log" with an optional prefix taken from the ini's name:
    //   PingStat-gw1-televes.ini -> PingStat-gw1-televes--verbose.tsv
    // With no ini argument the prefix is empty, so the names stay as they were.
    public static class PathManager
    {
        public static AppPaths Resolve(string[] args)
        {
            if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
            {
                var iniPath = Path.GetFullPath(args[0]);   // against the working dir
                var prefix = Path.Combine(
                    Path.GetDirectoryName(iniPath) ?? string.Empty,
                    Path.GetFileNameWithoutExtension(iniPath) + "--");
                return new AppPaths(iniPath, prefix);
            }

            // No ini given: default beside the working dir, no prefix.
            return new AppPaths(Path.GetFullPath("PingStat.ini"), string.Empty);
        }

        // Hosts a fresh ini is seeded with; also drives the startup message.
        public static readonly string[] DefaultHosts = { "1.1.1.1", "8.8.8.8" };

        public static void CreateDefaultIni(string iniPath)
        {
            var dir = Path.GetDirectoryName(iniPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            var sb = new StringBuilder();
            sb.AppendLine("[main]");
            sb.AppendLine("PollInterval=2000");
            sb.AppendLine();
            sb.AppendLine("[hosts]");
            foreach (var host in DefaultHosts)
                sb.AppendLine(host + "=");

            File.WriteAllText(iniPath, sb.ToString());
        }
    }

    public class AppPaths
    {
        public AppPaths(string iniPath, string prefix)
        {
            IniPath = iniPath;
            VerboseLogPath = prefix + "verbose.tsv";
            LineStatusLogPath = prefix + "linestatus.log";
        }

        public string IniPath { get; }
        public string VerboseLogPath { get; }
        public string LineStatusLogPath { get; }
    }
}

using System.IO;

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

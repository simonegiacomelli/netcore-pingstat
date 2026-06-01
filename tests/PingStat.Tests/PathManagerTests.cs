using System.IO;
using PingStat;
using Xunit;

public class PathManagerTests
{
    [Fact]
    public void Resolve_WithIniArg_PlacesPrefixedOutputsBesideTheIni()
    {
        var ini = Path.Combine(Path.GetTempPath(), "PingStat-gw1-televes.ini");

        var paths = PathManager.Resolve(new[] { ini });

        var iniFull = Path.GetFullPath(ini);
        var stem = iniFull.Substring(0, iniFull.Length - ".ini".Length);   // ".ini" -> "--"

        Assert.Equal(iniFull, paths.IniPath);
        Assert.Equal(stem + "--verbose.tsv", paths.VerboseLogPath);
        Assert.Equal(stem + "--linestatus.log", paths.LineStatusLogPath);
    }

    [Fact]
    public void Resolve_NoArg_UsesLegacyNamesAndDefaultIniInWorkingDir()
    {
        var paths = PathManager.Resolve(new string[0]);

        Assert.Equal("verbose.tsv", paths.VerboseLogPath);
        Assert.Equal("linestatus.log", paths.LineStatusLogPath);
        Assert.True(Path.IsPathRooted(paths.IniPath));
        Assert.EndsWith("PingStat.ini", paths.IniPath);
        Assert.Equal(Path.GetFullPath("PingStat.ini"), paths.IniPath);
    }

    [Fact]
    public void Resolve_BlankArg_FallsBackToTheDefault()
    {
        var paths = PathManager.Resolve(new[] { "   " });

        Assert.Equal("verbose.tsv", paths.VerboseLogPath);
        Assert.Equal(Path.GetFullPath("PingStat.ini"), paths.IniPath);
    }

    [Fact]
    public void Resolve_RelativeArg_ResolvedAgainstWorkingDir()
    {
        var paths = PathManager.Resolve(new[] { Path.Combine("conf", "My.ini") });

        Assert.Equal(Path.GetFullPath(Path.Combine("conf", "My.ini")), paths.IniPath);
        Assert.True(Path.IsPathRooted(paths.VerboseLogPath));
        Assert.EndsWith(Path.Combine("conf", "My--verbose.tsv"), paths.VerboseLogPath);
        Assert.EndsWith(Path.Combine("conf", "My--linestatus.log"), paths.LineStatusLogPath);
    }

    [Fact]
    public void Resolve_PrefixOnlyStripsTheIniExtension()
    {
        // A dotted stem keeps everything but the trailing ".ini".
        var ini = Path.Combine(Path.GetTempPath(), "PingStat.v2.ini");

        var paths = PathManager.Resolve(new[] { ini });

        Assert.EndsWith("PingStat.v2--verbose.tsv", paths.VerboseLogPath);
    }

    [Fact]
    public void DefaultHosts_AreThePublicResolvers()
    {
        Assert.Equal(new[] { "1.1.1.1", "8.8.8.8" }, PathManager.DefaultHosts);
    }

    [Fact]
    public void CreateDefaultIni_WritesAHostsSectionParseableByHosts()
    {
        var path = Path.Combine(Path.GetTempPath(), "PingStat-defaults-test.ini");
        var logPath = Path.Combine(Path.GetTempPath(), "PingStat-defaults-test--verbose.tsv");
        File.Delete(path);
        File.Delete(logPath);
        try
        {
            PathManager.CreateDefaultIni(path);

            Assert.True(File.Exists(path));

            // Round-trips through the real IniFile/Hosts parsing, in declared order.
            var hosts = new Hosts(new IniFile(path)) { VerboseLogPath = logPath };
            hosts.WriteVerboseLog();
            var header = File.ReadAllLines(logPath)[0];
            Assert.Equal("time\t1.1.1.1\t8.8.8.8", header);
        }
        finally { File.Delete(path); File.Delete(logPath); }
    }

    [Fact]
    public void CreateDefaultIni_CreatesMissingParentDirectory()
    {
        var dir = Path.Combine(Path.GetTempPath(), "PingStat-newdir-test");
        var path = Path.Combine(dir, "PingStat.ini");
        if (Directory.Exists(dir))
            Directory.Delete(dir, recursive: true);
        try
        {
            PathManager.CreateDefaultIni(path);

            Assert.True(File.Exists(path));
        }
        finally { if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true); }
    }
}

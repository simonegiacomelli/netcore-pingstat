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
}

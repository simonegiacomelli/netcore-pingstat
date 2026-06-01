using System;
using System.Collections.Generic;
using System.IO;
using PingStat;
using Xunit;

public class HostsTests
{
    private static Host H(string name) => new Host { Name = name };

    private static Hosts Make(params Host[] hosts) => new Hosts(new List<Host>(hosts))
    {
        // Deterministic defaults; individual tests override as needed.
        Now = () => new DateTime(2020, 1, 1),
        PingAction = h => h.PingSuccess = false,
    };

    [Fact]
    public void RefreshPing_FirstCall_AlwaysReportsStatusChanged()
    {
        var hosts = Make(H("a"));
        hosts.PingAction = h => h.PingSuccess = true;

        hosts.RefreshPing();

        Assert.True(hosts.OnLine);
        Assert.True(hosts.OnLineStatusChanged);
    }

    [Fact]
    public void RefreshPing_SameStatus_DoesNotReportStatusChanged()
    {
        var hosts = Make(H("a"));
        hosts.PingAction = h => h.PingSuccess = true;

        hosts.RefreshPing();   // first call -> changed
        hosts.RefreshPing();   // still up

        Assert.True(hosts.OnLine);
        Assert.False(hosts.OnLineStatusChanged);
    }

    [Fact]
    public void RefreshPing_Transition_ReportsChangeWithElapsedSpan()
    {
        var up = true;
        var now = new DateTime(2020, 1, 1, 0, 0, 0);
        var hosts = Make(H("a"));
        hosts.Now = () => now;
        hosts.PingAction = h => h.PingSuccess = up;

        hosts.RefreshPing();          // up at t0
        now = now.AddSeconds(5);
        up = false;
        hosts.RefreshPing();          // down at t0 + 5s

        Assert.False(hosts.OnLine);
        Assert.True(hosts.OnLineStatusChanged);
        Assert.Equal(TimeSpan.FromSeconds(5), hosts.LastStateSpan);
    }

    [Fact]
    public void OnLine_IsTrue_WhenAnyHostSucceeds()
    {
        var hosts = Make(H("a"), H("b"));
        hosts.PingAction = h => h.PingSuccess = h.Name == "b";

        hosts.RefreshPing();

        Assert.True(hosts.OnLine);
    }

    [Fact]
    public void OnLine_IsFalse_WhenAllHostsFail()
    {
        var hosts = Make(H("a"), H("b"));

        hosts.RefreshPing();

        Assert.False(hosts.OnLine);
    }

    [Fact]
    public void OnLineStatusClear_ResetsTheChangedFlag()
    {
        var hosts = Make(H("a"));
        hosts.PingAction = h => h.PingSuccess = true;

        hosts.RefreshPing();
        Assert.True(hosts.OnLineStatusChanged);

        hosts.OnLineStatusClear();
        Assert.False(hosts.OnLineStatusChanged);
    }

    [Fact]
    public void WriteVerboseLog_WritesHeaderAndValues_MinusOneForFailures()
    {
        var path = Path.GetTempFileName();
        File.Delete(path);   // absence triggers the header line
        try
        {
            var hosts = Make(H("a"), H("b"));
            hosts.VerboseLogPath = path;
            hosts.Now = () => new DateTime(2020, 1, 2, 3, 4, 5);
            hosts.PingAction = h =>
            {
                if (h.Name == "a") { h.PingSuccess = true; h.Time = 42; }
                else h.PingSuccess = false;
            };

            hosts.RefreshPing();
            hosts.WriteVerboseLog();

            var lines = File.ReadAllLines(path);
            Assert.Equal("time\ta\tb", lines[0]);
            Assert.Equal("2020-01-02 03:04:05\t42\t-1", lines[1]);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Constructor_FromIni_ParsesNames_PreferringValueThenKey()
    {
        var iniPath = Path.GetTempFileName();
        var logPath = Path.GetTempFileName();
        File.Delete(logPath);
        try
        {
            File.WriteAllText(iniPath,
                "[hosts]\r\n" +
                "google=8.8.8.8\r\n" +   // value used as name
                "fallback=\r\n" +        // empty value -> key used as name
                "named=example.com\r\n");

            var hosts = new Hosts(new IniFile(iniPath)) { VerboseLogPath = logPath };

            hosts.WriteVerboseLog();   // header carries the parsed names; no ping needed

            var header = File.ReadAllLines(logPath)[0];
            Assert.Equal("time\t8.8.8.8\tfallback\texample.com", header);
        }
        finally { File.Delete(iniPath); File.Delete(logPath); }
    }
}

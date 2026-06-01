using System;
using PingStat;
using Xunit;

public class StatusChangeFormatTests
{
    [Fact]
    public void Up_FirstEvent_NoSpan_IsBareArrow()
    {
        Assert.Equal("UP ↑", Program.FormatStatusChange(online: true, TimeSpan.Zero));
    }

    [Fact]
    public void Down_FirstEvent_NoSpan_IsIndentedBareArrow()
    {
        var line = Program.FormatStatusChange(online: false, TimeSpan.Zero);

        Assert.Equal(new string(' ', 30) + "DOWN ↓", line);
    }

    [Fact]
    public void Up_WithSpan_ReportsPaddedDownDuration()
    {
        var line = Program.FormatStatusChange(online: true, new TimeSpan(0, 0, 2));

        Assert.Equal("UP ↑ was down for        2s", line);
    }

    [Fact]
    public void Down_WithSpan_IsIndentedAndReportsPaddedUpDuration()
    {
        var line = Program.FormatStatusChange(online: false, new TimeSpan(7, 8, 21));

        Assert.Equal(new string(' ', 30) + "DOWN ↓ was up for  7h08m21s", line);
    }

    [Fact]
    public void Down_StartsToTheRightOfUp()
    {
        var up = Program.FormatStatusChange(online: true, new TimeSpan(0, 0, 2));
        var down = Program.FormatStatusChange(online: false, new TimeSpan(0, 0, 2));

        Assert.Equal(0, up.IndexOf("UP", StringComparison.Ordinal));
        Assert.True(down.IndexOf("DOWN", StringComparison.Ordinal) > up.IndexOf("UP", StringComparison.Ordinal));
    }
}

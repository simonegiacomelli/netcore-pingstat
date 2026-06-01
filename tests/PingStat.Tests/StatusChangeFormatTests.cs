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
    public void Down_FirstEvent_NoSpan_IsArrowAloneInItsColumn()
    {
        var line = Program.FormatStatusChange(online: false, TimeSpan.Zero);

        // No phrase, so only the right-anchored arrow remains, at its fixed column.
        Assert.Equal(new string(' ', 30) + "DOWN ↓", line);
    }

    [Fact]
    public void Up_WithSpan_ArrowLeft_DownDurationToTheRight()
    {
        var line = Program.FormatStatusChange(online: true, new TimeSpan(0, 0, 16));

        Assert.Equal("UP ↑" + new string(' ', 33) + "was down for       16s", line);
    }

    [Fact]
    public void Down_WithSpan_UpDurationRightAligned_ArrowOnTheRight()
    {
        var line = Program.FormatStatusChange(online: false, new TimeSpan(13, 38, 52));

        Assert.Equal(new string(' ', 5) + "was up for 13h38m52s" + new string(' ', 5) + "DOWN ↓", line);
    }

    [Fact]
    public void Down_ShorterDuration_KeepsArrowInTheSameColumn()
    {
        // The padded duration keeps the phrase a fixed width, so the arrow never moves.
        var longSpan = Program.FormatStatusChange(online: false, new TimeSpan(13, 38, 52));
        var shortSpan = Program.FormatStatusChange(online: false, new TimeSpan(0, 0, 16));

        Assert.Equal(longSpan.IndexOf("DOWN", StringComparison.Ordinal),
                     shortSpan.IndexOf("DOWN", StringComparison.Ordinal));
    }

    [Fact]
    public void UpArrow_LeftOfDownArrow_AndUpInfoRightOfDownArrow()
    {
        var up = Program.FormatStatusChange(online: true, new TimeSpan(0, 0, 16));
        var down = Program.FormatStatusChange(online: false, new TimeSpan(0, 0, 16));

        var upArrow = up.IndexOf("UP", StringComparison.Ordinal);
        var downArrow = down.IndexOf("DOWN", StringComparison.Ordinal);
        var upInfo = up.IndexOf("was down for", StringComparison.Ordinal);

        Assert.True(upArrow < downArrow);   // arrows sit in distinct columns
        Assert.True(upInfo > downArrow);    // UP's text trails past the DOWN arrow column
    }
}

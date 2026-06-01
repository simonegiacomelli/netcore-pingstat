using System;
using PingStat;
using Xunit;

public class DurationFormatterTests
{
    [Theory]
    // The four magnitudes from the approved log mock-up.
    [InlineData(0, 0, 2, "2s")]
    [InlineData(0, 1, 3, "1m03s")]
    [InlineData(7, 8, 21, "7h08m21s")]
    [InlineData(13, 38, 52, "13h38m52s")]
    public void Format_DropsLeadingZeroUnits_KeepsInnerPadding(int h, int m, int s, string expected)
    {
        var span = new TimeSpan(h, m, s);

        Assert.Equal(expected, DurationFormatter.Format(span));
    }

    [Fact]
    public void Format_Zero_IsBareZeroSeconds()
    {
        Assert.Equal("0s", DurationFormatter.Format(TimeSpan.Zero));
    }

    [Fact]
    public void Format_SubSecond_TruncatesToWholeSeconds()
    {
        // 2.99s reports as 2s, matching the original ".1728480"-style truncation.
        var span = TimeSpan.FromMilliseconds(2990);

        Assert.Equal("2s", DurationFormatter.Format(span));
    }

    [Fact]
    public void Format_SecondsOnly_NeverPadsTheLeadingUnit()
    {
        Assert.Equal("9s", DurationFormatter.Format(TimeSpan.FromSeconds(9)));
        Assert.Equal("59s", DurationFormatter.Format(TimeSpan.FromSeconds(59)));
    }

    [Fact]
    public void Format_MinutesPadSecondsButNotMinutes()
    {
        Assert.Equal("9m00s", DurationFormatter.Format(new TimeSpan(0, 9, 0)));
        Assert.Equal("59m05s", DurationFormatter.Format(new TimeSpan(0, 59, 5)));
    }

    [Fact]
    public void Format_HoursAccumulateBeyondADay_NotResetAt24()
    {
        // 1 day + 2h = 26h; hours is the total, never the 0-23 clock component.
        var span = new TimeSpan(1, 2, 3, 4);

        Assert.Equal("26h03m04s", DurationFormatter.Format(span));
    }

    [Fact]
    public void Format_NegativeSpan_IsTreatedAsItsMagnitude()
    {
        Assert.Equal("1m03s", DurationFormatter.Format(new TimeSpan(0, -1, -3)));
    }

    [Theory]
    // Padded to a fixed column so values right-align (widest is "13h38m52s").
    [InlineData(0, 0, 2, "       2s")]
    [InlineData(0, 1, 3, "    1m03s")]
    [InlineData(7, 8, 21, " 7h08m21s")]
    [InlineData(13, 38, 52, "13h38m52s")]
    public void FormatPadded_RightAlignsToColumnWidth(int h, int m, int s, string expected)
    {
        var span = new TimeSpan(h, m, s);

        var padded = DurationFormatter.FormatPadded(span);

        Assert.Equal(expected, padded);
        Assert.Equal(DurationFormatter.ColumnWidth, padded.Length);
    }

    [Fact]
    public void FormatPadded_OversizedValue_ExceedsColumnWithoutTruncation()
    {
        // 100h+ is wider than the column; PadLeft is a floor, not a cap.
        var padded = DurationFormatter.FormatPadded(new TimeSpan(100, 0, 0, 0));

        Assert.Equal("2400h00m00s", padded);
    }
}

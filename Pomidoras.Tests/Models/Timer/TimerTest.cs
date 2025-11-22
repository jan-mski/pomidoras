using FluentAssertions;
using Pomidoras.Models.Timer;
using TimerClass = Pomidoras.Models.Timer.Timer;

namespace Pomidoras.Tests.Models.Timer;

public class TimerTest
{

    [Fact]
    public void Constructor_InitializesProperties_Correctly()
    {
        var timer = TimerMother.Default();

        timer.Mode.Should().Be(TimerMother.DefaultMode);
        timer.Remaining.Should().Be(TimerMother.DefaultDuration);
        timer.Interval.Should().Be(TimerMother.DefaultInterval);
        timer.IsRunning.Should().BeFalse();
    }

    [Fact]
    public void Start_WhenNotRunning_StartsTimerSuccessfully()
    {
        var timer = TimerMother.Default();
        using var monitoredTimer = timer.Monitor();

        timer.Start();

        timer.IsRunning.Should().BeTrue();
        monitoredTimer.OccurredEvents.Should().SatisfyRespectively(e =>
            {
                e.EventName.Should().Be(nameof(timer.IsRunningChanged));
                e.Parameters[1].Should().Be(true);
            }
        );
    }

    [Fact]
    public void Start_WhenRunning_HasNoEffect()
    {
        var timer = TimerMother.Default();
        timer.Start();
        using var monitoredTimer = timer.Monitor();

        timer.Start();

        timer.IsRunning.Should().BeTrue();
        monitoredTimer.OccurredEvents.Should().BeEmpty();
    }

    [Fact]
    public void Stop_WhenRunning_StopsTimerSuccessfully()
    {
        var timer = TimerMother.Default();
        timer.Start();
        using var monitoredTimer = timer.Monitor();

        timer.Stop();

        timer.IsRunning.Should().BeFalse();
        monitoredTimer.OccurredEvents.Should().SatisfyRespectively(
            e =>
            {
                e.EventName.Should().Be(nameof(timer.RemainingChanged));
                e.Parameters[1].Should().Be(TimerMother.DefaultDuration);
            },
            e =>
            {
                e.EventName.Should().Be(nameof(timer.IsRunningChanged));
                e.Parameters[1].Should().Be(false);
            }
        );
    }

    [Fact]
    public void Stop_WhenNotRunning_HasNoEffect()
    {
        var timer = TimerMother.Default();
        using var monitoredTimer = timer.Monitor();

        timer.Stop();
        
        timer.IsRunning.Should().BeFalse();
        monitoredTimer.OccurredEvents.Should().BeEmpty();
    }

    [Fact]
    public void Tick_DecrementsSuccessfully()
    {
        var expectedRemainingValues = new[]
        {
            TimerMother.DefaultDuration - TimerMother.DefaultInterval,
            TimerMother.DefaultDuration - 2 * TimerMother.DefaultInterval
        };

        var timer = TimerMother.Default();
        using var monitoredTimer = timer.Monitor();

        timer.Tick();
        timer.Remaining.Should().Be(expectedRemainingValues[0]);
        timer.Tick();
        timer.Remaining.Should().Be(expectedRemainingValues[1]);

        monitoredTimer.OccurredEvents.Should().SatisfyRespectively(
            e =>
            {
                e.EventName.Should().Be(nameof(timer.RemainingChanged));
                e.Parameters[1].Should().Be(expectedRemainingValues[0]);
            },
            e =>
            {
                e.EventName.Should().Be(nameof(timer.RemainingChanged));
                e.Parameters[1].Should().Be(expectedRemainingValues[1]);
            }
        );
    }

    [Theory]
    [InlineData(1)]
    [InlineData(0.5)]
    [InlineData(0)]
    [InlineData(-1)]
    public void Tick_CompletesSuccessfully(double durationSeconds)
    {
        var duration = TimeSpan.FromSeconds(durationSeconds);
        var timer = TimerMother.WithDuration(duration);
        using var monitoredTimer = timer.Monitor();

        timer.Tick();

        timer.Remaining.Should().Be(TimeSpan.Zero);
        monitoredTimer.OccurredEvents.Should().SatisfyRespectively(
            e => { e.EventName.Should().Be(nameof(timer.RemainingChanged)); },
            e => { e.EventName.Should().Be(nameof(timer.Completed)); }
        );
    }

}
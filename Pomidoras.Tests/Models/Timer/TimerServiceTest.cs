using FluentAssertions;
using Pomidoras.Models.Timer;
using Pomidoras.Models.Timer.Configuration;
using Pomidoras.Tests.Models.Timer.Configuration.Repository;
using Pomidoras.Tests.Models.Timer.Configuration;

namespace Pomidoras.Tests.Models.Timer;

public class TimerServiceTest
{

    private readonly TimerConfigurationRepositoryStub _timerConfigurationRepositoryStub = new();
    private readonly TimerConfigurationService _timerConfigurationService;

    public TimerServiceTest()
    {
        _timerConfigurationRepositoryStub.SaveConfiguration(TimerConfigurationMother.Default());
        _timerConfigurationService = new TimerConfigurationService(_timerConfigurationRepositoryStub);
    }

    [Fact]
    public async Task Start_WhenNotRunning_StartsTimerSuccessfully()
    {
        var duration = TimeSpan.FromMilliseconds(40);
        var interval = TimeSpan.FromMilliseconds(20);

        _timerConfigurationRepositoryStub.SaveConfiguration(
            TimerConfigurationMother.With_WorkDuration_Interval(duration, interval));
        await using var timerService = new TimerService(_timerConfigurationService);
        using var monitoredTimerService = timerService.Monitor();

        timerService.Start();

        await TestUtils.WaitUntilAsync(() => monitoredTimerService.OccurredEvents.Length == 4,
            TimeSpan.FromSeconds(5),
            TimeSpan.FromMilliseconds(20));

        monitoredTimerService.OccurredEvents.Should().SatisfyRespectively(
            e =>
            {
                e.EventName.Should().Be(nameof(timerService.IsRunningChanged));
                e.Parameters[1].Should().Be(true);
            },
            e =>
            {
                e.EventName.Should().Be(nameof(timerService.RemainingChanged));
                e.Parameters[1].Should().Be(TimeSpan.FromMilliseconds(20));
            },
            e =>
            {
                e.EventName.Should().Be(nameof(timerService.RemainingChanged));
                e.Parameters[1].Should().Be(TimeSpan.Zero);
            },
            e =>
            {
                e.EventName.Should().Be(nameof(timerService.IsRunningChanged));
                e.Parameters[1].Should().Be(false);
            }
        );
    }

    [Theory]
    [InlineData(10)]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task Start_WhenDurationLessThanInterval_StartsTimerSuccessfully(double durationMilliseconds)
    {
        var duration = TimeSpan.FromMilliseconds(durationMilliseconds);
        var interval = TimeSpan.FromMilliseconds(20);

        _timerConfigurationRepositoryStub.SaveConfiguration(
            TimerConfigurationMother.With_WorkDuration_Interval(duration, interval));
        await using var timerService = new TimerService(_timerConfigurationService);
        using var monitoredTimerService = timerService.Monitor();

        timerService.Start();

        await TestUtils.WaitUntilAsync(() => monitoredTimerService.OccurredEvents.Length == 3,
            TimeSpan.FromSeconds(5),
            TimeSpan.FromMilliseconds(20));

        monitoredTimerService.OccurredEvents.Should().SatisfyRespectively(
            e =>
            {
                e.EventName.Should().Be(nameof(timerService.IsRunningChanged));
                e.Parameters[1].Should().Be(true);
            },
            e =>
            {
                e.EventName.Should().Be(nameof(timerService.RemainingChanged));
                e.Parameters[1].Should().Be(TimeSpan.Zero);
            },
            e =>
            {
                e.EventName.Should().Be(nameof(timerService.IsRunningChanged));
                e.Parameters[1].Should().Be(false);
            }
        );
    }

    [Fact]
    public async Task Start_WhenRunning_HasNoEffect()
    {
        await using var timerService = new TimerService(_timerConfigurationService);
        timerService.Start();
        using var monitoredTimer = timerService.Monitor();

        timerService.Start();

        timerService.IsRunning.Should().BeTrue();
        monitoredTimer.OccurredEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task Stop_WhenRunning_StopsTimerSuccessfully()
    {
        await using var timerService = new TimerService(_timerConfigurationService);
        timerService.Start();
        using var monitoredTimer = timerService.Monitor();

        timerService.Stop();

        timerService.IsRunning.Should().BeFalse();

        var expectedDuration = TimerConfigurationMother.DefaultWorkDuration;
        timerService.Remaining.Should().Be(expectedDuration);
        monitoredTimer.OccurredEvents.Should().SatisfyRespectively(
            e =>
            {
                e.EventName.Should().Be(nameof(timerService.IsRunningChanged));
                e.Parameters[1].Should().Be(false);
            },
            e =>
            {
                e.EventName.Should().Be(nameof(timerService.RemainingChanged));
                e.Parameters[1].Should().Be(expectedDuration);
            }
        );
    }

    [Fact]
    public async Task Stop_WhenRunning_NoMoreEventsAfter()
    {
        var duration = TimeSpan.FromMilliseconds(80);
        var interval = TimeSpan.FromMilliseconds(20);

        _timerConfigurationRepositoryStub.SaveConfiguration(
            TimerConfigurationMother.With_WorkDuration_Interval(duration, interval));
        await using var timerService = new TimerService(_timerConfigurationService);

        timerService.Start();
        timerService.Stop();
        using var monitoredTimer = timerService.Monitor();
        await Task.Delay(interval * 2, TestContext.Current.CancellationToken);

        monitoredTimer.OccurredEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task Stop_WhenNotRunning_HasNoEffect()
    {
        await using var timerService = new TimerService(_timerConfigurationService);
        using var monitoredTimer = timerService.Monitor();

        timerService.Stop();

        timerService.IsRunning.Should().BeFalse();
        monitoredTimer.OccurredEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task SwitchModeNext_WhenRunning_SwitchesModeSuccessfully()
    {
        await using var timerService = new TimerService(_timerConfigurationService);

        timerService.Start();
        using var monitoredTimer = timerService.Monitor();
        timerService.SwitchModeNext();

        timerService.IsRunning.Should().BeFalse();

        var expectedDuration = TimerConfigurationMother.DefaultBreakShortDuration;
        timerService.Remaining.Should().Be(expectedDuration);
        monitoredTimer.OccurredEvents.Should().SatisfyRespectively(
            e =>
            {
                e.EventName.Should().Be(nameof(timerService.IsRunningChanged));
                e.Parameters[1].Should().Be(false);
            },
            e =>
            {
                e.EventName.Should().Be(nameof(timerService.RemainingChanged));
                e.Parameters[1].Should().Be(expectedDuration);
            },
            e =>
            {
                e.EventName.Should().Be(nameof(timerService.ModeChanged));
                e.Parameters[1].Should().Be(TimerMode.BreakShort);
            }
        );
    }

    [Fact]
    public async Task SwitchModeNext_WhenRunning_NoMoreEventsAfter()
    {
        var duration = TimeSpan.FromMilliseconds(80);
        var interval = TimeSpan.FromMilliseconds(20);

        _timerConfigurationRepositoryStub.SaveConfiguration(
            TimerConfigurationMother.With_WorkDuration_Interval(duration, interval));
        await using var timerService = new TimerService(_timerConfigurationService);

        timerService.Start();
        timerService.SwitchModeNext();
        using var monitoredTimer = timerService.Monitor();

        await Task.Delay(interval * 2, TestContext.Current.CancellationToken);

        monitoredTimer.OccurredEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task SwitchModeNext_WhenNotRunning_SwitchesModeSuccessfully()
    {
        await using var timerService = new TimerService(_timerConfigurationService);

        using var monitoredTimer = timerService.Monitor();
        timerService.SwitchModeNext();

        timerService.IsRunning.Should().BeFalse();

        var expectedDuration = TimerConfigurationMother.DefaultBreakShortDuration;
        timerService.Remaining.Should().Be(expectedDuration);
        monitoredTimer.OccurredEvents.Should().SatisfyRespectively(
            e =>
            {
                e.EventName.Should().Be(nameof(timerService.RemainingChanged));
                e.Parameters[1].Should().Be(expectedDuration);
            },
            e =>
            {
                e.EventName.Should().Be(nameof(timerService.ModeChanged));
                e.Parameters[1].Should().Be(TimerMode.BreakShort);
            }
        );
    }

    [Fact]
    public async Task SwitchModePrevious_WhenRunning_SwitchesModeSuccessfully()
    {
        await using var timerService = new TimerService(_timerConfigurationService);

        timerService.Start();
        using var monitoredTimer = timerService.Monitor();
        timerService.SwitchModePrevious();

        timerService.IsRunning.Should().BeFalse();

        var expectedDuration = TimerConfigurationMother.DefaultBreakLongDuration;
        timerService.Remaining.Should().Be(expectedDuration);
        monitoredTimer.OccurredEvents.Should().SatisfyRespectively(
            e =>
            {
                e.EventName.Should().Be(nameof(timerService.IsRunningChanged));
                e.Parameters[1].Should().Be(false);
            },
            e =>
            {
                e.EventName.Should().Be(nameof(timerService.RemainingChanged));
                e.Parameters[1].Should().Be(expectedDuration);
            },
            e =>
            {
                e.EventName.Should().Be(nameof(timerService.ModeChanged));
                e.Parameters[1].Should().Be(TimerMode.BreakLong);
            }
        );
    }

    [Fact]
    public async Task SwitchModePrevious_WhenRunning_NoMoreEventsAfter()
    {
        var duration = TimeSpan.FromMilliseconds(80);
        var interval = TimeSpan.FromMilliseconds(20);

        _timerConfigurationRepositoryStub.SaveConfiguration(
            TimerConfigurationMother.With_WorkDuration_Interval(duration, interval));
        await using var timerService = new TimerService(_timerConfigurationService);

        timerService.Start();
        timerService.SwitchModePrevious();
        using var monitoredTimer = timerService.Monitor();

        await Task.Delay(interval * 2, TestContext.Current.CancellationToken);

        monitoredTimer.OccurredEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task SwitchModePrevious_WhenNotRunning_SwitchesModeSuccessfully()
    {
        await using var timerService = new TimerService(_timerConfigurationService);

        using var monitoredTimer = timerService.Monitor();
        timerService.SwitchModePrevious();

        timerService.IsRunning.Should().BeFalse();

        var expectedDuration = TimerConfigurationMother.DefaultBreakLongDuration;
        timerService.Remaining.Should().Be(expectedDuration);
        monitoredTimer.OccurredEvents.Should().SatisfyRespectively(
            e =>
            {
                e.EventName.Should().Be(nameof(timerService.RemainingChanged));
                e.Parameters[1].Should().Be(expectedDuration);
            },
            e =>
            {
                e.EventName.Should().Be(nameof(timerService.ModeChanged));
                e.Parameters[1].Should().Be(TimerMode.BreakLong);
            }
        );
    }

    [Fact]
    public async Task Dispose_StopsTimer()
    {
        var duration = TimeSpan.FromMilliseconds(80);
        var interval = TimeSpan.FromMilliseconds(20);

        _timerConfigurationRepositoryStub.SaveConfiguration(
            TimerConfigurationMother.With_WorkDuration_Interval(duration, interval));
        var timerService = new TimerService(_timerConfigurationService);

        timerService.Start();
        timerService.Dispose();
        using var monitoredTimer = timerService.Monitor();
        await Task.Delay(interval * 2, TestContext.Current.CancellationToken);

        monitoredTimer.OccurredEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task DisposeAsync_StopsTimer()
    {
        var duration = TimeSpan.FromMilliseconds(80);
        var interval = TimeSpan.FromMilliseconds(20);

        _timerConfigurationRepositoryStub.SaveConfiguration(
            TimerConfigurationMother.With_WorkDuration_Interval(duration, interval));
        var timerService = new TimerService(_timerConfigurationService);

        timerService.Start();
        await timerService.DisposeAsync();
        using var monitoredTimer = timerService.Monitor();
        await Task.Delay(interval * 2, TestContext.Current.CancellationToken);

        monitoredTimer.OccurredEvents.Should().BeEmpty();
    }

}
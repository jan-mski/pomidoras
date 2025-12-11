using FluentAssertions;
using Pomidoras.Models.Timer;
using Pomidoras.Models.Timer.Configuration;
using Pomidoras.Tests.Models.Timer.Configuration.Repository;
using Pomidoras.Tests.Models.Timer.Configuration;

namespace Pomidoras.Tests.Models.Timer;

public class TimerServiceTest
{

    private readonly TimerConfigurationRepositoryStub _timerConfigurationRepositoryStub;
    private readonly TimerConfigurationService _timerConfigurationService;

    public TimerServiceTest()
    {
        _timerConfigurationRepositoryStub = new TimerConfigurationRepositoryStub();
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

        timerService.IsRunning.Should().BeTrue();

        await TestUtils.WaitUntilAsync(() => monitoredTimerService.OccurredEvents.Length == 4,
            TimeSpan.FromSeconds(5), interval);
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

        timerService.IsRunning.Should().BeFalse();
        timerService.Remaining.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public async Task Start_AfterStarted_HasNoEffect()
    {
        var duration = TimeSpan.FromMilliseconds(40);
        var interval = TimeSpan.FromMilliseconds(20);

        _timerConfigurationRepositoryStub.SaveConfiguration(
            TimerConfigurationMother.With_WorkDuration_Interval(duration, interval));
        await using var timerService = new TimerService(_timerConfigurationService);
        using var monitoredTimerService = timerService.Monitor();

        timerService.Start();
        timerService.Start();

        timerService.IsRunning.Should().BeTrue();

        await TestUtils.WaitUntilAsync(() => monitoredTimerService.OccurredEvents.Length == 4,
            TimeSpan.FromSeconds(5), interval);
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

        timerService.IsRunning.Should().BeFalse();
        timerService.Remaining.Should().Be(TimeSpan.Zero);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(0)]
    public async Task Start_WhenDurationLessThanInterval_StartsTimerSuccessfully(double durationMilliseconds)
    {
        var duration = TimeSpan.FromMilliseconds(durationMilliseconds);
        var interval = TimeSpan.FromMilliseconds(20);

        _timerConfigurationRepositoryStub.SaveConfiguration(
            TimerConfigurationMother.With_WorkDuration_Interval(duration, interval));
        await using var timerService = new TimerService(_timerConfigurationService);
        using var monitoredTimerService = timerService.Monitor();

        timerService.Start();

        timerService.IsRunning.Should().BeTrue();

        await TestUtils.WaitUntilAsync(() => monitoredTimerService.OccurredEvents.Length == 3,
            TimeSpan.FromSeconds(5),
            interval);
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

        timerService.IsRunning.Should().BeFalse();
        timerService.Remaining.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public async Task Stop_WhenRunning_StopsTimerSuccessfully()
    {
        var duration = TimeSpan.FromMilliseconds(80);
        var interval = TimeSpan.FromMilliseconds(20);

        _timerConfigurationRepositoryStub.SaveConfiguration(
            TimerConfigurationMother.With_WorkDuration_Interval(duration, interval));
        await using var timerService = new TimerService(_timerConfigurationService);

        timerService.Start();
        using var monitoredTimerService = timerService.Monitor();
        timerService.Stop();

        timerService.IsRunning.Should().BeFalse();
        timerService.Remaining.Should().Be(duration);

        await Task.Delay(interval * 2, TestContext.Current.CancellationToken); // wait for any erroneous running events
        monitoredTimerService.OccurredEvents.Should().SatisfyRespectively(
            e =>
            {
                e.EventName.Should().Be(nameof(timerService.IsRunningChanged));
                e.Parameters[1].Should().Be(false);
            },
            e =>
            {
                e.EventName.Should().Be(nameof(timerService.RemainingChanged));
                e.Parameters[1].Should().Be(duration);
            }
        );
    }

    [Fact]
    public async Task Stop_AfterStopped_HasNoEffect()
    {
        var duration = TimeSpan.FromMilliseconds(80);
        var interval = TimeSpan.FromMilliseconds(20);

        _timerConfigurationRepositoryStub.SaveConfiguration(
            TimerConfigurationMother.With_WorkDuration_Interval(duration, interval));
        await using var timerService = new TimerService(_timerConfigurationService);

        timerService.Start();
        using var monitoredTimerService = timerService.Monitor();
        timerService.Stop();
        timerService.Stop();

        timerService.IsRunning.Should().BeFalse();
        timerService.Remaining.Should().Be(duration);

        await Task.Delay(interval * 2, TestContext.Current.CancellationToken); // wait for any erroneous running events
        monitoredTimerService.OccurredEvents.Should().SatisfyRespectively(
            e =>
            {
                e.EventName.Should().Be(nameof(timerService.IsRunningChanged));
                e.Parameters[1].Should().Be(false);
            },
            e =>
            {
                e.EventName.Should().Be(nameof(timerService.RemainingChanged));
                e.Parameters[1].Should().Be(duration);
            }
        );
    }

    [Fact]
    public async Task Stop_WhenNotRunning_HasNoEffect()
    {
        await using var timerService = new TimerService(_timerConfigurationService);
        using var monitoredTimerService = timerService.Monitor();

        timerService.Stop();

        timerService.IsRunning.Should().BeFalse();
        monitoredTimerService.OccurredEvents.Should().BeEmpty();
    }

    public static TheoryData<int, TimeSpan, TimerMode> NextModeData()
    {
        return new TheoryData<int, TimeSpan, TimerMode>
        {
            { 0, TimerConfigurationMother.DefaultBreakShortDuration, TimerMode.BreakShort },
            { 1, TimerConfigurationMother.DefaultWorkDuration, TimerMode.Work },
            { 2, TimerConfigurationMother.DefaultBreakShortDuration, TimerMode.BreakShort },
            { 3, TimerConfigurationMother.DefaultWorkDuration, TimerMode.Work },
            { 4, TimerConfigurationMother.DefaultBreakShortDuration, TimerMode.BreakShort },
            { 5, TimerConfigurationMother.DefaultWorkDuration, TimerMode.Work },
            { 6, TimerConfigurationMother.DefaultBreakLongDuration, TimerMode.BreakLong },
            { 7, TimerConfigurationMother.DefaultWorkDuration, TimerMode.Work }
        };
    }

    [Theory]
    [MemberData(nameof(NextModeData))]
    public async Task SwitchModeNext_WhenRunning_SwitchesModeSuccessfully(
        int initialModeIndex,
        TimeSpan expectedDuration,
        TimerMode expectedMode)
    {
        var interval = TimeSpan.FromMilliseconds(20);

        _timerConfigurationRepositoryStub.SaveConfiguration(
            TimerConfigurationMother.With_InitialModeIndex_Interval(initialModeIndex, interval));
        await using var timerService = new TimerService(_timerConfigurationService);

        timerService.Start();
        using var monitoredTimerService = timerService.Monitor();
        timerService.SwitchModeNext();

        timerService.IsRunning.Should().BeFalse();
        timerService.Remaining.Should().Be(expectedDuration);

        await Task.Delay(interval * 2, TestContext.Current.CancellationToken); // wait for any erroneous running events
        monitoredTimerService.OccurredEvents.Should().SatisfyRespectively(
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
                e.Parameters[1].Should().Be(expectedMode);
            }
        );
    }

    [Theory]
    [MemberData(nameof(NextModeData))]
    public async Task SwitchModeNext_WhenNotRunning_SwitchesModeSuccessfully(
        int initialModeIndex,
        TimeSpan expectedDuration,
        TimerMode expectedMode)
    {
        _timerConfigurationRepositoryStub.SaveConfiguration(
            TimerConfigurationMother.With_InitialModeIndex(initialModeIndex));
        await using var timerService = new TimerService(_timerConfigurationService);
        using var monitoredTimerService = timerService.Monitor();

        timerService.SwitchModeNext();

        timerService.IsRunning.Should().BeFalse();
        timerService.Remaining.Should().Be(expectedDuration);
        monitoredTimerService.OccurredEvents.Should().SatisfyRespectively(
            e =>
            {
                e.EventName.Should().Be(nameof(timerService.RemainingChanged));
                e.Parameters[1].Should().Be(expectedDuration);
            },
            e =>
            {
                e.EventName.Should().Be(nameof(timerService.ModeChanged));
                e.Parameters[1].Should().Be(expectedMode);
            }
        );
    }

    public static TheoryData<int, TimeSpan, TimerMode> PreviousModeData()
    {
        return new TheoryData<int, TimeSpan, TimerMode>
        {
            { 0, TimerConfigurationMother.DefaultBreakLongDuration, TimerMode.BreakLong },
            { 1, TimerConfigurationMother.DefaultWorkDuration, TimerMode.Work },
            { 2, TimerConfigurationMother.DefaultBreakShortDuration, TimerMode.BreakShort },
            { 3, TimerConfigurationMother.DefaultWorkDuration, TimerMode.Work },
            { 4, TimerConfigurationMother.DefaultBreakShortDuration, TimerMode.BreakShort },
            { 5, TimerConfigurationMother.DefaultWorkDuration, TimerMode.Work },
            { 6, TimerConfigurationMother.DefaultBreakShortDuration, TimerMode.BreakShort },
            { 7, TimerConfigurationMother.DefaultWorkDuration, TimerMode.Work }
        };
    }

    [Theory]
    [MemberData(nameof(PreviousModeData))]
    public async Task SwitchModePrevious_WhenRunning_SwitchesModeSuccessfully(
        int initialModeIndex,
        TimeSpan expectedDuration,
        TimerMode expectedMode)
    {
        var interval = TimeSpan.FromMilliseconds(20);

        _timerConfigurationRepositoryStub.SaveConfiguration(
            TimerConfigurationMother.With_InitialModeIndex_Interval(initialModeIndex, interval));
        await using var timerService = new TimerService(_timerConfigurationService);

        timerService.Start();
        using var monitoredTimerService = timerService.Monitor();
        timerService.SwitchModePrevious();

        timerService.IsRunning.Should().BeFalse();
        timerService.Remaining.Should().Be(expectedDuration);

        await Task.Delay(interval * 2, TestContext.Current.CancellationToken); // wait for any erroneous running events
        monitoredTimerService.OccurredEvents.Should().SatisfyRespectively(
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
                e.Parameters[1].Should().Be(expectedMode);
            }
        );
    }

    [Theory]
    [MemberData(nameof(PreviousModeData))]
    public async Task SwitchModePrevious_WhenNotRunning_SwitchesModeSuccessfully(
        int initialModeIndex,
        TimeSpan expectedDuration,
        TimerMode expectedMode)
    {
        _timerConfigurationRepositoryStub.SaveConfiguration(
            TimerConfigurationMother.With_InitialModeIndex(initialModeIndex));
        await using var timerService = new TimerService(_timerConfigurationService);
        using var monitoredTimerService = timerService.Monitor();

        timerService.SwitchModePrevious();

        timerService.IsRunning.Should().BeFalse();
        timerService.Remaining.Should().Be(expectedDuration);
        monitoredTimerService.OccurredEvents.Should().SatisfyRespectively(
            e =>
            {
                e.EventName.Should().Be(nameof(timerService.RemainingChanged));
                e.Parameters[1].Should().Be(expectedDuration);
            },
            e =>
            {
                e.EventName.Should().Be(nameof(timerService.ModeChanged));
                e.Parameters[1].Should().Be(expectedMode);
            }
        );
    }

    [Fact]
    public async Task Dispose_CancelsTimer()
    {
        var duration = TimeSpan.FromMilliseconds(80);
        var interval = TimeSpan.FromMilliseconds(20);

        _timerConfigurationRepositoryStub.SaveConfiguration(
            TimerConfigurationMother.With_WorkDuration_Interval(duration, interval));
        var timerService = new TimerService(_timerConfigurationService);

        timerService.Start();
        using var monitoredTimerService = timerService.Monitor();
        timerService.Dispose();

        await Task.Delay(interval * 2, TestContext.Current.CancellationToken);

        monitoredTimerService.OccurredEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task DisposeAsync_CancelsTimer()
    {
        var duration = TimeSpan.FromMilliseconds(80);
        var interval = TimeSpan.FromMilliseconds(20);

        _timerConfigurationRepositoryStub.SaveConfiguration(
            TimerConfigurationMother.With_WorkDuration_Interval(duration, interval));
        var timerService = new TimerService(_timerConfigurationService);

        timerService.Start();
        using var monitoredTimerService = timerService.Monitor();
        await timerService.DisposeAsync();

        await Task.Delay(interval * 2, TestContext.Current.CancellationToken);

        monitoredTimerService.OccurredEvents.Should().BeEmpty();
    }

}
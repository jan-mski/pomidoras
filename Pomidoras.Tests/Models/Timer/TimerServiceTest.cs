using FluentAssertions;
using Pomidoras.Models.Timer;
using Pomidoras.Models.Timer.Configuration;
using Pomidoras.Models.Timer.Configuration.Repository;

namespace Pomidoras.Tests.Models.Timer;

public class TimerServiceTest
{

    private readonly InMemoryTimerConfigurationRepository _timerConfigurationRepository = new();
    private readonly TimerConfigurationService _timerConfigurationService;

    public TimerServiceTest()
    {
        _timerConfigurationRepository.SaveConfiguration(TimerConfigurationMother.Default());
        _timerConfigurationService = new TimerConfigurationService(_timerConfigurationRepository);
    }

    [Fact]
    public async Task Start_WhenNotRunning_StartsTimerSuccessfully()
    {
        var duration = TimeSpan.FromMilliseconds(40);
        var interval = TimeSpan.FromMilliseconds(20);

        _timerConfigurationRepository.SaveConfiguration(
            TimerConfigurationMother.With_WorkDuration_Interval(duration, interval));
        var timerService = new TimerService(_timerConfigurationService);
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

        _timerConfigurationRepository.SaveConfiguration(
            TimerConfigurationMother.With_WorkDuration_Interval(duration, interval));
        var timerService = new TimerService(_timerConfigurationService);
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
    public void Start_WhenRunning_HasNoEffect()
    {
        var timerService = new TimerService(_timerConfigurationService);
        timerService.Start();
        using var monitoredTimer = timerService.Monitor();

        timerService.Start();

        timerService.IsRunning.Should().BeTrue();
        monitoredTimer.OccurredEvents.Should().BeEmpty();
    }

    [Fact]
    public void Stop_WhenRunning_StopsTimerSuccessfully()
    {
        var timerService = new TimerService(_timerConfigurationService);
        timerService.Start();
        using var monitoredTimer = timerService.Monitor();

        timerService.Stop();

        timerService.IsRunning.Should().BeFalse();
        monitoredTimer.OccurredEvents.Should().SatisfyRespectively(
            e =>
            {
                e.EventName.Should().Be(nameof(timerService.IsRunningChanged));
                e.Parameters[1].Should().Be(false);
            },
            e =>
            {
                e.EventName.Should().Be(nameof(timerService.RemainingChanged));
                e.Parameters[1].Should().Be(TimerConfigurationMother.DefaultWorkDuration);
            }
        );
    }

    [Fact]
    public void Stop_WhenNotRunning_HasNoEffect()
    {
        var timerService = new TimerService(_timerConfigurationService);
        using var monitoredTimer = timerService.Monitor();

        timerService.Stop();

        timerService.IsRunning.Should().BeFalse();
        monitoredTimer.OccurredEvents.Should().BeEmpty();
    }

}
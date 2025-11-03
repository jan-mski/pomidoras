using System;

namespace Pomidoras.Models.Timer;

public class TimerConfigurationService
{

    public TimerConfiguration GetTimerConfiguration()
    {
        return new TimerConfiguration(
            TimeSpan.FromMinutes(25),
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(20),
            TimeSpan.FromSeconds(1),
            TimerMode.Work,
            4);
    }

}
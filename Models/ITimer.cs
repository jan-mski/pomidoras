using System;

namespace Pomidoras.Models;

public interface ITimer
{

    TimeSpan Duration { get; set; }
    TimeSpan Remaining { get; set; }
    bool IsRunning { get; }
    void Start();
    void Stop();

}
using System.Diagnostics;

namespace Pomidoras.Tests;

public static class TestUtils
{

    public static async Task WaitUntilAsync(Func<bool> condition, TimeSpan timeout, TimeSpan? pollInterval = null)
    {
        var interval = pollInterval ?? TimeSpan.FromMilliseconds(100);
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < timeout)
        {
            if (condition())
            {
                return;
            }

            await Task.Delay(interval);
        }

        throw new TimeoutException($"Timeout after waiting {timeout} for condition");
    }

}
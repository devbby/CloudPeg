

namespace CloudPeg.Application.Utility;

public class EtaUtility
{
     
    /// <summary>
    /// Estimates the time remaining until a task is 100% complete.
    /// </summary>
    /// <param name="percentageCompleted">The percentage of the task completed (0.0 to 100.0).</param>
    /// <param name="timePassed">The amount of time that has passed since the task started.</param>
    /// <returns>A TimeSpan representing the estimated time remaining. Returns TimeSpan.Zero if the task is complete or if the percentage is invalid. Returns a large TimeSpan value for "infinite" if percentage is near zero.</returns>
    public static TimeSpan EstimateRemainingTime(double percentageCompleted, TimeSpan timePassed)
    {
        // Handle invalid inputs.
        if (percentageCompleted <= 0 || timePassed <= TimeSpan.Zero)
        {
            return TimeSpan.MaxValue; // Represents an infinite or indeterminable time.
        }

        // The task is already complete.
        if (percentageCompleted >= 100)
        {
            return TimeSpan.Zero;
        }

        try
        {
            // Calculate total estimated time.
            double totalTimeInSeconds = timePassed.TotalSeconds / (percentageCompleted / 100.0);
            TimeSpan totalEstimatedTime = TimeSpan.FromSeconds(totalTimeInSeconds);
            
            // Calculate remaining time.
            return totalEstimatedTime - timePassed;
        }
        catch (OverflowException)
        {
            // Catch potential overflows with extremely small percentage values.
            return TimeSpan.MaxValue;
        }
    }
    
}
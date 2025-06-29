
using System;
using System.Threading;
namespace ParkRoutePlanner;

public static class AlertScheduler
{
    private static Timer? alertTimer;

    public static void Start()
    {
        alertTimer = new Timer(_ =>
        {
            Console.WriteLine("Checking alerts for all active users...");
            foreach (var userId in ParkRoutePlanner.ActiveUserRoutes.Keys)
            {
                ParkRoutePlanner.CheckForRelevantLoadAlerts(userId);
            }
        }, null, TimeSpan.Zero, TimeSpan.FromMinutes(10));
    }

    public static void Stop()
    {
        alertTimer?.Dispose();
    }
}

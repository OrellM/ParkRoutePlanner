
using System;
public static class AlertScheduler
{
    private static Timer? alertTimer;

    // ? מופע גלובלי
    //private static ParkRoutePlanner.ParkRoutePlanner planner = new ParkRoutePlanner.ParkRoutePlanner();
    private static ParkRoutePlanner.ParkRoutePlanner? planner;
    public static void Initialize(IConfiguration config)
    {
        planner = new ParkRoutePlanner.ParkRoutePlanner(config);
    }

    public static void Start()
    {
        alertTimer = new Timer(_ =>
        {
            Console.WriteLine("Checking alerts for all active users...");
            foreach (var userId in planner.ActiveUserRoutes.Keys)
            {
                planner.CheckForRelevantLoadAlerts(userId);
            }
        }, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
    }

    public static void Stop()
    {
        alertTimer?.Dispose();
    }

    // אופציונלי – גישה חיצונית למופע הזה אם תצטרכי אותו
    public static ParkRoutePlanner.ParkRoutePlanner GetPlanner() => planner;
}
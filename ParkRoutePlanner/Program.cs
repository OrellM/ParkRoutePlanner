
using ParkRoutePlanner;
using ParkRoutePlanner.entity;
using ParkRoutePlanner.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// מוסיפים מדיניות CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")  // כתובת ה-React שלך
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// קובעים פורט ספציפי, לדוגמה 5001
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5001);
});

var app = builder.Build();

List<string> loadAlerts = new List<string>();

app.UseCors("AllowReactApp");  // מפעילים את מדיניות CORS

app.MapGet("/api/load-alerts", () =>
{
    return Results.Json(loadAlerts);
});
string FormatArrivalTime(int baseHour, int baseMinute, int minutesFromStart)
{
    int totalMinutes = baseHour * 60 + baseMinute + minutesFromStart;
    int hour = (totalMinutes / 60) % 24;
    int minute = totalMinutes % 60;
    return $"{hour:D2}:{minute:D2}";
}


app.MapPost("/api/visitor", (VisitorModel visitor) =>
{
    Console.WriteLine("🧾 JSON שהתקבל מהקליינט:");
    Console.WriteLine(JsonSerializer.Serialize(visitor));
    // --- קריאת קובץ עומסים והמרה למילון מקונן ---
    string json = File.ReadAllText("loads-2025-06-12.json");
    Dictionary<string, Dictionary<string, double>> futureLoads =
        JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, double>>>(json);

    if (futureLoads != null &&
        futureLoads.ContainsKey("אנקונדה") &&
        futureLoads["אנקונדה"].ContainsKey("13"))
    {
        Console.WriteLine("עומס באנוקונדה ב-13: " + futureLoads["אנקונדה"]["13"]);
    }

    // שליפת נתונים מה-DB
    string connectionString = "Data Source=DESKTOP\\SQLEXPRESS;Initial Catalog=NewDataPark;Integrated Security=True;";
    List<int> rideIds = new List<int>();
    List<string> attractionNamesList = new List<string>();
    List<int> durationsList = new List<int>();
    List<int> minAgesList = new List<int>();
    List<int> maxAgesList = new List<int>();
    List<int> minHeightsList = new List<int>();

    using (SqlConnection connection = new SqlConnection(connectionString))
    {
        connection.Open();

        // שליפת מתקנים
        string queryRides = @"
            SELECT ride_id, ride_name, avg_duration_minutes, min_age, max_age, min_height_cm

            FROM dbo.rides";

        using (SqlCommand cmdRides = new SqlCommand(queryRides, connection))
        using (SqlDataReader reader = cmdRides.ExecuteReader())
        {
            while (reader.Read())
            {
                rideIds.Add(Convert.ToInt32(reader["ride_id"]));
                attractionNamesList.Add(reader["ride_name"].ToString());
                durationsList.Add(reader["avg_duration_minutes"] == DBNull.Value ? 0 : Convert.ToInt32(reader["avg_duration_minutes"]));

                minAgesList.Add(reader["min_age"] == DBNull.Value ? 0 : Convert.ToInt32(reader["min_age"]));
                maxAgesList.Add(reader["max_age"] == DBNull.Value ? 120 : Convert.ToInt32(reader["max_age"]));
                minHeightsList.Add(reader["min_height_cm"] == DBNull.Value ? 0 : Convert.ToInt32(reader["min_height_cm"]));
            }
        }

        // ✅ הוספת שער כניסה כמתקן ראשון ידנית
        rideIds.Insert(0, -1); // מזהה פיקטיבי
        attractionNamesList.Insert(0, "שער כניסה");
        durationsList.Insert(0, 0); // אין זמן שהייה בשער

        minAgesList.Insert(0, 0);    // שער כניסה - אין הגבלת גיל מינימלי
        maxAgesList.Insert(0, 120);  // שער כניסה - אין הגבלת גיל מקסימלי
        minHeightsList.Insert(0, 0); // שער כניסה - אין הגבלת גובה


        int n = rideIds.Count;
        string[] attractionNames = attractionNamesList.ToArray();
        int[] durations = durationsList.ToArray();

        

        // ✅ אתחול מטריצת מרחקים עם שער כניסה
        int[,] distances = new int[n, n];
        int INF = 999999;
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                distances[i, j] = (i == j) ? 0 : INF;

        // שליפת מרחקים
        string queryDistances = "SELECT from_ride_id, to_ride_id, distance_meters FROM dbo.ride_distances";

        using (SqlCommand cmdDistances = new SqlCommand(queryDistances, connection))
        using (SqlDataReader reader = cmdDistances.ExecuteReader())
        {
            while (reader.Read())
            {
                int fromId = Convert.ToInt32(reader["from_ride_id"]);
                int toId = Convert.ToInt32(reader["to_ride_id"]);
                int distance = Convert.ToInt32(reader["distance_meters"]);

                int fromIndex = rideIds.IndexOf(fromId);
                int toIndex = rideIds.IndexOf(toId);
                int walkingTimeMinutes = (int)Math.Ceiling(distance / 80.0);

                if (fromIndex != -1 && toIndex != -1)
                {
                    distances[fromIndex, toIndex] = walkingTimeMinutes;
                    distances[toIndex, fromIndex] = walkingTimeMinutes;
                }
            }
        }

        int walkingTimeToEntrance = (int)Math.Ceiling(200 / 80.0);
        for (int i = 1; i < n; i++)
        {
            distances[0, i] = walkingTimeToEntrance;
            distances[i, 0] = walkingTimeToEntrance;
        }


        int[] minAges = minAgesList.ToArray();
        int[] maxAges = maxAgesList.ToArray();
        int[] minHeights = minHeightsList.ToArray();

        // יצירת מיפוי של שם מתקן לאינדקס ברשימה
        Dictionary<string, int> attractionIndexMap = new();
           for (int i = 0; i < attractionNamesList.Count; i++)
           {
               attractionIndexMap[attractionNamesList[i]] = i;
           }

           // אתחול מערך ההחרגות
           bool[] isExcluded = new bool[attractionNamesList.Count];
           if (visitor.ExcludedAttractions != null)
           {
            Console.WriteLine("🔎 visitor.ExcludedAttractions is not null.");
            Console.WriteLine($"🔢 Count: {visitor.ExcludedAttractions.Count}");
            foreach (var name in visitor.ExcludedAttractions)
            {
                Console.WriteLine($"📛 Excluded name: {name}");
                if (attractionIndexMap.TryGetValue(name, out int idx))
                {
                    isExcluded[idx] = true;
                }
            }


        }
        // סינון לפי גיל וגובה
        for (int i = 0; i < attractionNamesList.Count; i++)
        {
            if (visitor.Age < minAges[i] || visitor.Age > maxAges[i] || visitor.Height < minHeights[i])
            {
                isExcluded[i] = true;
                Console.WriteLine($"🚫 מתקן '{attractionNamesList[i]}' נשלל בגלל תנאי גיל/גובה: גיל={visitor.Age}, גובה={visitor.Height}");
            }
        }

        // 🔍 השוואת נתוני Load מול futureLoad (בדיקה אם יש חריגות)
        List<string> alerts = new List<string>();
        DateTime now = DateTime.Now;
        DateTime roundedHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0); // שעה עגולה

        string queryLoads = "SELECT attraction, visitors, timestamp FROM dbo.load WHERE timestamp >= @t";

        using (SqlCommand cmdLoad = new SqlCommand(queryLoads, connection))
        {
            cmdLoad.Parameters.AddWithValue("@t", now.AddMinutes(-5));
            using (SqlDataReader reader = cmdLoad.ExecuteReader())
            {
                while (reader.Read())
                {
                    string attraction = reader["attraction"].ToString();
                    int actual = Convert.ToInt32(reader["visitors"]);
                    DateTime ts = Convert.ToDateTime(reader["timestamp"]);
                    string hourKey = ts.Hour.ToString();

                    if (futureLoads != null &&
                        futureLoads.ContainsKey(attraction) &&
                        futureLoads[attraction].ContainsKey(hourKey))
                    {
                        double predicted = futureLoads[attraction][hourKey];
                        double percent = (actual - predicted) / predicted;

                        if (percent > 0.3) // יותר מ-30% עומס
                        {
                            alerts.Add($"📈 עומס חריג במתקן '{attraction}' בשעה {hourKey}: בפועל {actual}, חזוי {predicted}");
                        }
                    }
                }
            }
        }

        foreach (var alert in alerts)
        {
            Console.WriteLine("🚨 התראה: " + alert);
        }

        loadAlerts.Clear();
        loadAlerts.AddRange(alerts);


        int[] preferences = new int[attractionNames.Length];

        for (int i = 0; i < attractionNames.Length; i++)
        {
            if (visitor.PreferredAttractions.Contains(attractionNames[i]))
            {
                preferences[i] = 1;
            }
            else
            {
                preferences[i] = 0;
            }
        }
  

        int startNode = 0;

        // ParkRoutePlanner.ParkRoutePlanner.SetVisitTimes(visitor.VisitStartTime, visitor.VisitEndTime);

        var res = ParkRoutePlanner.ParkRoutePlanner.TSP(distances, durations, futureLoads, preferences, startNode, isExcluded, attractionNames);

        string[] namedRoute = new string[res.IndexRoute.Length];
        for (int i = 0; i < res.IndexRoute.Length; i++)
        {
            namedRoute[i] = attractionNames[res.IndexRoute[i]];
        }
        Console.WriteLine("מסלול שנבנה (גם אם חלקי): " + string.Join(" → ", namedRoute));
        foreach (var idx in res.IndexRoute)
        {
            string name = attractionNames[idx];
            int arrival = res.ArrivalTimes.ContainsKey(idx) ? res.ArrivalTimes[idx] : -1;
            Console.WriteLine($"🕓 {name} → {arrival} דקות מההתחלה");
        }
        Console.WriteLine("⏱ ArrivalTimes:");
        foreach (var kvp in res.ArrivalTimes)
        {
            Console.WriteLine($" - {kvp.Key}: {kvp.Value} דקות מההתחלה");
        }

        // 🕓 מחשבים שעת התחלה מתוך שעת ביקור
        string[] startParts = visitor.VisitStartTime.Split(":");
        int baseHour = int.Parse(startParts[0]);
        int baseMinute = int.Parse(startParts[1]);

        // ✨ בונים את המסלול עם השעות
        List<RouteStop> routeStops = new List<RouteStop>();

        for (int i = 0; i < res.IndexRoute.Length; i++)
        {
            int idx = res.IndexRoute[i];
            int minutesFromStart = res.ArrivalTimes.ContainsKey(idx) ? res.ArrivalTimes[idx] : 0;

            routeStops.Add(new RouteStop
            {
                Name = attractionNames[idx],
                ArrivalTime = FormatArrivalTime(baseHour, baseMinute, minutesFromStart)
            });
        }

        // 👑 בונים את האובייקט הסופי שמוחזר לקליינט
        FinalRoute result = new FinalRoute
        {
            Time = res.Time,
            RidesRoute = routeStops
        };

        return Results.Json(result);


    }
});

app.Run();



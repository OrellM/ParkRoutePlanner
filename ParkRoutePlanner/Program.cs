
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

app.UseCors("AllowReactApp");  // מפעילים את מדיניות CORS

//את זה להשאיר זה טוב
////////////////////////
/*app.MapPost("/api/visitor", (VisitorModel visitor) =>
{
    Console.WriteLine($"גיל: {visitor.Age}, גובה: {visitor.Height}");
    Console.WriteLine("קטגוריות מועדפות: " + string.Join(", ", visitor.PreferredCategories ?? new List<string>()));
    Console.WriteLine("שעת ביקור: " + visitor.VisitStartTime + " עד " + visitor.VisitEndTime);
    Console.WriteLine("מתקנים מועדפים: " + string.Join(", ", visitor.PreferredAttractions ?? new List<string>()));

    // שכפול של הלוגיקה מ־GET
    string[] attractionNames = {
        "שער כניסה",
        "מגלשות מים",
        "מכוניות מתנגשות",
        "ברייקדאנס",
        "גלגל ענק",
        "אנקונדה"
    };

    int[,] distances = {
        { 0, 5, 7, 8, 10, 12 },
        { 5, 0, 10, 15, 20, 25 },
        { 7, 10, 0, 12, 18, 22 },
        { 8, 15, 12, 0, 10, 14 },
        { 10, 20, 18, 10, 0, 12 },
        { 12, 25, 22, 14, 12, 0 }
    };

    int[] durations = { 0, 20, 30, 15, 20, 60 };

    int[] capacity = { 0, 100, 90, 150, 80, 200 };
    
    int[] preferences = { 0, 1, 1, 1, 0, 1 };
    int startNode = 0;

    var res = ParkRoutePlanner.ParkRoutePlanner.TSP(distances, durations, preferences, startNode);

    string[] namedRoute = new string[res.IndexRoute.Length];
    for (int i = 0; i < res.IndexRoute.Length; i++)
    {
        namedRoute[i] = attractionNames[res.IndexRoute[i]];
    }

    FinalRoute result = new FinalRoute
    {
        Time = res.Time,
        RidesRoute = namedRoute
    };
    Console.WriteLine("\nFinalRoute Object:");
    Console.WriteLine("Total Time: " + result.Time);
    Console.WriteLine("Rides Route: " + string.Join(" -> ", result.RidesRoute));

    return Results.Json(result);
});*/


/*app.MapPost("/api/visitor", async (VisitorModel visitor) =>
{
    string connectionString = @"Server=DESKTOP\SQLEXPRESS;Database=ParkData;Integrated Security=True;";
    int rideCount = 27;

    int[,] distances = new int[rideCount, rideCount];
    int[] durations = new int[rideCount];

    // אתחול מטריצת המרחקים עם ערכים התחלתיים
    for (int i = 0; i < rideCount; i++)
        for (int j = 0; j < rideCount; j++)
            distances[i, j] = (i == j) ? 0 : int.MaxValue;

    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        await conn.OpenAsync();

        // שליפת המרחקים
        string query = "SELECT from_ride_id, to_ride_id, distance_meters FROM ride_distances";
        using (SqlCommand cmd = new SqlCommand(query, conn))
        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                int from = reader.GetInt32(0) - 1;
                int to = reader.GetInt32(1) - 1;
                int distance = reader.GetInt32(2);
                distances[from, to] = distance;
                distances[to, from] = distance;
            }
        }

        // שליפת משכי שהייה ממוצעים
        string durationQuery = "SELECT avg_duration_minutes FROM rides ORDER BY ride_id";
        using (SqlCommand durationCmd = new SqlCommand(durationQuery, conn))
        using (SqlDataReader durationReader = await durationCmd.ExecuteReaderAsync())
        {
            int index = 0;
            while (await durationReader.ReadAsync())
            {
                durations[index++] = durationReader.GetInt32(0);
            }
        }
    }

    // לדוגמה, העדפות משתמש (תעדכן לפי הנתונים מ-visitor או ברירת מחדל)
    int[] preferences = new int[rideCount];
    for (int i = 0; i < rideCount; i++) preferences[i] = 1; // פשוט נותן 1 לכל המתקנים

    int startNode = 0;

    // קריאה לפונקציית תכנון המסלול - תחליף עם הלוגיקה שלך
    var result = ParkRoutePlanner.ParkRoutePlanner.TSP(distances, durations, preferences, startNode);

    // לדוגמה - נניח שברצונך להחזיר את הנתיב לפי שמות מתקנים
    // צריך לשלוף גם את שמות המתקנים מהדאטה
    string[] attractionNames = new string[rideCount];
    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        await conn.OpenAsync();
        string namesQuery = "SELECT ride_name FROM rides ORDER BY ride_id";
        using (SqlCommand namesCmd = new SqlCommand(namesQuery, conn))
        using (SqlDataReader namesReader = await namesCmd.ExecuteReaderAsync())
        {
            int i = 0;
            while (await namesReader.ReadAsync())
            {
                attractionNames[i++] = namesReader.GetString(0);
            }
        }
    }

    string[] namedRoute = new string[result.IndexRoute.Length];
    for (int i = 0; i < result.IndexRoute.Length; i++)
    {
        namedRoute[i] = attractionNames[result.IndexRoute[i]];
    }

    var finalRoute = new FinalRoute
    {
        Time = result.Time,
        RidesRoute = namedRoute
    };

    return Results.Json(finalRoute);
});*/

/*app.MapPost("/api/visitor", (VisitorModel visitor) =>
{
    string connectionString = "Data Source=DESKTOP\\SQLEXPRESS;Initial Catalog=ParkData;Integrated Security=True;";

    // רשימות זמניות לאחסון נתונים מהDB
    List<int> rideIds = new List<int>();
    List<string> attractionNamesList = new List<string>();
    List<int> durationsList = new List<int>();

    using (SqlConnection connection = new SqlConnection(connectionString))
    {
        connection.Open();

        // 1. שליפת מתקנים עם ride_id, ride_name ו־avg_duration_minutes
        string queryRides = @"
            SELECT ride_id, ride_name, avg_duration_minutes
            FROM dbo.rides";

        using (SqlCommand cmdRides = new SqlCommand(queryRides, connection))
        {
            using (SqlDataReader reader = cmdRides.ExecuteReader())
            {
                while (reader.Read())
                {
                    rideIds.Add(Convert.ToInt32(reader["ride_id"]));
                    attractionNamesList.Add(reader["ride_name"].ToString());
                    durationsList.Add(reader["avg_duration_minutes"] == DBNull.Value ? 0 : Convert.ToInt32(reader["avg_duration_minutes"]));
                }
            }
        }

        int n = rideIds.Count;

        // המרה למערכים
        string[] attractionNames = attractionNamesList.ToArray();
        int[] durations = durationsList.ToArray();

        // אתחול מטריצת מרחקים עם ערך INF (999999) למרחקים לא ידועים
        int[,] distances = new int[n, n];
        int INF = 999999;
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                distances[i, j] = (i == j) ? 0 : INF;

        // 2. שליפת מרחקים מהטבלה
        string queryDistances = "SELECT from_ride_id, to_ride_id, distance_meters FROM dbo.ride_distances";

        using (SqlCommand cmdDistances = new SqlCommand(queryDistances, connection))
        {
            using (SqlDataReader reader = cmdDistances.ExecuteReader())
            {
                while (reader.Read())
                {
                    int fromId = Convert.ToInt32(reader["from_ride_id"]);
                    int toId = Convert.ToInt32(reader["to_ride_id"]);
                    int distance = Convert.ToInt32(reader["distance_meters"]);

                    int fromIndex = rideIds.IndexOf(fromId);
                    int toIndex = rideIds.IndexOf(toId);

                    if (fromIndex != -1 && toIndex != -1)
                    {
                        distances[fromIndex, toIndex] = distance;
                        distances[toIndex, fromIndex] = distance; // אם רוצים לסמטרי
                    }
                }
            }
        }

        // 3. הגדרת מערך העדפות - כאן לדוגמה אתחול אפסים עם אורך מותאם
        int[] preferences = new int[n];
        // ניתן לאתחל לפי visitor.PreferredCategories או visitor.PreferredAttractions

        // לדוגמה, אם יש visitor.PreferredAttractions, אפשר לסמן 1 במערך העדפות:
        if (visitor.PreferredAttractions != null)
        {
            foreach (var prefName in visitor.PreferredAttractions)
            {
                int idx = Array.IndexOf(attractionNames, prefName);
                if (idx != -1)
                {
                    preferences[idx] = 1;
                }
            }
        }

        int startNode = 0; // לדוגמה, תמיד מתחילים ממתקן ראשון (שער כניסה)

        // קריאה לפונקציית תכנון המסלול (TSP)
        var res = ParkRoutePlanner.ParkRoutePlanner.TSP(distances, durations, preferences, startNode);

        string[] namedRoute = new string[res.IndexRoute.Length];
        for (int i = 0; i < res.IndexRoute.Length; i++)
        {
            namedRoute[i] = attractionNames[res.IndexRoute[i]];
        }

        FinalRoute result = new FinalRoute
        {
            Time = res.Time,
            RidesRoute = namedRoute
        };

        return Results.Json(result);
    }
});
*/

/*app.MapPost("/api/visitor", (VisitorModel visitor) =>
{

    // --- קריאת קובץ עומסים והמרה למילון מקונן ---
    string json = File.ReadAllText("loads-2025-06-12.json");
    Dictionary<string, Dictionary<string, double>> futureLoads =
        JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, double>>>(json);

    // דוגמה להדפסת עומס בשעה 13 באנוקונדה:
    if (futureLoads != null &&
        futureLoads.ContainsKey("אנקונדה") &&
        futureLoads["אנקונדה"].ContainsKey("13"))
    {
        Console.WriteLine("עומס באנוקונדה ב-13: " + futureLoads["אנקונדה"]["13"]);
    }


    string[] attractionNames = {
    "שער כניסה",             // 0
    "אנקונדה",               // 1
    "בלאק ממבה",             // 2
    "קרייזי טוויסטר",         // 3
    "טופ ספין / \"הסוכרייה\"", // 4
    "The King / המלך",        // 5
    "רכבת הרים",             // 6
    "קרוסלת פילים",          // 7
    "רכבת שדים",             // 8
    "מגלשות מים"             // 9
};

    int[,] distances = new int[10, 10]
    {
    //  0  1  2  3  4  5  6  7  8  9
    {  0, 5, 6, 7, 6, 5, 9, 7, 8, 9 }, // 0 שער כניסה
    {  5, 0, 4, 7, 5, 7, 8, 6, 7, 6 }, // 1 אנקונדה
    {  6, 4, 0, 5, 6, 5, 9, 8, 7, 7 }, // 2 בלאק ממבה
    {  7, 7, 5, 0, 4, 8, 6, 7, 6, 6 }, // 3 קרייזי טוויסטר
    {  6, 5, 6, 4, 0, 6, 7, 5, 6, 6 }, // 4 טופ ספין
    {  5, 7, 5, 8, 6, 0, 6, 7, 8, 7 }, // 5 המלך
    {  9, 8, 9, 6, 7, 6, 0, 8, 9, 8 }, // 6 רכבת הרים
    {  7, 6, 8, 7, 5, 7, 8, 0, 5, 6 }, // 7 קרוסלת פילים
    {  8, 7, 7, 6, 6, 8, 9, 5, 0, 6 }, // 8 רכבת שדים
    {  9, 6, 7, 6, 6, 7, 8, 6, 6, 0 }  // 9 מגלשות מים
    };

    int[] durations = {
    0, 60, 50, 40, 45, 50, 70, 35, 40, 35};



    //int[] preferences = {
    //0, 1, 0, 1, 1, 0, 0, 1, 0, 1};  // שער כניסה (לא נחשב העדפה)

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

    ParkRoutePlanner.ParkRoutePlanner.SetVisitTimes(visitor.VisitStartTime, visitor.VisitEndTime);

    var res = ParkRoutePlanner.ParkRoutePlanner.TSP(distances, durations, futureLoads, preferences, startNode);

    string[] namedRoute = new string[res.IndexRoute.Length];
    for (int i = 0; i < res.IndexRoute.Length; i++)
    {
        namedRoute[i] = attractionNames[res.IndexRoute[i]];
    }

    FinalRoute result = new FinalRoute
    {
        Time = res.Time,
        RidesRoute = namedRoute
    };

    return Results.Json(result);

});*/

/*app.MapPost("/api/visitor", (VisitorModel visitor) =>
{
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

    using (SqlConnection connection = new SqlConnection(connectionString))
    {
        connection.Open();

        // שליפת מתקנים
        string queryRides = @"
            SELECT ride_id, ride_name, avg_duration_minutes
            FROM dbo.rides";

        using (SqlCommand cmdRides = new SqlCommand(queryRides, connection))
        using (SqlDataReader reader = cmdRides.ExecuteReader())
        {
            while (reader.Read())
            {
                rideIds.Add(Convert.ToInt32(reader["ride_id"]));
                attractionNamesList.Add(reader["ride_name"].ToString());
                durationsList.Add(reader["avg_duration_minutes"] == DBNull.Value ? 0 : Convert.ToInt32(reader["avg_duration_minutes"]));
            }
        }

        int n = rideIds.Count;
        string[] attractionNames = attractionNamesList.ToArray();
        int[] durations = durationsList.ToArray();

        // אתחול מטריצת מרחקים
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

                if (fromIndex != -1 && toIndex != -1)
                {
                    distances[fromIndex, toIndex] = distance;
                    distances[toIndex, fromIndex] = distance;
                }
            }
        }

        // המשך הקוד כמו שהוא:

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

        var res = ParkRoutePlanner.ParkRoutePlanner.TSP(distances, durations, futureLoads, preferences, startNode);

        string[] namedRoute = new string[res.IndexRoute.Length];
        for (int i = 0; i < res.IndexRoute.Length; i++)
        {
            namedRoute[i] = attractionNames[res.IndexRoute[i]];
        }

        FinalRoute result = new FinalRoute
        {
            Time = res.Time,
            RidesRoute = namedRoute
        };

        return Results.Json(result);
    }
});*/
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

    using (SqlConnection connection = new SqlConnection(connectionString))
    {
        connection.Open();

        // שליפת מתקנים
        string queryRides = @"
            SELECT ride_id, ride_name, avg_duration_minutes
            FROM dbo.rides";

        using (SqlCommand cmdRides = new SqlCommand(queryRides, connection))
        using (SqlDataReader reader = cmdRides.ExecuteReader())
        {
            while (reader.Read())
            {
                rideIds.Add(Convert.ToInt32(reader["ride_id"]));
                attractionNamesList.Add(reader["ride_name"].ToString());
                durationsList.Add(reader["avg_duration_minutes"] == DBNull.Value ? 0 : Convert.ToInt32(reader["avg_duration_minutes"]));
            }
        }

        // ✅ הוספת שער כניסה כמתקן ראשון ידנית
        rideIds.Insert(0, -1); // מזהה פיקטיבי
        attractionNamesList.Insert(0, "שער כניסה");
        durationsList.Insert(0, 0); // אין זמן שהייה בשער

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

                if (fromIndex != -1 && toIndex != -1)
                {
                    distances[fromIndex, toIndex] = distance;
                    distances[toIndex, fromIndex] = distance;
                }
            }
        }

        // ✅ מרחקים מהשער לכל מתקן אחר
        for (int i = 1; i < n; i++)
        {
            distances[0, i] = 200; // לדוגמה – 200 מטר
            distances[i, 0] = 200;
        }


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

        var res = ParkRoutePlanner.ParkRoutePlanner.TSP(distances, durations, futureLoads, preferences, startNode, isExcluded);

        string[] namedRoute = new string[res.IndexRoute.Length];
        for (int i = 0; i < res.IndexRoute.Length; i++)
        {
            namedRoute[i] = attractionNames[res.IndexRoute[i]];
        }
        Console.WriteLine("מסלול שנבנה (גם אם חלקי): " + string.Join(" → ", namedRoute));

        FinalRoute result = new FinalRoute
        {
            Time = res.Time,
            RidesRoute = namedRoute
        };

        return Results.Json(result);
    }
});

app.Run();



//app.Run();


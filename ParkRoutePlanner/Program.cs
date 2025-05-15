using ParkRoutePlanner.entity;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace ParkRoutePlanner
{
    class Program
    {
        static void Main()
        {
            string[] attractionNames = {
                "שער כניסה",// 0
                "מגלשות מים",        
                "מכוניות מתנגשות",   // 1
                "ברייקדאנס",         // 2
                "גלגל ענק",          // 3
                "אנקונדה"            // 4
};
            // טבלת מרחקים בין 5 המתקנים
            int[,] distances = {
               {   0,  5,  7,  8, 10, 12 }, // שער כניסה
               {   5,  0, 10, 15, 20, 25 }, // מגלשות מים
               {   7, 10,  0, 12, 18, 22 }, // מכוניות מתנגשות
               {   8, 15, 12,  0, 10, 14 }, // ברייקדאנס
               {  10, 20, 18, 10,  0, 12 }, // גלגל ענק
               {  12, 25, 22, 14, 12,  0 }  // אנקונדה
            };
            // משך שהייה בכל מתקן (כולל זמן המתנה)
            int[] durations = { 0, 20, 30, 15, 20, 60 };

            // תחזיות עומסים עתידיים (לפי שעה)
            Dictionary<TimeOnly, List<int>> futureLoads = new Dictionary<TimeOnly, List<int>>
            {
                { new TimeOnly(10,0), new List<int> { 0,0, 0, 0, 0, 0 } },
                {  new TimeOnly(11,0), new List<int> {0, 15, 20, 5, 10, 25 } },
                {  new TimeOnly(12,0), new List<int> { 0, 20, 25, 10, 15, 40 } },
                {  new TimeOnly(13,0), new List<int> { 0, 30, 30, 15, 20, 60 } },
                {  new TimeOnly(14,0), new List<int> { 0, 40, 40, 25, 30, 90 } },
                {  new TimeOnly(15,0), new List<int> { 0, 40, 40, 25, 30, 90 } },
                {  new TimeOnly(16,0), new List<int> { 0, 35, 35, 20, 25, 80 } },
                {  new TimeOnly(17,0), new List<int> { 0, 30, 30, 15, 20, 70 } },
                { new TimeOnly(18, 0), new List<int> { 0, 25, 25, 10, 15, 50 } },
                { new TimeOnly(19, 0), new List<int> { 0, 20, 20, 7, 12, 40 } },
                { new TimeOnly(20, 0), new List<int> { 0, 15, 18, 6, 10, 30 } },
                { new TimeOnly(21, 0), new List<int> { 0, 15, 18, 6, 10, 30 } },
                { new TimeOnly(22, 0), new List<int> { 0, 15, 20, 12, 18, 30 } } // עומס מתון ב-22:00

            };


            // העדפות מבקרים (1 = עדיפות גבוהה, 0 = נמוכה)
            int[] preferences = {0, 1, 1, 1, 0, 1 };

            // נקודת התחלה - מגלשות מים
            int startNode = 0;

            // ParkRoutePlanner.TSP(distances, durations, futureLoads, preferences, startNode);
            Result res = ParkRoutePlanner.TSP(distances, durations, futureLoads, preferences, startNode) ;

            Console.WriteLine("Total Time: " + res.Time);
            Console.WriteLine("Optimal Route: " + string.Join(" -> ", res.IndexRoute));

            string[] namedRoute = new string[res.IndexRoute.Length];
            for (int i = 0; i < res.IndexRoute.Length; i++)
            {
                namedRoute[i] = attractionNames[res.IndexRoute[i]];
            }
            // יצירת אובייקט FinalRoute
            FinalRoute result = new FinalRoute
            {
                Time = res.Time,
                RidesRoute = namedRoute
            };

            // הדפסת האובייקט FinalRoute
            Console.WriteLine("\nFinalRoute Object:");
            Console.WriteLine("Total Time: " + result.Time);
            Console.WriteLine("Rides Route: " + string.Join(" -> ", result.RidesRoute));

            // הדפסת המסלול החלקי אם קיים
            if (ParkRoutePlanner.bestPathPartial != null && ParkRoutePlanner.bestPathPartial.Length > 0)
            {
                Console.WriteLine("\n>>> Best Partial Route Found (Fallback):");
                Console.WriteLine("Total Time: " + ParkRoutePlanner.bestTimePartial);

                List<string> partialNamedRoute = new List<string>();
                foreach (var index in ParkRoutePlanner.bestPathPartial)
                {
                    if (index == 0 && partialNamedRoute.Count > 0)
                        break;

                    partialNamedRoute.Add(attractionNames[index]);
                }

                Console.WriteLine("Rides Route: " + string.Join(" -> ", partialNamedRoute));
            }



        }
    }
}

/*using System;
using System.Linq;
using ParkRoutePlanner.Models;

class Program
{
    static void Main()
    {
        DbInspector.ShowAttractionsAndDistances();

    }
}*/



/*class Program
{
    static void Main(string[] args)
    {
        try
        {
            string[] names;
            int[,] matrix;

            DataLoader.LoadDistances(out names, out matrix);

            Console.WriteLine("✅ טענו בהצלחה את השמות והמרחקים!");

            // הדפסת שמות המתקנים
            Console.WriteLine("\n🎢 שמות המתקנים:");
            for (int i = 0; i < names.Length; i++)
            {
                Console.WriteLine($"{i}: {names[i]}");
            }

            // הדפסת חלק מהמטריצה (למשל 5 שורות ראשונות)
            Console.WriteLine("\n📏 חלק ממטריצת המרחקים:");
            int n = Math.Min(5, names.Length); // עד 5 שורות
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    string val = matrix[i, j] == int.MaxValue ? "∞" : matrix[i, j].ToString();
                    Console.Write(val.PadLeft(6));
                }
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ שגיאה בטעינת המידע: " + ex.Message);
        }

        Console.ReadLine(); // כדי שהחלון לא ייסגר מיד
    }
}*/




/*
    class Program
    {
        static void Main()
        {
            string connectionString = @"Server=DESKTOP\SQLEXPRESS;Database=ParkData;Integrated Security=True;";
            int rideCount = 27;
            int[,] distances = new int[rideCount, rideCount];
            int[] durations = new int[rideCount];

            // נאתחל את מטריצת המרחקים לערכים התחלתיים
            for (int i = 0; i < rideCount; i++)
                for (int j = 0; j < rideCount; j++)
                    distances[i, j] = (i == j) ? 0 : int.MaxValue;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                Console.WriteLine("🎉 התחברת למסד בהצלחה");

                // שליפת מרחקים
                string query = "SELECT from_ride_id, to_ride_id, distance_meters FROM ride_distances";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
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
                using (SqlDataReader durationReader = durationCmd.ExecuteReader())
                {
                    int index = 0;
                    while (durationReader.Read())
                    {
                        durations[index++] = durationReader.GetInt32(0);
                    }
                }
            }

            // טבלת עומסים עתידיים (הדגמה עבור 5 מתקנים — אם יש 27 תצטרכי לעדכן גם את זה)
            Dictionary<TimeOnly, List<int>> futureLoads = new Dictionary<TimeOnly, List<int>>
            {
                { new TimeOnly(10,0), new List<int> { 40, 0, 0, 0, 0 } },
                { new TimeOnly(11,0), new List<int> { 15, 20, 5, 10, 25 } },
                { new TimeOnly(12,0), new List<int> { 20, 25, 10, 15, 40 } },
                { new TimeOnly(13,0), new List<int> { 30, 30, 15, 20, 60 } },
                { new TimeOnly(14,0), new List<int> { 40, 40, 25, 30, 90 } },
                { new TimeOnly(15,0), new List<int> { 40, 40, 25, 30, 90 } },
                { new TimeOnly(16,0), new List<int> { 35, 35, 20, 25, 80 } },
                { new TimeOnly(17,0), new List<int> { 30, 30, 15, 20, 70 } },
                { new TimeOnly(18,0), new List<int> { 25, 25, 10, 15, 50 } },
                { new TimeOnly(19,0), new List<int> { 20, 20, 7, 12, 40 } },
                { new TimeOnly(20,0), new List<int> { 15, 18, 6, 10, 30 } },
                { new TimeOnly(21,0), new List<int> { 15, 18, 6, 10, 30 } },
                { new TimeOnly(22,0), new List<int> { 15, 20, 12, 18, 30 } }
            };

            // העדפות משתמש (נניח לדוגמה עבור 5 מתקנים – תעדכני לפי הצורך)
            int[] preferences = { 1, 1, 1, 0, 1 };

            // נקודת התחלה – מתקן ראשון
            int startNode = 0;

            // קריאה לפונקציית תכנון מסלול
            TSP(distances, durations, futureLoads, preferences, startNode);
        }

        static void TSP(int[,] distances, int[] durations, Dictionary<TimeOnly, List<int>> futureLoads, int[] preferences, int startNode)
        {
            // כאן שימי את הלוגיקה של האלגוריתם שלך 💜
            Console.WriteLine("הפונקציה TSP הופעלה עם הנתונים שהובאו ממסד הנתונים ✅");
        }
    }

*/
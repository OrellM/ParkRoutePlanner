/*using System;
using System.Collections.Generic;

namespace ParkRoutePlanner
{
    class Program
    {
        static void Main()
        {
            // טבלת מרחקים בין המתקנים (8 מתקנים)
            //מתקן נוסף
            int[,] distances = {
                { 0, 12, 18, 25, 30, 40, 22, 10 },
                { 12, 0, 10, 35, 15, 28, 16, 20 },
                { 18, 10, 0, 12, 22, 30, 14, 25 },
                { 25, 35, 12, 0, 8, 15, 26, 40 },
                { 30, 15, 22, 8, 0, 20, 10, 30 },
                { 40, 28, 30, 15, 20, 0, 18, 35 },
                { 22, 16, 14, 26, 10, 18, 0, 14 },
                { 10, 20, 25, 40, 30, 35, 14, 0 }
            };

            // משך שהייה בכל מתקן
            int[] durations = { 6, 12, 9, 15, 10, 8, 14, 7 };

            // תחזיות עומסים עתידיים
            Dictionary<int, List<int>> futureLoads = new Dictionary<int, List<int>>
            {
                { 0, new List<int> { 0, 3, 5, 7, 10, 12, 15, 18 } },
                { 1, new List<int> { 2, 5, 8, 10, 12, 15, 18, 20 } },
                { 2, new List<int> { 3, 6, 9, 12, 15, 18, 21, 24 } },
                { 3, new List<int> { 4, 8, 12, 16, 20, 24, 28, 30 } },
                { 4, new List<int> { 2, 5, 7, 10, 13, 16, 19, 22 } },
                { 5, new List<int> { 6, 9, 12, 15, 18, 21, 24, 27 } },
                { 6, new List<int> { 5, 10, 15, 20, 25, 30, 35, 40 } },
                { 7, new List<int> { 1, 4, 7, 10, 13, 16, 19, 22 } }
            };

            // העדפות מבקרים (1 = עדיפות גבוהה, 0 = נמוכה)
            int[] preferences = { 1, 0, 1, 1, 1, 0, 1, 0 };

            // נקודת התחלה
            int startNode = 0;

            ParkRoutePlanner.TSP(distances, durations, futureLoads, preferences, startNode);
        }
    }
}*/
using System;
using System.Collections.Generic;

namespace ParkRoutePlanner
{
    class Program
    {
        static void Main()
        {
            // טבלת מרחקים בין 5 המתקנים
            int[,] distances = {
                {  0, 10, 15, 20, 25 }, // מגלשות מים
                { 10,  0, 12, 18, 22 }, // מכוניות מתנגשות
                { 15, 12,  0, 10, 14 }, // ברייקדאנס
                { 20, 18, 10,  0, 12 }, // גלגל ענק
                { 25, 22, 14, 12,  0 }  // אנקונדה
            };

            // משך שהייה בכל מתקן (כולל זמן המתנה)
            int[] durations = { 20, 30, 15, 20, 60 };

            // תחזיות עומסים עתידיים (לפי שעה)
            Dictionary<TimeOnly, List<int>> futureLoads = new Dictionary<TimeOnly, List<int>>
            {
                { new TimeOnly(10,0), new List<int> { 40, 0, 0, 0, 0 } },
                {  new TimeOnly(11,0), new List<int> { 15, 20, 5, 10, 25 } },
                {  new TimeOnly(12,0), new List<int> { 20, 25, 10, 15, 40 } },
                {  new TimeOnly(13,0), new List<int> { 30, 30, 15, 20, 60 } },
                {  new TimeOnly(14,0), new List<int> { 40, 40, 25, 30, 90 } },
                {  new TimeOnly(15,0), new List<int> { 40, 40, 25, 30, 90 } },
                {  new TimeOnly(16,0), new List<int> { 35, 35, 20, 25, 80 } },
                {  new TimeOnly(17,0), new List<int> { 30, 30, 15, 20, 70 } },
                { new TimeOnly(18, 0), new List<int> { 25, 25, 10, 15, 50 } },
                { new TimeOnly(19, 0), new List<int> { 20, 20, 7, 12, 40 } },
                { new TimeOnly(20, 0), new List<int> { 15, 18, 6, 10, 30 } },
                { new TimeOnly(21, 0), new List<int> { 15, 18, 6, 10, 30 } },
                { new TimeOnly(22, 0), new List<int> { 15, 20, 12, 18, 30 } } // עומס מתון ב-22:00
            };

            // העדפות מבקרים (1 = עדיפות גבוהה, 0 = נמוכה)
            int[] preferences = { 1, 1, 1, 0, 1 };

            // נקודת התחלה - מגלשות מים
            int startNode = 0;

            ParkRoutePlanner.TSP(distances, durations, futureLoads, preferences, startNode);
        }
    }
}

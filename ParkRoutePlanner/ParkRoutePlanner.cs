using ParkRoutePlanner.entity;
using System;
using System.Collections.Generic;

namespace ParkRoutePlanner
{
    public class ParkRoutePlanner
    {
        //public static TimeOnly openingTime;
        //public static TimeOnly closingTime;
        public static TimeOnly openingTime = new TimeOnly(10, 0);
        public static TimeOnly closingTime = new TimeOnly(23, 0);
       /* public static void SetVisitTimes(string start, string end)
        {
            openingTime = TimeOnly.Parse(start);
            closingTime = TimeOnly.Parse(end);
        }*/
        private static int callsCounter = 0;  // ספירת קריאות לפונקציה
        private static int prunedPaths = 0;   // ספירת מסלולים שנחתכו

        private static int N;
        private static int[] finalPath;
        private static bool[] visited;
        private static int finalRes = int.MaxValue;
        //private static Dictionary<TimeOnly, List<int>> futureLoad;
        private static Dictionary<string, Dictionary<string, double>> futureLoad;
        private static int[,] adjMatrix;
        private static int[] rideDuration;
        private static int[] userPreferences;
        private static int startNode;

        static int timeLimitInMinutes; // זמן השהות של המבקר בפארק

        private static int partialBestRes = int.MaxValue;
        private static int partialBestLength = 0;
        private static int[] partialBestPath;
        public static Dictionary<int, int> attractionArrivalTimes = new();
        private static string[] attractionNames;
        private static void CopyToFinal(int[] currPath)
        {
            Array.Copy(currPath, finalPath, N);
            finalPath[N] = currPath[0];
        }



        //הפונקציה הקודמת שעבדה
        /* private static int GetFutureLoad(int attraction, int time)
         {
             TimeOnly now = TimeOnly.FromDateTime(DateTime.Now);
             TimeOnly targetTime = now.AddMinutes(time);

             // ננרמל את השעה כלפי מעלה לשעה עגולה (לדוגמה: 10:33 → 11:00)
             if (targetTime.Minute > 0 || targetTime.Second > 0)
             {
                 int newHour = targetTime.Hour + 1;
                 if (newHour >= 24) newHour = 23; // לא לעבור על השעה האחרונה בטווח
                 targetTime = new TimeOnly(newHour, 0);
             }

             if (futureLoad.ContainsKey(targetTime))
             {
                 var loads = futureLoad[targetTime];
                 Console.WriteLine($"[Load Check] Time: {targetTime}, Attraction: {attraction}, Load: {loads[attraction]}");
                 return loads[attraction];
             }

             Console.WriteLine($"[Load Check] Time: {targetTime}, Attraction: {attraction}, Load: 0 (Not found)");
             return 0;
         }*/
        /*זאת הפונקציה של השליפה מהקובץ
                private static int GetFutureLoad(int attraction, int time)
                {
                    TimeOnly now = TimeOnly.FromDateTime(DateTime.Now);
                    TimeOnly targetTime = now.AddMinutes(time);

                    // נרמל כלפי מעלה לשעה עגולה
                    if (targetTime.Minute > 0 || targetTime.Second > 0)
                    {
                        int newHour = targetTime.Hour + 1;
                        if (newHour >= 24) newHour = 23;
                        targetTime = new TimeOnly(newHour, 0);
                    }

                    string timeKey = targetTime.ToString("HH:mm");
                    string attractionId = attraction.ToString();

                    // טען את קובץ העומסים אם צריך
                    LoadManager.LoadDailyLoads();

                    // ניגש לנתונים
                    if (LoadManager.loadsData != null &&
                        LoadManager.loadsData.ContainsKey(timeKey) &&
                        LoadManager.loadsData[timeKey].ContainsKey(attractionId))
                    {
                        var load = LoadManager.loadsData[timeKey][attractionId];
                        Console.WriteLine($"[Load Check] Time: {timeKey}, Attraction: {attractionId}, Load: {load}");
                        return (int)load;
                    }

                    Console.WriteLine($"[Load Check] Time: {timeKey}, Attraction: {attractionId}, Load: 0 (Not found)");
                    return 0;
                }*/
        /* private static int GetFutureLoad(int attraction, int time)
         {
             TimeOnly now = TimeOnly.FromDateTime(DateTime.Now);
             TimeOnly targetTime = now.AddMinutes(time);

             // נרמל כלפי מעלה לשעה עגולה
             if (targetTime.Minute > 0 || targetTime.Second > 0)
             {
                 int newHour = targetTime.Hour + 1;
                 if (newHour >= 24) newHour = 23;
                 targetTime = new TimeOnly(newHour, 0);
             }

             string timeKey = targetTime.ToString("HH:mm");
             string attractionId = attraction.ToString();

             // ניגש לנתונים מתוך המילון שנשלח לפונקציה
             if (futureLoad != null &&
                 futureLoad.ContainsKey(timeKey) &&
                 futureLoad[timeKey].ContainsKey(attractionId))
             {
                 var load = futureLoad[timeKey][attractionId];
                 Console.WriteLine($"[Load Check] Time: {timeKey}, Attraction: {attractionId}, Load: {load}");
                 return (int)load;
             }

             Console.WriteLine($"[Load Check] Time: {timeKey}, Attraction: {attractionId}, Load: 0 (Not found)");
             return 0;
         }*/
        private static int GetFutureLoad(int attractionIndex, int time)
        {
            TimeOnly now = TimeOnly.FromDateTime(DateTime.Now);
            TimeOnly targetTime = now.AddMinutes(time);

            if (targetTime.Minute > 0 || targetTime.Second > 0)
            {
                int newHour = targetTime.Hour + 1;
                if (newHour >= 24) newHour = 23;
                targetTime = new TimeOnly(newHour, 0);
            }

            string hourKey = targetTime.Hour.ToString();
            string attractionName = attractionNames[attractionIndex];  // שימוש ב-attractionNames שהוגדרו גלובלית

            if (futureLoad != null &&
                futureLoad.ContainsKey(attractionName) &&
                futureLoad[attractionName].ContainsKey(hourKey))
            {
                var load = futureLoad[attractionName][hourKey];
                Console.WriteLine($"[Load Check] Time: {hourKey}, Attraction: {attractionName}, Load: {load}");
                return (int)load;
            }

            Console.WriteLine($"[Load Check] Time: {hourKey}, Attraction: {attractionName}, Load: 0 (Not found)");
            return 0;
        }



        private static bool ContainsAttraction(int[] path, int length, int attraction)
        {
            for (int i = 0; i < length; i++)
            {
                if (path[i] == attraction)
                    return true;
            }
            return false;
        }


        private static int FirstMin(int i)
        {
            int min = int.MaxValue;
            for (int k = 0; k < N; k++)
            {
                if (adjMatrix[i, k] < min && i != k)
                    min = adjMatrix[i, k];
            }
            return min;
        }

        private static int SecondMin(int i)
        {
            int first = int.MaxValue, second = int.MaxValue;
            for (int j = 0; j < N; j++)
            {
                if (i == j) continue;
                if (adjMatrix[i, j] <= first)
                {
                    second = first;
                    first = adjMatrix[i, j];
                }
                else if (adjMatrix[i, j] <= second && adjMatrix[i, j] != first)
                {
                    second = adjMatrix[i, j];
                }
            }
            return second;
        }
        // משתנים חדשים לשמירה על המסלול האופטימלי החלקי

        public static int[] bestPathPartial; // הכרזה בלבד, בלי new
        public static int bestTimePartial = int.MaxValue; //זמן השהייה במסלול החלקי
        public static int[] attractionVisitorsCount;
        public static int capacityPenaltyPoints = 20;

        private static void TSPRec(int currBound, int currWeight, int level, int[] currPath, int currTime, bool[] isExcluded, Dictionary<int, int> arrivalTimes)
        {
            Console.WriteLine($"[ENTER] TSPRec - level = {level}, currPath = {string.Join(" -> ", currPath.Take(level))}, currTime = {currTime}");

            callsCounter++;// מונה קריאות הפונקציה הרקורסיבית
            Console.WriteLine($"[DEBUG] Time Limit = {timeLimitInMinutes}");

            if (level == N)// אם הגענו לכל הקודקודים 
            {
                // מוודאים שיש דרך לחזור לנקודת ההתחלה (סגירת המעגל)

                if (adjMatrix[currPath[level - 1], currPath[0]] != 0)
                {
                    Console.WriteLine($"Trying to close path from {currPath[level - 1]} to {currPath[0]}: {adjMatrix[currPath[level - 1], currPath[0]]}");

                    int currRes = currWeight + adjMatrix[currPath[level - 1], currPath[0]];// חישוב המשקל הכולל של המסלול הסגור
                    if (currRes < finalRes)  // אם המשקל החדש קטן מהטוב ביותר שנמצא
                    {
                        CopyToFinal(currPath); // מעתיקים את הנתיב למשתנה המסלול הטוב ביותר
                        finalRes = currRes; // מעדכנים את המשקל הטוב ביותר
                    }
                    

                }
                return; // יוצאים מהקריאה הרקורסיבית כי סיימנו מסלול מלא
            }

            if (currPath[0] == 0 && (level > partialBestLength && currTime <= timeLimitInMinutes) ||
                (level == partialBestLength && currTime < bestTimePartial))
            // שמירת המסלול החלקי הטוב ביותר לפי מספר מתקנים ומשך זמן כולל
            {

                //Console.WriteLine($"[PARTIAL SAVE] New best partial path! Level = {level}, currTime = {currTime}");
                Array.Copy(currPath, bestPathPartial, level);
                attractionArrivalTimes = new Dictionary<int, int>(arrivalTimes);

                bestPathPartial[0] = startNode; // 👈 מוסיפים את זה כאן
                bestTimePartial = currTime;
                partialBestLength = level;
                partialBestRes = currWeight;

                Console.WriteLine($"[PARTIAL SAVE] Path: {string.Join(" -> ", currPath.Take(level))}");
            }

            for (int i = 0; i < N; i++) // עבור כל אטרקציה אפשרית
            {
                if (isExcluded[i])
                {
                    Console.WriteLine($"🚫 דילגנו על מתקן מוחרג: אינדקס {i}");
                    continue;
                }

                if (adjMatrix[currPath[level - 1], i] != 0 && !visited[i])
                {

                    int temp = currBound;
                    int tempTime = currTime; // שומרים זמנית את הזמן הנוכחי
                    int tempWeight = currWeight; // שומרים זמנית את המשקל הנוכחי
                    visited[i] = true;// סומן שבוקר כבר
                    int travelTime = adjMatrix[currPath[level - 1], i]; //זמן הנסיעה מהאטרקציה הנוכחית

                    int arrivalTime = currWeight + travelTime;
                    arrivalTimes[i] = arrivalTime;

                    int rideTime = rideDuration[i]; //משך זמן שהייה באטרקציה
                    int load = GetFutureLoad(i, currTime + travelTime); // המתנה משוערת בעת ההגעה
                    int waitTime = (load / 10) * rideTime; //חישוב זמן ההמתנה

                    Console.WriteLine($"[LOAD] Time: {currTime + travelTime}, Attraction: {i}, Load: {load}");

                    if (load == -1) // אם אין נתוני עומס לאטרקציה בזמן זה
                    {
                        Console.WriteLine($"[SKIP] Load missing for attraction {i} at time {currTime + travelTime}");
                        visited[i] = false; // משחררים את האטרקציה לסיבוב הבא
                        continue; // עוברים לאטרקציה הבאה בלולאה
                    }

                    waitTime = userPreferences[i] == 1 ? (int)(waitTime * 0.7) : (int)(waitTime * 1.5);
                    int totalTime = travelTime + rideTime + waitTime; //סה"כ זמן להוספה למסלול
                    int newTime = currTime + totalTime; //זמן מצטבר לאחר הוספת אטרקציה זו

                    Console.WriteLine($"[TRY] i={i}, travel={travelTime}, ride={rideTime}, wait={waitTime}, total={totalTime}, newTime={newTime}");

                    // שמירת המסלול החלקי הטוב ביותר לפי מספר מתקנים ומשך זמן
                    if (!ContainsAttraction(currPath, level, i) && (level + 1 > partialBestLength && newTime <= timeLimitInMinutes) ||
                        (level + 1 == partialBestLength && newTime < bestTimePartial))
                    {
                        Array.Copy(currPath, bestPathPartial, level); // מעתיקים את הנתיב עד לפני הוספת האטרקציה החדשה
                        attractionArrivalTimes = new Dictionary<int, int>(arrivalTimes);
                        bestPathPartial[0] = startNode;  // תמיד מגדירים את המקום הראשון!

                        bestPathPartial[level] = i; // מוסיפים את האטרקציה החדשה למסלול החלקי
                        bestTimePartial = newTime; // מעדכנים את זמן המסלול החלקי 
                        partialBestLength = level + 1; // מעדכנים את האורך החלקי
                        partialBestRes = currWeight + totalTime; // מעדכנים את המשקל של המסלול החלקי

                        //Console.WriteLine($"[PARTIAL SAVE - NEW] Time = {newTime}, Path = {string.Join(" -> ", currPath.Take(level))} -> {i}");
                    }

                    if (newTime > timeLimitInMinutes) // אם זמן המסלול עבר את המגבלה
                    {
                        // שמירת המסלול החלקי אם הוא טוב יותר
                        if ((level > partialBestLength && currTime <= timeLimitInMinutes) ||
                            (level == partialBestLength && currTime < bestTimePartial))
                        {
                            Array.Copy(currPath, bestPathPartial, level);
                            bestPathPartial[0] = startNode;  // 👈 חייב להיות כאן!
                            bestTimePartial = currTime;
                            partialBestLength = level;
                            partialBestRes = currWeight;
                            attractionArrivalTimes = new Dictionary<int, int>(arrivalTimes);

                            //Console.WriteLine($"[PRE-CUT SAVE] Time = {currTime}, Level = {level}, Path = {string.Join(" -> ", currPath.Take(level))}");
                        }

                        //Console.WriteLine($"[PRUNE] Time overflow: newTime = {newTime}, Limit = {timeLimitInMinutes}");
                        prunedPaths++; // מגדילים מונה חיתוכים
                        visited[i] = false; // משחררים את האטרקציה לבחירה אחרת
                        continue; // מדלגים על ענף זה
                    }

                    currPath[level] = i; // מוסיפים את האטרקציה למסלול הנוכחי ברמה הנוכחית
                    visited[i] = true; // מסמנים שבוקר

                    // מעדכנים את המשקל והזמן הכולל במסלול
                    currWeight += totalTime;
                    currTime += totalTime;
                    // התאמת המשקל לפי העדפות המשתמש (עדיפות מתקנים מועדפים)
                    //currWeight += userPreferences[i] == 1 ? -5 : 5;
                    currWeight += userPreferences[i] == 1 ? -1 * rideDuration[i] : rideDuration[i];

                    // חישוב Bound חדש לפי הנקודה ברמה הנוכחית
                    if (level == 1)
                        currBound -= (FirstMin(currPath[level - 1]) + FirstMin(i)) / 2;
                    else
                        currBound -= (SecondMin(currPath[level - 1]) + FirstMin(i)) / 2;

                    if (currBound + currWeight < finalRes) // בדיקת גבול כדי להחליט אם להמשיך עם הענף
                    {
                        TSPRec(currBound, currWeight, level + 1, currPath, currTime, isExcluded, new Dictionary<int, int>(arrivalTimes)); // קריאה רקורסיבית לרמה הבאה
                    }
                    else
                    {
                        Console.WriteLine($"[PRUNE] Bound = {currBound + currWeight} >= finalRes = {finalRes}");
                        prunedPaths++; // מניית החיתוכים
                    }

                    visited[i] = false; // מסמנים לא ביקרנו - משחררים לפני חזרה
                    currTime = tempTime;// מחזירים את הזמן למצב הקודם לפני קריאה רקורסיבית
                    currWeight = tempWeight;// מחזירים את המשקל למצב הקודם
                    currBound = temp;// מחזירים את bound למצב הקודם
                }
            }
        }


        public static Result TSP(int[,] distances, int[] durations, Dictionary<string, Dictionary<string, double>> futureLoads, int[] preferences, int start, bool[] isExcluded, string[] attractionNames)
        {
            DateTime startTime = DateTime.Now;
            TimeOnly now = TimeOnly.FromDateTime(DateTime.Now);  // הזמן הנוכחי

            if (now < openingTime || now >= closingTime) // בדיקת שעות פתיחה
            {
                Console.WriteLine("הפארק סגור כעת. לא ניתן לחשב מסלול.");
                return new Result { Time = -1, IndexRoute = Array.Empty<int>() }; // מחזירים תוצאה ריקה
            }

            N = distances.GetLength(0); // מספר האטרקציות
            adjMatrix = distances; // מטריצת זמני נסיעה בין אטרקציות
            rideDuration = durations; // זמני רכיבה בכל אטרקציה
            futureLoad = futureLoads; // עומסים עתידיים לכל אטרקציה וזמן
            userPreferences = preferences; // העדפות המשתמש
            ParkRoutePlanner.attractionNames = attractionNames;//

            startNode = start; // נקודת התחלה במסלול
            finalPath = new int[N + 1]; // מערך שמכיל את המסלול המלא הטוב ביותר
            visited = new bool[N]; // מערך לבקרות באטרקציות
            int[] currPath = new int[N + 1]; // המסלול הנוכחי בחיפוש
            bestPathPartial = new int[N + 1]; // המסלול החלקי הטוב ביותר
            bestTimePartial = int.MaxValue; // הזמן הטוב ביותר למסלול חלקי
            partialBestRes = int.MaxValue; // המשקל הטוב ביותר למסלול חלקי
            partialBestLength = 0; // האורך של המסלול החלקי הטוב ביותר
            attractionVisitorsCount = new int[N];
            int currBound = 0; // החישוב הראשוני של Bound (הערכת עליונה למינימום מסלול)
            Array.Fill(currPath, -1); // אתחול המסלול הנוכחי לערכים ריקים
            Array.Fill(visited, false); // אתחול מערך הביקורות

            // חישוב Bound התחלתי לפי מינימום שני הקצוות לכל צומת
            for (int i = 0; i < N; i++)
                currBound += (FirstMin(i) + SecondMin(i));

            currBound = (currBound % 2 == 1) ? currBound / 2 + 1 : currBound / 2; // תיקון עבור זוגיות
            visited[startNode] = true;
            currPath[0] = startNode; // הגדרת נקודת ההתחלה במסלול
            bestPathPartial[0] = startNode;

            // הגדרת זמן השהות של המשתמש בפארק
            timeLimitInMinutes = (int)(closingTime.ToTimeSpan() - now.ToTimeSpan()).TotalMinutes;
            Console.WriteLine($"Time limit in minutes: {timeLimitInMinutes}");

            TSPRec(currBound, 0, 1, currPath, 0, isExcluded, new Dictionary<int, int>()); // קריאה ראשונית לפונקציה הרקורסיבית

           // במידה ולא נמצא מסלול מלא, מחזירים את המסלול החלקי הטוב ביותר
            if (finalRes == int.MaxValue && partialBestLength > 1)
            {
                int[] partialResult = new int[partialBestLength + 1];
                Array.Copy(bestPathPartial, 0, partialResult, 0, partialBestLength);
                partialResult[partialBestLength] = bestPathPartial[0]; // סוגרים את המסלול חזרה להתחלה


                Console.WriteLine("לא נמצא מסלול מלא, מחזירים את המסלול החלקי הטוב ביותר.");
                return new Result
                {
                    Time = partialBestRes,
                    IndexRoute = partialResult,
                    ArrivalTimes = new Dictionary<int, int>(attractionArrivalTimes)

                };
            }
            else
            {
                Result result = new Result
                {
                    Time = bestTimePartial,
                    IndexRoute = new int[bestPathPartial.Length],
                    ArrivalTimes = new Dictionary<int, int>(attractionArrivalTimes)

                };
                Array.Copy(bestPathPartial, result.IndexRoute, bestPathPartial.Length);

                DateTime endTime = DateTime.Now;
                Console.WriteLine($"התחלתי את החישוב ב: {startTime:HH:mm:ss.fff}");
                Console.WriteLine($"סיימתי את החישוב ב: {endTime:HH:mm:ss.fff}");
                Console.WriteLine($"משך זמן הרצה: {(endTime - startTime).TotalMilliseconds} מילישניות");
                return result;
            }
        }
    }
}
    

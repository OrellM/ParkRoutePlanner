
using ParkRoutePlanner.entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ParkRoutePlanner
{

    public class ParkRoutePlanner
    {
        private readonly IConfiguration _config;

        public ParkRoutePlanner(IConfiguration config)
        {
            _config = config;
        }

        public TimeOnly OpeningTime;
        public TimeOnly ClosingTime;

        private int callsCounter = 0;
        private int prunedPaths = 0;

        private int N;
        private int[] finalPath;
        private bool[] visited;
        private int finalRes = int.MaxValue;
        private Dictionary<string, Dictionary<string, double>> futureLoad;
        private int[,] adjMatrix;
        private int[] rideDuration;
        private int[] userPreferences;
        private int startNode;
        public Dictionary<int, Result> ActiveUserRoutes = new();
        public Dictionary<int, HashSet<string>> SentAlerts = new();

        private int timeLimitInMinutes;

        private int partialBestRes = int.MaxValue;
        private int partialBestLength = 0;
        private int[] partialBestPath;
        //public int[] BestPathPartial;
        //public int BestTimePartial = int.MaxValue;
        public int[] AttractionVisitorsCount;
        public int CapacityPenaltyPoints = 20;
        public Dictionary<int, int> AttractionArrivalTimes = new();
        private string[] attractionNames;
        private int[] attractionCapacities;

        private void CopyToFinal(int[] currPath)
        {
            Array.Copy(currPath, finalPath, N);
            finalPath[N] = currPath[0];
        }

        public void CheckForRelevantLoadAlerts(int userId)
        {
            if (!ActiveUserRoutes.TryGetValue(userId, out Result userRoute))
                return;

            string connectionString = "Data Source=DESKTOP\\SQLEXPRESS;Initial Catalog=NewDataPark;Integrated Security=True;";

            foreach (int attractionIndex in userRoute.IndexRoute)
            {
                if (!userRoute.ArrivalTimes.TryGetValue(attractionIndex, out int arrivalMinutes))
                    continue;

                int arrivalHour = (arrivalMinutes / 60) + OpeningTime.Hour;

                string attractionName = attractionNames[attractionIndex];
                int actualLoad = 0;

                string sql = @"
                    SELECT AVG(CAST(visitors AS FLOAT))
                    FROM dbo.load
                    WHERE attraction = @attraction
                      AND DATEPART(HOUR, timestamp) = @hour
                      AND CAST(timestamp AS DATE) = CAST(GETDATE() AS DATE)";

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@attraction", attractionName);
                    cmd.Parameters.AddWithValue("@hour", arrivalHour);

                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        actualLoad = Convert.ToInt32(Math.Round(Convert.ToDouble(result)));
                    }
                }

                double expectedLoad = 0;
                if (futureLoad.TryGetValue(attractionName, out var loadsByHour) &&
                    loadsByHour.TryGetValue(arrivalHour.ToString(), out double expected))
                {
                    expectedLoad = expected;
                }

                Console.WriteLine($"🔍 בדיקה למתקן '{attractionName}' בשעה {arrivalHour}: בפועל {actualLoad}, חזוי {expectedLoad}");

                if (expectedLoad > 0 && actualLoad > expectedLoad * 1.5)
                {
                    string alertKey = $"{attractionName}-{arrivalHour}";

                    if (!SentAlerts.TryGetValue(userId, out var alerts))
                    {
                        alerts = new HashSet<string>();
                        SentAlerts[userId] = alerts;
                    }

                    if (!alerts.Contains(alertKey))
                    {
                        Console.WriteLine($"⚠️ עומס יתר במתקן {attractionName} בשעה {arrivalHour}: בפועל {actualLoad}, חזוי {expectedLoad}");
                        GlobalData.loadAlerts[userId] = $"📢 עומס חריג במתקן '{attractionName}' בשעה {arrivalHour}: בפועל {actualLoad}, חזוי {expectedLoad}";
                        alerts.Add(alertKey);
                    }
                }
            }
        }

        private bool CanAddVisitor(int attractionIndex, int arrivalTimeHour)
        {
            int currentLoad = 0;
            if (ParkLoadTracker.DynamicLoadMatrix.TryGetValue(attractionIndex, out var hourDict))
            {
                if (hourDict.TryGetValue(arrivalTimeHour, out var load))
                {
                    currentLoad = load;
                }
            }

            string attractionName = attractionNames[attractionIndex];
            string hourKey = arrivalTimeHour.ToString();

            double capacity = 0;
            if (futureLoad.ContainsKey(attractionName) && futureLoad[attractionName].ContainsKey(hourKey))
            {
                capacity = futureLoad[attractionName][hourKey];
            }
            else
            {
                Console.WriteLine($"⚠️ אין נתוני עומס עתידי למתקן {attractionName} בשעה {hourKey}");
                return false;
            }

            Console.WriteLine($"✅ בדיקת עומס למתקן {attractionName} ({attractionIndex}) בשעה {hourKey}: נוכחי = {currentLoad}, קיבולת = {capacity}");

            return currentLoad < capacity;
        }

        private int GetFutureLoad(int attractionIndex, int time)
        {
            TimeOnly now = OpeningTime;
            TimeOnly targetTime = now.AddMinutes(time);

            if (targetTime.Minute > 0 || targetTime.Second > 0)
            {
                int newHour = targetTime.Hour + 1;
                if (newHour >= 24) newHour = 23;
                targetTime = new TimeOnly(newHour, 0);
            }

            string hourKey = targetTime.Hour.ToString();
            string attractionName = attractionNames[attractionIndex];

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

        private bool ContainsAttraction(int[] path, int length, int attraction)
        {
            for (int i = 0; i < length; i++)
                if (path[i] == attraction)
                    return true;
            return false;
        }

        private int FirstMin(int i)
        {
            int min = int.MaxValue;
            for (int k = 0; k < N; k++)
                if (adjMatrix[i, k] < min && i != k)
                    min = adjMatrix[i, k];
            return min;
        }

        private int SecondMin(int i)
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
        public int[] bestPathPartial;
        public int bestTimePartial = int.MaxValue;
        public int[] attractionVisitorsCount;
        public int capacityPenaltyPoints = 20;

        private void TSPRec(int currBound, int currWeight, int level, int[] currPath, int currTime, List<int> allowedAttractions, bool[] isExcluded, Dictionary<int, int> arrivalTimes)
        {
            Console.WriteLine($"[ENTER] TSPRec - level = {level}, currPath = {string.Join(" -> ", currPath.Take(level))}, currTime = {currTime}");

            callsCounter++;
            Console.WriteLine($"[DEBUG] Time Limit = {timeLimitInMinutes}");

            if (level == N)
            {
                if (adjMatrix[currPath[level - 1], currPath[0]] != 0)
                {
                    Console.WriteLine($"Trying to close path from {currPath[level - 1]} to {currPath[0]}: {adjMatrix[currPath[level - 1], currPath[0]]}");
                    int currRes = currWeight + adjMatrix[currPath[level - 1], currPath[0]];
                    if (currRes < finalRes)
                    {
                        CopyToFinal(currPath);
                        finalRes = currRes;
                    }
                }
                return;
            }

            if (currPath[0] == 0 && (level > partialBestLength && currTime <= timeLimitInMinutes) ||
                (level == partialBestLength && currTime < bestTimePartial))
            {
                Array.Copy(currPath, bestPathPartial, level);
                AttractionArrivalTimes = new Dictionary<int, int>(arrivalTimes);
                bestPathPartial[0] = startNode;
                bestTimePartial = currTime;
                partialBestLength = level;
                partialBestRes = currWeight;
                Console.WriteLine($"[PARTIAL SAVE] Path: {string.Join(" -> ", currPath.Take(level))}");
            }

            //for (int i = 0; i < N; i++)
            foreach (int i in allowedAttractions)

            {
                /*if (isExcluded[i])
                {
                    Console.WriteLine($"🚫 דילגנו על מתקן מוחרג: אינדקס {i}");
                    continue;
                }*/

                if (adjMatrix[currPath[level - 1], i] != 0 && !visited[i])
                {
                    int temp = currBound;
                    int tempTime = currTime;
                    int tempWeight = currWeight;

                    int travelTime = adjMatrix[currPath[level - 1], i];
                    int arrivalTime = currWeight + travelTime;
                    arrivalTimes[i] = arrivalTime;

                    int arrivalHour = (int)Math.Ceiling((currTime + travelTime) / 60.0) + OpeningTime.Hour;

                    if (!CanAddVisitor(i, arrivalHour))
                    {
                        Console.WriteLine($"[SKIP] Capacity reached for attraction {i} at hour {arrivalHour}");
                        visited[i] = false;
                        continue;
                    }

                    int rideTime = rideDuration[i];
                    int load = GetFutureLoad(i, currTime + travelTime);
                    int capacity = attractionCapacities[i];

                    if (load == -1)
                    {
                        Console.WriteLine($"[SKIP] Load missing for attraction {i} at time {currTime + travelTime}");
                        visited[i] = false;
                        continue;
                    }

                    int waitTime = 0;
                    if (capacity > 0 && rideTime > 0)
                    {
                        int cyclesNeeded = (int)Math.Ceiling((double)load / capacity);
                        waitTime = cyclesNeeded * rideTime;
                    }

                    //waitTime = userPreferences[i] == 1 ? (int)(waitTime * 0.7) : (int)(waitTime * 1.5);
                    double boost = double.Parse(_config["Settings:PreferenceBoostFactor"]);
                    double penalty = double.Parse(_config["Settings:NonPreferencePenaltyFactor"]);

                    waitTime = userPreferences[i] == 1 ? (int)(waitTime * boost) : (int)(waitTime * penalty);

                    int totalTime = travelTime + rideTime + waitTime;
                    int newTime = currTime + totalTime;

                    Console.WriteLine($"[TRY] i={i}, travel={travelTime}, ride={rideTime}, wait={waitTime}, total={totalTime}, newTime={newTime}");

                    if (!ContainsAttraction(currPath, level, i) &&
                        ((level + 1 > partialBestLength && newTime <= timeLimitInMinutes) ||
                        (level + 1 == partialBestLength && newTime < bestTimePartial)))
                    {
                        Array.Copy(currPath, bestPathPartial, level);
                        AttractionArrivalTimes = new Dictionary<int, int>(arrivalTimes);
                        bestPathPartial[0] = startNode;
                        bestPathPartial[level] = i;
                        bestTimePartial = newTime;
                        partialBestLength = level + 1;
                        partialBestRes = currWeight + totalTime;
                    }

                    if (newTime > timeLimitInMinutes)
                    {
                        if ((level > partialBestLength && currTime <= timeLimitInMinutes) ||
                            (level == partialBestLength && currTime < bestTimePartial))
                        {
                            Array.Copy(currPath, bestPathPartial, level);
                            bestPathPartial[0] = startNode;
                            bestTimePartial = currTime;
                            partialBestLength = level;
                            partialBestRes = currWeight;
                            AttractionArrivalTimes = new Dictionary<int, int>(arrivalTimes);
                        }

                        prunedPaths++;
                        visited[i] = false;
                        continue;
                    }

                    currPath[level] = i;
                    visited[i] = true;
                    currWeight += totalTime;
                    currTime += totalTime;
                    currWeight += userPreferences[i] == 1 ? -1 * rideDuration[i] : rideDuration[i];

                    if (level == 1)
                        currBound -= (FirstMin(currPath[level - 1]) + FirstMin(i)) / 2;
                    else
                        currBound -= (SecondMin(currPath[level - 1]) + FirstMin(i)) / 2;

                    if (currBound + currWeight < finalRes)
                    {
                        TSPRec(currBound, currWeight, level + 1, currPath, currTime, allowedAttractions, isExcluded, new Dictionary<int, int>(arrivalTimes));
                    }
                    else
                    {
                        Console.WriteLine($"[PRUNE] Bound = {currBound + currWeight} >= finalRes = {finalRes}");
                        prunedPaths++;
                    }

                    visited[i] = false;
                    currTime = tempTime;
                    currWeight = tempWeight;
                    currBound = temp;
                }
            }
        }

        public Result TSP(
     int[,] distances,
     int[] durations,
     Dictionary<string, Dictionary<string, double>> futureLoads,
     int[] preferences,
     int start,
     bool[] isExcluded,
     string[] attractionNames,
     int[] capacities,
     int userId,
     List<int> allowedAttractions)
        {
            DateTime startTime = DateTime.Now;
            TimeOnly now = OpeningTime;

            if (now < OpeningTime || now >= ClosingTime)
            {
                Console.WriteLine("הפארק סגור כעת. לא ניתן לחשב מסלול.");
                return new Result { Time = -1, IndexRoute = Array.Empty<int>() };
            }

            N = distances.GetLength(0);
            adjMatrix = distances;
            rideDuration = durations;
            futureLoad = futureLoads;
            userPreferences = preferences;
            attractionCapacities = capacities;
            this.attractionNames = attractionNames;

            startNode = start;
            finalPath = new int[N + 1];
            visited = new bool[N];
            int[] currPath = new int[N + 1];
            bestPathPartial = new int[N + 1];
            bestTimePartial = int.MaxValue;
            this.bestPathPartial = new int[N + 1];
            this.bestPathPartial[0] = startNode;

            partialBestRes = int.MaxValue;
            partialBestLength = 0;
            attractionVisitorsCount = new int[N];

            int currBound = 0;
            Array.Fill(currPath, -1);
            Array.Fill(visited, false);

            for (int i = 0; i < N; i++)
                currBound += (FirstMin(i) + SecondMin(i));

            currBound = (currBound % 2 == 1) ? currBound / 2 + 1 : currBound / 2;
            visited[startNode] = true;
            currPath[0] = startNode;
            bestPathPartial[0] = startNode;

            timeLimitInMinutes = (int)(ClosingTime.ToTimeSpan() - OpeningTime.ToTimeSpan()).TotalMinutes;
            //timeLimitInMinutes = 800;
            Console.WriteLine($"Time limit in minutes: {timeLimitInMinutes}");

            TSPRec(currBound, 0, 1, currPath, 0, allowedAttractions, isExcluded, new Dictionary<int, int>());

            if (finalRes == int.MaxValue && partialBestLength > 1)
            {
                int[] partialResult = new int[partialBestLength + 1];
                Array.Copy(bestPathPartial, 0, partialResult, 0, partialBestLength);
                partialResult[partialBestLength] = bestPathPartial[0];

                var arrivalTimes = new Dictionary<int, int>(AttractionArrivalTimes);

                for (int i = 0; i < partialBestLength; i++)
                {
                    int attractionIndex = partialResult[i];
                    if (arrivalTimes.TryGetValue(attractionIndex, out int arrivalTimeInMinutes))
                    {
                        int arrivalHour = (arrivalTimeInMinutes + 59) / 60 + OpeningTime.Hour;
                        ParkLoadTracker.DynamicLoadMatrix[attractionIndex][arrivalHour]++;
                    }
                }

                Console.WriteLine("לא נמצא מסלול מלא, מחזירים את המסלול החלקי הטוב ביותר.");

                ActiveUserRoutes[userId] = new Result
                {
                    Time = partialBestRes,
                    IndexRoute = partialResult,
                    ArrivalTimes = new Dictionary<int, int>(AttractionArrivalTimes)
                };
                return ActiveUserRoutes[userId];
            }
            else
            {
                Result result = new Result
                {
                    Time = bestTimePartial,
                    IndexRoute = new int[bestPathPartial.Length],
                    ArrivalTimes = new Dictionary<int, int>(AttractionArrivalTimes)
                };
                Array.Copy(bestPathPartial, result.IndexRoute, bestPathPartial.Length);

                var arrivalTimes = result.ArrivalTimes;
                var route = result.IndexRoute;

                for (int i = 0; i < route.Length - 1; i++)
                {
                    int attractionIndex = route[i];
                    if (arrivalTimes.TryGetValue(attractionIndex, out int arrivalTimeInMinutes))
                    {
                        int arrivalHour = (arrivalTimeInMinutes + 59) / 60 + OpeningTime.Hour;
                        ParkLoadTracker.DynamicLoadMatrix[attractionIndex][arrivalHour]++;
                    }
                }

                ActiveUserRoutes[userId] = result;

                DateTime endTime = DateTime.Now;
                Console.WriteLine($"התחלתי את החישוב ב: {startTime:HH:mm:ss.fff}");
                Console.WriteLine($"סיימתי את החישוב ב: {endTime:HH:mm:ss.fff}");
                Console.WriteLine($"משך זמן הרצה: {(endTime - startTime).TotalMilliseconds} מילישניות");

                return result;
            }
        }
    }
}

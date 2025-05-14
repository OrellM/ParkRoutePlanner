using ParkRoutePlanner.entity;
using System;
using System.Collections.Generic;

namespace ParkRoutePlanner
{
    public class ParkRoutePlanner
    {
        private static readonly TimeOnly openingTime = new TimeOnly(9, 0);
        private static readonly TimeOnly closingTime = new TimeOnly(23, 40);

        private static int callsCounter = 0;  // ספירת קריאות לפונקציה
        private static int prunedPaths = 0;   // ספירת מסלולים שנחתכו

        private static int N;
        private static int[] finalPath;
        private static bool[] visited;
        private static int finalRes = int.MaxValue;
        private static Dictionary<TimeOnly, List<int>> futureLoad;
        private static int[,] adjMatrix;
        private static int[] rideDuration;
        private static int[] userPreferences;
        private static int startNode;

        static int timeLimitInMinutes; // זמן השהות של המבקר בפארק

        private static int partialBestRes = int.MaxValue;
        private static int partialBestLength = 0;
        private static int[] partialBestPath;
        private static void CopyToFinal(int[] currPath)
        {
            Array.Copy(currPath, finalPath, N);
            finalPath[N] = currPath[0];
        }
        //time= מספר הדקות שהמשתמש הולך להיות במתקנים קודמים לאטרקציה + מספר הדקות שלוקח לו להגיע עד לאטרקציה החדשה
        /* private static int GetFutureLoad(int attraction, int time)
         {
             TimeOnly hour = TimeOnly.FromDateTime(DateTime.Now).AddMinutes(time);
             if (futureLoad.ContainsKey(hour))
             {
                 var loads = futureLoad[hour];

                 return loads[attraction-1];
             }

             return 0; // במקרה שאין נתונים, מחזירים 0
         }*/

        //זה הקודם שלי
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

        private static int GetFutureLoad(int attraction, int time)
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
        public static int bestTimePartial = int.MaxValue;

        private static void TSPRec(int currBound, int currWeight, int level, int[] currPath, int currTime)
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

            // ✨ שינוי 1: עדיפות לפי מספר מתקנים > זמן כולל
            if ((level > partialBestLength && currTime <= timeLimitInMinutes) ||
                (level == partialBestLength && currTime < bestTimePartial))
            {
                Console.WriteLine($"[PARTIAL SAVE] New best partial path! Level = {level}, currTime = {currTime}");
                Array.Copy(currPath, bestPathPartial, level);
                bestTimePartial = currTime;
                partialBestLength = level;
                partialBestRes = currWeight;
                Console.WriteLine($"[PARTIAL SAVE] Path: {string.Join(" -> ", currPath.Take(level))}");
            }

            for (int i = 0; i < N; i++)
            {
                if (adjMatrix[currPath[level - 1], i] != 0 && !visited[i])
                {
                    int temp = currBound;
                    int tempTime = currTime;
                    int tempWeight = currWeight;

                    visited[i] = true;

                    int travelTime = adjMatrix[currPath[level - 1], i];
                    int rideTime = rideDuration[i];
                    int load = GetFutureLoad(i, currTime + travelTime);
                    int waitTime = (load / 10) * rideTime;

                    Console.WriteLine($"[LOAD] Time: {currTime + travelTime}, Attraction: {i}, Load: {load}");

                    if (load == -1)
                    {
                        Console.WriteLine($"[SKIP] Load missing for attraction {i} at time {currTime + travelTime}");
                        visited[i] = false;
                        continue;
                    }

                    waitTime = userPreferences[i] == 1 ? (int)(waitTime * 0.7) : (int)(waitTime * 1.5);
                    int totalTime = travelTime + rideTime + waitTime;
                    int newTime = currTime + totalTime;

                    Console.WriteLine($"[TRY] i={i}, travel={travelTime}, ride={rideTime}, wait={waitTime}, total={totalTime}, newTime={newTime}");

                    // ✨ שינוי 2: גם כאן עדיפות לפי מספר מתקנים
                    if ((level + 1 > partialBestLength && newTime <= timeLimitInMinutes) ||
                        (level + 1 == partialBestLength && newTime < bestTimePartial))
                    {
                        Array.Copy(currPath, bestPathPartial, level);
                        bestPathPartial[level] = i;
                        bestTimePartial = newTime;
                        partialBestLength = level + 1;
                        partialBestRes = currWeight + totalTime;
                        Console.WriteLine($"[PARTIAL SAVE - NEW] Time = {newTime}, Path = {string.Join(" -> ", currPath.Take(level))} -> {i}");
                    }

                    if (newTime > timeLimitInMinutes)
                    {
                        // ✨ שינוי 3: גם כאן נעדיף יותר מתקנים
                        if ((level > partialBestLength && currTime <= timeLimitInMinutes) ||
                            (level == partialBestLength && currTime < bestTimePartial))
                        {
                            Array.Copy(currPath, bestPathPartial, level);
                            bestTimePartial = currTime;
                            partialBestLength = level;
                            partialBestRes = currWeight;
                            Console.WriteLine($"[PRE-CUT SAVE] Time = {currTime}, Level = {level}, Path = {string.Join(" -> ", currPath.Take(level))}");
                        }

                        Console.WriteLine($"[PRUNE] Time overflow: newTime = {newTime}, Limit = {timeLimitInMinutes}");
                        prunedPaths++;
                        visited[i] = false;
                        continue;
                    }

                    currPath[level] = i;
                    visited[i] = true;

                    currWeight += totalTime;
                    currTime += totalTime;
                    currWeight += userPreferences[i] == 1 ? -5 : 5;

                    if (level == 1)
                        currBound -= (FirstMin(currPath[level - 1]) + FirstMin(i)) / 2;
                    else
                        currBound -= (SecondMin(currPath[level - 1]) + FirstMin(i)) / 2;

                    if (currBound + currWeight < finalRes)
                    {
                        TSPRec(currBound, currWeight, level + 1, currPath, currTime);
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







        public static Result TSP(int[,] distances, int[] durations, Dictionary<TimeOnly, List<int>> futureLoads, int[] preferences, int start)
        {
            TimeOnly now = TimeOnly.FromDateTime(DateTime.Now);
            if (now < openingTime || now >= closingTime)
            {
                Console.WriteLine("הפארק סגור כעת. לא ניתן לחשב מסלול.");
                return new Result { Time = -1, IndexRoute = Array.Empty<int>() };
            }

            N = distances.GetLength(0);
            adjMatrix = distances;
            rideDuration = durations;
            futureLoad = futureLoads;
            userPreferences = preferences;
            startNode = start;
            finalPath = new int[N + 1];
            visited = new bool[N];
            int[] currPath = new int[N + 1];
            bestPathPartial = new int[N + 1];
            bestTimePartial = int.MaxValue;
            partialBestRes = int.MaxValue;
            partialBestLength = 0;

            int currBound = 0;
            Array.Fill(currPath, -1);
            Array.Fill(visited, false);

            for (int i = 0; i < N; i++)
                currBound += (FirstMin(i) + SecondMin(i));

            currBound = (currBound % 2 == 1) ? currBound / 2 + 1 : currBound / 2;
            visited[startNode] = true;
            currPath[0] = startNode;
            // הגדרת זמן השהות של המשתמש בפארק
            timeLimitInMinutes = (int)(closingTime.ToTimeSpan() - now.ToTimeSpan()).TotalMinutes;
            //timeLimitInMinutes = 240;
            Console.WriteLine($"Time limit in minutes: {timeLimitInMinutes}");

            TSPRec(currBound, 0, 1, currPath, 0);

           /* if (bestTimePartial != int.MaxValue)
            {
                Console.WriteLine("===== Optimal Route Summary =====");
                var filteredPath = bestPathPartial
                    .Where((value, index) => value != 0 || index == 0 || index == bestPathPartial.Length - 1)
                    .ToList();

                // הדפסת המסלול
                Console.WriteLine("Partial Best Route: " + string.Join(" -> ", filteredPath)); Console.WriteLine("Partial Best Time: " + bestTimePartial);
                Console.WriteLine("=================================");
            }
            else
            {
                Console.WriteLine("No optimal route found within the time limit.");
            }*/

            if (finalRes == int.MaxValue && partialBestLength > 1)
            {
                int[] partialResult = new int[partialBestLength + 1];
                Array.Copy(bestPathPartial, 0, partialResult, 0, partialBestLength);
                partialResult[partialBestLength] = bestPathPartial[0];

                Console.WriteLine("לא נמצא מסלול מלא, מחזירים את המסלול החלקי הטוב ביותר.");
                return new Result
                {
                    Time = partialBestRes,
                    IndexRoute = partialResult
                };
            }
            else
            {
                Result result = new Result
                {
                    Time = bestTimePartial,
                    IndexRoute = new int[bestPathPartial.Length]
                };
                Array.Copy(bestPathPartial, result.IndexRoute, bestPathPartial.Length);
                return result;
            }
        }
    }
}
    

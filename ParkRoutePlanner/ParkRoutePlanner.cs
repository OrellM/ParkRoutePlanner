using ParkRoutePlanner.entity;
using System;
using System.Collections.Generic;

namespace ParkRoutePlanner
{
    public class ParkRoutePlanner
    {
        private static readonly TimeOnly openingTime = new TimeOnly(10, 0);
        private static readonly TimeOnly closingTime = new TimeOnly(22, 0);

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

        private static void TSPRec(int currBound, int currWeight, int level, int[] currPath, int currTime)
        {
            // 💡 בדיקה אם אין עוד אטרקציות שאפשר להמשיך אליהן בזמן המותר
          
            callsCounter++;  // סופרים כל קריאה לפונקציה

            /*TimeOnly currentTime = TimeOnly.FromDateTime(DateTime.Now).AddMinutes(currTime);
            if (currentTime > closingTime)
            {
                prunedPaths++; // ספירת חיתוכים
                return; // אם הזמן עבר, חתוך את המסלול
            }
            if (currentTime > closingTime)
            {
                // שמירת מסלול חלקי – טוב יותר ממה שהיה
                if (currWeight < finalRes && level > 1)
                {
                    Array.Copy(currPath, finalPath, level);
                    finalPath[level] = currPath[0]; // סגירה חזרה להתחלה
                    finalRes = currWeight;
                    N = level; // עדכון מספר התחנות במסלול האופטימלי
                }

                prunedPaths++;
                return;
            }*/
            TimeOnly currentVisitTime = TimeOnly.FromDateTime(DateTime.Now).AddMinutes(currTime);
            if (currentVisitTime > closingTime)
            {
                if (level > partialBestLength || (level == partialBestLength && currWeight < partialBestRes))
                {
                    Array.Copy(currPath, partialBestPath, level);
                    partialBestLength = level;
                    partialBestRes = currWeight;
                }

                prunedPaths++;
                return;
            }


            if (level == N)
            {
                if (adjMatrix[currPath[level - 1], currPath[0]] != 0)
                {
                    int currRes = currWeight + adjMatrix[currPath[level - 1], currPath[0]];
                    if (currRes < finalRes)
                    {
                        CopyToFinal(currPath);
                        finalRes = currRes;
                    }
                }
                return;
            }

            for (int i = 0; i < N; i++)
            {
                if (adjMatrix[currPath[level - 1], i] != 0 && !visited[i])
                {
                    int temp = currBound;
                    int travelTime = adjMatrix[currPath[level - 1], i];
                    int rideTime = rideDuration[i];
                    int load = GetFutureLoad(i, currTime + travelTime);
                    int waitTime = (load / 10) * rideTime;
                    int totalTime = travelTime + rideTime + waitTime;

                     // השפעת העדפות המבקר על זמן ההמתנה
                     if (userPreferences[i] == 1)
                         waitTime = (int)(waitTime * 0.7); // העדפה גבוהה – מקטינים את זמן ההמתנה ב-30%
                     else
                         waitTime = (int)(waitTime * 1.5); // העדפה נמוכה – מגדילים את זמן ההמתנה ב-50%
        
                    currWeight += totalTime;
                    currTime += totalTime;
                    // חיתוך מסלולים חורגים משעות הפעילות
                    /*TimeOnly currentVisitTime = TimeOnly.FromDateTime(DateTime.Now).AddMinutes(currTime);
                    if (currentVisitTime < openingTime || currentVisitTime > closingTime)
                    {
                        prunedPaths++; // ספירת חיתוכים
                        return; // לא ממשיכים במסלול הזה
                    }*/

                  /*  // חיתוך מסלולים חורגים משעות הפעילות

                    currentTime = TimeOnly.FromDateTime(DateTime.Now).AddMinutes(currTime);
                    if (currentTime > closingTime)
                    {
                        prunedPaths++; // ספירת חיתוכים
                        return; // לא ממשיכים במסלול הזה
                    }
                  */
                    // השפעת העדפות המבקר על המשקל הכולל
                    if (userPreferences[i] == 1)
                        currWeight -= 5; // נותן בונוס שלילי (מוריד מהמשקל) אם המבקר מעדיף את האטרקציה
                    else
                        currWeight += 5; // מוסיף משקל אם האטרקציה פחות מועדפת
                    

                    if (level == 1)
                        currBound -= (FirstMin(currPath[level - 1]) + FirstMin(i)) / 2;
                    else
                        currBound -= (SecondMin(currPath[level - 1]) + FirstMin(i)) / 2;

                    if (currBound + currWeight < finalRes)
                    {
                        currPath[level] = i;
                        visited[i] = true;
                        TSPRec(currBound, currWeight, level + 1, currPath, currTime);
                    }
                    else
                    {
                        prunedPaths++;  // ספרנו מסלול שנחתך
                        continue;  // דילוג על הענף הזה
                    }

                    currWeight -= totalTime;
                    currBound = temp;
                    visited[i] = false;
                }
            }
        }
        public static Result TSP(int[,] distances, int[] durations, Dictionary<TimeOnly, List<int>> futureLoads, int[] preferences, int start)
        {
            TimeOnly now = TimeOnly.FromDateTime(DateTime.Now);
            if (now < openingTime || now >= closingTime)
            {
                Console.WriteLine("הפארק סגור כעת. לא ניתן לחשב מסלול.");
                return new Result
                {
                    Time = -1,
                    IndexRoute = Array.Empty<int>()
                };
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

            partialBestPath = new int[N + 1];

            int currBound = 0;
            Array.Fill(currPath, -1);
            Array.Fill(visited, false);

            for (int i = 0; i < N; i++)
                currBound += (FirstMin(i) + SecondMin(i));

            currBound = (currBound % 2 == 1) ? currBound / 2 + 1 : currBound / 2;
            visited[startNode] = true;
            currPath[0] = startNode;

            TSPRec(currBound, 0, 1, currPath, 0);
            if (finalRes == int.MaxValue && partialBestLength > 0)
            {
                int[] partialRoute = new int[partialBestLength + 1];
                Array.Copy(partialBestPath, partialRoute, partialBestLength);
                partialRoute[partialBestLength] = partialBestPath[0]; // חזרה להתחלה

                return new Result
                {
                    Time = partialBestRes,
                    IndexRoute = partialRoute
                };
            }
            else
            { 
            Result result = new Result
            {
                Time = finalRes,
                IndexRoute = new int[finalPath.Length]
            };
            //Array.Copy(finalPath, result.IndexRoute, N + 1);
            Array.Copy(finalPath, result.IndexRoute, finalPath.Length);

            return result;
            }
            /* Console.WriteLine("Minimum time: " + finalRes);
             Console.Write("Optimal Path: ");
             for (int i = 0; i <= N; i++)
                 Console.Write(finalPath[i] + " ");
             //הדפסה
             Console.WriteLine("\nTotal recursive calls: " + callsCounter);
             Console.WriteLine("Total pruned paths: " + prunedPaths);*/
        }
    }
}

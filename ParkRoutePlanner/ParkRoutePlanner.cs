using System;
using System.Collections.Generic;

namespace ParkRoutePlanner
{
    public class ParkRoutePlanner
    {
        private static int N;
        private static int[] finalPath;
        private static bool[] visited;
        private static int finalRes = int.MaxValue;
        private static Dictionary<int, List<int>> futureLoad;
        private static int[,] adjMatrix;
        private static int[] rideDuration;
        private static int[] userPreferences;
        private static int startNode;

        private static void CopyToFinal(int[] currPath)
        {
            Array.Copy(currPath, finalPath, N);
            finalPath[N] = currPath[0];
        }

        private static int GetFutureLoad(int attraction, int time)
        {
            if (futureLoad.ContainsKey(attraction))
            {
                var loads = futureLoad[attraction];

                // בדיקה שהאינדקס בטווח המותר
                if (time >= 0 && time < loads.Count)
                {
                    return loads[time];
                }
            }

            return 0; // במקרה שאין נתונים, מחזירים 0
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
                    int waitTime = GetFutureLoad(i, currTime + travelTime);
                    int totalTime = travelTime + rideTime + waitTime;

                    currWeight += totalTime;
                    currTime += totalTime;

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

                    currWeight -= totalTime;
                    currBound = temp;
                    visited[i] = false;
                }
            }
        }

        public static void TSP(int[,] distances, int[] durations, Dictionary<int, List<int>> futureLoads, int[] preferences, int start)
        {
            N = distances.GetLength(0);
            adjMatrix = distances;
            rideDuration = durations;
            futureLoad = futureLoads;
            userPreferences = preferences;
            startNode = start;
            finalPath = new int[N + 1];
            visited = new bool[N];
            int[] currPath = new int[N + 1];

            int currBound = 0;
            Array.Fill(currPath, -1);
            Array.Fill(visited, false);

            for (int i = 0; i < N; i++)
                currBound += (FirstMin(i) + SecondMin(i));

            currBound = (currBound % 2 == 1) ? currBound / 2 + 1 : currBound / 2;
            visited[startNode] = true;
            currPath[0] = startNode;

            TSPRec(currBound, 0, 1, currPath, 0);

            Console.WriteLine("Minimum time: " + finalRes);
            Console.Write("Optimal Path: ");
            for (int i = 0; i <= N; i++)
                Console.Write(finalPath[i] + " ");
        }
    }
}

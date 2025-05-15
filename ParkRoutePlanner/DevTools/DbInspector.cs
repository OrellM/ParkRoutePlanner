// TestQueries.cs
using System;
using System.Linq;
using ParkRoutePlanner.Models;

public static class DbInspector
{
    public static void ShowAttractionsAndDistances()
    {
        using var context = new ParkDataContext();

        var attractions = context.Rides.OrderBy(a => a.RideId).ToList();
        string[] attractionNames = attractions.Select(a => a.RideName).ToArray();
        int[] durations = attractions.Select(a => a.AvgDurationMinutes ?? 0).ToArray();
        int n = attractions.Count;
        int[,] distances = new int[n, n];
        var rideDistances = context.RideDistances.ToList();

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (i == j)
                {
                    distances[i, j] = 0;
                }
                else
                {
                    int fromId = attractions[i].RideId;
                    int toId = attractions[j].RideId;

                    var distanceEntry = rideDistances
                        .FirstOrDefault(d => d.FromRideId == fromId && d.ToRideId == toId);

                    distances[i, j] = distanceEntry?.DistanceMeters ?? int.MaxValue;
                }
            }
        }

        Console.WriteLine("Attractions:");
        for (int i = 0; i < n; i++)
            Console.WriteLine($"{i}: {attractionNames[i]}");

        Console.WriteLine("\nAttractions with durations:");
        for (int i = 0; i < n; i++)
            Console.WriteLine($"{i}: {attractionNames[i]} - Duration: {durations[i]} min");

        Console.WriteLine("\nDistances matrix:");
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
                Console.Write($"{distances[i, j],6} ");
            Console.WriteLine();
        }
    }
}

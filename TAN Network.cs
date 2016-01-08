using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TAN_Network
{
    class Station : IComparable<Station>
    {
        public string Identifier { get; set; }
        public string FullName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Type { get; set; }
        public List<Station> Destinations { get; set; }

        public double CalculateDistanceTo(Station other)
        {
            double x = (other.Longitude - this.Longitude) * Math.Cos((other.Latitude + this.Latitude) / 2);
            double y = (other.Latitude - this.Latitude);

            double distance = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) * 6371;

            return distance;
        }

        public bool LeadsDirectlyTo(Station other)
        {
            return this.Destinations.Contains(other);
        }

        public int CompareTo(Station other)
        {
            return this.Identifier.CompareTo(other.Identifier);
        }
    }

    public static class Calculations
    {
        public static double ToRadians(this double degrees)
        {
            return (Math.PI / 180) * degrees;
        }
    }

    public static class EnumerableExtension
    {
        public static IEnumerable<T> Except<T>(this IEnumerable<T> items, T item) where T : IComparable<T>
        {
            foreach (var i in items)
            {
                if (!i.Equals(item))
                {
                    yield return i;
                }
            }
        }
    }
    
    class Network
    {
        private HashSet<Station> Stations;

        private Station StartingPoint;
        private Station EndingPoint;

        private const string NoSuchStationsExist = "IMPOSSIBLE";

        public void ReadFromConsole()
        {
            Program.WriteLineLocally("Enter the identifier of the starting point of the journey: ");
            string startIdentifier = Console.ReadLine().Split(':').Last();

            Program.WriteLineLocally("Enter the identifier of the ending point of the journey: ");
            string endIdentifier = Console.ReadLine().Split(':').Last();

            Program.WriteLineLocally("How many stations are there in total? ");
            int numStations = int.Parse(Console.ReadLine());

            this.Stations = new HashSet<Station>();

            Program.WriteLineLocally("Please enter, line by line, information about each station: ");
            for (int count = 0; count < numStations; count++)
            {
                string[] input = Console.ReadLine().Split(':', ',');

                var station = new Station();
                station.Identifier = input[1];
                station.FullName = input[2].Replace("\"", "");
                
                double latitudeDegrees = double.Parse(input[4]);
                station.Latitude = latitudeDegrees.ToRadians();

                double longitudeDegrees = double.Parse(input[5]);
                station.Longitude = longitudeDegrees.ToRadians();

                station.Type = int.Parse(input[8]);
                station.Destinations = new List<Station>();

                Stations.Add(station);

                if (station.Identifier == startIdentifier)
                {
                    this.StartingPoint = station;
                }


                if (station.Identifier == endIdentifier)
                {
                    this.EndingPoint = station;
                }
            }

            Program.WriteLineLocally("How many routes are there in total? ");
            int numRoutes = int.Parse(Console.ReadLine());

            Program.WriteLineLocally("Please enter, line by line, information about each route: ");
            for (int count = 0; count < numRoutes; count++)
            {
                string[] input = Console.ReadLine().Split(':', ' ');

                string firstIdentifier = input[1];
                string secondIdentifier = input[3];

                Station firstStation = this.Stations.Where(station => station.Identifier == firstIdentifier).First();
                Station secondStation = this.Stations.Where(station => station.Identifier == secondIdentifier).First();

                firstStation.Destinations.Add(secondStation);
            }
        }

        private Stack<Station> findShortestRoute()
        {
            const double UnknownDistance = double.MaxValue;

            var unvisitedStations = new HashSet<Station>(this.Stations);
            var shortestPath = new Stack<Station>();

            var distancesFromStartingPoint = new Dictionary<Station, double>();
            var previousStationAlongShortestPath = new Dictionary<Station, Station>();


            distancesFromStartingPoint.Add(this.StartingPoint, 0);
            
            foreach (Station station in this.Stations.Except(this.StartingPoint))
            {
                distancesFromStartingPoint.Add(station, UnknownDistance);
            }

            bool foundPathToEndStation = false;
            while (!foundPathToEndStation && unvisitedStations.Any())
            {
                Station currentStation = distancesFromStartingPoint.Where(pair => unvisitedStations.Contains(pair.Key))
                                                                   .OrderBy(pair => pair.Value)
                                                                   .First().Key;
                unvisitedStations.Remove(currentStation);

                if (currentStation == this.EndingPoint)
                {
                    foundPathToEndStation = true;
                    shortestPath = composeShortestPath(distancesFromStartingPoint, previousStationAlongShortestPath);
                }
                else
                {
                    foreach (Station neighbour in currentStation.Destinations)
                    {
                        double distanceFromCurrentStationToNeighbour = currentStation.CalculateDistanceTo(neighbour);
                        double distanceFromStartingPointToNeighbour = distancesFromStartingPoint[currentStation] + distanceFromCurrentStationToNeighbour;

                        if (distanceFromStartingPointToNeighbour < distancesFromStartingPoint[neighbour])
                        {
                            distancesFromStartingPoint[neighbour] = distanceFromStartingPointToNeighbour;
                            previousStationAlongShortestPath[neighbour] = currentStation;
                        }
                    }
                }
            }

            return shortestPath;
        }

        private Stack<Station> composeShortestPath(Dictionary<Station, double> distancesFromStartingPoint, Dictionary<Station, Station> previousStationAlongShortestPath)
        {
            var shortestPath = new Stack<Station>();
            Station target = this.EndingPoint;

            while (previousStationAlongShortestPath.ContainsKey(target))
            {
                shortestPath.Push(target);
                target = previousStationAlongShortestPath[target];
            }

            if (target.Equals(this.StartingPoint))
            {
                shortestPath.Push(target);
            }
            else
            {
                shortestPath.Clear();
            }

            return shortestPath;
        }

        public void PrintStationsAlongShortestPath()
        {
            Stack<Station> shortestPath = this.findShortestRoute();
            var stationNames = new List<string>();

            if (shortestPath.Count == 0)
            {
                stationNames.Add(NoSuchStationsExist);
            }
            else
            {
                foreach (Station station in shortestPath)
                {
                    stationNames.Add(station.FullName);
                }
            }

            foreach (string stationName in stationNames)
            {
                Console.WriteLine(stationName);
            }
        }
    }

    class Program
    {
        private static bool isRunningLocally;

        internal static void WriteLineLocally(string input)
        {
            if (isRunningLocally)
            {
                Console.WriteLine(input);
            }
        }

        static void Main(string[] args)
        {
            isRunningLocally = args.Length > 0;

            var network = new Network();
            network.ReadFromConsole();
            network.PrintStationsAlongShortestPath();

            Console.WriteLine("Press any key to exit: ");
            Console.ReadKey();
        }
    }
}

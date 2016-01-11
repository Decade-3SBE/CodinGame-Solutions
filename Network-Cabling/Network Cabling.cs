using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkCabling
{
    public class Buildings
    {
        private const long NotCalculatedYet = -1;

        private Point[] locations;
        private long cableLength = NotCalculatedYet;

        public long CableLength
        {
            get
            {
                if (this.cableLength == NotCalculatedYet)
	            {
                    this.cableLength = calculateShortestCableLength(this.locations);
	            }

                return this.cableLength;
            }
        }

        public void ReadFromConsole()
        {
            Program.WriteLineLocally("How many buildings are there? ");

            int numBuildings = int.Parse(Console.ReadLine());
            locations = new Point[numBuildings];

            Program.WriteLineLocally("Please enter, line by line, the x-y coordinates of each building, separating each pair with a white space: ");

            for (int building = 0; building < numBuildings; building++)
            {
                string[] coordinates = Console.ReadLine().Split();
                int x = int.Parse(coordinates[0]);
                int y = int.Parse(coordinates[1]);

                Point location = new Point(x, y);
                locations[building] = location;
            }
        }

        private static long calculateShortestCableLength(Point[] locations)
        {
            long mainCableLength = calculateMainCableLength(locations);
            long shortestTotalLengthOfVerticalCables = calculateShortestTotalLengthOfVerticalCables(locations);

            return mainCableLength + shortestTotalLengthOfVerticalCables;
        }

        private static long calculateMainCableLength(Point[] locations)
        {
            long largestXCoordinate = locations.Max(location => location.X);
            long smallestXCoordinate = locations.Min(location => location.X);

            return largestXCoordinate - smallestXCoordinate;
        }

        private static long calculateShortestTotalLengthOfVerticalCables(Point[] locations)
        {
            int medianY = calculateMedianYCoordinate(locations);

            long totalLengthOfVerticalCables = 0;

            foreach (Point location in locations)
            {
                totalLengthOfVerticalCables += Math.Abs(medianY - location.Y);
            }

            return totalLengthOfVerticalCables;
        }

        private static int calculateMedianYCoordinate(Point[] locations)
        {
            int[] orderedYCoordinatesOfLocations = locations.Select(location => location.Y).OrderBy(yCoordinate => yCoordinate).ToArray();
            int medianIndex = locations.Count() / 2;
            int medianY = orderedYCoordinatesOfLocations[medianIndex];

            return medianY;
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

            Buildings buildings = new Buildings();
            buildings.ReadFromConsole();

            Console.WriteLine(buildings.CableLength);

            Console.WriteLine("Press any key to exit: ");
            Console.ReadKey();
        }
    }
}

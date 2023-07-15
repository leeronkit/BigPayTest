using ConsoleApp1;
using System;
using System.IO;

namespace MailDelivery
{
    class Program
    {
        static void Main(string[] args)
        {
            // Initialize the trains
            var trains = new List<Trains>
            {
                new Trains("Q1", 6, "B", false),
            };

            // Initialize the packages
            var packages = new List<Packages>
            {
                new Packages("K1", 5, "A", "C"),
            };

            TrainNodes graphNodes = new();
            // Add the station
            graphNodes.AddNode("A");
            graphNodes.AddNode("B");
            graphNodes.AddNode("C");

            // Add the edges
            graphNodes.AddEdge("A", "B", 30);
            graphNodes.AddEdge("B", "C", 10);

            var assignSchedules = new Dictionary<Trains, Packages> { };

            foreach (var package in packages)
            {
                // (Train Name, time taken to reach the package, capacity)
                var timeTakenForTrain = new List<Tuple<string, int, int>> { };

                foreach (var train in trains.Where(x => x.Selected == false))
                {
                    // Look for the nearest train which have enough capacity where is not selected yet
                    List<PathSegment> paths = graphNodes.FindPath(train.StartingNode, package.StartingNode);
                    timeTakenForTrain.Add(Tuple.Create(train.Name, paths.Sum(x => x.Time), train.Capacity));
                }

                // Filter where train that have enough capacity with the shortest time
                var eligibleTrain = timeTakenForTrain.Where(x => x.Item3 >= package.Capacity).OrderBy(x => x.Item2).FirstOrDefault();

                if (eligibleTrain != null)
                {
                    var selectedTrain = trains.FirstOrDefault(x => x.Name == eligibleTrain.Item1);

                    if (selectedTrain != null)
                    {
                        selectedTrain.Selected = true;
                        assignSchedules.Add(selectedTrain, package);
                    }
                }
            }
           
            var moves = ResolveTrainPath(assignSchedules, graphNodes);

            if (moves.Count > 0)
            {
                var groupedMoves = moves.GroupBy(x => x.Train);

                foreach (var groupedMove in groupedMoves)
                {
                    Console.WriteLine($"Train Name: {groupedMove.Key}");

                    foreach (var move in groupedMove)
                    {
                        Console.WriteLine("W={0}, T={1}, N1={2}, P1={3}, N2={4}, P2={5}, JourneyTime={6}",
                            move.Time, move.Train, move.Node1, string.Join(",", move.Packages1), move.Node2, string.Join(",", move.Packages2), move.JourneyTime);
                    }

                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("No moves found.");
            }
            
            Console.ReadLine();
        }

        private static List<TrainMoves> ResolveTrainPath(Dictionary<Trains, Packages> schedules, TrainNodes graphNodes)
        {
            var moves = new List<TrainMoves>();

            foreach (var schedule in schedules)
            {
                var train = schedule.Key;
                var package = schedule.Value;
                var journeyTime = 0;

                // Fetch package
                if (train.StartingNode != package.StartingNode)
                {
                    // Find the path to pick up the package
                    List<PathSegment> paths = graphNodes.FindPath(train.StartingNode, package.StartingNode);

                    if (paths.Count <= 0)
                        continue;

                    foreach (var path in paths)
                    {
                        journeyTime += path.Time;
                        // Add new moves
                        moves.Add(new TrainMoves(path.Time, train.Name, path.StartNode, new List<string> { }, path.EndNode, new List<string> { }, journeyTime));
                    }
                }

                // Packaged load on the train
                // Train will start from the station where it picks the package
                train.StartingNode = package.StartingNode;

                // After package load, find the route to the destination
                List<PathSegment> destinationPaths = graphNodes.FindPath(train.StartingNode, package.DestinationNode);

                foreach (var destinationPath in destinationPaths)
                {
                    if (destinationPath.EndNode != package.DestinationNode)
                    {
                        journeyTime += destinationPath.Time;

                        // Have not reach package destination
                        moves.Add(new TrainMoves(destinationPath.Time, train.Name, destinationPath.StartNode, new List<string> { package.Name }, destinationPath.EndNode, new List<string>() { }, journeyTime));
                    }
                    else
                    {
                        journeyTime += destinationPath.Time;

                        // Reached package destination
                        moves.Add(new TrainMoves(destinationPath.Time, train.Name, destinationPath.StartNode, new List<string> { }, destinationPath.EndNode, new List<string>() { package.Name }, journeyTime));
                    }
                }
            }
           
            return moves;
        }

        private class Edges
        {
            public string Name { get; set; }
            public string Node1 { get; set; }
            public string Node2 { get; set; }
            public int JourneyTime { get; set; }

            public Edges(string name, string node1, string node2, int journeyTime)
            {
                Name = name;
                Node1 = node1;
                Node2 = node2;
                JourneyTime = journeyTime;
            }
        }

        private class Trains
        {
            public string Name { get; set; }
            public int Capacity { get; set; }
            public string StartingNode { get; set; }
            public bool Selected { get; set; }

            public Trains(string name, int capacity, string startingNode, bool selected)
            {
                Name = name;
                Capacity = capacity;
                StartingNode = startingNode;
                Selected = selected;
            }
        }

        private class Packages
        {
            public string Name { get; set; }
            public int Capacity { get; set; }
            public string StartingNode { get; set; }
            public string DestinationNode { get; set; }

            public Packages(string name, int capacity, string startingNode, string destinationNode)
            {
                Name = name;
                Capacity = capacity;
                StartingNode = startingNode;
                DestinationNode = destinationNode;
            }
        }

        private class TrainMoves
        {
            public int Time { get; set; }
            public string Train { get; set; }
            public string Node1 { get; set; }
            public List<string> Packages1 { get; set; }
            public string Node2 { get; set; }
            public List<string> Packages2 { get; set; }
            public int JourneyTime { get; set; }

            public TrainMoves(int time, string train, string node1, List<string> packages1, string node2, List<string> packages2, int journeyTime)
            {
                Time = time;
                Train = train;
                Node1 = node1;
                Packages1 = packages1;
                Node2 = node2;
                Packages2 = packages2;
                JourneyTime = journeyTime;
            }
        }
    }
}

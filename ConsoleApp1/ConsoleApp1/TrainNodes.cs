namespace ConsoleApp1
{
    public class TrainNodes
    {
        private Dictionary<string, List<Edge>> adjacencyList;

        public TrainNodes()
        {
            adjacencyList = new Dictionary<string, List<Edge>>();
        }

        public void AddNode(string node)
        {
            if (!adjacencyList.ContainsKey(node))
            {
                adjacencyList[node] = new List<Edge>();
            }
        }

        public void AddEdge(string node1, string node2, int time)
        {
            if (adjacencyList.ContainsKey(node1) && adjacencyList.ContainsKey(node2))
            {
                adjacencyList[node1].Add(new Edge(node1, node2, time));
                adjacencyList[node2].Add(new Edge(node2, node1, time));
            }
        }

        public List<PathSegment> FindPath(string startNode, string endNode)
        {
            HashSet<string> visited = new();
            Dictionary<string, string> parent = new();
            Dictionary<string, int> timeTaken = new();
            List<PathSegment> pathSegments = new();

            if (!adjacencyList.ContainsKey(startNode) || !adjacencyList.ContainsKey(endNode))
            {
                return pathSegments;
            }

            DFS(startNode, endNode, visited, parent, timeTaken, pathSegments);

            if (!parent.ContainsKey(endNode))
            {
                // No path found
                return pathSegments;
            }

            // Reconstruct the path from endNode to startNode
            string currentNode = endNode;
            while (currentNode != startNode)
            {
                string parentNode = parent[currentNode];
                int time = timeTaken[currentNode];
                pathSegments.Insert(0, new PathSegment(parentNode, currentNode, time));
                currentNode = parentNode;
            }

            return pathSegments;
        }

        private void DFS(string currentNode, string endNode, HashSet<string> visited, Dictionary<string, string> parent, Dictionary<string, int> timeTaken, List<PathSegment> pathSegments)
        {
            visited.Add(currentNode);

            if (currentNode == endNode)
            {
                return;
            }

            foreach (Edge edge in adjacencyList[currentNode])
            {
                if (!visited.Contains(edge.Node2))
                {
                    parent[edge.Node2] = currentNode;
                    timeTaken[edge.Node2] = edge.Time;
                    DFS(edge.Node2, endNode, visited, parent, timeTaken, pathSegments);
                }
                else if (edge.Node2 == endNode && !parent.ContainsKey(endNode))
                {
                    // If the destination node is already visited and it's the end node,
                    // add the path segment only if it's not present in the parent dictionary
                    pathSegments.Add(new PathSegment(currentNode, edge.Node2, edge.Time));
                }
            }
        }

        private class Edge
        {
            public string Node1 { get; }
            public string Node2 { get; }
            public int Time { get; }

            public Edge(string node1, string node2, int time)
            {
                Node1 = node1;
                Node2 = node2;
                Time = time;
            }
        }
    }

    public class PathSegment
    {
        public string StartNode { get; }
        public string EndNode { get; }
        public int Time { get; }

        public PathSegment(string startNode, string endNode, int time)
        {
            StartNode = startNode;
            EndNode = endNode;
            Time = time;
        }
    }
}

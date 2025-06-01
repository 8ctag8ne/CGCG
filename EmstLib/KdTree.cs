
namespace EmstLib
{
    public class KDTree
    {
        private readonly KDNode _root;
        public int Count { get; }

        public KDTree(IReadOnlyList<Point> points)
        {
            Count = points.Count;
            _root = BuildTree(points, 0);
        }

        private KDNode BuildTree(IReadOnlyList<Point> points, int depth)
        {
            if (points == null || points.Count == 0)
                return null;

            int axis = depth % 2;
            int medianIndex = points.Count / 2;

            // Сортування по поточній осі
            var sortedPoints = axis == 0 
                ? points.OrderBy(p => p.X).ToList() 
                : points.OrderBy(p => p.Y).ToList();

            return new KDNode(
                point: sortedPoints[medianIndex],
                left: BuildTree(sortedPoints.Take(medianIndex).ToList(), depth + 1),
                right: BuildTree(sortedPoints.Skip(medianIndex + 1).ToList(), depth + 1),
                depth: depth
            );
        }

        // Повертає ребра для всіх точок з k найближчими сусідами
        public HashSet<Edge> GetAllEdges(int k)
        {
            var edges = new HashSet<Edge>();
            var allPoints = new List<Point>();
            CollectPoints(_root, allPoints);

            foreach (var point in allPoints)
            {
                var neighbors = KNearestNeighbors(point, k + 1); // +1 включає саму точку
                foreach (var neighbor in neighbors)
                {
                    if (point.Equals(neighbor)) continue;
                    edges.Add(new Edge(point, neighbor));
                }
            }
            return edges;
        }

        public List<Point> KNearestNeighbors(Point target, int k)
        {
            var neighbors = new NearestNeighbors(k);
            SearchNN(_root, target, neighbors);
            return neighbors.GetPoints();
        }

        private void SearchNN(KDNode node, Point target, NearestNeighbors neighbors)
        {
            if (node == null) return;

            double distSq = node.Point.DistanceSquaredTo(target);
            neighbors.AddCandidate(node.Point, distSq);

            int axis = node.Depth % 2;
            double targetValue = axis == 0 ? target.X : target.Y;
            double nodeValue = axis == 0 ? node.Point.X : node.Point.Y;

            // Визначаємо "ближчу" та "дальню" гілки
            KDNode nearBranch = targetValue < nodeValue ? node.Left : node.Right;
            KDNode farBranch = targetValue < nodeValue ? node.Right : node.Left;

            SearchNN(nearBranch, target, neighbors);

            // Перевіряємо чи потрібно шукати у "дальній" гілці
            double planeDist = targetValue - nodeValue;
            double planeDistSq = planeDist * planeDist;
            
            if (neighbors.CanAdd(planeDistSq))
            {
                SearchNN(farBranch, target, neighbors);
            }
        }

        private void CollectPoints(KDNode node, List<Point> points)
        {
            if (node == null) return;
            points.Add(node.Point);
            CollectPoints(node.Left, points);
            CollectPoints(node.Right, points);
        }

        private class KDNode
        {
            public Point Point { get; }
            public KDNode Left { get; }
            public KDNode Right { get; }
            public int Depth { get; }

            public KDNode(Point point, KDNode left, KDNode right, int depth)
            {
                Point = point;
                Left = left;
                Right = right;
                Depth = depth;
            }
        }

        // Допоміжний клас для відстеження k найближчих сусідів
        private class NearestNeighbors
        {
            private readonly List<(Point Point, double DistSq)> _candidates = new();
            public double MaxDistSq { get; private set; } = double.PositiveInfinity;
            public int K { get; }

            public NearestNeighbors(int k)
            {
                K = k;
            }

            public void AddCandidate(Point point, double distSq)
            {
                // Додаємо кандидата, якщо ще не маємо достатньо сусідів
                // або поточний кандидат кращий за існуючих
                if (_candidates.Count < K || distSq < MaxDistSq)
                {
                    _candidates.Add((point, distSq));
                    
                    // Якщо перевищили ліміт - видаляємо найгіршого кандидата
                    if (_candidates.Count > K)
                    {
                        RemoveWorstCandidate();
                    }
                    UpdateMaxDistance();
                }
            }

            public bool CanAdd(double planeDistSq)
            {
                // Перевіряємо чи потенційно може знайтись кращий кандидат
                return _candidates.Count < K || planeDistSq < MaxDistSq;
            }

            public List<Point> GetPoints() => _candidates.Select(c => c.Point).ToList();

            private void RemoveWorstCandidate()
            {
                if (_candidates.Count == 0) return;

                int worstIndex = 0;
                double maxDist = _candidates[0].DistSq;

                for (int i = 1; i < _candidates.Count; i++)
                {
                    if (_candidates[i].DistSq > maxDist)
                    {
                        maxDist = _candidates[i].DistSq;
                        worstIndex = i;
                    }
                }
                _candidates.RemoveAt(worstIndex);
            }

            private void UpdateMaxDistance()
            {
                MaxDistSq = _candidates.Count == 0 
                    ? double.PositiveInfinity 
                    : _candidates.Max(c => c.DistSq);
            }
        }
    }
}
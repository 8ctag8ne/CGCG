using KdTree;

namespace EmstLib;

public static class KruskalAlgorithm
{
    public static List<Edge> BuildEmst(List<Point> points)
    {
        var kdTree = new KDTree(points);
        HashSet<Edge> edges = kdTree.GetAllEdges(k: Math.Min(points.Count, 6));
        return FindMst(edges, points);
    }

    public static List<Edge> BuildEmstLibFunc(List<Point> points)
    {
        var tree = new KdTree<double, Point>(dimensions: 2, new KdTree.Math.DoubleMath());

        // Додаємо точки в KD-дерево
        foreach (var p in points)
        {
            tree.Add(new[] { p.X, p.Y }, p);
        }

        // Формуємо множину ребер за k найближчими сусідами
        var edgeSet = new HashSet<Edge>();

        foreach (var point in points)
        {
            var neighbors = tree.GetNearestNeighbours(new[] { point.X, point.Y }, count: Math.Min(points.Count, 6));

            foreach (var neighbor in neighbors)
            {
                var q = neighbor.Value;

                if (!point.Equals(q)) // захист від ребра до себе
                {
                    var edge = new Edge(point, q);
                    edgeSet.Add(edge);
                }
            }
        }

        return FindMst(edgeSet, points);
    }
    public static List<Edge> FindMst(IEnumerable<Edge> edges, List<Point> points)
    {
        var sortedEdges = edges.ToList();
        sortedEdges.Sort();

        var pointIndex = new Dictionary<Point, int>();
        for (int i = 0; i < points.Count; i++)
            pointIndex[points[i]] = i;

        var dsu = new DSU(points.Count);
        var mst = new List<Edge>();

        foreach (var edge in sortedEdges)
        {
            int u = pointIndex[edge.A];
            int v = pointIndex[edge.B];

            if (dsu.Union(u, v))
                mst.Add(edge);
        }

        return mst;
    }
}
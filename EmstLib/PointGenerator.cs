namespace EmstLib
{
    public static class PointGenerator
    {
        public static List<Point> GenerateRandomPoints(
            int count,
            double xMin,
            double yMin,
            double xMax,
            double yMax,
            int? seed = null)
        {
            var random = seed.HasValue ? new Random(seed.Value) : new Random();
            var points = new HashSet<Point>();

            while (points.Count < count)
            {
                double x = xMin + (xMax - xMin) * random.NextDouble();
                double y = yMin + (yMax - yMin) * random.NextDouble();
                points.Add(new Point(x, y));
            }

            return points.ToList();
        }
    }
}
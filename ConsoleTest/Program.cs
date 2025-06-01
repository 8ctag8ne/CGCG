using EmstLib;
var points = new List<Point>
{
    new Point(0, 0),
    new Point(1, 2),
    new Point(3, 1),
    new Point(4, 3)
};

var points3 = new List<Point>
{
    // Лівий квадрат
    new Point(0, 0),
    new Point(1, 0),
    new Point(0, 1),
    new Point(1, 1),
    
    // Правий квадрат
    new Point(2, 0),
    new Point(3, 0)
};

var colinear = new List<Point>
{
    // Лівий квадрат
    new Point(0, 0),
    new Point(1, 1),
    new Point(2, 2),
    new Point(3, 3),
    new Point(4, 4),
    new Point(5, 5),
    new Point(6, 6),
    
    // Правий квадрат
    // new Point(2, 0),
    // new Point(3, 0)
};

var points5 = new List<Point>
{
    new Point(0, 0),    // Центр
    new Point(1, 0),    // 0°
    new Point(0.707, 0.707),  // 45°
    new Point(0, 1),    // 90°
    new Point(-0.707, 0.707), // 135°
    new Point(-1, 0),   // 180°
    new Point(-0.707, -0.707),// 225°
    new Point(0, -1)    // 270°
};

var ultimate = new List<Point>
{
    new Point(1, 1),
    new Point(1, 2),
    new Point(1, 4),
    new Point(4, 2),
    new Point(4, 4),
    new Point(4, 7),
    new Point(7, 3),
    new Point(7, 5),
    new Point(7, 6),
};

var testMax = PointGenerator.GenerateRandomPoints(500000, 0, 0, 100000, 100000, 1488);
var startTime = DateTime.Now;
var emst = KruskalAlgorithm.BuildEmst(testMax);
var endTime = DateTime.Now;

// foreach (var edge in emst)
// {
//     Console.WriteLine($"({edge.A.X},{edge.A.Y}) - ({edge.B.X},{edge.B.Y})");
// }
Console.WriteLine(endTime - startTime);
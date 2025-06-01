using EmstLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EmstApp;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private int _mstEdgeIndex = 0;

    private double _settingsDisplayTime = 0;
    private const double SettingsDisplayDuration = 3.0;
    private readonly List<int> _pointLevels = new() { 100, 200, 500, 1000, 2000, 5000, 10000 };
    private int _currentLevelIndex = 0;

    private int _pointCount = 50;
    private double _spreadFactor = 1.0;
    private bool _showSettings = false;

    private MouseState _prevMouse;
    private KeyboardState _prevKeyboard;

    private List<EmstLib.Point> _points = new();
    private List<Edge> _edges = new();
    private List<Edge> _allEdges = new();
    private List<Edge> _mstEdges = new();

    private int _currentEdgeIndex = 0;
    private double _edgeDrawTimer = 0;
    private double _edgeDrawInterval = 0.001;
    private bool _emstBuilt = false;

    private Texture2D _pointTexture;
    private Texture2D _pixel;

    // Додано поля для керування камерою
    private float _zoom = 1.0f;
    private Vector2 _offset = Vector2.Zero;
    private bool _isDragging = false;
    private Vector2 _dragStart;
    private Vector2 _dragStartOffset;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _currentLevelIndex = 0; 
        _pointCount = _pointLevels[_currentLevelIndex];
        GeneratePoints();
        base.Initialize();
    }

    private void GeneratePoints()
    {
        double centerX = 400;
        double centerY = 300;
        double baseWidth = 700;
        double baseHeight = 500;
        double actualWidth = baseWidth * _spreadFactor;
        double actualHeight = baseHeight * _spreadFactor;
        
        _points = PointGenerator.GenerateRandomPoints(
            _pointCount,
            centerX - actualWidth / 2,
            centerY - actualHeight / 2,
            centerX + actualWidth / 2,
            centerY + actualHeight / 2
        );
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        _pointTexture = CreateCircleTexture(6, Color.Red);
    }

    protected override void Update(GameTime gameTime)
    {
        var mouse = Mouse.GetState();
        var keyboard = Keyboard.GetState();

        // Перетворення позиції миші у світові координати
        var worldMousePos = ScreenToWorld(new Vector2(mouse.X, mouse.Y));

        // Додавання точки при кліці
        if (mouse.LeftButton == ButtonState.Pressed && 
            _prevMouse.LeftButton == ButtonState.Released &&
            !keyboard.IsKeyDown(Keys.Space))
        {
            var pos = new EmstLib.Point(worldMousePos.X, worldMousePos.Y);
            if (!_points.Any(p => p.X == pos.X && p.Y == pos.Y))
            {
                _points.Add(pos);
                _emstBuilt = false;
            }
        }

        // Керування камерою
        HandleCameraControl(mouse, keyboard);

        if (keyboard.IsKeyDown(Keys.C) && !_prevKeyboard.IsKeyDown(Keys.C))
        {
            _points.Clear();
            _edges.Clear();
            _allEdges.Clear();
            _mstEdges.Clear();
            _emstBuilt = false;
        }

        if (keyboard.IsKeyDown(Keys.G) && !_prevKeyboard.IsKeyDown(Keys.G))
        {
            _points = PointGenerator.GenerateRandomPoints(
                count: 100, xMin: 50, yMin: 50, xMax: 750, yMax: 550);
            _edges.Clear();
            _allEdges.Clear();
            _mstEdges.Clear();
            _emstBuilt = false;
        }

        if (keyboard.IsKeyDown(Keys.M) && !_prevKeyboard.IsKeyDown(Keys.M))
        {
            var kdTree = new KDTree(_points);
            _allEdges = kdTree.GetAllEdges(k: Math.Min(_points.Count, 6)).ToList();
            _allEdges.Sort();
            _mstEdges = KruskalAlgorithm.FindMst(_allEdges, _points);
            _mstEdgeIndex = 0;

            _edges.Clear();
            _currentEdgeIndex = 0;
            _edgeDrawTimer = 0;
            _emstBuilt = true;
        }

        // Анімація: покрокове додавання ребер
        if (_emstBuilt && _currentEdgeIndex < _allEdges.Count && _mstEdgeIndex < _mstEdges.Count)
        {
            _edgeDrawTimer += gameTime.ElapsedGameTime.TotalSeconds;
            if (_edgeDrawTimer >= _edgeDrawInterval)
            {
                var currentEdge = _allEdges[_currentEdgeIndex];

                if (_mstEdgeIndex < _mstEdges.Count && currentEdge.Equals(_mstEdges[_mstEdgeIndex]))
                {
                    _edges.Add(currentEdge);
                    _mstEdgeIndex++;
                }

                _currentEdgeIndex++;
                _edgeDrawTimer = 0;
            }

            if (_mstEdgeIndex >= _mstEdges.Count)
            {
                _emstBuilt = false;
            }
        }

        // Зміна кількості точок (фіксовані рівні)
        if (keyboard.IsKeyDown(Keys.Up) && !_prevKeyboard.IsKeyDown(Keys.Up))
        {
            _currentLevelIndex = Math.Min(_currentLevelIndex + 1, _pointLevels.Count - 1);
            _pointCount = _pointLevels[_currentLevelIndex];
            _showSettings = true;
            _settingsDisplayTime = 0;
        }
        if (keyboard.IsKeyDown(Keys.Down) && !_prevKeyboard.IsKeyDown(Keys.Down))
        {
            _currentLevelIndex = Math.Max(_currentLevelIndex - 1, 0);
            _pointCount = _pointLevels[_currentLevelIndex];
            _showSettings = true;
            _settingsDisplayTime = 0;
        }

        // Зміна розкиду точок
        if (keyboard.IsKeyDown(Keys.OemPlus) && !_prevKeyboard.IsKeyDown(Keys.OemPlus))
        {
            _spreadFactor = Math.Min(_spreadFactor * 1.1, 8.0);
            _showSettings = true;
            _settingsDisplayTime = 0;
        }
        if (keyboard.IsKeyDown(Keys.OemMinus) && !_prevKeyboard.IsKeyDown(Keys.OemMinus))
        {
            _spreadFactor = Math.Max(_spreadFactor * 0.9, 0.1);
            _showSettings = true;
            _settingsDisplayTime = 0;
        }

        // Генерація точок з новими параметрами
        if (keyboard.IsKeyDown(Keys.G) && !_prevKeyboard.IsKeyDown(Keys.G))
        {
            GeneratePoints();
            _edges.Clear();
            _allEdges.Clear();
            _mstEdges.Clear();
            _emstBuilt = false;
        }

        // Скидання параметрів
        if (keyboard.IsKeyDown(Keys.R) && !_prevKeyboard.IsKeyDown(Keys.R) && !keyboard.IsKeyDown(Keys.Space))
        {
            _currentLevelIndex = 0;
            _pointCount = _pointLevels[_currentLevelIndex];
            _spreadFactor = 1.0;
            _zoom = 1.0f;
            _offset = Vector2.Zero;
            _showSettings = true;
            _settingsDisplayTime = 0;
            // GeneratePoints();
        }

        // Таймер відображення налаштувань
        if (_showSettings)
        {
            _settingsDisplayTime += gameTime.ElapsedGameTime.TotalSeconds;
            if (_settingsDisplayTime >= SettingsDisplayDuration)
            {
                _showSettings = false;
            }
        }
        // Генерація точок з новими параметрами
        if (keyboard.IsKeyDown(Keys.G) && !_prevKeyboard.IsKeyDown(Keys.G))
        {
            GeneratePoints();
            _edges.Clear();
            _allEdges.Clear();
            _mstEdges.Clear();
            _emstBuilt = false;
        }

        _prevMouse = mouse;
        _prevKeyboard = keyboard;

        base.Update(gameTime);
    }

    // Обробка масштабування та перетягування камери
    private void HandleCameraControl(MouseState mouse, KeyboardState keyboard)
    {
        // Масштабування колесом миші
        int scrollDelta = mouse.ScrollWheelValue - _prevMouse.ScrollWheelValue;
        if (scrollDelta != 0)
        {
            float zoomChange = scrollDelta * 0.001f;
            _zoom = MathHelper.Clamp(_zoom + zoomChange, 0.1f, 5.0f);
        }

        // Перетягування камери (пробіл + ЛКМ)
        if (keyboard.IsKeyDown(Keys.Space) && mouse.LeftButton == ButtonState.Pressed)
        {
            if (!_isDragging)
            {
                _isDragging = true;
                _dragStart = new Vector2(mouse.X, mouse.Y);
                _dragStartOffset = _offset;
            }
            else
            {
                Vector2 currentPos = new Vector2(mouse.X, mouse.Y);
                Vector2 delta = (currentPos - _dragStart) / _zoom;
                _offset = _dragStartOffset - delta;
            }
        }
        else
        {
            _isDragging = false;
        }

        // Скидання камери (клавіша R)
        if (keyboard.IsKeyDown(Keys.R) && !_prevKeyboard.IsKeyDown(Keys.R))
        {
            _zoom = 1.0f;
            _offset = Vector2.Zero;
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        // Застосовуємо трансформації камери
        Matrix transform = Matrix.CreateTranslation(-_offset.X, -_offset.Y, 0) * 
                           Matrix.CreateScale(_zoom);
        
        _spriteBatch.Begin(transformMatrix: transform);

        // Поточне ребро - червоне
        if (_emstBuilt && _currentEdgeIndex < _allEdges.Count)
        {
            var current = _allEdges[_currentEdgeIndex];
            DrawLine(current.A.ToVector2(), current.B.ToVector2(), Color.Red, 2);
        }

        // MST ребра - зелені
        foreach (var edge in _edges)
        {
            DrawLine(edge.A.ToVector2(), edge.B.ToVector2(), Color.LimeGreen, 2);
        }

        // Точки
        foreach (var p in _points)
        {
            var pos = p.ToVector2();
            _spriteBatch.Draw(_pointTexture, pos - new Vector2(6, 6), Color.Red);
        }
        
        double centerX = 400;
        double centerY = 300;
        double baseWidth = 700;
        double baseHeight = 500;
        double actualWidth = baseWidth * _spreadFactor;
        double actualHeight = baseHeight * _spreadFactor;
        
        Vector2 topLeft = new Vector2((float)(centerX - actualWidth / 2), (float)(centerY - actualHeight / 2));
        Vector2 topRight = new Vector2((float)(centerX + actualWidth / 2), (float)(centerY - actualHeight / 2));
        Vector2 bottomRight = new Vector2((float)(centerX + actualWidth / 2), (float)(centerY + actualHeight / 2));
        Vector2 bottomLeft = new Vector2((float)(centerX - actualWidth / 2), (float)(centerY + actualHeight / 2));

        // Товщина ліній = 2 пікселі (не залежить від масштабу)
        int thickness = (int)Math.Max(1, 2 / _zoom);
        DrawLine(topLeft, topRight, Color.White, thickness);
        DrawLine(topRight, bottomRight, Color.White, thickness);
        DrawLine(bottomRight, bottomLeft, Color.White, thickness);
        DrawLine(bottomLeft, topLeft, Color.White, thickness);

        _spriteBatch.End();

        // Малювання налаштувань У ЕКРАННИХ КООРДИНАТАХ (верхній лівий кут)
        _spriteBatch.Begin();
        
        if (_showSettings)
        {
            Vector2 settingsPos = new Vector2(10, 10);
            
            // Відображення рівня точок
            for (int i = 0; i <= _currentLevelIndex; i++)
            {
                _spriteBatch.Draw(_pixel, 
                    new Rectangle((int)settingsPos.X + i * 15, (int)settingsPos.Y, 12, 12), 
                    Color.Yellow);
            }
            
            // Відображення розкиду
            int spreadBarWidth = (int)(_spreadFactor * 20); // 20 пікселів на одиницю розкиду
            _spriteBatch.Draw(_pixel, 
                new Rectangle((int)settingsPos.X, (int)settingsPos.Y + 20, spreadBarWidth, 8), 
                Color.Green);
            
            // Рамка для розкиду (100 пікселів = max 5.0)
            _spriteBatch.Draw(_pixel, 
                new Rectangle((int)settingsPos.X, (int)settingsPos.Y + 20, 100, 8), 
                Color.White);
        }
        
        _spriteBatch.End();
        
        base.Draw(gameTime);
    }

    private void DrawLine(Vector2 start, Vector2 end, Color color, int thickness)
    {
        Vector2 edge = end - start;
        float angle = (float)Math.Atan2(edge.Y, edge.X);
        _spriteBatch.Draw(_pixel, new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), thickness),
            null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
    }

    private Texture2D CreateCircleTexture(int radius, Color color)
    {
        int diameter = radius * 2;
        Texture2D texture = new Texture2D(GraphicsDevice, diameter, diameter);
        Color[] colorData = new Color[diameter * diameter];

        for (int y = 0; y < diameter; y++)
        {
            for (int x = 0; x < diameter; x++)
            {
                int dx = x - radius;
                int dy = y - radius;
                if (dx * dx + dy * dy <= radius * radius)
                    colorData[y * diameter + x] = color;
                else
                    colorData[y * diameter + x] = Color.Transparent;
            }
        }

        texture.SetData(colorData);
        return texture;
    }

    // Перетворення екранних координат у світові
    private Vector2 ScreenToWorld(Vector2 screenPosition)
    {
        return screenPosition / _zoom + _offset;
    }
}
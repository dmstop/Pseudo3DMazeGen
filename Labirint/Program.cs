using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MazeGenerator;

namespace Labirint
{
    class Program
    {
        private static readonly Maze MazeGenerator = new Maze();
        
        private const int ScreenWidth = 50;
        private const int ScreenHeight = 30;

        private const int MazeHeight = 8;
        private const int MazeWidth = 8;

        private const double Depth = 16;
        private const double Fov = Math.PI / 3.5;

        private static double _playerX = 1.5;
        private static double _playerY = 1.5;
        private static double _playerA = 0;

        private const char MapWall = '#';
        private const char MapEmpty = '.';

        private static string _map = "";
        private static int _mapHeight = MazeHeight * 3 + 1;
        private static int _mapWidth = MazeWidth * 3 + 1;

        static void Main()
        {
            Console.SetWindowSize(ScreenWidth, ScreenHeight);
            Console.SetBufferSize(ScreenWidth, ScreenHeight);
            Console.CursorVisible = false;

            Start();
        }

        static void Start()
        {
            _playerX = 1.5;
            _playerY = 1.5;
            _playerA = 0;

            InitMap();

            var screen = new char[ScreenWidth * ScreenHeight];

            DateTime dateTimeFrom = DateTime.Now;

            while (true)
            {
                var dateTimeTo = DateTime.Now;
                double elapsedTime = (dateTimeTo - dateTimeFrom).TotalSeconds;
                dateTimeFrom = dateTimeTo;

                CheckControls(elapsedTime);

                for (int x = 0; x < ScreenWidth; x++)
                {
                    double rayAngle = (_playerA - Fov / 2) + x * Fov / ScreenWidth;

                    double rayX = Math.Cos(rayAngle);
                    double rayY = Math.Sin(rayAngle);

                    double distanceToWall = 0;
                    bool hitWall = false;
                    bool isBound = false;

                    while (!hitWall && distanceToWall < Depth)
                    {
                        distanceToWall += 0.1;

                        int testX = (int)(_playerX + rayX * distanceToWall);
                        int testY = (int)(_playerY + rayY * distanceToWall);

                        if (testX < 0 || testX >= Depth + _playerX || testY < 0 || testY >= Depth + _playerY)
                        {
                            hitWall = true;
                            distanceToWall = Depth;
                        }
                        else
                        {
                            char testCell = _map[testY * _mapWidth + testX];

                            if (testCell == '#')
                            {
                                hitWall = true;

                                distanceToWall = distanceToWall * Math.Cos(rayAngle - _playerA);

                                var boundsVectorsList = new List<(double X, double Y)>();

                                for (int tx = 0; tx < 2; tx++)
                                {
                                    for (int ty = 0; ty < 2; ty++)
                                    {
                                        double vx = testX + tx - _playerX;
                                        double vy = testY + ty - _playerY;

                                        double vectorModule = Math.Sqrt(vx * vx + vy * vy);
                                        double cosAngle = (rayX * vx / vectorModule) + (rayY * vy / vectorModule);
                                        boundsVectorsList.Add((vectorModule, cosAngle));
                                    }
                                }

                                boundsVectorsList = boundsVectorsList.OrderBy(v => v.X).ToList();

                                double boundAngle = 0.03 / distanceToWall;

                                if (Math.Acos(boundsVectorsList[0].Y) < boundAngle ||
                                    Math.Acos(boundsVectorsList[1].Y) < boundAngle)
                                    isBound = true;
                            }
                        }
                    }

                    int ceiling = (int)(ScreenHeight / 2.0 - ScreenHeight * Fov / distanceToWall);
                    int floor = ScreenHeight - ceiling;

                    ceiling += ScreenHeight - ScreenHeight;

                    char wallShade;

                    if (isBound)
                        wallShade = '|';
                    else if (distanceToWall <= Depth / 4.0)
                        wallShade = '\u2588';
                    else if (distanceToWall < Depth / 3.0)
                        wallShade = '\u2593';
                    else if (distanceToWall < Depth / 2.0)
                        wallShade = '\u2592';
                    else if (distanceToWall < Depth)
                        wallShade = '\u2591';
                    else
                        wallShade = ' ';

                    for (int y = 0; y < ScreenHeight; y++)
                    {
                        if (y < ceiling)
                            screen[y * ScreenWidth + x] = ' ';
                        else if (y > ceiling && y <= floor)
                            screen[y * ScreenWidth + x] = wallShade;
                        else
                        {
                            char floorShade;
                            double b = 1.0 - (y - ScreenHeight / 2.0) / (ScreenHeight / 2.0);

                            if (b < 0.25)
                                floorShade = '#';
                            else if (b < 0.5)
                                floorShade = 'x';
                            else if (b < 0.75)
                                floorShade = '-';
                            else if (b < 0.9)
                                floorShade = '.';
                            else
                                floorShade = ' ';

                            screen[y * ScreenWidth + x] = floorShade;
                        }
                    }
                }

                //Stats
                char[] stats = $"X: {_playerX}, Y: {_playerY}, A: {_playerA}, FPS: {(int)(1 / elapsedTime)}"
                    .ToCharArray();
                stats.CopyTo(screen, 0);

                //Map
                for (int x = 0; x < _mapWidth; x++)
                {
                    for (int y = 0; y < _mapHeight; y++)
                    {
                        screen[(y + 1) * ScreenWidth + x] = _map[y * _mapWidth + x];
                    }
                }

                screen[(int)(_playerY + 1) * ScreenWidth + (int)_playerX] = 'P';

                Console.SetCursorPosition(0, 0);
                Console.Write(screen, 0, ScreenWidth * ScreenHeight);
            }
        }

        static void CheckControls(double elapsedTime)
        {
            if (Console.KeyAvailable)
            {
                ConsoleKey consoleKey = Console.ReadKey(true).Key;

                switch (consoleKey)
                {
                    case ConsoleKey.A:
                        _playerA -= elapsedTime * 20;
                        break;
                    case ConsoleKey.D:
                        _playerA += elapsedTime * 20;
                        break;
                    case ConsoleKey.W:
                        {
                            _playerX += Math.Cos(_playerA) * 60 * elapsedTime;
                            _playerY += Math.Sin(_playerA) * 60 * elapsedTime;

                            if (_map[(int)_playerY * _mapWidth + (int)_playerX] == '#')
                            {
                                _playerX -= Math.Cos(_playerA) * 60 * elapsedTime;
                                _playerY -= Math.Sin(_playerA) * 60 * elapsedTime;
                            }

                            break;
                        }

                    case ConsoleKey.S:
                        {
                            _playerX -= Math.Cos(_playerA) * 60 * elapsedTime;
                            _playerY -= Math.Sin(_playerA) * 60 * elapsedTime;

                            if (_map[(int)_playerY * _mapWidth + (int)_playerX] == '#')
                            {
                                _playerX += Math.Cos(_playerA) * 60 * elapsedTime;
                                _playerY += Math.Sin(_playerA) * 60 * elapsedTime;
                            }

                            break;
                        }

                    case ConsoleKey.Spacebar:
                    {
                        Start();
                        break;
                    }
                }
            }
        }

        static void InitMap()
        {
            StringBuilder sb = new StringBuilder();

            char[,] map = MazeGenerator.GenerateCharMap(MazeWidth, MazeHeight, MapWall, MapEmpty);

            for (int y = 0; y < map.GetLength(1); y++)
            {
                for (int x = 0; x < map.GetLength(0); x++)
                {
                    sb.Append(map[x, y]);
                }
            }

            _map = sb.ToString();
        }
    }
}

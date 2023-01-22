using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BigMath;

namespace ExprObjModel
{
    public interface IConsole
    {
        void Clear();
        void Write(string str);
        void WriteLine(string str);
        void WriteLine();
        string ReadLine();
        bool CanSetSize { get; }
        void MoveTo(int x, int y);
        void SetSize(int x, int y);
        int Width { get; }
        int Height { get; }
        object ReadKey();
        void SetColor(int x, int y);
        Tuple<int, int> GetColor();
    }

    public class PlainConsole : IConsole
    {
        public PlainConsole() { }

        public void Clear()
        {
            Console.Clear();
        }

        public void Write(string str)
        {
            Console.Write(str);
        }

        public void WriteLine(string str)
        {
            Console.WriteLine(str);
        }

        public void WriteLine()
        {
            Console.WriteLine();
        }

        public string ReadLine()
        {
            return Console.ReadLine();
        }

        public bool CanSetSize { get { return true; } }

        public void MoveTo(int x, int y)
        {
            Console.SetCursorPosition(x, y);
        }

        public void SetSize(int x, int y)
        {
            Console.Clear();

            bool shrinkWindow = false;

            int windowWidth = Console.WindowWidth;
            int windowHeight = Console.WindowHeight;
            int windowWidth2 = windowWidth;
            int windowHeight2 = windowHeight;
            if (x < windowWidth)
            {
                shrinkWindow = true;
                windowWidth2 = x;
            }
            if (y < windowHeight)
            {
                shrinkWindow = true;
                windowHeight2 = y;
            }
            if (shrinkWindow) Console.SetWindowSize(windowWidth2, windowHeight2);

            Console.SetBufferSize(x, y);

            Console.SetWindowSize
            (
                Math.Min(x, Console.LargestWindowWidth),
                Math.Min(y, Console.LargestWindowHeight)
            );
        }

        public int Width { get { return Console.BufferWidth; } }

        public int Height { get { return Console.BufferHeight; } }

        public object ReadKey()
        {
            ConsoleKeyInfo cki = Console.ReadKey(true);
            SchemeHashMap shm = new SchemeHashMap();
            shm[new Symbol("keychar")] = cki.KeyChar;
            shm[new Symbol("key")] = BigInteger.FromInt32((int)cki.Key);
            shm[new Symbol("keyname")] = new Symbol(cki.Key.ToString());
            shm[new Symbol("modifiers")] = BigInteger.FromInt32((int)cki.Modifiers);
            return shm;
        }

        public void SetColor(int fg, int bg)
        {
            Console.ForegroundColor = (ConsoleColor)fg;
            Console.BackgroundColor = (ConsoleColor)bg;
        }

        public Tuple<int, int> GetColor()
        {
            return new Tuple<int, int>((int)(Console.ForegroundColor), (int)(Console.BackgroundColor));
        }
    }
}

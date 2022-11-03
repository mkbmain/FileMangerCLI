using System;
using System.Linq;

namespace FileManagerCLI
{
    class Program
    {
        public static ConsoleColor BackColor = ConsoleColor.White;
        public static ConsoleColor ForeColor = ConsoleColor.Black;
        public static ConsoleModifiers ModKey = ConsoleModifiers.Control;

        private static void IfMod(ConsoleModifiers modifier, Action invoke)
        {
            if (modifier.HasFlag(ModKey))
            {
                invoke();
            }
        }

        static void Main(string[] args)
        {
            Console.Title = "FileManager";
            Console.Clear();
            var display1 = new FileManagerDisplay();
            while (true)
            {
                var readKey = Console.ReadKey(true);
                switch (readKey.Key)
                {
                    case ConsoleKey.UpArrow:
                        display1.ChangeSelected(true);
                        break;
                    case ConsoleKey.DownArrow:
                        display1.ChangeSelected(false);
                        break;
                    case ConsoleKey.Enter:
                        display1.Select();
                        break;
                    case ConsoleKey.H:
                        display1.ToggleHidden();
                        break;
                    case ConsoleKey.D:
                        IfMod(readKey.Modifiers, display1.Delete);
                        break;
                    case ConsoleKey.S:
                        display1.Store();
                        break;
                }
            }
        }
    }
}
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
            FileManagerDisplay.InitDisplay();
            while (true)
            {
                var readKey = Console.ReadKey(true);
                switch (readKey.Key)
                {
                    case ConsoleKey.UpArrow:
                        FileManagerDisplay.ChangeSelected(true);
                        break;
                    case ConsoleKey.DownArrow:
                        FileManagerDisplay.ChangeSelected(false);
                        break;

                    case ConsoleKey.Enter:
                        FileManagerDisplay.Select();
                        break;
                    case ConsoleKey.H:
                        FileManagerDisplay.ToggleHidden();
                        break;

                    case ConsoleKey.D:
                        IfMod(readKey.Modifiers, FileManagerDisplay.Delete);
                        break;
                    case ConsoleKey.S:
                        FileManagerDisplay.Store();
                        break;
                    case ConsoleKey.Q:
                        IfMod(readKey.Modifiers, Console.Clear);
                        break;
                }
            }
        }
    }
}
using System;
using System.IO;

namespace FileManagerCLI
{
    class Program
    {
        public static Config Config = new Config();
        private const string ConfigFileName = "config.json";

        private static void IfMod(ConsoleModifiers modifier, Action invoke) => IfMod(modifier, () =>
        {
            invoke();
            return true;
        });

        private static T IfMod<T>(ConsoleModifiers modifiers, Func<T> invoke)
        {
            if (modifiers.HasFlag(Config.ModKey))
            {
                return invoke();
            }

            return default;
        }

        static void Main(string[] args)
        {
            Console.Clear();
            Console.Title = "FileManager";
            if (File.Exists(ConfigFileName))
            {
                var json = System.IO.File.ReadAllText(ConfigFileName);
                var config = System.Text.Json.JsonSerializer.Deserialize<Config>(json);
                Config = config;
            }

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
                    case ConsoleKey.M:
                        display1.Move();
                        break;
                    case ConsoleKey.C:
                        display1.Copy();
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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using FileManagerCLI.FileManager;
using FileManagerCLI.Settings;

namespace FileManagerCLI
{
    class Program
    {
        public static Config Config = new Config();
        private const string ConfigFileName = "config.json";
        private Size Size = new Size(Console.WindowWidth, Console.WindowHeight);

        private static void ChangeDisplays(IReadOnlyList<FileManagerWindow> fileManagerWindows)
        {
            var total = fileManagerWindows.Count;
            var pos = 1m / total;
            for (int i = 0; i < fileManagerWindows.Count; i++)
            {
                fileManagerWindows[i].UpdateDisplayDetails(pos, i * pos);
            }
        }

        public static void Main(string[] args)
        {
            Console.Clear();
            Console.Title = "FileManager";
            if (File.Exists(ConfigFileName))
            {
                var json = System.IO.File.ReadAllText(ConfigFileName);
                var config = System.Text.Json.JsonSerializer.Deserialize<Config>(json);
                Config = config;
            }

            var displays = new List<FileManagerWindow>();
            var size = new Size(Console.WindowWidth, Console.WindowHeight);
            displays.Add(new FileManagerWindow(1, 0));
            var selectedDisplay = displays.First();
            while (true)
            {
                if (size.Width != Console.WindowWidth || size.Height != Console.WindowHeight)
                {
                    size = new Size(Console.WindowWidth, Console.WindowHeight);
                    foreach (var item in displays)
                    {
                        item.Redraw();
                    }
                }


                var readKey = Console.ReadKey(true);
                switch (readKey.Key)
                {
                    case ConsoleKey.LeftArrow:
                    {
                        var wasMod = IfMod(readKey.Modifiers, () =>
                        {
                            displays = displays.Take(displays.IndexOf(selectedDisplay) + 1).ToList();
                            ChangeDisplays(displays);
                            return true;
                        });

                        if (!wasMod && selectedDisplay != displays.First())
                        {
                            selectedDisplay = displays[displays.IndexOf(selectedDisplay) - 1];
                        }
                    }
                        break;
                    case ConsoleKey.RightArrow:
                    {
                        var wasMod = IfMod(readKey.Modifiers, () =>
                        {
                            displays.Add(new FileManagerWindow(0, 0));
                            ChangeDisplays(displays);
                            selectedDisplay = displays.Last();
                            return true;
                        });

                        if (!wasMod && selectedDisplay != displays.Last())
                        {
                            selectedDisplay = displays[displays.IndexOf(selectedDisplay) + 1];
                        }
                    }
                        break;
                    case ConsoleKey.UpArrow:
                        selectedDisplay.MoveSelected(true);
                        break;
                    case ConsoleKey.DownArrow:
                        selectedDisplay.MoveSelected(false);
                        break;
                    case ConsoleKey.Enter:
                        selectedDisplay.Select();
                        break;
                    case ConsoleKey.H:
                        selectedDisplay.ToggleHidden();
                        break;
                    case ConsoleKey.Q:
                        IfMod(readKey.Modifiers, () => Process.GetCurrentProcess().Kill());
                        break;
                    case ConsoleKey.M:
                        selectedDisplay.Move();
                        break;
                    case ConsoleKey.C:
                        selectedDisplay.Copy();
                        break;
                    case ConsoleKey.D:
                        IfMod(readKey.Modifiers, selectedDisplay.Delete);
                        break;
                    case ConsoleKey.S:
                        if (!IfMod(readKey.Modifiers, () =>
                            {
                                selectedDisplay.ClearStore();
                                return true;
                            }))
                        {
                            selectedDisplay.Store();
                        }

                        break;
                }
            }
        }

        private static void IfMod(ConsoleModifiers modifier, Action invoke) => IfMod(modifier, () =>
        {
            invoke();
            return true;
        });

        private static T IfMod<T>(ConsoleModifiers modifiers, Func<T> invoke) =>
            modifiers.HasFlag(Config.ModKey) ? invoke() : default;
    }
}
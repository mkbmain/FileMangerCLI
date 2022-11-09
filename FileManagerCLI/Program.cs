using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using FileManagerCLI.Enums;
using FileManagerCLI.FileManager;
using FileManagerCLI.Settings;

namespace FileManagerCLI
{
    class Program
    {
        public static Config Config = new Config();
        private const string ConfigFileName = "config.json";
        private static void ChangeDisplays(IReadOnlyList<FileManagerWindow> fileManagerWindows)
        {
            Console.Clear();
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
                var json = File.ReadAllText(ConfigFileName);
                var config = System.Text.Json.JsonSerializer.Deserialize<Config>(json);
                Config = config;
            }

            var displays = new List<FileManagerWindow>();
            var size = new Size(Console.WindowWidth, Console.WindowHeight);
            displays.Add(new FileManagerWindow());
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
                                displays.Add(new FileManagerWindow());
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
                    case ConsoleKey.K:
                        if (displays.Count == 1) { continue; }
                        var index = displays.IndexOf(selectedDisplay);
                        displays = displays.Where(e => e != selectedDisplay).ToList();
                        ChangeDisplays(displays);
                        selectedDisplay = displays.Count -1 >= index ? displays[index] : displays.Last();
                        break;
                    case ConsoleKey.PageUp:
                        selectedDisplay.MoveSelected(MoveSelected.Top);
                        break;
                    case ConsoleKey.PageDown:
                        selectedDisplay.MoveSelected(MoveSelected.Bottom);
                        break;
                    case ConsoleKey.UpArrow:
                        if (IfMod(readKey.Modifiers))
                        {
                            selectedDisplay.MoveSelected(MoveSelected.TenUp);
                            continue;
                        }
                        selectedDisplay.MoveSelected(MoveSelected.OneUp);
                        break;
                    case ConsoleKey.DownArrow:
                        if (IfMod(readKey.Modifiers))
                        {
                            selectedDisplay.MoveSelected(MoveSelected.TenDown);
                            continue;
                        }
                        selectedDisplay.MoveSelected(MoveSelected.OneDown);
                        break;
                    case ConsoleKey.Enter:
                        selectedDisplay.Select();
                        break;
                    case ConsoleKey.L:
                        IfMod(readKey.Modifiers, () =>
                        {
                            selectedDisplay.EditLocation();
                            foreach (var t in displays) t.Redraw();

                        });
                        break;
                    case ConsoleKey.H:
                        selectedDisplay.ToggleHidden();
                        break;
                    case ConsoleKey.Q:
                        if (IfMod(readKey.Modifiers))
                        {
                            Console.Clear();
                            return;
                        }
                        break;
                    case ConsoleKey.M:
                        selectedDisplay.Move();
                        break;
                    case ConsoleKey.B:
                        selectedDisplay.TopDirectory();
                        break;
                    case ConsoleKey.C:
                        selectedDisplay.Copy();
                        break;
                    case ConsoleKey.D:
                        IfMod(readKey.Modifiers, selectedDisplay.Delete);
                        break;
                    case ConsoleKey.S:
                        if (IfMod(readKey.Modifiers))
                        {
                            selectedDisplay.ClearStore();
                            continue;
                        }
                        selectedDisplay.Store();
                        
                        break;
                }
            }
        }

        private static void IfMod(ConsoleModifiers modifier, Action invoke) => IfMod(modifier, () =>
        {
            invoke();
            return true;
        });
        private static T IfMod<T>(ConsoleModifiers modifiers, Func<T> invoke) => IfMod(modifiers) ? invoke() : default;
        private static bool IfMod(ConsoleModifiers modifiers) => modifiers.HasFlag(Config.ModKey);
    }
}
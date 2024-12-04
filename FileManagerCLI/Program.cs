using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using FileManagerCLI.Data;
using FileManagerCLI.Enums;
using FileManagerCLI.FileManager;
using FileManagerCLI.Settings;

namespace FileManagerCLI
{
    class Program
    {
        public static Config Config = new();
        private static List<LogEvent> _logEvents = new();

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

            Console.WriteLine(KeyBindings.Replace("(mod)", Config.ModKey.ToString()));
            Console.ReadLine();

            FileManagerDisplay.LogEvent += FileManagerWindowOnLogEvent;

            var displays = new List<FileManagerWindow>();

            var size = new Size(Console.WindowWidth, Console.WindowHeight);
            displays.Add(new FileManagerWindow(Config.ShowHiddenByDefault));
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
                            displays.Add(new FileManagerWindow(Config.ShowHiddenByDefault));
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
                        if (displays.Count == 1)
                        {
                            continue;
                        }

                        var index = displays.IndexOf(selectedDisplay);
                        displays = displays.Where(e => e != selectedDisplay).ToList();
                        ChangeDisplays(displays);
                        selectedDisplay = displays.Count - 1 >= index ? displays[index] : displays.Last();
                        break;
                    case ConsoleKey.PageUp:
                        selectedDisplay.MoveSelected(MoveSelected.Top);
                        break;
                    case ConsoleKey.PageDown:
                        selectedDisplay.MoveSelected(MoveSelected.Bottom);
                        break;
                    case ConsoleKey.UpArrow:
                        selectedDisplay.MoveSelected(IfMod(readKey.Modifiers) ? MoveSelected.TenUp : MoveSelected.OneUp);
                        break;
                    case ConsoleKey.DownArrow:
                        selectedDisplay.MoveSelected(IfMod(readKey.Modifiers) ? MoveSelected.TenDown : MoveSelected.OneDown);
                        break;
                    case ConsoleKey.Enter:
                        selectedDisplay.Select();
                        break;
                    case ConsoleKey.R:
                        selectedDisplay.Reload();
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
                        Program.Config.DisplayItemSize = !Program.Config.DisplayItemSize;
                        foreach (var item in displays)
                        {
                           item.Reload();
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

        private static void FileManagerWindowOnLogEvent(object sender, LogEvent logEvent)
        {
            if (_logEvents.Count > 100)
            {
                _logEvents = _logEvents.Skip(1).ToList();
            }

            _logEvents.Add(logEvent);

            if (string.IsNullOrWhiteSpace(Config.LogFile)) return;
            using var sw = new StreamWriter(File.Open(Config.LogFile, FileMode.Append), Encoding.Default);
            sw.WriteLine(
                $"{DateTime.Now:g} - {logEvent.Log} -- {logEvent.LogType} {logEvent.Exception?.Message ?? ""}");
        }

        private static void IfMod(ConsoleModifiers modifier, Action invoke) => IfMod(modifier, () =>
        {
            invoke();
            return true;
        });

        private static T IfMod<T>(ConsoleModifiers modifiers, Func<T> invoke) => IfMod(modifiers) ? invoke() : default;
        private static bool IfMod(ConsoleModifiers modifiers) => modifiers.HasFlag(Config.ModKey);

        private const string ConfigFileName = "config.json";

        private const string KeyBindings = @"Simple key bindings
left/right = move between tabs
(mod) + left/right = will create new tabs or collapse all tabs to right
K = will kill current tab

up/down = move up and down in a tab (use mod key to jump 10 at a time)
pageup/pagedown = go to top of current tab or bottom
Enter = select

B = Top directory
H = show hidden files on tab
(mod) + q = exit

(mod) + d Deletes current selected file /folder

S = stores current selected item in buffer
(mod) + s = clears buffer
C = Copy (Copy the current item in buffer here)
M = move (Moves the current item in buffer here)
R = Reload current display
Q = Calculate Size toggle (please note on big systems this might take some time)
";
    }
}
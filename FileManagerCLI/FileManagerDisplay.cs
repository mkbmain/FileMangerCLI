using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using FileManagerCLI.Data;

namespace FileManagerCLI
{
    public static class FileManagerDisplay
    {
        private static Size WindowSize;
        private const int HeightOffset = 3;
        private static string _xxPath;
        private static int _maxWidth = int.MaxValue;
        private static IoItem _selected;
        private static List<IoItem> _displayItems = new List<IoItem>();
        private static int _offset = 0;
        private static StoredIoItem _stored = null;
        private static bool ShowHidden = true;

        private static string Path
        {
            get => _xxPath;
            set
            {
                _xxPath = value;
                if (_xxPath.EndsWith(GetIoInfo.PathSeparator) == false)
                {
                    _xxPath += GetIoInfo.PathSeparator;
                }

                var items = GetIoInfo.GetDetailsForPath(Path).Where(e => ShowHidden || e.Hidden == false).ToList();
                _selected = items.First();
                Display(items);
            }
        }

        public static void InitDisplay(int maxWidth = int.MaxValue)
        {
            _maxWidth = maxWidth;
            Path = Environment.CurrentDirectory;
        }

        private static void Display(IEnumerable<IoItem> items)
        {
            _offset = 0;
            Display(items, _offset);
        }

        private static void Display(IEnumerable<IoItem> items, int offset)
        {
            Console.Clear();
            _offset = offset;
            _displayItems = items.ToList();
            BuildDisplay(_maxWidth);

            Console.SetCursorPosition(0, 0);
            Console.WriteLine(FitWidth(Path, false));
            WriteStored();
        }

        private static void BuildDisplay(int maxWidth)
        {
            _maxWidth = maxWidth;
            WindowSize = new Size(Math.Min(_maxWidth, Console.WindowWidth), Console.WindowHeight - HeightOffset);
            int i = 0;
            for (i = _offset; i < Math.Min(_displayItems.Count, WindowSize.Height + _offset); i++)
            {
                OutPutDisplay(_displayItems[i].DisplayName, i + 1 - _offset, _displayItems[i] == _selected);
            }

            for (i = i; i < WindowSize.Height; i++)
            {
                OutPutDisplay(" ", i + 1 - _offset, false);
            }

            Console.SetCursorPosition(0, Console.WindowHeight - 1);
            Console.Write(
                $"Mod = {Program.ModKey.ToString()} | Exit:Mod+q | Hidden:H | Store:S".PadRight(WindowSize.Width, ' '));
        }



        private static void OutPutDisplay(string text, int y, bool selected)
        {
            if (WindowSize.Width != Math.Min(_maxWidth, Console.WindowWidth) ||
                WindowSize.Height != Console.WindowHeight - HeightOffset)
            {
                Display(_displayItems, _offset);
                return;
            }

            Console.SetCursorPosition(0, y + 1);
            Console.BackgroundColor = selected ? Program.ForeColor : Program.BackColor;
            Console.ForegroundColor = selected ? Program.BackColor : Program.ForeColor;
            Console.Write(FitWidth(text, false));
        }

        private static void WriteStored()
        {
            Console.BackgroundColor = Program.BackColor;
            Console.ForegroundColor = Program.ForeColor;
            Console.SetCursorPosition(0, 1);
            Console.Write(FitWidth(_stored?.FullPath, false));
        }

        public static void Store()
        {
            switch (_selected.IoType)
            {
                case IoItemType.File:
                case IoItemType.Directory:
                    _stored = new StoredIoItem(_selected, Path);
                    WriteStored();
                    break;
            }
        }

        public static void Delete()
        {
            var path = System.IO.Path.Combine(Path, _selected.Name);
            switch (_selected.IoType)
            {
                case IoItemType.File:
                    System.IO.File.Delete(path);
                    break;
                case IoItemType.Directory:
                    System.IO.Directory.Delete(path);
                    break;
                default:
                    return;
                    break;
            }

            if (_stored?.FullPath == path)
            {
                _stored = null;
            }
            Path = Path;
        }

        public static void ToggleHidden()
        {
            ShowHidden = !ShowHidden;
            Path = Path;
        }

        public static void Select()
        {
            switch (_selected.IoType)
            {
                case IoItemType.Directory:
                    Path = System.IO.Path.Combine(Path, _selected.Name);
                    break;
                case IoItemType.Back:
                    Path = new DirectoryInfo(Path).Parent.FullName;
                    break;
            }
        }

        public static void ChangeSelected(bool up)
        {
            var currentSlectedIndex = _displayItems.IndexOf(_selected);
            if ((up && currentSlectedIndex == 0) || (!up && currentSlectedIndex == _displayItems.Count - 1)) return;

            var newSelectedIndex = currentSlectedIndex + (up ? -1 : 1);
            var previous = _selected;
            _selected = _displayItems[newSelectedIndex];

            if (newSelectedIndex > (_offset + WindowSize.Height - 1))
            {
                _offset += 10;
                Display(_displayItems, _offset);
                return;
            }

            if (newSelectedIndex < _offset)
            {
                _offset -= 10;
                Display(_displayItems, _offset);
                return;
            }

            OutPutDisplay(previous.DisplayName, currentSlectedIndex - _offset + 1, false);
            OutPutDisplay(_selected.DisplayName, newSelectedIndex - _offset + 1, true);
        }

        private static string FitWidth(string format, bool keepStart) => (format ?? "").Length > WindowSize.Width
            ? keepStart
                ? (format ?? "")[..WindowSize.Width]
                : (format ?? "").Substring((format ?? "").Length - WindowSize.Width, WindowSize.Width)
            : (format ?? "").PadRight(WindowSize.Width, ' ');
    }
}
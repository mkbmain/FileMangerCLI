using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using FileManagerCLI.Data;
using FileManagerCLI.Extension;

namespace FileManagerCLI
{
    public static class FileManagerDisplay
    {
        private const int HeightOffset = 3;
        private const char Empty = ' ';
        private static string _xxPath;
        private static int _maxWidth = int.MaxValue;
        private static DisplayElement[][] _display = Array.Empty<DisplayElement[]>();
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
            var lineNum = 0;
            _displayItems = items.ToList();
            BuildDisplay(_maxWidth);
            foreach (var line in _displayItems.Skip(offset))
            {
                EnterLine(line.DisplayName, lineNum, line == _selected);

                lineNum++;
                if (lineNum >= (_display.First()?.Length ?? 0) - 1) break;
            }

            Console.SetCursorPosition(0, 0);
            Console.WriteLine(FitWidth(Path, false));
            WriteStored();
        }

        private static void BuildDisplay(int maxWidth)
        {
            _maxWidth = maxWidth;
            _display = new DisplayElement[Math.Min(_maxWidth, Console.WindowWidth)][];
            for (var item = 0; item < _display.Length; item++)
            {
                var widthCol = new DisplayElement[Console.WindowHeight - HeightOffset];
                for (var i = 0; i < widthCol.Length; i++)
                {
                    widthCol[i] = new DisplayElement {Value = Empty, Point = new Point(item, i + 1)};
                }

                _display[item] = widthCol;
            }

            OutPutDisplay();
            Console.SetCursorPosition(0, Console.WindowHeight - 1);
            Console.Write(
                $"Mod = {Program.ModKey.ToString()} | Exit:Mod+q | Hidden:H | Store:S".PadRight(_display.Length, ' '));
        }

        private static void EnterLine(string text, int lineNum, bool selected)
        {
            for (int i = 0; i < _display.Length; i++)
            {
                var item = _display[i][lineNum];
                item.Value = text.Length - 1 < i ? ' ' : text[i];
                item.Selected = selected;
                OutPutDisplay(item);
            }
        }

        private static void OutPutDisplay() => _display.Foreach(OutPutDisplay);

        private static void OutPutDisplay(DisplayElement[] displayElements) => displayElements.Foreach(OutPutDisplay);

        private static void OutPutDisplay(DisplayElement displayElement)
        {
            if (_display.Length != Math.Min(_maxWidth, Console.WindowWidth) ||
                _display.First().Length != Console.WindowHeight - HeightOffset)
            {
                Display(_displayItems, _offset);
            }

            Console.SetCursorPosition(displayElement.Point.X, displayElement.Point.Y + 1);
            Console.BackgroundColor = displayElement.BackgroundColor;
            Console.ForegroundColor = displayElement.ForegroundColor;
            Console.Write(displayElement.Value);
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

            if (newSelectedIndex > (_offset + _display.First().Length - 2))
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

            EnterLine(previous.DisplayName, currentSlectedIndex - _offset, false);
            EnterLine(_selected.DisplayName, newSelectedIndex - _offset, true);
        }

        private static string FitWidth(string format, bool keepStart) => (format ?? "").Length > _display.Length
            ? keepStart
                ? (format ?? "")[.._display.Length]
                : (format ?? "").Substring((format ?? "").Length - _display.Length, _display.Length)
            : (format ?? "").PadRight(_display.Length, ' ');
    }
}
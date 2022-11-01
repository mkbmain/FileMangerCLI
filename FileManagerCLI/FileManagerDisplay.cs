using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using FileManagerCLI.Data;
using FileManagerCLI.Extension;

namespace FileManagerCLI
{
    public class FileManagerDisplay
    {
        private const char Empty = ' ';
        private static string _xxPath;
        private static int MaxWidth = int.MaxValue;
        private static DisplayElement[][] _display = Array.Empty<DisplayElement[]>();
        private static IoItem _selected;
        private static List<IoItem> _displayItems = new List<IoItem>();
        private static int Offset = 0;
        private static string Stored = null;

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

                var items = GetIoInfo.GetDetailsForPath(Path);
                _selected = items.First();
                Display(items);

                var display = _xxPath;
                if (_display.Length < display.Length)
                {
                    display = display.Substring(display.Length - _display.Length, _display.Length);
                }
                else if (display.Length < _display.Length)
                {
                    display = display.PadRight(_display.Length, ' ');
                }

                Console.SetCursorPosition(0, 0);
                Console.Write(display);
            }
        }

        public static void InitDisplay(int maxWidth = int.MaxValue)
        {
            MaxWidth = maxWidth;
            Path = Environment.CurrentDirectory;
        }

        private static void Display(IEnumerable<IoItem> items)
        {
            Offset = 0;
            Display(items, Offset);
        }

        private static void Display(IEnumerable<IoItem> items, int offset)
        {
            var lineNum = 0;
            _displayItems = items.ToList();
            BuildDisplay(MaxWidth);
            foreach (var line in _displayItems.Skip(offset))
            {
                EnterLine(line.DisplayName, lineNum, line == _selected);

                lineNum++;
                if (lineNum >= (_display.First()?.Length ?? 0) - 1) break;
            }
        }

        private static void BuildDisplay(int maxWidth)
        {
            MaxWidth = maxWidth;
            _display = new DisplayElement[Math.Min(MaxWidth, Console.WindowWidth)][];
            for (var item = 0; item < _display.Length; item++)
            {
                var widthCol = new DisplayElement[Console.WindowHeight - 2];
                for (var i = 0; i < widthCol.Length; i++)
                {
                    widthCol[i] = new DisplayElement {Value = Empty, Point = new Point(item, i + 1)};
                }

                _display[item] = widthCol;
            }

            OutPutDisplay();
            Console.SetCursorPosition(0, Console.WindowHeight);
            Console.Write("Mod = CTRL | Exit:Mod+q | Store:s");
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
            Console.SetCursorPosition(displayElement.Point.X, displayElement.Point.Y + 1);
            Console.BackgroundColor = displayElement.BackgroundColor;
            Console.ForegroundColor = displayElement.ForegroundColor;
            Console.Write(displayElement.Value);
        }

        public static void Store()
        {
            switch (_selected.IoType)
            {
                case IoItemType.File:
                case IoItemType.Directory:
                    Stored = System.IO.Path.Combine(Path, _selected.Name);
                    break;
                case IoItemType.Back:
                    Path = new DirectoryInfo(Path).Parent.FullName;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static void Select()
        {
            switch (_selected.IoType)
            {
                case IoItemType.File:
                    break;
                case IoItemType.Directory:
                    Path = System.IO.Path.Combine(Path, _selected.Name);
                    break;
                case IoItemType.Back:
                    Path = new DirectoryInfo(Path).Parent.FullName;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static void ChangeSelected(bool up)
        {
            var currentSlectedIndex = _displayItems.IndexOf(_selected);
            if ((up && currentSlectedIndex == 0) || (!up && currentSlectedIndex == _displayItems.Count - 1)) return;

            var newSelectedIndex = currentSlectedIndex + (up ? -1 : 1);
            var previous = _selected;
            _selected = _displayItems[newSelectedIndex];

            if (newSelectedIndex > (Offset + _display.First().Length - 2))
            {
                Offset += 10;
                Display(_displayItems, Offset);
                return;
            }

            if (newSelectedIndex < Offset)
            {
                Offset -= 10;
                Display(_displayItems, Offset);
                return;
            }

            EnterLine(previous.DisplayName, currentSlectedIndex - Offset, false);
            EnterLine(_selected.DisplayName, newSelectedIndex - Offset, true);
        }
    }
}
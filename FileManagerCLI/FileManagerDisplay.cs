using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using FileManagerCLI.Data;

namespace FileManagerCLI
{
    public class FileManagerDisplay
    {
        private Size WindowSize;
        private const int HeightOffset = 3;
        private int StartLeft;
        private string _xxPath;
        private int _maxWidth = int.MaxValue;
        private IoItem _selected;
        private List<IoItem> _displayItems = new List<IoItem>();
        private int _offset = 0;
        private static StoredIoItem _stored = null;
        private bool ShowHidden = true;

        public FileManagerDisplay(int maxWidth = int.MaxValue, int startLeft = 0)
        {
            StartLeft = startLeft;
            _maxWidth = maxWidth;
            Path = Environment.CurrentDirectory;
        }

        private string Path
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


        private void Display(IEnumerable<IoItem> items)
        {
            _offset = 0;
            Display(items, _offset);
        }

        private void Display(IEnumerable<IoItem> items, int offset)
        {
            Console.Clear();
            _offset = offset;
            _displayItems = items.ToList();
            BuildDisplay(_maxWidth);

            Console.SetCursorPosition(StartLeft, 0);
            Console.WriteLine(FitWidth(Path, false));
            WriteStored();
        }

        private void BuildDisplay(int maxWidth)
        {
            _maxWidth = maxWidth;
            WindowSize = new Size(Math.Min(_maxWidth, Console.WindowWidth - StartLeft),
                Console.WindowHeight - HeightOffset);
            int i = 0;
            for (i = _offset; i < Math.Min(_displayItems.Count, WindowSize.Height + _offset); i++)
            {
                OutPutDisplay(_displayItems[i].DisplayName, i + 1 - _offset, _displayItems[i] == _selected);
            }

            for (i = i; i < WindowSize.Height; i++)
            {
                OutPutDisplay(" ", i + 1 - _offset, false);
            }

            Console.SetCursorPosition(StartLeft, Console.WindowHeight - 1);
            Console.Write(
                $"Mod = {Program.Config.ModKey.ToString()} | Exit:Mod+q | Hidden:H | Store:S".PadRight(WindowSize.Width, ' '));
        }


        private void OutPutDisplay(string text, int y, bool selected)
        {
            if (WindowSize.Width != Math.Min(_maxWidth, Console.WindowWidth - StartLeft) ||
                WindowSize.Height != Console.WindowHeight - HeightOffset)
            {
                Display(_displayItems, _offset);
                return;
            }

            Console.SetCursorPosition(StartLeft, y + 1);
            Console.BackgroundColor = selected ? Program.Config.ForegroundColor : Program.Config.BackgroundColor;
            Console.ForegroundColor = selected ? Program.Config.BackgroundColor : Program.Config.ForegroundColor;
            Console.Write(FitWidth(text, false));
        }

        private void WriteStored()
        {
            Console.BackgroundColor = Program.Config.BackgroundColor;
            Console.ForegroundColor = Program.Config.ForegroundColor;
            Console.SetCursorPosition(0, 1); // there is only one of these we force it to always be 0
            Console.Write(FitWidth(_stored?.FullPath, false));
        }

        public void Store()
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

        public void Delete()
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

        public void ToggleHidden()
        {
            ShowHidden = !ShowHidden;
            Path = Path;
        }

        public void Select()
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

        public void ChangeSelected(bool up)
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

        private string FitWidth(string format, bool keepStart) => (format ?? "").Length > WindowSize.Width
            ? keepStart
                ? (format ?? "")[..WindowSize.Width]
                : (format ?? "").Substring((format ?? "").Length - WindowSize.Width, WindowSize.Width)
            : (format ?? "").PadRight(WindowSize.Width, ' ');
    }
}
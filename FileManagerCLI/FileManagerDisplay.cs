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
        private static StoredIoItem _xxstored; // we only want 1 stored globally
        private int _maxWidth = int.MaxValue;
        private IoItem _selected;
        private List<IoItem> _displayItems = new List<IoItem>();
        private int _offset = 0;

        private StoredIoItem _stored
        {
            get => _xxstored;
            set
            {
                _xxstored = value;
                WriteStored();
            }
        }

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
                if (_xxPath.EndsWith(IoUtil.PathSeparator) == false)
                {
                    _xxPath += IoUtil.PathSeparator;
                }

                var items = IoUtil.GetDetailsForPath(Path).Where(e => ShowHidden || e.Hidden == false).ToList();
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
                OutPutDisplay(_displayItems[i].DisplayName, i  - _offset, _displayItems[i] == _selected);
            }

            for (i = i - _offset; i < WindowSize.Height; i++)
            {
                OutPutDisplay(" ", i  , false);
            }

            WriteMenu();
        }

        private void WriteMenu()
        {
            var storedDetails = _stored is null ? "" : " | Copy:C | Move:M | Clear:Mod+S";
            Console.SetCursorPosition(0, Console.WindowHeight - 1);
            Console.Write(FitWidth(
                $"Mod = {Program.Config.ModKey.ToString()} | Exit:Mod+Q | Delete:Mod+D | Hidden:H | Store:S{storedDetails}"
                    .PadRight(WindowSize.Width, ' '), true));
        }


        private void OutPutDisplay(string text, int y, bool selected)
        {
            if (WindowSize.Width != Math.Min(_maxWidth, Console.WindowWidth - StartLeft) ||
                WindowSize.Height != Console.WindowHeight - HeightOffset)
            {
                Display(_displayItems, _offset);
                return;
            }

            Console.SetCursorPosition(StartLeft, y + 2);
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
            WriteMenu();
        }

        public bool Copy()
        {
            if (_stored is null)
            {
                return false;
            }

            switch (_stored.IoType)
            {
                case IoItemType.File:
                    File.Copy(_stored.FullPath, System.IO.Path.Combine(Path, _stored.Name), true);
                    Path = Path;
                    return true;
                case IoItemType.Directory:
                    IoUtil.DirectoryCopy(_stored.FullPath, System.IO.Path.Combine(Path, _stored.Name));
                    Path = Path;
                    return true;
            }

            return false;
        }

        public void ClearStore()
        {
            _stored = null;
        }

        public void Store()
        {
            switch (_selected.IoType)
            {
                case IoItemType.File:
                case IoItemType.Directory:
                    _stored = new StoredIoItem(_selected, Path);
                    break;
            }
        }

        public void Move()
        {
            if (!Copy()) return;
            switch (_stored.IoType)
            {
                case IoItemType.File:
                    System.IO.File.Delete(_stored.FullPath);
                    break;
                case IoItemType.Directory:
                    System.IO.Directory.Delete(_stored.FullPath);
                    break;
                default:
                    return;
                    break;
            }

            _stored = null;
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
                    System.IO.Directory.Delete(path, true);
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

            OutPutDisplay(previous.DisplayName, currentSlectedIndex - _offset , false);
            OutPutDisplay(_selected.DisplayName, newSelectedIndex - _offset , true);
        }

        private string FitWidth(string format, bool keepStart) => (format ?? "").Length > WindowSize.Width
            ? keepStart
                ? (format ?? "")[..WindowSize.Width]
                : (format ?? "").Substring((format ?? "").Length - WindowSize.Width, WindowSize.Width)
            : (format ?? "").PadRight(WindowSize.Width, ' ');
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using FileManagerCLI.Data;
using FileManagerCLI.Utils;

namespace FileManagerCLI.FileManager
{
    public abstract class FileManagerDisplay
    {
        protected bool ShowHidden = true;
        protected Size WindowSize;
        private const int HeightOffset = 3;
        private int StartLeft;
        private string _xxPath;
        private static StoredIoItem _xxstored; // we only want 1 stored globally
        private int _maxWidth = int.MaxValue;
        protected IoItem _selected;
        protected List<IoItem> _displayItems = new List<IoItem>();
        protected int _offset = 0;

        protected StoredIoItem _stored
        {
            get => _xxstored;
            set
            {
                _xxstored = value;
                WriteStored();
            }
        }


        public FileManagerDisplay(int maxWidth, int startLeft)
        {
            StartLeft = startLeft;
            _maxWidth = maxWidth;
            Path = Environment.CurrentDirectory;
        }

        protected string Path
        {
            get => _xxPath;
            set
            {
                _xxPath = value;
                if (_xxPath.EndsWith(FileIoUtil.PathSeparator) == false)
                {
                    _xxPath += FileIoUtil.PathSeparator;
                }

                var items = FileIoUtil.GetDetailsForPath(Path).Where(e => ShowHidden || e.Hidden == false).ToList();
                _selected = items.First();
                Display(items);
            }
        }


        protected void Display(IEnumerable<IoItem> items)
        {
            _offset = 0;
            Display(items, _offset);
        }

        protected void Display(IEnumerable<IoItem> items, int offset)
        {
            Console.Clear();
            _offset = offset;
            _displayItems = items.ToList();
            BuildDisplay(_maxWidth);

            Console.SetCursorPosition(StartLeft, 0);
            Console.WriteLine(FitWidth(Path, false));
            WriteStored();
        }

        protected void BuildDisplay(int maxWidth)
        {
            _maxWidth = maxWidth;
            WindowSize = new Size(Math.Min(_maxWidth, Console.WindowWidth - StartLeft),
                Console.WindowHeight - HeightOffset);
            int i = 0;
            for (i = _offset; i < Math.Min(_displayItems.Count, WindowSize.Height + _offset); i++)
            {
                OutPutDisplay(_displayItems[i].DisplayName, i - _offset, _displayItems[i] == _selected);
            }

            for (i = i - _offset; i < WindowSize.Height; i++)
            {
                OutPutDisplay(" ", i, false);
            }

            WriteMenu();
        }

        protected void WriteMenu()
        {
            var storedDetails = _stored is null ? "" : " | Copy:C | Move:M | Clear:Mod+S";
            Console.SetCursorPosition(0, Console.WindowHeight - 1);
            Console.Write(FitWidth(
                $"Mod = {Program.Config.ModKey.ToString()} | Exit:Mod+Q | Delete:Mod+D | Hidden:H | Store:S{storedDetails}"
                    .PadRight(WindowSize.Width, ' '), true));
        }


        protected void OutPutDisplay(string text, int y, bool selected)
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

        protected void WriteStored()
        {
            Console.BackgroundColor = Program.Config.BackgroundColor;
            Console.ForegroundColor = Program.Config.ForegroundColor;
            Console.SetCursorPosition(0, 1); // there is only one of these we force it to always be 0
            Console.Write(FitWidth(_stored?.FullPath, false));
            WriteMenu();
        }


        protected string FitWidth(string format, bool keepStart) => (format ?? "").Length > WindowSize.Width
            ? keepStart
                ? (format ?? "")[..WindowSize.Width]
                : (format ?? "").Substring((format ?? "").Length - WindowSize.Width, WindowSize.Width)
            : (format ?? "").PadRight(WindowSize.Width, ' ');
    }
}
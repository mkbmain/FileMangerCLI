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
        private readonly int _startLeft;
        private string _xxPath;
        private static StoredIoItem _xxstored; // we only want 1 stored globally
        private int _maxWidth = int.MaxValue;
        protected IoItem Selected;
        protected List<IoItem> DisplayItems = new List<IoItem>();
        protected int Offset;

        protected StoredIoItem Stored
        {
            get => _xxstored;
            set
            {
                _xxstored = value;
                WriteStored();
            }
        }

        protected FileManagerDisplay(int maxWidth, int startLeft)
        {
            _startLeft = startLeft;
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
                Selected = items.First();
                Display(items, 0);
            }
        }

        protected void Display(IEnumerable<IoItem> items, int offset)
        {
            Offset = offset;
            DisplayItems = items.ToList();
            BuildDisplay(_maxWidth);

            Console.SetCursorPosition(_startLeft, 0);
            Console.WriteLine(FitWidth(Path, false));
            WriteStored();
        }

        private void BuildDisplay(int maxWidth)
        {
            _maxWidth = maxWidth;
            WindowSize = new Size(Math.Min(_maxWidth, Console.WindowWidth - _startLeft),
                Console.WindowHeight - HeightOffset);
            int i;
            for (i = Offset; i < Math.Min(DisplayItems.Count, WindowSize.Height + Offset); i++)
            {
                OutPutDisplay(DisplayItems[i].DisplayName, i - Offset, DisplayItems[i] == Selected);
            }

            for (i -= Offset; i < WindowSize.Height; i++)
            {
                OutPutDisplay(" ", i, false);
            }

            WriteMenu();
        }

        private void WriteMenu()
        {
            var storedDetails = Stored is null ? "" : " | Copy:C | Move:M | Clear:Mod+S";
            Console.SetCursorPosition(0, Console.WindowHeight - 1);
            Console.Write(FitWidth(
                $"Mod = {Program.Config.ModKey.ToString()} | Exit:Mod+Q | Delete:Mod+D | Hidden:H | Store:S{storedDetails}"
                    .PadRight(WindowSize.Width, ' '), true));
        }


        protected void OutPutDisplay(string text, int y, bool selected)
        {
            if (WindowSize.Width != Math.Min(_maxWidth, Console.WindowWidth - _startLeft) ||
                WindowSize.Height != Console.WindowHeight - HeightOffset)
            {
                Display(DisplayItems, Offset);
                return;
            }

            Console.SetCursorPosition(_startLeft, y + 2);
            Console.BackgroundColor = selected ? Program.Config.ForegroundColor : Program.Config.BackgroundColor;
            Console.ForegroundColor = selected ? Program.Config.BackgroundColor : Program.Config.ForegroundColor;
            Console.Write(FitWidth(text, false));
        }

        private void WriteStored()
        {
            Console.BackgroundColor = Program.Config.BackgroundColor;
            Console.ForegroundColor = Program.Config.ForegroundColor;
            Console.SetCursorPosition(0, 1); // there is only one of these we force it to always be 0
            Console.Write(FitWidth(Stored?.FullPath, false));
            WriteMenu();
        }


        private string FitWidth(string format, bool keepStart) => (format ?? "").Length > WindowSize.Width
            ? keepStart
                ? (format ?? "")[..WindowSize.Width]
                : (format ?? "").Substring((format ?? "").Length - WindowSize.Width, WindowSize.Width)
            : (format ?? "").PadRight(WindowSize.Width, ' ');
    }
}
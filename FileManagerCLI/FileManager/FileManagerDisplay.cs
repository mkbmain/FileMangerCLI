using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using FileManagerCLI.Data;
using FileManagerCLI.Enums;
using FileManagerCLI.Utils;

namespace FileManagerCLI.FileManager
{
    public abstract class FileManagerDisplay
    {
        protected bool ShowHidden = true;
        protected Size WindowSize;
        private const int HeightOffset = 3;

        public delegate void LogEventHandler(object sender, LogEvent logEvent);

        public static event LogEventHandler LogEvent = null;

        private int _startLeft() =>
            StartLeftPercentPercent == 0 ? 0 : (int) (Console.WindowWidth * StartLeftPercentPercent);

        protected decimal StartLeftPercentPercent;
        private string _xxPath;
        private static StoredIoItem _xxstored; // we only want 1 stored globally
        protected decimal WidthPercent;
        protected IoItem Selected;
        protected List<IoItem> DisplayItems = new List<IoItem>();
        protected int Offset;
        public bool CalculateDirectorySize = false;

        protected StoredIoItem Stored
        {
            get => _xxstored;
            set
            {
                _xxstored = value;
                WriteStored();
            }
        }

        protected FileManagerDisplay(bool showHidden,decimal widthPercent, decimal startLeftPercent)
        {
            ShowHidden = showHidden;
            WidthPercent = widthPercent;
            StartLeftPercentPercent = startLeftPercent;
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

                var items = FileIoUtil.GetDetailsForPath(Path, CalculateDirectorySize)
                    .Where(e => ShowHidden || e.Hidden == false).ToList();
                Selected = items.First();
                Display(items, 0);
            }
        }

        protected void Display(IEnumerable<IoItem> items, int offset)
        {
            Offset = offset;
            DisplayItems = items.ToList();
            BuildDisplay(WidthPercent);

            Console.SetCursorPosition(_startLeft(), 0);
            Console.WriteLine(FitWidth(Path, false));
            WriteStored();
        }

        private void BuildDisplay(decimal widthPercent)
        {
            this.WidthPercent = widthPercent;
            WindowSize = new Size(
                Math.Min((int) (Console.WindowWidth * widthPercent), Console.WindowWidth - _startLeft()),
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

            FirstLogLineSetup();
        }

        protected bool RunWithErrorHandle(Action action, string failure)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception e)
            {
                WriteLog(this, failure, LogType.Error, e);
            }
            return false;
        }

        private static bool _firstBuild = true;

        private void FirstLogLineSetup()
        {
            if (!_firstBuild) return;
            WriteLog(this, "", LogType.Draw);
            _firstBuild = false;
        }

        protected static void WriteLog(object caller, string comment, LogType type, Exception exception = null)
        {
            Console.SetCursorPosition(0, Console.WindowHeight - 1);
            Console.BackgroundColor =  Program.Config.BackgroundColor;
            Console.ForegroundColor = type == LogType.Error ? Program.Config.ErrorLogColor : Program.Config.ForegroundColor;
            Console.Write(FitWidth(comment, true, Console.WindowWidth));
            if(type ==LogType.Draw) return;
            LogEvent?.Invoke(caller, new LogEvent {Log = comment, LogType = type, Exception = exception});
        }


        protected void OutPutDisplay(string text, int y, bool selected)
        {
            if (WindowSize.Width !=
                Math.Min((int) (Console.WindowWidth * WidthPercent), Console.WindowWidth - _startLeft()) ||
                WindowSize.Height != Console.WindowHeight - HeightOffset)
            {
                Display(DisplayItems, Offset);
                return;
            }

            Console.SetCursorPosition(_startLeft(), y + 2);
            Console.BackgroundColor = selected ? Program.Config.ForegroundColor : Program.Config.BackgroundColor;
            Console.ForegroundColor = selected ? Program.Config.BackgroundColor : Program.Config.ForegroundColor;
            Console.Write(FitWidth(text, false));
        }

        private void WriteStored()
        {
            Console.BackgroundColor = Program.Config.BackgroundColor;
            Console.ForegroundColor = Program.Config.ForegroundColor;
            Console.SetCursorPosition(0, 1); // there is only one of these we force it to always be 0
            Console.Write(FitWidth(Stored?.FullPath ?? "", false, Console.WindowWidth));
        }


        private string FitWidth(string format, bool keepStart) => FitWidth(format ?? "", keepStart, WindowSize.Width);

        protected static string FitWidth(string format, bool keepStart, int width) => format.Length > width
            ? keepStart
                ? format[..width]
                : format.Substring(format.Length - width, width)
            : format.PadRight(width, ' ');
    }
}
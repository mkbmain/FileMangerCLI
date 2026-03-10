using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileManagerCLI.Data;
using FileManagerCLI.Enums;
using FileManagerCLI.Utils;

namespace FileManagerCLI.FileManager;

public abstract class FileManagerDisplay
{
    protected bool ShowHidden;
    protected Size WindowSize;
    private const int HeightOffset = 3;

    public delegate void LogEventHandler(object sender, LogEvent logEvent);

    public static event LogEventHandler LogEvent;

    private int _startLeft() =>
        StartLeftPercentPercent == 0 ? 0 : (int)(Console.WindowWidth * StartLeftPercentPercent);

    protected decimal StartLeftPercentPercent;
    private string _path;
    private static StoredIoItem _stored;
    protected decimal WidthPercent;
    protected IoItem Selected;
    protected List<IoItem> DisplayItems = new List<IoItem>();
    protected int Offset;
    private CancellationTokenSource _sizeCts;

    protected StoredIoItem Stored
    {
        get => _stored;
        set
        {
            _stored = value;
            WriteStored();
        }
    }

    protected FileManagerDisplay(bool showHidden, decimal widthPercent, decimal startLeftPercent, string path = null)
    {
        ShowHidden = showHidden;
        WidthPercent = widthPercent;
        StartLeftPercentPercent = startLeftPercent;
        Path = path ?? Environment.CurrentDirectory;
    }

    protected string Path
    {
        get => _path;
        set
        {
            _path = value;
            if (_path.EndsWith(FileIoUtil.PathSeparator) == false)
            {
                _path += FileIoUtil.PathSeparator;
            }

            _sizeCts?.Cancel();
            _sizeCts = new CancellationTokenSource();

            var items = FileIoUtil.GetDetailsForPath(Path)
                .Where(e => ShowHidden || e.Hidden == false).ToList();
            Selected = items.FirstOrDefault() ?? Selected;
            Display(items, 0);

            if (Program.Config.DisplayItemSize)
                StartSizeComputationAsync(_sizeCts.Token);
        }
    }

    private void StartSizeComputationAsync(CancellationToken token)
    {
        var dirs = DisplayItems
            .Where(e => e.IoType == IoItemType.Directory)
            .ToList();
        var currentPath = _path;
        var snapshot = DisplayItems.ToList();
        var offset = Offset;

        Task.Run(() =>
        {
            foreach (var item in dirs)
            {
                if (token.IsCancellationRequested) return;
                item.Size = FileIoUtil.SizeOfDirectory(System.IO.Path.Combine(currentPath, item.Name), token);
                if (token.IsCancellationRequested) return;
                var index = snapshot.IndexOf(item);
                if (index >= offset && index < offset + WindowSize.Height)
                    OutPutDisplay(item.DisplayName, index - offset, item == Selected);
            }
        }, token);
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
            Math.Min((int)(Console.WindowWidth * widthPercent), Console.WindowWidth - _startLeft()),
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
        Console.BackgroundColor = Program.Config.BackgroundColor;
        Console.ForegroundColor = type == LogType.Error ? Program.Config.ErrorLogColor : Program.Config.ForegroundColor;
        Console.Write(FitWidth(comment, true, Console.WindowWidth));
        if (type == LogType.Draw) return;
        LogEvent?.Invoke(caller, new LogEvent { Log = comment, LogType = type, Exception = exception });
    }

    protected void OutPutDisplay(string text, int y, bool selected)
    {
        if (WindowSize.Width !=
            Math.Min((int)(Console.WindowWidth * WidthPercent), Console.WindowWidth - _startLeft()) ||
            WindowSize.Height != Console.WindowHeight - HeightOffset)
        {
            Display(DisplayItems, Offset);
            return;
        }

        Console.SetCursorPosition(_startLeft(), y + 2);
        Console.BackgroundColor = selected ? Program.Config.ForegroundColor : Program.Config.BackgroundColor;
        Console.ForegroundColor = selected ? Program.Config.BackgroundColor : Program.Config.ForegroundColor;
        Console.Write(FitWidth(text, Program.Config.DisplayTrimOptions == TrimOptions.TrimEnd));
    }

    private void WriteStored()
    {
        Console.BackgroundColor = Program.Config.BackgroundColor;
        Console.ForegroundColor = Program.Config.ForegroundColor;
        Console.SetCursorPosition(0, 1);
        Console.Write(FitWidth(Stored?.FullPath ?? "", false, Console.WindowWidth));
    }

    protected string ReadName(
        string prompt,
        string initial = "",
        Func<string, int, ConsoleKeyInfo, (string input, int cursorPos)> onTab = null,
        Func<string, string> validateEnter = null)
    {
        var input = initial;
        var cursorPos = initial.Length;
        var prefix = $"{prompt}: ";
        Console.CursorVisible = true;
        while (true)
        {
            Console.SetCursorPosition(0, Console.WindowHeight - 1);
            Console.BackgroundColor = Program.Config.ForegroundColor;
            Console.ForegroundColor = Program.Config.BackgroundColor;
            Console.Write(FitWidth(prefix + input, true, Console.WindowWidth));
            Console.SetCursorPosition(Math.Min(prefix.Length + cursorPos, Console.WindowWidth - 1), Console.WindowHeight - 1);

            var key = Console.ReadKey(true);
            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    var error = validateEnter?.Invoke(input);
                    if (error != null)
                    {
                        WriteLog(this, error, LogType.Info);
                        break;
                    }

                    Console.CursorVisible = false;
                    WriteLog(this, "", LogType.Draw);
                    return input;
                case ConsoleKey.Escape:
                    Console.CursorVisible = false;
                    WriteLog(this, "", LogType.Draw);
                    return null;
                case ConsoleKey.Tab:
                    if (onTab != null)
                        (input, cursorPos) = onTab(input, cursorPos, key);
                    break;
                case ConsoleKey.Backspace:
                    if (cursorPos > 0)
                    {
                        input = input[..(cursorPos - 1)] + input[cursorPos..];
                        cursorPos--;
                    }

                    break;
                case ConsoleKey.Delete:
                    if (cursorPos < input.Length)
                        input = input[..cursorPos] + input[(cursorPos + 1)..];
                    break;
                case ConsoleKey.LeftArrow:
                    if (cursorPos > 0) cursorPos--;
                    break;
                case ConsoleKey.RightArrow:
                    if (cursorPos < input.Length) cursorPos++;
                    break;
                case ConsoleKey.Home:
                    cursorPos = 0;
                    break;
                case ConsoleKey.End:
                    cursorPos = input.Length;
                    break;
                default:
                    if (!char.IsControl(key.KeyChar))
                    {
                        input = input[..cursorPos] + key.KeyChar + input[cursorPos..];
                        cursorPos++;
                    }

                    break;
            }
        }
    }

    protected void RefreshDisplay() => Display(DisplayItems, Offset);

    private string FitWidth(string format, bool keepStart) => FitWidth(format ?? "", keepStart, WindowSize.Width);

    protected static string FitWidth(string format, bool keepStart, int width) => format.Length > width
        ? keepStart
            ? format[..width]
            : format.Substring(format.Length - width, width)
        : format.PadRight(width, ' ');
}
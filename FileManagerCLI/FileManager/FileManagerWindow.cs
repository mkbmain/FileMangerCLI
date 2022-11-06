using System;
using System.IO;
using System.Linq;
using FileManagerCLI.Data;
using FileManagerCLI.Enums;
using FileManagerCLI.Utils;

namespace FileManagerCLI.FileManager
{
    public class FileManagerWindow : FileManagerDisplay
    {
        public FileManagerWindow(decimal widthPercent = 1, decimal startLeftPercent = 0) : base(widthPercent,
            startLeftPercent)
        {
        }

        public void UpdateDisplayDetails(decimal widthPercent, decimal startLeftPercent)
        {
            _widthPercent = widthPercent;
            _startLeftPercentPercent = startLeftPercent;
            Redraw();
        }

        public bool Copy()
        {
            if (Stored is null) return false;

            var newPath = System.IO.Path.Combine(Path, Stored.Name);
            if (newPath == Stored.FullPath) return false;

            switch (Stored.IoType)
            {
                case IoItemType.File:
                    File.Copy(Stored.FullPath, newPath, true);
                    break;
                case IoItemType.Directory:
                    FileIoUtil.DirectoryCopy(Stored.FullPath, newPath);
                    break;
                default:
                    return false;
            }

            Path = Path;
            return true;
        }

        public void ClearStore()
        {
            Stored = null;
        }

        public void Store()
        {
            switch (Selected.IoType)
            {
                case IoItemType.File:
                case IoItemType.Directory:
                    Stored = new StoredIoItem(Selected, Path);
                    break;
            }
        }

        public void Move()
        {
            if (!Copy()) return;
            switch (Stored.IoType)
            {
                case IoItemType.File:
                    File.Delete(Stored.FullPath);
                    break;
                case IoItemType.Directory:
                    Directory.Delete(Stored.FullPath);
                    break;
                default:
                    return;
            }

            Stored = null;
        }

        public void Delete()
        {
            var path = System.IO.Path.Combine(Path, Selected.Name);
            switch (Selected.IoType)
            {
                case IoItemType.File:
                    File.Delete(path);
                    break;
                case IoItemType.Directory:
                    Directory.Delete(path, true);
                    break;
                default:
                    return;
            }

            if (Stored?.FullPath == path)
            {
                Stored = null;
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
            switch (Selected.IoType)
            {
                case IoItemType.Directory:
                    Path = System.IO.Path.Combine(Path, Selected.Name);
                    break;
                case IoItemType.Back:
                    Path = new DirectoryInfo(Path).Parent?.FullName;
                    break;
            }
        }

        public void EditLocation()
        {
            Console.SetCursorPosition(0, 0);
            Console.Write(FitWidth(Path, false, Console.WindowWidth));
            string path = Path;
            var keys = "ABCDEFGHIJKLMNOPQRS\\/TUVWXYZ.-_".ToCharArray().GroupBy(e => e.ToString())
                .ToDictionary(w => w.Key, w => w.First());
            bool tab = false;
            while (true)
            {
                Console.CursorVisible = true;
                Console.BackgroundColor = Program.Config.ForegroundColor;
                Console.ForegroundColor = Program.Config.BackgroundColor;
                Console.SetCursorPosition(0, 0);
                Console.Write(FitWidth(path, false, Console.WindowWidth));
                
                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        if (Directory.Exists(path))
                        {
                            Path = path;
                            Console.CursorVisible = false;
                            return;
                        }

                        break;
                    case ConsoleKey.Tab:
                        var partialPathParts = path.Split(FileIoUtil.PathSeparator);
                        var partialPath = string.Join(FileIoUtil.PathSeparator,
                            partialPathParts.Take(partialPathParts.Length - 1));
                        if (!Directory.Exists(partialPath))
                        {
                            continue;
                        }

                        var items = FileIoUtil.GetDetailsForPath(partialPath)
                            .Where(e => e.IoType == IoItemType.Directory).ToList();
                        if (!items.Any())
                        {
                            continue;
                        }

                        if (tab)
                        {
                            var match = items.FirstOrDefault(w => w.Name == partialPathParts.Last());
                            if (match is null)
                            {
                                tab = false;
                                continue;
                            }

                            path = System.IO.Path.Combine(partialPath,
                                match == items.Last() ? items.Last().Name : items[items.IndexOf(match) + 1].Name);
                        }
                        else
                        {
                            var item = items.FirstOrDefault(x => x.Name.StartsWith(partialPathParts.Last()));
                            if (item is null)
                            {
                                continue;
                            }

                            tab = true;
                            path = System.IO.Path.Combine(partialPath, item.Name);
                        }
                        break;

                    case ConsoleKey.Delete:
                    case ConsoleKey.Backspace:
                        path = path[..^1];
                        break;
                    default:

                        if (keys.ContainsKey(key.KeyChar.ToString().ToUpper()))
                        {
                            path += key.Modifiers.HasFlag(ConsoleModifiers.Shift)
                                ? key.KeyChar.ToString().ToUpper()
                                : key.KeyChar.ToString().ToLower();
                        }

                        break;
                }
            }
        }

        public void Redraw() => Display(DisplayItems, Offset);

        public void MoveSelected(bool up)
        {
            var selectedIndex = DisplayItems.IndexOf(Selected);
            if ((up && selectedIndex == 0) || (!up && selectedIndex == DisplayItems.Count - 1)) return;

            var newSelectedIndex = selectedIndex + (up ? -1 : 1);
            var previous = Selected;
            Selected = DisplayItems[newSelectedIndex];

            if (newSelectedIndex > (Offset + WindowSize.Height - 1))
            {
                Offset += 10;
                Display(DisplayItems, Offset);
                return;
            }

            if (newSelectedIndex < Offset)
            {
                Offset -= 10;
                Display(DisplayItems, Offset);
                return;
            }

            OutPutDisplay(previous.DisplayName, selectedIndex - Offset, false);
            OutPutDisplay(Selected.DisplayName, newSelectedIndex - Offset, true);
        }
    }
}
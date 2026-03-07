using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileManagerCLI.Data;
using FileManagerCLI.Enums;
using FileManagerCLI.Utils;

namespace FileManagerCLI.FileManager;

public class FileManagerWindow : FileManagerDisplay
{
    public FileManagerWindow(bool showHidden = true, decimal widthPercent = 1, decimal startLeftPercent = 0) : base(
        showHidden, widthPercent,
        startLeftPercent)
    {
    }

    public void UpdateDisplayDetails(decimal widthPercent, decimal startLeftPercent)
    {
        WidthPercent = widthPercent;
        StartLeftPercentPercent = startLeftPercent;
        Redraw();
    }

    public bool Copy()
    {
        if (Stored is null) return false;

        var newPath = System.IO.Path.Combine(Path, Stored.Name);
        if (newPath == Stored.FullPath) return false;

        bool destinationExists = Stored.IoType == IoItemType.File
            ? File.Exists(newPath)
            : Directory.Exists(newPath);

        if (destinationExists)
        {
            var confirm = ReadName($"Overwrite '{Stored.Name}'? (y/n)");
            if (confirm?.ToLower() != "y")
            {
                Redraw();
                return false;
            }
        }

        bool copied;
        switch (Stored.IoType)
        {
            case IoItemType.File:
                copied = RunWithErrorHandle(() => File.Copy(Stored.FullPath, newPath, true),
                    $"Failed to Copy {Stored.FullPath}");
                break;
            case IoItemType.Directory:
                copied = RunWithErrorHandle(() => FileIoUtil.DirectoryCopy(Stored.FullPath, newPath),
                    $"Failed to Copy directory {Stored.FullPath}");
                break;
            default:
                return false;
        }

        if (!copied) return false;
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
        if (Stored is null) return;
        var destPath = System.IO.Path.Combine(Path, Stored.Name);
        if (destPath == Stored.FullPath) return;

        bool destinationExists = Stored.IoType == IoItemType.File
            ? File.Exists(destPath)
            : Directory.Exists(destPath);

        if (destinationExists)
        {
            var confirm = ReadName($"Overwrite '{Stored.Name}'? (y/n)");
            if (confirm?.ToLower() != "y")
            {
                Redraw();
                return;
            }
        }

        var stored = Stored;
        bool moved = stored.IoType switch
        {
            IoItemType.File => MoveFile(stored.FullPath, destPath),
            IoItemType.Directory => MoveDirectory(stored.FullPath, destPath),
            _ => false
        };

        if (!moved) return;
        Stored = null;
        Path = Path;
    }

    private bool MoveFile(string source, string dest)
    {
        try
        {
            File.Move(source, dest, true);
            return true;
        }
        catch (IOException)
        {
            // Cross-device move: fall back to copy + delete
            bool copied = RunWithErrorHandle(() => File.Copy(source, dest, true),
                $"Failed to Move file {dest}");
            if (copied)
                RunWithErrorHandle(() => Directory.Delete(source, true), $"Failed to clean up {source}");
            return copied;
        }
    }


    private bool MoveDirectory(string source, string dest)
    {
        try
        {
            Directory.Move(source, dest);
            return true;
        }
        catch (IOException)
        {
            // Cross-device move: fall back to copy + delete
            bool copied = RunWithErrorHandle(() => FileIoUtil.DirectoryCopy(source, dest),
                $"Failed to move directory {source}");
            if (copied)
                RunWithErrorHandle(() => Directory.Delete(source, true), $"Failed to clean up {source}");
            return copied;
        }
    }

    public void Delete()
    {
        if (Selected.IoType != IoItemType.File && Selected.IoType != IoItemType.Directory) return;
        var confirm = ReadName($"Delete '{Selected.Name}'? (y/n)");
        if (confirm?.ToLower() != "y")
        {
            Redraw();
            return;
        }

        Delete(System.IO.Path.Combine(Path, Selected.Name), Selected.IoType);
    }

    private void Delete(string path, IoItemType type)
    {
        bool deleted;
        switch (type)
        {
            case IoItemType.File:
                deleted = RunWithErrorHandle(() => File.Delete(path), $"Failed to delete file {path}");
                break;
            case IoItemType.Directory:
                deleted = RunWithErrorHandle(() => Directory.Delete(path, true),
                    $"Failed to delete directory {path}");
                break;
            default:
                return;
        }

        if (!deleted) return;

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

    public void TopDirectory()
    {
        var back = DisplayItems.FirstOrDefault(w => w.IoType == IoItemType.Back);
        if (back is null) return;

        Selected = back;
        Select();
    }

    public void Select()
    {
        switch (Selected.IoType)
        {
            case IoItemType.Directory:
                Path = System.IO.Path.Combine(Path, Selected.Name);
                break;
            case IoItemType.Back:
                var parent = new DirectoryInfo(Path).Parent?.FullName;
                if (parent != null) Path = parent;
                break;
        }
    }

    public void Rename()
    {
        if (Selected.IoType != IoItemType.File && Selected.IoType != IoItemType.Directory) return;
        var newName = ReadName($"Rename '{Selected.Name}'");
        if (string.IsNullOrWhiteSpace(newName) || newName == Selected.Name)
        {
            Redraw();
            return;
        }

        var oldPath = System.IO.Path.Combine(Path, Selected.Name);
        var newPath = System.IO.Path.Combine(Path, newName);

        bool renamed = Selected.IoType == IoItemType.File
            ? RunWithErrorHandle(() => File.Move(oldPath, newPath), $"Failed to rename {oldPath}")
            : RunWithErrorHandle(() => Directory.Move(oldPath, newPath), $"Failed to rename {oldPath}");

        if (renamed) Path = Path;
        else Redraw();
    }

    public void CreateDirectory()
    {
        var name = ReadName("New folder name");
        if (string.IsNullOrWhiteSpace(name))
        {
            Redraw();
            return;
        }

        var newPath = System.IO.Path.Combine(Path, name);
        var created = RunWithErrorHandle(() => Directory.CreateDirectory(newPath),
            $"Failed to create directory {newPath}");
        if (created) Path = Path;
        else Redraw();
    }

    public void CreateFile()
    {
        var name = ReadName("New file name");
        if (string.IsNullOrWhiteSpace(name))
        {
            Redraw();
            return;
        }

        var newPath = System.IO.Path.Combine(Path, name);
        var created = RunWithErrorHandle(() =>
            {
                using var _ = File.Create(newPath);
            },
            $"Failed to create file {newPath}");
        if (created) Path = Path;
        else Redraw();
    }

    public void EditLocation()
    {
        List<IoItem> tabItems = null;
        int tabIndex = -1;
        string lastTabInput = null;

        (string, int) TabComplete(string input, int cursorPos, ConsoleKeyInfo key)
        {
            if (input != lastTabInput) { tabItems = null; tabIndex = -1; }

            var parts = input.Split(FileIoUtil.PathSeparator);
            var parentPath = string.Join(FileIoUtil.PathSeparator, parts.Take(parts.Length - 1));
            if (!Directory.Exists(parentPath)) return (input, cursorPos);

            var isShift = key.Modifiers.HasFlag(ConsoleModifiers.Shift);

            if (tabItems == null)
            {
                var namePrefix = parts.Last();
                tabItems = FileIoUtil.GetDetailsForPath(parentPath)
                    .Where(e => e.IoType == IoItemType.Directory && e.Name.StartsWith(namePrefix))
                    .ToList();
                if (!tabItems.Any()) return (input, cursorPos);
                tabIndex = isShift ? tabItems.Count - 1 : 0;
            }
            else
            {
                tabIndex = isShift
                    ? (tabIndex - 1 + tabItems.Count) % tabItems.Count
                    : (tabIndex + 1) % tabItems.Count;
            }

            var newInput = System.IO.Path.Combine(parentPath, tabItems[tabIndex].Name);
            lastTabInput = newInput;
            return (newInput, newInput.Length);
        }

        var result = ReadName("Path", Path,
            onTab: TabComplete,
            validateEnter: p => Directory.Exists(p) ? null : $"Cannot find directory: {p}");

        if (result != null) Path = result;
    }

    public void Reload() => Path = Path;

    public void Redraw() => RefreshDisplay();

    public void MoveSelected(MoveSelected selected)
    {
        var selectedIndex = DisplayItems.IndexOf(Selected);
        bool up = selected == Enums.MoveSelected.Top || selected == Enums.MoveSelected.OneUp ||
                  selected == Enums.MoveSelected.TenUp;

        if ((up && selectedIndex == 0) || (!up && selectedIndex == DisplayItems.Count - 1)) return;

        var indexModify = 0;
        switch (selected)
        {
            case Enums.MoveSelected.OneUp:
                indexModify = -1;
                break;
            case Enums.MoveSelected.OneDown:
                indexModify = 1;
                break;
            case Enums.MoveSelected.TenUp:
                indexModify = selectedIndex < 10 ? -selectedIndex : -10;
                break;
            case Enums.MoveSelected.TenDown:
                indexModify = DisplayItems.Count - 1 - selectedIndex < 10
                    ? DisplayItems.Count - 1 - selectedIndex
                    : 10;
                break;
            case Enums.MoveSelected.Bottom:
                indexModify = DisplayItems.Count - selectedIndex - 1;
                break;
            case Enums.MoveSelected.Top:
                indexModify = -selectedIndex;
                break;
        }

        var newSelectedIndex = selectedIndex + indexModify;
        var previous = Selected;
        Selected = DisplayItems[newSelectedIndex];

        if (newSelectedIndex > Offset + WindowSize.Height - 1 || newSelectedIndex < Offset)
        {
            Offset = (newSelectedIndex / 10) * 10;
            Display(DisplayItems, Offset);
            return;
        }

        OutPutDisplay(previous.DisplayName, selectedIndex - Offset, false);
        OutPutDisplay(Selected.DisplayName, newSelectedIndex - Offset, true);
    }
}
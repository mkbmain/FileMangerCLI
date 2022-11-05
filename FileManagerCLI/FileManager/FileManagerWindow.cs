using System.IO;
using FileManagerCLI.Data;
using FileManagerCLI.Enums;
using FileManagerCLI.Utils;

namespace FileManagerCLI.FileManager
{
    public class FileManagerWindow : FileManagerDisplay
    {
        public FileManagerWindow(int maxWidth = int.MaxValue, int startLeft = 0) : base(maxWidth, startLeft)
        {
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
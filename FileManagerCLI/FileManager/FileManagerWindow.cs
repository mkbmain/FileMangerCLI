using System.IO;
using FileManagerCLI.Data;
using FileManagerCLI.Utils;

namespace FileManagerCLI.FileManager
{
    public class FileManagerWindow : FileManagerDisplay
    {
        public FileManagerWindow(int maxWidth = int.MaxValue, int startLeft = 0) : base(maxWidth = int.MaxValue,
            startLeft = 0)
        {
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
                    FileIoUtil.DirectoryCopy(_stored.FullPath, System.IO.Path.Combine(Path, _stored.Name));
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

            OutPutDisplay(previous.DisplayName, currentSlectedIndex - _offset, false);
            OutPutDisplay(_selected.DisplayName, newSelectedIndex - _offset, true);
        }
    }
}
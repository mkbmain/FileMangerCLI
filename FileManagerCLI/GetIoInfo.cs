using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileManagerCLI.Data;

namespace FileManagerCLI
{
    public static class GetIoInfo
    {
        public static string PathSeparator = System.IO.Path.Combine(" ", " ").Trim();

        public static IoItem[] GetDetailsForPath(string path)
        {
            var folders = Directory.GetDirectories(path)
                .Select(w => new DirectoryInfo(w))
                .Select(w => new IoItem
                {
                    Name = w.Name,
                    IoType = IoItemType.Directory,
                    Hidden = w.Attributes.HasFlag(FileAttributes.Hidden )
                })
                .OrderBy(w => w.Name);

            var files = Directory.GetFiles(path).Select(w => new FileInfo(w)).Select(w => new IoItem
            {
                IoType = IoItemType.File,
                Hidden = w.Attributes.HasFlag(FileAttributes.Hidden),
                Name = w.Name
            }).OrderBy(w => w.Name);

            var part = folders.Concat(files);
            
            if (path.ToCharArray().Count(x => PathSeparator.First() == x) > 1)
            {
                return new[] {new IoItem {Hidden = false, IoType = IoItemType.Back, Name = ".."}}.Concat(part)
                    .ToArray();
            }

            return part.ToArray();
        }
    }
}
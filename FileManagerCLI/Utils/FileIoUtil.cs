using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileManagerCLI.Data;
using FileManagerCLI.Enums;

namespace FileManagerCLI.Utils
{
    public static class FileIoUtil
    {
        public static readonly string PathSeparator = Path.Combine(" ", " ").Trim();

        public static IEnumerable<IoItem> GetDetailsForPath(string path)
        {
            var folders = Directory.GetDirectories(path)
                .Select(w => new DirectoryInfo(w))
                .Select(w => new IoItem
                {
                    Name = w.Name,
                    IoType = IoItemType.Directory,
                    Hidden = w.Attributes.HasFlag(FileAttributes.Hidden)
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
                return new[] {new IoItem {Hidden = false, IoType = IoItemType.Back, Name = ".."}}.Concat(part);
            }

            return part;
        }

        private static readonly string[] Suffix = {"", "K", "M", "G", "T", "P", "E"}; //Longs run out around EB

        public static string BytesToString(long byteCount)
        {
            if (byteCount == 0) return $"0{Suffix[0]}";
            var bytes = Math.Abs(byteCount);
            var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            var num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return $"{(Math.Sign(byteCount) * num)}{Suffix[place]}B";
        }

        public static void DirectoryCopy(string sourceDirName, string destDirName)
        {
            // Get the subdirectories for the specified directory.
            var dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    $"Source directory does not exist or could not be found: {sourceDirName}");
            }

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            foreach (var file in dir.GetFiles())
            {
                var tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            foreach (var directoryInfo in dir.GetDirectories())
            {
                var tempPath = Path.Combine(destDirName, directoryInfo.Name);
                DirectoryCopy(directoryInfo.FullName, tempPath);
            }
        }
    }
}
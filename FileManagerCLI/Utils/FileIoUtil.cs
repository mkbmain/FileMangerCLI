using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileManagerCLI.Data;
using FileManagerCLI.Enums;

namespace FileManagerCLI.Utils;

public static class FileIoUtil
{
    public static readonly string PathSeparator = Path.DirectorySeparatorChar.ToString();

    public static IEnumerable<IoItem> GetDetailsForPath(string path)
    {
        var folders = ProjectToIoItem(Directory.GetDirectories(path).Select(w => new DirectoryInfo(w)));
        var files = ProjectToIoItem(Directory.GetFiles(path).Select(w => new FileInfo(w)));
        var part = folders.Concat(files);

        if (new DirectoryInfo(path).Parent != null)
        {
            return new[] { new IoItem { Hidden = false, IoType = IoItemType.Back, Name = ".." } }.Concat(part);
        }

        return part;
    }

    private static readonly string[] Suffix = { "", "K", "M", "G", "T", "P", "E" };

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
        var dir = new DirectoryInfo(sourceDirName);

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException(
                $"Source directory does not exist or could not be found: {sourceDirName}");
        }

        Directory.CreateDirectory(destDirName);

        foreach (var file in dir.GetFiles())
        {
            var tempPath = Path.Combine(destDirName, file.Name);
            file.CopyTo(tempPath, true);
        }

        foreach (var directoryInfo in dir.GetDirectories())
        {
            var tempPath = Path.Combine(destDirName, directoryInfo.Name);
            DirectoryCopy(directoryInfo.FullName, tempPath);
        }
    }

    public static long SizeOfDirectory(string path)
    {
        try
        {
            return Directory.GetFiles(path).Sum(e => new FileInfo(e).Length) +
                   Directory.GetDirectories(path).Sum(e => SizeOfDirectory(e));
        }
        catch (Exception)
        {
            return -1;
        }
    }

    private static IOrderedEnumerable<IoItem> ProjectToIoItem(IEnumerable<DirectoryInfo> items)
        => ProjectToIoItem(items, IoItemType.Directory, _ => -1L);

    private static IOrderedEnumerable<IoItem> ProjectToIoItem(IEnumerable<FileInfo> items) =>
        ProjectToIoItem(items, IoItemType.File, info => info.Length);

    private static IOrderedEnumerable<IoItem> ProjectToIoItem<T>(IEnumerable<T> fileSystemInfos, IoItemType type,
        Func<T, long> size) where T : FileSystemInfo =>
        fileSystemInfos.Select(w => new IoItem
        {
            Size = size(w),
            Name = w.Name,
            IoType = type,
            Hidden = w.Attributes.HasFlag(FileAttributes.Hidden)
        }).OrderBy(w => w.Name);
}

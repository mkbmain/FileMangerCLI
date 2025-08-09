using System.IO;

namespace FileManagerCLI.Data;

public class StoredIoItem : IoItemDetails
{
    public StoredIoItem(IoItemDetails item, string path)
    {
        FullPath = Path.Combine(path, item.Name);
        Name = item.Name;
        Hidden = item.Hidden;
        IoType = item.IoType;
    }

    public string FullPath { get; }
}
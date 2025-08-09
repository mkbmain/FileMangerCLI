using FileManagerCLI.Enums;
using FileManagerCLI.Utils;

namespace FileManagerCLI.Data;

public class IoItemDetails
{
    public string Name { get; set; }

    public IoItemType IoType { get; set; }

    public bool Hidden { get; set; }

    public long Size { get; set; }

    protected string DisplaySize => Program.Config.DisplayItemSize ? FileIoUtil.BytesToString(Size).PadRight(7) : "";
}
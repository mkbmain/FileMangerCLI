using FileManagerCLI.Enums;
using FileManagerCLI.Utils;

namespace FileManagerCLI.Data;

public class IoItem : IoItemDetails
{
    public string DisplayName => IoType switch
    {
        IoItemType.Directory => $"{DisplaySize} {BuildDisplay($"\ud83d\udcc1", Program.Config.FolderIconsOptions)}{FileIoUtil.PathSeparator}{Name}",
        IoItemType.File => $"{DisplaySize} {BuildDisplay($"\ud83d\uddce", Program.Config.FileIconsOptions)}{Name}",
        _ => Name
    };

    private static string BuildDisplay(string icon, IconsOptions iconsOptions) => iconsOptions switch
    {
        IconsOptions.Show => icon,
        IconsOptions.ShowWithPadding => $"{icon} ",
        _ => string.Empty
    };
}
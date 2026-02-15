using FileManagerCLI.Enums;
using FileManagerCLI.Utils;

namespace FileManagerCLI.Data;

public class IoItem : IoItemDetails
{
    public string DisplayName => 
        IoType == IoItemType.Directory ? $"{DisplaySize} {BuildDisplay($"\ud83d\udcc1", Program.Config.FolderIconsOptions)}{FileIoUtil.PathSeparator}{Name}" :
        IoType == IoItemType.File ? $"{DisplaySize} {BuildDisplay($"\ud83d\uddce", Program.Config.FileIconsOptions)}{Name}" :
        Name;

    private static string BuildDisplay(string icon, IconsOptions iconsOptions)
    {
        switch (iconsOptions)
        {
            case IconsOptions.Show:
                return icon;
            case IconsOptions.ShowWithPadding:
                return $"{icon} ";
            case IconsOptions.Hide:
            default:
                return string.Empty;
        }
    }
}
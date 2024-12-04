using FileManagerCLI.Enums;
using FileManagerCLI.Utils;

namespace FileManagerCLI.Data
{
    public class IoItem : IoItemDetails
    {
        public string DisplayName => 
            IoType == IoItemType.Directory ? $"{DisplaySize} {(Program.Config.DisplayFolderIcons ? "\ud83d\udcc1" : "")}{FileIoUtil.PathSeparator}{Name}" :
            IoType == IoItemType.File ? $"{DisplaySize} {(Program.Config.DisplayFileIcons ? "\ud83d\uddce" : "") }{Name}" :
            Name;
    }
}
using FileManagerCLI.Enums;
using FileManagerCLI.Utils;

namespace FileManagerCLI.Data
{
    public class IoItem : IoItemDetails
    {
        public string DisplayName => IoType == IoItemType.Directory ? $"{(Program.Config.DisplayIcons ? "\ud83d\udcc1" : "")}{FileIoUtil.PathSeparator}{Name}" :
            IoType == IoItemType.File ? $"{(Program.Config.DisplayIcons ? "\ud83d\uddce" : "") }{Name}" :
            Name;
    }
}
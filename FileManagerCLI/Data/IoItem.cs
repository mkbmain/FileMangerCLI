using FileManagerCLI.Enums;
using FileManagerCLI.Utils;

namespace FileManagerCLI.Data
{
    public class IoItem : IoItemDetails
    {
        public string DisplayName => IoType == IoItemType.Directory ? $"{FileIoUtil.PathSeparator}{Name}" : Name;
    }
}
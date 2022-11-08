using FileManagerCLI.Enums;
using FileManagerCLI.Utils;

namespace FileManagerCLI.Data
{
    public class IoItemDetails
    {
        public string Name { get; set; }

        public IoItemType IoType { get; set; }

        public bool Hidden { get; set; }

        public long Size { get; set; }

        public string DisplaySize => FileIoUtil.BytesToString(Size);
    }
}
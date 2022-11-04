using FileManagerCLI.Enums;

namespace FileManagerCLI.Data
{
    public class IoItemDetails
    {
        public string Name { get; set; }

        public IoItemType IoType { get; set; }

        public bool Hidden { get; set; }
    }
}
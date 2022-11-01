namespace FileManagerCLI.Data
{
    public class IoItem
    {
        public string DisplayName => IoType == IoItemType.Directory ? $"{GetIoInfo.PathSeparator}{Name}" : Name;
        public string Name { get; set; }
        
        public IoItemType IoType { get; set; }
        
        public bool Hidden { get; set; }
        
    }

    public enum IoItemType
    {
        File,
        Directory,
        Back
    }
}
using System.IO;

namespace FileManagerCLI.Data
{
    public class StoredIoItem : IoItemDetails
    {
        public StoredIoItem(IoItemDetails item,string path)
        {
            this.FullPath = System.IO.Path.Combine(path, item.Name);
            this.Name = item.Name;
            this.Hidden = item.Hidden;
            this.IoType = item.IoType;
        }

        public string FullPath { get; set; }
    }
}
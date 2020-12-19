using System.IO;

namespace icon_manager
{
    public class Directory
    {
        public string Path { get; }
        public FileInfo FileInfo { get; }
        public DirectoryInfo DirInfo { get; }

        public Directory(string path)
        {
            Path = path;
            FileInfo = new FileInfo(path);
            DirInfo = new DirectoryInfo(path);
        }

        public static Directory FromPath(string path)
        {
            var exists = System.IO.Directory.Exists(path);
            if (!exists)
            {
                throw new DirectoryNotFoundException($"Path doesn't exist {path}");
            }

            return new Directory(path);
        }
    }

    public class Icon
    {
    }

    public class Ini
    {
    }
}
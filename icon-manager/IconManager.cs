using System.IO;
using ImageMagick;

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
        public string Path { get; }
        public FileInfo Info { get; }

        private Icon(string path)
        {
            Path = path;
            Info = new FileInfo(path);
        }

        public static Icon FromPath(string path)
        {
            var exists = File.Exists(path);
            if (!exists)
            {
                throw new FileNotFoundException($"File doesn't exist {path}");
            }

            if (!System.IO.Path.GetExtension(path).Equals(".ico"))
            {
                throw new FileFormatException($"File is not .ico {path}");
            }

            return new Icon(path);
        }

        public static Icon CreateFromImage(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"File doesn't exist {path}");
            }

            var file = new FileInfo(path);
            var icoPath = System.IO.Path.Combine(file.Directory.FullName, "icon.ico");

            // TODO: Add checking for image shape
            // TODO: Add checking of input image format
            using (var image = new MagickImage(path))
            {
                image.Scale(256, 256);
                image.Write(icoPath);
            }

            return new Icon(icoPath);
        }
    }

    public class Ini
    {
    }
}
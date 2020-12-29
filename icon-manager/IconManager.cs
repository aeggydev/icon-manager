using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImageMagick;
using Ini.Net;

namespace icon_manager
{
    public class File
    {
        public File(string path)
        {
            if (!System.IO.File.Exists(path)) throw new FileNotFoundException($"File doesn't exist {path}");

            Path = path;
            FileInfo = new FileInfo(path);
        }

        public bool IsReadOnly
        {
            get => FileInfo.IsReadOnly;
            set => FileInfo.IsReadOnly = value;
        }

        public string Path { get; }
        public FileInfo FileInfo { get; }
        public bool Exists => System.IO.File.Exists(Path);


        public bool IsHidden
        {
            get => FileInfo.Attributes.HasFlag(FileAttributes.Hidden);
            set
            {
                if (value) // Hide
                    System.IO.File.SetAttributes(Path, System.IO.File.GetAttributes(Path) | FileAttributes.Hidden);
                else // Unhide
                    System.IO.File.SetAttributes(Path, System.IO.File.GetAttributes(Path) & FileAttributes.Hidden);
            }
        }

        public bool IsSystem
        {
            get => FileInfo.Attributes.HasFlag(FileAttributes.System);
            set
            {
                if (value) // System
                    System.IO.File.SetAttributes(Path, System.IO.File.GetAttributes(Path) | FileAttributes.System);
                else // Unsystem
                    System.IO.File.SetAttributes(Path, System.IO.File.GetAttributes(Path) & FileAttributes.System);
            }
        }
    }

    public class Directory
    {
        public string Path { get; }

        private Directory(string path)
        {
            Path = path;
            DirInfo = new DirectoryInfo(path);
        }

        public bool IsReadOnly
        {
            get => DirInfo.Attributes.HasFlag(FileAttributes.ReadOnly);
            set
            {
                if (value) // System
                    System.IO.File.SetAttributes(Path, System.IO.File.GetAttributes(Path) | FileAttributes.System);
                else // Unsystem
                    System.IO.File.SetAttributes(Path, System.IO.File.GetAttributes(Path) & FileAttributes.System);
            }
        }

        public DirectoryInfo DirInfo { get; }
        public bool HasConfig => System.IO.File.Exists(System.IO.Path.Combine(Path, "desktop.ini"));
        public bool HasIcon => System.IO.File.Exists(System.IO.Path.Combine(Path, "icon.ico"));

        public Icon GetIcon()
        {
            if (!HasIcon) throw new FileNotFoundException("Icon doesn't exist");
            return Icon.FromPath(System.IO.Path.Combine(Path, "icon.ico"));
        }

        public bool SetIconIfHas()
        {
            if (HasIcon)
            {
                ChangeIcon(GetIcon());
                return true;
            }
            
            if (HasConfig)
            {
                var conf = Config.FromDir(DirInfo);
                conf.RemoveIcon();
            }
            return false;
        }

        public List<Directory> RecurseToList()
        {
            var dirs = new List<Directory> {this};
            dirs.AddRange(System.IO.Directory.GetDirectories(Path, "*.*", SearchOption.AllDirectories)
                .Select(dirName => FromPath(dirName)));

            return dirs;
        }
        
        public void RecursivelySet()
        {
            foreach (var dir in RecurseToList())
            {
                dir.SetIconIfHas();
            }
        }

        public void ChangeIcon(Icon icon)
        {
            Config conf;
            if (!HasConfig)
            {
                conf = Config.MakeFileInDir(DirInfo);
            }
            else
            {
                conf = Config.FromDir(DirInfo);
            }

            if (!icon.Exists) throw new FileNotFoundException($"Icon's file doesn't exist {icon.Path}");
            conf.IsSystem = false;
            conf.IsHidden = false;
            conf.ChangeIconPath(icon);
            conf.IsSystem = true;
            conf.IsHidden = true;

            IsReadOnly = true;
        }

        public static Directory FromPath(string path)
        {
            var exists = System.IO.Directory.Exists(path);
            if (!exists) throw new DirectoryNotFoundException($"Path doesn't exist {path}");

            return new Directory(path);
        }
    }

    public class Icon : File
    {
        private Icon(string path) : base(path)
        {
        }

        public static Icon FromPath(string path)
        {
            var exists = System.IO.File.Exists(path);
            if (!exists) throw new FileNotFoundException($"File doesn't exist {path}");

            if (!System.IO.Path.GetExtension(path).Equals(".ico"))
                throw new FileFormatException($"File is not .ico {path}");

            return new Icon(path);
        }

        public static Icon CreateFromImage(string path)
        {
            var exists = System.IO.File.Exists(path);
            if (!exists) throw new FileNotFoundException($"File doesn't exist {path}");

            var file = new FileInfo(path);
            var icoPath = System.IO.Path.Combine(file.Directory.FullName, "icon.ico");

            // TODO: Add checking for image shape
            // TODO: Add checking of input image format
            using (var image = new MagickImage(path))
            {
                image.Scale(256, 256);
                image.BackgroundColor = MagickColors.Transparent;
                image.Write(icoPath);
            }

            return new Icon(icoPath);
        }
    }

    public class Config : File
    {
        private static readonly string DefaultContent = @"[.ShellClassInfo]
iconresource =C:\Windows\System32\SHELL32.dll,5

[ViewState]
mode = 
vid = 
foldertype =Generic";

        public Config(string path) : base(path)
        {
        }

        public void ChangeIconPath(Icon icon)
        {
            var ini = new IniFile(Path);
            ini.WriteString(".ShellClassInfo", "IconResource", $"{icon.Path},0");
        }

        public static Config FromDir(DirectoryInfo directory)
        {
            return FromPath(System.IO.Path.Combine(directory.FullName, "desktop.ini"));
        }

        public static Config FromPath(string path)
        {
            if (!System.IO.File.Exists(path)) throw new FileNotFoundException($"File doesn't exist {path}");

            return new Config(path);
        }

        public void RemoveIcon()
        {
            var ini = new IniFile(Path);
            if (ini.KeyExists(".ShellClassInfo", "IconResource"))
            {
                ini.DeleteKey(".ShellClassInfo", "IconResource");
                ini.DeleteSection(".ShellClassInfo");
            }
        }

        public static Config MakeFileInDir(DirectoryInfo directory)
        {
            return MakeFileFromDefault(System.IO.Path.Combine(directory.FullName, "desktop.ini"));
        }

        public static Config MakeFileFromDefault(string path)
        {
            System.IO.File.WriteAllText(path, DefaultContent);
            return new Config(path);
        }
    }
}
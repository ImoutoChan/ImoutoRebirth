using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Utils;

namespace ImoutoViewer.Model
{
    class LocalImageList : IEnumerable
    {
        #region Static members

        #region Static fields

        private static SortMethod _filesSortMethod = SortMethod.ByName;

        private static SortMethod _foldersSortMethod = SortMethod.ByCreateDate;

        private static DirectorySearchFlags _filesGettingMethod = DirectorySearchFlags.Folder |
                                                                DirectorySearchFlags.AllDepthSubfolders;

        #endregion //Static fields

        #region Static Properties

        public static SortMethod FilesSortMethod
        {
            private get { return _filesSortMethod; }
            set { _filesSortMethod = value; }
        }

        public static bool IsFilesSortMethodDescending { private get; set; }

        public static SortMethod FoldersSortMethod
        {
            private get { return _foldersSortMethod; }
            set { _foldersSortMethod = value; }
        }

        public static bool IsFoldersSortMethodDescending { private get; set; }

        public static DirectorySearchFlags FilesGettingMethods
        {
            private get { return _filesGettingMethod; }
            set { _filesGettingMethod = value; }
        }

        #endregion //Static Properties

        #region Static methods

        private static IEnumerable<DirectoryInfo> GetDirectories(DirectoryInfo source, bool isRecursive = false)
        {
            var result = new List<DirectoryInfo>();

            try
            {
                #region Check on softlink !!Very slow
                if ((File.GetAttributes(source.FullName) & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                {
                    return result;
                }
                #endregion

                foreach (DirectoryInfo folder in source.GetDirectories().OrderByWithDirection(GetDirectoryOrderProperty, IsFoldersSortMethodDescending))
                {
                    result.Add(folder);

                    if (isRecursive)
                    {
                        result.AddRange(GetDirectories(folder, true));
                    }
                }
            }
            catch (UnauthorizedAccessException) { }

            return result;
        }


        private static bool IsFlagged(DirectorySearchFlags flags, DirectorySearchFlags value)
        {
            return (flags & value) == value;
        }

        private static object GetFilesOrderProperty(string imagePath)
        {
            switch (FilesSortMethod)
            {
                default:
                    return imagePath.Split('\\').Last();
                case SortMethod.ByCreateDate:
                    return (new FileInfo(imagePath)).CreationTimeUtc;
                case SortMethod.ByUpdateDate:
                    return (new FileInfo(imagePath)).LastWriteTimeUtc;
                case SortMethod.BySize:
                    return (new FileInfo(imagePath)).Length;
            }
        }

        private static object GetDirectoryOrderProperty(DirectoryInfo dirPath)
        {
            switch (FoldersSortMethod)
            {
                default:
                    return dirPath.Name;
                case SortMethod.ByCreateDate:
                    return dirPath.CreationTimeUtc;
                case SortMethod.ByUpdateDate:
                    return dirPath.LastWriteTimeUtc;
            }
        }

        #endregion // Static members

        #endregion //Static members

        #region Fields

        private List<LocalImage> _imageList;

        private readonly List<DirectoryInfo> _directoriesList;
        private DirectoryInfo _currentDirectory;
        private LocalImage _currentImage;

        #endregion //Fields

        #region Constructors

        public LocalImageList() : this(new string[0]) { }

        public LocalImageList(string imagePath) : this(new[] { imagePath }) { }

        public LocalImageList(IEnumerable<string> imagePaths)
        {
            _imageList = new List<LocalImage>();
            _directoriesList = new List<DirectoryInfo>();
            var paths = imagePaths as IList<string> ?? imagePaths.ToList();

            //Try process directories
            var dirs =
                from dir in paths
                where Directory.Exists(dir)
                select dir;
            var dirsList = dirs as IList<string> ?? dirs.ToList();

            if (dirsList.Any())
            {
                foreach (var dir in dirsList)
                {
                    LoadDirectories(new DirectoryInfo(dir), DirectorySearchFlags.Folder);
                }
                _currentDirectory = _directoriesList.First();
                LoadImages(_currentDirectory);
                CurrentImage = _imageList.First();

                IsDirectoryActive = true;
                return;
            }

            //Processing files
            var images =
                from image in paths
                where image.IsImage()
                select image;
            var imagesList = images as IList<string> ?? images.ToList();

            int imagesCount = imagesList.Count();

            if (imagesCount > 1)
            {
                LoadImages(imagesList);
                CurrentImage = _imageList.First();
                _directoriesList.Add((new FileInfo(CurrentImage.Path)).Directory);
                _currentDirectory = _directoriesList.First();
                IsDirectoryActive = false;
            }
            else if (imagesCount == 1)
            {
                // Load directories
                var image = new FileInfo(imagesList.First());
                LoadDirectories(image.Directory, FilesGettingMethods);

                // Set current directory
                bool flag = false;
                foreach (var item in _directoriesList.Where(item => item.FullName == image.DirectoryName))
                {
                    _currentDirectory = item;
                    flag = true;
                    break;
                }
                if (!flag)
                {
                    _currentDirectory = _directoriesList.First();
                }

                // Load images
                LoadImages(_currentDirectory);

                // Detect current image
                var file =
                    from i in _imageList
                    where i.Path == image.FullName
                    select i;

                var localImages = file as IList<LocalImage> ?? file.ToList();
                CurrentImage = (localImages.Any()) ? localImages.First() : _imageList.First();
                IsDirectoryActive = true;
            }
            else
            {
                IsEmpty = true;
                _imageList.Add(LocalImage.GetEmptyImage());
                CurrentImage = _imageList.First();
                IsDirectoryActive = false;
            }
        }

        #endregion //Constructors

        #region Properties

        public LocalImage CurrentImage
        {
            get { return _currentImage; }
            private set
            {
                _currentImage = value;
                OnCurrentImageChanged();
            }
        }

        public int ImagesCount
        {
            get
            {
                return _imageList.Count;
            }
        }

        public int CurrentImageIndex
        {
            get
            {
                return _imageList.IndexOf(CurrentImage);
            }
        }

        public DirectoryInfo CurrentDirectory
        {
            get
            {
                return _currentDirectory;
            }
        }

        public int DirectoriesCount
        {
            get
            {
                return _directoriesList.Count;
            }
        }

        public int CurrentDirectoryIndex
        {
            get
            {
                return _directoriesList.IndexOf(_currentDirectory);
            }
        }

        public bool IsEmpty { get; private set; }

        public bool IsDirectoryActive { get; private set; }

        #endregion //Properties

        #region Public methods

        public void Next()
        {
            if (IsEmpty)
            {
                return;
            }

            CurrentImage.FreeMemory();
            CurrentImage.ResetZoom();

            int currentIndex = _imageList.IndexOf(CurrentImage);
            int maxIndex = _imageList.Count;
            currentIndex++;

            if (currentIndex > maxIndex - 1)
            {
                NextDirectory();
                currentIndex = 0;
            }

            CurrentImage = _imageList[currentIndex];
        }

        public void Previous()
        {
            if (IsEmpty)
            {
                return;
            }
            CurrentImage.FreeMemory();
            CurrentImage.ResetZoom();

            int currentIndex = _imageList.IndexOf(CurrentImage);
            currentIndex--;

            if (currentIndex < 0)
            {
                PrevDirectory();
                currentIndex = _imageList.Count - 1;
            }

            CurrentImage = _imageList[currentIndex];
        }

        public void NextFolder()
        {
            NextDirectory();
            CurrentImage = _imageList.First();
        }

        public void PrevFolder()
        {
            PrevDirectory();
            CurrentImage = _imageList.Last();
        }

        internal void ResortFiles()
        {
            _imageList = new List<LocalImage>(_imageList.OrderByWithDirection(x => GetFilesOrderProperty(x.Path), IsFilesSortMethodDescending));
        }

        #endregion //Public methods

        #region Methods

        private void LoadImages(IEnumerable<string> imagePaths)
        {

            var images =
                imagePaths.Where(x=>x.IsImage())
                    .OrderByWithDirection(GetFilesOrderProperty, IsFilesSortMethodDescending)
                    .Select(x => new LocalImage(x));

            _imageList.AddRange(images);
        }

        private void LoadImages(DirectoryInfo sourceFolder)
        {
            if (!sourceFolder.Exists)
            {
                throw new Exception("Directory not found.");
            }

            var files = Directory.GetFiles(sourceFolder.FullName, "*.*")
                    .Where(x=>x.IsImage())
                    .OrderByWithDirection(GetFilesOrderProperty, IsFilesSortMethodDescending)
                    .Select(x => new LocalImage(x));

            var localImages = files as IList<LocalImage> ?? files.ToList();

            if (!localImages.Any())
            {
                throw new Exception("There are no image files in directory.");
            }

            _imageList = new List<LocalImage>();
            _imageList.AddRange(localImages);
        }

        private void LoadDirectories(DirectoryInfo sourceFolder, DirectorySearchFlags flags)
        {
            if (!sourceFolder.Exists)
            {
                throw new Exception("Директория не найдена");
            }


            if (IsFlagged(flags, DirectorySearchFlags.Folder))
            {
                AddDirectory(sourceFolder);
            }

            if (IsFlagged(flags, DirectorySearchFlags.AllDepthSubfolders))
            {
                AddDirectoryRange(GetDirectories(sourceFolder, true));
            }
            else if (IsFlagged(flags, DirectorySearchFlags.Subfolders))
            {
                AddDirectoryRange(GetDirectories(sourceFolder));
            }


            if (IsFlagged(flags, DirectorySearchFlags.AllDepthPrefolder))
            {
                DirectoryInfo workfolder = sourceFolder;
                while (workfolder.Parent != null)
                {
                    AddDirectory(workfolder.Parent);

                    foreach (var item in GetDirectories(workfolder.Parent)
                        .Where(item => item.FullName != workfolder.FullName))
                    {
                        if (IsFlagged(flags, DirectorySearchFlags.Folder))
                        {
                            AddDirectory(item);
                        }

                        if (IsFlagged(flags, DirectorySearchFlags.AllDepthSubfolders))
                        {
                            AddDirectoryRange(GetDirectories(item, true));
                        }
                        else if (IsFlagged(flags, DirectorySearchFlags.Subfolders))
                        {
                            AddDirectoryRange(GetDirectories(item));
                        }
                    }
                    workfolder = workfolder.Parent;
                }
            }
            else if (IsFlagged(flags, DirectorySearchFlags.Prefolders) && sourceFolder.Parent != null)
            {
                AddDirectory(sourceFolder.Parent);

                foreach (var item in GetDirectories(sourceFolder.Parent)
                    .Where(item => item.FullName != sourceFolder.FullName))
                {
                    if (IsFlagged(flags, DirectorySearchFlags.Folder))
                    {
                        AddDirectory(item);
                    }

                    if (IsFlagged(flags, DirectorySearchFlags.AllDepthSubfolders))
                    {
                        AddDirectoryRange(GetDirectories(item, true));
                    }
                    else if (IsFlagged(flags, DirectorySearchFlags.Subfolders))
                    {
                        AddDirectoryRange(GetDirectories(item));
                    }
                }
            }
            IsDirectoryActive = true;
        }

        private void AddDirectory(DirectoryInfo sourceFolder)
        {
            if (!sourceFolder.Exists)
            {
                throw new Exception("Directory not found.");
            }
            try
            {
                var files =
                    from file in Directory.GetFiles(sourceFolder.FullName, "*.*")
                    where file.IsImage()
                    select file;

                if (files.Any())
                {
                    _directoriesList.Add(sourceFolder);
                }
            }
            catch { }
        }

        private void AddDirectoryRange(IEnumerable<DirectoryInfo> sourceFolders)
        {
            foreach (var item in sourceFolders)
            {
                AddDirectory(item);
            }
        }

        private void NextDirectory()
        {
            if (!IsDirectoryActive)
            {
                return;
            }

            int currentIndex = _directoriesList.IndexOf(_currentDirectory);
            int maxIndex = _directoriesList.Count;
            currentIndex++;

            if (currentIndex > maxIndex - 1)
            {
                currentIndex = 0;
            }

            _currentDirectory = _directoriesList[currentIndex];

            try
            {
                LoadImages(_currentDirectory);
            }
            catch
            {
                NextDirectory();
            }
        }

        private void PrevDirectory()
        {
            if (!IsDirectoryActive)
            {
                return;
            }

            int currentIndex = _directoriesList.IndexOf(_currentDirectory);
            int maxIndex = _directoriesList.Count;
            currentIndex--;

            if (currentIndex < 0)
            {
                currentIndex = maxIndex - 1;
            }

            _currentDirectory = _directoriesList[currentIndex];

            try
            {
                LoadImages(_currentDirectory);
            }
            catch
            {
                PrevDirectory();
            }
        }

        #endregion //Methods

        #region Events

        public event EventHandler CurrentImageChanged;
        private void OnCurrentImageChanged()
        {
            if (CurrentImageChanged != null)
            {
                CurrentImageChanged(this, new EventArgs());
            }
        }

        #endregion //Events

        #region IEnumerable members

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable) _imageList).GetEnumerator();
        }

        #endregion // IEnumerable members
    }
}

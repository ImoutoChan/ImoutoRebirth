using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ImoutoViewer.Model
{
    class LocalImageList : IEnumerable
    {
        #region Static members

        #region Static fields

        private static SortMethod _filesSortMethod = SortMethod.ByName;

        private static SortMethod _foldersSortMethod = SortMethod.ByCreateDate;

        private static FilesGettingMethod _filesGettingMethod = FilesGettingMethod.Folder |
                                                                FilesGettingMethod.AllDepthSubfolders;

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

        public static FilesGettingMethod FilesGettingMethods
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

        private static bool IsImage(string file)
        {
            var ci = new CultureInfo("en-US");
            const string formats = @".jpg|.png|.jpeg|.bmp|.gif|.tiff";

            return formats.Split('|').Any(item => file.EndsWith(item, true, ci));
        }

        private static bool IsFlagged(FilesGettingMethod flags, FilesGettingMethod value)
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

            var images =
                from image in imagePaths
                where IsImage(image)
                select image;

            var foundImages = images as IList<string> ?? images.ToList();

            int imagesCount = foundImages.Count();
            if (imagesCount > 1)
            {
                LoadImages(foundImages);
            }
            else if (imagesCount == 1)
            {
                // Load directories
                var image = new FileInfo(foundImages.First());
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
            }
            else
            {
                IsEmpty = true;
                _imageList.Add(LocalImage.GetEmptyImage());
                CurrentImage = _imageList.First();
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
                imagePaths.Where(IsImage)
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
                    .Where(IsImage)
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

        private void LoadDirectories(DirectoryInfo sourceFolder, FilesGettingMethod flags)
        {
            if (!sourceFolder.Exists)
            {
                throw new Exception("Директория не найдена");
            }


            if (IsFlagged(flags, FilesGettingMethod.Folder))
            {
                AddDirectory(sourceFolder);
            }

            if (IsFlagged(flags, FilesGettingMethod.AllDepthSubfolders))
            {
                AddDirectoryRange(GetDirectories(sourceFolder, true));
            }
            else if (IsFlagged(flags, FilesGettingMethod.Subfolders))
            {
                AddDirectoryRange(GetDirectories(sourceFolder));
            }


            if (IsFlagged(flags, FilesGettingMethod.AllDepthPrefolder))
            {
                DirectoryInfo workfolder = sourceFolder;
                while (workfolder.Parent != null)
                {
                    AddDirectory(workfolder.Parent);

                    foreach (var item in GetDirectories(workfolder.Parent)
                        .Where(item => item.FullName != workfolder.FullName))
                    {
                        if (IsFlagged(flags, FilesGettingMethod.Folder))
                        {
                            AddDirectory(item);
                        }

                        if (IsFlagged(flags, FilesGettingMethod.AllDepthSubfolders))
                        {
                            AddDirectoryRange(GetDirectories(item, true));
                        }
                        else if (IsFlagged(flags, FilesGettingMethod.Subfolders))
                        {
                            AddDirectoryRange(GetDirectories(item));
                        }
                    }
                    workfolder = workfolder.Parent;
                }
            }
            else if (IsFlagged(flags, FilesGettingMethod.Prefolders) && sourceFolder.Parent != null)
            {
                AddDirectory(sourceFolder.Parent);

                foreach (var item in GetDirectories(sourceFolder.Parent)
                    .Where(item => item.FullName != sourceFolder.FullName))
                {
                    if (IsFlagged(flags, FilesGettingMethod.Folder))
                    {
                        AddDirectory(item);
                    }

                    if (IsFlagged(flags, FilesGettingMethod.AllDepthSubfolders))
                    {
                        AddDirectoryRange(GetDirectories(item, true));
                    }
                    else if (IsFlagged(flags, FilesGettingMethod.Subfolders))
                    {
                        AddDirectoryRange(GetDirectories(item));
                    }
                }
            }
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
                    where IsImage(file)
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

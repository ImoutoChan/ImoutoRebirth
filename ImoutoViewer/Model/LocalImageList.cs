using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ImoutoViewer.Model
{
    class LocalImageList : IEnumerable
    {
        private const FilesGettingMethod DefaultFilesGettingMethod = FilesGettingMethod.Folder | FilesGettingMethod.AllDepthSubfolders; 

        #region Fields

        private List<LocalImage> _imageList;
        private LocalImage _currnetImage;

        private FilesGettingMethod _currentLoadedFlags;

        private List<DirectoryInfo> _directoriesList;
        private DirectoryInfo _currentDirectory;

        #endregion //Fields

        #region Constructors

        public LocalImageList(FilesGettingMethod filesGettingMethod = DefaultFilesGettingMethod) : this(new string[0], filesGettingMethod) { }

        public LocalImageList(string imagePath, FilesGettingMethod filesGettingMethod = DefaultFilesGettingMethod) : this(new[] { imagePath }, filesGettingMethod) { }

        public LocalImageList(IEnumerable<string> imagePaths, FilesGettingMethod filesGettingMethod = DefaultFilesGettingMethod)
        {
            _imageList = new List<LocalImage>();
            _directoriesList = new List<DirectoryInfo>();

            var images =
                 from i in imagePaths
                 where IsImage(i)
                 select i;

            var enumImages = images as IList<string> ?? images.ToList();
            if (enumImages.Count() > 1)
            {
                foreach (var item in enumImages)
                {
                    _imageList.Add(new LocalImage(item));
                }
            }
            else if (enumImages.Count() == 1)
            {
                var image = new FileInfo(enumImages.First());

                LoadDirectories(image.Directory, filesGettingMethod);

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

                LoadImages(_currentDirectory);

                var file =
                    from i in _imageList
                    where i.Path == image.FullName
                    select i;

                var localImages = file as IList<LocalImage> ?? file.ToList();
                _currnetImage = (localImages.Any()) ? localImages.First() : _imageList.First();
            }
            else
            {
                IsEmpty = true;
                _imageList.Add(LocalImage.GetEmptyImage());
                _currnetImage = _imageList.First();
            }
        }

        #endregion //Constructors

        #region Properties

        public LocalImage CurrentImage
        {
            get
            {
                return _currnetImage;
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
                return _imageList.IndexOf(_currnetImage);
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

        public void Add(LocalImage item)
        {
            _imageList.Add(item);
        }

        public LocalImage Next()
        {
            if (IsEmpty)
            {
                return CurrentImage;
            }

//#if DEBUG
//            LocalImageList.Add(GC.GetTotalMemory(false));
//#endif

            _currnetImage.FreeMemory();
            _currnetImage.ResetZoom();

            int currentIndex = _imageList.IndexOf(_currnetImage);
            int maxIndex = _imageList.Count;
            currentIndex++;

            if (currentIndex > maxIndex - 1)
            {
                NextDirectory();
                currentIndex = 0;
            }

            _currnetImage = _imageList[currentIndex];

            return _currnetImage;
        }

        public LocalImage Previous()
        {
            if (IsEmpty)
            {
                return CurrentImage;
            }
            _currnetImage.FreeMemory();
            _currnetImage.ResetZoom();

            int currentIndex = _imageList.IndexOf(_currnetImage);
            currentIndex--;

            if (currentIndex < 0)
            {
                PrevDirectory();
                currentIndex = _imageList.Count - 1;
            }

            _currnetImage = _imageList[currentIndex];
            return _currnetImage;
        }

        #endregion //Public methods

        #region Methods

        private void LoadImages(DirectoryInfo sourceFolder)
        {
            if (!sourceFolder.Exists)
            {
                throw new Exception("Directory not found.");
            }

            var files =
                from file in Directory.GetFiles(sourceFolder.FullName, "*.*")
                where IsImage(file)
                select new LocalImage(file);

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


            if (IsFlagged(flags, FilesGettingMethod.Folder) && !IsFlagged(_currentLoadedFlags, FilesGettingMethod.Folder))
            {
                AddDirectory(sourceFolder);
            }

            if (IsFlagged(flags, FilesGettingMethod.AllDepthSubfolders) 
                && !IsFlagged(_currentLoadedFlags, FilesGettingMethod.AllDepthSubfolders))
            {
                AddDirectoryRange(GetDirectories(sourceFolder, true));
            }
            else if (IsFlagged(flags, FilesGettingMethod.Subfolders) 
                && !IsFlagged(_currentLoadedFlags, FilesGettingMethod.Subfolders | FilesGettingMethod.AllDepthSubfolders))
            {
                AddDirectoryRange(GetDirectories(sourceFolder));
            }


            if (IsFlagged(flags, FilesGettingMethod.AllDepthPrefolder) 
                && !IsFlagged(_currentLoadedFlags, FilesGettingMethod.AllDepthPrefolder))
            {
                DirectoryInfo workfolder = sourceFolder;
                while (workfolder.Parent != null)
                {
                    AddDirectory(workfolder.Parent);

                    foreach (var item in GetDirectories(workfolder.Parent))
                    {
                        if (item.FullName == workfolder.FullName)
                        {
                            continue;
                        }

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
            else if (IsFlagged(flags, FilesGettingMethod.Prefolders) 
                && !IsFlagged(_currentLoadedFlags, FilesGettingMethod.Prefolders | FilesGettingMethod.AllDepthPrefolder))
            {
                if (sourceFolder.Parent != null)
                {
                    AddDirectory(sourceFolder.Parent);

                    foreach (var item in GetDirectories(sourceFolder.Parent))
                    {
                        if (item.FullName == sourceFolder.FullName)
                        {
                            continue;
                        }

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
            _currentLoadedFlags = flags;
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

        #region Static methods

        private static IEnumerable<DirectoryInfo> GetDirectories(DirectoryInfo source, bool isRecursive = false)
        {
            var result = new List<DirectoryInfo>();

            try
            {
                if ((File.GetAttributes(source.FullName) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
                {
                    foreach (DirectoryInfo folder in source.GetDirectories())
                    {
                        result.Add(folder);

                        if (isRecursive)
                        {
                            result.AddRange(GetDirectories(folder, true));
                        }
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

        #endregion // Static members

        #region IEnumerable members

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable) _imageList).GetEnumerator();
        }

        #endregion // IEnumerable members
    }

#if DEBUG
    public class DebugClass
    {
        public static Stopwatch StopWatch = new Stopwatch();
        public static long CurrentTime = 0;

        public static void Add(long size)
        {
            if (StopWatch.IsRunning)
            {
                StopWatch.Stop();
                CurrentTime += StopWatch.ElapsedMilliseconds;
            }

            MemoryStat.Add(CurrentTime, size);
            StopWatch.Start();

        }

        public static Dictionary<long, long> MemoryStat = new Dictionary<long, long>();

        public static void Save()
        {
            using(var sw = new StreamWriter(@"log.txt"))
            {
                foreach (var item in MemoryStat)
	            {
                    sw.WriteLine("{0}\t{1}", item.Key, item.Value);
	            }
            }
        }
    }
#endif

}

//using ImagesDBLibrary.Model;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ImoutoNavigator.ViewModel
//{
//    class SourceVM : VMBase
//    {
//        #region Fields

//        private SourceM _source;
//        private CollectionM _collection;
//        private bool _isLoading;
//        private string _path = "_undefined_";

//        #endregion Fields

//        #region Constructors

//        public SourceVM(string path, CollectionM collection)
//        {
//            _collection = collection;
//            _path = path;

//            AddSourceAsyns(path);
//        }

//        public SourceVM(SourceM source, CollectionM collection)
//        {
//            _source = source;
//            _collection = collection;
//        }

//        #endregion Constructors

//        #region Properties

//        public bool IsLoading
//        {
//            get
//            {
//                return _isLoading;
//            }
//            private set
//            {
//                _isLoading = value;
//                OnPropertyChanged("IsLoading");
//            }
//        }

//        public SourceM Source { get { return _source; } }
        
//        public string Path
//        {
//            get
//            {
//                if (_source != null)
//                {
//                    return _source.Path;
//                }
//                else
//                {
//                    return _path;
//                }
//            }
//        }

//        #endregion Properties

//        #region Public methods

//        public void Remove()
//        {
//            RemoveSourceAsync();
//        }

//        #endregion Public methods

//        #region Private methods

//        private async void AddSourceAsyns(string path)
//        {
//            IsLoading = true;
//            _source = await AddSourceAsyncTask(path);
//            IsLoading = false;

//            if (_collection.IsActive)
//            {
//                _collection.Activate();
//            }
//        }

//        private Task<SourceM> AddSourceAsyncTask(string path)
//        {
//            return Task.Run<SourceM>(() => _collection.AddSource(path));
//        }

//        private async void RemoveSourceAsync()
//        {
//            IsLoading = true;
//            await RemoveSourceAsyncTask();
//            IsLoading = false;

//            if (_collection.IsActive)
//            {
//                _collection.Activate();
//            }
//        }

//        private Task RemoveSourceAsyncTask()
//        {
//            return Task.Run(() => _collection.RemoveSource(_source));
//        }

//        #endregion Private methods
//    }
//}

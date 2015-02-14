//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using ImagesDBLibrary.Model;
//using System.Collections.ObjectModel;
//using System.Windows.Input;
//using ImoutoNavigator.Commands;
//using System.Windows.Forms;
//using System.IO;

//namespace ImoutoNavigator.ViewModel
//{
//    class CollectionVM : VMBase
//    {
//        private CollectionM _collection;
//        private ObservableCollection<SourceVM> _sourcesCollection;
//        private ICommand _addSourceCommand;
//        private ICommand _removeSourceCommand;

//        public CollectionVM(CollectionM collection = null, string newCollectionName = null)
//        {
//            _collection = collection ?? CollectionM.Create(newCollectionName);

//            LoadSources();

//            _sourcesCollection.CollectionChanged += _sourcesCollection_CollectionChanged;
//        }

//        #region Properties

//        public string Name
//        {
//            get
//            {
//                return _collection.Name;
//            }
//            set
//            {
//                _collection.Rename(value);
//            }
//        }

//        public ObservableCollection<SourceVM> Sources
//        {
//            get
//            {
//                return _sourcesCollection;
//            }
//        }

//        public SourceVM SelectedSource { get; set; }

//        public bool IsActive 
//        { 
//            get
//            {
//                return _collection.IsActive;
//            }
//        }

//        #endregion Properties

//        #region Methods
        
//        private void LoadSources()
//        {
//            _sourcesCollection = new ObservableCollection<SourceVM>(_collection.Sources.Select(x => new SourceVM(x, _collection)));
//        }

//        public void Remove()
//        {
//            _collection.Remove();
//        }

//        public void Rename(string newName)
//        {
//            _collection.Rename(newName);
//        }

//        public void Activate()
//        {
//            _collection.Activate();
//        }

//        #endregion Methods

//        #region Handlers

//        private void _sourcesCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
//        {
//            if (e.NewItems != null)
//            {
//                foreach (var item in e.NewItems)
//                {
//                    if (item is SourceM)
//                    {
//                        var source = item as SourceM;
//                        _collection.AddSource(source.Path);
//                    }
//                }
//            }
//            if (e.OldItems != null)
//            {
//                foreach (var item in e.OldItems)
//                {
//                    if (item is SourceM)
//                    {
//                        var source = item as SourceM;
//                        _collection.RemoveSource(source);
//                    }
//                }
//            }
//        }

//        #endregion Handlers

//        #region Commands
        
//        public ICommand AddSourceCommand
//        {
//            get
//            {
//                return _addSourceCommand ?? (_addSourceCommand = new RelayCommand(AddSource));
//            }
//        }

//        public ICommand RemoveSourceCommand
//        {
//            get
//            {
//                return _removeSourceCommand ?? (_removeSourceCommand = new RelayCommand(RemoveSource, CanRemoveSource));
//            }
//        }

//        #endregion Commands

//        #region Command Handlers

//        private void AddSource(object param)
//        {
//            try
//            {
//                var folderBrowserDialog = new FolderBrowserDialog();

//                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
//                {
//                    var path = folderBrowserDialog.SelectedPath;
//                    _sourcesCollection.Add(new SourceVM(path, _collection));
//                }
//            }
//            catch
//            {
//            }
//        }

//        private void RemoveSource(object param)
//        {
//            if (SelectedSource != null)
//            {
//                SelectedSource.Remove();
//                _sourcesCollection.Remove(SelectedSource);
//            }
//        }

//        public bool CanRemoveSource(object param)
//        {
//            return SelectedSource != null;
//        }

//        #endregion Command Handlers
//    }
//}

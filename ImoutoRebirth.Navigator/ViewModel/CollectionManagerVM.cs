using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Imouto.WcfExchangeLibrary.Core.Data;
using ImoutoRebirth.Navigator.Commands;
using ImoutoRebirth.Navigator.WCF;

namespace ImoutoRebirth.Navigator.ViewModel
{
    class CollectionManagerVM : VMBase
    {
        #region Fields

        private ICommand _removeCommand;
        private CollectionVM _selectedCollection;

        #endregion Fields

        #region Constructors

        public CollectionManagerVM() { }

        public void ReloadCollections()
        {
            try
            {
                var sw = new Stopwatch();
                sw.Start();

                List<Collection> collections = ImoutoCollectionService.Use(imoutoService =>
                {
                    var res = imoutoService.GetCollections();
                    return res;
                });

                sw.Stop();
                Debug.WriteLine($"Collections reloaded in {sw.ElapsedMilliseconds}ms.");

                Collections.Clear();
                collections.ForEach(x => Collections.Add(new CollectionVM(x.Id, x.Name)));

                foreach (var collectionVm in Collections)
                {
                    collectionVm.LoadFolders();
                }

                SelectedCollection = Collections.FirstOrDefault();
            }
            catch (Exception ex)
            {
                App.MainWindowVM?.SetStatusError("Can't reload collections", ex.Message);
                Debug.WriteLine("Collections reload error: " + ex.Message);
            }
        }

        #endregion Constructors

        #region Properties

        public ObservableCollection<CollectionVM> Collections { get; } = new ObservableCollection<CollectionVM>();

        public CollectionVM SelectedCollection
        {
            get { return _selectedCollection; }
            set { OnPropertyChanged(ref _selectedCollection, value, () => this.SelectedCollection); }
        }

        #endregion Properties

        #region Methods
        public string Rename(object param)
        {
            try
            {
                SelectedCollection?.Rename(param as string);
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string CreateCollection(string name)
        {
            try
            {
                ImoutoCollectionService.Use(imoutoService =>
                {
                    imoutoService.CreateCollection(new Collection {Name = name});
                });

                ReloadCollections();
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Cannot create collection: " + ex.Message);
                return ex.Message;
            }
        }

        #endregion Methods

        #region Commands

        public ICommand RemoveCommand
        {
            get
            {
                return _removeCommand ?? (_removeCommand = new RelayCommand(Remove, CanDoCollectionCommand));
            }
        }

        #endregion Commands

        #region Command Handlers
        

        private void Remove(object param)
        {
            if (SelectedCollection != null)
            {
                SelectedCollection.Remove();
                ReloadCollections();
            }
        }

        private bool CanDoCollectionCommand(object param)
        {
            return SelectedCollection != null;
        }

        #endregion Command Handlers
    }
}

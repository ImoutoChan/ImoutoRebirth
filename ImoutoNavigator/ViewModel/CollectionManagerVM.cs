using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ImoutoNavigator.Commands;
using ImagesDBLibrary.Model;
using System.Collections.ObjectModel;

namespace ImoutoNavigator.ViewModel
{
    class CollectionManagerVM : VMBase
    {
        #region Fields

        private ICommand _renameCommand;
        private ICommand _removeCommand;
        private ICommand _createCommand;
        private ICommand _activateCommand;

        #endregion Fields

        #region Constructors

        public CollectionManagerVM()
        {
            Reload();
        }

        #endregion Constructors

        #region Properties

        public ObservableCollection<CollectionVM> Collections { get; set; }

        public CollectionVM SelectedCollection { get; set; }

        #endregion Properties

        #region Methods

        public void Reload()
        {
            Collections = new ObservableCollection<CollectionVM>(CollectionM.Collections.Select(x => new CollectionVM(x)));
            SelectedCollection = Collections.FirstOrDefault(x => x.IsActive);
            OnPropertyChanged("Collections");
            OnPropertyChanged("SelectedCollection");

        }

        #endregion Methods

        #region Commands

        public ICommand RenameCommand
        {
            get
            {
                return _renameCommand ?? (_renameCommand = new RelayCommand(Rename, CanDoCollectionCommand));
            }
        }

        public ICommand RemoveCommand
        {
            get
            {
                return _removeCommand ?? (_removeCommand = new RelayCommand(Remove, CanDoCollectionCommand));
            }
        }

        public ICommand ActivateCommand
        {
            get
            {
                return _activateCommand ?? (_activateCommand = new RelayCommand(Activate));
            }
        }

        #endregion Commands

        #region Command Handlers

        private void Activate(object obj)
        {
            if (SelectedCollection != null)
            {
                SelectedCollection.Activate();
            }
        }

        private void Rename(object param)
        {
            if (SelectedCollection != null)
            {
                SelectedCollection.Rename(param as string);
                Reload();
            }
        }

        private void Remove(object param)
        {
            if (SelectedCollection != null)
            {
                SelectedCollection.Remove();
                Reload();
            }
        }

        private bool CanDoCollectionCommand(object param)
        {
            return SelectedCollection != null;
        }

        public string CreateCollection(string name)
        {
            try
            {
                CollectionM.Create(name);
                Reload();
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }            
        }

        #endregion Command Handlers
    }
}

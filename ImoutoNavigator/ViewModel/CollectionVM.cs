using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImagesDBLibrary.Model;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ImoutoNavigator.Commands;
using System.Windows.Forms;
using System.IO;

namespace ImoutoNavigator.ViewModel
{
    class CollectionVM : VMBase
    {
        private CollectionM _collection;
        private ObservableCollection<SourceM> _sourcesCollection;
        private ICommand _addSourceCommand;
        private ICommand _removeSourceCommand;

        public CollectionVM(CollectionM collection = null, string newCollectionName = null)
        {
            _collection = collection ?? CollectionM.Create(newCollectionName);

            Reload();

            _sourcesCollection.CollectionChanged += _sourcesCollection_CollectionChanged;
        }

        #region Properties

        public string Name
        {
            get
            {
                return _collection.Name;
            }
            set
            {
                _collection.Rename(value);
            }
        }

        public ObservableCollection<SourceM> Sources
        {
            get
            {
                return _sourcesCollection;
            }
        }

        public SourceM SelectedSource { get; set; }

        #endregion Properties

        #region Methods
        
        private void Reload()
        {
            _sourcesCollection = new ObservableCollection<SourceM>(_collection.Sources);
            OnPropertyChanged("Sources");
        }

        public void Remove()
        {
            _collection.Remove();
        }

        public void Rename(string newName)
        {
            _collection.Rename(newName);
        }

        #endregion Methods

        #region Handlers

        private void _sourcesCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is SourceM)
                    {
                        var source = item as SourceM;
                        _collection.AddSource(source.Path);
                    }
                }
            }
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is SourceM)
                    {
                        var source = item as SourceM;
                        _collection.RemoveSource(source);
                    }
                }
            }
        }

        #endregion Handlers

        #region Commands
        
        public ICommand AddSourceCommand
        {
            get
            {
                return _addSourceCommand ?? (_addSourceCommand = new RelayCommand(AddSource));
            }
        }

        public ICommand RemoveSourceCommand
        {
            get
            {
                return _removeSourceCommand ?? (_removeSourceCommand = new RelayCommand(RemoveSource, CanRemoveSource));
            }
        }

        #endregion Commands

        #region Command Handlers

        private void AddSource(object param)
        {
            try
            {
                var fbd = new FolderBrowserDialog();
                DialogResult result = fbd.ShowDialog();
                string[] files = Directory.GetFiles(fbd.SelectedPath);
                System.Windows.Forms.MessageBox.Show("Files found: " + files.Length.ToString(), "Message");

                OpenFileDialog openFileDialog1 = new OpenFileDialog();

                openFileDialog1.FileName = "Filename will be ignored";
                openFileDialog1.CheckPathExists = true;
                openFileDialog1.ShowReadOnly = false;
                openFileDialog1.ReadOnlyChecked = true;
                openFileDialog1.CheckFileExists = false;
                openFileDialog1.ValidateNames = false;

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    // openFileDialog1.FileName should contain the folder and a dummy filename
                }

                _collection.AddSource(@"C:\Users\Владимир\Downloads\Обои\Обои\magnificent_palaces");
                Reload();
            }
            catch
            {

            }
        }

        private void RemoveSource(object param)
        {
            if (SelectedSource != null)
            {
                _collection.RemoveSource(SelectedSource);
                Reload();
            }
        }

        public bool CanRemoveSource(object param)
        {
            return SelectedSource != null;
        }

        #endregion Command Handlers
    }
}

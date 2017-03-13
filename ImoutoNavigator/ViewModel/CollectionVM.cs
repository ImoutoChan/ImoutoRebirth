using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using Imouto.Navigator.Commands;
using Imouto.Navigator.WCF;
using Imouto.WCFExchageLibrary.Data;

namespace Imouto.Navigator.ViewModel
{
    class CollectionVM : VMBase
    {
        private ICommand _addSourceCommand;
        private ICommand _removeSourceCommand;
        private int _id;
        private string _name;
        private DestinationFolderVM _destination;
        private SourceFolderVM _selectedSource;

        public CollectionVM(int id, string name)
        {
            Id = id;
            Name = name;
        }

        #region Properties

        public int Id
        {
            get { return _id; }
            set { OnPropertyChanged(ref _id, value, () => this.Id); }
        }

        public string Name
        {
            get { return _name; }
            set { OnPropertyChanged(ref _name, value, () => this.Name); }
        }

        public ObservableCollection<SourceFolderVM> Sources { get; } = new ObservableCollection<SourceFolderVM>();

        public SourceFolderVM SelectedSource
        {
            get { return _selectedSource; }
            set { OnPropertyChanged(ref _selectedSource, value, () => this.SelectedSource); }
        }

        public DestinationFolderVM Destination
        {
            get { return _destination; }
            set { OnPropertyChanged(ref _destination, value, () => this.Destination); }
        }

        #endregion Properties

        #region Methods

        public void LoadFolders()
        {
            try
            {
                var folders = ImoutoCollectionService.Use(imoutoService =>
                {
                    return imoutoService.GetFolders(this.Id);
                });

                this.Destination = null;
                this.Sources.Clear();

                foreach (var folder in folders)
                {
                    switch (folder.Type)
                    {
                        case FolderType.Destination:
                            var destinationFolderVM = new DestinationFolderVM(
                                folder.Id,
                                folder.Path,
                                folder.NeedDevideImagesByHash,
                                folder.NeedRename,
                                folder.IncorrectFormatSubpath,
                                folder.IncorrectHashSubpath,
                                folder.NonHashSubpath
                                );
                            destinationFolderVM.ResetRequest += FolderVM_ResetRequest;
                            destinationFolderVM.SaveRequest += FolderVM_SaveRequest;
                            destinationFolderVM.RemoveRequest += DestinationFolderVM_RemoveRequest;

                            this.Destination = destinationFolderVM;
                            break;
                        case FolderType.Source:
                            var sourceFolderVM = new SourceFolderVM(
                                folder.Id,
                                folder.Path,
                                folder.NeedCheckFormat,
                                folder.NeedCheckNameHash,
                                folder.Extensions,
                                folder.TagsFromSubfoder,
                                folder.AddTagFromFileName
                                );
                            sourceFolderVM.ResetRequest += FolderVM_ResetRequest;
                            sourceFolderVM.SaveRequest += FolderVM_SaveRequest;

                            this.Sources.Add(sourceFolderVM);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                App.MainWindowVM?.SetStatusError("Folders reload error", ex.Message);
                Debug.WriteLine("Folders reload error: " + ex.Message);
            }
        }
        
        public void Remove()
        {
            try
            {
                ImoutoCollectionService.Use(imoutoService =>
                {
                    imoutoService.DeleteCollection(this.Id);
                });
            }
            catch (Exception ex)
            {
                App.MainWindowVM?.SetStatusError("Can't remove collection", ex.Message);
                Debug.WriteLine("Can't remove collection: " + ex.Message);
            }
        }

        public void Rename(string newName)
        {
            try
            {
                ImoutoCollectionService.Use(imoutoService =>
                {
                    imoutoService.UpdateCollection(new Collection { Id = this.Id, Name = newName });
                });

                Name = newName;
            }
            catch (Exception ex)
            {
                App.MainWindowVM?.SetStatusError("Can't rename collection", ex.Message);
                Debug.WriteLine("Can't rename collection: " + ex.Message);
            }
        }

        #endregion Methods

        #region Handlers
        private void DestinationFolderVM_RemoveRequest(object sender, EventArgs e)
        {
            var folderVM = sender as FolderVM;

            if (folderVM.Id.HasValue)
            {
                try
                {
                    ImoutoCollectionService.Use(imoutoService =>
                    {
                        imoutoService.DeleteFolder(this.Id, folderVM.Id.Value);
                    });
                }
                catch (Exception ex)
                {
                    App.MainWindowVM?.SetStatusError("Can't remove folder", ex.Message);
                    Debug.WriteLine("Can't remove folder: " + ex.Message);
                }

            }
            else
            {
                LoadFolders();
            }
        }

        private void FolderVM_SaveRequest(object sender, System.EventArgs e)
        {
            var folderVM = sender as FolderVM;

            try
            {
                if (folderVM.Id.HasValue)
                {
                    ImoutoCollectionService.Use(imoutoService =>
                    {
                        imoutoService.UpdateFolder(this.Id, WCFMapper.MapFolder(folderVM));
                    });
                    LoadFolders();
                }
                else
                {
                    ImoutoCollectionService.Use(imoutoService =>
                    {
                        imoutoService.CreateFolder(this.Id, WCFMapper.MapFolder(folderVM));
                    });
                    LoadFolders();
                }
            }
            catch (Exception ex)
            {
                App.MainWindowVM?.SetStatusError("Can't save folder", ex.Message);
                Debug.WriteLine("Can't save folder: " + ex.Message);
            }
        }

        private void FolderVM_ResetRequest(object sender, System.EventArgs e)
        {
            LoadFolders();
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


        private ICommand _createDestinationFolderCommand;
        public ICommand CreateDestinationFolderCommand => _createDestinationFolderCommand ?? (_createDestinationFolderCommand = new RelayCommand(CreateDestinationFolder));

        private void CreateDestinationFolder(object obj)
        {
            var destinationFolderVM = new DestinationFolderVM(null, String.Empty, false, false, "!IncorrectFormat", "!IncorrectHash", "!NonHash");
            destinationFolderVM.ResetRequest += FolderVM_ResetRequest;
            destinationFolderVM.SaveRequest += FolderVM_SaveRequest;
            destinationFolderVM.RemoveRequest += DestinationFolderVM_RemoveRequest;
            Destination = destinationFolderVM;
        }

        #endregion Commands

        #region Command Handlers

        private void AddSource(object param)
        {
            var newSource = new SourceFolderVM(null, String.Empty, false, false, null, false, false);
            newSource.ResetRequest += FolderVM_ResetRequest;
            newSource.SaveRequest += FolderVM_SaveRequest;
            Sources.Add(newSource);
        }

        private void RemoveSource(object param)
        {
            if (SelectedSource != null)
            { 
                var folderVM = SelectedSource as FolderVM;

                if (folderVM.Id.HasValue)
                {
                    try
                    {
                        ImoutoCollectionService.Use(imoutoService =>
                        {
                            imoutoService.DeleteFolder(this.Id, folderVM.Id.Value);
                        });
                        LoadFolders();
                    }
                    catch (Exception ex)
                    {
                        App.MainWindowVM?.SetStatusError("Can't remove folder", ex.Message);
                        Debug.WriteLine("Can't remove folder: " + ex.Message);
                    }
                }
                else
                {
                    LoadFolders();
                }
            }
        }


        public bool CanRemoveSource(object param)
        {
            return SelectedSource != null;
        }

        #endregion Command Handlers
    }
}

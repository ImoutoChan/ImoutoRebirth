using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using ImoutoRebirth.Navigator.Commands;

namespace ImoutoRebirth.Navigator.ViewModel
{
    class DestinationFolderVM : FolderVM, IDataErrorInfo
    {
        #region Fields

        private bool _needDevideImagesByHash;
        private bool _needRename;
        private string _incorrectFormatSubpath;
        private string _incorrectHashSubpath;
        private string _nonHashSubpath;

        #endregion Fields

        #region Properties

        public bool NeedDevideImagesByHash
        {
            get { return _needDevideImagesByHash; }
            set { OnPropertyChanged(ref _needDevideImagesByHash, value, () => this.NeedDevideImagesByHash); }
        }

        public bool NeedRename
        {
            get { return _needRename; }
            set { OnPropertyChanged(ref _needRename, value, () => this.NeedRename); }
        }

        public string IncorrectFormatSubpath
        {
            get { return _incorrectFormatSubpath; }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    //value = "!IncorrectFormat";
                }

                OnPropertyChanged(ref _incorrectFormatSubpath, value, () => this.IncorrectFormatSubpath);
            }
        }

        public string IncorrectHashSubpath
        {
            get { return _incorrectHashSubpath; }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    //value = "!IncorrectHash";
                }
                OnPropertyChanged(ref _incorrectHashSubpath, value, () => this.IncorrectHashSubpath);
            }
        }

        public string NonHashSubpath
        {
            get { return _nonHashSubpath; }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    //value = "!NonHash";
                }
                OnPropertyChanged(ref _nonHashSubpath, value, () => this.NonHashSubpath);
            }
        }

        #endregion Properties

        #region Constructors

        public DestinationFolderVM(int? id,
            string path,
            bool needDevideImagesByHash,
            bool needRename,
            string incorrectFormatSubpath,
            string incorrectHashSubpath,
            string nonHashSubpath)
            : base(id, path)
        {
            NeedDevideImagesByHash = needDevideImagesByHash;
            NeedRename = needRename;
            IncorrectFormatSubpath = incorrectFormatSubpath;
            IncorrectHashSubpath = incorrectHashSubpath;
            NonHashSubpath = nonHashSubpath;
        }

        #endregion Constructors

        #region Commands

        private ICommand _removeCommand;
        public ICommand RemoveCommand
        {
            get
            {
                return _removeCommand ?? (_removeCommand = new RelayCommand((s) => OnRemoveRequest()));
            }
        }
        private ICommand _saveCommand;

        public ICommand SaveCommand
        {
            get
            {
                return _saveCommand ??
                       (_saveCommand = new RelayCommand((s) => OnSaveRequest(), (s) => String.IsNullOrWhiteSpace(Error)));
            }
        }

        #endregion Commands

        #region Events

        public event EventHandler RemoveRequest;
        private void OnRemoveRequest()
        {
            var handler = RemoveRequest;
            handler?.Invoke(this, new EventArgs());
        }

        #endregion Events

        public string this[string columnName]
        {
            get
            {
                String errorMessage = String.Empty;
                switch (columnName)
                {
                    case "Path":
                        if (String.IsNullOrWhiteSpace(Path))
                        {
                            errorMessage = "Path can't be empty";
                        }
                        else
                        {
                            try
                            {
                                var di = new DirectoryInfo(Path);
                            }
                            catch (Exception)
                            {
                                errorMessage = "Incorrect path format";
                            }
                        }
                        break;
                    case "IncorrectFormatSubpath":
                        if (String.IsNullOrWhiteSpace(IncorrectFormatSubpath))
                        {
                            errorMessage = "Can't be empty";
                        }
                        else
                        {
                            try
                            {
                                var di = new DirectoryInfo("C:\\" + IncorrectFormatSubpath);
                            }
                            catch (Exception)
                            {
                                errorMessage = "Incorrect path format";
                            }
                        }
                        break;
                    case "IncorrectHashSubpath":
                        if (String.IsNullOrWhiteSpace(IncorrectHashSubpath))
                        {
                            errorMessage = "Can't be empty";
                        }
                        else
                        {
                            try
                            {
                                var di = new DirectoryInfo("C:\\" + IncorrectHashSubpath);
                            }
                            catch (Exception)
                            {
                                errorMessage = "Incorrect path format";
                            }
                        }
                        break;
                    case "NonHashSubpath":
                        if (String.IsNullOrWhiteSpace(NonHashSubpath))
                        {
                            errorMessage = "Can't be empty";
                        }
                        else
                        {
                            try
                            {
                                var di = new DirectoryInfo("C:\\" + NonHashSubpath);
                            }
                            catch (Exception)
                            {
                                errorMessage = "Incorrect path format";
                            }
                        }
                        break;
                }
                return errorMessage;
            }
        }

        public override string Error
        {
            get
            {
                return this["Path"] + Environment.NewLine
                       + this["IncorrectFormatSubpath"] + Environment.NewLine
                       + this["IncorrectHashSubpath"] + Environment.NewLine
                       + this["NonHashSubpath"] + Environment.NewLine;
            } 
        }
    }
}

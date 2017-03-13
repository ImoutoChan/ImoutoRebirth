using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Imouto.Navigator.Commands;

namespace Imouto.Navigator.ViewModel
{
    class SourceFolderVM : FolderVM, IDataErrorInfo
    {
        #region Fields

        private bool _checkFormat;
        private bool _checkNameHash;
        private bool _tagsFromSubfolder;
        private bool _addTagFromFilename;

        #endregion Fields

        #region Constructors

        public SourceFolderVM(int? id, string path, bool checkFormat, bool checkNameHash, List<string> extensions, bool tagsFromSubfoder, bool addTagFromFilename) : base(id, path)
        {
            CheckFormat = checkFormat;
            CheckNameHash = checkNameHash;
            TagsFromSubfolder = tagsFromSubfoder;
            AddTagFromFileName = addTagFromFilename;

            SupportedExtensionsRaw = (extensions != null) ? new ObservableCollection<string>(extensions) : new ObservableCollection<string>();
        }

        #endregion Constructors

        #region Properties

        public bool CheckFormat
        {
            get
            {
                return _checkFormat;
            }
            set
            {
                OnPropertyChanged(ref _checkFormat, value, () => this.CheckFormat);
            }
        }

        public bool CheckNameHash
        {
            get
            {
                return _checkNameHash;
            }
            set
            {
                OnPropertyChanged(ref _checkNameHash, value, () => this.CheckNameHash);
            }
        }

        public string SupportedExtensions
        {
            get { return String.Join(";", SupportedExtensionsRaw); }
            set
            {
                try
                {
                    var list = value.Split(';').ToList();
                    SupportedExtensionsRaw.Clear();
                    foreach (var item in list)
                    {
                        SupportedExtensionsRaw.Add(item);
                    }
                }
                catch { }
            }
        }

        public ObservableCollection<string> SupportedExtensionsRaw { get; }

        public bool TagsFromSubfolder
        {
            get
            {
                return _tagsFromSubfolder;
            }
            set
            {
                OnPropertyChanged(ref _tagsFromSubfolder, value, () => this.TagsFromSubfolder);
            }
        }

        public bool AddTagFromFileName
        {
            get
            {
                return _addTagFromFilename;
            }
            set
            {
                OnPropertyChanged(ref _addTagFromFilename, value, () => this.AddTagFromFileName);
            }
        }

        #endregion Properties

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
                }
                return errorMessage;
            }
        }

        public override string Error
        {
            get { return this["Path"]; }
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
    }
}

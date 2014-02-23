using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Xml.Serialization;
using ImoutoNavigator.Commands;
using ImoutoNavigator.Model;

namespace ImoutoNavigator.ViewModel
{
    class MainWindowVM : VMBase
    {
        #region Fields

        private int _previewSide = 32;
        private MainWindow _view;
        private ObservableCollection<ImageEntry> _imageList;

        #endregion //Fields

        #region Constructors

        public MainWindowVM()
        {
            _view = new MainWindow { DataContext = this };
            _view.Loaded += _view_Loaded;
            _view.Show();
        }

        #endregion //Constructors

        #region Properties

        public Size PreviewSize
        {
            get
            {
                return new Size(_previewSide, _previewSide);
            }
        }

        public ObservableCollection<ImageEntry> ImageList
        {
            get { return _imageList; }
        }

        #endregion //Properties

        #region Commands

        public ICommand ZoomInCommand { get; set; }
        public ICommand ZoomOutCommand { get; set; }

        private void InitializeCommands()
        {
            ZoomInCommand = new RelayCommand(x =>
                {
                    _previewSide = Convert.ToInt32(Math.Floor(_previewSide * 1.1));
                    Reload();
                } 
            );
            ZoomOutCommand = new RelayCommand(x =>
                {
                    _previewSide = Convert.ToInt32(Math.Floor(_previewSide * 0.9));
                    Reload();
                }
            );
        }

        #endregion //Commands

        #region Methods

        private void GetImageLIst()
        {
            _imageList = new ObservableCollection<ImageEntry>(
                Directory.GetFiles(@"c:\Users\oniii-chan\Downloads\DLS\art\loli\")
                    .Where(ImageEntry.IsImage)
                    .Select(x => new ImageEntry(x, PreviewSize))
                    .ToList()
                );
        }

        private void Reload()
        {
            GetImageLIst();
            OnPropertyChanged("ImageList");
        }

        #endregion //Methods

        #region Event handlers

        private void _view_Loaded(object sender, RoutedEventArgs e)
        {
            Reload();
        }

        #endregion //Event handlers
    }
}

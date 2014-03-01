using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

        private int _previewSide = 256;
        private MainWindow _view;
        private ObservableCollection<ImageEntryVM> _imageList;
        private Thread _updateThread;

        #endregion //Fields

        #region Constructors

        public MainWindowVM()
        {
            InitializeCommands();

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

        public ObservableCollection<ImageEntryVM> ImageList
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
                    UpdatePreviews();
                } 
            );
            ZoomOutCommand = new RelayCommand(x =>
                {
                    _previewSide = Convert.ToInt32(Math.Floor(_previewSide * 0.9));
                    UpdatePreviews();
                }
            );
        }

        #endregion //Commands

        #region Methods

        private void GetImageList()
        {
            _imageList = new ObservableCollection<ImageEntryVM>(
                Directory.GetFiles(@"c:\Users\oniii-chan\Downloads\DLS\art\loli\")
                    .Where(ImageEntry.IsImage)
                    .Select(x =>
                    {
                        var im = new ImageEntryVM(x, PreviewSize);
                        im.ImageEntryChanged += im_ImageChanged;
                        return im;
                    })
                    .ToList()
                );
        }

        private void Reload()
        {
            //if (_updateThread != null)
            //{
            //   _updateThread.Abort();
            //   //_updateThread.Join();
            //}
            // _updateThread = new Thread(GetImageList);
            // _updateThread.Start();

             GetImageList();
             OnPropertyChanged("ImageList");
        }

        private void UpdatePreviews()
        {
            ImageEntry.AbortAllLoading();

            var startTime = DateTime.Now;
            
            foreach (var imageEntry in _imageList)
            {
                imageEntry.UpdatePreview(PreviewSize);
            }

            Debug.Print("!UPDATED FOR {0} ms", (DateTime.Now - startTime).TotalMilliseconds);

            OnPropertyChanged("PreviewSize");
        }

        #endregion //Methods

        #region Event handlers

        private void _view_Loaded(object sender, RoutedEventArgs e)
        {
            Reload();
        }

        void im_ImageChanged(object sender, EventArgs e)
        {
            OnPropertyChanged("ImageList");
        }

        #endregion //Event handlers
    }
}

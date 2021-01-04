using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ImoutoRebirth.Navigator.UserControls
{
    /// <summary>
    /// Interaction logic for UgoiraPlayer.xaml
    /// </summary>
    public partial class UgoiraPlayer : UserControl
    {
        private bool _isPlaying = false;
        private int _currentFrame = 0;
        private bool _loaded = false;
        private readonly SemaphoreSlim _loadLocker = new(1);

        public UgoiraPlayer()
        {
            InitializeComponent();
        }

        private BitmapSource? CurrentImage => Frames.Count > _currentFrame ? Frames[_currentFrame] : null;

        private ObservableCollection<BitmapSource> Frames { get; } = new();

        public static readonly DependencyProperty SourceProperty 
            = DependencyProperty.Register(
                nameof(Source), 
                typeof (string), 
                typeof (UgoiraPlayer), 
                new UIPropertyMetadata(null, OnSourceChanged));

        public string Source
        {
            get => (string) GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        private static async void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var path = (string) e.NewValue;
            var control = (UgoiraPlayer) d;

            if (string.IsNullOrEmpty(path))
            {
                control.UnloadUgoira();
            }
            else
            {
                await control.LoadUgoira(path);
            }
        }


        private async Task LoadUgoira(string path)
        {
            if (_loaded)
                return;

            await _loadLocker.WaitAsync();

            try
            {
                if (_loaded)
                    return;

                Frames.Clear();
                StopPlaying();

                await using var zipToOpen = new FileStream(path, FileMode.Open, FileAccess.Read);
                using var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read);

                foreach (var entry in archive.Entries)
                {
                    await using var previewStream = entry.Open();
                    await using var resultImage = new MemoryStream();
                    await previewStream.CopyToAsync(resultImage);
                    resultImage.Position = 0;

                    var frame = BitmapFrame.Create(resultImage, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

                    Frames.Add(frame);
                }

                StartPlaying();

                _loaded = true;
            }
            finally
            {
                _loadLocker.Release();
            }
        }
        private void UnloadUgoira()
        {
            StopPlaying();
            Frames.Clear();
        }

        private async void StartPlaying()
        {
            if (_isPlaying)
                return;

            _isPlaying = true;
            
            while (_isPlaying)
            {
                _currentFrame++;

                if (_currentFrame >= Frames.Count)
                    _currentFrame = 0;

                if (Frames.Count == 0)
                    return;

                SynchronizationContext.Current?.Post(x => { Image.Source = CurrentImage; }, null);

                await Task.Delay(33);
            };
        }

        private void StopPlaying()
        {
            _isPlaying = false;
        }
    }
}

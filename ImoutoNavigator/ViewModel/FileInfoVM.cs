using System.IO;
using Imouto.Utils;

namespace Imouto.Navigator.ViewModel
{
    class FileInfoVM : VMBase
    {
        private string _name;
        private string _hash;
        private long? _size;

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                OnPropertyChanged(ref _name, value, () => Name);
            }
        }

        public long? Size
        {
            get
            {
                return _size;
            }
            set
            {
                OnPropertyChanged(ref _size, value, () => Size);
            }
        }

        public string Hash
        {
            get
            {
                return _hash;
            }
            set
            {
                OnPropertyChanged(ref _hash, value, () => Hash);
            }
        }

        public void UpdateCurrentInfo(INavigatorListEntry navigatorListEntry)
        {
            if (navigatorListEntry == null)
            {
                Name = null;
                Size = null;
                Hash = null;
                return;
            }

            var fi = new FileInfo(navigatorListEntry.Path);
            if (!fi.SpeedExists())
            {
                return;
            }

            Name = fi.Name;
            Size = fi.Length;
            Hash = fi.GetMd5Checksum();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ImageViewer.Model
{
    class LocalImageList : IEnumerable
    {
        #region Fields

        private List<LocalImage> _imageList;
        private LocalImage _currnetImage;

        #endregion //Fields

        #region Constructors

        public LocalImageList(string[] imgFiles, string currentImage = null)
        {
            _imageList = new List<LocalImage>();
            foreach (var item in imgFiles)
            {
                LocalImage li = new LocalImage(item);
                _imageList.Add(li);

                if (item == currentImage)
                {
                    _currnetImage = li;
                }
            }

            if (_imageList.Count == 0)
            {
                _imageList = null;
                //throw new Exception
            }
            else
            {
                if (_currnetImage == null)
                {
                    _currnetImage = _imageList.First();
                }
            }            
        }

        #endregion //Constructors

        #region Properties

        public LocalImage CurrentImage
        {
            get
            {
                return _currnetImage;
            }
        }

        #endregion //Properties

        #region Public methods

        public void Add(LocalImage item)
        {
            _imageList.Add(item);
        }

        public LocalImage Next()
        {
            _currnetImage.FreeMemory();
            _currnetImage.ResetZoom();

            int currentIndex = _imageList.IndexOf(_currnetImage);
            int maxIndex = _imageList.Count;
            currentIndex++;

            if (currentIndex > maxIndex - 1)
            {
                currentIndex = 0;
            }

            _currnetImage = _imageList[currentIndex];
            return _currnetImage;
        }

        public LocalImage Previous()
        {
            _currnetImage.FreeMemory();
            _currnetImage.ResetZoom();

            int currentIndex = _imageList.IndexOf(_currnetImage);
            int maxIndex = _imageList.Count;
            currentIndex--;

            if (currentIndex < 0)
            {
                currentIndex = maxIndex - 1;
            }

            _currnetImage = _imageList[currentIndex];
            return _currnetImage;
        }

        #endregion //Public methods

        #region Methods
        #endregion //Methods

        #region Event handlers
        #endregion //Event handlers

        #region Events
        #endregion //Events

        #region IEnumerable members

        public IEnumerator GetEnumerator()
        {
            return (IEnumerator) _imageList.GetEnumerator();
        }

        #endregion //IEnumerable members
    }
}

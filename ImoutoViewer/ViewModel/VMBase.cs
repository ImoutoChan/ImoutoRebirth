using System;
using System.ComponentModel;
using System.Diagnostics;

namespace ImoutoViewer.ViewModel
{
    class VMBase : INotifyPropertyChanged
    {
        #region Constructors

        protected VMBase() { }

        #endregion //Constructors

        #region Debug

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,  
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] != null) return;

            string msg = "Invalid property name: " + propertyName;

            if (ThrowOnInvalidPropertyName)
            {
                throw new Exception(msg);
            }

            Debug.Fail(msg);
        }

        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        #endregion //Debug

        #region Implement INotyfyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            VerifyPropertyName(propertyName);

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion //Implement INotyfyPropertyChanged members
    }
}

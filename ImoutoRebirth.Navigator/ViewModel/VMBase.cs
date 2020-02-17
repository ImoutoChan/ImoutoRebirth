using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;

namespace ImoutoRebirth.Navigator.ViewModel
{
    class VMBase : INotifyPropertyChanged
    {
        #region Constructors

        protected VMBase() { }

        #endregion Constructors

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

        #endregion Debug

        #region Implement INotyfyPropertyChanged members

        protected void OnPropertyChanged<T>(ref T value, T newValue, Expression<Func<T>> action)
        {
            OnPropertyChanged(ref value, newValue, GetPropertyName(action));
        }

        private void OnPropertyChanged<T>(ref T value, T newValue, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(value, newValue))
                return;
            OnPropertyChanging(ref value, newValue, propertyName);
        }

        private void OnPropertyChanging<T>(ref T value, T newValue, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(value, newValue))
                return;
            value = newValue;

            RaisePropertyChanged(propertyName);
        }

        public void OnPropertyChanged<T>(Expression<Func<T>> action)
        {
            var propertyName = GetPropertyName(action);
            RaisePropertyChanged(propertyName);
        }

        public static string GetPropertyName<T>(Expression<Func<T>> action)
        {
            var expression = (MemberExpression)action.Body;
            var propertyName = expression.Member.Name;
            return propertyName;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            VerifyPropertyName(propertyName);

            RaisePropertyChanged(propertyName);
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Implement INotyfyPropertyChanged members
    }
}

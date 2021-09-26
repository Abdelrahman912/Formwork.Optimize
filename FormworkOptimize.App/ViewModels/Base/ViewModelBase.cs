using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FormworkOptimize.App.ViewModels.Base
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        #region Event Handler

        //Set the event to empty anonymous function in order not to throw NullReferenceException
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Methods

        /// <summary>
        /// Notify WPF Binding System that the property has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Sets value of the property if it is changed to new value. 
        /// Notify WPF Binding System of that change.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="backingfield">Backing field of the property.</param>
        /// <param name="value">New value of the property.</param>
        /// <param name="propertyName">Property Name.</param>
        /// <returns></returns>
        protected virtual bool NotifyPropertyChanged<T>(ref T backingfield, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingfield, value))
                return false;
            backingfield = value;
            NotifyPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }
}

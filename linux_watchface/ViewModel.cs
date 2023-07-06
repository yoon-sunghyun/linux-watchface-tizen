using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace linux_watchface
{
    public class ViewModel<T>: INotifyPropertyChanged
    {
        private T _Value;
        public T Value
        {
            get => _Value;
            set
            {
                if (!EqualityComparer<T>.Default.Equals(_Value, value))
                {
                    _Value = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

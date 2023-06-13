using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DuckTape.Core
{
    internal class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string Name = null)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Name)); }
    }
}

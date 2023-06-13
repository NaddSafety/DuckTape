using System.Windows.Media;

using DuckTape.Core;

namespace DuckTape.MVVM.ViewModel
{
    internal class MainViewModel : ObservableObject
    {
        public RelayCommand HomeViewCommand { get; set; }
        public RelayCommand InfoViewCommand { get; set; }
        public RelayCommand ResourceViewCommand { get; set; }
        public HomeViewModel HomeVm { get; set; }
        private object _currentView;
        public InfoViewModel InfoVm { get; set; }
        public ResourceViewModel ResourceVm { get; set; }

        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }




        public MainViewModel()
        {
            HomeVm = new HomeViewModel();
            InfoVm = new InfoViewModel();
            ResourceVm = new ResourceViewModel();
            CurrentView = HomeVm;
            HomeViewCommand = new RelayCommand(o => { CurrentView = HomeVm; });
            InfoViewCommand = new RelayCommand(o => { CurrentView = InfoVm; });
            ResourceViewCommand = new RelayCommand(o => { CurrentView = ResourceVm; });
        }
        private Brush _backgroundColor = Brushes.Red;
        public Brush BackgroundColor
        {
            get { return _backgroundColor; }
            set { _backgroundColor = value; OnPropertyChanged(); }
        }

    }
}

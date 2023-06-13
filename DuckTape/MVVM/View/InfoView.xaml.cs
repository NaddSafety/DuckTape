using LiveCharts;
using LiveCharts.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DuckTape.MVVM.ViewModel;

namespace DuckTape.MVVM.View
{
    /// <summary>
    /// Interaction logic for InfoView.xaml
    /// </summary>
    public partial class InfoView : UserControl
    {
        public InfoView()
        {
            InitializeComponent();
            DataContext = new InfoViewModel();
            ChartData = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "Windows",
                    Fill = new SolidColorBrush(Color.FromRgb(31, 25, 51)),
                    Values = new ChartValues<double> { 63 }
                },
                new PieSeries
                {
                    Title = "Unix",
                    Fill = new SolidColorBrush(Color.FromRgb(255,0,0)),
                    Values = new ChartValues<double> { 6 }
                },
                new PieSeries
                {
                    Title = "Mac",
                    Fill = new SolidColorBrush(Color.FromRgb(139, 0, 0)),
                    Values = new ChartValues<double> { 5 }
                },
                new PieSeries
                {
                    Title = "Android",
                    Fill = new SolidColorBrush(Color.FromRgb(0, 255, 127)),
                    Values = new ChartValues<double> { 4 }
                }
            };
            SecondChartData = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "GUI R",
                    Fill = new SolidColorBrush(Color.FromRgb(31, 25, 51)),
                    Values = new ChartValues<double> { 58 }
                },
                new PieSeries
                {
                    Title = "GUI S",
                     Fill = new SolidColorBrush(Color.FromRgb(0, 255, 127)),
                    Values = new ChartValues<double> { 1 }
                },
                new PieSeries
                {
                    Title = "GUI X A",
                     Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0)),
                    Values = new ChartValues<double> { 1 }
                },
                new PieSeries
                {
                    Title = "CTRL ESC",
                    Fill = new SolidColorBrush(Color.FromRgb(139, 0, 0)),
                    Values = new ChartValues<double> { 3 }
                },
            };
        }


        public SeriesCollection ChartData { get; set; }
        public SeriesCollection SecondChartData { get; set; }

        private void PieChart_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as PieChart).DataContext = this;

        }
    }
}

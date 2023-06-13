using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DuckTape.MVVM.View
{
    /// <summary>
    /// Interaction logic for ResourceView.xaml
    /// </summary>
    public partial class ResourceView : UserControl
    {
        public ResourceView()
        {
            InitializeComponent();
        }
        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton != null)
            {
                switch (radioButton.Content.ToString())
                {
                    case " GitHub Hak5 ":
                        Process.Start("https://github.com/hak5/usbrubberducky-payloads/tree/master/payloads/library");
                        break;
                    case " Design #1 ":
                        Process.Start("https://www.youtube.com/watch?v=OJygSefHVr0");
                        break;
                    case " Design #2 ":
                        Process.Start("https://www.youtube.com/watch?v=PzP8mw7JUzI&t=1969s");
                        break;
                    case " This Project ":
                        Process.Start("https://github.com/NaddSafety/DuckTape");
                        break;
                }
            }
        }

    }
}

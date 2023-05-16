using System.Windows;

namespace Millonaire
{
    /// <summary>
    /// Interaction logic for Task.xaml
    /// </summary>
    public partial class Task : Window
    {
        public Task()
        {
            InitializeComponent();
        }

        private void YaoLink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://en.wikipedia.org/wiki/Yao%27s_Millionaires%27_problem",
                UseShellExecute = true
            });
        }

        private void SocialistMillLink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://ru.wikipedia.org/wiki/Задача_миллионеров-социалистов",
                UseShellExecute = true
            });
        }

        private void PrimitiveRootLink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://en.wikipedia.org/wiki/Primitive_root_modulo_n",
                UseShellExecute = true
            });
        }

        private void MODPLink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://www.ietf.org/rfc/rfc3526.txt",
                UseShellExecute = true
            });
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

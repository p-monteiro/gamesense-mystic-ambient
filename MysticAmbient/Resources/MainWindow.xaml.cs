using System.Threading.Tasks;
using System.Windows;

namespace MysticAmbient.Resources
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width - 10;
            this.Top = desktopWorkingArea.Bottom - this.Height - 10;
            this.Topmost = true;
        }

        private async void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ModernWpf.Controls.ToggleSwitch ts = (ModernWpf.Controls.ToggleSwitch)sender;

            ts.IsEnabled = false;

            await Task.Delay(5000);

            ts.Toggled -= ToggleSwitch_Toggled;
            ts.IsOn = false;

            ts.Toggled += ToggleSwitch_Toggled;
            ts.IsEnabled = true;

        }
    }
}

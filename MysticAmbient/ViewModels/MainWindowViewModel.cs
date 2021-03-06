using MysticAmbient.Utils;
using System.Windows;
using System.Windows.Input;

namespace MysticAmbient.ViewModels
{
    class MainWindowViewModel
    {
        public ICommand CloseWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () =>
                    {
                        Application.Current.MainWindow.Close();
                        Application.Current.MainWindow = null;
                    }
                };
            }
        }
    }
}

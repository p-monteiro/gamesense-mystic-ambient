using MysticAmbient.Resources;
using MysticAmbient.Utils;
using System.Windows;
using System.Windows.Input;

namespace MysticAmbient.ViewModels
{
    class NotifyIconViewModel
    {
        /// <summary>
        /// Shows a window, if none is already open.
        /// </summary>
        public ICommand OpenMainWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => Application.Current.MainWindow == null,
                    CommandAction = () =>
                    {
                        Application.Current.MainWindow = new MainWindow();
                        Application.Current.MainWindow.Show();
                    }
                };
            }
        }

        public ICommand EnableLightsCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () =>
                    {
                    }
                };
            }
        }

        public ICommand DisableLightsCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () =>
                    {
                    }
                };
            }
        }

        /// <summary>
        /// Shuts down the application.
        /// </summary>
        public ICommand ExitApplicationCommand
        {
            get
            {
                return new DelegateCommand { CommandAction = () => Application.Current.Shutdown() };
            }
        }
    }
}

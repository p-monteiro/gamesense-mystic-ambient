using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using MysticAmbient.Resources;
using MysticAmbient.Utils;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MysticAmbient.ViewModels
{
    public class AppViewModel : ObservableObject
    {
        public static readonly string APP_ID = "MYS_AMB";
        public static readonly string APP_NAME = "Mystic Ambient";
        public static readonly string APP_DEV = "Pedro Monteiro";

        public GameSenseClient SseClient { get; private set; }

        public AppViewModel()
        {
            SseClient = new GameSenseClient(APP_ID, APP_NAME, APP_DEV);

            OpenMainWindowCommand = new RelayCommand(OpenMainWindow, OpenMainWindowCanExecute);
            CloseMainWindowCommand = new RelayCommand(CloseWindow);
            ExitApplicationCommand = new RelayCommand(ExitApplication);

            TryConnectSseCommand = new AsyncRelayCommand(TryConnectSse, TryConnectSseCanExecute);

            OpenMainWindowCommand.Execute(null);
            //TryConnectSseCommand.Execute(null);

        }


        public RelayCommand OpenMainWindowCommand { get; }
        private bool OpenMainWindowCanExecute() => Application.Current.MainWindow == null || Application.Current.MainWindow.GetType().Name == "AdornerWindow";
        private void OpenMainWindow()
        {
            Application.Current.MainWindow = new MainWindow
            {
                DataContext = this
            };
            Application.Current.MainWindow.Show();

            (OpenMainWindowCommand as RelayCommand).NotifyCanExecuteChanged();
        }

        public RelayCommand CloseMainWindowCommand { get; }
        public void CloseWindow()
        {
            Application.Current.MainWindow.Close();
            Application.Current.MainWindow = null;

            (OpenMainWindowCommand as RelayCommand).NotifyCanExecuteChanged();
        }

        public RelayCommand ExitApplicationCommand { get; }
        private void ExitApplication() { Application.Current.Shutdown(); }

        public AsyncRelayCommand TryConnectSseCommand { get; }
        private bool TryConnectSseCanExecute() => !IsConnecting && !IsConnected;
        private async Task TryConnectSse()
        {
            IsOpen = true;
            IsConnecting = true;
            (TryConnectSseCommand as AsyncRelayCommand).NotifyCanExecuteChanged();

            ConnectStatus = "Connecting to GameSense";
            await Task.Delay(1000);

            IsConnected = await SseClient.InitGameSenseAsync();
            IsConnecting = false;
            (TryConnectSseCommand as AsyncRelayCommand).NotifyCanExecuteChanged();

            if (IsConnected)
            {
                ConnectStatus = "Connected to GameSense";
                await Task.Delay(1000);
                IsOpen = false;
            }
            else
            {
                ConnectStatus = "Failed to connect to GameSense";
            }
            
        }

        private string _connectStatus;
        public string ConnectStatus { get => _connectStatus; private set => SetProperty(ref _connectStatus, value); }

        private bool _isConnecting;
        public bool IsConnecting { get => _isConnecting; private set { SetProperty(ref _isConnecting, value); OnPropertyChanged("ShowTryAgain"); } }

        private bool _isConnected;
        public bool IsConnected { get => _isConnected; private set { SetProperty(ref _isConnected, value); OnPropertyChanged("ShowTryAgain"); } }

        private bool _isOpen = false;
        public bool IsOpen { get => _isOpen; private set { SetProperty(ref _isOpen, value);} }

        public bool ShowTryAgain { get => !IsConnected && !IsConnecting; }
    }
}

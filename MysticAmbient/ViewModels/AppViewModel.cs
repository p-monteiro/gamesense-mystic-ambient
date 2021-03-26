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
            TryConnectSseCommand.Execute(null);

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

        #region Connect to SSE

        public AsyncRelayCommand TryConnectSseCommand { get; }
        private bool TryConnectSseCanExecute() => !IsConnecting && !IsConnected;
        private async Task TryConnectSse()
        {
            IsConnectionStatusPanelOpen = true;
            IsConnecting = true;
            TryConnectSseCommand.NotifyCanExecuteChanged();

            ConnectStatusLabel = "Connecting to GameSense";
            await Task.Delay(1000);

            IsConnected = await SseClient.InitGameSenseAsync();
            IsConnecting = false;
            TryConnectSseCommand.NotifyCanExecuteChanged();

            if (IsConnected)
            {
                ConnectStatusLabel = "Connected to GameSense";
                await Task.Delay(1000);
                IsConnectionStatusPanelOpen = false;
            }
            else
            {
                ConnectStatusLabel = "Failed to connect to GameSense";
            }

        }

        private bool _isConnectionStatusPanelOpen = false;
        public bool IsConnectionStatusPanelOpen { get => _isConnectionStatusPanelOpen; private set { SetProperty(ref _isConnectionStatusPanelOpen, value); } }

        private string _connectionStatusLabel;
        public string ConnectStatusLabel { get => _connectionStatusLabel; private set => SetProperty(ref _connectionStatusLabel, value); }

        private bool _isConnecting;
        public bool IsConnecting { get => _isConnecting; private set { SetProperty(ref _isConnecting, value); OnPropertyChanged("ShowTryAgain"); } }

        private bool _isConnected;
        public bool IsConnected { get => _isConnected; private set { SetProperty(ref _isConnected, value); OnPropertyChanged("ShowTryAgain"); } }

        public bool ShowTryAgain { get => !IsConnected && !IsConnecting; }

        #endregion

        private bool _isEnabled = false;
        public bool IsEnabled
        {
            get => _isEnabled; 
            set
            {
                TryEnableSSE();
            }
        }

        private bool _isEnabling = true;
        public bool IsEnabling
        {
            get => _isEnabling;
            private set => SetProperty(ref _isEnabling, value);

        }

        private async void TryEnableSSE()
        {
            IsEnabling = false;
            await Task.Delay(10000);
            Debug.WriteLine("VM: Change " + (!IsEnabled).ToString());
            _isEnabled = !_isEnabled;
            IsEnabling = true;

            OnPropertyChanged("IsEnabled");
            //SetProperty(ref _isEnabling, value);
        }

    }
}

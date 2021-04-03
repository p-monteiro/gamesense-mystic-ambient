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

        #region Enable Lights

        private bool _isEnabled = false;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                TryEnableSSE();
            }
        }

        private bool _isEnabling = false;
        public bool IsEnabling
        {
            get => _isEnabling;
            private set => SetProperty(ref _isEnabling, value);

        }

        private bool _isEnableStatusPanelOpen = false;
        public bool IsEnableStatusPanelOpen
        {
            get => _isEnableStatusPanelOpen;
            private set => SetProperty(ref _isEnableStatusPanelOpen, value);
        }

        private bool _errorEnabling = false;
        public bool ErrorEnabling
        {
            get => _errorEnabling;
            private set => SetProperty(ref _errorEnabling, value);
        }

        private string _enablingStatusLabel = string.Empty;
        public string EnablingStatusLabel
        {
            get => _enablingStatusLabel;
            private set => SetProperty(ref _enablingStatusLabel, value);
        }

        private int _enablingProgress = 0;
        public int EnablingProgress
        {
            get => _enablingProgress;
            private set => SetProperty(ref _enablingProgress, value);
        }

        private async void TryEnableSSE()
        {
            ErrorEnabling = false;
            IsEnableStatusPanelOpen = true;
            IsEnabling = true;
            EnablingStatusLabel = string.Empty;
            EnablingProgress = 0;

            //Register application in SSE
            EnablingProgress = 50;
            EnablingStatusLabel = "Registering";
            await Task.Delay(1000);
            if (ErrorEnabling = !await SseClient.RegisterGame())
            {
                EnablingStatusLabel = "Error Registering";
                IsEnabling = false;
                return;
            }

            //Register application's GoLisp Handlers
            EnablingProgress = 70;
            EnablingStatusLabel = "GoLisp";
            await Task.Delay(2000);
            if (ErrorEnabling = await SseClient.RegisterGoLispHandlers(BuildLispEventCode()))
            {
                EnablingStatusLabel = "Error GoLisp";
                IsEnabling = false;
                return;
            }

            _isEnabled = !_isEnabled;
            IsEnabling = false;

            OnPropertyChanged("IsEnabled");
            IsEnableStatusPanelOpen = false;
            //SetProperty(ref _isEnabling, value);
        }

        #endregion



        private string BuildLispEventCode()
        {
            return

                "(add-custom-zone '(\"mystic_right\" 9 10 11))" + "\n" +
                "(add-custom-zone '(\"mystic_center_r\" 0 1 2 3 4 5 6 7 8))" + "\n" +
                "(add-custom-zone '(\"mystic_center_l\" 12 13 14 15 16 17 18 19 20))" + "\n" +
                "(add-custom-zone '(\"mystic_left\" 21 22 23))" + "\n" +
                "" + "\n" +
                "(handler \"COLORS\"" + "\n" +
                "    (lambda (data)" + "\n" +
                "        (let* (" + "\n" +
                "                (right_r (right-r: (frame: data)))" + "\n" +
                "                (right_g (right-g: (frame: data)))" + "\n" +
                "                (right_b (right-b: (frame: data)))" + "\n" +
                "                (center_r_r (center-r-r: (frame: data)))" + "\n" +
                "                (center_r_g (center-r-g: (frame: data)))" + "\n" +
                "                (center_r_b (center-r-b: (frame: data)))" + "\n" +
                "                (center_l_r (center-l-r: (frame: data)))" + "\n" +
                "                (center_l_g (center-l-g: (frame: data)))" + "\n" +
                "                (center_l_b (center-l-b: (frame: data)))" + "\n" +
                "                (left_r (left-r: (frame: data)))" + "\n" +
                "                (left_g (left-g: (frame: data)))" + "\n" +
                "                (left_b (left-b: (frame: data)))" + "\n" +
                "" + "\n" +
                "" + "\n" +
                "                (right_c (list right_r right_g right_b))" + "\n" +
                "                (center_r_c (list center_r_r center_r_g center_r_b))" + "\n" +
                "                (center_l_c (list center_l_r center_l_g center_l_b))" + "\n" +
                "                (left_c (list left_r left_g left_b))" + "\n" +
                "            )" + "\n" +
                "" + "\n" +
                "            (on-device 'rgb-24-zone show-on-zone: right_c mystic_right:)" + "\n" +
                "            (on-device 'rgb-24-zone show-on-zone: center_r_c mystic_center_r:)" + "\n" +
                "            (on-device 'rgb-24-zone show-on-zone: center_l_c mystic_center_l:)" + "\n" +
                "            (on-device 'rgb-24-zone show-on-zone: left_c mystic_left:)" + "\n" +
                "        )" + "\n" +
                "    )" + "\n" +
                ")" + "\n" +
                "" + "\n" +
                "(add-event-zone-use-with-specifier \"COLORS\" \"all\" \"rgb-24-zone\")";

        }


    }
}

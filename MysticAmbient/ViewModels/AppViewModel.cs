using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using MysticAmbient.Models;
using MysticAmbient.Resources;
using MysticAmbient.Utils;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
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
        public static readonly int N_LEDS = 24;
        public static readonly int WARL_PORT = 21324;

        public GameSenseClient SseClient { get; private set; }

        public LedLight[] Leds { get; private set; }
        public LedZone[] Zones { get; private set; }

        UdpClient udpWarlClient;
        IPEndPoint warlEndPoint;
        Thread warlReceiverThread;
        private bool runWarlUpdates;

        public AppViewModel()
        {
            SseClient = new GameSenseClient(APP_ID, APP_NAME, APP_DEV);

            Leds = new LedLight[N_LEDS];
            for (int i = 0; i < N_LEDS; i++)
                Leds[i] = new LedLight(i);

            Zones = new LedZone[4] {
                new LedZone(new LedLight[3]{ Leds[9], Leds[10], Leds[11]}),
                new LedZone(new LedLight[9]{ Leds[0], Leds[1], Leds[2], Leds[3], Leds[4], Leds[5], Leds[6], Leds[7], Leds[8]}),
                new LedZone(new LedLight[9]{ Leds[12], Leds[13], Leds[14], Leds[15], Leds[16], Leds[17], Leds[18], Leds[19], Leds[20]}),
                new LedZone(new LedLight[3]{ Leds[21], Leds[22], Leds[23]})
            };

            OpenMainWindowCommand = new RelayCommand(OpenMainWindow, OpenMainWindowCanExecute);
            CloseMainWindowCommand = new RelayCommand(CloseWindow);
            ExitApplicationCommand = new AsyncRelayCommand(ExitApplication);

            TryConnectSseCommand = new AsyncRelayCommand(TryConnectSse, TryConnectSseCanExecute);

            HideEnableStatusPanelCommand = new RelayCommand(HideEnableStatusPanel);

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

        public AsyncRelayCommand ExitApplicationCommand { get; }
        private async Task ExitApplication() {  await DisableSSE(); Application.Current.Shutdown();}

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
                if (!_isEnabled)
                    TryEnableSSE();
                else
                    DisableSSE();
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


        public RelayCommand HideEnableStatusPanelCommand { get; }
        public void HideEnableStatusPanel()
        {
            IsEnableStatusPanelOpen = false;
        }

        private async void TryEnableSSE()
        {
            ErrorEnabling = false;
            IsEnabling = true;
            EnablingStatusLabel = string.Empty;
            EnablingProgress = 0;
            IsEnableStatusPanelOpen = true;

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
            await Task.Delay(1000);
            if (ErrorEnabling = !await SseClient.RegisterGoLispHandlers(BuildLispEventCode()))
            {
                EnablingStatusLabel = "Error GoLisp";
                IsEnabling = false;
                return;
            }

            //Start WARL Server and Thread Loop
            EnablingProgress = 80;
            EnablingStatusLabel = "WARL";
            await Task.Delay(2000);
            try
            {
                udpWarlClient = new(WARL_PORT);
                udpWarlClient.Client.ReceiveTimeout = 1000;
                warlEndPoint = new(IPAddress.Any, WARL_PORT);

                warlReceiverThread = new(() =>
                {
                    runWarlUpdates = true;

                    while (runWarlUpdates)
                    {
                        try
                        {
                            byte[] data = udpWarlClient.Receive(ref warlEndPoint);

                            // WARL Data
                            // data[0] -> type
                            // data[1] -> waitSeconds
                            // -- LED Block 1
                            // data[2] -> ledNumber
                            // data[3] -> Red
                            // data[4] -> Green
                            // data[5] -> Blue
                            // ...


                            int type = (data.Length >= 1) ? Convert.ToInt32(data[0]) : -1;
                            int waitSeconds = (data.Length >= 2) ? Convert.ToInt32(data[1]) : -1;

                            for (int i = 2; i < data.Length; i += 4)
                            {
                                int zone = Convert.ToInt32(data[i]); // WARL LED

                                if (zone > -1 && zone < N_LEDS)
                                {
                                    if (zone < Zones.Length)
                                    {
                                        lock (Zones[zone])
                                        {
                                            App.Current.Dispatcher.Invoke(() => Zones[zone].SetZoneColor(data[i + 1], data[i + 2], data[i + 3]));
                                        }
                                    }
                                }
                            }
                        }
                        catch (SocketException ex)
                        {
                            Debug.WriteLine($"{DateTime.Now.ToLongTimeString()} - No updates from WARL received");
                        }
                    }

                    runWarlUpdates = true;
                });

                warlReceiverThread.Start();

                while (!runWarlUpdates)
                    await Task.Delay(1);
            }
            catch (SocketException ex)
            {
                Debug.WriteLine(ex.Message);
                EnablingStatusLabel = "Error WARL Port in Use";
                IsEnabling = false;
                ErrorEnabling = true;
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                EnablingStatusLabel = "Error WARL";
                IsEnabling = false;
                ErrorEnabling = true;
                return;
            }

            _isEnabled = true;
            IsEnabling = false;

            OnPropertyChanged("IsEnabled");
            IsEnableStatusPanelOpen = false;

        }


        private async Task DisableSSE()
        {
            IsEnabling = true;

            runWarlUpdates = false;

            while (!runWarlUpdates)
                await Task.Delay(1);

            udpWarlClient.Dispose();
            warlEndPoint = null;

            foreach (LedZone zone in Zones)
                zone.SetZoneColor(0, 0, 0);

            IsEnabling = false;
            _isEnabled = false;

            OnPropertyChanged("IsEnabled");
        }

        #endregion



        private string BuildLispEventCode()
        {
            string lispCode = string.Empty;

            // (add-custom-zone '("ma_zone_i" 0 1 2 ...))
            for (int i = 0; i < Zones.Length; i++)
            {
                string zone_lisp = $"(add-custom-zone '(\"ma_zone_{i}\"";
                foreach (LedLight led in Zones[i].Leds)
                {
                    zone_lisp += $" {led.Number}";
                }
                zone_lisp += "))" + "\n";
                lispCode += zone_lisp;
            }

            lispCode +=
                "(handler \"UPDATELEDS\"" + "\n" +
                "   (lambda (data)" + "\n" +
                "       (let* ((device (value: data))" + "\n" +
                "           (zoneData (frame: data))" + "\n" +
                "           (zones (frame-keys zoneData)))" + "\n" +
                "       (do ((zoneDo zones (cdr zoneDo)))" + "\n" +
                "           ((nil? zoneDo))" + "\n" +
                "           (let* ((zone (car zoneDo))" + "\n" +
                "           (color (get-slot zoneData zone)))" + "\n" +
                "           (on-device device show-on-zone: color zone))))))" + "\n" +
                "(add-event-zone-use-with-specifier \"UPDATELEDS\" \"all\" \"rgb-24-zone\")";

            return lispCode;

        }


    }
}

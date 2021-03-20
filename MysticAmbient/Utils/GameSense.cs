using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using System.Diagnostics;
using System.Net;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace MysticAmbient.Utils
{
    public class GameSenseClient : ObservableObject
    {
        private static string COREPROPS_PATH = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"/SteelSeries/SteelSeries Engine 3/coreProps.json";

        public static string SseAddress { get; private set; } = string.Empty;

        public string SseGameId { get; private set; } = string.Empty;
        public string SseGameDisplayName { get; private set; } = string.Empty;
        public string SseGameDeveloperName { get; private set; } = string.Empty;

        private bool _isReady = false;
        public bool IsReady { get => _isReady; private set => SetProperty(ref _isReady, value); }

        private RestClient sseApiClient = null;

        public GameSenseClient(string gameId, string gameDisplayName, string gameDeveloperName)
        {
            SseGameId = gameId;
            SseGameDisplayName = gameDisplayName;
            SseGameDeveloperName = gameDeveloperName;
        }

        public async Task<bool> InitGameSenseAsync()
        {
            //return (IsReady = false);
            try
            {
                // Read configuration from %PROGRAMDATA%/SteelSeries/SteelSeries Engine 3/coreProps.json
                string corePropsContent = await File.ReadAllTextAsync(COREPROPS_PATH);
                SseCoreProps coreProps = JsonConvert.DeserializeObject<SseCoreProps>(corePropsContent);
                SseAddress = coreProps.address;

                sseApiClient = new();
                sseApiClient.BaseUrl = new Uri($"http://{SseAddress}");

                return (IsReady = true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return (IsReady = false);
            }

        }

        public bool RegisterGame(int msDeinitializeTimer = 5000)
        {
            if (!IsReady)
            {
                return false;
            }

            try
            {
                RestRequest sseApiRequest = new(Method.POST) { Resource = "game_metadata" };

                string payload = JsonConvert.SerializeObject(new SseGameDetails()
                {
                    game = SseGameId,
                    developer = SseGameDeveloperName,
                    deinitialize_timer_length_ms = msDeinitializeTimer,
                    game_display_name = SseGameDisplayName
                });

                sseApiRequest.AddParameter("application/json", payload, ParameterType.RequestBody);

                IRestResponse sseApiResponse = sseApiClient.Execute(sseApiRequest);

                if (sseApiResponse.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

    }

    public class SseCoreProps
    {
        public string address { get; set; }
        public string encrypted_address { get; set; }
    }

    public class SseGameDetails
    {
        public string game { get; set; }
        public string game_display_name { get; set; }
        public string developer { get; set; }
        public int deinitialize_timer_length_ms { get; set; }
    }
}

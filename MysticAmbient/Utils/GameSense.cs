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
            try
            {
                // Read configuration from %PROGRAMDATA%/SteelSeries/SteelSeries Engine 3/coreProps.json
                string corePropsContent = await File.ReadAllTextAsync(COREPROPS_PATH);
                SseCoreProps coreProps = JsonConvert.DeserializeObject<SseCoreProps>(corePropsContent);
                SseAddress = coreProps.address;

                Debug.WriteLine("GameSenseClient :: API found at " + SseAddress + ". Trying to connect");

                sseApiClient = new(new Uri($"http://{SseAddress}"));

                if (IsReady = await PingSseServer())
                    Debug.WriteLine("GameSenseClient :: Connected to SSE server.");
                else
                    Debug.WriteLine("GameSenseClient :: SSE Server not responding.");

                return IsReady;
            }
            catch (System.IO.FileNotFoundException ex)
            {
                Debug.WriteLine("GameSenseClient :: API Configuration file not found.\n" + ex.Message);
                return (IsReady = false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return (IsReady = false);
            }

        }


        private async Task<bool> PingSseServer()
        {
            if (sseApiClient == null)
                return false;

            try
            {
                RestRequest sseApiRequest = new(Method.GET);
                IRestResponse sseApiResponse = await sseApiClient.ExecuteAsync(sseApiRequest);

                //If the SSE server responds with 404, it is there!
                if (sseApiResponse.StatusCode == HttpStatusCode.NotFound)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                return false;
            }

        }


        public async Task<bool> RegisterGame(int msDeinitializeTimer = 5000)
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

                IRestResponse sseApiResponse = await sseApiClient.ExecuteAsync(sseApiRequest);

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

        public async Task<bool> RegisterGoLispHandlers(string goLispHandlers)
        {
            if (!IsReady)
            {
                return false;
            }

            try
            {
                RestRequest sseApiRequest = new(Method.POST) { Resource = "load_golisp_handlers" };

                string payload = JsonConvert.SerializeObject(new SseGoLispHandlers()
                {
                    game = SseGameId,
                    golisp = goLispHandlers
                });

                sseApiRequest.AddParameter("application/json", payload, ParameterType.RequestBody);

                IRestResponse sseApiResponse = await sseApiClient.ExecuteAsync(sseApiRequest);

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

        public async Task<bool> SendGameEvent(string eventName, object data)
        {
            if (!IsReady)
            {
                return false;
            }

            try
            {
                SseGameEvent gameEvent = new SseGameEvent()
                {
                    game = SseGameId,
                    @event = eventName,
                    data = data
                };

                RestRequest sseApiRequest = new(Method.POST) { Resource = "game_event" };
                sseApiRequest.AddParameter("application/json", JsonConvert.SerializeObject(gameEvent), ParameterType.RequestBody);

                IRestResponse sseApiResponse = await sseApiClient.ExecuteAsync(sseApiRequest);

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

    public class SseGoLispHandlers
    {
        public string game { get; set; }
        public string golisp { get; set; }
    }

    public class SseGameEvent
    {
        public string game { get; set; }
        public string @event { get; set; }
        public object data { get; set; }
    }
}

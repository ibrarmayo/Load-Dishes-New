using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace SupersonicWisdomSDK
{
    internal class SwCoreTracker : ISwGameProgressionListener
    {
        #region --- Constants ---

        public const string CUSTOM1 = "custom1";

        private const string CUSTOM6 = "custom6";
        private const string CLIENT_CATEGORY_KEY = "clientCategory";
        private const string PROGRESS_EVENT_TYPE = "Progress";
        private const string GAMEPLAY_TYPE_KEY = "gameplayType";
        private const string GAMEPLAY_PROGRESS_TYPE_KEY = "gameplayProgressType";
        private const string LEVEL_TYPE_KEY = "levelType";
        private const string LEVEL_NUMBER_KEY = "levelNumber";
        private const string LEVEL_ATTEMPTS_KEY = "levelAttempts";
        private const string LEVEL_REVIVES_KEY = "levelRevives";
        private const string PLAYTIME_KEY = "playtime";
        private const string LEVEL_CUSTOM_STRING_KEY = CUSTOM6;
        private const string PREVIOUS_LEVEL_TYPE_KEY = "previousLevelType";
        private const string PREVIOUS_LEVEL_TYPE_NUMBER_KEY = "previousLevelTypeNumber";

        #endregion


        #region --- Members ---

        private static readonly Dictionary<NetworkReachability, string> ConnectionDictionary = new Dictionary<NetworkReachability, string>
        {
            [NetworkReachability.NotReachable] = "offline",
            [NetworkReachability.ReachableViaLocalAreaNetwork] = "wifi",
            [NetworkReachability.ReachableViaCarrierDataNetwork] = "carrier"
        };

        private readonly Dictionary<string, object> _gameProgressDictionary = new Dictionary<string, object>
        {
            { GAMEPLAY_TYPE_KEY, ESwGameplayType.Level },
            { GAMEPLAY_PROGRESS_TYPE_KEY, SwProgressEvent.LevelStarted },
            { LEVEL_CUSTOM_STRING_KEY, "" },
            { PREVIOUS_LEVEL_TYPE_KEY, ESwLevelType.Regular },
            { PREVIOUS_LEVEL_TYPE_NUMBER_KEY, 0 },
            { PLAYTIME_KEY, 0f }
        };

        private readonly ISwWebRequestClient _webRequestClient;
        private readonly SwCoreNativeAdapter _wisdomCoreNativeAdapter;
        private readonly SwCoreUserData _coreUserData;
        private readonly SwTimerManager _timerManager;

        #endregion


        #region --- Properties ---

        public EConfigListenerType ListenerType
        {
            get { return EConfigListenerType.EndOfGame; }
        }

        private float PlaytimeElapsed
        {
            get { return _timerManager?.CurrentSessionPlaytimeStopWatch?.Elapsed ?? -1f; }
        }

        #endregion


        #region --- Construction ---

        public SwCoreTracker(SwCoreNativeAdapter wisdomCoreNativeAdapter, SwCoreUserData coreUserData, ISwWebRequestClient webRequestClient, SwTimerManager timerManager)
        {
            _wisdomCoreNativeAdapter = wisdomCoreNativeAdapter;
            _coreUserData = coreUserData;
            _webRequestClient = webRequestClient;
            _timerManager = timerManager;
        }

        #endregion


        #region --- Public Methods ---

        public static SwJsonDictionary GenerateEventCustoms(params string[] customs)
        {
            var customParams = new SwJsonDictionary();

            for (var i = 0; i < customs.Length; i++)
            {
                customParams.Add("custom" + (i + 1), customs[i] ?? "");
            }

            return customParams;
        }

        public void OnTimeBasedGameStarted()
        {
            TrackGameProgressEvent(SwProgressEvent.TimeBasedGameStart);
        }

        public void OnLevelCompleted(ESwLevelType levelType, long level, string customString, long attempts, long revives)
        {
            TrackGameProgressEvent(SwProgressEvent.LevelCompleted, levelType, level, customString, attempts, PlaytimeElapsed, revives);
        }

        public void OnLevelFailed(ESwLevelType levelType, long level, string customString, long attempts, long revives)
        {
            TrackGameProgressEvent(SwProgressEvent.LevelFailed, levelType, level, customString, attempts, PlaytimeElapsed, revives);
        }

        public void OnLevelRevived(ESwLevelType levelType, long level, string customString, long attempts, long revives)
        {
            TrackGameProgressEvent(SwProgressEvent.LevelRevived, levelType, level, customString, attempts, PlaytimeElapsed, revives);
        }

        public void OnLevelSkipped(ESwLevelType levelType, long level, string customString, long attempts, long revives)
        {
            TrackGameProgressEvent(SwProgressEvent.LevelSkipped, levelType, level, customString, attempts, PlaytimeElapsed, revives);
        }

        public void OnLevelStarted(ESwLevelType levelType, long level, string customString, long attempts, long revives)
        {
            TrackGameProgressEvent(SwProgressEvent.LevelStarted, levelType, level, customString, attempts, PlaytimeElapsed, revives);
        }

        public void OnMetaStarted(string customString)
        {
            var userState = _coreUserData.ImmutableUserState();

            _gameProgressDictionary.Clear();
            _gameProgressDictionary[GAMEPLAY_TYPE_KEY] = ESwGameplayType.Meta;
            _gameProgressDictionary[GAMEPLAY_PROGRESS_TYPE_KEY] = SwProgressEvent.MetaStarted;
            _gameProgressDictionary[LEVEL_CUSTOM_STRING_KEY] = customString;
            _gameProgressDictionary[PREVIOUS_LEVEL_TYPE_KEY] = userState.previousLevelType;
            _gameProgressDictionary[PREVIOUS_LEVEL_TYPE_NUMBER_KEY] = userState.previousLevelTypeNumber;
            _gameProgressDictionary[PLAYTIME_KEY] = (int)Mathf.Round(PlaytimeElapsed);

            TrackEventWithParams(PROGRESS_EVENT_TYPE, _gameProgressDictionary);
        }

        public void OnMetaEnded(string customString)
        {
            var userState = _coreUserData.ImmutableUserState();

            _gameProgressDictionary.Clear();
            _gameProgressDictionary[GAMEPLAY_TYPE_KEY] = ESwGameplayType.Meta;
            _gameProgressDictionary[GAMEPLAY_PROGRESS_TYPE_KEY] = SwProgressEvent.MetaEnded;
            _gameProgressDictionary[LEVEL_CUSTOM_STRING_KEY] = customString;
            _gameProgressDictionary[PREVIOUS_LEVEL_TYPE_KEY] = userState.previousLevelType;
            _gameProgressDictionary[PREVIOUS_LEVEL_TYPE_NUMBER_KEY] = userState.previousLevelTypeNumber;
            _gameProgressDictionary[PLAYTIME_KEY] = (int)Mathf.Round(PlaytimeElapsed);

            TrackEventWithParams(PROGRESS_EVENT_TYPE, _gameProgressDictionary);
        }

        public IEnumerator SendEvent(string url, object data)
        {
            SwInfra.Logger.Log(EWisdomLogType.Analytics, "endpoint | " + url);

            if (SwTestUtils.IsRunningTests)
            {
                yield break;
            }

            var response = new SwWebResponse();

            yield return _webRequestClient.Post(url, data, response, SwConstants.DefaultRequestTimeout);
            SwInfra.Logger.Log(EWisdomLogType.Analytics, "sent");

            if (response.DidFail)
            {
                SwInfra.Logger.LogError(EWisdomLogType.Analytics, "Fail | " + $"code: {response.code} | error: {response.error} | " + $"Internet Reachability: {Application.internetReachability}");
            }
            else
            {
                SwInfra.Logger.Log(EWisdomLogType.Analytics, "Success");
            }
        }

        public void SendUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            SwInfra.CoroutineService.StartCoroutine(SendUrlCoroutine(url));
        }

        public void TrackEvent(string evt, params string[] customs)
        {
            SwInfra.Logger.Log(EWisdomLogType.Analytics, $"Event name: {evt}");
            TrackEventInternal(evt, customs);
        }

        public void TrackGameProgressEvent(SwProgressEvent progress)
        {
            TrackEventInternal(PROGRESS_EVENT_TYPE, $"{progress}");
        }

        public void TrackGameProgressEvent(SwProgressEvent progress, string customString)
        {
            TrackEventInternal(PROGRESS_EVENT_TYPE, $"{progress}", customString);
        }

        public void TrackGameProgressEvent(SwProgressEvent progress, ESwLevelType levelType, long levelNumber, string customString)
        {
            TrackEventInternal(PROGRESS_EVENT_TYPE, $"{progress}", $"{levelNumber}", $"{customString}");
        }

        public void TrackGameProgressEvent(SwProgressEvent progress, long levelNum, long attempts, float playtime = 0f, long revives = 0)
        {
            TrackEventInternal(PROGRESS_EVENT_TYPE, $"{progress}", $"{levelNum}", $"{attempts}", $"{(int)Mathf.Round(playtime)}", $"{revives}");
        }

        public void TrackGameProgressEvent(SwProgressEvent progress, ESwLevelType levelType, long levelNum, string customString, long attempts, float playtime = 0f, long revives = 0)
        {
            var userState = _coreUserData.ImmutableUserState();

            _gameProgressDictionary.Clear();
            _gameProgressDictionary[GAMEPLAY_TYPE_KEY] = ESwGameplayType.Level;
            _gameProgressDictionary[GAMEPLAY_PROGRESS_TYPE_KEY] = progress;
            _gameProgressDictionary[LEVEL_CUSTOM_STRING_KEY] = customString;
            _gameProgressDictionary[PREVIOUS_LEVEL_TYPE_KEY] = userState.previousLevelType;
            _gameProgressDictionary[PREVIOUS_LEVEL_TYPE_NUMBER_KEY] = userState.previousLevelTypeNumber;
            _gameProgressDictionary[PLAYTIME_KEY] = (int)Mathf.Round(playtime);
            _gameProgressDictionary.Add(LEVEL_TYPE_KEY, levelType);
            _gameProgressDictionary.Add(LEVEL_NUMBER_KEY, levelNum);
            _gameProgressDictionary.Add(LEVEL_ATTEMPTS_KEY, attempts);
            _gameProgressDictionary.Add(LEVEL_REVIVES_KEY, revives);

            //The progression events are duplicated to keep backwards compatibility with the old events (new event structure was inserted in v7.4)
            var eventCustoms = GenerateEventCustoms($"{progress}", $"{levelNum}", $"{attempts}", $"{(int)Mathf.Round(playtime)}", $"{revives}", $"{customString}", $"{userState.consecutiveRvWatched}");
            _gameProgressDictionary.SwMerge(true, eventCustoms);
            
            TrackEventWithParams(PROGRESS_EVENT_TYPE, _gameProgressDictionary);
            
            //Save the previous level type and level number for the next level
            _coreUserData.ModifyUserStateSync(state =>
            {
                state.previousLevelType = levelType;
                state.previousLevelTypeNumber = levelNum;
            });
        }

        public void TrackInfraEvent(params string[] customs)
        {
            TrackInfraEvent(GenerateEventCustoms(customs));
        }

        public void TrackInfraEvent(Dictionary<string, object> customs)
        {
            TrackEventWithParams(ClientCategory.Infra.ToString(), customs);
        }

        public void TrackEventWithParams(string eventName, Dictionary<string, object> customs = null, ClientCategory? clientCategory = null)
        {
            customs ??= new SwJsonDictionary();
            if (clientCategory.HasValue)
            {
                customs.SwAddOrReplace(CLIENT_CATEGORY_KEY, clientCategory.Value.ToString());
            }
            var customsJson = customs.SwToJsonString();
            var extraJson = JsonConvert.SerializeObject(GetEventDetailsExtra());

            _wisdomCoreNativeAdapter.TrackEvent(eventName, customsJson, extraJson);
        }
        
        public void TrackEventWithParams(string eventName, Dictionary<string, object> customsDictionary, ClientCategory? clientCategory = null, params string[] customs)
        {
            var eventCustoms = GenerateEventCustoms(customs);
            if (clientCategory.HasValue)
            {
                eventCustoms.SwAddOrReplace(CLIENT_CATEGORY_KEY, clientCategory.Value.ToString());
            }
            customsDictionary.SwMerge(false, eventCustoms);
            var customsJson = customsDictionary.SwToJsonString();
            var extraJson = JsonUtility.ToJson(GetEventDetailsExtra());

            _wisdomCoreNativeAdapter.TrackEvent(eventName, customsJson, extraJson);
        }

        #endregion


        #region --- Private Methods ---

        private static IEnumerator SendUrlCoroutine(string url)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                SwInfra.Logger.Log(EWisdomLogType.Analytics, "SendEvent | error | network not reachable");

                yield break;
            }

            using (var webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.SendWebRequest();
                SwInfra.Logger.Log(EWisdomLogType.Analytics, $"Url: {url}");
                var code = webRequest.responseCode;

                if (code == 0 || code >= 400)
                {
                    SwInfra.Logger.LogError(EWisdomLogType.Analytics, $"Fail: {code}");
                }
                else
                {
                    SwInfra.Logger.Log(EWisdomLogType.Analytics, "Success");
                }
            }
        }

        protected internal void TrackEventInternal(string eventName, params string[] customs)
        {
            var eventCustoms = GenerateEventCustoms(customs);
            TrackEventWithParams(eventName, eventCustoms);
        }

        protected SwEventDetailsExtra GetEventDetailsExtra()
        {
            var eventDetailsExtra = new SwEventDetailsExtra
            {
                lang = _coreUserData.Language,
                country = _coreUserData.Country
            };

            // The following properties are relying Unity API.
            // Unity API can be accessed only via main thread
            if (SwUtils.IsRunningOnMainThread)
            {
                eventDetailsExtra.connection = ConnectionDictionary[Application.internetReachability];
                eventDetailsExtra.dpi = $"{Screen.dpi}";
                eventDetailsExtra.resolutionWidth = $"{Screen.currentResolution.width}";
                eventDetailsExtra.resolutionHeight = $"{Screen.currentResolution.height}";
            }

            return eventDetailsExtra;
        }

        #endregion


        #region --- Enums ---

        internal enum ClientCategory
        {
            Infra,
            Notification
        }

        #endregion
    }
}
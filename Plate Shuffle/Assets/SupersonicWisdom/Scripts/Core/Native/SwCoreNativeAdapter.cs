using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace SupersonicWisdomSDK
{
    internal class SwCoreNativeAdapter : ISwReadyEventListener, ISwCoreConfigListener
    {
        #region --- Constants ---

        private const string EventsRemoteConfigStorageKey = "SupersonicWisdomEventsConfig";

        #endregion


        #region --- Members ---

        protected readonly SwCoreUserData CoreUserData;
        private bool _didFirstSessionStart;

        private string _abId = "";
        private string _abName = "";
        private string _abVariant = "";
        private EConfigStatus _configStatus = EConfigStatus.NotInitialized;
        
        private readonly ISwSessionListener[] _sessionListeners;
        private readonly ISwSettings _settings;
        private readonly ISwNativeApi _wisdomNativeApi;
        private readonly SwNativeRequestManager _nativeRequestManager;

        #endregion


        #region --- Properties ---

        public Tuple<EConfigListenerType, EConfigListenerType> ListenerType
        {
            get { return new Tuple<EConfigListenerType, EConfigListenerType>(EConfigListenerType.FinishWaitingForRemote, EConfigListenerType.EndOfGame); }
        }

        #endregion


        #region --- Construction ---

        public SwCoreNativeAdapter(ISwNativeApi wisdomNativeApi, ISwSettings settings, SwCoreUserData coreUserData, ISwSessionListener[] listeners)
        {
            _wisdomNativeApi = wisdomNativeApi;
            _settings = settings;
            CoreUserData = coreUserData;
            _sessionListeners = listeners ?? new ISwSessionListener[] { };
            _nativeRequestManager = new SwNativeRequestManager(wisdomNativeApi);
        }

        #endregion


        #region --- Public Methods ---

        public virtual IEnumerator InitNativeSession()
        {
            _wisdomNativeApi.InitializeSession(GetEventMetadata());

            if (_wisdomNativeApi.IsSupported() && GetEventsConfig().enabled)
            {
                while (!_didFirstSessionStart)
                {
                    yield return null;
                }
            }
        }

        public virtual IEnumerator InitSDK()
        {
            var eventsConfig = GetEventsConfig();

            if (!eventsConfig.enabled)
            {
                SwInfra.Logger.LogWarning(EWisdomLogType.Native, $"enabled: {eventsConfig.enabled}");

                yield break;
            }

            SwInfra.Logger.Log(EWisdomLogType.Native, $"enabled: {eventsConfig.enabled}");

            yield return _wisdomNativeApi.Init(GetWisdomNativeConfiguration());

            _wisdomNativeApi.AddSessionStartedCallback(OnSessionStarted);
            _wisdomNativeApi.AddSessionEndedCallback(OnSessionEnded);
            _nativeRequestManager.Init();

            UpdateMetadata();
        }

        public virtual string GetSubdomain()
        {
            var version = Application.version.Replace('.', '-');

            return $"{version}-{_settings.GetGameId()}";
        }

        public void StoreNativeConfig(SwNativeEventsConfig config)
        {
            var jsonConfig = JsonUtility.ToJson(config);

            if (string.IsNullOrEmpty(jsonConfig))
            {
                SwInfra.Logger.Log(EWisdomLogType.Native, "Config is null");

                return;
            }

            SwInfra.KeyValueStore.SetString(EventsRemoteConfigStorageKey, jsonConfig);
            SwInfra.KeyValueStore.Save();
        }

        public bool ToggleBlockingLoader(bool shouldPresent)
        {
            return _wisdomNativeApi.ToggleBlockingLoader(shouldPresent);
        }

        public void TrackEvent(string eventName, string customsJson, string extraJson)
        {
            SwInfra.Logger.Log(EWisdomLogType.Native, $"eventName={eventName} | customsJson={customsJson} | extraJson = {extraJson}");
            _wisdomNativeApi.TrackEvent(eventName, customsJson, extraJson);
        }

        public void UpdateAbData(string abId, string abKey, string abGroup, EConfigStatus configStatus)
        {
            SwInfra.Logger.Log(EWisdomLogType.Native, $"abKey = {abKey}");

            _abId = abId;
            _abName = abKey;
            _abVariant = abGroup;
            _configStatus = configStatus; 

            UpdateMetadata();
        }

        public void UpdateConfig()
        {
            if (GetEventsConfig().enabled)
            {
                _wisdomNativeApi.UpdateWisdomConfiguration(GetWisdomNativeConfiguration());
            }
            else
            {
                _wisdomNativeApi.Destroy();
            }
        }

        public void UpdateMetadata()
        {
            _wisdomNativeApi.UpdateMetadata(GetEventMetadata());
        }

        public virtual void RequestRateUsPopup()
        {
            _wisdomNativeApi.RequestRateUsPopup();
        }
        
        public string GetAppInstallSource()
        {
            return _wisdomNativeApi.GetAppInstallSource();
        }
        
        public void OnSwReady ()
        {
            // Waiting for readiness for updating appsFlyerId which is available only after appsFlyer init complete
            UpdateMetadata();
        }

        public void SendRequest(string url, string body, ISwNativeRequestListener listener, string headers, int connectionTimeout, int readTimeout, int cap)
        {
            _nativeRequestManager.SendRequest(url, headers, body, listener, connectionTimeout, readTimeout, cap);
        }

        public void OnConfigResolved(ISwCoreInternalConfig configAccessor, ISwConfigManagerState state)
        {
            _configStatus = state.Status;

            UpdateMetadata();
        }

        #endregion


        #region --- Private Methods ---

        protected virtual SwNativeEventsConfig GetDefaultConfig()
        {
            return new SwNativeEventsConfig();
        }

        protected virtual SwEventMetadataDto GetEventMetadata()
        {
            var attStatus = SwAttUtils.GetStatus();

            var eventMetadata = new SwEventMetadataDto
            {
                bundle = CoreUserData.BundleIdentifier,
                os = CoreUserData.Platform,
                osVer = SystemInfo.operatingSystem,
                uuid = CoreUserData.Uuid,
                swInstallationId = CoreUserData.CustomUuid,
                device = SystemInfo.deviceModel,
                version = Application.version,
                sdkVersion = SwConstants.SdkVersion,
                sdkVersionId = SwConstants.SdkVersionId,
                sdkStage = SwStageUtils.CurrentStage.sdkStage.ToString(),
                installDate = CoreUserData.InstallDate,
                apiKey = _settings.GetAppKey(),
                gameId = _settings.GetGameId(),
                feature = SwConstants.Feature,
                featureVersion = SwConstants.FeatureVersion,
                unityVersion = SwUtils.UnityVersion,
                attStatus = attStatus == SwAttAuthorizationStatus.Unsupported ? "" : $"{attStatus}",
                abId = _abId ?? "",
                abName = _abName ?? "",
                abVariant = _abVariant ?? "",
                configStatus = _configStatus.ToString(),
            };
            
            var organizationAdvertisingId = CoreUserData.OrganizationAdvertisingId;
#if UNITY_IOS
            eventMetadata.sandbox = SwUtils.IsIosSandbox ? "1" : "0";
            eventMetadata.idfv = organizationAdvertisingId;
#endif
#if UNITY_ANDROID
            eventMetadata.appSetId = organizationAdvertisingId;
#endif

            return eventMetadata;
        }

        protected virtual SwNativeEventsConfig GetEventsConfig()
        {
            var jsonConfig = SwInfra.KeyValueStore.GetString(EventsRemoteConfigStorageKey, null);

            return JsonUtility.FromJson<SwNativeEventsConfig>(jsonConfig) ?? GetDefaultConfig();
        }

        protected virtual SwNativeConfig GetWisdomNativeConfiguration()
        {
            return CreateWisdomNativeConfiguration();
        }

        private SwNativeConfig CreateWisdomNativeConfiguration()
        {
            var config = GetEventsConfig();
            var blockingLoaderResourceRelativePath = SwUtils.IsRunningOnIos() ? "SupersonicWisdom/LoaderFrames" : "SupersonicWisdom/LoaderGif/animated_loader.gif";

            return new SwNativeConfig
            {
                Subdomain = GetSubdomain(),
                ConnectTimeout = config.connectTimeout,
                ReadTimeout = config.readTimeout,
                IsLoggingEnabled = _settings.IsDebugEnabled(),
                InitialSyncInterval = config.initialSyncInterval,
                StreamingAssetsFolderPath = Application.streamingAssetsPath,
                BlockingLoaderResourceRelativePath = blockingLoaderResourceRelativePath,
                BlockingLoaderViewportPercentage = 20,
            };
        }

        private void OnSessionEnded(string sessionId)
        {
            _sessionListeners?.ToList().ForEach(e => e.OnSessionEnded(sessionId));
        }

        private void OnSessionStarted(string sessionId)
        {
            _sessionListeners?.ToList().ForEach(e => e.OnSessionStarted(sessionId));

            if (!_didFirstSessionStart)
            {
                _didFirstSessionStart = true;
            }
        }

        #endregion
    }
}
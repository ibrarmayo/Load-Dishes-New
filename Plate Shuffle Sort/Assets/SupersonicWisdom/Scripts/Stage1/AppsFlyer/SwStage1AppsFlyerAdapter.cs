#if SW_STAGE_STAGE1_OR_ABOVE
using System;
using System.Collections.Generic;
using AppsFlyerSDK;

namespace SupersonicWisdomSDK
{
    internal static class SwStage1AppsFlyerConstants
    {
        #region --- Constants ---

        public const string APPSFLYER_INIT_INTERNAL_EVENT_NAME = "AppsFlyerInit";

        #endregion
    }

    internal class SwStage1AppsFlyerAdapter : ISwReadyEventListener, ISwAppsFlyerListener, ISwLocalConfigProvider, ISwAdapter, ISwStage1ConfigListener
    {
        #region --- Constants ---

        /// <summary>
        ///     This key is read by Wisdom Native. Do not rename it.
        /// </summary>
        private const string APPS_FLYER_DEV_KEY = "nYwfftoacbopmuszWBPGnd";
        private const string APPS_FLYER_CONVERSION_DATA_KEY = "AFConversionData";

        #endregion


        #region --- Members ---

        protected readonly SwSettingsManager<SwSettings> SettingsManager;
        protected readonly SwCoreTracker Tracker;
        protected bool DidApplyAppsFlyerAnonymize;
        protected bool DidAppsFlyerRespond;

        protected bool IsSwReady;
        protected bool? ShouldAnonymizeAppsFlyer = null;

        private readonly SwAppsFlyerEventDispatcher _eventDispatcher;
        private readonly SwStage1UserData _userData;

        private bool _didInitAppsFlyer;
        private bool _didTrackConversionDataFail;
        private ISwAppsFlyerListener _listener;
        private string _appsFlyerHostname;
        private string _conversionDataFailError;
        private string _sdkStatus;

        #endregion


        #region --- Properties ---

        public Tuple<EConfigListenerType, EConfigListenerType> ListenerType
        {
            get { return new Tuple<EConfigListenerType, EConfigListenerType>(EConfigListenerType.FinishWaitingForRemote, EConfigListenerType.FinishWaitingForRemote); }
        }

        #endregion


        #region --- Construction ---

        public SwStage1AppsFlyerAdapter(SwAppsFlyerEventDispatcher eventDispatcher, SwStage1UserData userData, SwSettingsManager<SwSettings> settingsManager, SwCoreTracker tracker)
        {
            _eventDispatcher = eventDispatcher;
            SettingsManager = settingsManager;
            _userData = userData;
            Tracker = tracker;
        }

        #endregion


        #region --- Public Methods ---

        public virtual void Init()
        {
            if (_didInitAppsFlyer)
            {
                return;
            }

            _didInitAppsFlyer = true;
            SwInfra.Logger.Log(EWisdomLogType.AppsFlyer, "Init");

            try
            {
                _eventDispatcher.AddListener(this);

                if (!string.IsNullOrEmpty(_appsFlyerHostname))
                {
                    AppsFlyer.setHost("", _appsFlyerHostname);
                }

                AppsFlyer.setIsDebug(SettingsManager.Settings.enableDebug);
                AppsFlyer.setCurrencyCode("USD");
                AppsFlyer.initSDK(APPS_FLYER_DEV_KEY, SettingsManager.Settings.IosAppId, _eventDispatcher);
                SwInternalEvent.Invoke(SwStage1AppsFlyerConstants.APPSFLYER_INIT_INTERNAL_EVENT_NAME);
                AppsFlyer.startSDK();

                _userData.AppsFlyerId = AppsFlyer.getAppsFlyerId();
                _didInitAppsFlyer = true;
                _sdkStatus = _didInitAppsFlyer.ToString();
            }
            catch (Exception e)
            {
                SwInfra.Logger.LogError(EWisdomLogType.AppsFlyer, e.Message);
                _sdkStatus = e.Message;
            }
        }

        public Dictionary<string, string> CreateEventValues()
        {
            var organizationAdvertisingId = _userData.OrganizationAdvertisingId;
            var eventValues = new Dictionary<string, string>();

            if (SwUtils.IsRunningOnAndroid())
            {
                eventValues["appSetId"] = organizationAdvertisingId;
            }
            else if (SwUtils.IsRunningOnIos())
            {
                eventValues["idfv"] = organizationAdvertisingId;
            }

            eventValues["af_sw_stage"] = SwStageUtils.CurrentStage.sdkStage.ToString();

            return eventValues;
        }

        public SwLocalConfig GetLocalConfig()
        {
            return new SwStage1AppsFlyerLocalConfig();
        }

        public SwAdapterData GetAdapterStatusAndVersion()
        {
            var adapterData = new SwAdapterData
            {
                adapterName = nameof(AppsFlyer),
                adapterStatus = _sdkStatus,
                adapterVersion = AppsFlyer.getSdkVersion(),
            };

            return adapterData;
        }

        public void onAppOpenAttribution(string attributionData)
        {
            AppsFlyer.AFLog("onAppOpenAttribution", attributionData);
            SwInfra.Logger.Log(EWisdomLogType.AppsFlyer, $"{attributionData}");
        }

        public void onAppOpenAttributionFailure(string error)
        {
            AppsFlyer.AFLog("onAppOpenAttributionFailure", error);
        }

        public void onConversionDataFail(string error)
        {
            AppsFlyer.AFLog("didReceiveConversionDataWithError", error);
            _conversionDataFailError = error;
            TrackConversionDataFailIfNeeded();
        }

        public void onConversionDataSuccess(string conversionData)
        {
            AppsFlyer.AFLog("didReceiveConversionData", conversionData);

            // This key is being read in wisdom native for it to be sent in all events
            SwInfra.KeyValueStore.SetString(APPS_FLYER_CONVERSION_DATA_KEY, conversionData);
        }

        public void OnSwReady()
        {
            IsSwReady = true;
            TrackConversionDataFailIfNeeded();
        }

        public virtual void OnConfigResolved(ISwStage1InternalConfig config, ISwConfigManagerState state)
        {
            var appsFlyerHostname = config?.GetValue(SwStage1AppsFlyerLocalConfig.APPS_FLYER_DEFAULT_DOMAIN_KEY, SwStage1AppsFlyerLocalConfig.APPS_FLYER_DEFAULT_DOMAIN_DEFAULT_VALUE);
            SetHost(appsFlyerHostname);
        }

        #endregion


        #region --- Private Methods ---

        protected void AnonymizeIfNeeded()
        {
            if (!DidApplyAppsFlyerAnonymize && ShouldAnonymizeAppsFlyer != null)
            {
                try
                {
                    AppsFlyer.anonymizeUser((bool)ShouldAnonymizeAppsFlyer);
                    SwInfra.Logger.Log(EWisdomLogType.AppsFlyer, $"Privacy | {SwPrivacyPolicy.Gdpr} | AppsFlyer | anonymizeUser | {(bool)ShouldAnonymizeAppsFlyer}");
                    DidApplyAppsFlyerAnonymize = true;
                }
                catch (Exception e)
                {
                    SwInfra.Logger.LogError(EWisdomLogType.AppsFlyer, e.Message);
                }
            }
        }

        protected void SendEvent(string eventName, Dictionary<string, string> eventValues)
        {
            AppsFlyer.sendEvent(eventName, CreateEventValues().SwMerge(true, eventValues));
        }

        private void TrackConversionDataFailIfNeeded()
        {
            if (!_didTrackConversionDataFail && IsSwReady && !string.IsNullOrEmpty(_conversionDataFailError))
            {
                _didTrackConversionDataFail = true;
                Tracker.TrackInfraEvent("AFConversionFailure", _conversionDataFailError);
            }
        }
        
        private void SetHost(string appsflyerHostname)
        {
            if (appsflyerHostname.SwIsNullOrEmpty()) return;
            
            _appsFlyerHostname = appsflyerHostname;
            SwInfra.Logger.Log(EWisdomLogType.AppsFlyer, _appsFlyerHostname);
        }

        #endregion


        #region --- Event Handler ---

        public void OnAppsFlyerRequestResponse(object sender, EventArgs args)
        {
            if (args is AppsFlyerRequestEventArgs appsFlyerArgs && !DidAppsFlyerRespond)
            {
                DidAppsFlyerRespond = true;
                SwInfra.Logger.Log(EWisdomLogType.AppsFlyer, $"statusCode = {appsFlyerArgs.statusCode}");
                AnonymizeIfNeeded();
            }
        }

        #endregion
    }
}
#endif
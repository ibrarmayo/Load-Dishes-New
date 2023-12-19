#if SW_STAGE_STAGE1_OR_ABOVE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace SupersonicWisdomSDK
{
    internal class SwStage1Container : SwCoreContainer
    {
        #region --- Members ---

        protected internal readonly SwBlockingApiHandler BlockingApiHandler;
        protected internal readonly SwStage1FacebookAdapter FacebookAdapter;
        protected internal readonly SwStage1GameAnalyticsAdapter GameAnalyticsAdapter;
        protected internal readonly SwStage1Tracker Stage1Tracker;
        protected internal readonly SwFilesCacheManager FilesCacheManager;
        protected readonly List<ISwScriptLifecycleListener> ApplicationFocusListeners;

        #endregion


        #region --- Construction ---

        internal SwStage1Container(Dictionary<string, object> initParamsDictionary, SwStage1MonoBehaviour mono, SwFilesCacheManager filesCacheManager, ISwAsyncCatchableRunnable stageSpecificCustomInitRunnable, SwSettingsManager<SwSettings> settingsManager, ISwReadyEventListener[] readyEventListeners, ISwUserStateListener[] userStateListeners, ISwLocalConfigProvider[] configProviders, ISwAdapter[] coreAdapters, SwStage1NativeAdapter wisdomNativeAdapter, SwStage1DeepLinkHandler deepLinkHandler, SwStage1DevTools devTools, SwCoreUserData coreUserData, SwStage1Tracker tracker, ISwConfigManager configManager, SwBlockingApiHandler blockingApiHandler, SwGameStateSystem gameStateSystem, SwStage1AppsFlyerAdapter appsFlyerAdapter, SwStage1FacebookAdapter facebookAdapter, SwStage1GameAnalyticsAdapter gameAnalyticsAdapter, SwTimerManager timerManager) : base(initParamsDictionary, mono, stageSpecificCustomInitRunnable, settingsManager, readyEventListeners, userStateListeners, configProviders, coreAdapters, wisdomNativeAdapter, deepLinkHandler, devTools, coreUserData, tracker, configManager, timerManager, gameStateSystem)
        {
            FilesCacheManager = filesCacheManager;
            BlockingApiHandler = blockingApiHandler;
            FacebookAdapter = facebookAdapter;
            GameAnalyticsAdapter = gameAnalyticsAdapter;
            FacebookAdapter.OnFacebookInitCompleteEvent += OnFacebookInitComplete;
            Stage1Tracker = tracker;

            ApplicationFocusListeners = new List<ISwScriptLifecycleListener>
            {
                FacebookAdapter
            };
        }

        #endregion


        #region --- Mono Override ---

        public override void OnApplicationPause(bool pauseStatus)
        {
            ApplicationFocusListeners.ForEach(e => e.OnApplicationPause(pauseStatus));
            
            SwInfra.Logger.Log(EWisdomLogType.Container, $"{pauseStatus}");
        }

        public override void OnApplicationQuit()
        {
            SwInfra.Logger.Log(EWisdomLogType.Container);
        }

        #endregion


        #region --- Public Methods ---

        [Preserve]
        public new static ISwContainer GetInstance(Dictionary<string, object> initParamsDictionary)
        {
            var mono = SwContainerUtils.InstantiateSupersonicWisdom<SwStage1MonoBehaviour>("Stage1/SupersonicWisdomStage1");
            SwInfra.Initialize(mono, mono);

            var filesCacheManager = new SwFilesCacheManager();
            var settingsManager = new SwSettingsManager<SwSettings>();
            var wisdomNativeApi = SwNativeApiFactory.GetInstance();
            var userData = new SwStage1UserData(settingsManager.Settings, wisdomNativeApi);
            var sessionListener = new SwStage1SessionListener(userData);
            var timerManager = new SwTimerManager(mono);
            ISwSessionListener[] swSessionListeners = { sessionListener};
            var wisdomNativeAdapter = new SwStage1NativeAdapter(wisdomNativeApi, settingsManager.Settings, userData, swSessionListeners);
            var webRequestClient = new SwUnityWebRequestClient();
            var deepLinkHandler = new SwStage1DeepLinkHandler(settingsManager.Settings, webRequestClient);
            var devTools = new SwStage1DevTools(filesCacheManager);
            var tracker = new SwStage1Tracker(wisdomNativeAdapter, userData, webRequestClient, timerManager);
            var swGameStateSystem = new SwGameStateSystem(tracker, userData);
            var appsFlyerEventDispatcher = mono.GetComponent<SwAppsFlyerEventDispatcher>();
            var appsFlyerAdapter = new SwStage1AppsFlyerAdapter(appsFlyerEventDispatcher, userData, settingsManager, tracker);
            var facebookAdapter = new SwStage1FacebookAdapter();
            var gameAnalyticsAdapter = new SwStage1GameAnalyticsAdapter();
            var configManager = new SwStage1ConfigManager(settingsManager.Settings, userData, tracker, wisdomNativeAdapter, deepLinkHandler);
            var blockingApiHandler = new SwBlockingApiHandler(settingsManager.Settings, swGameStateSystem, null);
            
            var initThirdPartiesStep = new SwStage1InitThirdParties(facebookAdapter, gameAnalyticsAdapter);
            var fetchRemoteConfigStep = new SwStage1FetchRemoteConfig(configManager);
            var initAppsflyerStep = new SwStage1InitAppsflyer(appsFlyerAdapter);

            var stageSpecificCustomInitRunnable = new SwAsyncFlow(new[]
            {
                new SwAsyncFlowStep(fetchRemoteConfigStep, 0),
                new SwAsyncFlowStep(initThirdPartiesStep, 0),
                //We excluded the initialization of AppsFlyer due to a dependency in a remote config value that determines the AF hostname.
                new SwAsyncFlowStep(initAppsflyerStep, 1),
            });

            ISwAdapter[] swAdapters = { appsFlyerAdapter, gameAnalyticsAdapter, facebookAdapter };
            // User data should be after config manager
            ISwReadyEventListener[] readyEventListeners = { configManager, appsFlyerAdapter, wisdomNativeAdapter, timerManager };
            ISwUserStateListener[] userStateListeners = { };
            ISwLocalConfigProvider[] configProviders = { configManager, appsFlyerAdapter };
            
            configManager.AddListeners(new List<ISwStage1ConfigListener> { userData, appsFlyerAdapter, wisdomNativeAdapter });
            swGameStateSystem.AddGameStateListeners(new List<ISwGameStateSystemListener>() { configManager });
            swGameStateSystem.AddGameProgressionListeners(new ISwGameProgressionListener[] { gameAnalyticsAdapter, tracker });

            return new SwStage1Container(initParamsDictionary, mono, filesCacheManager, stageSpecificCustomInitRunnable, settingsManager, readyEventListeners, userStateListeners, configProviders, swAdapters, wisdomNativeAdapter, deepLinkHandler, devTools, userData, tracker, configManager, blockingApiHandler, swGameStateSystem, appsFlyerAdapter, facebookAdapter, gameAnalyticsAdapter, timerManager);
        }

        public override ISwInitParams CreateInitParams()
        {
            return new SwStage1InitParams();
        }

        public override void OnAwake()
        {
            base.OnAwake();
            SwInfra.Logger.Log(EWisdomLogType.Container);
        }

        public override void OnStart()
        {
            SwInfra.Logger.Log(EWisdomLogType.Container);
        }

        public override void PopulateInitParams(Dictionary<string, object> initParamsDictionary)
        {
            base.PopulateInitParams(initParamsDictionary);
        }

        #endregion


        #region --- Private Methods ---

        protected override IEnumerator BeforeReady()
        {
            yield return base.BeforeReady();
            yield return BlockingApiHandler.PrepareForGameStarted();
        }

        internal SwUserState CopyOfUserState()
        {
            return CoreUserData.ImmutableUserState();
        }

        private IEnumerator VerifyFirstLaunchWithApple()
        {
            yield return new WaitForSeconds(1);
            
            SwSKAdNetworkAdapter.UpdatePostbackConversionValue(
                conversionValue: 0,
                callback: authorizationStatusString => Stage1Tracker.TrackConversionValueEvent(0, authorizationStatusString));
        }

        #endregion


        #region --- Event Handler ---

        private void OnFacebookInitComplete()
        {
            if (SwUtils.IsRunningOnIos() && CoreUserData.IsNew)
            {
                SwInfra.CoroutineService.StartCoroutine(VerifyFirstLaunchWithApple());
            }
        }

        #endregion
    }
}
#endif
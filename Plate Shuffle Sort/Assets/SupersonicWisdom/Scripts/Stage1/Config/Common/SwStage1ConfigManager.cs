#if SW_STAGE_STAGE1_OR_ABOVE

using System.Collections.Generic;
using UnityEngine;

namespace SupersonicWisdomSDK
{
    internal class SwStage1ConfigManager : SwCoreConfigManager
    {
        #region --- Members ---

        private readonly SwDeepLinkHandler _deepLinkHandler;

        private readonly List<ISwStage1ConfigListener> _listeners;

        #endregion


        #region --- Construction ---

        public SwStage1ConfigManager(ISwSettings settings, SwCoreUserData coreUserData, SwCoreTracker tracker, SwStage1NativeAdapter swStage1NativeAdapter, SwDeepLinkHandler deepLinkHandler) : base(settings, coreUserData, tracker, swStage1NativeAdapter)
        {
            _deepLinkHandler = deepLinkHandler;
            _listeners = new List<ISwStage1ConfigListener>();
        }

        #endregion


        #region --- Public Methods ---

        public void AddListeners(List<ISwStage1ConfigListener> listeners)
        {
            _listeners.AddRange(listeners);
        }

        #endregion


        #region --- Private Methods ---

        protected override SwRemoteConfigRequestPayload CreatePayload()
        {
            var payload = base.CreatePayload();
            payload.testId = SwInfra.KeyValueStore.GetString(SwStage1DeepLinkConstants.TestIdStorageKey);

            return payload;
        }
        
        protected override SwCoreConfig CreateLocalConfig(Dictionary<string, object> localConfigValues)
        {
            return new SwStage1Config(localConfigValues);;
        }

        protected override SwCoreConfig ParseConfig(string configStr)
        {
            return JsonUtility.FromJson<SwStage1Config>(configStr);
        }
        
        protected override void NotifyInternalListeners()
        {
            base.NotifyInternalListeners();

            var config = Config as SwStage1Config;
            
            if (_listeners != null && _listeners.Count > 0)
            {
                foreach (var listener in _listeners)
                {
                    if (listener.ListenerType.Item1 <= Timing && listener.ListenerType.Item2 >= Timing)
                    {
                        listener.OnConfigResolved(config, this);
                    }
                }
            }
        }

        protected override void OnConfigReady()
        {
            base.OnConfigReady();

            TryLoadDeepLinkConfig();
        }

        private void TryLoadDeepLinkConfig()
        {
            var resolvedDeepLinkConfig = SwConfigUtils.ResolveDeepLinkConfig(_deepLinkHandler.DeepLinkParams);
            SwInfra.Logger.Log(EWisdomLogType.Config, resolvedDeepLinkConfig.SwToJsonString());
            Config.DynamicConfig.SwMerge(true, resolvedDeepLinkConfig);
        }

        #endregion
    }
}

#endif

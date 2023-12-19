#if SW_STAGE_STAGE1_OR_ABOVE
using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace SupersonicWisdomSDK
{
    [Serializable]
    internal class SwStage1Config : SwCoreConfig, ISwStage1InternalConfig
    {
        #region --- Members ---

        public bool unavailable;
        public SwAgentConfig agent;
        public SwNativeEventsConfig events;

        #endregion


        #region --- Properties ---

        public bool Unavailable
        {
            get { return unavailable; }
        }

        public SwNativeEventsConfig Events
        {
            get { return events; }
        }

        public SwAgentConfig Agent
        {
            get { return agent; }
        }
        
        #endregion


        #region --- Construction ---

        public SwStage1Config(Dictionary<string, object> defaultDynamicConfig) : base(defaultDynamicConfig)
        {
            events = new SwNativeEventsConfig();
        }

        #endregion
    }
}
#endif
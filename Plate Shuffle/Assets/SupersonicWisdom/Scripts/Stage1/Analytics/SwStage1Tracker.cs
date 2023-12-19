#if SW_STAGE_STAGE1_OR_ABOVE
using System.Collections.Generic;
using System.Globalization;

namespace SupersonicWisdomSDK
{
    internal class SwStage1Tracker : SwCoreTracker
    {
        #region --- Constants ---
        
        protected const string UPDATE_CONVERSION_VALUE_EVENT_TYPE = "UpdateCV";

        #endregion


        #region --- Construction ---

        public SwStage1Tracker(SwCoreNativeAdapter wisdomCoreNativeAdapter, SwCoreUserData coreUserData, ISwWebRequestClient webRequestClient, SwTimerManager timerManager) : base(wisdomCoreNativeAdapter, coreUserData, webRequestClient, timerManager)
        { }

        #endregion


        #region --- Public Methods ---

        public void TrackConversionValueEvent(int conversionValue, string authorizationStatusString = "", string payload = "", string coarseValue = "", string postback = "")
        { 
            TrackEventInternal(UPDATE_CONVERSION_VALUE_EVENT_TYPE, SwEncryptor.Encrypt($"{conversionValue}"), SwEncryptor.Encrypt(payload), authorizationStatusString, coarseValue, postback);
        }

        #endregion
    }
}

#endif
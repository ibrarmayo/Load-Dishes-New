#if SW_STAGE_STAGE1_OR_ABOVE
using System;
using JetBrains.Annotations;

namespace SupersonicWisdomSDK
{
    internal interface ISwStage1InternalConfig : ISwCoreInternalConfig
    {
        #region --- Properties ---

        bool Unavailable { get; }
        SwAgentConfig Agent { get; }
        SwNativeEventsConfig Events { get; }

        #endregion
    }

    internal interface ISwStage1ConfigListener
    {
        #region --- Properties ---

        // The tuple represent the <start, end> timing which accepted by the listener (inclusive)
        Tuple<EConfigListenerType, EConfigListenerType> ListenerType { get; }

        #endregion


        #region --- Public Methods ---

        /// This method can be invoked more than one time
        void OnConfigResolved([NotNull] ISwStage1InternalConfig swConfigAccessor, ISwConfigManagerState state);

        #endregion
    }
}
#endif
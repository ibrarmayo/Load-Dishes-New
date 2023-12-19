#if SW_STAGE_STAGE1_OR_ABOVE

using AppsFlyerSDK;
using JetBrains.Annotations;

namespace SupersonicWisdomSDK
{
    internal class SwStage1NativeAdapter : SwCoreNativeAdapter, ISwStage1ConfigListener
    {
        #region --- Construction ---

        public SwStage1NativeAdapter(ISwNativeApi wisdomNativeApi, ISwSettings settings, SwCoreUserData coreUserData, [CanBeNull] ISwSessionListener[] listeners) : base(wisdomNativeApi, settings, coreUserData, listeners)
        { }

        #endregion


        #region --- Private Methods ---

        protected override SwEventMetadataDto GetEventMetadata ()
        {
            var eventMetadata = base.GetEventMetadata();
            var stage1UserData = (SwStage1UserData)CoreUserData;
            eventMetadata.appsFlyerId = stage1UserData.AppsFlyerId;

            return eventMetadata;
        }

        #endregion


        public void OnConfigResolved(ISwStage1InternalConfig swConfigAccessor, ISwConfigManagerState state)
        {
            // Must be before base class
            var eventsConfiguration = swConfigAccessor.Events;
            StoreNativeConfig(eventsConfiguration);
            UpdateConfig();
            
            base.OnConfigResolved(swConfigAccessor, state);
        }
    }
}
#endif
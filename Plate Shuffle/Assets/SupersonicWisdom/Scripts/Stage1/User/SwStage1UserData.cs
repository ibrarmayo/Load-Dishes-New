#if SW_STAGE_STAGE1_OR_ABOVE

namespace SupersonicWisdomSDK
{
    internal class SwStage1UserData : SwCoreUserData, ISwStage1ConfigListener
    {
        #region --- Properties ---

        public string AppsFlyerId { get; set; }

        #endregion


        #region --- Construction ---

        public SwStage1UserData(ISwSettings settings, ISwAdvertisingIdsGetter idsGetter) : base(settings, idsGetter) { }

        #endregion


        public void OnConfigResolved(ISwStage1InternalConfig swConfigAccessor, ISwConfigManagerState state)
        {
            if (swConfigAccessor?.Agent != null)
            {
                Country = swConfigAccessor.Agent.country;
            }
        }
    }
}

#endif
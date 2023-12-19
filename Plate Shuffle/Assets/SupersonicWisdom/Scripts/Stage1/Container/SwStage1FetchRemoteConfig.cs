#if SW_STAGE_STAGE1_OR_ABOVE
using System.Collections;

namespace SupersonicWisdomSDK
{
    internal class SwStage1FetchRemoteConfig : ISwAsyncRunnable
    {
        #region --- Members ---

        private readonly SwStage1ConfigManager _configManager;

        #endregion


        #region --- Construction ---

        public SwStage1FetchRemoteConfig(SwStage1ConfigManager configManager)
        {
            _configManager = configManager;
        }

        #endregion


        #region --- Public Methods ---

        public IEnumerator Run ()
        {
            yield return _configManager.Fetch();
        }

        #endregion
    }
}
#endif
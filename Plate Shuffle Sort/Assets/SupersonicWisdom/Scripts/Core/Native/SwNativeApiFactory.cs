#if UNITY_EDITOR
using SupersonicWisdomSDK.Editor;
#endif

namespace SupersonicWisdomSDK
{
    internal static class SwNativeApiFactory
    {
        #region --- Public Methods ---

        public static ISwNativeApi GetInstance()
        {
            if (SwUtils.IsRunningOnAndroid())
            {
                return new SwNativeAndroidApi(new SwNativeAndroidBridge());
            }

            if (SwUtils.IsRunningOnIos())
            {
                return new SwNativeIosApi(new SwNativeIosBridge());
            }

#if UNITY_EDITOR
            if (SwUtils.IsRunningOnEditor())
            {
                return new SwNativeEditorApi(null);
            }
#endif

            return new SwNativeUnsupportedApi(null);
        }

        #endregion
    }
}
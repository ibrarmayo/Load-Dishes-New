#if SW_STAGE_STAGE1_OR_ABOVE

namespace SupersonicWisdomSDK
{
    internal class SwStage1DevTools : SwDevTools
    {
        #region --- Constructor ---

        internal SwStage1DevTools(SwFilesCacheManager filesCacheManager) : base(filesCacheManager)
        { }

        #endregion
        
        
        #region --- Private Methods ---

        protected override void SetupDevToolsPopup ()
        {
            if (!IsDevtoolEnabled) return;

            var devToolsPopup = DevToolsMonoManager.PopupContainer.gameObject.AddComponent<SwStage1DevToolsPopup>();
            DevToolsMonoManager.Setup(FilesCacheManager);
            devToolsPopup.Setup(FilesCacheManager);
        }
        
        #endregion
    }
}
#endif
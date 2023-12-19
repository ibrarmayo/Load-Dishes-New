#if SW_STAGE_STAGE1_OR_ABOVE

using System;
using System.Collections.Generic;

namespace SupersonicWisdomSDK
{
    internal class SwStage1DevToolsPopup : SwDevToolsPopup
    {
        #region --- Members ---

        private readonly Dictionary<string, Action> _buttons = new Dictionary<string, Action>()
        {
        };

        #endregion

        
        #region --- Public Methods ---

        public void Setup (SwFilesCacheManager filesCacheManager)
        {
            FilesCacheManager = filesCacheManager;
            CreateButtons(_buttons);
        }

        #endregion

        
        #region --- Private Methods ---

        protected override void CreateButtons(Dictionary<string, Action> buttons = null)
        {
            ConcatButtons(_buttons);
            base.CreateButtons(buttons);
        }

        #endregion
    }
}
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace SupersonicWisdomSDK
{
    internal class SwDevTools
    {
        #region --- Members ---

        protected readonly SwFilesCacheManager FilesCacheManager;
        protected SwDevToolsMonoManager DevToolsMonoManager;

        #endregion
        

        #region --- Properties ---
        
        protected bool IsDevtoolEnabled { get; set; }

        #endregion
        
        
        #region --- Constants ---
        
        private const string DEVTOOLS_PREFAB_PATH = "Core/DevTools/SwDevToolsCanvas";

        #endregion

        
        #region --- Construction ---

        public SwDevTools(SwFilesCacheManager filesCacheManager)
        {
            FilesCacheManager = filesCacheManager;
        }

        #endregion


        #region --- Public Methods ---
        
        public void EnableDevtools(bool isEnabled)
        {
            IsDevtoolEnabled = isEnabled;

            if (IsDevtoolEnabled)
            {
                DevToolsMonoManager = Create<SwDevToolsMonoManager>();
                SetupDevToolsPopup();
            }
            else if (DevToolsMonoManager != null)
            {
                Object.Destroy(DevToolsMonoManager.gameObject);
                DevToolsMonoManager = null;
            }
        }

        #endregion


        #region  --- Private Methods ---
        protected virtual void SetupDevToolsPopup()
        {
            if(!IsDevtoolEnabled) return;
            
            DevToolsMonoManager.PopupContainer.gameObject.AddComponent<SwDevToolsPopup>();
            DevToolsMonoManager.Setup(FilesCacheManager);
        }
            
        private T Create<T>() where T : SwDevToolsMonoManager
        {
            var swDevToolsPrefab = (GameObject) Resources.Load(DEVTOOLS_PREFAB_PATH);

            if (swDevToolsPrefab == null)
            {
                SwInfra.Logger.LogError(EWisdomLogType.Devtools, "Couldn't load DevTools prefab");
            }

            var devTools = Object.Instantiate(swDevToolsPrefab);

            return devTools.GetComponent<T>();
        }

        #endregion
    }
}
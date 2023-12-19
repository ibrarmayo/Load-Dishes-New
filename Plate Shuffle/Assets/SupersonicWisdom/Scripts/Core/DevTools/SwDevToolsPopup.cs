using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SupersonicWisdomSDK
{
    internal class SwDevToolsPopup : MonoBehaviour
    {
        #region --- Events ---

        public event Action ClosedEvent;

        #endregion


        #region --- Members ---

        protected static SwFilesCacheManager FilesCacheManager;
        protected LayoutGroup ButtonLayoutGroup;
        protected SwButton ButtonPrefab;
        
        private Dictionary<string, Action> _buttons = new Dictionary<string, Action>
        {
            { "Clear SDK Cache", ClearSDKCache },
            { "Delete PlayerPrefs", DeletePlayerPrefs },
            { "Quit Application", QuitApplication },
        };

        #endregion
        

        #region --- Button Methods ---

        public static void ClearSDKCache()
        {
            var files = FilesCacheManager?.GetAllFilesFromCache() ?? new string[] { };

            foreach (var file in files)
            {
                FilesCacheManager?.DeleteFile(file);
            }
        }

        public static void DeletePlayerPrefs()
        {
            SwInfra.KeyValueStore.DeleteAll();
        }

        public static void QuitApplication()
        {
            Application.Quit();
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }
        
        public void Setup(SwFilesCacheManager filesCacheManager, LayoutGroup buttonLayoutGroup, SwButton buttonPrefab)
        {
            FilesCacheManager = filesCacheManager;
            ButtonLayoutGroup = buttonLayoutGroup;
            ButtonPrefab = buttonPrefab;
        }
        
        public void Hide()
        {
            ClosedEvent?.Invoke();
        }

        #endregion
        
        
        #region --- Private Methods ---
        
        protected virtual void CreateButtons(Dictionary<string, Action> buttons = null)
        {
            foreach (var button in _buttons)
            {
                var swButton = (SwButton) Instantiate(ButtonPrefab, ButtonLayoutGroup.transform);
                swButton.Text = button.Key;
                swButton.onClick.AddListener(() => button.Value.Invoke());
            }
        }

        protected void ConcatButtons(Dictionary<string, Action> buttons)
        {
            if (buttons == null) return;
            
            foreach (var button in buttons)
            {
                if (!_buttons.ContainsKey(button.Key) )   
                {
                    _buttons.Add(button.Key, button.Value);
                }
            }
        }

        #endregion
    }
}
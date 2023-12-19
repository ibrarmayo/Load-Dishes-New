using UnityEngine;
using UnityEngine.UI;

namespace SupersonicWisdomSDK
{
    internal class SwDevToolsMonoManager : MonoBehaviour
    {
        #region --- Inspector ---

        [SerializeField] private SwDevToolsScreenStickyWidget widget;
        [SerializeField] private LayoutGroup buttonLayoutGroup;
        [SerializeField] private SwButton buttonPrefab;
        [SerializeField] private Transform popupContainer;

        #endregion


        #region --- Constants ---

        private const float ANIMATION_DURATION = 1f;

        #endregion


        #region --- Private Members ---

        private SwDevToolsPopup _popup;

        #endregion


        #region --- Properties ---

        internal Transform PopupContainer => popupContainer;

        #endregion

        
        #region --- Mono Override ---

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            widget.ClickedEvent += OnWidgetClicked;
        }

        #endregion


        #region --- Public Methods ---

        public void Setup(SwFilesCacheManager filesCacheManager)
        {
            _popup = popupContainer.GetComponent<SwDevToolsPopup>();
            _popup.Setup(filesCacheManager, buttonLayoutGroup, buttonPrefab);
            _popup.ClosedEvent -= OnPopupClosed;
            _popup.ClosedEvent += OnPopupClosed;
            _popup.gameObject.SetActive(false);
        }

        public void HidePopup ()
        {
            _popup.Hide();
        }

        #endregion


        #region --- Event Handler ---

        private void OnPopupClosed()
        {
            widget.gameObject.SetActive(true);
            _popup.ShrinkObject(duration: ANIMATION_DURATION, callback: () => _popup.gameObject.SetActive(false));
            widget.Show();
        }

        private void OnWidgetClicked()
        {
            _popup.gameObject.SetActive(true);
            _popup.EnlargeObject(duration: ANIMATION_DURATION);
        }

        #endregion
    }
}
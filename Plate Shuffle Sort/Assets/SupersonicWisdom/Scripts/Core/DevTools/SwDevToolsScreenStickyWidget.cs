using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SupersonicWisdomSDK
{
    [RequireComponent(typeof(RectTransform))]
    internal class SwDevToolsScreenStickyWidget : MonoBehaviour
    {
        #region --- Events ---

        public event Action ClickedEvent;

        #endregion


        #region --- Inspector ---

        [SerializeField] private Canvas canvas;

        #endregion


        #region --- Constants ---

        private const float ROLL_SPEED = 6.5f;

        #endregion

        
        #region --- Members ---

        private bool _wasDragged;
        private RectTransform _rectTransform;
        private Vector3 _previousLocation;

        #endregion


        #region --- Mono Override ---

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _previousLocation = new Vector3(-Screen.width / 2f + _rectTransform.sizeDelta.x / 2, 0, 0);
            _rectTransform.localPosition = _previousLocation;
        }

        #endregion


        #region --- Public Methods ---

        public void DragHandler(BaseEventData data)
        {
            _wasDragged = true;
            var pointerData = (PointerEventData)data;

            Vector2 position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform, pointerData.position, canvas.worldCamera, out position);

            transform.position = canvas.transform.TransformPoint(position);
        }

        public void MouseUpHandler(BaseEventData data)
        {
            if (_wasDragged)
            {
                _wasDragged = false;
                SetPosition(GetCurrentSize());
            }
            else
            {
                StartCoroutine(RollToScreenCanter());
            }
        }

        public void Show()
        {
            StartCoroutine(RollFromScreenCanter());
        }

        #endregion


        #region --- Private Methods ---

        private IEnumerator RollToScreenCanter()
        {
            var animationTime = 5f;
            var startPosition = _rectTransform.localPosition;
            var toPosition = Vector3.zero;
            var fromRotation = Vector3.zero;
            var toRotation = new Vector3(0f, 0f, 360f);

            yield return StartCoroutine(RollTo(startPosition, toPosition, fromRotation, toRotation, animationTime));

            ClickedEvent?.Invoke();
            gameObject.SetActive(false);
        }

        private IEnumerator RollFromScreenCanter()
        {
            _rectTransform.localPosition = Vector3.zero;
            gameObject.SetActive(true);

            var animationTime = 5f;
            var startPosition = _rectTransform.localPosition;
            var toPosition = _previousLocation;
            var fromRotation = Vector3.zero;
            var toRotation = new Vector3(0f, 0f, 360f);

            yield return StartCoroutine(RollTo(startPosition, toPosition, fromRotation, toRotation, animationTime));
        }

        private IEnumerator RollTo(Vector3 startPosition, Vector3 toPosition, Vector3 fromRotation, Vector3 toRotation, float animationTime)
        {
            var animationPassedTime = 0f;

            while (Vector3.Distance(_rectTransform.localPosition, toPosition) > 2f)
            {
                animationPassedTime += Time.deltaTime * ROLL_SPEED;
                _rectTransform.localPosition = Vector3.Lerp(startPosition, toPosition, animationPassedTime / animationTime);
                _rectTransform.localEulerAngles = Vector3.Lerp(fromRotation, toRotation, animationPassedTime / animationTime);

                yield return null;
            }

            _rectTransform.localPosition = toPosition;
            _rectTransform.eulerAngles = toRotation;
        }

        private void SetPosition(EScreenSize size)
        {
            var screenHalfWidth = Screen.width / 2;

            var sizeDelta = _rectTransform.sizeDelta;
            var myHalfWidth = sizeDelta.x / 2;

            var myPosition = transform.localPosition;

            switch (size)
            {
                case EScreenSize.Left:
                    _rectTransform.localPosition = new Vector3(-screenHalfWidth + myHalfWidth, myPosition.y, 0);
                    break;
                case EScreenSize.Right:
                    _rectTransform.localPosition = new Vector3(screenHalfWidth - myHalfWidth, myPosition.y, 0);
                    break;
            }

            _previousLocation = _rectTransform.localPosition;
        }

        private EScreenSize GetCurrentSize()
        {
            var screenHalfWidth = Screen.width / 2;
            
            float leftDistance;
            float rightDistance;

            var widthDistanceFromCenter = Mathf.Abs(_rectTransform.localPosition.x);

            if (_rectTransform.localPosition.x < 0)
            {
                leftDistance = screenHalfWidth - widthDistanceFromCenter;
                rightDistance = screenHalfWidth + widthDistanceFromCenter;
            }
            else
            {
                rightDistance = screenHalfWidth - widthDistanceFromCenter;
                leftDistance = screenHalfWidth + widthDistanceFromCenter;
            }

            var minDistance = Mathf.Min(leftDistance, rightDistance);

            if (Math.Abs(minDistance - leftDistance) < 0.1f)
            {
                return EScreenSize.Left;
            }
            
            return EScreenSize.Right;
        }

        #endregion
    }

    internal enum EScreenSize
    {
        Left,
        Right,
    }
}
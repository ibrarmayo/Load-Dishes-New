using System.Collections;
using System.Globalization;
using UnityEngine;

namespace SupersonicWisdomSDK
{
    internal class SwTimerManager : ISwReadyEventListener
    {
        #region --- Constants ---
        
        private const int PLAYTIME_TICK_INTERVAL = 1;
        private const int SAVE_PLAYTIME_INTERVAL = 5;
        private const string ACCUMULATED_SESSIONS_PLAYTIME = "AccumulatedSessionsPlaytime";

        #endregion


        #region --- Members ---

        protected readonly ISwTimer _currentSessionPlaytimeStopWatch;
        private readonly float _previousSessionsPlaytimeInSeconds;
        private readonly SwCoreMonoBehaviour _mono;

        #endregion


        #region --- Properties ---

        public ISwTimerListener CurrentSessionPlaytimeStopWatch
        {
            get { return _currentSessionPlaytimeStopWatch; }
        }

        public float AllSessionsPlaytime
        {
            get { return _previousSessionsPlaytimeInSeconds + _currentSessionPlaytimeStopWatch?.Elapsed ?? 0; }
        }

        #endregion


        #region --- Construction ---

        public SwTimerManager(SwCoreMonoBehaviour mono)
        {
            _mono = mono;
            _currentSessionPlaytimeStopWatch = SwStopWatch.Create(mono.gameObject, $"{ETimers.CurrentSessionPlaytimeMinutes}", true, PLAYTIME_TICK_INTERVAL);
            float.TryParse(SwInfra.KeyValueStore.GetString(ACCUMULATED_SESSIONS_PLAYTIME, "0"), NumberStyles.Float, CultureInfo.InvariantCulture, out _previousSessionsPlaytimeInSeconds);
            _mono.ApplicationPausedEvent += OnApplicationPaused;
        }

        ~SwTimerManager()
        {
            _mono.ApplicationPausedEvent -= OnApplicationPaused;
        }

        public void OnSwReady()
        {
            _currentSessionPlaytimeStopWatch.StartTimer();
            SavePlaytimeRepeating();
        }

        #endregion


        #region --- Public Methods ---

        private void OnApplicationPaused(bool isPaused)
        {
            if (!isPaused) return;

            SavePlaytime();
        }

        private void SavePlaytimeRepeating()
        {
            _mono.RunActionEndlessly(SavePlaytime, SAVE_PLAYTIME_INTERVAL, () => false);
        }

        private void SavePlaytime()
        {
            // F symbolizes float format - #.## 
            SwInfra.KeyValueStore.SetString(ACCUMULATED_SESSIONS_PLAYTIME, AllSessionsPlaytime.ToString("F", CultureInfo.InvariantCulture));
        }

        #endregion
    }

    internal enum ETimers
    {
        CurrentSessionPlaytimeMinutes,
    }
}
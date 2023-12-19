#if SW_STAGE_STAGE1_OR_ABOVE
using System;

namespace SupersonicWisdomSDK
{
    [Serializable]
    public class SwStage1TesterState
    {
        #region --- Properties ---

        public long Age { get; set; }
        public long TodaySessionsCount { get; set; }
        public long TotalSessionsCount { get; set; }
        public long Level { get; set; }

        #endregion


        #region --- Construction ---

        public SwStage1TesterState(long age, long todaySessionsCount, long totalSessionsCount, long level)
        {
            Age = age;
            TodaySessionsCount = todaySessionsCount;
            TotalSessionsCount = totalSessionsCount;
            Level = level;
        }

        public SwStage1TesterState() { }

        #endregion


        #region --- Public Methods ---

        public override string ToString()
        {
            return "Age: " + Age + "\nTodaySC: " + TodaySessionsCount + "\nTSC: " + TotalSessionsCount + "\nCompleted Levels: " + Level;
        }

        #endregion
    }
}
#endif
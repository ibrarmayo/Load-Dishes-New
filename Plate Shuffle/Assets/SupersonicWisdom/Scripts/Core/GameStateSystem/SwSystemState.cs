namespace SupersonicWisdomSDK
{
    internal static class SwSystemState
    {
        public enum EGameState
        {
            Loading,
            //Start Playtime settings
            Time,
            Regular,
            Bonus,
            Tutorial,
            Meta,
            //End Playtime settings
            BetweenLevels,
        }

        public enum EStateEvent
        {
            Completed,
            Failed,
            Skipped,
        } 
    }
}
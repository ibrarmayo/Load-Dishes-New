#if SW_STAGE_STAGE1_OR_ABOVE

namespace SupersonicWisdomSDK
{
    internal class SwStage1SessionListener : ISwSessionListener
    {
        #region --- Members ---

        private readonly SwCoreUserData _coreUserData;

        #endregion


        #region --- Construction ---

        public SwStage1SessionListener(SwCoreUserData coreUserData)
        {
            _coreUserData = coreUserData;
        }

        #endregion


        #region --- Public Methods ---

        public virtual void OnSessionEnded(string sessionId)
        {
            SwInfra.Logger.Log(EWisdomLogType.Session, $"Unity:SwSessions:OnSessionEnded:{sessionId}");
        }

        public virtual void OnSessionStarted(string sessionId)
        {
            UpdateUserStateOnStartSession(sessionId);
            SwInfra.Logger.Log(EWisdomLogType.Session, $"Unity:SwSessions:OnSessionStarted:{sessionId}");
        }

        #endregion


        #region --- Private Methods ---

        private void UpdateUserStateOnStartSession(string sessionId)
        {
            _coreUserData.ModifyUserStateSync(mutableUserState =>
            {
                _coreUserData.UpdateAge(mutableUserState);
                mutableUserState.SessionId = sessionId;
                mutableUserState.todaySessionsCount++;
                mutableUserState.totalSessionsCount++;
            });

            _coreUserData.AfterUserStateChangeInternal(SwUserStateChangeReason.SessionStart, true);
        }

        #endregion
    }
}

#endif
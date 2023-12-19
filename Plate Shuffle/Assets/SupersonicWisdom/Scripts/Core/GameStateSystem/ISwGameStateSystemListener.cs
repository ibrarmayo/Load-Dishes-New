namespace SupersonicWisdomSDK
{
    internal interface ISwGameStateSystemListener
    {
        void HandleGameSystemStateChange(object sender, SwSystemStateEventArgs eventArgs);
    }
}
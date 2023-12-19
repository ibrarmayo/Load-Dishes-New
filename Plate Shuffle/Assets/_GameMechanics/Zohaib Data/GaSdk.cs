using SupersonicWisdomSDK;
using UnityEngine;

namespace _GameMechanics.Zohaib_Data
{
    public class GaSdk : MonoBehaviour
    {
        void Awake()
        {
            // Subscribe
            SupersonicWisdom.Api.AddOnReadyListener(OnSupersonicWisdomReady);
            // Then initialize
            SupersonicWisdom.Api.Initialize();
        }

        void OnSupersonicWisdomReady()
        {
            SupersonicWisdom.Api.NotifyLevelStarted(0, 0, null);

            // Start your game from this point
        }
    }
}

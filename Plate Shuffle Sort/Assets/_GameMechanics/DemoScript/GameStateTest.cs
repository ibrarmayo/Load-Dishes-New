using AxisGames.BasicGameSet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateTest : MonoBehaviour
{
    public void LevelComplete() { GameController.changeGameState.Invoke(GameState.Complete); }

    public void LevelFail() { GameController.changeGameState.Invoke(GameState.Fail); }
}

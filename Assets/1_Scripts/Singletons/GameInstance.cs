using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DcClass;

public struct PlayerData
{
    public bool active;
    public int sprite;
}

public class GameInstance : Singleton<GameInstance>
{
    PlayerData[] players = new PlayerData[InputManager.MAX_PLAYERS];
    public PlayerData[] PlayerData { get { return players; } }


    public void PlayerJoin(int id)
    {
        players[id].active = true;
        players[id].sprite = 0;
    }

    public void PlayerLeft(int id)
    {
        players[id].active = false;
        players[id].sprite = 0;
    }
}

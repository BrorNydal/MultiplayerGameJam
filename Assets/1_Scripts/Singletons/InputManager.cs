using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public enum ControllerType
{
    WASD, Arrows
}

public class InputManager : Singleton<InputManager>
{
    public const int MAX_PLAYERS = 4;

    int playersActive = 0;
    PlayerInput[] players = new PlayerInput[4];

    [SerializeField] InputAction joinAction;
    [SerializeField] InputAction leaveAction;

    PlayerInputSettings playerInputSettings;
    public PlayerInputSettings PlayerInputSettings { get { return playerInputSettings; } }

    protected override void Awake()
    {
        base.Awake();

        playerInputSettings = new PlayerInputSettings();
    }

    private void Start()
    {
        PlayerInputManager.instance.onPlayerJoined += OnPlayerJoined;
        PlayerInputManager.instance.onPlayerLeft += OnPlayerLeft;

        joinAction.Enable();
        joinAction.started += JoinAction_started;

        leaveAction.Enable();
        leaveAction.started += LeaveAction_started;
    }    

    private void OnPlayerJoined(PlayerInput input)
    {
        PlayerController playerController = input.GetComponent<PlayerController>();
        if (playerController)
        {

        }
        else
            Debug.Log("No player controller found!");

        input.defaultActionMap = input.gameObject.name;
        players[playersActive] = input;   
        
        playersActive++; 
    }

    private void OnPlayerLeft(PlayerInput input)
    {
        for(int i = 0; i < MAX_PLAYERS; i++)
        {
            if(input == players[i])
            {
                players[i] = null;
                playersActive--;
            }
        }
    }

    private void JoinAction_started(InputAction.CallbackContext obj)
    {
        PlayerInputManager.instance.JoinPlayerFromActionIfNotAlreadyJoined(obj);
    }

    private void LeaveAction_started(InputAction.CallbackContext obj)
    {
    }
}

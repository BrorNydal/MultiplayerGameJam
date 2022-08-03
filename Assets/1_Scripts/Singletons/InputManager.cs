using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DcClass;

[System.Serializable]
public enum ControllerType
{
    Gamepad, Keyboard, SecondaryKeyboard
}

public struct PlayerControllerData
{
    public ControllerType ControllerType;
    public PlayerInput playerInput;
    public PlayerMovement playerMovement;
}

public class InputManager : Singleton<InputManager>
{
    public const int MAX_PLAYERS = 4;

    int playersActive = 0;
    Dictionary<ControllerType, int> controllerCount = new Dictionary<ControllerType, int>();    

    bool Space { get { return playersActive < MAX_PLAYERS; } }

    PlayerControllerData[] players = new PlayerControllerData[4];

    [SerializeField] InputAction controllerJoin;    
    [SerializeField] InputAction leaveAction;
    [SerializeField] InputAction keyboardJoin;
    [SerializeField] InputAction secondaryKeyboardJoin;

    PlayerInputSettings playerInputSettings;
    public PlayerInputSettings PlayerInputSettings { get { return playerInputSettings; } }

    ControllerType joiningControllerType;
    
    

    protected override void Awake()
    {
        base.Awake();

        playerInputSettings = new PlayerInputSettings();
    }

    private void Start()
    {
        controllerCount.Add(ControllerType.Gamepad, 0);
        controllerCount.Add(ControllerType.Keyboard, 0);
        controllerCount.Add(ControllerType.SecondaryKeyboard, 0);

        PlayerInputManager.instance.onPlayerJoined += OnPlayerJoined;
        PlayerInputManager.instance.onPlayerLeft += OnPlayerLeft;

        controllerJoin.Enable();
        controllerJoin.started += gamepadJoinAction_started;

        leaveAction.Enable();
        leaveAction.started += LeaveAction_started;

        keyboardJoin.Enable();
        keyboardJoin.started += keyboardJoinAction_started;

        secondaryKeyboardJoin.Enable();
        secondaryKeyboardJoin.started += SecondaryKeyboardJoin_started;
    }

    private void SecondaryKeyboardJoin_started(InputAction.CallbackContext obj)
    {
        if (Space && controllerCount[ControllerType.Keyboard] > 0 && controllerCount[ControllerType.SecondaryKeyboard] <= 2)
        {
            joiningControllerType = ControllerType.SecondaryKeyboard;
            PlayerInputManager.instance.JoinPlayer(playersActive);
        }
    }    

    private void keyboardJoinAction_started(InputAction.CallbackContext obj)
    {
        if (Space && controllerCount[ControllerType.Keyboard] <= 2)
        {
            joiningControllerType = ControllerType.Keyboard;
            PlayerInputManager.instance.JoinPlayer(playersActive);            
        }
    }

    private void gamepadJoinAction_started(InputAction.CallbackContext obj)
    {
        if (Space && controllerCount[ControllerType.Gamepad] <= 4)
        {
            joiningControllerType = ControllerType.Gamepad;
            PlayerInputManager.instance.JoinPlayerFromActionIfNotAlreadyJoined(obj);
        }
    }

    private void OnPlayerJoined(PlayerInput input)
    {
        PlayerControllerData data;
        data.playerInput = input;

        input.gameObject.name = "Player" + (playersActive + 1).ToString();

        PlayerMovement playerMovement = input.GetComponent<PlayerMovement>();
        data.playerMovement = playerMovement;
        data.ControllerType = joiningControllerType;

        if (playerMovement)
        {
            controllerCount[joiningControllerType]++;

            switch (joiningControllerType)
            {
                case ControllerType.Gamepad:
                    playerMovement.SetPlayerInput(input);
                    playerMovement.ChangeInputActionMap(input.actions.FindActionMap("Player" + (controllerCount[ControllerType.Gamepad]).ToString()));
                    break;
                case ControllerType.Keyboard:
                    playerMovement.SetPlayerInput(input);
                    playerMovement.ChangeInputActionMap(input.actions.FindActionMap("PlayerK" + (controllerCount[ControllerType.Keyboard]).ToString()));
                    break;
                case ControllerType.SecondaryKeyboard:
                    playerMovement.SetPlayerInput(input);
                    
                    for(int i = 0; i < MAX_PLAYERS; i++)
                    {
                        if (players[i].ControllerType == ControllerType.Keyboard && !players[i].playerMovement.HasSecondKeyboard)
                            players[i].playerMovement.AttachSecondKeyboard(playerMovement);
                    }

                    //playerMovement.ChangeInputActionMap(input.actions.FindActionMap("PlayerK" + (controllerCount[ControllerType.SecondaryKeyboard]).ToString()));
                    break;
            }
        }
        else
            Debug.Log("No player controller found!");

        players[playersActive] = data;        
        playersActive++;
    }

    private void OnPlayerLeft(PlayerInput input)
    {
        //for(int i = 0; i < MAX_PLAYERS; i++)
        //{
        //    if(input == players[i])
        //    {
        //        players[i] = null;
        //        playersActive--;
        //    }
        //}
    }    

    private void LeaveAction_started(InputAction.CallbackContext obj)
    {
    }
}

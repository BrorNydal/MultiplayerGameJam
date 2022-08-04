using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DcClass;
using UnityEngine.InputSystem.Users;

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
    bool RoomForPlayers { get { return playersActive < MAX_PLAYERS; } }
    Dictionary<ControllerType, int> controllerCount = new Dictionary<ControllerType, int>();    
    PlayerInput[] players = new PlayerInput[MAX_PLAYERS];
    InputDevice[] devices = new InputDevice[MAX_PLAYERS];

    [SerializeField] GameObject playerPrefab;
    [SerializeField] InputAction controllerJoin;
    [SerializeField] InputAction leaveAction;
    [SerializeField] InputAction keyboardJoin;
    [SerializeField] InputAction secondaryKeyboardJoin;

    PlayerInputSettings playerInputSettings;
    public PlayerInputSettings PlayerInputSettings { get { return playerInputSettings; } }

    ControllerType joiningControllerType;

    List<int> secondaryKeyboards;

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

        controllerJoin.Enable();
        controllerJoin.started += gamepadJoinAction_started;

        leaveAction.Enable();
        //leaveAction.started += LeaveAction_started;

        keyboardJoin.Enable();
        keyboardJoin.started += keyboardJoinAction_started;

        secondaryKeyboardJoin.Enable();
        secondaryKeyboardJoin.started += SecondaryKeyboardJoin_started;
    }

    private void keyboardJoinAction_started(InputAction.CallbackContext obj)
    {
        if (DeviceExists(obj.control.device)) return;
        if (RoomForPlayers && controllerCount[ControllerType.Keyboard] <= 2)
        {
            devices[playersActive] = obj.control.device;
            PlayerInput input = PlayerInput.Instantiate(playerPrefab, controlScheme: "Keyboard");
            PlayerJoin(input);
            controllerCount[ControllerType.Keyboard]++;
            //joiningControllerType = ControllerType.Keyboard;
            //PlayerInputManager.instance.JoinPlayer(playersActive);
        }
    }

    private void SecondaryKeyboardJoin_started(InputAction.CallbackContext obj)
    {
        if (RoomForPlayers && controllerCount[ControllerType.Keyboard] > 0 && controllerCount[ControllerType.SecondaryKeyboard] < controllerCount[ControllerType.Keyboard])
        {
            devices[playersActive] = obj.control.device;
            PlayerInput input = PlayerInput.Instantiate(playerPrefab, controlScheme: "RightKeyboard", pairWithDevice: Keyboard.current);
            PlayerJoin(input);
            controllerCount[ControllerType.SecondaryKeyboard]++;
        }
    }

    private void gamepadJoinAction_started(InputAction.CallbackContext obj)
    {
        if (DeviceExists(obj.control.device)) return;
        if (RoomForPlayers && controllerCount[ControllerType.Gamepad] <= 4)
        {
            devices[playersActive] = obj.control.device;
            PlayerInput input = PlayerInput.Instantiate(playerPrefab, controlScheme: "Gamepad");
            PlayerJoin(input);
            controllerCount[ControllerType.Gamepad]++;
            //PlayerInput.Instantiate(playerPrefab, controlScheme: "Gamepad");
            //joiningControllerType = ControllerType.Gamepad;
            //PlayerInputManager.instance.JoinPlayerFromActionIfNotAlreadyJoined(obj);
        }
    }
    private void PlayerJoin(PlayerInput input)
    {
        PlayerMovement movement = input.GetComponent<PlayerMovement>();

        if (movement != null)
        {
            movement.PlayerInput = input;
            movement.InputActionMap = input.currentActionMap;
        }

        players[playersActive] = input;
        playersActive++;
        input.gameObject.name = "Player" + playersActive.ToString();
    }

    private bool DeviceExists(InputDevice device)
    {
        bool exists = false;

        foreach(var dev in devices)
        {
            if (dev == device) exists = true;
        }

        return exists;
    }
}

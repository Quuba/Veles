using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//Based of https://www.youtube.com/watch?v=VzOEM-4A2OM
public class DebugController : MonoBehaviour
{
    private void OnEnable()
    {
        InputController.Instance.UIActionMap.ToggleDebug.performed += OnToggleDebug;
        InputController.Instance.UIActionMap.Enter.performed += OnPressEnter;

    }

    private void OnDisable()
    {
        InputController.Instance.UIActionMap.ToggleDebug.performed -= OnToggleDebug;
        InputController.Instance.UIActionMap.Enter.performed -= OnPressEnter;


    }

    [SerializeField] private int commandLabelHeight = 40;
    
    private string input;
    
    private bool showConsole;
    private bool showHelp;

    
    
    static DebugCommand HELP;
    static DebugCommand<string> GOTO;

    enum HelpCommandType
    {
        Basic,
        Goto
    }

    private HelpCommandType helpCommandType;
    
    [Serializable]
    struct GotoPlace
    {
        public string name;
        public Vector2 position;
    }

    [SerializeField] private GotoPlace[] gotoPlaces;
    private readonly Dictionary<string, Vector2> gotoPlacesDict = new Dictionary<string, Vector2>();

    private void OnValidate()
    {
        foreach (var place in gotoPlaces)
        {
            gotoPlacesDict[place.name] = place.position;
        }
    }

    private List<object> commandList;

    private void Awake()
    {
        GOTO = new DebugCommand<string>("goto", "teleports player to specified place", "goto <target>|help", (value) =>
        {
            switch (value)
            {
                case "help":
                    showHelp = true;
                    helpCommandType = HelpCommandType.Goto;
                    break;
                default:
                    if (gotoPlacesDict.ContainsKey(value))
                    {
                        Debug.Log($"Going to: {value} {gotoPlacesDict[value]}");
                        Player.Instance.TeleportTo(gotoPlacesDict[value]);
                    }
                    
                    GameController.Instance.Resume();
                    showConsole = false;
                    
                    break;
            }
        });

        HELP = new DebugCommand("help", "shows available commands", "help", () =>
        {
            showHelp = true;
            helpCommandType = HelpCommandType.Basic;
        });
        commandList = new List<object>
        {
            HELP,
            GOTO,
        };
    }

    private void OnToggleDebug(InputAction.CallbackContext callbackContext)
    {
        showConsole = !showConsole;
        //TODO: Pause/Resume game
        if (showConsole)
        {
            GameController.Instance.Pause();
        }
        else
        {
            GameController.Instance.Resume();
        }
    }

    private void OnPressEnter(InputAction.CallbackContext callbackContext)
    {
        if (showConsole)
        {
            HandleInput();
            input = "";
        }
    }


    private Vector2 scroll;
    private void OnGUI()
    {
        if (!showConsole) return;
        
        float y = 0f;
        GUI.skin.label.fontSize = 30;
        GUI.skin.label.hover.textColor = GUI.skin.label.normal.textColor;


        if (showHelp)
        {
            GUI.Box(new Rect(0, y, Screen.width, 100),"");

            Rect viewport = new Rect(0, 0, Screen.width - 30, commandLabelHeight * commandList.Count);

            scroll = GUI.BeginScrollView(new Rect(0, y + 5, Screen.width, 90), scroll, viewport);

            switch (helpCommandType)
            {
                case HelpCommandType.Basic:
                    for (int i = 0; i < commandList.Count; i++)
                    {
                        DebugCommandBase command = commandList[i] as DebugCommandBase;

                        string label = $"{command.CommandFormat} => {command.CommandDescription}";
                        Rect labelRect = new Rect(5, commandLabelHeight * i, viewport.width - 100, commandLabelHeight);
                        GUI.Label(labelRect, label);
                    }
                    break;
                case HelpCommandType.Goto:
                    int index = 0;
                    foreach (KeyValuePair<string, Vector2> pair in gotoPlacesDict)
                    {
                        string label = $"{pair.Key} => {pair.Value}";
                        Rect labelRect = new Rect(5, commandLabelHeight * index, viewport.width - 100, commandLabelHeight);
                        GUI.Label(labelRect, label);
                        index++;
                    }
                    break;
                    
            }
            GUI.EndScrollView();
            
            y += 100;
        }
        

        GUI.Box(new Rect(0, y, Screen.width, 60f), "");
        GUI.backgroundColor = new Color(0, 0, 0, 0);
        GUI.skin.textField.fontSize = 30;
        GUI.SetNextControlName("inputField");
        input = GUI.TextField(new Rect(10f, y + 5f, Screen.width - 20f, commandLabelHeight), input);
        
        GUI.FocusControl("inputField");
    }
    
    private void HandleInput()
    {
        string[] properties = input.Split(' ');
        for (int i = 0; i < commandList.Count; i++)
        {
            DebugCommandBase commandBase = commandList[i] as DebugCommandBase;
            if (input.Contains(commandBase.CommandId))
            {
                if (commandList[i] is DebugCommand)
                {
                    (commandList[i] as DebugCommand)?.Invoke();
                }else if (commandList[i] is DebugCommand<string>)
                {
                    (commandList[i] as DebugCommand<string>)?.Invoke(properties[1]); // Only the first property
                }else if (commandList[i] is DebugCommand<int>)
                {
                    (commandList[i] as DebugCommand<int>)?.Invoke(int.Parse(properties[1])); // Only the first property
                }
            }
        }
    }
}

public class DebugCommandBase
{
    private string commandId;
    private string commandDescription;
    private string commandFormat;

    public string CommandId => commandId;
    public string CommandDescription => commandDescription;
    public string CommandFormat => commandFormat;

    public DebugCommandBase(string commandId, string commandDescription, string commandFormat)
    {
        this.commandId = commandId;
        this.commandDescription = commandDescription;
        this.commandFormat = commandFormat;
    }
}

public class DebugCommand : DebugCommandBase
{
    private Action command;

    public DebugCommand(string commandId, string commandDescription, string commandFormat, Action command) :
        base(commandId, commandDescription, commandFormat)
    {
        this.command = command;
    }

    public void Invoke()
    {
        command.Invoke();
    }
}

public class DebugCommand<T1> : DebugCommandBase
{
    private Action<T1> command;

    public DebugCommand(string commandId, string commandDescription, string commandFormat, Action<T1> command) :
        base(commandId, commandDescription, commandFormat)
    {
        this.command = command;
    }

    public void Invoke(T1 value)
    {
        command.Invoke(value);
    }
}
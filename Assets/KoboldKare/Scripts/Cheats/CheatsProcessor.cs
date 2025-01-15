using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class CheatsProcessor : MonoBehaviour {
    private static CheatsProcessor instance;
    [SerializeField, SerializeReference, SerializeReferenceButton]
    private List<Command> commands;
    private const int maxLength = 10000;

    private StringBuilder commandOutput;

    public delegate void OutputChangedAction(string newOutput);

    private event OutputChangedAction outputChanged;

    public static void AppendText(string text) {
        instance.commandOutput.Append(text);
        if (instance.commandOutput.Length > maxLength) {
            instance.commandOutput.Remove(0, Mathf.Max(instance.commandOutput.Length - maxLength,0));
        }

        instance.outputChanged?.Invoke(instance.commandOutput.ToString());
    }

    public static void AddOutputChangedListener(OutputChangedAction action) {
        instance.outputChanged += action;
    }

    public static void RemoveOutputChangedListener(OutputChangedAction action) {
        instance.outputChanged -= action;
    }

    public static void SetCheatsEnabled(bool cheatsEnabled) {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions() {
            CachingOption = EventCaching.AddToRoomCache,
            Receivers = ReceiverGroup.All,
        };
        PhotonNetwork.RaiseEvent(NetworkManager.CustomCheatEvent, cheatsEnabled, raiseEventOptions, new SendOptions() { Reliability = true });
    }

    public static bool GetCheatsEnabled() {
        return (Application.isEditor && PhotonNetwork.IsMasterClient) || NetworkManager.instance.GetCheatsEnabled();
    }

    public static ReadOnlyCollection<Command> GetCommands() {
        return instance.commands.AsReadOnly();
    }

    void Awake() {
        //Check if instance already exists
        if (instance == null) {
            //if not, set instance to this
            instance = this;
        } else if (instance != this) {
            //If instance already exists and it's not this:
            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);
            return;
        }
        commandOutput = new StringBuilder();
    }

    public class CommandException : System.Exception {
        public CommandException(string message) : base(message) { }
    }

    private void ProcessCommand(Kobold kobold, string[] args) {
        // empty command
        if (args.Length <= 0) {
            return;
        }
        // First result is empty??? should be impossible but checking just in case.
        if (args[0].Length <= 0) {
            return;
        }
        // Not a command
        if (args[0][0] != '/') {
            return;
        }

        foreach (var command in commands) {
            if (command.GetArg0() == args[0]) {
                command.Execute(commandOutput, kobold, args);
                outputChanged?.Invoke(commandOutput.ToString());
                return;
            }
        }
        throw new CommandException($"`{args[0]}` Not a command. Use /help to see the available commands.");
    }

    public static void ProcessCommand(Kobold kobold, string command) {
        if (kobold == null || kobold != (Kobold)PhotonNetwork.LocalPlayer.TagObject) {
            return;
        }
        string[] args = command.Split(' ');
        try {
            instance.ProcessCommand(kobold, args);
        } catch (CommandException exception) {
            instance.commandOutput.Append($"<#ff4f00>{exception.Message}</color>\n");
            if (instance.commandOutput.Length > maxLength) {
                instance.commandOutput.Remove(0, Mathf.Max(instance.commandOutput.Length - maxLength,0));
            }
            instance.outputChanged?.Invoke(instance.commandOutput.ToString());
        }
    }

    private void OnValidate() {
        if (commands == null) {
            return;
        }

        foreach (var command in commands) {
            command.OnValidate();
        }
    }
}

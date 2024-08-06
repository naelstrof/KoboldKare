using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using Photon.Pun;
using UnityEngine;

public class CheatsProcessor : MonoBehaviour {
    private static CheatsProcessor instance;
    private bool cheatsEnabled = false;
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
        instance.cheatsEnabled = cheatsEnabled;
    }

    public static bool GetCheatsEnabled() {
        return instance.cheatsEnabled || Application.isEditor;
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
        if (!PhotonNetwork.IsMasterClient) {
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

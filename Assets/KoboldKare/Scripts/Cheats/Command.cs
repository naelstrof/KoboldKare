using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public class Command {
    public class AutocompleteResult {

        public string label;
        public string value;

        public AutocompleteResult(string value)
        {
            label = this.value = value;
        }

        public AutocompleteResult(string label, string value)
        {
            this.label = label;
            this.value = value;
        }

        public void Accept(TMP_InputField inputField) {
            var lastSpace = inputField.text.LastIndexOf(' ');
            if (lastSpace < 0) {
                inputField.text = $"{value} ";
            } else {
                inputField.text = $"{inputField.text.Substring(0, lastSpace)} {value} ";
            }
            inputField.caretPosition = inputField.text.Length;
            inputField.stringPosition = inputField.text.Length;
            inputField.selectionAnchorPosition = inputField.text.Length;
            inputField.selectionFocusPosition = inputField.text.Length;
            inputField.ForceLabelUpdate();
        }
    }

    /// <summary>
    /// This is used to cache and register commands and what they do. Arg0 should be the first argument. Example: "/give"
    /// </summary>
    /// <returns></returns>
    public virtual string GetArg0() => "null";

    [SerializeField]
    private LocalizedString description;
    public virtual LocalizedString GetDescription() => description;

    /// <summary>
    /// </summary>
    /// <param name="output"></param>
    /// <param name="k">The kobold that tried to run the command. Commands are only processed by the master client, use this kobold to figure out if they have permission to run the command.</param>
    /// <param name="args">The full argument list, space separated. For example: {"/give", "MelonJuice", "30"}.</param>
    public virtual void Execute(StringBuilder output, Kobold k, string[] args) {}

    public virtual void OnValidate() { }

    public virtual IEnumerable<AutocompleteResult> Autocomplete(int argumentIndex, string[] arguments, string text) {
        return Array.Empty<AutocompleteResult>();
    }
}

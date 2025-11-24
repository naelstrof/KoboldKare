using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ChatPanel : MonoBehaviour {
    [SerializeField] private ScrollRect chatScrollView;
    [SerializeField] private TMP_Text chatDisplay;
    [SerializeField] private TMP_InputField chatInput;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject autocompleteContainer;
    [SerializeField] private RectTransform autocompleteContent;
    [SerializeField] private GameObject autocompleteTemplate;
    
    void OnEnable() {
        GameManager.SetControlsActive(false);

        if (SteamManager.Initialized) {
            RectTransform rectTransform = chatInput.GetComponent<RectTransform>();
            SteamUtils.ShowFloatingGamepadTextInput(
                EFloatingGamepadTextInputMode.k_EFloatingGamepadTextInputModeModeSingleLine,
                (int)rectTransform.position.x, (int)rectTransform.position.y, (int)rectTransform.sizeDelta.x,
                (int)rectTransform.sizeDelta.y);
        }

        chatDisplay.text = CheatsProcessor.GetOutput();
        CheatsProcessor.AddOutputChangedListener(OnChatChanged);
        StartCoroutine(WaitThenSubscribe());
    }

    private void OnCloseChat(InputAction.CallbackContext obj) {
        MainMenu.ShowMenuStatic(MainMenu.MainMenuMode.None);
    }

    private bool subscribed = false;
    IEnumerator WaitThenSubscribe() {
        yield return new WaitForSecondsRealtime(0.1f);
        closeButton.interactable = true;
        var controls = GameManager.GetPlayerControls();
        controls.UI.Cancel.performed += OnCloseChat;
        controls.UI.AcceptAutoComplete.performed += OnAcceptAutoComplete;
        subscribed = true;
        chatInput.Select();
        chatInput.onSubmit.AddListener(OnTextSubmit);
        chatInput.onValueChanged.AddListener(OnTextChanged);
    }

    private void OnAcceptAutoComplete(InputAction.CallbackContext obj) {
        if (currentAutoCompleteResults.Count > 0) {
            currentAutoCompleteResults[0].Accept(chatInput);
        }
    }

    private void OnChatChanged(string newOutput) {
        chatDisplay.text = newOutput;
    }

    void Update() {
        if (EventSystem.current.currentSelectedGameObject != chatInput.gameObject) {
            EventSystem.current.SetSelectedGameObject(chatInput.gameObject);
        }
        chatInput.ActivateInputField();
    }

    private void OnDisable() {
        closeButton.interactable = false;
        GameManager.SetControlsActive(true);
        CheatsProcessor.RemoveOutputChangedListener(OnChatChanged);
        if (SteamManager.Initialized) {
            SteamUtils.DismissFloatingGamepadTextInput();
        }

        if (subscribed) {
            chatInput.onSubmit.RemoveListener(OnTextSubmit);
            chatInput.onValueChanged.RemoveListener(OnTextChanged);
            var controls = GameManager.GetPlayerControls();
            controls.UI.Cancel.performed -= OnCloseChat;
            controls.UI.AcceptAutoComplete.performed -= OnAcceptAutoComplete;
        } else {
            StopAllCoroutines();
        }
    }
    private List<Command.AutocompleteResult> currentAutoCompleteResults = new List<Command.AutocompleteResult>();

    private void ClearAutoComplete() {
        var suggestions = new Transform[autocompleteContent.childCount];
        for(int i=0; i < autocompleteContent.childCount; i++) {
            suggestions[i] = autocompleteContent.GetChild(i);
        }
        for (int i = 0; i < suggestions.Length; i++) {
            Destroy(suggestions[i].gameObject);
        }

        currentAutoCompleteResults.Clear();
    }
    private void AddAutoCompleteItem(Command.AutocompleteResult autoComplete) {
        currentAutoCompleteResults.Add(autoComplete);
    }

    private void SubmitAutoCompleteList() {
        currentAutoCompleteResults.Reverse();
        for (int i = 0; i < currentAutoCompleteResults.Count; i++) {
            var autoComplete = currentAutoCompleteResults[i];
            var instance = Instantiate(autocompleteTemplate, autocompleteContent, false);
            instance.SetActive(true);

            if (instance.TryGetComponent(out TMP_Text text)) {
                text.text = autoComplete.label;
            }

            if (!instance.TryGetComponent(out EventTrigger trigger)) {
                trigger = instance.AddComponent<EventTrigger>();
            }

            var entry = new EventTrigger.Entry() {
                eventID = EventTriggerType.PointerClick,
            };

            entry.callback.AddListener((_) => { autoComplete.Accept(chatInput); });

            trigger.triggers.Add(entry);
        }
        currentAutoCompleteResults.Reverse();
    }
    
    private void OnTextChanged(string t) {
        t = t.TrimStart();

        //We want to test this properly in editor, so ensure we use the network manager check instead of CheatsProcessor
        if (string.IsNullOrEmpty(t) || !t.StartsWith("/"))  {
            autocompleteContainer.SetActive(false);
        } else {
            autocompleteContainer.SetActive(true);
            ClearAutoComplete();
            if (t[0] != '/') return;
            if(t.IndexOf(' ') < 0) { //still writing the cheat
                var commands = CheatsProcessor.GetCommands();
                if (CheatsProcessor.GetCheatsEnabled()) {
                    foreach (var command in commands) {
                        if (command.GetArg0().Contains(t, System.StringComparison.OrdinalIgnoreCase)) {
                            AddAutoCompleteItem(new Command.AutocompleteResult(command.GetArg0()));
                        }
                    }
                } else {
                    AddAutoCompleteItem(new Command.AutocompleteResult("/cheats"));
                }
            } else {
                var arguments = t.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

                var commandName = arguments[0];

                //Assume the first command matching will be the one we want
                var command = CheatsProcessor.GetCommands().FirstOrDefault(x => x.GetArg0().Contains(commandName, System.StringComparison.OrdinalIgnoreCase));
                if (command == null) {
                    return;
                }

                var argIndex = t.Count(x => x == ' ');

                var lastSpace = t.LastIndexOf(' ');

                var argText = lastSpace + 1 < t.Length ? t.Substring(lastSpace + 1, t.Length - lastSpace - 1) : "";

                var results = command.Autocomplete(argIndex, arguments, argText).ToArray();

                if (results.Length == 0) {
                    autocompleteContainer.SetActive(false);
                } else {
                    foreach(var result in results) {
                        AddAutoCompleteItem(result);
                    }
                }
            }
        }
        SubmitAutoCompleteList();
    }

    private void OnTextSubmit(string t) {
        chatInput.text="";
        chatScrollView.normalizedPosition = new Vector2(0, 0);
        if (!string.IsNullOrEmpty(t)) {
            RaiseEventOptions options = new RaiseEventOptions() {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.All,
            };
            PhotonNetwork.RaiseEvent(NetworkManager.CustomChatEvent, t.TrimEnd(), options, SendOptions.SendReliable);
        }
        MainMenu.ShowMenuStatic(MainMenu.MainMenuMode.None);
    }
}

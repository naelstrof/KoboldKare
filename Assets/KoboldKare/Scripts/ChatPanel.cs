using System.Collections;
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
    [SerializeField] private TMPro.TMP_Text chatDisplay;
    [SerializeField] private TMPro.TMP_InputField chatInput;
    [SerializeField] private InputActionReference closeChatAction;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject autocompleteContainer;
    [SerializeField] private RectTransform autocompleteContent;
    [SerializeField] private GameObject autocompleteTemplate;

    private PlayerPossession playerControls;
    
    void OnEnable() {
        if (PlayerPossession.TryGetPlayerInstance(out playerControls)) {
            playerControls.SetControlsActive(false);
        }

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
        closeChatAction.action.Enable();
        closeChatAction.action.performed += OnCloseChat;
        subscribed = true;
        chatInput.Select();
        chatInput.onSubmit.AddListener(OnTextSubmit);
        chatInput.onValueChanged.AddListener(OnTextChanged);
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
        if (playerControls != null) {
            playerControls.SetControlsActive(true);
        }
        CheatsProcessor.RemoveOutputChangedListener(OnChatChanged);
        if (SteamManager.Initialized) {
            SteamUtils.DismissFloatingGamepadTextInput();
        }

        if (subscribed) {
            chatInput.onSubmit.RemoveListener(OnTextSubmit);
            chatInput.onValueChanged.RemoveListener(OnTextChanged);
            closeChatAction.action.performed -= OnCloseChat;
        } else {
            StopAllCoroutines();
        }
    }

    private void OnTextChanged(string t) {
        t = t.TrimStart();

        //We want to test this properly in editor, so ensure we use the network manager check instead of CheatsProcessor
        if (string.IsNullOrEmpty(t) || NetworkManager.instance.GetCheatsEnabled() == false)  {
            autocompleteContainer.SetActive(false);
        }
        else {
            autocompleteContainer.SetActive(true);

            while(autocompleteContent.childCount > 0) {
                DestroyImmediate(autocompleteContent.GetChild(0).gameObject);
            }

            void AddAutoCompleteItem(string label, string value) {
                var instance = Instantiate(autocompleteTemplate, autocompleteContent, false);

                instance.SetActive(true);

                var t = instance.GetComponent<TextMeshProUGUI>();

                if(t != null) {
                    t.text = label;
                }

                var trigger = instance.GetComponent<EventTrigger>();

                if(trigger == null) {
                    trigger = instance.AddComponent<EventTrigger>();
                }

                var entry = new EventTrigger.Entry() {
                    eventID = EventTriggerType.PointerClick,
                };

                entry.callback.AddListener((_) => {
                    var lastSpace = chatInput.text.LastIndexOf(' ');

                    if(lastSpace < 0) {
                        chatInput.text = $"{value} ";
                    } else {
                        chatInput.text = $"{chatInput.text.Substring(0, lastSpace)} {value} ";
                    }

                    chatInput.caretPosition = chatInput.text.Length;
                });

                trigger.triggers.Add(entry);
            }

            if (t[0] == '/') {
                if(t.IndexOf(' ') < 0) { //still writing the cheat
                    var commands = CheatsProcessor.GetCommands();

                    foreach(var command in commands) {
                        if(command.GetArg0().Contains(t, System.StringComparison.OrdinalIgnoreCase)) {
                            AddAutoCompleteItem(command.GetArg0(), command.GetArg0());
                        }
                    }
                } else {
                    var arguments = t.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

                    var commandName = arguments[0];

                    //Assume the first command matching will be the one we want
                    var command = CheatsProcessor.GetCommands()
                        .FirstOrDefault(x => x.GetArg0().Contains(commandName, System.StringComparison.OrdinalIgnoreCase));

                    var argIndex = t.Count(x => x == ' ');

                    var lastSpace = t.LastIndexOf(' ');

                    var argText = lastSpace + 1 < t.Length ? t.Substring(lastSpace + 1, t.Length - lastSpace - 1) : "";

                    var results = command.Autocomplete(argIndex, arguments, argText).ToArray();

                    if (results.Length == 0) {
                        autocompleteContainer.SetActive(false);
                    } else {
                        foreach(var result in results) {
                            AddAutoCompleteItem(result.label, result.value);
                        }
                    }
                }
            }
        }
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

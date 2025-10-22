using System.Collections;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Steamworks;
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
            closeChatAction.action.performed -= OnCloseChat;
        } else {
            StopAllCoroutines();
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
            PhotonNetwork.RaiseEvent(NetworkManager.CustomChatEvent, t, options, SendOptions.SendReliable);
        }
        MainMenu.ShowMenuStatic(MainMenu.MainMenuMode.None);
    }
}

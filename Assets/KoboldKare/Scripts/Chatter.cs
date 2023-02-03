using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Chatter : MonoBehaviourPun {
    [SerializeField]
    public TMPro.TMP_Text chatText;
    [SerializeField]
    private float textSpeedPerCharacter = 0.1f;
    private float minTextTimeout = 2f;
    [SerializeField]
    private AudioPack yowls;

    private Kobold kobold;
    private Coroutine displayMessageRoutine;

    public void SetTextOutput(TMPro.TMP_Text newChatText) {
        chatText = newChatText;
    }

    public void SetYowlPack(AudioPack newPack) {
        yowls = newPack;
    }

    private void Start() {
        kobold = GetComponent<Kobold>();
    }

    IEnumerator DisplayMessage(string message, float duration) {
        chatText.text = message;
        chatText.alpha = 1f;
        duration += message.Length * textSpeedPerCharacter; // Add additional seconds per character

        yield return new WaitForSeconds(duration);
        float endTime = Time.time + 1f;
        while(Time.time < endTime) {
            chatText.alpha = endTime-Time.time;
            yield return null;
        }
        chatText.alpha = 0f;
    }
    
    [PunRPC]
    public void RPCSendChat(string message) {
        //GameManager.instance.SpawnAudioClipInWorld(yowls[UnityEngine.Random.Range(0,yowls.Length)], transform.position);
        GameManager.instance.SpawnAudioClipInWorld(yowls, transform.position);
        if (displayMessageRoutine != null) {
            StopCoroutine(displayMessageRoutine);
        }

        foreach (var player in PhotonNetwork.PlayerList) {
            if ((Kobold)player.TagObject != GetComponent<Kobold>()) continue;
            CheatsProcessor.AppendText($"{player.NickName}: {message}\n");
            CheatsProcessor.ProcessCommand(kobold, message);
            displayMessageRoutine = StartCoroutine(DisplayMessage(message,minTextTimeout));
            break;
        }
    }
}

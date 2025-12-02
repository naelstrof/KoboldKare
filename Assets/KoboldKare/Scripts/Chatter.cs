using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Chatter : MonoBehaviour {
    [SerializeField]
    public TMPro.TMP_Text chatText;
    [SerializeField]
    private float textSpeedPerCharacter = 0.1f;
    private float minTextTimeout = 2f;
    [SerializeField]
    private AudioPack yowls;

    private Coroutine displayMessageRoutine;

    public void SetTextOutput(TMPro.TMP_Text newChatText) {
        chatText = newChatText;
    }

    public void SetYowlPack(AudioPack newPack) {
        yowls = newPack;
    }

    public void DisplayMessage(string message, float duration) {
        if (displayMessageRoutine != null) {
            StopCoroutine(displayMessageRoutine);
            displayMessageRoutine = null;
        }
        displayMessageRoutine = StartCoroutine(DisplayMessageRoutine(message, Mathf.Max(duration, minTextTimeout)));
    }

    IEnumerator DisplayMessageRoutine(string message, float duration) {
        GameManager.instance.SpawnAudioClipInWorld(yowls, transform.position);
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
        displayMessageRoutine = null;
    }
    
    
    // FIXME FISHNET
    /*
    private Player GetPlayer() {
        foreach (var player in PhotonNetwork.PlayerList) {
            if ((Kobold)player.TagObject != GetComponent<Kobold>()) continue;
            return player;
        }
        return null;
    }*/
}

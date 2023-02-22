using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayButton : MonoBehaviour {
    private void Start() {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick() {
        GameManager.StartCoroutineStatic(LoadSinglePlayer());
    }

    IEnumerator LoadSinglePlayer() {
        GetComponent<Button>().interactable = false;
        var handle = MapSelector.PromptForMapSelect(false);
        yield return handle;
        if (handle.Cancelled) {
            GetComponent<Button>().interactable = true;
            yield break;
        }
        NetworkManager.instance.SetSelectedMap(handle.Result.playableMap);
        NetworkManager.instance.StartSinglePlayer();
        GetComponent<Button>().interactable = true;
    }
}

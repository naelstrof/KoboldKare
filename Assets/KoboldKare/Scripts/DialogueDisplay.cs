using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dialogue;
using TMPro;
using RichTextSubstringHelper;
using UnityEngine.Events;

public class DialogueDisplay : MonoBehaviour {
    public DialogueGraph graph;
    public TMPro.TextMeshPro text;
    public Dialogue.CharacterInfo character;
    public GameObject dialogueButton;
    public List<GameObject> dialogueOptions = new List<GameObject>();
    private Chat currentChat = null;
    [Range(1f,50f)]
    public float textSpeed = 25f;
    [Range(0f,5f)]
    public float dialogueDelay = 3f;
    [Range(0f,1f)]
    public float spaceDelay = 0.1f;
    [Range(0f,1f)]
    public float periodDelay = 0.25f;
    public UnityEvent onChange;
    public UnityEvent onFinishSpeak;
    private bool triggered = false;
    IEnumerator SpeakText(string targetString) {
        int length = targetString.RichTextLength();
        string realText = targetString.RichTextSubString(length, false);
        int i = 0;
        text.text = targetString;
        text.maxVisibleCharacters = 0;
        while(i < length) {
            float delay = 0f;
            switch (realText[i]) {
                case ' ': delay += spaceDelay; break;
                case '.': delay += periodDelay; break;
                case ',': delay += periodDelay; break;
                case '?': delay += periodDelay; break;
                case '!': delay += periodDelay; break;
            }
            text.maxVisibleCharacters = i;
            //text.text = targetString.RichTextSubString(i);
            yield return new WaitForSeconds(1f / textSpeed + delay);
            i++;
        }
        text.maxVisibleCharacters = length;
        yield return new WaitForSeconds(dialogueDelay);
        if (dialogueOptions.Count == 0) {
            text.color = Color.clear;
            // Clear the current text (move to the next dialogue)
            if (triggered) {
                onFinishSpeak.Invoke();
                triggered = false;
            }
            graph.AnswerQuestion("");
        }
    }
    //IEnumerator HideTextAfterDelay(float delay) {
        //yield return new WaitForSeconds(delay);
        //text.color = Color.clear;
        //graph.AnswerQuestion("");
    //}
    public void SwitchTo(DialogueGraph graph) {
        StopAllCoroutines();
        text.color = Color.clear;
        this.graph.Unregister(OnChange);
        this.graph = graph;
        this.graph.Register(OnChange);
    }
    public void OnChange(ref HashSet<Chat> currentChats) {
        currentChat = null;
        // Figure out which speaker is us.
        foreach (Chat c in currentChats) {
            if (c.character == character) {
                currentChat = c;
                break;
            }
        }
        // We aren't speaking at this point.
        if (currentChat == null ) {
            return;
        }
        text.color = character.color;
        StopAllCoroutines();
        foreach(GameObject g in dialogueOptions) {
            Destroy(g);
        }
        dialogueOptions.Clear();
        int i = 0;
        foreach(Chat.Answer a in currentChat.answers) {
            i++;
            float rot = ((float)i / (float)currentChat.answers.Count) * Mathf.PI * 2f;
            rot += Mathf.PI/2f;
            Vector3 offset;
            if (currentChat.answers.Count % 2 == 0 || currentChat.answers.Count == 1) {
                offset = transform.right * Mathf.Sin(rot) + transform.up * Mathf.Cos(rot);
            } else {
                offset = transform.right * Mathf.Cos(rot) + transform.up * Mathf.Sin(rot);
            }
            GameObject d = GameObject.Instantiate(dialogueButton, transform.position + offset + Vector3.down*0.5f, Quaternion.identity, transform.parent);
            d.GetComponent<TMPro.TextMeshPro>().text = a.text;
            //d.GetComponent<AdvancedInteractionButton>().onGrab.AddListener(()=>Answer(a.text));
            dialogueOptions.Add(d);
        }
        if (!triggered) {
            onChange.Invoke();
            triggered = true;
        }
        StartCoroutine(SpeakText(currentChat.text));
    }
    public void Answer(string s) {
        if (triggered) {
            onFinishSpeak.Invoke();
            triggered = false;
        }
        graph.AnswerQuestion(s);
    }
    private void Awake() {
        text.color = Color.clear;
        graph.Register(OnChange);
    }
    private void OnDestroy() {
        graph.Unregister(OnChange);
    }
}

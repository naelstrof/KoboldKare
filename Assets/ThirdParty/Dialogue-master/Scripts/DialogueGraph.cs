using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XNode;

namespace Dialogue {
    [CreateAssetMenu(menuName = "Dialogue/Graph", order = 0)]
    public class DialogueGraph : NodeGraph {
        public delegate void OnChange(ref HashSet<Chat> current);
        public List<OnChange> subscribe = new List<OnChange>();
        public void Register(OnChange func) {
            subscribe.Add(func);
            if (currentChats.Count != 0) {
                func(ref currentChats);
            }
        }
        public void Unregister(OnChange func) {
            subscribe.Remove(func);
        }
        public void TransitionTo(Chat c, int priority) {
            // If nobody cares that we're trying to do conversation stuff, then just stop.
            if (subscribe.Count == 0 ) {
                return;
            }
            if (currentChats.Count > 0) {
                int currentChatPriority = 0;
                foreach (Chat ch in currentChats) {
                    currentChatPriority = ch.priority;
                }
                // Ignore lower or equal priority requests. (Don't cancel dialogues with equal priority.)
                if (priority <= currentChatPriority) {
                    return;
                }
                currentChats.Clear();
                foreach (DialogueBaseNode n in needsClearOnChange) {
                    n.Clear();
                }
                needsClearOnChange.Clear();
            }
            if (currentChats.Count == 0) {
                currentChats.Add(c);
                needsClearOnChange.Add(c);
                foreach (OnChange oc in subscribe) {
                    oc(ref currentChats);
                }
            }
        }

        public HashSet<DialogueBaseNode> needsClearOnChange = new HashSet<DialogueBaseNode>();
        public HashSet<Chat> currentChats = new HashSet<Chat>();
        public void AnswerQuestion(string s) {
            HashSet<Chat> copy = new HashSet<Chat>(currentChats);
            // Clear out all the current chats.
            currentChats.Clear();
            foreach (DialogueBaseNode n in needsClearOnChange) {
                n.Clear();
            }
            needsClearOnChange.Clear();
            foreach(Chat c in copy) {
                c.AnswerQuestion(s);
            }
            //foreach(OnChange oc in subscribe) {
                //oc(ref currentChats);
            //}
        }
    }
}
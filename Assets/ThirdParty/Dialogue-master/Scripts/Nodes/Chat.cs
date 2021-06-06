using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace Dialogue {
    [NodeTint("#CCFFCC")]
    public class Chat : DialogueBaseNode {
        public CharacterInfo character;
        public int priority;
        [TextArea] public string text;
        [Output(instancePortList = true)] public List<Answer> answers = new List<Answer>();

        [System.Serializable] public class Answer {
            public string text;
        }

        public bool AnswerQuestion(string s) {
            NodePort port = null;
            if (answers.Count == 0) {
                port = GetOutputPort("output");
            }
            for(int i = 0; i < answers.Count; i++) {
                if (answers[i].text == s) {
                    port = GetOutputPort("answers " + i);
                }
            }
            if (port == null) {
                return false;
            }
            for (int i = 0; i < port.ConnectionCount; i++) {
                NodePort connection = port.GetConnection(i);
                (connection.node as DialogueBaseNode).Trigger(priority);
            }
            return true;
        }

        public override void Trigger(int priority) {
            this.priority = priority;
            (graph as DialogueGraph).TransitionTo(this, priority);
        }

        public override void Clear() {
        }
    }
}
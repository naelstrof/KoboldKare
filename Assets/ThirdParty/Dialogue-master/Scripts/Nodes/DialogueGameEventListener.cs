using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using KoboldKare;

namespace Dialogue {
	[NodeTint("#FFFFAA")]
	public class DialogueGameEventListener : DialogueBaseNode {
        [Output(dynamicPortList = true)] public List<GEvent> events = new List<GEvent>();
        private bool listening = false;
        [System.Serializable] public class GEvent {
            public GameEventGeneric.GameEventActionGeneric action;
            public GameEventGeneric gameEvent;
            [Range(-5,5)]
            public int priority;
        }
        public new void OnEnable() {
            if (input == null) {
                listening = true;
            }
            foreach(GEvent e in events) {
                if (e.gameEvent!=null) {
                    e.action = (obj) => {OnEventRaised(e,obj);};
                    e.gameEvent.AddListener(e.action);
                }
            }
            base.OnEnable();
        }
        public void OnDisable() {
            foreach(GEvent e in events) {
                if (e.gameEvent!=null) {
                    e.gameEvent.RemoveListener(e.action);
                }
            }
        }
        public void OnEventRaised(GEvent e, object nothing) {
            NodePort port = null;
            int priority = 0;
            for(int i = 0; i < events.Count; i++) {
                if (e == events[i]) {
                    port = GetOutputPort("events " + i);
                    priority = events[i].priority;
                }
            }
            if (port == null) return;
            for (int i = 0; i < port.ConnectionCount; i++) {
                NodePort connection = port.GetConnection(i);
                (connection.node as DialogueBaseNode).Trigger(priority);
            }
            if (input != null) {
                listening = false;
            }
        }
        public override void Trigger(int priority) {
            listening = true;
        }
        public override void Clear() {
            listening = false;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using KoboldKare;

namespace Dialogue {
	[NodeTint("#FFFFAA")]
	public class DialogueGameEventListener : DialogueBaseNode, IGameEventListener {
        [Output(dynamicPortList = true)] public List<GEvent> events = new List<GEvent>();
        private bool listening = false;
        [System.Serializable] public class GEvent {
            public GameEvent gameEvent;
            [Range(-5,5)]
            public int priority;
        }
        public new void OnEnable() {
            if (input == null) {
                listening = true;
            }
            foreach(GEvent e in events) {
                if (e.gameEvent!=null) {
                    e.gameEvent.RegisterListener(this);
                }
            }
            base.OnEnable();
        }
        public void OnDisable() {
            foreach(GEvent e in events) {
                if (e.gameEvent!=null) {
                    e.gameEvent.UnregisterListener(this);
                }
            }
        }
        public void OnEventRaised(GameEvent e) {
            NodePort port = null;
            int priority = 0;
            for(int i = 0; i < events.Count; i++) {
                if (e == events[i].gameEvent) {
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
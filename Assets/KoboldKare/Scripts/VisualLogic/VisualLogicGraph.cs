using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XNode;
using KoboldKare;

namespace VisualLogic {
    [CreateAssetMenu(menuName = "VisualLogic/Graph", order = 0)]
    public class VisualLogicGraph : NodeGraph {
        public void TriggerEvent(GameObject self, Event.EventType type, object[] parameters) {
            foreach(var node in nodes) {
                if (node is Event && (node as Event).eventType == type) {
                    (node as Event).parameters = parameters;
                    Task t = new Task((node as Event).Trigger(self));
                }
            }
        }
    }
}
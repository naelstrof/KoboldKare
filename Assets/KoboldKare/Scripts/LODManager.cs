using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LODManager : MonoBehaviour {
    private Camera internalMainCamera;
    private Camera mainCamera {
        get {
            if (internalMainCamera == null) {
                internalMainCamera = Camera.current;
            }
            if (internalMainCamera == null) {
                internalMainCamera = Camera.main;
            }
            return internalMainCamera;
        }
    }
    [System.Serializable]
    public class Resource {
        public GenericLODConsumer.ConsumerType type;
        public int highQualityCount;
        [HideInInspector]
        public List<GenericLODConsumer> registeredConsumers = new List<GenericLODConsumer>();
    }

    public List<Resource> consumerResources = new List<Resource>();
    public static LODManager instance;
    void Awake() {
        //Check if instance already exists
        if (instance == null) {
            //if not, set instance to this
            instance = this;
        } else if (instance != this) { //If instance already exists and it's not this:
            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);
            return;
        }
    }
    void Update() {
        // If we don't have a camera, don't continue;
        if (mainCamera == null) {
            return;
        }
        Vector3 cameraPos = mainCamera.transform.position;
        // Lazily bubble sort
        // Make sure only `availableResources` are activated
        foreach(Resource resource in consumerResources) {
            float a = 0;
            for (int i = 0; i < resource.registeredConsumers.Count; i++) {
                if (resource.registeredConsumers[i] == null) {
                    resource.registeredConsumers.RemoveAt(i);
                    continue;
                }
                resource.registeredConsumers[i].SetLOD(i <= resource.highQualityCount);

                float b = Vector3.Distance(resource.registeredConsumers[i].transform.position, cameraPos);
                if (b < a) {
                    (resource.registeredConsumers[i - 1], resource.registeredConsumers[i]) = (resource.registeredConsumers[i], resource.registeredConsumers[i - 1]);
                    if (i - 1 <= resource.highQualityCount && i > resource.highQualityCount) {
                        resource.registeredConsumers[i-1].SetLOD(true);
                        resource.registeredConsumers[i].SetLOD(false);
                    }
                }
                a = b;
            }
        }
    }
    public void RegisterConsumer(GenericLODConsumer g, GenericLODConsumer.ConsumerType type) {
        foreach (Resource resource in consumerResources) {
            if (resource.type == type) {
                resource.registeredConsumers.Add(g);
            }
        }
    }

    public void UnregisterConsumer(GenericLODConsumer g, GenericLODConsumer.ConsumerType type) {
        foreach (Resource resource in consumerResources) {
            if (resource.type == type && resource.registeredConsumers.Contains(g)) {
                resource.registeredConsumers.Remove(g);
            }
        }
    }
}

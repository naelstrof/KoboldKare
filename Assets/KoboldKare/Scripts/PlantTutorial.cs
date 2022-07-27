using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantTutorial : MonoBehaviour {
    [SerializeField]
    private Canvas waterCanvas;
    [SerializeField]
    private Canvas timeCanvas;
    private GenericReagentContainer container;
    private Plant plant;
    void Start() {
        plant = GetComponent<Plant>();
        container = GetComponent<GenericReagentContainer>();
        container.OnFilled.AddListener(OnFilled);
        plant.switched += OnSwitched;
    }
    void OnDestroy() {
        if (plant != null) {
            plant.switched -= OnSwitched;
            container.OnFilled.RemoveListener(OnFilled);
        }
    }
    void OnFilled(ReagentContents contents, GenericReagentContainer.InjectType injectType) {
        if (plant.plant.possibleNextGenerations.Length == 0) {
            waterCanvas.gameObject.SetActive(false);
            timeCanvas.gameObject.SetActive(false);
            return;
        }
        timeCanvas.gameObject.SetActive(true);
        waterCanvas.gameObject.SetActive(false);
    }
    void OnSwitched() {
        if (plant.plant.possibleNextGenerations.Length == 0) {
            waterCanvas.gameObject.SetActive(false);
            timeCanvas.gameObject.SetActive(false);
            return;
        }
        timeCanvas.gameObject.SetActive(false);
        waterCanvas.gameObject.SetActive(true);
    }
}


using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class CheckIfModdingIsSupported : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField]
    private GameObject hoverHelp;

    private Selectable selectable;
    private void OnEnable() {
        selectable = GetComponent<Selectable>();
        selectable.interactable = ModManager.IsValid();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (!selectable.interactable) {
            hoverHelp.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        hoverHelp.SetActive(false);
    }
}

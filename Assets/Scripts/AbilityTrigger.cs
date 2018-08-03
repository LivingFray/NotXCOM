using UnityEngine;
using UnityEngine.EventSystems;

public class AbilityTrigger : MonoBehaviour, IPointerDownHandler {
    UIController controller;

    public void OnPointerDown(PointerEventData eventData) {
        controller.TriggerAbility();
    }

    void Start() {
        controller = GetComponentInParent<UIController>();
    }

}

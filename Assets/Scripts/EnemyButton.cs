using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyButton : MonoBehaviour, IPointerDownHandler {

    public int id;

    UIController controller;

    public void OnPointerDown(PointerEventData eventData) {
        controller.EnemyClicked(id);
    }

    void Start() {
        controller = GetComponentInParent<UIController>();
    }

}

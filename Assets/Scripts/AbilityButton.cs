using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AbilityButton : MonoBehaviour, IPointerDownHandler{

    public int id;

    UIController controller;

    public void OnPointerDown(PointerEventData eventData) {
        controller.AbilityClicked(id);
    }

    void Start () {
        controller = GetComponentInParent<UIController>();
	}

}

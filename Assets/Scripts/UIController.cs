using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIController : MonoBehaviour{

    public int maxAbilities = 15;

    Image[] images;

    Entity entity;

    public Image image;

    public float spacingX = 50.0f;

    private void Awake() {
        images = new Image[maxAbilities];
        for(int i = 0; i < maxAbilities; i++) {
            images[i] = Instantiate(image, transform);
            images[i].rectTransform.position += new Vector3(i * spacingX,0,0);
            images[i].gameObject.SetActive(false);
            images[i].GetComponent<AbilityButton>().id = i;
        }
    }

    //TODO: Hover

    public void AbilityClicked(int aNo) {
        Debug.Log("Click");
        entity.abilities[aNo].SelectAction(entity);
    }

    public void AddAbilities(Ability[] abilities, Entity e) {
        entity = e;
        for (int i = 0; i < abilities.Length && i < maxAbilities; i++) {
            images[i].gameObject.SetActive(true);
            images[i].sprite = abilities[i].icon;
        }
    }

    public void ClearAbilities() {
        entity = null;
        foreach(Image image in images) {
            image.gameObject.SetActive(false);
        }
    }
}

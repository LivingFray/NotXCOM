using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {

    public int maxAbilities = 15;

    public int maxEnemies = 10;

    Image[] abilities;

    Image[] enemies;

    Entity entity;

    public Image abilityImage;

    public Image enemyImage;

    public GameObject fireButton;

    TMPro.TextMeshProUGUI fireText;

    public GameObject panel;

    public float abilitySpacing = 50.0f;
    public float enemySpacing = 20.0f;

    int currentAbility;

    private void Awake() {
        abilities = new Image[maxAbilities];
        for (int i = 0; i < maxAbilities; i++) {
            abilities[i] = Instantiate(abilityImage, transform);
            abilities[i].rectTransform.position += new Vector3(i * abilitySpacing, 0, 0);
            abilities[i].gameObject.SetActive(false);
            abilities[i].GetComponent<AbilityButton>().id = i;
        }
        enemies = new Image[maxEnemies];
        for (int i = 0; i < maxEnemies; i++) {
            enemies[i] = Instantiate(enemyImage, transform);
            enemies[i].rectTransform.position += new Vector3(i * enemySpacing, 0, 0);
            enemies[i].gameObject.SetActive(false);
            enemies[i].GetComponent<EnemyButton>().id = i;
        }
        fireText = fireButton.GetComponent<TMPro.TextMeshProUGUI>();
        RemoveEntity();
    }

    //TODO: Hover

    public void AbilityClicked(int aNo) {
        if (entity.abilities[aNo].SelectAction(entity)) {
            currentAbility = aNo;
            fireButton.SetActive(true);
            fireText.text = entity.abilities[aNo].abilityName;
        }
    }

    public void EnemyClicked(int eNo) {
        entity.SetSelectedEntity(eNo);
    }

    public void AddAbilities(Ability[] abilities, Entity e) {
        entity = e;
        panel.SetActive(true);
        for (int i = 0; i < abilities.Length && i < maxAbilities; i++) {
            this.abilities[i].gameObject.SetActive(true);
            this.abilities[i].sprite = abilities[i].icon;
        }
    }

    public void RemoveEntity() {
        entity = null;
        panel.SetActive(false);
        fireButton.SetActive(false);
        foreach (Image image in abilities) {
            image.gameObject.SetActive(false);
        }
        foreach (Image image in enemies) {
            image.gameObject.SetActive(false);
        }
    }

    public void AddEnemies(List<Entity> enemies, Entity e) {
        //I suck at UI
        entity = e;
        for (int i = 0; i < enemies.Count && i < maxEnemies; i++) {
            this.enemies[i].gameObject.SetActive(true);
            //this.enemies[i].sprite = enemies[i].icon;
        }
    }

    public void TriggerAbility() {
        if(entity != null) {
            entity.abilities[currentAbility].TriggerAction(entity);
        }
    }
}

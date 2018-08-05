using System.Collections;
using System;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class Entity : MonoBehaviour {

    [NonSerialized]
    public Team team;
    [NonSerialized]
    public GameController controller;
    [NonSerialized]
    public Board board;

    public Ability[] abilities;

    public TextMeshProUGUI healthText;

    public GameObject hitIndicator;

    public float movementSpeed = 5.0f;

    public int movementPoints = 12;

    public byte actions = 2;

    public Vector3Int GridPos { get; private set; }

    public int maxHealth = 5;

    int health;

    List<Entity> visibleEntities;
    List<byte> visibleEntitiesCover;
    int selectedEntity;

    public GameObject entityUI;

    UIController uiController;
    
    //Other stats like aim, defence, etc here

    public float aim = 0.6f;

    public Gun gun;

    [HideInInspector]
    public int ammo;

    public void SetPosition(Vector3Int position) {
        GridPos = position;
        transform.position = position;
    }

    public void MoveTo(Vector3Int target) {
        StartCoroutine(Move_Coroutine(target));
    }

    IEnumerator Move_Coroutine(Vector3Int target) {
        GridPos = target;
        float dist = (target - transform.position).magnitude;
        float d = 0;
        Vector3 start = transform.position;
        while (d < dist) {
            d += movementSpeed * Time.deltaTime;
            transform.position = Vector3.Lerp(start, target, d / dist);
            yield return null;
        }
    }

    public void FollowPath(Vector3Int[] path) {
        if(path == null) {
            Debug.LogWarning("No path to follow");
            return;
        }
        if(actions == 0 || path.Length > movementPoints) {
            return;
        }
        StartCoroutine(Path_Coroutine(path));
        actions--;
    }

    IEnumerator Path_Coroutine(Vector3Int[] path) {
        for (int i = 0; i < path.Length; i++) {
            yield return Move_Coroutine(path[i]);
        }
    }

    public void ShowHitIndicator(float hitOdds, int damage, Entity enemy) {
        string text;
        string strOdds = (hitOdds * 100.0f).ToString("N0");
        if(damage == 0) {
            text = "Miss! (" + strOdds + "%)";
        } else {
            text = "-" + damage + " (" + strOdds + "%)";
        }
        GameObject hit = Instantiate(hitIndicator, enemy.transform.position, Quaternion.identity);
        hit.transform.Find("HitIndicator").GetComponent<TextMeshProUGUI>().text = text;
        Destroy(hit, 2.0f);
    }

    public float GetHitChance(Entity enemy, byte coverType) {
        //Debug.Log("Aim: " + aim);
        //TODO: Make calculation more flexible / setting based
        //Calculate hit chance:
        //TODO: Aiming angles: better angle = better chance
        float distanceModifier = gun.baseHitChance - gun.hitChanceFalloff * (GridPos - enemy.GridPos).magnitude;
        //Debug.Log("Distance: " + distanceModifier);
        float coverModifier;
        if(coverType == (byte)CoverType.FULL) {
            coverModifier = -0.4f;
        } else if (coverType == (byte)CoverType.HALF) {
            coverModifier = -0.2f;
        } else {
            coverModifier = 0.0f;
        }
        //Debug.Log("Cover: " + coverModifier);
        return aim + distanceModifier + coverModifier;
    }

    public void Damage(int damage) {
        health -= damage;
        if(health <= 0) {
            health = 0;
            //Do death
            team.EntityDied(this);
            gameObject.SetActive(false);
        }
        healthText.text = "Health: " + health + "/" + maxHealth;
    }

    public void ShowVisibleEntities() {
        UpdateVisibleEntities();
        uiController.AddEnemies(visibleEntities, this);
    }

    public void UpdateVisibleEntities() {
        //Find all visible entities
        visibleEntities.Clear();
        visibleEntitiesCover.Clear();
        foreach (GameObject entObj in controller.entities) {
            Entity ent = entObj.GetComponent<Entity>();
            if (ent == null || ent.team == team) {
                continue;
            }
            byte cover;
            if (board.HasLineOfSight(GridPos, ent.GridPos, out cover)) {
                visibleEntities.Add(ent);
                visibleEntitiesCover.Add(cover);
            }
        }
        //Sort by hit chance?

        selectedEntity = 0;
    }

    public Entity GetSelectedEntity(out byte cover) {
        cover = 0;
        //Ensure a target exists
        if (selectedEntity >= visibleEntities.Count) {
            return null;
        }
        //TODO: Get weapon from entity and extract values that way
        Entity enemy = visibleEntities[selectedEntity];
        cover = visibleEntitiesCover[selectedEntity];
        return enemy;
    }

    public void SetSelectedEntity(int pos) {
        if(pos < 0 || pos >= visibleEntities.Count) {
            return;
        }
        selectedEntity = pos;
    }

    public void OnSelected() {
        CreateUnitUI();
    }

    public void OnDeselected() {
        DestroyUnitUI();
    }

    void CreateUnitUI() {
        if(uiController == null) {
            uiController = entityUI.GetComponent<UIController>();
        }
        uiController.AddAbilities(abilities, this);
    }

    void DestroyUnitUI() {
        uiController.RemoveEntity();
    }

    private void Awake() {
        GridPos = new Vector3Int((int)transform.position.x, (int)transform.position.y, (int)transform.position.z);
        health = maxHealth;
        healthText.text = "Health: " + health + "/" + maxHealth;
        visibleEntities = new List<Entity>();
        visibleEntitiesCover = new List<byte>();
        ammo = gun.maxAmmo;
    }

    private void OnMouseDown() {
        if (!EventSystem.current.IsPointerOverGameObject()) {
            team.EntityClicked(this);
        }
    }
}

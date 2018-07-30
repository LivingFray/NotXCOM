using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EntityController : MonoBehaviour {

    [HideInInspector]
    public Team team;
    [HideInInspector]
    public GameController controller;

    public TextMeshProUGUI healthText;

    public GameObject hitIndicator;

    public float movementSpeed = 5.0f;

    public int movementPoints = 12;

    public byte actions = 2;

    public Vector3Int GridPos { get; private set; }

    public int maxHealth = 5;

    int health;

    //Other stats like aim, defence, etc here

    //TODO: Refactor attacks to be separate class
    //TODO: Make damage dealt a stat of weapon
    public int minDamage = 3;
    public int maxDamage = 5;

    public float aim = 0.6f;

    public float distHitBase = 0.4f;
    public float distHitFall = 0.045f;

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

    public void ShootEnemy(EntityController enemy) {
        byte cover;
        bool los = controller.HasLineOfSightDDA(GridPos, enemy.GridPos, out cover);
        if (los) {
            float hitChance = GetHitChance(enemy, cover);
            if(Random.Range(0.0f, 1.0f) < hitChance) {
                int damage = Random.Range(minDamage, maxDamage + 1);
                enemy.Damage(damage);
                ShowHitIndicator(hitChance, damage, enemy);
            } else {
                ShowHitIndicator(hitChance, 0, enemy);
            }
            //Pretty fire animations and such
        } else {
            Debug.Log("Shot blocked");
        }
        actions--;
    }

    void ShowHitIndicator(float hitOdds, int damage, EntityController enemy) {
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

    public float GetHitChance(EntityController enemy, byte coverType) {
        //Debug.Log("Aim: " + aim);
        //TODO: Make calculation more flexible / setting based
        //Calculate hit chance:
        //TODO: Aiming angles: better angle = better chance
        float distanceModifier = distHitBase - distHitFall * (GridPos - enemy.GridPos).magnitude;
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
            //Doesn't remove from list yet
            team.EntityDied(this);
            gameObject.SetActive(false);
        }
        healthText.text = "Health: " + health + "/" + maxHealth;
    }

    private void Awake() {
        GridPos = new Vector3Int((int)transform.position.x, (int)transform.position.y, (int)transform.position.z);
        health = maxHealth;
        healthText.text = "Health: " + health + "/" + maxHealth;
    }

    private void OnMouseDown() {
        team.EntityClicked(this);
    }
}
